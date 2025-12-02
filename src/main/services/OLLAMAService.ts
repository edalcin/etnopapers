/**
 * OLLAMA Service
 * Handles communication with OLLAMA API for text extraction and translation
 */

import axios, { AxiosInstance } from 'axios';
import { IOLLAMAService, ExtractedMetadata } from '@shared/types/services';
import { OLLAMAConfig } from '@shared/types/config';
import { OLLAMAConnectionError, OLLAMAExtractionError } from '@shared/types/errors';

export class OLLAMAService implements IOLLAMAService {
  private client: AxiosInstance;
  private config: OLLAMAConfig;

  private defaultPrompt = `Extract metadata from the following academic article text. Return a JSON object with these fields:
- titulo: Article title in original language
- autores: Array of author names
- ano: Publication year (as number)
- resumo: Article abstract/summary in Portuguese (translate if needed)
- especies: Array of plant species mentioned with {vernacular, nomeCientifico, tipoUso}
- comunidade: Object with {nome, localizacao} if indigenous/traditional community is mentioned
- metodologia: Research methodology (in Portuguese)
- pais: Country
- estado: State/Province
- municipio: Municipality
- bioma: Biome/ecosystem
- local: Specific location

Return ONLY valid JSON, no markdown formatting.`;

  constructor(config: OLLAMAConfig) {
    this.config = config;
    this.client = axios.create({
      baseURL: config.baseUrl,
      timeout: config.timeout || 120000,
    });
  }

  /**
   * Check if OLLAMA service is healthy and responsive
   */
  async checkHealth(): Promise<boolean> {
    try {
      const response = await this.client.get('/api/tags');
      return response.status === 200;
    } catch (error) {
      throw new OLLAMAConnectionError(this.config.baseUrl);
    }
  }

  /**
   * Extract metadata from PDF text using OLLAMA
   */
  async extractMetadata(text: string): Promise<ExtractedMetadata> {
    try {
      if (!text || text.trim().length === 0) {
        throw new OLLAMAExtractionError('Empty text provided for extraction');
      }

      // Truncate text to reasonable size (prevent token overflow)
      const maxLength = 8000;
      const truncatedText = text.length > maxLength ? text.substring(0, maxLength) : text;

      const response = await this.client.post('/api/generate', {
        model: this.config.model,
        prompt: `${this.config.prompt || this.defaultPrompt}\n\nText to analyze:\n${truncatedText}`,
        stream: false,
        temperature: 0.1, // Lower temperature for more consistent extraction
      });

      if (!response.data.response) {
        throw new OLLAMAExtractionError('No response from OLLAMA');
      }

      // Parse JSON response
      const jsonMatch = response.data.response.match(/\{[\s\S]*\}/);
      if (!jsonMatch) {
        throw new OLLAMAExtractionError('Could not parse JSON from OLLAMA response');
      }

      const extracted = JSON.parse(jsonMatch[0]) as ExtractedMetadata;

      // Ensure minimum required fields
      if (!extracted.titulo || !extracted.autores || !extracted.ano || !extracted.resumo) {
        throw new OLLAMAExtractionError(
          'Missing required fields in extraction. Ensure PDF contains complete article information.'
        );
      }

      return extracted;
    } catch (error) {
      if (error instanceof OLLAMAExtractionError) {
        throw error;
      }
      if (error instanceof SyntaxError) {
        throw new OLLAMAExtractionError(`Invalid JSON in response: ${error.message}`);
      }
      throw new OLLAMAExtractionError(
        error instanceof Error ? error.message : 'Unknown error during extraction'
      );
    }
  }

  /**
   * Translate text to Portuguese
   */
  async translateToPortuguese(text: string): Promise<string> {
    try {
      if (!text || text.trim().length === 0) {
        return '';
      }

      const response = await this.client.post('/api/generate', {
        model: this.config.model,
        prompt: `Translate the following text to Brazilian Portuguese. Return only the translated text, nothing else:\n\n${text}`,
        stream: false,
        temperature: 0.3,
      });

      if (!response.data.response) {
        throw new OLLAMAExtractionError('No response from OLLAMA');
      }

      return response.data.response.trim();
    } catch (error) {
      throw new OLLAMAExtractionError(
        error instanceof Error
          ? error.message
          : 'Translation failed'
      );
    }
  }

  /**
   * Get list of available models from OLLAMA
   */
  async getAvailableModels(): Promise<string[]> {
    try {
      const response = await this.client.get('/api/tags');

      if (!response.data.models || !Array.isArray(response.data.models)) {
        return [this.config.model]; // Return configured model as fallback
      }

      return response.data.models.map((model: any) => model.name || model);
    } catch (error) {
      throw new OLLAMAConnectionError(this.config.baseUrl);
    }
  }

  /**
   * Update configuration
   */
  updateConfig(config: OLLAMAConfig): void {
    this.config = config;
    this.client = axios.create({
      baseURL: config.baseUrl,
      timeout: config.timeout || 120000,
    });
  }
}
