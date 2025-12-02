/**
 * Configuration API for preload script
 * Exposes configuration operations to renderer process with IPC
 */

import { ipcRenderer } from 'electron';
import { AppConfiguration } from '@shared/types/config';
import { OLLAMAConfig, MongoDBConfig } from '@shared/types/config';

/**
 * Configuration API exposed to renderer process
 */
export const configAPI = {
  /**
   * Load entire configuration
   */
  async load(): Promise<AppConfiguration> {
    const result = await ipcRenderer.invoke('config:load');
    if (!result.success) {
      throw new Error(result.error);
    }
    return result.data;
  },

  /**
   * Get specific configuration value
   */
  async get<K extends keyof AppConfiguration>(
    key: K
  ): Promise<AppConfiguration[K]> {
    const result = await ipcRenderer.invoke('config:get', key);
    if (!result.success) {
      throw new Error(result.error);
    }
    return result.data;
  },

  /**
   * Update specific configuration value
   */
  async update<K extends keyof AppConfiguration>(
    key: K,
    value: AppConfiguration[K]
  ): Promise<void> {
    const result = await ipcRenderer.invoke('config:update', key, value);
    if (!result.success) {
      throw new Error(result.error);
    }
  },

  /**
   * Update OLLAMA configuration
   */
  async updateOllama(config: OLLAMAConfig): Promise<void> {
    const result = await ipcRenderer.invoke('config:updateOllama', config);
    if (!result.success) {
      throw new Error(result.error);
    }
  },

  /**
   * Update MongoDB configuration
   */
  async updateMongoDB(config: MongoDBConfig): Promise<void> {
    const result = await ipcRenderer.invoke('config:updateMongoDB', config);
    if (!result.success) {
      throw new Error(result.error);
    }
  },

  /**
   * Reset configuration to defaults
   */
  async reset(): Promise<AppConfiguration> {
    const result = await ipcRenderer.invoke('config:reset');
    if (!result.success) {
      throw new Error(result.error);
    }
    return result.data;
  },

  /**
   * Load default configuration
   */
  async loadDefaults(): Promise<AppConfiguration> {
    const result = await ipcRenderer.invoke('config:loadDefaults');
    if (!result.success) {
      throw new Error(result.error);
    }
    return result.data;
  },

  /**
   * Test OLLAMA server connection
   */
  async testOllamaConnection(baseUrl: string): Promise<boolean> {
    const result = await ipcRenderer.invoke('config:testOllamaConnection', baseUrl);
    if (!result.success) {
      throw new Error(result.error);
    }
    return result.success;
  },

  /**
   * Test MongoDB connection
   */
  async testMongoDBConnection(mongoUri: string): Promise<boolean> {
    const result = await ipcRenderer.invoke('config:testMongoDBConnection', mongoUri);
    if (!result.success) {
      throw new Error(result.error);
    }
    return result.success;
  },
};
