import axios from 'axios'

// Auto-detect API base URL: in production, use relative path to current host
// In development, use localhost:8000
const getAPIBaseURL = (): string => {
  // If running on localhost (dev), use direct connection to API port
  if (window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1') {
    return 'http://localhost:8000/api'
  }
  // In production, use relative path (same host)
  // e.g., if accessing http://192.168.1.10:8007, use /api
  return '/api'
}

const API_BASE_URL = getAPIBaseURL()

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
})

export const articlesAPI = {
  list: (page: number = 1, pageSize: number = 50) =>
    api.get('/referencias', { params: { page, page_size: pageSize } }),
  get: (id: string) => api.get(`/referencias/${id}`),
  create: (data: any) => api.post('/referencias', data),
  update: (id: string, data: any) => api.put(`/referencias/${id}`, data),
  delete: (id: string) => api.delete(`/referencias/${id}`),
  // TODO: Implement check-duplicate endpoint in backend when needed
  // checkDuplicate: (data: any) => api.post('/referencias/check-duplicate', data),
}

export const speciesAPI = {
  validate: (name: string) => api.post('/species/validate', { nome_cientifico: name }),
}

export const databaseAPI = {
  info: () => api.get('/database/info'),
  download: () => api.get('/database/download', { responseType: 'blob' }),
}

export const validateAPIKey = async (provider: string, key: string): Promise<boolean> => {
  try {
    // This is a stub - actual validation would call the provider's API
    // For now, just check if the key is non-empty
    return key.trim().length > 0
  } catch {
    return false
  }
}

export default api
