import { create } from 'zustand'

export interface Article {
  _id: string
  titulo: string
  ano_publicacao: number
  autores: Array<{ nome: string; sobrenome: string; email?: string }>
  doi?: string
  resumo?: string
  status: 'rascunho' | 'finalizado'
}

interface UploadState {
  file: File | null
  error: string | null
}

interface ExtractState {
  data: any
  loading: boolean
  error: string | null
}

interface StoreState {
  // API Key Management
  apiKey: string | null
  apiKeyValid: boolean
  setAPIKey: (key: string) => void
  setAPIKeyValidity: (valid: boolean) => void

  // Upload State
  uploadState: UploadState
  setUploadFile: (file: File | null) => void
  setUploadError: (error: string | null) => void

  // Articles
  articles: Article[]
  setArticles: (articles: Article[]) => void
  addArticle: (article: Article) => void

  // Extract State
  extractState: ExtractState
  setExtractedData: (data: any) => void
  setExtractLoading: (loading: boolean) => void
  setExtractError: (error: string | null) => void
  setCurrentDraft: (draft: any) => void
}

const useStoreBase = create<StoreState>((set) => ({
  // API Key Management
  apiKey: null,
  apiKeyValid: false,
  setAPIKey: (key: string) => set({ apiKey: key }),
  setAPIKeyValidity: (valid: boolean) => set({ apiKeyValid: valid }),

  // Upload State
  uploadState: { file: null, error: null },
  setUploadFile: (file: File | null) => set((state) => ({ uploadState: { ...state.uploadState, file } })),
  setUploadError: (error: string | null) => set((state) => ({ uploadState: { ...state.uploadState, error } })),

  // Articles
  articles: [],
  setArticles: (articles: Article[]) => set({ articles }),
  addArticle: (article: Article) => set((state) => ({ articles: [...state.articles, article] })),

  // Extract State
  extractState: { data: null, loading: false, error: null },
  setExtractedData: (data: any) => set((state) => ({ extractState: { ...state.extractState, data } })),
  setExtractLoading: (loading: boolean) => set((state) => ({ extractState: { ...state.extractState, loading } })),
  setExtractError: (error: string | null) => set((state) => ({ extractState: { ...state.extractState, error } })),
  setCurrentDraft: (draft: any) => set((state) => ({ extractState: { ...state.extractState, data: draft } })),
}))

// Hook-style selectors for cleaner component imports
export const useSetAPIKey = () => useStoreBase((state) => state.setAPIKey)
export const useSetAPIKeyValidity = () => useStoreBase((state) => state.setAPIKeyValidity)
export const useAPIKey = () => useStoreBase((state) => state.apiKey)

export const useSetUploadFile = () => useStoreBase((state) => state.setUploadFile)
export const useSetUploadError = () => useStoreBase((state) => state.setUploadError)
export const useUploadState = () => useStoreBase((state) => state.uploadState)

export const useArticles = () => useStoreBase((state) => state.articles)
export const useSetArticles = () => useStoreBase((state) => state.setArticles)

export const useSetExtractedData = () => useStoreBase((state) => state.setExtractedData)
export const useSetExtractLoading = () => useStoreBase((state) => state.setExtractLoading)
export const useSetExtractError = () => useStoreBase((state) => state.setExtractError)
export const useAddArticle = () => useStoreBase((state) => state.addArticle)
export const useCurrentDraft = () => useStoreBase((state) => state.extractState.data)
export const useSetCurrentDraft = () => useStoreBase((state) => state.setCurrentDraft)

// Export the base store for advanced usage if needed
export const useStore = useStoreBase
