/**
 * Zustand store for records management
 * Manages records list, selection, filtering, and loading state
 */

import { create } from 'zustand';
import { ArticleRecord } from '@shared/types/article';

export interface RecordsFilter {
  search?: string;
  year?: number;
  author?: string;
  status?: 'local' | 'pending' | 'synced' | 'failed';
}

interface RecordsStore {
  // State
  records: ArticleRecord[];
  selectedIds: Set<string>;
  filters: RecordsFilter;
  isLoading: boolean;
  error: string | null;

  // Actions
  setRecords: (records: ArticleRecord[]) => void;
  addRecord: (record: ArticleRecord) => void;
  updateRecord: (id: string, updates: Partial<ArticleRecord>) => void;
  deleteRecord: (id: string) => void;
  deleteRecords: (ids: string[]) => void;

  // Selection
  selectRecord: (id: string) => void;
  deselectRecord: (id: string) => void;
  toggleRecord: (id: string) => void;
  selectAll: () => void;
  deselectAll: () => void;

  // Filtering
  setFilters: (filters: RecordsFilter) => void;
  clearFilters: () => void;
  getFilteredRecords: () => ArticleRecord[];

  // Loading & Error
  setLoading: (loading: boolean) => void;
  setError: (error: string | null) => void;
}

export const useRecordsStore = create<RecordsStore>((set, get) => ({
  // Initial state
  records: [],
  selectedIds: new Set(),
  filters: {},
  isLoading: false,
  error: null,

  // Actions
  setRecords: (records) => set({ records }),
  addRecord: (record) =>
    set((state) => ({
      records: [record, ...state.records],
    })),
  updateRecord: (id, updates) =>
    set((state) => ({
      records: state.records.map((r) => (r._id === id ? { ...r, ...updates } : r)),
    })),
  deleteRecord: (id) =>
    set((state) => ({
      records: state.records.filter((r) => r._id !== id),
      selectedIds: new Set([...state.selectedIds].filter((s) => s !== id)),
    })),
  deleteRecords: (ids) => {
    const idSet = new Set(ids);
    set((state) => ({
      records: state.records.filter((r) => !idSet.has(r._id)),
      selectedIds: new Set([...state.selectedIds].filter((s) => !idSet.has(s))),
    }));
  },

  // Selection
  selectRecord: (id) =>
    set((state) => ({
      selectedIds: new Set([...state.selectedIds, id]),
    })),
  deselectRecord: (id) =>
    set((state) => {
      const newIds = new Set(state.selectedIds);
      newIds.delete(id);
      return { selectedIds: newIds };
    }),
  toggleRecord: (id) =>
    set((state) => {
      const newIds = new Set(state.selectedIds);
      if (newIds.has(id)) {
        newIds.delete(id);
      } else {
        newIds.add(id);
      }
      return { selectedIds: newIds };
    }),
  selectAll: () =>
    set((state) => ({
      selectedIds: new Set(state.records.map((r) => r._id)),
    })),
  deselectAll: () =>
    set(() => ({
      selectedIds: new Set(),
    })),

  // Filtering
  setFilters: (filters) => set({ filters }),
  clearFilters: () => set({ filters: {} }),
  getFilteredRecords: () => {
    const { records, filters } = get();

    return records.filter((record) => {
      // Search filter
      if (filters.search) {
        const searchLower = filters.search.toLowerCase();
        const matchTitle = record.titulo.toLowerCase().includes(searchLower);
        const matchAuthor = record.autores.some((a) => a.toLowerCase().includes(searchLower));
        const matchAno = record.ano.toString().includes(searchLower);

        if (!matchTitle && !matchAuthor && !matchAno) return false;
      }

      // Year filter
      if (filters.year && record.ano !== filters.year) return false;

      // Author filter
      if (filters.author && !record.autores.some((a) => a.toLowerCase().includes(filters.author!.toLowerCase()))) {
        return false;
      }

      // Status filter
      if (filters.status && record.syncStatus !== filters.status) return false;

      return true;
    });
  },

  // Loading & Error
  setLoading: (loading) => set({ isLoading: loading }),
  setError: (error) => set({ error }),
}));
