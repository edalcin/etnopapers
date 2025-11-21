/**
 * Anthropic Claude API Client for ethnobotany metadata extraction
 */

import axios from 'axios'
import type { ExtractedMetadata } from '@types'

const CLAUDE_API_URL = 'https://api.anthropic.com/v1/messages'
const CLAUDE_VERSION = '2024-01-15'

interface ClaudeMessage {
  role: 'user' | 'assistant'
  content: string
}

interface ClaudeRequest {
  model: string
  max_tokens: number
  system: string
  messages: ClaudeMessage[]
}

interface ClaudeResponse {
  content: {
    type: string
    text: string
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

export async function extractWithClaude(
  pdfText: string,
  apiKey: string,
  researcherProfile?: string
): Promise<ExtractedMetadata> {
  try {
    if (!apiKey) {
      throw new Error('API key do Claude não configurada')
    }

    let userPrompt = `Analise o seguinte texto e extraia os metadados:\n\n---\n${pdfText.substring(0, 10000)}\n---`

    if (researcherProfile) {
      userPrompt += `\n\nContexto do pesquisador: ${researcherProfile}`
    }

    const request: ClaudeRequest = {
      model: 'claude-3-haiku-20240307',
      max_tokens: 2000,
      system: SYSTEM_PROMPT,
      messages: [
        {
          role: 'user',
          content: userPrompt,
        },
      ],
    }

    const response = await axios.post<ClaudeResponse>(CLAUDE_API_URL, request, {
      headers: {
        'x-api-key': apiKey,
        'anthropic-version': CLAUDE_VERSION,
        'Content-Type': 'application/json',
      },
    })

    const responseText = response.data.content[0].text

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
      throw new Error(`Erro Claude: ${message}`)
    }
    throw error
  }
}

export async function validateClaudeKey(apiKey: string): Promise<boolean> {
  try {
    const response = await axios.post<ClaudeResponse>(
      CLAUDE_API_URL,
      {
        model: 'claude-3-haiku-20240307',
        max_tokens: 10,
        messages: [
          {
            role: 'user',
            content: 'Say "OK"',
          },
        ],
      },
      {
        headers: {
          'x-api-key': apiKey,
          'anthropic-version': CLAUDE_VERSION,
          'Content-Type': 'application/json',
        },
      }
    )

    return !!(response.data.content && response.data.content.length > 0)
  } catch {
    return false
  }
}
