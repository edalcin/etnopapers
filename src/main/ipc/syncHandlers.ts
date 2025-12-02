/**
 * IPC handlers for MongoDB synchronization
 * Exposes sync operations to renderer process
 */

import { ipcMain } from 'electron';
import { MongoDBSyncService } from '../services/MongoDBSyncService';
import { getConfigService } from './configHandlers';
import { ArticleRecord } from '@shared/types/article';

let syncService: MongoDBSyncService | null = null;

/**
 * Initialize sync IPC handlers
 */
export async function registerSyncHandlers(): Promise<void> {
  const configService = getConfigService();
  const config = await configService.load();

  if (config.mongodb) {
    syncService = new MongoDBSyncService(config.mongodb);
  }

  /**
   * Channel: sync:testConnection
   * Tests MongoDB connection
   */
  ipcMain.handle('sync:testConnection', async () => {
    try {
      if (!syncService) {
        const config = await configService.load();
        if (!config.mongodb) {
          return { success: false, error: 'MongoDB not configured' };
        }
        syncService = new MongoDBSyncService(config.mongodb);
      }

      const connected = await syncService.testConnection();
      return { success: true, data: connected };
    } catch (error) {
      console.error('IPC sync:testConnection error:', error);
      return {
        success: false,
        error: error instanceof Error ? error.message : String(error),
      };
    }
  });

  /**
   * Channel: sync:uploadRecord
   * Uploads single record to MongoDB
   */
  ipcMain.handle('sync:uploadRecord', async (_, record: ArticleRecord) => {
    try {
      if (!syncService) {
        throw new Error('MongoDB sync service not initialized');
      }

      await syncService.uploadRecord(record);
      return { success: true };
    } catch (error) {
      console.error('IPC sync:uploadRecord error:', error);
      return {
        success: false,
        error: error instanceof Error ? error.message : String(error),
      };
    }
  });

  /**
   * Channel: sync:uploadBatch
   * Uploads multiple records to MongoDB
   */
  ipcMain.handle('sync:uploadBatch', async (_, records: ArticleRecord[]) => {
    try {
      if (!syncService) {
        throw new Error('MongoDB sync service not initialized');
      }

      const result = await syncService.uploadBatch(records);
      return { success: true, data: result };
    } catch (error) {
      console.error('IPC sync:uploadBatch error:', error);
      return {
        success: false,
        error: error instanceof Error ? error.message : String(error),
      };
    }
  });

  /**
   * Channel: sync:getStatus
   * Gets current sync status
   */
  ipcMain.handle('sync:getStatus', async () => {
    try {
      if (!syncService) {
        return {
          success: true,
          data: {
            totalRecords: 0,
            syncedRecords: 0,
            isConnected: false,
          },
        };
      }

      const status = await syncService.getStatus();
      return { success: true, data: status };
    } catch (error) {
      console.error('IPC sync:getStatus error:', error);
      return {
        success: false,
        error: error instanceof Error ? error.message : String(error),
      };
    }
  });

  /**
   * Channel: sync:deleteAfterSync
   * Deletes record from MongoDB after successful sync
   */
  ipcMain.handle('sync:deleteAfterSync', async (_, id: string) => {
    try {
      if (!syncService) {
        throw new Error('MongoDB sync service not initialized');
      }

      await syncService.deleteAfterSync(id);
      return { success: true };
    } catch (error) {
      console.error('IPC sync:deleteAfterSync error:', error);
      return {
        success: false,
        error: error instanceof Error ? error.message : String(error),
      };
    }
  });

  /**
   * Channel: sync:updateConfig
   * Updates MongoDB configuration for sync service
   */
  ipcMain.handle('sync:updateConfig', async (_, mongoConfig: any) => {
    try {
      if (!syncService) {
        syncService = new MongoDBSyncService(mongoConfig);
      } else {
        syncService.updateConfig(mongoConfig);
      }
      return { success: true };
    } catch (error) {
      console.error('IPC sync:updateConfig error:', error);
      return {
        success: false,
        error: error instanceof Error ? error.message : String(error),
      };
    }
  });
}

/**
 * Get sync service instance (for testing/other processes)
 */
export function getSyncService(): MongoDBSyncService {
  if (!syncService) {
    throw new Error('Sync service not initialized');
  }
  return syncService;
}
