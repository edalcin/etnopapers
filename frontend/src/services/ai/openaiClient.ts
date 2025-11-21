/**
 * OpenAI ChatGPT API Client for ethnobotany metadata extraction
 */

import axios from 'axios'
import type { ExtractedMetadata, Author } from '@types'

const OPENAI_API_URL = 'https://api.openai.com/v1/chat/completions'

interface OpenAIMessage {
  role: 'system' | 'user'
  content: string
}

interface OpenAIRequest {
  model: string
  messages: OpenAIMessage[]
  temperature: number
  max_tokens: number
}

interface OpenAIResponse {
  choices: {
    message: {
      content: string
    }
  }[]
}

const SYSTEM_PROMPT = `Você é um especialista em etnobotânica e processamento de documentos científicos.
Analise o texto do artigo científico fornecido e extraia APENAS os seguintes metadados em formato JSON válido:

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
- Para comunidades, inclua nomes indígenas, quilombolas, ribeirinhos, caiçaras, etc.`

export async function extractWithOpenAI(
  pdfText: string,
  apiKey: string,
  researcherProfile?: string
): Promise<ExtractedMetadata> {
  try {
    if (!apiKey) {
      throw new Error('API key do OpenAI não configurada')
    }

    let userPrompt = `Analise o seguinte texto e extraia os metadados:\n\n---\n${pdfText.substring(0, 10000)}\n---`

    if (researcherProfile) {
      userPrompt += `\n\nContexto do pesquisador: ${researcherProfile}`
    }

    const request: OpenAIRequest = {
      model: 'gpt-3.5-turbo',
      messages: [
        {
          role: 'system',
          content: SYSTEM_PROMPT,
        },
        {
          role: 'user',
          content: userPrompt,
        },
      ],
      temperature: 0.3,
      max_tokens: 2000,
    }

    const response = await axios.post<OpenAIResponse>(OPENAI_API_URL, request, {
      headers: {
        Authorization: `Bearer ${apiKey}`,
        'Content-Type': 'application/json',
      },
    })

    const responseText = response.data.choices[0].message.content

    // Extract JSON from response
    const jsonMatch = responseText.match(/\{[\s\S]*\}/)
    if (!jsonMatch) {
      throw new Error('Resposta inválida: não conseguiu extrair JSON')
    }

    const metadata = JSON.parse(jsonMatch[0]) as ExtractedMetadata

    return {
      titulo: metadata.titulo || '',
      ano_publicacao: metadata.ano_publicacao || new Date().getFullYear(),
      autores: metadata.autores || [],
      doi: metadata.doi || undefined,
      resumo: metadata.resumo || undefined,
      especies: metadata.especies || [],
      regioes: metadata.regioes || [],
      comunidades: metadata.comunidades || [],
    }
  } catch (error) {
    if (axios.isAxiosError(error)) {
      const message = error.response?.data?.error?.message || error.message
      throw new Error(`Erro OpenAI: ${message}`)
    }
    throw error
  }
}

export async function validateOpenAIKey(apiKey: string): Promise<boolean> {
  try {
    const response = await axios.post<OpenAIResponse>(
      OPENAI_API_URL,
      {
        model: 'gpt-3.5-turbo',
        messages: [
          {
            role: 'user',
            content: 'Say "OK"',
          },
        ],
        max_tokens: 10,
      },
      {
        headers: {
          Authorization: `Bearer ${apiKey}`,
          'Content-Type': 'application/json',
        },
      }
    )

    return !!(response.data.choices && response.data.choices.length > 0)
  } catch {
    return false
  }
}
