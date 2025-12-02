/**
 * Storage API for preload script
 * Exposes data storage operations to renderer process with IPC
 */

import { ipcRenderer } from 'electron';
import { ArticleRecord } from '@shared/types/article';

/**
 * Storage API exposed to renderer process
 */
export const storageAPI = {
  /**
   * Get all records
   */
  async getAll(): Promise<ArticleRecord[]> {
    const result = await ipcRenderer.invoke('storage:getAll');
    if (!result.success) {
      throw new Error(result.error);
    }
    return result.data;
  },

  /**
   * Get record by ID
   */
  async getById(id: string): Promise<ArticleRecord | null> {
    const result = await ipcRenderer.invoke('storage:getById', id);
    if (!result.success) {
      throw new Error(result.error);
    }
    return result.data;
  },

  /**
   * Create new record
   */
  async create(
    data: Omit<ArticleRecord, '_id' | 'createdAt' | 'updatedAt'>
  ): Promise<ArticleRecord> {
    const result = await ipcRenderer.invoke('storage:create', data);
    if (!result.success) {
      throw new Error(result.error);
    }
    return result.data;
  },

  /**
   * Update record
   */
  async update(id: string, data: Partial<ArticleRecord>): Promise<ArticleRecord> {
    const result = await ipcRenderer.invoke('storage:update', id, data);
    if (!result.success) {
      throw new Error(result.error);
    }
    return result.data;
  },

  /**
   * Delete record
   */
  async delete(id: string): Promise<void> {
    const result = await ipcRenderer.invoke('storage:delete', id);
    if (!result.success) {
      throw new Error(result.error);
    }
  },

  /**
   * Delete multiple records
   */
  async deleteMany(ids: string[]): Promise<void> {
    const result = await ipcRenderer.invoke('storage:deleteMany', ids);
    if (!result.success) {
      throw new Error(result.error);
    }
  },

  /**
   * Get total record count
   */
  async count(): Promise<number> {
    const result = await ipcRenderer.invoke('storage:count');
    if (!result.success) {
      throw new Error(result.error);
    }
    return result.data;
  },

  /**
   * Check if storage limit is not exceeded
   */
  async checkLimit(): Promise<boolean> {
    const result = await ipcRenderer.invoke('storage:checkLimit');
    if (!result.success) {
      throw new Error(result.error);
    }
    return result.data;
  },

  /**
   * Get storage usage percentage
   */
  async getUsage(): Promise<number> {
    const result = await ipcRenderer.invoke('storage:getUsage');
    if (!result.success) {
      throw new Error(result.error);
    }
    return result.data;
  },
};
