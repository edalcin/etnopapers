import axios from 'axios'

// Determine API base URL based on environment
const getApiBaseUrl = (): string => {
  // If explicitly set via environment variable, use it
  if (process.env.REACT_APP_API_URL) {
    return process.env.REACT_APP_API_URL
  }

  // Check if running in Docker container or production
  const hostname = window.location.hostname
  const isDocker = hostname !== 'localhost' && hostname !== '127.0.0.1'

  // In development with Vite dev server
  if (import.meta.env.DEV) {
    // If accessed from Docker host address, use that for API calls
    if (isDocker) {
      return `http://${hostname}:8000/api`
    }
    // Otherwise use localhost
    return 'http://localhost:8000/api'
  }

  // In production, use relative URL (works with backend serving frontend)
  return '/api'
}

const API_BASE_URL = getApiBaseUrl()

// Log API URL for debugging
console.log(`Etnopapers API URL: ${API_BASE_URL}`)

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 30000, // 30 second timeout
})

// Add response interceptor for better error handling
apiClient.interceptors.response.use(
  response => response,
  error => {
    if (error.response) {
      // Server responded with error status
      console.error(`API Error [${error.response.status}]:`, error.response.data)
    } else if (error.request) {
      // Request was made but no response
      console.error('API Error: No response from server', error.request)
    } else {
      // Error in request setup
      console.error('API Error:', error.message)
    }
    return Promise.reject(error)
  }
)

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
      'https://generativelanguage.googleapis.com/v1/models/gemini-1.5-flash:generateContent',
      {
        contents: [
          {
            parts: [
              {
                text: 'Say ok',
              },
            ],
          },
        ],
      },
      {
        params: { key: apiKey },
        timeout: 10000, // Increased timeout to 10 seconds
      }
    )
    // Success if we get a response (even if it's a warning)
    return !!response.data
  } catch (error) {
    console.error('Gemini validation error:', error)
    return false
  }
}

async function validateOpenAIKey(apiKey: string): Promise<boolean> {
  try {
    const response = await axios.get('https://api.openai.com/v1/models', {
      headers: {
        Authorization: `Bearer ${apiKey}`,
      },
      timeout: 10000, // Increased timeout to 10 seconds
    })
    return response.status === 200
  } catch (error) {
    console.error('OpenAI validation error:', error)
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
            content: 'Say ok',
          },
        ],
      },
      {
        headers: {
          'anthropic-version': '2024-01-15',
          'x-api-key': apiKey,
        },
        timeout: 10000, // Increased timeout to 10 seconds
      }
    )
    return response.status === 200
  } catch (error) {
    console.error('Claude validation error:', error)
    return false
  }
}

export default apiClient
