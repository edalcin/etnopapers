/**
 * Service interface types
 * Defines contracts for all main services
 */

import { ArticleRecord } from './article';
import { AppConfiguration } from './config';

// PDF Processing Service
export interface IPDFProcessingService {
  extractText(filePath: string): Promise<string>;
  getMetadata(filePath: string): Promise<PDFMetadata>;
  validatePDF(filePath: string): Promise<boolean>;
  checkTextLayers(filePath: string): Promise<boolean>;
}

export interface PDFMetadata {
  title?: string;
  author?: string;
  subject?: string;
  creator?: string;
  producer?: string;
  creationDate?: Date;
  modificationDate?: Date;
  pageCount: number;
}

// OLLAMA Service
export interface IOLLAMAService {
  checkHealth(): Promise<boolean>;
  extractMetadata(text: string): Promise<ExtractedMetadata>;
  translateToPortuguese(text: string): Promise<string>;
  getAvailableModels(): Promise<string[]>;
}

export interface ExtractedMetadata {
  titulo: string;
  autores: string[];
  ano: number;
  resumo: string;
  especies?: any[];
  comunidade?: any;
  metodologia?: string;
  pais?: string;
  estado?: string;
  municipio?: string;
  local?: string;
  bioma?: string;
}

// Data Storage Service
export interface IDataStorageService {
  initialize(): Promise<void>;
  getAll(): Promise<ArticleRecord[]>;
  getById(id: string): Promise<ArticleRecord | null>;
  create(record: Omit<ArticleRecord, '_id' | 'createdAt' | 'updatedAt'>): Promise<ArticleRecord>;
  update(id: string, data: Partial<ArticleRecord>): Promise<ArticleRecord>;
  delete(id: string): Promise<void>;
  deleteMany(ids: string[]): Promise<void>;
  count(): Promise<number>;
  checkLimit(): Promise<boolean>;
}

// Validation Service
export interface IValidationService {
  validateRecord(record: any): Promise<ValidationResult>;
  validateExtractedData(data: any): Promise<ValidationResult>;
  checkMandatoryFields(record: any): Promise<boolean>;
}

export interface ValidationResult {
  valid: boolean;
  errors: ValidationError[];
}

export interface ValidationError {
  field: string;
  message: string;
}

// Extraction Pipeline Service
export interface IExtractionPipelineService {
  extractFromPDF(filePath: string): Promise<ArticleRecord>;
  cancelExtraction(): Promise<void>;
  getExtractionStatus(): Promise<ExtractionStatus>;
}

export interface ExtractionStatus {
  isExtracting: boolean;
  progress: number; // 0-100
  currentStep: string;
  error?: string;
}

// Configuration Service
export interface IConfigurationService {
  load(): Promise<AppConfiguration>;
  save(config: AppConfiguration): Promise<void>;
  get<K extends keyof AppConfiguration>(key: K): Promise<AppConfiguration[K]>;
  update<K extends keyof AppConfiguration>(key: K, value: AppConfiguration[K]): Promise<void>;
  reset(): Promise<void>;
  loadDefaults(): Promise<AppConfiguration>;
}

// MongoDB Sync Service
export interface IMongoDBSyncService {
  testConnection(): Promise<boolean>;
  uploadRecord(record: ArticleRecord): Promise<void>;
  uploadBatch(records: ArticleRecord[]): Promise<SyncResult>;
  getStatus(): Promise<SyncStatus>;
  deleteAfterSync(id: string): Promise<void>;
}

export interface SyncStatus {
  totalRecords: number;
  syncedRecords: number;
  lastSyncAt?: Date;
  isConnected: boolean;
  error?: string;
}

export interface SyncResult {
  success: number;
  failed: number;
  errors: Record<string, string>;
}

// Logger Service
export interface ILoggerService {
  debug(message: string, ...args: any[]): void;
  info(message: string, ...args: any[]): void;
  warn(message: string, ...args: any[]): void;
  error(message: string, ...args: any[]): void;
  getLogFilePath(): string;
}
