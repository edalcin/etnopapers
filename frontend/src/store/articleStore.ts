import { create } from 'zustand'

interface ArticleFilters {
  searchQuery: string
  sortBy: string
  sortOrder: 'asc' | 'desc'
  page: number
  limit: number
}

interface ArticleStoreState {
  filters: ArticleFilters
  setSearchQuery: (query: string) => void
  setSortBy: (field: string) => void
  setSortOrder: (order: 'asc' | 'desc') => void
  setPage: (page: number) => void
  setLimit: (limit: number) => void
  resetFilters: () => void
}

const defaultFilters: ArticleFilters = {
  searchQuery: '',
  sortBy: 'titulo',
  sortOrder: 'asc',
  page: 1,
  limit: 20
}

export const useArticleStore = create<ArticleStoreState>((set) => ({
  filters: defaultFilters,
  
  setSearchQuery: (searchQuery) =>
    set((state) => ({
      filters: { ...state.filters, searchQuery, page: 1 }
    })),
  
  setSortBy: (sortBy) =>
    set((state) => ({
      filters: { ...state.filters, sortBy }
    })),
  
  setSortOrder: (sortOrder) =>
    set((state) => ({
      filters: { ...state.filters, sortOrder }
    })),
  
  setPage: (page) =>
    set((state) => ({
      filters: { ...state.filters, page }
    })),
  
  setLimit: (limit) =>
    set((state) => ({
      filters: { ...state.filters, limit, page: 1 }
    })),
  
  resetFilters: () =>
    set({ filters: defaultFilters })
}))
