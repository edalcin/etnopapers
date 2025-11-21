/**
 * Google Gemini API Client for ethnobotany metadata extraction
 */

import axios from 'axios'
import type { ExtractedMetadata, Author } from '@types'

const GEMINI_API_URL = 'https://generativelanguage.googleapis.com/v1/models/gemini-pro:generateContent'

interface GeminiRequest {
  contents: {
    parts: {
      text: string
    }[]
  }[]
}

interface GeminiResponse {
  candidates: {
    content: {
      parts: {
        text: string
      }[]
    }
  }[]
}

const EXTRACTION_PROMPT = `Você é um especialista em etnobotânica e processamento de documentos científicos.
Analise o seguinte texto de artigo científico e extraia APENAS os seguintes metadados em formato JSON válido:

{
  "titulo": "Título exato do artigo",
  "ano_publicacao": número (ano entre 1900-2100),
  "autores": [
    {"nome": "Nome", "sobrenome": "Sobrenome", "email": "email@exemplo.com"}
  ],
  "doi": "DOI se existir, senão null",
  "resumo": "Resumo do artigo",
  "especies": ["Nome científico 1", "Nome científico 2"],
  "regioes": ["Região 1", "Região 2"],
  "comunidades": ["Comunidade indígena 1", "Comunidade 2"]
}

IMPORTANTE:
- Retorne APENAS JSON válido, sem explicações
- Se algum campo não existir, use null
- Para autores, extraia todos os autores listados
- Para espécies, liste TODOS os nomes científicos mencionados
- Para comunidades, inclua nomes indígenas, quilombolas, ribeirinhos, etc.

Texto do artigo:
---
{TEXTO}
---

Retorne apenas o JSON:`

export async function extractWithGemini(
  pdfText: string,
  apiKey: string,
  researcherProfile?: string
): Promise<ExtractedMetadata> {
  try {
    if (!apiKey) {
      throw new Error('API key do Gemini não configurada')
    }

    const prompt = EXTRACTION_PROMPT.replace('{TEXTO}', pdfText.substring(0, 10000))

    const userPrompt = researcherProfile
      ? `${prompt}\n\nContexto do pesquisador: ${researcherProfile}`
      : prompt

    const request: GeminiRequest = {
      contents: [
        {
          parts: [
            {
              text: userPrompt,
            },
          ],
        },
      ],
    }

    const response = await axios.post<GeminiResponse>(GEMINI_API_URL, request, {
      params: { key: apiKey },
      timeout: 60000,
      headers: {
        'Content-Type': 'application/json',
      },
    })

    if (
      !response.data.candidates ||
      !response.data.candidates[0] ||
      !response.data.candidates[0].content
    ) {
      throw new Error('Resposta vazia do Gemini')
    }

    const text = response.data.candidates[0].content.parts[0].text
    const jsonMatch = text.match(/\{[\s\S]*\}/)

    if (!jsonMatch) {
      throw new Error('Resposta do Gemini não contém JSON válido')
    }

    const parsed = JSON.parse(jsonMatch[0])

    // Validate and normalize response
    return normalizeMetadata(parsed)
  } catch (error) {
    if (axios.isAxiosError(error)) {
      const message = error.response?.data?.error?.message || error.message
      throw new Error(`Erro Gemini: ${message}`)
    }
    throw error
  }
}

function normalizeMetadata(data: any): ExtractedMetadata {
  return {
    titulo: data.titulo || '',
    ano_publicacao: parseInt(data.ano_publicacao) || new Date().getFullYear(),
    autores: (data.autores || []).map((a: any) => ({
      nome: a.nome || '',
      sobrenome: a.sobrenome,
      email: a.email,
    })),
    doi: data.doi,
    resumo: data.resumo,
    especies: data.especies || [],
    regioes: data.regioes || [],
    comunidades: data.comunidades || [],
  }
}

export async function validateGeminiKey(apiKey: string): Promise<boolean> {
  try {
    const response = await axios.post(
      GEMINI_API_URL,
      {
        contents: [
          {
            parts: [
              {
                text: 'test',
              },
            ],
          },
        ],
      },
      {
        params: { key: apiKey },
        timeout: 5000,
      }
    )
    return response.status === 200
  } catch (error) {
    return false
  }
}
