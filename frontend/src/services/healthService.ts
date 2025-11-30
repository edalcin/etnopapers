import { apiCall } from './api'

export async function checkHealth() {
  try {
    const response = await apiCall('/health')
    return response.ok
  } catch {
    return false
  }
}

export async function checkOllamaHealth() {
  try {
    const response = await apiCall('/health/ollama')
    return response.ok
  } catch {
    return false
  }
}

export async function checkMongoDBHealth() {
  try {
    const response = await apiCall('/health/mongodb')
    return response.ok
  } catch {
    return false
  }
}
