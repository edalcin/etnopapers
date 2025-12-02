/**
 * IPC handlers for configuration operations
 * Exposes configuration service methods to renderer process
 */

import { ipcMain } from 'electron';
import { ConfigurationService } from '../services/ConfigurationService';
import { AppConfiguration } from '@shared/types/config';
import { MongoDBConfig, OLLAMAConfig } from '@shared/types/config';

let configService: ConfigurationService;

/**
 * Initialize configuration IPC handlers
 */
export function registerConfigHandlers(): void {
  configService = new ConfigurationService();

  /**
   * Channel: config:load
   * Loads entire configuration from storage
   */
  ipcMain.handle('config:load', async () => {
    try {
      const config = await configService.load();
      return { success: true, data: config };
    } catch (error) {
      console.error('IPC config:load error:', error);
      return {
        success: false,
        error: error instanceof Error ? error.message : String(error),
      };
    }
  });

  /**
   * Channel: config:get
   * Retrieves a specific configuration value by key
   */
  ipcMain.handle('config:get', async (_, key: string) => {
    try {
      const value = await configService.get(key as keyof AppConfiguration);
      return { success: true, data: value };
    } catch (error) {
      console.error('IPC config:get error:', error);
      return {
        success: false,
        error: error instanceof Error ? error.message : String(error),
      };
    }
  });

  /**
   * Channel: config:update
   * Updates a specific configuration value
   */
  ipcMain.handle('config:update', async (_, key: string, value: any) => {
    try {
      await configService.update(key as keyof AppConfiguration, value);
      return { success: true };
    } catch (error) {
      console.error('IPC config:update error:', error);
      return {
        success: false,
        error: error instanceof Error ? error.message : String(error),
      };
    }
  });

  /**
   * Channel: config:updateOllama
   * Updates OLLAMA configuration
   */
  ipcMain.handle('config:updateOllama', async (_, ollamaConfig: OLLAMAConfig) => {
    try {
      await configService.update('ollama', ollamaConfig);
      return { success: true };
    } catch (error) {
      console.error('IPC config:updateOllama error:', error);
      return {
        success: false,
        error: error instanceof Error ? error.message : String(error),
      };
    }
  });

  /**
   * Channel: config:updateMongoDB
   * Updates MongoDB configuration
   */
  ipcMain.handle('config:updateMongoDB', async (_, mongoConfig: MongoDBConfig) => {
    try {
      await configService.update('mongodb', mongoConfig);
      return { success: true };
    } catch (error) {
      console.error('IPC config:updateMongoDB error:', error);
      return {
        success: false,
        error: error instanceof Error ? error.message : String(error),
      };
    }
  });

  /**
   * Channel: config:reset
   * Resets configuration to defaults
   */
  ipcMain.handle('config:reset', async () => {
    try {
      await configService.reset();
      const newConfig = await configService.load();
      return { success: true, data: newConfig };
    } catch (error) {
      console.error('IPC config:reset error:', error);
      return {
        success: false,
        error: error instanceof Error ? error.message : String(error),
      };
    }
  });

  /**
   * Channel: config:loadDefaults
   * Loads default configuration
   */
  ipcMain.handle('config:loadDefaults', async () => {
    try {
      const defaults = await configService.loadDefaults();
      return { success: true, data: defaults };
    } catch (error) {
      console.error('IPC config:loadDefaults error:', error);
      return {
        success: false,
        error: error instanceof Error ? error.message : String(error),
      };
    }
  });

  /**
   * Channel: config:testOllamaConnection
   * Tests OLLAMA server connection
   */
  ipcMain.handle('config:testOllamaConnection', async (_, baseUrl: string) => {
    try {
      // Test connection with a health check endpoint
      const controller = new AbortController();
      const timeoutId = setTimeout(() => controller.abort(), 5000); // 5 second timeout

      const response = await fetch(`${baseUrl}/api/tags`, {
        signal: controller.signal,
      });

      clearTimeout(timeoutId);

      if (!response.ok) {
        return {
          success: false,
          error: `OLLAMA server returned ${response.status}`,
        };
      }

      return { success: true };
    } catch (error) {
      console.error('IPC config:testOllamaConnection error:', error);
      return {
        success: false,
        error:
          error instanceof Error
            ? error.message
            : 'Failed to connect to OLLAMA server',
      };
    }
  });

  /**
   * Channel: config:testMongoDBConnection
   * Tests MongoDB connection
   */
  ipcMain.handle(
    'config:testMongoDBConnection',
    async (_, mongoUri: string) => {
      try {
        const { MongoClient } = await import('mongodb');
        const client = new MongoClient(mongoUri, {
          serverSelectionTimeoutMS: 5000,
          connectTimeoutMS: 5000,
        });

        await client.connect();
        await client.db('admin').command({ ping: 1 });
        await client.close();

        return { success: true };
      } catch (error) {
        console.error('IPC config:testMongoDBConnection error:', error);
        return {
          success: false,
          error:
            error instanceof Error
              ? error.message
              : 'Failed to connect to MongoDB',
        };
      }
    }
  );
}

/**
 * Get configuration service instance (for testing/other processes)
 */
export function getConfigService(): ConfigurationService {
  if (!configService) {
    configService = new ConfigurationService();
  }
  return configService;
}
