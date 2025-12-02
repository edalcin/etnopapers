/**
 * IPC handlers for OLLAMA operations
 * Exposes OLLAMA service methods to renderer process
 */

import { ipcMain } from 'electron';
import { OLLAMAService } from '../services/OLLAMAService';
import { OLLAMAConfig } from '@shared/types/config';
import { ExtractedMetadata } from '@shared/types/services';
import { getConfigService } from './configHandlers';

let ollamaService: OLLAMAService | null = null;

/**
 * Initialize OLLAMA IPC handlers
 */
export async function registerOllamaHandlers(): Promise<void> {
  // Load initial config
  const configService = getConfigService();
  const config = await configService.load();
  ollamaService = new OLLAMAService(config.ollama);

  /**
   * Channel: ollama:checkHealth
   * Checks if OLLAMA service is running and responsive
   */
  ipcMain.handle('ollama:checkHealth', async () => {
    try {
      const isHealthy = await ollamaService!.checkHealth();
      return { success: true, data: isHealthy };
    } catch (error) {
      console.error('IPC ollama:checkHealth error:', error);
      return {
        success: false,
        error: error instanceof Error ? error.message : String(error),
      };
    }
  });

  /**
   * Channel: ollama:extract
   * Extracts metadata from PDF text
   */
  ipcMain.handle('ollama:extract', async (_, text: string) => {
    try {
      const metadata = await ollamaService!.extractMetadata(text);
      return { success: true, data: metadata };
    } catch (error) {
      console.error('IPC ollama:extract error:', error);
      return {
        success: false,
        error: error instanceof Error ? error.message : String(error),
      };
    }
  });

  /**
   * Channel: ollama:translate
   * Translates text to Portuguese
   */
  ipcMain.handle('ollama:translate', async (_, text: string) => {
    try {
      const translated = await ollamaService!.translateToPortuguese(text);
      return { success: true, data: translated };
    } catch (error) {
      console.error('IPC ollama:translate error:', error);
      return {
        success: false,
        error: error instanceof Error ? error.message : String(error),
      };
    }
  });

  /**
   * Channel: ollama:getModels
   * Gets list of available models
   */
  ipcMain.handle('ollama:getModels', async () => {
    try {
      const models = await ollamaService!.getAvailableModels();
      return { success: true, data: models };
    } catch (error) {
      console.error('IPC ollama:getModels error:', error);
      return {
        success: false,
        error: error instanceof Error ? error.message : String(error),
      };
    }
  });

  /**
   * Channel: ollama:updateConfig
   * Updates OLLAMA configuration and reinitializes service
   */
  ipcMain.handle('ollama:updateConfig', async (_, newConfig: OLLAMAConfig) => {
    try {
      if (ollamaService) {
        ollamaService.updateConfig(newConfig);
      } else {
        ollamaService = new OLLAMAService(newConfig);
      }
      return { success: true };
    } catch (error) {
      console.error('IPC ollama:updateConfig error:', error);
      return {
        success: false,
        error: error instanceof Error ? error.message : String(error),
      };
    }
  });
}

/**
 * Get OLLAMA service instance (for testing/other processes)
 */
export function getOllamaService(): OLLAMAService {
  if (!ollamaService) {
    throw new Error('OLLAMA service not initialized');
  }
  return ollamaService;
}
