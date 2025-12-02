/**
 * Validation Service
 * Handles validation of article records and extracted metadata using Zod
 */

import { IValidationService, ValidationResult, ValidationError } from '@shared/types/services';
import {
  ArticleRecordSchema,
  PlantSpeciesSchema,
  CommunitySchema,
} from '@shared/validation/schemas';
import { ArticleRecord } from '@shared/types/article';
import { ExtractedMetadata } from '@shared/types/services';

export class ValidationService implements IValidationService {
  /**
   * Validate complete article record
   */
  async validateRecord(record: any): Promise<ValidationResult> {
    try {
      // Parse with Zod
      ArticleRecordSchema.parse(record);
      return {
        valid: true,
        errors: [],
      };
    } catch (error) {
      const errors = this.parseZodError(error);
      return {
        valid: false,
        errors,
      };
    }
  }

  /**
   * Validate extracted metadata from OLLAMA
   */
  async validateExtractedData(data: any): Promise<ValidationResult> {
    const errors: ValidationError[] = [];

    // Validate mandatory fields
    const mandatoryFields = ['titulo', 'autores', 'ano', 'resumo'];
    for (const field of mandatoryFields) {
      if (!data[field]) {
        errors.push({
          field,
          message: `${field} is required`,
        });
      }
    }

    // Validate field types
    if (data.titulo && typeof data.titulo !== 'string') {
      errors.push({
        field: 'titulo',
        message: 'titulo must be a string',
      });
    }

    if (data.autores && !Array.isArray(data.autores)) {
      errors.push({
        field: 'autores',
        message: 'autores must be an array',
      });
    } else if (data.autores && data.autores.length === 0) {
      errors.push({
        field: 'autores',
        message: 'at least one author is required',
      });
    }

    if (data.ano && typeof data.ano !== 'number') {
      errors.push({
        field: 'ano',
        message: 'ano must be a number',
      });
    } else if (data.ano < 1500 || data.ano > new Date().getFullYear() + 1) {
      errors.push({
        field: 'ano',
        message: `ano must be between 1500 and ${new Date().getFullYear() + 1}`,
      });
    }

    if (data.resumo && typeof data.resumo !== 'string') {
      errors.push({
        field: 'resumo',
        message: 'resumo must be a string',
      });
    } else if (data.resumo && data.resumo.trim().length < 50) {
      errors.push({
        field: 'resumo',
        message: 'resumo must be at least 50 characters',
      });
    }

    // Validate optional fields if provided
    if (data.especies && Array.isArray(data.especies)) {
      data.especies.forEach((sp: any, index: number) => {
        try {
          PlantSpeciesSchema.parse(sp);
        } catch {
          errors.push({
            field: `especies[${index}]`,
            message: 'invalid species entry',
          });
        }
      });
    }

    if (data.comunidade) {
      try {
        CommunitySchema.parse(data.comunidade);
      } catch {
        errors.push({
          field: 'comunidade',
          message: 'invalid community entry',
        });
      }
    }

    return {
      valid: errors.length === 0,
      errors,
    };
  }

  /**
   * Check if mandatory fields are present
   */
  async checkMandatoryFields(record: any): Promise<boolean> {
    const mandatoryFields = ['titulo', 'autores', 'ano', 'resumo'];

    for (const field of mandatoryFields) {
      if (!record[field]) {
        return false;
      }

      if (field === 'autores' && Array.isArray(record[field]) && record[field].length === 0) {
        return false;
      }

      if (field === 'resumo' && typeof record[field] === 'string' && record[field].trim().length < 50) {
        return false;
      }
    }

    return true;
  }

  /**
   * Parse Zod error into ValidationError array
   */
  private parseZodError(error: any): ValidationError[] {
    const errors: ValidationError[] = [];

    if (error.errors && Array.isArray(error.errors)) {
      error.errors.forEach((err: any) => {
        const field = err.path.join('.');
        const message = err.message;
        errors.push({ field, message });
      });
    }

    return errors;
  }
}
