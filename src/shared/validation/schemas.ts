/**
 * Zod validation schemas for EtnoPapers data models
 */

import { z } from 'zod';

// Plant use types
const PlantUseTypeEnum = z.enum([
  'medicinal', 'alimentação', 'construção', 'ritual', 'artesanato', 'combustível', 'forrageiro', 'outro',
]);

// Plant species schema
export const PlantSpeciesSchema = z.object({
  vernacular: z.string().min(2).max(200),
  nomeCientifico: z.string().regex(/^[A-Z][a-z]+ [a-z]+.*$/).optional(),
  tipoUso: PlantUseTypeEnum.optional(),
  notasAdicionais: z.string().max(500).optional(),
});

// Community schema
export const CommunitySchema = z.object({
  nome: z.string().min(2).max(200),
  localizacao: z.string().max(500).optional(),
});

// Source file metadata
export const SourceFileMetadataSchema = z.object({
  fileName: z.string(),
  fileSize: z.number().positive(),
  extractedAt: z.date(),
  pageCount: z.number().positive(),
});

// Extraction metadata
export const ExtractionMetadataSchema = z.object({
  model: z.string(),
  extractedAt: z.date(),
  confidence: z.number().min(0).max(1).optional(),
  executionTime: z.number().positive().optional(),
});

// Main article record schema
export const ArticleRecordSchema = z.object({
  _id: z.string().uuid(),
  createdAt: z.date(),
  updatedAt: z.date(),
  syncStatus: z.enum(['local', 'pending', 'synced', 'failed']),

  // Mandatory fields
  ano: z.number().int().min(1500).max(new Date().getFullYear() + 1),
  titulo: z.string().min(5).max(500),
  autores: z.array(z.string().min(1)).min(1),
  resumo: z.string().min(50),

  // Optional fields
  publicacao: z.string().max(500).optional(),
  especies: z.array(PlantSpeciesSchema).optional(),
  comunidade: CommunitySchema.optional(),
  metodologia: z.string().max(2000).optional(),

  // Geographic data
  pais: z.string().max(100).optional(),
  estado: z.string().max(100).optional(),
  municipio: z.string().max(100).optional(),
  local: z.string().max(500).optional(),
  bioma: z.string().max(100).optional(),

  // Custom attributes
  customAttributes: z.record(z.any()).optional(),

  // Source metadata
  sourceFile: SourceFileMetadataSchema.optional(),
  extractionMetadata: ExtractionMetadataSchema.optional(),
});

// Type exports
export type ArticleRecord = z.infer<typeof ArticleRecordSchema>;
export type PlantSpecies = z.infer<typeof PlantSpeciesSchema>;
export type Community = z.infer<typeof CommunitySchema>;

// Validation helpers
export function validateArticleRecord(data: unknown) {
  return ArticleRecordSchema.safeParse(data);
}

export function validatePlantSpecies(data: unknown) {
  return PlantSpeciesSchema.safeParse(data);
}
