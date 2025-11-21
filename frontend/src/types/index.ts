// API Provider types
export type AIProvider = 'gemini' | 'openai' | 'claude'

// Article types
export interface Author {
  nome: string
  sobrenome?: string
  email?: string
}

export interface Article {
  id: number
  titulo: string
  doi?: string
  ano_publicacao: number
  autores: Author[]
  resumo?: string
  status: 'rascunho' | 'finalizado'
  editado_manualmente: boolean
  data_processamento: string
  data_ultima_modificacao: string
}

// Extract metadata types
export interface ExtractedMetadata {
  titulo: string
  doi?: string
  ano_publicacao: number
  autores: Author[]
  resumo?: string
  especies?: string[]
  regioes?: string[]
  comunidades?: string[]
}

// State types
export interface APIKeyState {
  provider: AIProvider
  key: string
  isValid: boolean
}

export interface UploadState {
  file: File | null
  loading: boolean
  error: string | null
  progress: number
}

export interface ExtractState {
  loading: boolean
  error: string | null
  data: ExtractedMetadata | null
}

export interface DraftState {
  id?: number
  data: ExtractedMetadata
  savedAt: string
}

export interface ResearcherProfile {
  nome: string
  instituicao: string
  foco_pesquisa: string
  regioes_interesse?: string[]
  comunidades_interesse?: string[]
}

export interface AppState {
  // API Key state
  apiKey: APIKeyState
  setAPIKey: (provider: AIProvider, key: string) => void
  setAPIKeyValidity: (valid: boolean) => void
  clearAPIKey: () => void

  // Upload state
  uploadFile: File | null
  uploadProgress: number
  uploadError: string | null
  setUploadFile: (file: File | null) => void
  setUploadProgress: (progress: number) => void
  setUploadError: (error: string | null) => void

  // Extract state
  extractedData: ExtractedMetadata | null
  extractLoading: boolean
  extractError: string | null
  setExtractedData: (data: ExtractedMetadata) => void
  setExtractLoading: (loading: boolean) => void
  setExtractError: (error: string | null) => void

  // Draft state
  drafts: DraftState[]
  addDraft: (draft: DraftState) => void
  removeDraft: (id: number) => void
  clearDrafts: () => void

  // Articles state
  articles: Article[]
  loadedArticles: boolean
  setArticles: (articles: Article[]) => void
  addArticle: (article: Article) => void
  updateArticle: (id: number, article: Partial<Article>) => void

  // UI state
  sidebarOpen: boolean
  toggleSidebar: () => void
}
