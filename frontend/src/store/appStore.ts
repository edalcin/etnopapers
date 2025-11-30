import { create } from 'zustand'

type AppState = 'loading' | 'unconfigured' | 'configured' | 'error'

interface AppStoreState {
  appState: AppState
  error: string | null
  setAppState: (state: AppState) => void
  setError: (error: string | null) => void
  initialize: () => Promise<void>
}

export const useAppStore = create<AppStoreState>((set) => ({
  appState: 'loading',
  error: null,
  
  setAppState: (appState: AppState) => set({ appState }),
  setError: (error: string | null) => set({ error }),
  
  initialize: async () => {
    try {
      set({ appState: 'loading', error: null })
      
      // Check API health
      const healthResponse = await fetch('/api/health')
      if (!healthResponse.ok) {
        throw new Error('API indisponivel')
      }
      
      // Check configuration status
      const configResponse = await fetch('/api/config/status')
      const configData = await configResponse.json()
      
      if (configData.configurado) {
        set({ appState: 'configured' })
      } else {
        set({ appState: 'unconfigured' })
      }
    } catch (error) {
      set({
        appState: 'error',
        error: error instanceof Error ? error.message : 'Erro desconhecido'
      })
    }
  }
}))
