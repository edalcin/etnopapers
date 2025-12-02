/**
 * Configuration Service for main process
 * Manages application settings using electron-store
 */

import Store from 'electron-store';
import { AppConfiguration, DEFAULT_CONFIG } from '@shared/types/config';
import { IConfigurationService } from '@shared/types/services';

export class ConfigurationService implements IConfigurationService {
  private store: Store<AppConfiguration>;

  constructor() {
    this.store = new Store<AppConfiguration>({
      name: 'etnopapers-config',
      defaults: DEFAULT_CONFIG,
      clearInvalidConfig: false,
    });
  }

  /**
   * Load configuration from storage
   */
  async load(): Promise<AppConfiguration> {
    try {
      const config = this.store.store;
      return this.validateConfig(config);
    } catch (error) {
      console.error('Failed to load configuration:', error);
      throw new Error(`Configuration load failed: ${String(error)}`);
    }
  }

  /**
   * Save configuration to storage
   */
  async save(config: AppConfiguration): Promise<void> {
    try {
      const validatedConfig = this.validateConfig(config);
      this.store.store = validatedConfig;
    } catch (error) {
      console.error('Failed to save configuration:', error);
      throw new Error(`Configuration save failed: ${String(error)}`);
    }
  }

  /**
   * Get specific configuration value
   */
  async get<K extends keyof AppConfiguration>(
    key: K
  ): Promise<AppConfiguration[K]> {
    try {
      const value = this.store.get(key);
      return value;
    } catch (error) {
      console.error(`Failed to get config key ${String(key)}:`, error);
      throw new Error(`Configuration get failed: ${String(error)}`);
    }
  }

  /**
   * Update specific configuration value
   */
  async update<K extends keyof AppConfiguration>(
    key: K,
    value: AppConfiguration[K]
  ): Promise<void> {
    try {
      // Validate partial config update
      const currentConfig = this.store.store;
      const newConfig = { ...currentConfig, [key]: value };
      const validatedConfig = this.validateConfig(newConfig);
      this.store.set(key, validatedConfig[key]);
    } catch (error) {
      console.error(`Failed to update config key ${String(key)}:`, error);
      throw new Error(`Configuration update failed: ${String(error)}`);
    }
  }

  /**
   * Reset configuration to defaults
   */
  async reset(): Promise<void> {
    try {
      this.store.clear();
      this.store.store = DEFAULT_CONFIG;
    } catch (error) {
      console.error('Failed to reset configuration:', error);
      throw new Error(`Configuration reset failed: ${String(error)}`);
    }
  }

  /**
   * Load default configuration
   */
  async loadDefaults(): Promise<AppConfiguration> {
    try {
      return DEFAULT_CONFIG;
    } catch (error) {
      console.error('Failed to load defaults:', error);
      throw new Error(`Load defaults failed: ${String(error)}`);
    }
  }

  /**
   * Validate configuration structure and values
   */
  private validateConfig(config: any): AppConfiguration {
    // Basic validation - ensure required fields exist
    if (!config) {
      return DEFAULT_CONFIG;
    }

    // Ensure OLLAMA config exists
    if (!config.ollama) {
      config.ollama = DEFAULT_CONFIG.ollama;
    }
    if (!config.ollama.baseUrl) {
      config.ollama.baseUrl = DEFAULT_CONFIG.ollama.baseUrl;
    }
    if (!config.ollama.model) {
      config.ollama.model = DEFAULT_CONFIG.ollama.model;
    }

    // Ensure storage config exists
    if (!config.storage) {
      config.storage = DEFAULT_CONFIG.storage;
    }
    if (typeof config.storage.maxRecords !== 'number') {
      config.storage.maxRecords = DEFAULT_CONFIG.storage.maxRecords;
    }
    if (typeof config.storage.warningThreshold !== 'number') {
      config.storage.warningThreshold = DEFAULT_CONFIG.storage.warningThreshold;
    }
    if (typeof config.storage.autoSync !== 'boolean') {
      config.storage.autoSync = DEFAULT_CONFIG.storage.autoSync;
    }

    // Ensure language is valid
    if (!config.language || !['pt-BR', 'en-US'].includes(config.language)) {
      config.language = DEFAULT_CONFIG.language;
    }

    return config as AppConfiguration;
  }
}
