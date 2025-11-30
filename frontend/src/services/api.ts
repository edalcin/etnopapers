const API_BASE_URL = '/api'

export async function apiCall(
  endpoint: string,
  options: RequestInit = {}
): Promise<Response> {
  const url = `${API_BASE_URL}${endpoint}`
  
  const defaultOptions: RequestInit = {
    headers: {
      'Content-Type': 'application/json',
    },
    ...options
  }

  try {
    const response = await fetch(url, defaultOptions)
    return response
  } catch (error) {
    throw new Error(`API call failed: ${error}`)
  }
}

export async function getArticles(limit: number = 10, skip: number = 0) {
  const response = await apiCall(`/articles?limit=${limit}&skip=${skip}`)
  if (!response.ok) throw new Error('Failed to fetch articles')
  return response.json()
}

export async function getArticle(id: string) {
  const response = await apiCall(`/articles/${id}`)
  if (!response.ok) throw new Error('Failed to fetch article')
  return response.json()
}

export async function createArticle(data: any) {
  const response = await apiCall('/articles', {
    method: 'POST',
    body: JSON.stringify(data)
  })
  if (!response.ok) {
    const error = await response.json()
    throw new Error(error.detail || 'Failed to create article')
  }
  return response.json()
}

export async function updateArticle(id: string, data: any) {
  const response = await apiCall(`/articles/${id}`, {
    method: 'PUT',
    body: JSON.stringify(data)
  })
  if (!response.ok) throw new Error('Failed to update article')
  return response.json()
}

export async function deleteArticle(id: string) {
  const response = await apiCall(`/articles/${id}`, {
    method: 'DELETE'
  })
  if (!response.ok) throw new Error('Failed to delete article')
  return response.json()
}

export async function searchArticles(query: string) {
  const response = await apiCall(`/articles/search?q=${encodeURIComponent(query)}`)
  if (!response.ok) throw new Error('Failed to search articles')
  return response.json()
}
