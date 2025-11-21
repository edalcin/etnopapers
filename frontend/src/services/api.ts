import axios from 'axios'

const API_BASE_URL = 'http://localhost:8000/api'

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
})

export const articlesAPI = {
  list: (page: number = 1, pageSize: number = 50) =>
    api.get('/articles', { params: { page, page_size: pageSize } }),
  get: (id: string) => api.get(`/articles/${id}`),
  create: (data: any) => api.post('/articles', data),
  update: (id: string, data: any) => api.put(`/articles/${id}`, data),
  delete: (id: string) => api.delete(`/articles/${id}`),
}

export const speciesAPI = {
  validate: (name: string) => api.post('/species/validate', { nome_cientifico: name }),
}

export const databaseAPI = {
  info: () => api.get('/database/info'),
}

export default api
