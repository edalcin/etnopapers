/**
 * PDF API for preload script
 * Exposes PDF operations to renderer process with IPC
 */

import { ipcRenderer } from 'electron';
import { PDFMetadata } from '@shared/types/services';

/**
 * PDF API exposed to renderer process
 */
export const pdfAPI = {
  /**
   * Extract text from PDF file
   */
  async extractText(filePath: string): Promise<string> {
    const result = await ipcRenderer.invoke('pdf:extractText', filePath);
    if (!result.success) {
      throw new Error(result.error);
    }
    return result.data;
  },

  /**
   * Get PDF metadata
   */
  async getMetadata(filePath: string): Promise<PDFMetadata> {
    const result = await ipcRenderer.invoke('pdf:getMetadata', filePath);
    if (!result.success) {
      throw new Error(result.error);
    }
    return result.data;
  },

  /**
   * Validate PDF file
   */
  async validate(filePath: string): Promise<boolean> {
    const result = await ipcRenderer.invoke('pdf:validate', filePath);
    if (!result.success) {
      throw new Error(result.error);
    }
    return result.data;
  },

  /**
   * Check if PDF has text layers
   */
  async checkTextLayers(filePath: string): Promise<boolean> {
    const result = await ipcRenderer.invoke('pdf:checkTextLayers', filePath);
    if (!result.success) {
      throw new Error(result.error);
    }
    return result.data;
  },
};
