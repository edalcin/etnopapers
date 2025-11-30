import { create } from 'zustand'
import { persist } from 'zustand/middleware'

interface ConfigState {
  mongoUri: string | null
  ollamaUrl: string | null
  ollamaModel: string | null
  isConfigured: boolean
  setMongoUri: (uri: string) => void
  setOllamaConfig: (url: string, model: string) => void
  loadConfig: () => Promise<void>
  reset: () => void
}

export const useConfigStore = create<ConfigState>()(
  persist(
    (set) => ({
      mongoUri: null,
      ollamaUrl: null,
      ollamaModel: null,
      isConfigured: false,
      
      setMongoUri: (uri: string) => {
        set({ mongoUri: uri, isConfigured: true })
      },
      
      setOllamaConfig: (url: string, model: string) => {
        set({ ollamaUrl: url, ollamaModel: model })
      },
      
      loadConfig: async () => {
        try {
          const response = await fetch('/api/config/status')
          if (response.ok) {
            const data = await response.json()
            set({
              isConfigured: data.configurado,
              mongoUri: data.configurado ? 'configured' : null
            })
          }
        } catch (error) {
          console.error('Failed to load config:', error)
        }
      },
      
      reset: () => {
        set({
          mongoUri: null,
          ollamaUrl: null,
          ollamaModel: null,
          isConfigured: false
        })
      }
    }),
    {
      name: 'config-store'
    }
  )
)
