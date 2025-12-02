/**
 * PDF Processing Service
 * Handles PDF text extraction, metadata reading, and validation using pdf.js
 */

import * as fs from 'fs';
import * as path from 'path';
import * as pdfjsLib from 'pdfjs-dist';
import { IPDFProcessingService, PDFMetadata } from '@shared/types/services';
import { PDFValidationError, PDFNotTextError } from '@shared/types/errors';

// Set up PDF.js worker
pdfjsLib.GlobalWorkerOptions.workerSrc = `//cdnjs.cloudflare.com/ajax/libs/pdf.js/${pdfjsLib.version}/pdf.worker.min.js`;

export class PDFProcessingService implements IPDFProcessingService {
  /**
   * Extract text from PDF file
   */
  async extractText(filePath: string): Promise<string> {
    try {
      this.validateFilePath(filePath);

      const pdf = await pdfjsLib.getDocument(filePath).promise;
      let text = '';

      // Extract text from all pages
      for (let pageNum = 1; pageNum <= pdf.numPages; pageNum++) {
        const page = await pdf.getPage(pageNum);
        const textContent = await page.getTextContent();

        // Combine text items
        const pageText = textContent.items
          .map((item: any) => item.str)
          .join(' ');

        text += pageText + '\n';
      }

      if (!text.trim()) {
        throw new PDFNotTextError(path.basename(filePath));
      }

      return text;
    } catch (error) {
      if (error instanceof PDFNotTextError) {
        throw error;
      }
      throw new PDFValidationError(
        `Failed to extract text: ${error instanceof Error ? error.message : String(error)}`
      );
    }
  }

  /**
   * Get PDF metadata
   */
  async getMetadata(filePath: string): Promise<PDFMetadata> {
    try {
      this.validateFilePath(filePath);

      const pdf = await pdfjsLib.getDocument(filePath).promise;
      const metadata = await pdf.getMetadata();
      const info = metadata.info || {};

      return {
        title: info.Title || undefined,
        author: info.Author || undefined,
        subject: info.Subject || undefined,
        creator: info.Creator || undefined,
        producer: info.Producer || undefined,
        creationDate: info.CreationDate ? new Date(info.CreationDate) : undefined,
        modificationDate: info.ModDate ? new Date(info.ModDate) : undefined,
        pageCount: pdf.numPages,
      };
    } catch (error) {
      throw new PDFValidationError(
        `Failed to get metadata: ${error instanceof Error ? error.message : String(error)}`
      );
    }
  }

  /**
   * Validate PDF file is readable and has valid structure
   */
  async validatePDF(filePath: string): Promise<boolean> {
    try {
      this.validateFilePath(filePath);

      const pdf = await pdfjsLib.getDocument(filePath).promise;
      // Try to access first page to confirm validity
      await pdf.getPage(1);

      return true;
    } catch (error) {
      return false;
    }
  }

  /**
   * Check if PDF has text layers (not just images/scanned)
   */
  async checkTextLayers(filePath: string): Promise<boolean> {
    try {
      this.validateFilePath(filePath);

      const pdf = await pdfjsLib.getDocument(filePath).promise;
      const page = await pdf.getPage(1);
      const textContent = await page.getTextContent();

      // Check if first page has any text content
      const hasText = textContent.items.length > 0 && textContent.items.some((item: any) => item.str?.trim());

      return hasText;
    } catch (error) {
      return false;
    }
  }

  /**
   * Validate file path exists and is readable
   */
  private validateFilePath(filePath: string): void {
    if (!filePath) {
      throw new PDFValidationError('File path is required');
    }

    if (!fs.existsSync(filePath)) {
      throw new PDFValidationError(`File not found: ${filePath}`);
    }

    if (!filePath.toLowerCase().endsWith('.pdf')) {
      throw new PDFValidationError('File must be a PDF');
    }

    // Check file is readable
    try {
      fs.accessSync(filePath, fs.constants.R_OK);
    } catch {
      throw new PDFValidationError(`File is not readable: ${filePath}`);
    }
  }
}
