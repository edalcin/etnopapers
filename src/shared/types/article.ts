/**
 * Article record types for EtnoPapers
 * Represents extracted scientific paper metadata
 */

export type SyncStatus = 'local' | 'pending' | 'synced' | 'failed';

/**
 * Plant species mentioned in an article
 */
export interface PlantSpecies {
  /** Common/local name (required) */
  vernacular: string;
  /** Scientific name (genus species) */
  nomeCientifico?: string;
  /** Category of plant use */
  tipoUso?: PlantUseType;
  /** Additional notes about this species */
  notasAdicionais?: string;
}

/**
 * Categories of plant use
 */
export type PlantUseType =
  | 'medicinal'
  | 'alimentação'
  | 'construção'
  | 'ritual'
  | 'artesanato'
  | 'combustível'
  | 'forrageiro'
  | 'outro';

/**
 * Indigenous or traditional community studied
 */
export interface Community {
  /** Community name */
  nome: string;
  /** Location description */
  localizacao?: string;
}

/**
 * Source PDF file information
 */
export interface SourceFileMetadata {
  fileName: string;
  fileSize: number;
  extractedAt: Date;
  pageCount: number;
}

/**
 * AI extraction metadata
 */
export interface ExtractionMetadata {
  model: string;
  extractedAt: Date;
  confidence?: number;
  executionTime?: number; // in milliseconds
}

/**
 * Main article record entity
 */
export interface ArticleRecord {
  // Metadata
  _id: string; // UUID v4
  createdAt: Date;
  updatedAt: Date;
  syncStatus: SyncStatus;

  // Mandatory Fields
  ano: number; // Publication year
  titulo: string; // Normalized title (proper case)
  autores: string[]; // APA formatted author names
  resumo: string; // Abstract (always in Brazilian Portuguese)

  // Optional Standard Fields
  publicacao?: string; // Publication source (journal name, volume, pages)
  especies?: PlantSpecies[]; // Array of plant species mentioned
  comunidade?: Community; // Studied community information
  metodologia?: string; // Research methodology

  // Geographic Data
  pais?: string; // Country
  estado?: string; // State/province
  municipio?: string; // Municipality/city
  local?: string; // Specific location description
  bioma?: string; // Biome classification

  // Custom Attributes (user-added)
  customAttributes?: Record<string, any>; // Extensible custom fields

  // Source Metadata
  sourceFile?: SourceFileMetadata; // Original PDF information
  extractionMetadata?: ExtractionMetadata; // AI extraction details
}
