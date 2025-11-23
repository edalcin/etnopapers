import type { ExtractedMetadata } from '@types'

export const extractWithGemini = async (
  pdfText: string,
  apiKey: string,
  instructions?: string
): Promise<ExtractedMetadata> => {
  const defaultInstructions = `Você é um especialista em extração de metadados de artigos científicos sobre etnobotânica.

Sua tarefa é extrair as seguintes informações do texto do artigo e retornar um JSON estruturado:

**INFORMAÇÕES BIBLIOGRÁFICAS:**
- titulo: Nome completo do artigo (obrigatório)
- autores: Lista de objetos com {nome, sobrenome, email (opcional)} (obrigatório, mínimo 1)
- ano_publicacao: Ano de publicação (obrigatório, número 1900-2100)
- resumo: Resumo ou abstrato do artigo (opcional)
- doi: Digital Object Identifier (opcional)

**INFORMAÇÕES ETNOBOTÂNICAS:**
- especies: Array de objetos com {vernacular (nome comum), nomeCientifico} (opcional)
- tipo_de_uso: Tipo de uso das plantas (ex: medicinal, alimentar, ritual, combustível) (opcional)
- metodologia: Metodologia de pesquisa (ex: entrevistas, observação participante, levantamento) (opcional)
- comunidades: Array de nomes de comunidades indígenas ou tradicionais mencionadas (opcional)

**INFORMAÇÕES GEOGRÁFICAS:**
- pais: País onde o estudo foi realizado (opcional)
- estado: Estado ou região (opcional)
- municipio: Município (opcional)
- local: Local específico ou nome da comunidade/territorialidade (opcional)
- bioma: Bioma onde o estudo foi realizado (ex: Mata Atlântica, Cerrado, Amazônia) (opcional)
- regioes: Array de regiões mencionadas no estudo (opcional)

**IMPORTANTE:**
- Retorne APENAS um objeto JSON válido
- Extraia apenas informações que estão EXPLICITAMENTE mencionadas no texto
- Para campos não encontrados, omita-os do JSON
- Nomes científicos de plantas devem estar em formato binomial (Gênero species)
- Mantenha a ordem dos autores conforme aparecem no artigo`

  const systemPrompt = instructions || defaultInstructions

  try {
    const response = await fetch(
      `https://generativelanguage.googleapis.com/v1/models/gemini-1.5-flash:generateContent?key=${apiKey}`,
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
                  text: `${systemPrompt}\n\nExtraía os metadados do seguinte texto de artigo científico e retorne um objeto JSON válido:\n\n${pdfText}`,
                },
              ],
            },
          ],
          generationConfig: {
            temperature: 0.2,
            topK: 40,
            topP: 0.95,
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
    const metadata = JSON.parse(jsonStr.trim())

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
