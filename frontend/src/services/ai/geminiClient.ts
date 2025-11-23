import { getDefaultGeminiModel } from '../geminiModels'
import type { ExtractedMetadata } from '@types'

// Normalize model name: remove 'models/' prefix if present
const normalizeModelName = (modelName: string): string => {
  return modelName.replace(/^models\//, '').trim()
}

// Helper function to sleep (for retry backoff)
const sleep = (ms: number): Promise<void> => new Promise(resolve => setTimeout(resolve, ms))

// Helper to extract retry delay from error message
const extractRetryDelay = (errorMessage: string): number | null => {
  const match = errorMessage.match(/retry in ([\d.]+)s/i)
  if (match && match[1]) {
    return Math.ceil(parseFloat(match[1]) * 1000) + 1000 // Add 1 second buffer
  }
  return null
}

export const extractWithGemini = async (
  pdfText: string,
  apiKey: string,
  instructions?: string
): Promise<ExtractedMetadata> => {
  // Get model from localStorage or use default
  let geminiModel = localStorage.getItem('etnopapers_gemini_model') || getDefaultGeminiModel()

  // Normalize model name to ensure correct format
  geminiModel = normalizeModelName(geminiModel)

  // Clear invalid cached model (gemini-pro is deprecated)
  if (geminiModel === 'gemini-pro') {
    geminiModel = getDefaultGeminiModel()
    localStorage.removeItem('etnopapers_gemini_model')
  }

  const defaultInstructions = `Extract metadata from scientific article. Return ONLY valid JSON.

JSON schema:
{
  "titulo": "article title (required)",
  "autores": [{"nome": "first", "sobrenome": "last"}],
  "ano_publicacao": "year as number",
  "resumo": "abstract (optional)",
  "doi": "string (optional)",
  "especies": [{"vernacular": "common name", "nomeCientifico": "scientific name"}],
  "tipo_de_uso": "medicinal/food/etc (optional)",
  "metodologia": "research method (optional)",
  "pais": "country (optional)",
  "estado": "state (optional)",
  "municipio": "city (optional)"
}

Rules:
1. Return ONLY JSON, no markdown, no extra text
2. Extract only explicitly mentioned information
3. Omit missing fields
4. Scientific names in binomial format (Genus species)
5. Keep author order as in article`

  const systemPrompt = instructions || defaultInstructions

  // Retry logic with exponential backoff
  const maxRetries = 2
  let lastError: Error | null = null

  for (let retryCount = 0; retryCount <= maxRetries; retryCount++) {
    try {
      const response = await fetch(
        `https://generativelanguage.googleapis.com/v1/models/${geminiModel}:generateContent?key=${apiKey}`,
        {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify({
            contents: [
              {
                parts: [
                  {
                    text: `${systemPrompt}\n\nEXTRACT METADATA:\n\n${pdfText}`,
                  },
                ],
              },
            ],
            generationConfig: {
              temperature: 0.1,
              topK: 1,
              topP: 0.1,
              maxOutputTokens: 2048,
            },
          }),
        }
      )

      if (!response.ok) {
        const errorData = await response.json()
        const errorMessage = errorData.error?.message || 'Unknown error'

        // Check if rate limited
        if (response.status === 429 || errorMessage.includes('quota') || errorMessage.includes('rate limit')) {
          const retryDelay = extractRetryDelay(errorMessage)
          if (retryCount < maxRetries && retryDelay) {
            console.warn(`Rate limited by Gemini API. Waiting ${retryDelay}ms before retry...`)
            await sleep(retryDelay)
            continue
          }
        }

        throw new Error(`Gemini API error: ${errorMessage}`)
      }

      const data = await response.json()

      // Better error handling with debugging info
      if (!data.candidates || data.candidates.length === 0) {
        console.error('Gemini response:', JSON.stringify(data, null, 2))
        throw new Error(`Gemini API returned no candidates. Response: ${JSON.stringify(data).substring(0, 200)}`)
      }

      if (!data.candidates[0]?.content?.parts) {
        console.error('Gemini response:', JSON.stringify(data.candidates[0], null, 2))
        throw new Error('Gemini response has no content parts')
      }

      if (!data.candidates[0].content.parts[0]?.text) {
        console.error('Gemini response:', JSON.stringify(data.candidates[0].content, null, 2))
        throw new Error('Gemini response part has no text')
      }

      const responseText = data.candidates[0].content.parts[0].text

      // Try to extract JSON from the response (it might be wrapped in markdown code blocks)
      const jsonMatch = responseText.match(/```json\n([\s\S]*?)\n```/) ||
        responseText.match(/```\n([\s\S]*?)\n```/) || [null, responseText]

      const jsonStr = jsonMatch[1] || responseText

      let metadata
      try {
        metadata = JSON.parse(jsonStr.trim())
      } catch (parseError) {
        // If JSON parsing fails, provide helpful error message
        const preview = jsonStr.trim().substring(0, 100)
        throw new Error(`Invalid JSON response from Gemini. Response preview: "${preview}"`)
      }

      // Validate and normalize the response
      const extracted: ExtractedMetadata = {
        titulo: metadata.titulo || '',
        autores: Array.isArray(metadata.autores) ? metadata.autores : [],
        ano_publicacao: metadata.ano_publicacao || new Date().getFullYear(),
        resumo: metadata.resumo,
        doi: metadata.doi,
        especies: Array.isArray(metadata.especies) ? metadata.especies : [],
        regioes: Array.isArray(metadata.regioes) ? metadata.regioes : [],
        comunidades: Array.isArray(metadata.comunidades) ? metadata.comunidades : [],
        tipo_de_uso: metadata.tipo_de_uso,
        metodologia: metadata.metodologia,
        pais: metadata.pais,
        estado: metadata.estado,
        municipio: metadata.municipio,
        local: metadata.local,
        bioma: metadata.bioma,
      }

      return extracted
    } catch (error) {
      lastError = error instanceof Error ? error : new Error(String(error))

      // If this was the last retry, throw the error
      if (retryCount === maxRetries) {
        throw new Error(`Failed to extract metadata with Gemini: ${lastError.message}`)
      }

      // Otherwise, wait and retry with exponential backoff
      const backoffDelay = Math.pow(2, retryCount) * 1000
      console.warn(`Extraction failed (attempt ${retryCount + 1}/${maxRetries}). Retrying in ${backoffDelay}ms...`)
      await sleep(backoffDelay)
    }
  }

  // This should never be reached, but just in case
  throw new Error(`Failed to extract metadata with Gemini: ${lastError?.message || 'Unknown error'}`)
}
