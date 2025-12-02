/**
 * Records management hook
 * Provides CRUD operations and state management for records
 */

import { useCallback } from 'react';
import { useRecordsStore } from '../stores/useRecordsStore';
import { ArticleRecord } from '@shared/types/article';

export const useRecordsManager = () => {
  const store = useRecordsStore();

  const loadRecords = useCallback(async () => {
    try {
      store.setLoading(true);
      store.setError(null);
      const records = await window.etnopapers.storage.getAll();
      store.setRecords(records);
    } catch (error) {
      const errorMsg = error instanceof Error ? error.message : String(error);
      store.setError(errorMsg);
      console.error('Failed to load records:', error);
    } finally {
      store.setLoading(false);
    }
  }, [store]);

  const createRecord = useCallback(
    async (data: Omit<ArticleRecord, '_id' | 'createdAt' | 'updatedAt'>) => {
      try {
        store.setLoading(true);
        store.setError(null);
        const record = await window.etnopapers.storage.create(data);
        store.addRecord(record);
        return record;
      } catch (error) {
        const errorMsg = error instanceof Error ? error.message : String(error);
        store.setError(errorMsg);
        throw error;
      } finally {
        store.setLoading(false);
      }
    },
    [store]
  );

  const updateRecord = useCallback(
    async (id: string, updates: Partial<ArticleRecord>) => {
      try {
        store.setLoading(true);
        store.setError(null);
        const updated = await window.etnopapers.storage.update(id, updates);
        store.updateRecord(id, updates);
        return updated;
      } catch (error) {
        const errorMsg = error instanceof Error ? error.message : String(error);
        store.setError(errorMsg);
        throw error;
      } finally {
        store.setLoading(false);
      }
    },
    [store]
  );

  const deleteRecord = useCallback(
    async (id: string) => {
      try {
        store.setLoading(true);
        store.setError(null);
        await window.etnopapers.storage.delete(id);
        store.deleteRecord(id);
      } catch (error) {
        const errorMsg = error instanceof Error ? error.message : String(error);
        store.setError(errorMsg);
        throw error;
      } finally {
        store.setLoading(false);
      }
    },
    [store]
  );

  const deleteSelectedRecords = useCallback(async () => {
    const ids = Array.from(store.selectedIds);
    if (ids.length === 0) return;

    try {
      store.setLoading(true);
      store.setError(null);
      await window.etnopapers.storage.deleteMany(ids);
      store.deleteRecords(ids);
    } catch (error) {
      const errorMsg = error instanceof Error ? error.message : String(error);
      store.setError(errorMsg);
      throw error;
    } finally {
      store.setLoading(false);
    }
  }, [store]);

  return {
    // State
    records: store.records,
    selectedIds: store.selectedIds,
    filters: store.filters,
    isLoading: store.isLoading,
    error: store.error,
    filteredRecords: store.getFilteredRecords(),

    // Record operations
    loadRecords,
    createRecord,
    updateRecord,
    deleteRecord,
    deleteSelectedRecords,

    // Selection operations
    selectRecord: store.selectRecord,
    deselectRecord: store.deselectRecord,
    toggleRecord: store.toggleRecord,
    selectAll: store.selectAll,
    deselectAll: store.deselectAll,

    // Filter operations
    setFilters: store.setFilters,
    clearFilters: store.clearFilters,
  };
};
