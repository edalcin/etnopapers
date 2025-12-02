/**
 * Preload script for EtnoPapers
 * Exposes secure APIs to renderer process via context isolation
 */

import { contextBridge } from 'electron';
import { configAPI } from './configAPI';

/**
 * Expose APIs to renderer process
 * Using context isolation for security
 */
contextBridge.exposeInMainWorld('etnopapers', {
  config: configAPI,
});

/**
 * Type definitions for window object
 */
declare global {
  interface Window {
    etnopapers: {
      config: typeof configAPI;
    };
  }
}
