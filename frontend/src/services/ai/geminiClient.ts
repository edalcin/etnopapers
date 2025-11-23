import { getDefaultGeminiModel } from '../geminiModels'
import type { ExtractedMetadata } from '@types'

// Normalize model name: remove 'models/' prefix if present
const normalizeModelName = (modelName: string): string => {
  return modelName.replace(/^models\//, '').trim()
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

  const defaultInstructions = `INSTRUÇÃO CRÍTICA: Você DEVE retornar APENAS um objeto JSON válido. NADA de texto, NADA de explicações, APENAS JSON.

Você é um especialista em extração de metadados de artigos científicos sobre etnobotânica.

EXTRAIA estas informações do texto do artigo e retorne APENAS um objeto JSON válido (sem markdown, sem texto extra):

SCHEMA JSON ESPERADO:
{
  "titulo": "string (obrigatório)",
  "autores": [{"nome": "string", "sobrenome": "string", "email": "string (opcional)"}],
  "ano_publicacao": "número",
  "resumo": "string (opcional)",
  "doi": "string (opcional)",
  "especies": [{"vernacular": "string", "nomeCientifico": "string"}],
  "tipo_de_uso": "string (opcional)",
  "metodologia": "string (opcional)",
  "comunidades": ["string"],
  "pais": "string (opcional)",
  "estado": "string (opcional)",
  "municipio": "string (opcional)",
  "local": "string (opcional)",
  "bioma": "string (opcional)",
  "regioes": ["string"]
}

REGRAS OBRIGATÓRIAS:
1. Retorne APENAS JSON válido, sem markdown code blocks
2. NENHUM texto antes ou depois do JSON
3. Extraia apenas informações EXPLICITAMENTE mencionadas no texto
4. Omita campos não encontrados
5. Nomes científicos em formato binomial (Gênero species)
6. Mantenha ordem dos autores conforme artigo`

  const systemPrompt = instructions || defaultInstructions

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
                  text: `${systemPrompt}\n\nAGORA EXTRAIA OS METADADOS do seguinte texto de artigo científico.\n\nRETORNE APENAS O JSON, SEM NENHUM OUTRO TEXTO.\n\nTEXTO DO ARTIGO:\n${pdfText}`,
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
      throw new Error(
        `Gemini API error: ${errorData.error?.message || 'Unknown error'}`
      )
    }

    const data = await response.json()

    if (!data.candidates || !data.candidates[0]?.content?.parts[0]?.text) {
      throw new Error('Invalid response format from Gemini API')
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
    const message = error instanceof Error ? error.message : 'Unknown error'
    throw new Error(`Failed to extract metadata with Gemini: ${message}`)
  }
}
