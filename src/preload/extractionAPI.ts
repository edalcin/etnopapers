/**
 * Extraction API for preload script
 * Exposes extraction operations to renderer process with IPC
 */

import { ipcRenderer } from 'electron';
import { ArticleRecord } from '@shared/types/article';
import { ExtractionStatus } from '@shared/types/services';
import { OLLAMAConfig } from '@shared/types/config';

/**
 * Extraction API exposed to renderer process
 */
export const extractionAPI = {
  /**
   * Start extraction from PDF file
   */
  async start(filePath: string): Promise<ArticleRecord> {
    const result = await ipcRenderer.invoke('extraction:start', filePath);
    if (!result.success) {
      throw new Error(result.error);
    }
    return result.data;
  },

  /**
   * Cancel ongoing extraction
   */
  async cancel(): Promise<void> {
    const result = await ipcRenderer.invoke('extraction:cancel');
    if (!result.success) {
      throw new Error(result.error);
    }
  },

  /**
   * Get extraction status
   */
  async getStatus(): Promise<ExtractionStatus> {
    const result = await ipcRenderer.invoke('extraction:status');
    if (!result.success) {
      throw new Error(result.error);
    }
    return result.data;
  },

  /**
   * Update OLLAMA configuration
   */
  async updateConfig(config: OLLAMAConfig): Promise<void> {
    const result = await ipcRenderer.invoke('extraction:updateConfig', config);
    if (!result.success) {
      throw new Error(result.error);
    }
  },

  /**
   * Listen to extraction progress events
   */
  onProgress(callback: (status: ExtractionStatus) => void): () => void {
    const listener = (_: any, status: ExtractionStatus) => callback(status);
    ipcRenderer.on('extraction:progress', listener);

    // Return unsubscribe function
    return () => {
      ipcRenderer.removeListener('extraction:progress', listener);
    };
  },
};
