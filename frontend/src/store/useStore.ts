import { create } from 'zustand'

interface Article {
  _id: string
  titulo: string
  ano_publicacao: number
  autores: Array<{ nome: string; sobrenome: string; email?: string }>
  doi?: string
  resumo?: string
  status: 'rascunho' | 'finalizado'
  data_processamento: string
  data_ultima_modificacao: string
  editado_manualmente: boolean
  metadata_estudo?: Record<string, any>
  especies?: Array<Record<string, any>>
  comunidades?: Array<Record<string, any>>
  localizacoes?: Array<Record<string, any>>
}

interface ExtractedMetadata {
  titulo: string
  autores: Array<{ nome: string; sobrenome: string; email?: string }>
  ano_publicacao: number
  resumo?: string
  doi?: string
}

interface Store {
  apiProvider: 'gemini' | 'openai' | 'claude' | null
  apiKey: string | null
  setApiProvider: (provider: 'gemini' | 'openai' | 'claude') => void
  setApiKey: (key: string) => void
  extractedMetadata: ExtractedMetadata | null
  setExtractedMetadata: (metadata: ExtractedMetadata | null) => void
  articles: Article[]
  setArticles: (articles: Article[]) => void
  currentDraft: ExtractedMetadata | null
  setCurrentDraft: (draft: ExtractedMetadata | null) => void
  researcherProfile: Record<string, any> | null
  setResearcherProfile: (profile: Record<string, any> | null) => void
}

export const useStore = create<Store>((set) => ({
  apiProvider: null,
  apiKey: null,
  setApiProvider: (provider) => set({ apiProvider: provider }),
  setApiKey: (key) => set({ apiKey: key }),
  extractedMetadata: null,
  setExtractedMetadata: (metadata) => set({ extractedMetadata: metadata }),
  articles: [],
  setArticles: (articles) => set({ articles }),
  currentDraft: null,
  setCurrentDraft: (draft) => set({ currentDraft: draft }),
  researcherProfile: null,
  setResearcherProfile: (profile) => set({ researcherProfile: profile }),
}))
