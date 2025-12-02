/**
 * IPC handlers for PDF operations
 * Exposes PDF processing service methods to renderer process
 */

import { ipcMain } from 'electron';
import { PDFProcessingService } from '../services/PDFProcessingService';
import { PDFMetadata } from '@shared/types/services';

let pdfService: PDFProcessingService;

/**
 * Initialize PDF IPC handlers
 */
export function registerPdfHandlers(): void {
  pdfService = new PDFProcessingService();

  /**
   * Channel: pdf:extractText
   * Extracts text content from a PDF file
   */
  ipcMain.handle('pdf:extractText', async (_, filePath: string) => {
    try {
      const text = await pdfService.extractText(filePath);
      return { success: true, data: text };
    } catch (error) {
      console.error('IPC pdf:extractText error:', error);
      return {
        success: false,
        error: error instanceof Error ? error.message : String(error),
      };
    }
  });

  /**
   * Channel: pdf:getMetadata
   * Retrieves PDF metadata (title, author, page count, etc.)
   */
  ipcMain.handle('pdf:getMetadata', async (_, filePath: string) => {
    try {
      const metadata = await pdfService.getMetadata(filePath);
      return { success: true, data: metadata };
    } catch (error) {
      console.error('IPC pdf:getMetadata error:', error);
      return {
        success: false,
        error: error instanceof Error ? error.message : String(error),
      };
    }
  });

  /**
   * Channel: pdf:validate
   * Validates if PDF file is readable and has valid structure
   */
  ipcMain.handle('pdf:validate', async (_, filePath: string) => {
    try {
      const isValid = await pdfService.validatePDF(filePath);
      return { success: true, data: isValid };
    } catch (error) {
      console.error('IPC pdf:validate error:', error);
      return {
        success: false,
        error: error instanceof Error ? error.message : String(error),
      };
    }
  });

  /**
   * Channel: pdf:checkTextLayers
   * Checks if PDF has text layers (not just scanned images)
   */
  ipcMain.handle('pdf:checkTextLayers', async (_, filePath: string) => {
    try {
      const hasText = await pdfService.checkTextLayers(filePath);
      return { success: true, data: hasText };
    } catch (error) {
      console.error('IPC pdf:checkTextLayers error:', error);
      return {
        success: false,
        error: error instanceof Error ? error.message : String(error),
      };
    }
  });
}

/**
 * Get PDF service instance (for testing/other processes)
 */
export function getPdfService(): PDFProcessingService {
  if (!pdfService) {
    pdfService = new PDFProcessingService();
  }
  return pdfService;
}
