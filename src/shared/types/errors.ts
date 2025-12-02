/**
 * Custom error types for EtnoPapers
 */

export class EtnoPapersError extends Error {
  constructor(
    public code: string,
    message: string,
    public context?: Record<string, any>
  ) {
    super(message);
    this.name = 'EtnoPapersError';
    Object.setPrototypeOf(this, EtnoPapersError.prototype);
  }
}

export class PDFProcessingError extends EtnoPapersError {
  constructor(message: string, context?: Record<string, any>) {
    super('PDF_ERROR', message, context);
    this.name = 'PDFProcessingError';
    Object.setPrototypeOf(this, PDFProcessingError.prototype);
  }
}

export class PDFNotTextError extends PDFProcessingError {
  constructor(fileName: string) {
    super(
      `PDF "${fileName}" contains no extractable text. Please use the original digital file or apply OCR preprocessing.`,
      { fileName }
    );
    this.code = 'PDF_NO_TEXT';
  }
}

export class PDFValidationError extends PDFProcessingError {
  constructor(message: string) {
    super(`PDF validation failed: ${message}`);
    this.code = 'PDF_VALIDATION_ERROR';
  }
}

export class OLLAMAError extends EtnoPapersError {
  constructor(message: string, context?: Record<string, any>) {
    super('OLLAMA_ERROR', message, context);
    this.name = 'OLLAMAError';
    Object.setPrototypeOf(this, OLLAMAError.prototype);
  }
}

export class OLLAMAConnectionError extends OLLAMAError {
  constructor(baseUrl: string) {
    super(`Failed to connect to OLLAMA at ${baseUrl}`, { baseUrl });
    this.code = 'OLLAMA_CONNECTION_ERROR';
  }
}

export class OLLAMAExtractionError extends OLLAMAError {
  constructor(message: string, context?: Record<string, any>) {
    super(`Extraction failed: ${message}`, context);
    this.code = 'OLLAMA_EXTRACTION_ERROR';
  }
}

export class StorageError extends EtnoPapersError {
  constructor(message: string, context?: Record<string, any>) {
    super('STORAGE_ERROR', message, context);
    this.name = 'StorageError';
    Object.setPrototypeOf(this, StorageError.prototype);
  }
}

export class StorageLimitError extends StorageError {
  constructor(currentCount: number, maxCount: number) {
    super(`Storage limit reached: ${currentCount}/${maxCount} records`, {
      currentCount,
      maxCount,
    });
    this.code = 'STORAGE_LIMIT_ERROR';
  }
}

export class MongoDBError extends EtnoPapersError {
  constructor(message: string, context?: Record<string, any>) {
    super('MONGODB_ERROR', message, context);
    this.name = 'MongoDBError';
    Object.setPrototypeOf(this, MongoDBError.prototype);
  }
}

export class MongoDBConnectionError extends MongoDBError {
  constructor(uri: string) {
    super(`Failed to connect to MongoDB at ${uri}`, { uri });
    this.code = 'MONGODB_CONNECTION_ERROR';
  }
}

export class ExtractionError extends EtnoPapersError {
  constructor(message: string, context?: Record<string, any>) {
    super('EXTRACTION_ERROR', message, context);
    this.name = 'ExtractionError';
    Object.setPrototypeOf(this, ExtractionError.prototype);
  }
}

export class ValidationError extends EtnoPapersError {
  constructor(
    message: string,
    public errors: Array<{ field: string; message: string }> = []
  ) {
    super('VALIDATION_ERROR', message);
    this.name = 'ValidationError';
    Object.setPrototypeOf(this, ValidationError.prototype);
  }
}

export class ConfigurationError extends EtnoPapersError {
  constructor(message: string, context?: Record<string, any>) {
    super('CONFIG_ERROR', message, context);
    this.name = 'ConfigurationError';
    Object.setPrototypeOf(this, ConfigurationError.prototype);
  }
}
