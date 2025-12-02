/**
 * IPC handlers for data storage operations
 * Exposes storage service methods to renderer process
 */

import { ipcMain } from 'electron';
import { DataStorageService } from '../services/DataStorageService';
import { ArticleRecord } from '@shared/types/article';

let storageService: DataStorageService | null = null;

/**
 * Initialize storage IPC handlers
 */
export async function registerStorageHandlers(): Promise<void> {
  storageService = new DataStorageService();
  await storageService.initialize();

  /**
   * Channel: storage:getAll
   * Retrieves all stored records
   */
  ipcMain.handle('storage:getAll', async () => {
    try {
      const records = await storageService!.getAll();
      return { success: true, data: records };
    } catch (error) {
      console.error('IPC storage:getAll error:', error);
      return {
        success: false,
        error: error instanceof Error ? error.message : String(error),
      };
    }
  });

  /**
   * Channel: storage:getById
   * Retrieves a specific record by ID
   */
  ipcMain.handle('storage:getById', async (_, id: string) => {
    try {
      const record = await storageService!.getById(id);
      return { success: true, data: record };
    } catch (error) {
      console.error('IPC storage:getById error:', error);
      return {
        success: false,
        error: error instanceof Error ? error.message : String(error),
      };
    }
  });

  /**
   * Channel: storage:create
   * Creates a new record
   */
  ipcMain.handle(
    'storage:create',
    async (_, data: Omit<ArticleRecord, '_id' | 'createdAt' | 'updatedAt'>) => {
      try {
        const record = await storageService!.create(data);
        return { success: true, data: record };
      } catch (error) {
        console.error('IPC storage:create error:', error);
        return {
          success: false,
          error: error instanceof Error ? error.message : String(error),
        };
      }
    }
  );

  /**
   * Channel: storage:update
   * Updates an existing record
   */
  ipcMain.handle(
    'storage:update',
    async (_, id: string, data: Partial<ArticleRecord>) => {
      try {
        const record = await storageService!.update(id, data);
        return { success: true, data: record };
      } catch (error) {
        console.error('IPC storage:update error:', error);
        return {
          success: false,
          error: error instanceof Error ? error.message : String(error),
        };
      }
    }
  );

  /**
   * Channel: storage:delete
   * Deletes a record by ID
   */
  ipcMain.handle('storage:delete', async (_, id: string) => {
    try {
      await storageService!.delete(id);
      return { success: true };
    } catch (error) {
      console.error('IPC storage:delete error:', error);
      return {
        success: false,
        error: error instanceof Error ? error.message : String(error),
      };
    }
  });

  /**
   * Channel: storage:deleteMany
   * Deletes multiple records by IDs
   */
  ipcMain.handle('storage:deleteMany', async (_, ids: string[]) => {
    try {
      await storageService!.deleteMany(ids);
      return { success: true };
    } catch (error) {
      console.error('IPC storage:deleteMany error:', error);
      return {
        success: false,
        error: error instanceof Error ? error.message : String(error),
      };
    }
  });

  /**
   * Channel: storage:count
   * Gets total record count
   */
  ipcMain.handle('storage:count', async () => {
    try {
      const count = await storageService!.count();
      return { success: true, data: count };
    } catch (error) {
      console.error('IPC storage:count error:', error);
      return {
        success: false,
        error: error instanceof Error ? error.message : String(error),
      };
    }
  });

  /**
   * Channel: storage:checkLimit
   * Checks if storage limit is not exceeded
   */
  ipcMain.handle('storage:checkLimit', async () => {
    try {
      const withinLimit = await storageService!.checkLimit();
      return { success: true, data: withinLimit };
    } catch (error) {
      console.error('IPC storage:checkLimit error:', error);
      return {
        success: false,
        error: error instanceof Error ? error.message : String(error),
      };
    }
  });

  /**
   * Channel: storage:getUsage
   * Gets storage usage percentage
   */
  ipcMain.handle('storage:getUsage', async () => {
    try {
      const usage = await storageService!.getUsagePercentage();
      return { success: true, data: usage };
    } catch (error) {
      console.error('IPC storage:getUsage error:', error);
      return {
        success: false,
        error: error instanceof Error ? error.message : String(error),
      };
    }
  });
}

/**
 * Get storage service instance (for testing/other processes)
 */
export function getStorageService(): DataStorageService {
  if (!storageService) {
    throw new Error('Storage service not initialized');
  }
  return storageService;
}
