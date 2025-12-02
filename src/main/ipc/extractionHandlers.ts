/**
 * IPC handlers for extraction operations
 * Exposes extraction pipeline to renderer process with progress events
 */

import { ipcMain, BrowserWindow } from 'electron';
import { ExtractionPipelineService } from '../services/ExtractionPipelineService';
import { PDFProcessingService } from '../services/PDFProcessingService';
import { OLLAMAService } from '../services/OLLAMAService';
import { ValidationService } from '../services/ValidationService';
import { DataStorageService } from '../services/DataStorageService';
import { getConfigService } from './configHandlers';

let extractionService: ExtractionPipelineService | null = null;
let progressInterval: NodeJS.Timeout | null = null;

/**
 * Initialize extraction IPC handlers
 */
export async function registerExtractionHandlers(mainWindow: BrowserWindow): Promise<void> {
  const configService = getConfigService();
  const config = await configService.load();

  // Initialize services
  const pdfService = new PDFProcessingService();
  const ollamaService = new OLLAMAService(config.ollama);
  const validationService = new ValidationService();
  const storageService = new DataStorageService();
  await storageService.initialize();

  extractionService = new ExtractionPipelineService(
    pdfService,
    ollamaService,
    validationService,
    storageService
  );

  /**
   * Channel: extraction:start
   * Starts extraction from PDF file
   */
  ipcMain.handle('extraction:start', async (_, filePath: string) => {
    try {
      // Start progress reporting
      startProgressReporting(mainWindow);

      const record = await extractionService!.extractFromPDF(filePath);
      return { success: true, data: record };
    } catch (error) {
      console.error('IPC extraction:start error:', error);
      return {
        success: false,
        error: error instanceof Error ? error.message : String(error),
      };
    } finally {
      stopProgressReporting();
    }
  });

  /**
   * Channel: extraction:cancel
   * Cancels ongoing extraction
   */
  ipcMain.handle('extraction:cancel', async () => {
    try {
      await extractionService!.cancelExtraction();
      return { success: true };
    } catch (error) {
      console.error('IPC extraction:cancel error:', error);
      return {
        success: false,
        error: error instanceof Error ? error.message : String(error),
      };
    }
  });

  /**
   * Channel: extraction:status
   * Gets current extraction status
   */
  ipcMain.handle('extraction:status', async () => {
    try {
      const status = await extractionService!.getExtractionStatus();
      return { success: true, data: status };
    } catch (error) {
      console.error('IPC extraction:status error:', error);
      return {
        success: false,
        error: error instanceof Error ? error.message : String(error),
      };
    }
  });

  /**
   * Channel: extraction:updateConfig
   * Updates OLLAMA configuration for extraction
   */
  ipcMain.handle('extraction:updateConfig', async (_, ollamaConfig: any) => {
    try {
      extractionService!.updateOLLAMAConfig(ollamaConfig);
      return { success: true };
    } catch (error) {
      console.error('IPC extraction:updateConfig error:', error);
      return {
        success: false,
        error: error instanceof Error ? error.message : String(error),
      };
    }
  });
}

/**
 * Start periodic progress reporting to renderer
 */
function startProgressReporting(mainWindow: BrowserWindow): void {
  if (progressInterval) {
    clearInterval(progressInterval);
  }

  progressInterval = setInterval(async () => {
    if (!extractionService) return;

    try {
      const status = await extractionService.getExtractionStatus();
      mainWindow.webContents.send('extraction:progress', status);
    } catch (error) {
      console.error('Error reporting extraction progress:', error);
    }
  }, 500); // Report every 500ms
}

/**
 * Stop progress reporting
 */
function stopProgressReporting(): void {
  if (progressInterval) {
    clearInterval(progressInterval);
    progressInterval = null;
  }
}

/**
 * Get extraction service instance (for testing/other processes)
 */
export function getExtractionService(): ExtractionPipelineService {
  if (!extractionService) {
    throw new Error('Extraction service not initialized');
  }
  return extractionService;
}
