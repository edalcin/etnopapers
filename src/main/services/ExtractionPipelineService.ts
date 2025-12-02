/**
 * Extraction Pipeline Service
 * Orchestrates the complete PDF → AI → validation → storage workflow
 */

import { IExtractionPipelineService, ExtractionStatus } from '@shared/types/services';
import { ArticleRecord } from '@shared/types/article';
import { PDFProcessingService } from './PDFProcessingService';
import { OLLAMAService } from './OLLAMAService';
import { ValidationService } from './ValidationService';
import { DataStorageService } from './DataStorageService';
import { OLLAMAConfig } from '@shared/types/config';
import { ExtractionError } from '@shared/types/errors';

export class ExtractionPipelineService implements IExtractionPipelineService {
  private pdfService: PDFProcessingService;
  private ollamaService: OLLAMAService;
  private validationService: ValidationService;
  private storageService: DataStorageService;

  private isExtracting = false;
  private currentStep = '';
  private extractionProgress = 0;
  private extractionError: string | undefined;
  private abortController: AbortController | null = null;

  constructor(
    pdfService: PDFProcessingService,
    ollamaService: OLLAMAService,
    validationService: ValidationService,
    storageService: DataStorageService
  ) {
    this.pdfService = pdfService;
    this.ollamaService = ollamaService;
    this.validationService = validationService;
    this.storageService = storageService;
  }

  /**
   * Extract article metadata from PDF file
   */
  async extractFromPDF(filePath: string): Promise<ArticleRecord> {
    try {
      this.isExtracting = true;
      this.extractionProgress = 0;
      this.extractionError = undefined;
      this.abortController = new AbortController();

      // Step 1: Validate PDF
      this.currentStep = 'validating_pdf';
      this.extractionProgress = 10;
      const isValid = await this.pdfService.validatePDF(filePath);
      if (!isValid) {
        throw new ExtractionError('PDF validation failed');
      }

      // Step 2: Check for text layers
      this.currentStep = 'checking_text_layers';
      this.extractionProgress = 20;
      const hasText = await this.pdfService.checkTextLayers(filePath);
      if (!hasText) {
        throw new ExtractionError(
          'PDF contains no extractable text. Please use the original digital file or apply OCR preprocessing.'
        );
      }

      // Step 3: Extract text
      this.currentStep = 'extracting_text';
      this.extractionProgress = 30;
      const text = await this.pdfService.extractText(filePath);
      if (!text || text.trim().length === 0) {
        throw new ExtractionError('Failed to extract any text from PDF');
      }

      // Step 4: Get PDF metadata
      this.currentStep = 'getting_metadata';
      this.extractionProgress = 40;
      const pdfMetadata = await this.pdfService.getMetadata(filePath);

      // Step 5: Extract with OLLAMA
      this.currentStep = 'ai_extraction';
      this.extractionProgress = 60;
      const extractedData = await this.ollamaService.extractMetadata(text);

      // Step 6: Validate extracted data
      this.currentStep = 'validating_data';
      this.extractionProgress = 75;
      const validationResult = await this.validationService.validateExtractedData(extractedData);
      if (!validationResult.valid) {
        const errorMessages = validationResult.errors.map((e) => `${e.field}: ${e.message}`).join(', ');
        throw new ExtractionError(`Validation failed: ${errorMessages}`);
      }

      // Step 7: Create and store record
      this.currentStep = 'storing_record';
      this.extractionProgress = 85;

      const now = new Date();
      const record = await this.storageService.create({
        ...extractedData,
        createdAt: now,
        updatedAt: now,
        syncStatus: 'local',
        sourceFile: {
          fileName: filePath.split(/[\\\/]/).pop() || filePath,
          fileSize: 0,
          extractedAt: now,
          pageCount: pdfMetadata.pageCount,
        },
        extractionMetadata: {
          model: this.ollamaService['config']?.model || 'unknown',
          extractedAt: now,
          confidence: 0.8,
          executionTime: 0,
        },
      });

      this.currentStep = 'completed';
      this.extractionProgress = 100;

      return record;
    } catch (error) {
      this.extractionError = error instanceof Error ? error.message : String(error);
      this.currentStep = 'error';
      throw error;
    } finally {
      this.isExtracting = false;
    }
  }

  /**
   * Cancel ongoing extraction
   */
  async cancelExtraction(): Promise<void> {
    if (this.abortController) {
      this.abortController.abort();
    }
    this.isExtracting = false;
    this.currentStep = 'cancelled';
  }

  /**
   * Get current extraction status
   */
  async getExtractionStatus(): Promise<ExtractionStatus> {
    return {
      isExtracting: this.isExtracting,
      progress: this.extractionProgress,
      currentStep: this.currentStep,
      error: this.extractionError,
    };
  }

  /**
   * Update OLLAMA configuration
   */
  updateOLLAMAConfig(config: OLLAMAConfig): void {
    this.ollamaService.updateConfig(config);
  }
}
