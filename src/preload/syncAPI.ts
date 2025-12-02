/**
 * Sync API for preload script
 * Exposes MongoDB synchronization operations to renderer process
 */

import { ipcRenderer } from 'electron';
import { ArticleRecord } from '@shared/types/article';
import { SyncStatus, SyncResult } from '@shared/types/services';
import { MongoDBConfig } from '@shared/types/config';

/**
 * Sync API exposed to renderer process
 */
export const syncAPI = {
  /**
   * Test MongoDB connection
   */
  async testConnection(): Promise<boolean> {
    const result = await ipcRenderer.invoke('sync:testConnection');
    if (!result.success) {
      throw new Error(result.error);
    }
    return result.data;
  },

  /**
   * Upload single record to MongoDB
   */
  async uploadRecord(record: ArticleRecord): Promise<void> {
    const result = await ipcRenderer.invoke('sync:uploadRecord', record);
    if (!result.success) {
      throw new Error(result.error);
    }
  },

  /**
   * Upload batch of records to MongoDB
   */
  async uploadBatch(records: ArticleRecord[]): Promise<SyncResult> {
    const result = await ipcRenderer.invoke('sync:uploadBatch', records);
    if (!result.success) {
      throw new Error(result.error);
    }
    return result.data;
  },

  /**
   * Get synchronization status
   */
  async getStatus(): Promise<SyncStatus> {
    const result = await ipcRenderer.invoke('sync:getStatus');
    if (!result.success) {
      throw new Error(result.error);
    }
    return result.data;
  },

  /**
   * Delete record from MongoDB after successful sync
   */
  async deleteAfterSync(id: string): Promise<void> {
    const result = await ipcRenderer.invoke('sync:deleteAfterSync', id);
    if (!result.success) {
      throw new Error(result.error);
    }
  },

  /**
   * Update MongoDB configuration
   */
  async updateConfig(config: MongoDBConfig): Promise<void> {
    const result = await ipcRenderer.invoke('sync:updateConfig', config);
    if (!result.success) {
      throw new Error(result.error);
    }
  },
};
