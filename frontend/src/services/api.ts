import axios from 'axios'

const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:8000/api'

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
})

// Articles
export const articlesAPI = {
  list: (page = 1, pageSize = 50, status?: string, search?: string) =>
    apiClient.get('/articles', {
      params: { page, page_size: pageSize, status, search },
    }),
  create: (data: any) => apiClient.post('/articles', data),
  get: (id: number) => apiClient.get(`/articles/${id}`),
  update: (id: number, data: any) => apiClient.put(`/articles/${id}`, data),
  delete: (id: number) => apiClient.delete(`/articles/${id}`),
  checkDuplicate: (data: any) =>
    apiClient.post('/articles/check-duplicate', data),
  getSimilar: (id: number, limit = 5) =>
    apiClient.get(`/articles/${id}/similar`, { params: { limit } }),
}

// Species
export const speciesAPI = {
  validate: (nomesCientifico: string) =>
    apiClient.post('/species/validate', { nome_cientifico: nomesCientifico }),
  validateBulk: (species: string[]) =>
    apiClient.post('/species/validate-bulk', { species }),
  cacheStats: () => apiClient.get('/species/cache-stats'),
  clearCache: () => apiClient.post('/species/clear-cache'),
  clearExpired: () => apiClient.post('/species/clear-expired'),
}

// Health check
export const healthAPI = {
  check: () => apiClient.get('/health'),
}

// Validate API keys (test calls to AI providers)
export const validateAPIKey = async (
  provider: string,
  apiKey: string
): Promise<boolean> => {
  try {
    switch (provider) {
      case 'gemini':
        return await validateGeminiKey(apiKey)
      case 'openai':
        return await validateOpenAIKey(apiKey)
      case 'claude':
        return await validateClaudeKey(apiKey)
      default:
        return false
    }
  } catch (error) {
    console.error(`Error validating ${provider} key:`, error)
    return false
  }
}

async function validateGeminiKey(apiKey: string): Promise<boolean> {
  try {
    const response = await axios.post(
      'https://generativelanguage.googleapis.com/v1/models/gemini-pro:generateContent',
      {
        contents: [
          {
            parts: [
              {
                text: 'Test',
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

async function validateOpenAIKey(apiKey: string): Promise<boolean> {
  try {
    const response = await axios.get('https://api.openai.com/v1/models', {
      headers: {
        Authorization: `Bearer ${apiKey}`,
      },
      timeout: 5000,
    })
    return response.status === 200
  } catch (error) {
    return false
  }
}

async function validateClaudeKey(apiKey: string): Promise<boolean> {
  try {
    const response = await axios.post(
      'https://api.anthropic.com/v1/messages',
      {
        model: 'claude-3-haiku-20240307',
        max_tokens: 10,
        messages: [
          {
            role: 'user',
            content: 'test',
          },
        ],
      },
      {
        headers: {
          'anthropic-version': '2023-06-01',
          'x-api-key': apiKey,
        },
        timeout: 5000,
      }
    )
    return response.status === 200
  } catch (error) {
    return false
  }
}

export default apiClient
