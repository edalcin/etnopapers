/**
 * OLLAMA API for preload script
 * Exposes OLLAMA operations to renderer process with IPC
 */

import { ipcRenderer } from 'electron';
import { ExtractedMetadata } from '@shared/types/services';
import { OLLAMAConfig } from '@shared/types/config';

/**
 * OLLAMA API exposed to renderer process
 */
export const ollamaAPI = {
  /**
   * Check OLLAMA service health
   */
  async checkHealth(): Promise<boolean> {
    const result = await ipcRenderer.invoke('ollama:checkHealth');
    if (!result.success) {
      throw new Error(result.error);
    }
    return result.data;
  },

  /**
   * Extract metadata from text
   */
  async extract(text: string): Promise<ExtractedMetadata> {
    const result = await ipcRenderer.invoke('ollama:extract', text);
    if (!result.success) {
      throw new Error(result.error);
    }
    return result.data;
  },

  /**
   * Translate text to Portuguese
   */
  async translate(text: string): Promise<string> {
    const result = await ipcRenderer.invoke('ollama:translate', text);
    if (!result.success) {
      throw new Error(result.error);
    }
    return result.data;
  },

  /**
   * Get available models
   */
  async getModels(): Promise<string[]> {
    const result = await ipcRenderer.invoke('ollama:getModels');
    if (!result.success) {
      throw new Error(result.error);
    }
    return result.data;
  },

  /**
   * Update OLLAMA configuration
   */
  async updateConfig(config: OLLAMAConfig): Promise<void> {
    const result = await ipcRenderer.invoke('ollama:updateConfig', config);
    if (!result.success) {
      throw new Error(result.error);
    }
  },
};
