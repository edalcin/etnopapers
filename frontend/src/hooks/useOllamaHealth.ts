import { useState, useEffect } from 'react'

export interface OllamaHealth {
  status: 'ok' | 'unavailable' | 'checking'
  url?: string
  modelsCount?: number
  error?: string
  lastChecked?: Date
}

export function useOllamaHealth(checkInterval: number = 30000) {
  const [health, setHealth] = useState<OllamaHealth>({
    status: 'checking'
  })

  const checkHealth = async () => {
    setHealth(prev => ({ ...prev, status: 'checking' }))

    try {
      const response = await fetch('/api/health/ollama', {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json'
        }
      })

      if (response.ok) {
        const data = await response.json()

        setHealth({
          status: 'ok',
          url: data.url,
          modelsCount: data.models_count,
          error: undefined,
          lastChecked: new Date()
        })
      } else {
        const errorData = await response.json()

        setHealth({
          status: 'unavailable',
          error: errorData.detail || 'Ollama indisponível',
          lastChecked: new Date()
        })
      }
    } catch (err) {
      setHealth({
        status: 'unavailable',
        error: 'Não é possível conectar ao Ollama',
        lastChecked: new Date()
      })
    }
  }

  useEffect(() => {
    // Initial check
    checkHealth()

    // Set up interval for periodic checks
    const intervalId = setInterval(checkHealth, checkInterval)

    // Cleanup interval on unmount
    return () => clearInterval(intervalId)
  }, [checkInterval])

  return { health, checkHealth }
}
