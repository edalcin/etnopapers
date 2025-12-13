# Data Model: EtnoPapers

**Date**: 2025-12-01
**Feature**: Ethnobotanical Metadata Extraction Desktop Application
**Phase**: 1 - Data Model Design

## Overview

This document defines the complete data model for EtnoPapers, including TypeScript interfaces, validation rules, and data transformation logic. The model is designed to match the structure defined in `docs/estrutura.json` while providing type safety and validation.

---

## Core Entities

### 1. Article Record

The primary entity representing an extracted scientific paper with ethnobotanical metadata.

```typescript
interface ArticleRecord {
  // Metadata
  _id: string                    // UUID v4
  createdAt: Date                // Local creation timestamp
  updatedAt: Date                // Last modification timestamp
  syncStatus: SyncStatus         // Sync state with MongoDB

  // Mandatory Fields (always required)
  ano: number                    // Publication year (YYYY format)
  titulo: string                 // Normalized title (proper case)
  autores: string[]              // APA formatted author names
  resumo: string                 // Abstract (always in Brazilian Portuguese)

  // Optional Standard Fields
  publicacao?: string            // Publication source (journal name, volume, pages)
  especies?: PlantSpecies[]      // Array of plant species mentioned
  comunidade?: Community         // Studied community information
  metodologia?: string           // Research methodology

  // Geographic Data
  pais?: string                  // Country
  estado?: string                // State/province
  municipio?: string             // Municipality/city
  local?: string                 // Specific location description
  bioma?: string                 // Biome classification

  // Custom Attributes (user-added)
  customAttributes?: Record<string, any>  // Extensible custom fields

  // Source Metadata
  sourceFile?: SourceFileMetadata         // Original PDF information
  extractionMetadata?: ExtractionMetadata // AI extraction details
}
```

**Validation Rules**:
- `_id`: Must be valid UUID v4
- `ano`: Must be between 1500 and current year + 1
- `titulo`: Min 5 chars, max 500 chars, non-empty after trim
- `autores`: Array must have at least 1 element, each element non-empty
- `resumo`: Min 50 chars (typical abstract length)
- `publicacao`: Max 500 chars if provided
- `pais`, `estado`, `municipio`: Max 100 chars each
- `bioma`: Must match known biome list if provided

---

### 2. Plant Species

Represents a plant species mentioned in the article.

```typescript
interface PlantSpecies {
  vernacular: string          // Common/local name (required)
  nomeCientifico?: string     // Scientific name (genus species)
  tipoUso?: PlantUseType      // Category of use
  notasAdicionais?: string    // Additional notes about this species
}

type PlantUseType =
  | 'medicinal'
  | 'alimentação'
  | 'construção'
  | 'ritual'
  | 'artesanato'
  | 'combustível'
  | 'forrageiro'
  | 'outro'
```

**Validation Rules**:
- `vernacular`: Required, min 2 chars, max 200 chars
- `nomeCientifico`: If provided, must match pattern: `^[A-Z][a-z]+ [a-z]+.*$` (Genus species format)
- `tipoUso`: Must be one of enum values if provided

---

### 3. Community

Represents the indigenous or traditional community studied.

```typescript
interface Community {
  nome: string              // Community name (required)
  localizacao?: string      // Location description
  populacao?: number        // Population size
  etnia?: string            // Ethnic group
  notas?: string            // Additional notes
}
```

**Validation Rules**:
- `nome`: Required, min 2 chars, max 200 chars
- `populacao`: Must be positive integer if provided
- `localizacao`: Max 500 chars

---

### 4. Sync Status

Tracks the synchronization state of a record.

```typescript
type SyncStatus =
  | 'local'        // Only exists locally, never synced
  | 'pending'      // Marked for sync but not yet uploaded
  | 'syncing'      // Currently being uploaded
  | 'synced'       // Successfully uploaded to MongoDB
  | 'error'        // Failed to sync (will retry)

interface SyncMetadata {
  status: SyncStatus
  lastSyncAttempt?: Date
  lastSyncSuccess?: Date
  syncErrorMessage?: string
  retryCount?: number
}
```

---

### 5. Source File Metadata

Information about the original PDF file.

```typescript
interface SourceFileMetadata {
  originalFileName: string     // Original file name (e.g., "paper.pdf")
  fileSizeBytes: number        // File size
  uploadedAt: Date             // When PDF was uploaded
  pdfMetadata?: {
    title?: string             // PDF metadata title
    author?: string            // PDF metadata author
    producer?: string          // PDF producer
    creationDate?: Date        // PDF creation date
    pageCount?: number         // Number of pages
  }
}
```

---

### 6. Extraction Metadata

Details about the AI extraction process.

```typescript
interface ExtractionMetadata {
  model: string                // Cloud AI provider model used (e.g., "gemini-1.5-flash", "gpt-4o-mini", "claude-3-5-haiku")
  extractedAt: Date            // When extraction occurred
  processingTimeMs: number     // Time taken to extract
  promptVersion: string        // Version/hash of prompt used
  confidence?: {
    overall?: number           // 0-1 confidence score
    fieldScores?: Record<string, number>  // Per-field confidence
  }
  rawResponse?: string         // Raw AI provider response (for debugging)
  manuallyEdited: boolean      // Has user edited after extraction?
  editedFields?: string[]      // Which fields were edited
}
```

---

## Supporting Types

### Configuration

```typescript
interface AppConfiguration {
  aiProvider: AIProviderConfig
  mongodb: MongoDBConfig
  storage: StorageConfig
  ui: UIConfig
  version: string              // Config schema version
}

interface AIProviderConfig {
  provider: 'gemini' | 'openai' | 'anthropic'  // Selected cloud AI provider
  apiKey: string               // Encrypted API key
  model?: string               // Optional: specific model override
  timeout: number              // Seconds, default: 60

  // Legacy OLLAMA fields (deprecated, kept for backward compatibility)
  ollamaUrl?: string           // Legacy: http://localhost:11434
  ollamaModel?: string         // Legacy: llama2
  ollamaPrompt?: string        // Legacy: customizable extraction prompt
}

interface MongoDBConfig {
  uri: string | null           // User-provided connection string
  database: string             // Default: "etnopapers"
  collection: string           // Default: "articles"
  tlsEnabled: boolean          // Default: true for Atlas
  connectionTimeout: number    // Seconds, default: 10
}

interface StorageConfig {
  dataFilePath: string         // Path to local JSON file
  maxLocalRecords: number      // Default: 1000
  autoBackupReminder: boolean  // Default: true
  reminderIntervalDays: number // Default: 7
  enableAutoSave: boolean      // Default: true
  autoSaveIntervalSeconds: number  // Default: 30
}

interface UIConfig {
  theme: 'light' | 'dark'      // Default: 'light'
  language: 'pt-BR' | 'en-US'  // Default: 'pt-BR'
  windowState: {
    width: number
    height: number
    x?: number
    y?: number
    maximized: boolean
  }
}
```

---

## Data Transformations

### 1. Title Normalization

Convert title to proper case:

```typescript
function normalizeTitle(title: string): string {
  // Rules:
  // - First word and words after punctuation: Title Case
  // - Articles, conjunctions, prepositions: lowercase (except first word)
  // - Acronyms: preserve (detected by all caps)
  // - Scientific names: preserve italics markers if present

  const lowercaseWords = ['de', 'da', 'do', 'das', 'dos', 'e', 'ou', 'para',
                          'com', 'em', 'a', 'o', 'os', 'as', 'um', 'uma',
                          'the', 'of', 'and', 'or', 'in', 'on', 'at', 'to', 'for']

  // Implementation would split on spaces, apply rules, join
  // Example: "uso DE plantas" → "Uso de Plantas"
}
```

### 2. Author Formatting (APA Style)

Convert author names to APA format:

```typescript
function formatAuthorAPA(fullName: string): string {
  // APA format: LastName, FirstInitial.MiddleInitial.
  // Examples:
  // "João Silva" → "Silva, J."
  // "Maria da Silva Santos" → "Santos, M. S."
  // "John Smith Jr." → "Smith Jr., J."

  // Special handling for:
  // - Particles (de, da, van, von)
  // - Suffixes (Jr., Sr., III)
  // - Hyphenated names
}
```

### 3. Abstract Translation Detection

Detect if abstract needs translation to Portuguese:

```typescript
function detectLanguage(text: string): 'pt' | 'en' | 'es' | 'other' {
  // Use language detection library or heuristics
  // Common Portuguese indicators: "de", "para", "com", "são"
  // Common English indicators: "the", "and", "with", "are"
}

async function ensurePortugueseAbstract(
  abstract: string,
  aiProvider: IAIProvider
): Promise<string> {
  const lang = detectLanguage(abstract)

  if (lang === 'pt') {
    return abstract  // Already Portuguese
  }

  // Request translation from cloud AI provider
  return aiProvider.translate(abstract, targetLang: 'pt-BR')
}
```

---

## Validation Schema (Zod)

```typescript
import { z } from 'zod'

const PlantSpeciesSchema = z.object({
  vernacular: z.string().min(2).max(200),
  nomeCientifico: z.string().regex(/^[A-Z][a-z]+ [a-z]+.*$/).optional(),
  tipoUso: z.enum([
    'medicinal', 'alimentação', 'construção', 'ritual',
    'artesanato', 'combustível', 'forrageiro', 'outro'
  ]).optional(),
  notasAdicionais: z.string().max(1000).optional()
})

const CommunitySchema = z.object({
  nome: z.string().min(2).max(200),
  localizacao: z.string().max(500).optional(),
  populacao: z.number().int().positive().optional(),
  etnia: z.string().max(100).optional(),
  notas: z.string().max(1000).optional()
})

const ArticleRecordSchema = z.object({
  _id: z.string().uuid(),
  createdAt: z.date(),
  updatedAt: z.date(),
  syncStatus: z.enum(['local', 'pending', 'syncing', 'synced', 'error']),

  // Mandatory
  ano: z.number().int().min(1500).max(new Date().getFullYear() + 1),
  titulo: z.string().min(5).max(500).trim(),
  autores: z.array(z.string().min(1)).min(1),
  resumo: z.string().min(50),

  // Optional
  publicacao: z.string().max(500).optional(),
  especies: z.array(PlantSpeciesSchema).optional(),
  comunidade: CommunitySchema.optional(),
  metodologia: z.string().max(1000).optional(),

  // Geographic
  pais: z.string().max(100).optional(),
  estado: z.string().max(100).optional(),
  municipio: z.string().max(100).optional(),
  local: z.string().max(500).optional(),
  bioma: z.string().max(100).optional(),

  // Extensibility
  customAttributes: z.record(z.any()).optional(),
  sourceFile: z.any().optional(),  // Full schema omitted for brevity
  extractionMetadata: z.any().optional()
})

// Export for runtime validation
export function validateArticleRecord(data: unknown): ArticleRecord {
  return ArticleRecordSchema.parse(data)
}
```

---

## Database Schemas

### Local Storage (lowdb / JSON)

```json
{
  "articles": [
    {
      "_id": "uuid-here",
      "ano": 2010,
      "titulo": "Uso e Conhecimento Tradicional de Plantas Medicinais",
      "autores": ["Giraldi, M.", "Hanazaki, N."],
      "resumo": "O objetivo desta pesquisa...",
      ...
    }
  ],
  "metadata": {
    "version": "1.0.0",
    "recordCount": 1,
    "lastModified": "2025-12-01T10:00:00Z"
  }
}
```

### MongoDB Schema

```javascript
// MongoDB collection: articles
{
  _id: ObjectId("..."),          // MongoDB auto-generated
  externalId: "uuid-from-local", // Matches local _id for tracking

  // All fields from ArticleRecord
  ano: 2010,
  titulo: "...",
  autores: ["..."],
  resumo: "...",

  // MongoDB-specific metadata
  uploadedAt: ISODate("2025-12-01T10:00:00Z"),
  uploadedFrom: "EtnoPapers v1.0.0",
  originalLocalId: "uuid-from-local"
}

// Indexes
db.articles.createIndex({ "ano": 1 })
db.articles.createIndex({ "autores": 1 })
db.articles.createIndex({ "especies.nomeCientifico": 1 })
db.articles.createIndex({ "comunidade.nome": 1 })
db.articles.createIndex({ "bioma": 1 })
db.articles.createIndex({ "externalId": 1 }, { unique: true })
```

---

## Data Lifecycle

### 1. Creation (PDF Upload)
```
PDF → Markdown Conversion → Cloud AI Provider API → Raw JSON →
Validation → Normalization → ArticleRecord (local) →
User Review/Edit → Save to local storage
```

### 2. Update (User Edit)
```
Load from lowdb → UI Form → User Edits →
Validation → Update Record →
Save to lowdb → Update `updatedAt` + `manuallyEdited`
```

### 3. Sync (Upload to MongoDB)
```
Select Records → Set status='pending' →
For each record: Upload to MongoDB →
On success: Delete from local + status='synced' →
On error: Keep local + status='error' + error message
```

### 4. Deletion
```
User confirms → Remove from lowdb →
Update file metadata (recordCount) →
Compact JSON file (optional)
```

---

## Data Validation Rules Summary

| Field | Rule | Error Message (PT) |
|-------|------|-------------------|
| ano | 1500 ≤ ano ≤ (current year + 1) | "Ano deve estar entre 1500 e [ano atual]" |
| titulo | 5 ≤ length ≤ 500, not empty | "Título deve ter entre 5 e 500 caracteres" |
| autores | length ≥ 1, each element not empty | "Pelo menos um autor é obrigatório" |
| resumo | length ≥ 50 | "Resumo deve ter pelo menos 50 caracteres" |
| nomeCientifico | Matches `Genus species` format | "Nome científico deve seguir formato: Gênero espécie" |
| maxLocalRecords | record count ≤ 1000 | "Limite de 1000 registros locais atingido. Faça sync com MongoDB." |

---

## Migration & Versioning

### Schema Version
Current version: `1.0.0`

### Future Migration Strategy
When data model changes:
1. Add `schemaVersion` field to each record
2. Implement migration functions: `migrateFrom_v1_to_v2()`
3. Run migrations on app startup
4. Maintain backward compatibility for 2 versions

### Example Migration
```typescript
function migrateToV2(record: ArticleRecordV1): ArticleRecordV2 {
  return {
    ...record,
    schemaVersion: '2.0.0',
    // Add new fields with defaults
    newField: defaultValue,
    // Transform existing fields if needed
    oldField: transformOldField(record.oldField)
  }
}
```

---

## Performance Considerations

### Large Dataset Handling
- **Virtual scrolling**: Render only visible records in UI
- **Lazy loading**: Load records in batches of 50
- **Search indexing**: Build in-memory index for fast filtering
- **Pagination**: Paginate MongoDB queries (limit 100 per page)

### Memory Management
- **Record limit**: Hard cap at 1000 local records
- **File watching**: Debounce auto-save to avoid excessive writes
- **Garbage collection**: Clear unused cached data

---

## Conclusion

This data model provides:
- **Type safety**: Full TypeScript coverage
- **Validation**: Runtime checks with Zod
- **Flexibility**: Custom attributes for extensibility
- **Compatibility**: Matches docs/estrutura.json format
- **Traceability**: Metadata for sync and extraction tracking

The model supports the complete workflow from PDF extraction to MongoDB synchronization while maintaining data integrity and user control.
