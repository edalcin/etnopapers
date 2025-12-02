/**
 * Preload script for EtnoPapers
 * Exposes secure APIs to renderer process via context isolation
 */

import { contextBridge } from 'electron';
import { configAPI } from './configAPI';
import { pdfAPI } from './pdfAPI';
import { ollamaAPI } from './ollamaAPI';
import { storageAPI } from './storageAPI';
import { extractionAPI } from './extractionAPI';

/**
 * Expose APIs to renderer process
 * Using context isolation for security
 */
contextBridge.exposeInMainWorld('etnopapers', {
  config: configAPI,
  pdf: pdfAPI,
  ollama: ollamaAPI,
  storage: storageAPI,
  extraction: extractionAPI,
});

/**
 * Type definitions for window object
 */
declare global {
  interface Window {
    etnopapers: {
      config: typeof configAPI;
      pdf: typeof pdfAPI;
      ollama: typeof ollamaAPI;
      storage: typeof storageAPI;
      extraction: typeof extractionAPI;
    };
  }
}
