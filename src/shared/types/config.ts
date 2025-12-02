/**
 * Application configuration types
 */

export interface OLLAMAConfig {
  /** OLLAMA service base URL (default: http://localhost:11434) */
  baseUrl: string;
  /** Model name to use for extraction */
  model: string;
  /** Custom extraction prompt template */
  prompt?: string;
  /** Request timeout in milliseconds */
  timeout?: number;
}

export interface MongoDBConfig {
  /** MongoDB connection URI */
  uri: string;
  /** Database name */
  database: string;
  /** Collection name for articles */
  collection: string;
}

export interface StorageConfig {
  /** Maximum number of local records allowed */
  maxRecords: number;
  /** Warning threshold (percentage of max) */
  warningThreshold: number;
  /** Auto-sync enabled */
  autoSync: boolean;
  /** Auto-sync interval in milliseconds */
  autoSyncInterval?: number;
}

export interface AppConfiguration {
  /** Application version */
  version: string;
  /** OLLAMA configuration */
  ollama: OLLAMAConfig;
  /** MongoDB configuration (optional, can be configured later) */
  mongodb?: MongoDBConfig;
  /** Local storage configuration */
  storage: StorageConfig;
  /** Application language */
  language: 'pt-BR' | 'en-US';
  /** Last successful sync timestamp */
  lastSyncAt?: Date;
}

/**
 * Default configuration values
 */
export const DEFAULT_CONFIG: AppConfiguration = {
  version: '1.0.0',
  ollama: {
    baseUrl: 'http://localhost:11434',
    model: 'llama2',
    timeout: 120000, // 2 minutes
  },
  storage: {
    maxRecords: 1000,
    warningThreshold: 80, // Warn at 800 records
    autoSync: false,
    autoSyncInterval: 86400000, // 24 hours
  },
  language: 'pt-BR',
};
