/**
 * Service Interfaces: EtnoPapers
 *
 * This file defines the TypeScript interfaces for all internal services
 * in the EtnoPapers desktop application. These serve as contracts between
 * the UI layer and business logic layer.
 *
 * Date: 2025-12-01
 * Phase: 1 - Contracts
 */

import type {
  ArticleRecord,
  PlantSpecies,
  Community,
  AppConfiguration,
  SyncStatus,
  ExtrationMetadata,
} from '../data-model'

// ============================================================================
// PDF Processing Service
// ============================================================================

export interface IPDFProcessingService {
  /**
   * Extract text content from a PDF file
   * @param filePath - Absolute path to PDF file
   * @returns Extracted text content
   * @throws PDFProcessingError if extraction fails
   */
  extractText(filePath: string): Promise<string>

  /**
   * Get PDF metadata (page count, author, etc.)
   * @param filePath - Absolute path to PDF file
   * @returns PDF metadata object
   */
  getMetadata(filePath: string): Promise<PDFMetadata>

  /**
   * Validate that a file is a valid PDF
   * @param filePath - Absolute path to file
   * @returns true if valid PDF, false otherwise
   */
  validatePDF(filePath: string): Promise<boolean>
}

export interface PDFMetadata {
  title?: string
  author?: string
  producer?: string
  creationDate?: Date
  pageCount: number
  fileSizeBytes: number
}

export class PDFProcessingError extends Error {
  constructor(
    message: string,
    public readonly code: PDFErrorCode,
    public readonly originalError?: Error
  ) {
    super(message)
    this.name = 'PDFProcessingError'
  }
}

export enum PDFErrorCode {
  INVALID_FILE = 'INVALID_FILE',
  CORRUPTED_PDF = 'CORRUPTED_PDF',
  NO_TEXT_CONTENT = 'NO_TEXT_CONTENT',
  FILE_TOO_LARGE = 'FILE_TOO_LARGE',
  UNSUPPORTED_FORMAT = 'UNSUPPORTED_FORMAT',
}

// ============================================================================
// OLLAMA AI Service
// ============================================================================

export interface IOLLAMAService {
  /**
   * Check if OLLAMA service is available and responding
   * @returns true if connected, false otherwise
   */
  checkHealth(): Promise<boolean>

  /**
   * Extract structured metadata from text using AI
   * @param text - Extracted PDF text
   * @param prompt - Custom extraction prompt (optional)
   * @returns Extracted article data
   * @throws OLLAMAError if extraction fails
   */
  extractMetadata(
    text: string,
    prompt?: string
  ): Promise<ExtractedArticleData>

  /**
   * Stream extraction progress for large documents
   * @param text - Extracted PDF text
   * @param prompt - Custom extraction prompt (optional)
   * @param onProgress - Callback for partial results
   * @returns Final extracted data
   */
  extractMetadataStream(
    text: string,
    prompt?: string,
    onProgress?: (partial: Partial<ExtractedArticleData>) => void
  ): Promise<ExtractedArticleData>

  /**
   * Translate text to Portuguese
   * @param text - Text to translate
   * @param sourceLang - Source language (auto-detect if not provided)
   * @returns Translated text in Brazilian Portuguese
   */
  translateToPortuguese(
    text: string,
    sourceLang?: string
  ): Promise<string>

  /**
   * Get available OLLAMA models
   * @returns List of installed models
   */
  getAvailableModels(): Promise<string[]>

  /**
   * Get current configuration
   * @returns OLLAMA configuration
   */
  getConfig(): OLLAMAConfig

  /**
   * Update configuration
   * @param config - New configuration
   */
  updateConfig(config: Partial<OLLAMAConfig>): void
}

export interface OLLAMAConfig {
  url: string
  model: string
  timeout: number
  streamingEnabled: boolean
}

export interface ExtractedArticleData {
  // Mandatory fields
  titulo: string
  autores: string[]
  ano: number
  resumo: string

  // Optional fields
  publicacao?: string
  especies?: PlantSpecies[]
  comunidade?: Community
  metodologia?: string

  // Geographic
  pais?: string
  estado?: string
  municipio?: string
  local?: string
  bioma?: string

  // Confidence scores
  confidence?: {
    overall: number
    fields: Record<string, number>
  }
}

export class OLLAMAError extends Error {
  constructor(
    message: string,
    public readonly code: OLLAMAErrorCode,
    public readonly originalError?: Error
  ) {
    super(message)
    this.name = 'OLLAMAError'
  }
}

export enum OLLAMAErrorCode {
  SERVICE_UNAVAILABLE = 'SERVICE_UNAVAILABLE',
  TIMEOUT = 'TIMEOUT',
  INVALID_RESPONSE = 'INVALID_RESPONSE',
  MODEL_NOT_FOUND = 'MODEL_NOT_FOUND',
  RATE_LIMIT = 'RATE_LIMIT',
}

// ============================================================================
// Data Storage Service
// ============================================================================

export interface IDataStorageService {
  /**
   * Initialize storage (create file if needed, run migrations)
   * @throws StorageError if initialization fails
   */
  initialize(): Promise<void>

  /**
   * Get all article records
   * @param filter - Optional filter criteria
   * @returns Array of article records
   */
  getAll(filter?: RecordFilter): Promise<ArticleRecord[]>

  /**
   * Get a single record by ID
   * @param id - Record UUID
   * @returns Article record or null if not found
   */
  getById(id: string): Promise<ArticleRecord | null>

  /**
   * Create a new record
   * @param data - Article data (without _id, timestamps)
   * @returns Created record with generated metadata
   * @throws StorageError if creation fails or limit exceeded
   */
  create(data: Omit<ArticleRecord, '_id' | 'createdAt' | 'updatedAt'>): Promise<ArticleRecord>

  /**
   * Update an existing record
   * @param id - Record UUID
   * @param updates - Partial record updates
   * @returns Updated record
   * @throws StorageError if record not found
   */
  update(id: string, updates: Partial<ArticleRecord>): Promise<ArticleRecord>

  /**
   * Delete a record
   * @param id - Record UUID
   * @throws StorageError if record not found
   */
  delete(id: string): Promise<void>

  /**
   * Delete multiple records
   * @param ids - Array of record UUIDs
   * @returns Number of deleted records
   */
  deleteMany(ids: string[]): Promise<number>

  /**
   * Get current record count
   * @returns Number of records in storage
   */
  count(): Promise<number>

  /**
   * Check if storage limit is reached
   * @returns true if at or above limit
   */
  isLimitReached(): Promise<boolean>

  /**
   * Search records by text query
   * @param query - Search string
   * @param fields - Fields to search in
   * @returns Matching records
   */
  search(query: string, fields?: string[]): Promise<ArticleRecord[]>

  /**
   * Get storage statistics
   * @returns Storage stats
   */
  getStats(): Promise<StorageStats>
}

export interface RecordFilter {
  syncStatus?: SyncStatus | SyncStatus[]
  yearRange?: { min?: number; max?: number }
  authors?: string[]
  bioma?: string[]
  hasSpecies?: boolean
}

export interface StorageStats {
  recordCount: number
  maxRecords: number
  fileSizeBytes: number
  lastModified: Date
  utilizationPercent: number
}

export class StorageError extends Error {
  constructor(
    message: string,
    public readonly code: StorageErrorCode,
    public readonly originalError?: Error
  ) {
    super(message)
    this.name = 'StorageError'
  }
}

export enum StorageErrorCode {
  LIMIT_EXCEEDED = 'LIMIT_EXCEEDED',
  RECORD_NOT_FOUND = 'RECORD_NOT_FOUND',
  VALIDATION_FAILED = 'VALIDATION_FAILED',
  FILE_LOCKED = 'FILE_LOCKED',
  DISK_FULL = 'DISK_FULL',
  CORRUPTED_DATA = 'CORRUPTED_DATA',
}

// ============================================================================
// MongoDB Sync Service
// ============================================================================

export interface IMongoDBSyncService {
  /**
   * Test MongoDB connection
   * @returns true if connection successful
   */
  testConnection(): Promise<boolean>

  /**
   * Get connection status
   * @returns Current connection state
   */
  getStatus(): ConnectionStatus

  /**
   * Upload a single record to MongoDB
   * @param record - Article record to upload
   * @returns MongoDB ObjectId of inserted document
   * @throws MongoDBError if upload fails
   */
  uploadRecord(record: ArticleRecord): Promise<string>

  /**
   * Upload multiple records in batch
   * @param records - Array of records to upload
   * @param onProgress - Progress callback
   * @returns Results for each record
   */
  uploadBatch(
    records: ArticleRecord[],
    onProgress?: (completed: number, total: number) => void
  ): Promise<BatchUploadResult[]>

  /**
   * Get current configuration
   * @returns MongoDB configuration
   */
  getConfig(): MongoDBConfig

  /**
   * Update configuration
   * @param config - New configuration
   * @throws MongoDBError if connection test fails with new config
   */
  updateConfig(config: Partial<MongoDBConfig>): Promise<void>

  /**
   * Disconnect from MongoDB
   */
  disconnect(): Promise<void>
}

export interface MongoDBConfig {
  uri: string | null
  database: string
  collection: string
  tlsEnabled: boolean
  connectionTimeout: number
}

export interface ConnectionStatus {
  connected: boolean
  lastCheck: Date
  errorMessage?: string
}

export interface BatchUploadResult {
  recordId: string
  success: boolean
  mongoId?: string
  error?: string
}

export class MongoDBError extends Error {
  constructor(
    message: string,
    public readonly code: MongoDBErrorCode,
    public readonly originalError?: Error
  ) {
    super(message)
    this.name = 'MongoDBError'
  }
}

export enum MongoDBErrorCode {
  CONNECTION_FAILED = 'CONNECTION_FAILED',
  AUTHENTICATION_FAILED = 'AUTHENTICATION_FAILED',
  TIMEOUT = 'TIMEOUT',
  DUPLICATE_KEY = 'DUPLICATE_KEY',
  NETWORK_ERROR = 'NETWORK_ERROR',
  INVALID_URI = 'INVALID_URI',
}

// ============================================================================
// Configuration Service
// ============================================================================

export interface IConfigurationService {
  /**
   * Load configuration from disk
   * @returns Current configuration
   */
  load(): Promise<AppConfiguration>

  /**
   * Save configuration to disk
   * @param config - Configuration to save
   */
  save(config: AppConfiguration): Promise<void>

  /**
   * Get current configuration
   * @returns Current configuration
   */
  get(): AppConfiguration

  /**
   * Update partial configuration
   * @param updates - Configuration updates
   * @returns Updated configuration
   */
  update(updates: Partial<AppConfiguration>): Promise<AppConfiguration>

  /**
   * Reset to default configuration
   * @returns Default configuration
   */
  reset(): Promise<AppConfiguration>

  /**
   * Export configuration to file
   * @param filePath - Export destination
   */
  export(filePath: string): Promise<void>

  /**
   * Import configuration from file
   * @param filePath - Import source
   */
  import(filePath: string): Promise<AppConfiguration>
}

// ============================================================================
// Extraction Pipeline Service (Orchestration)
// ============================================================================

export interface IExtractionPipelineService {
  /**
   * Execute full extraction pipeline for a PDF
   * @param filePath - Path to PDF file
   * @param onProgress - Progress callback
   * @returns Extracted and validated article record
   * @throws ExtractionError if any step fails
   */
  extractFromPDF(
    filePath: string,
    onProgress?: (step: ExtractionStep, progress: number) => void
  ): Promise<ArticleRecord>

  /**
   * Cancel an ongoing extraction
   * @param extractionId - ID of extraction to cancel
   */
  cancelExtraction(extractionId: string): void

  /**
   * Get status of an ongoing extraction
   * @param extractionId - ID of extraction
   * @returns Current extraction status
   */
  getExtractionStatus(extractionId: string): ExtractionStatus | null
}

export enum ExtractionStep {
  VALIDATING_FILE = 'VALIDATING_FILE',
  EXTRACTING_TEXT = 'EXTRACTING_TEXT',
  CALLING_AI = 'CALLING_AI',
  PARSING_RESPONSE = 'PARSING_RESPONSE',
  VALIDATING_DATA = 'VALIDATING_DATA',
  NORMALIZING = 'NORMALIZING',
  TRANSLATING_ABSTRACT = 'TRANSLATING_ABSTRACT',
  SAVING = 'SAVING',
  COMPLETE = 'COMPLETE',
}

export interface ExtractionStatus {
  id: string
  currentStep: ExtractionStep
  progress: number // 0-100
  startTime: Date
  estimatedTimeRemaining?: number // seconds
  error?: Error
}

export class ExtractionError extends Error {
  constructor(
    message: string,
    public readonly step: ExtractionStep,
    public readonly originalError?: Error
  ) {
    super(message)
    this.name = 'ExtractionError'
  }
}

// ============================================================================
// Validation Service
// ============================================================================

export interface IValidationService {
  /**
   * Validate an article record against schema
   * @param record - Record to validate
   * @returns Validation result
   */
  validateRecord(record: Partial<ArticleRecord>): ValidationResult

  /**
   * Validate extracted data before saving
   * @param data - Extracted data
   * @returns Validation result
   */
  validateExtractedData(data: ExtractedArticleData): ValidationResult

  /**
   * Check if all mandatory fields are present
   * @param record - Record to check
   * @returns List of missing mandatory fields
   */
  checkMandatoryFields(record: Partial<ArticleRecord>): string[]

  /**
   * Validate custom attributes against allowed types
   * @param attributes - Custom attributes to validate
   * @returns Validation result
   */
  validateCustomAttributes(attributes: Record<string, any>): ValidationResult
}

export interface ValidationResult {
  valid: boolean
  errors: ValidationError[]
  warnings: ValidationWarning[]
}

export interface ValidationError {
  field: string
  message: string
  code: string
  value?: any
}

export interface ValidationWarning {
  field: string
  message: string
  suggestion?: string
}

// ============================================================================
// Notification Service
// ============================================================================

export interface INotificationService {
  /**
   * Show a success notification
   * @param message - Notification message
   * @param duration - Duration in milliseconds (default: 3000)
   */
  success(message: string, duration?: number): void

  /**
   * Show an error notification
   * @param message - Error message
   * @param error - Optional error object for details
   * @param duration - Duration in milliseconds (default: 5000)
   */
  error(message: string, error?: Error, duration?: number): void

  /**
   * Show a warning notification
   * @param message - Warning message
   * @param duration - Duration in milliseconds (default: 4000)
   */
  warning(message: string, duration?: number): void

  /**
   * Show an info notification
   * @param message - Info message
   * @param duration - Duration in milliseconds (default: 3000)
   */
  info(message: string, duration?: number): void

  /**
   * Show a persistent notification that requires user action
   * @param message - Message text
   * @param actions - Action buttons
   * @returns User's selected action
   */
  prompt(message: string, actions: NotificationAction[]): Promise<string>
}

export interface NotificationAction {
  label: string
  value: string
  variant?: 'default' | 'destructive' | 'outline'
}

// ============================================================================
// Logger Service
// ============================================================================

export interface ILoggerService {
  /**
   * Log debug message
   * @param message - Log message
   * @param meta - Additional metadata
   */
  debug(message: string, meta?: Record<string, any>): void

  /**
   * Log info message
   * @param message - Log message
   * @param meta - Additional metadata
   */
  info(message: string, meta?: Record<string, any>): void

  /**
   * Log warning message
   * @param message - Log message
   * @param meta - Additional metadata
   */
  warn(message: string, meta?: Record<string, any>): void

  /**
   * Log error message
   * @param message - Log message
   * @param error - Error object
   * @param meta - Additional metadata
   */
  error(message: string, error?: Error, meta?: Record<string, any>): void

  /**
   * Get log file path
   * @returns Path to current log file
   */
  getLogFilePath(): string

  /**
   * Clear old logs (keeps last N days)
   * @param daysToKeep - Number of days to retain
   */
  clearOldLogs(daysToKeep: number): Promise<void>
}
