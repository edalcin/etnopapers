import { create } from 'zustand'
import { persist } from 'zustand/middleware'
import type {
  AppState,
  AIProvider,
  ExtractedMetadata,
  Article,
  DraftState,
} from '@types'

// LocalStorage keys
const API_KEY_STORAGE_KEY = 'etnopapers_api_key'
const DRAFTS_STORAGE_KEY = 'etnopapers_drafts'

export const useStore = create<AppState>()(
  persist(
    (set, get) => ({
      // API Key state
      apiKey: {
        provider: 'gemini',
        key: '',
        isValid: false,
      },

      setAPIKey: (provider: AIProvider, key: string) => {
        set({
          apiKey: {
            provider,
            key,
            isValid: false, // Validity should be checked by API validation
          },
        })
      },

      setAPIKeyValidity: (valid: boolean) => {
        set(state => ({
          apiKey: {
            ...state.apiKey,
            isValid: valid,
          },
        }))
      },

      clearAPIKey: () => {
        set({
          apiKey: {
            provider: 'gemini',
            key: '',
            isValid: false,
          },
        })
      },

      // Upload state
      uploadFile: null,
      uploadProgress: 0,
      uploadError: null,

      setUploadFile: (file: File | null) => {
        set({
          uploadFile: file,
          uploadError: null,
          uploadProgress: 0,
        })
      },

      setUploadProgress: (progress: number) => {
        set({ uploadProgress: progress })
      },

      setUploadError: (error: string | null) => {
        set({ uploadError: error })
      },

      // Extract state
      extractedData: null,
      extractLoading: false,
      extractError: null,

      setExtractedData: (data: ExtractedMetadata) => {
        set({
          extractedData: data,
          extractError: null,
        })
      },

      setExtractLoading: (loading: boolean) => {
        set({ extractLoading: loading })
      },

      setExtractError: (error: string | null) => {
        set({ extractError: error })
      },

      // Draft state
      drafts: [],

      addDraft: (draft: DraftState) => {
        set(state => {
          const updatedDrafts = [...state.drafts, draft]
          return { drafts: updatedDrafts }
        })
      },

      removeDraft: (id: number) => {
        set(state => ({
          drafts: state.drafts.filter(d => d.id !== id),
        }))
      },

      clearDrafts: () => {
        set({ drafts: [] })
      },

      // Articles state
      articles: [],
      loadedArticles: false,

      setArticles: (articles: Article[]) => {
        set({
          articles,
          loadedArticles: true,
        })
      },

      addArticle: (article: Article) => {
        set(state => ({
          articles: [article, ...state.articles],
        }))
      },

      updateArticle: (id: number, updates: Partial<Article>) => {
        set(state => ({
          articles: state.articles.map(a =>
            a.id === id ? { ...a, ...updates } : a
          ),
        }))
      },

      // UI state
      sidebarOpen: true,

      toggleSidebar: () => {
        set(state => ({
          sidebarOpen: !state.sidebarOpen,
        }))
      },
    }),
    {
      name: 'etnopapers-store',
      partialize: (state: AppState) => ({
        apiKey: state.apiKey,
        drafts: state.drafts,
      }),
    }
  )
)

// Selector hooks for better performance
export const useAPIKey = () => useStore(s => s.apiKey)
export const useSetAPIKey = () => useStore(s => s.setAPIKey)
export const useSetAPIKeyValidity = () => useStore(s => s.setAPIKeyValidity)
export const useClearAPIKey = () => useStore(s => s.clearAPIKey)

export const useUploadState = () =>
  useStore(s => ({
    file: s.uploadFile,
    progress: s.uploadProgress,
    error: s.uploadError,
  }))
export const useSetUploadFile = () => useStore(s => s.setUploadFile)
export const useSetUploadProgress = () => useStore(s => s.setUploadProgress)
export const useSetUploadError = () => useStore(s => s.setUploadError)

export const useExtractState = () =>
  useStore(s => ({
    data: s.extractedData,
    loading: s.extractLoading,
    error: s.extractError,
  }))
export const useSetExtractedData = () => useStore(s => s.setExtractedData)
export const useSetExtractLoading = () => useStore(s => s.setExtractLoading)
export const useSetExtractError = () => useStore(s => s.setExtractError)

export const useDrafts = () => useStore(s => s.drafts)
export const useAddDraft = () => useStore(s => s.addDraft)
export const useRemoveDraft = () => useStore(s => s.removeDraft)
export const useClearDrafts = () => useStore(s => s.clearDrafts)

export const useArticles = () =>
  useStore(s => ({ articles: s.articles, loaded: s.loadedArticles }))
export const useSetArticles = () => useStore(s => s.setArticles)
export const useAddArticle = () => useStore(s => s.addArticle)
export const useUpdateArticle = () => useStore(s => s.updateArticle)

export const useSidebar = () => useStore(s => s.sidebarOpen)
export const useToggleSidebar = () => useStore(s => s.toggleSidebar)
