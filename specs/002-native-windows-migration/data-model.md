# Data Model: EtnoPapers Native Windows Application

**Date**: 2025-12-02
**Feature**: Migrate to Native Windows Desktop Application
**Status**: Design Complete

---

## Overview

This document defines the data structures used in the native EtnoPapers application. The model is based on the existing `docs/estrutura.json` schema and extended to support the new application requirements (local storage, sync status tracking, configuration management).

---

## Core Entities

### 1. ExtractedRecord

Represents a single research paper's extracted ethnobotanical metadata.

```csharp
public class ExtractedRecord
{
    // Identification
    [BsonId]
    public ObjectId Id { get; set; }                  // MongoDB unique identifier
    public string LocalId { get; set; }               // Local file identifier (GUID)

    // Paper Information
    public string Titulo { get; set; }                // Paper title (normalized case)
    public List<string> Autores { get; set; }         // Author array (APA format)
    public int Ano { get; set; }                      // Publication year

    // Abstract (always in Brazilian Portuguese per spec)
    public string Resumo { get; set; }                // Abstract/summary

    // Plant Species Data
    public List<Especie> Especies { get; set; }       // Plant species array

    // Geographic Location
    public string Pais { get; set; }                  // Country (e.g., "Brasil")
    public string Estado { get; set; }                // State/region (e.g., "SC")
    public string Municipio { get; set; }             // Municipality (e.g., "Florianópolis")
    public string Local { get; set; }                 // Specific location (e.g., "Sertão do Ribeirão")
    public string Bioma { get; set; }                 // Biome (e.g., "Mata Atlântica")

    // Community Information
    public Comunidade Comunidade { get; set; }        // Community details

    // Research Methodology
    public string Metodologia { get; set; }           // Research methodology used

    // Publication Reference
    public string Publicacao { get; set; }            // Publication citation (e.g., "Acta bot. bras. 24(2): 395-406")

    // Sync Status
    public SyncStatus SyncStatus { get; set; }        // Local/synced/pending status
    public DateTime? LastSyncedAt { get; set; }       // Last successful sync timestamp
    public string SyncErrorMessage { get; set; }      // Error message if sync failed

    // Audit
    public DateTime CreatedAt { get; set; }           // Record creation timestamp
    public DateTime UpdatedAt { get; set; }           // Last modification timestamp
    public DateTime? ExtractedAt { get; set; }        // When extracted from PDF
}
```

### 2. Especie (Plant Species)

Represents a single plant species mentioned in the research paper.

```csharp
public class Especie
{
    [BsonElement("vernacular")]
    public string Vernacular { get; set; }            // Vernacular/common name (e.g., "maçanilha")

    [BsonElement("nomeCientifico")]
    public string NomeCientifico { get; set; }        // Scientific name (e.g., "Chamomilla recutita")

    [BsonElement("tipoUso")]
    public string TipoUso { get; set; }               // Use type (e.g., "medicinal", "alimentação", "ritual")

    // Additional fields for enhanced data
    public List<string> OutrosUsos { get; set; }      // Other use types (optional)
    public string NovidadesCulturais { get; set; }    // Cultural/ethnobotanical notes (optional)
    public bool CientificamenteTestado { get; set; }  // Whether scientifically validated

    // Validation
    public bool ValidaCampos() => !string.IsNullOrWhiteSpace(Vernacular) &&
                                  !string.IsNullOrWhiteSpace(TipoUso);
}
```

### 3. Comunidade (Community)

Represents the indigenous or traditional community studied in the research.

```csharp
public class Comunidade
{
    [BsonElement("nome")]
    public string Nome { get; set; }                  // Community name (e.g., "Pescadores Artesanais")

    [BsonElement("localização")]
    public string Localizacao { get; set; }           // Community location (e.g., "Praia Branca")

    // Additional fields
    public string CodUNESCO { get; set; }             // UNESCO indigenous group code (optional)
    public string PopulacaoEstimada { get; set; }     // Estimated population (optional)
    public DateTime? DataColeta { get; set; }         // Data collection date (optional)

    // Validation
    public bool ValidaCampos() => !string.IsNullOrWhiteSpace(Nome) &&
                                  !string.IsNullOrWhiteSpace(Localizacao);
}
```

### 4. SyncStatus

Enumeration indicating synchronization state with MongoDB.

```csharp
public enum SyncStatus
{
    Local = 0,           // Only in local storage, never synced
    SyncPending = 1,     // Marked for sync but not yet uploaded
    Synced = 2,          // Successfully synced to MongoDB
    SyncFailed = 3,      // Sync attempted but failed (see error message)
    SyncPartial = 4      // Partially synced (some fields may be out of sync)
}
```

---

## Configuration Entity

### ApplicationConfiguration

Stores user settings and preferences.

```csharp
public class ApplicationConfiguration
{
    public string ConfigurationId { get; set; } = "singleton";  // Single instance per user

    // OLLAMA Configuration
    public OLLAMASettings OLLAMA { get; set; }

    // MongoDB Configuration
    public MongoDBSettings MongoDB { get; set; }

    // Application Settings
    public ApplicationSettings Application { get; set; }

    // Audit
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

#### OLLAMASettings

```csharp
public class OLLAMASettings
{
    public string Host { get; set; } = "localhost";           // OLLAMA server host
    public int Port { get; set; } = 11434;                   // OLLAMA server port
    public string Model { get; set; } = "mistral";           // Default model for extraction
    public bool IsConfigured { get; set; } = false;          // Configuration validated

    // Extraction Configuration
    public string ExtractionPrompt { get; set; }             // Custom extraction prompt
    public int TimeoutSeconds { get; set; } = 300;           // Request timeout

    // Last Known Status
    public DateTime? LastConnectionTest { get; set; }        // When connection was last validated
    public string LastConnectionError { get; set; }          // Last connection error message

    // Validation
    public bool ValidaCampos() =>
        !string.IsNullOrWhiteSpace(Host) &&
        Port > 0 && Port <= 65535 &&
        !string.IsNullOrWhiteSpace(Model);

    public string GetConnectionUrl() => $"http://{Host}:{Port}";
}
```

#### MongoDBSettings

```csharp
public class MongoDBSettings
{
    public string ConnectionUri { get; set; }                // MongoDB connection string
    public string DatabaseName { get; set; } = "etnopapers"; // Database name
    public string CollectionName { get; set; } = "records";  // Collection name
    public bool IsConfigured { get; set; } = false;          // Configuration validated

    // Connection Management
    public int ConnectionTimeoutMs { get; set; } = 10000;    // Connection timeout
    public int MaxPoolSize { get; set; } = 50;               // Connection pool size
    public bool AutoRetry { get; set; } = true;              // Enable automatic retries

    // Last Known Status
    public DateTime? LastConnectionTest { get; set; }        // When connection was last validated
    public string LastConnectionError { get; set; }          // Last connection error message

    // Validation
    public bool ValidaCampos() =>
        !string.IsNullOrWhiteSpace(ConnectionUri) &&
        !string.IsNullOrWhiteSpace(DatabaseName);
}
```

#### ApplicationSettings

```csharp
public class ApplicationSettings
{
    // UI Preferences
    public string Theme { get; set; } = "Light";             // "Light" or "Dark"
    public string Language { get; set; } = "pt-BR";          // Portuguese (Brazil)
    public int WindowWidth { get; set; } = 1200;             // Default window width
    public int WindowHeight { get; set; } = 800;             // Default window height
    public bool WindowMaximized { get; set; } = false;       // Window maximized state

    // Storage Management
    public string LocalStoragePath { get; set; }             // Path for local JSON files
    public int MaxLocalRecords { get; set; } = 1000;         // Warning threshold
    public bool ShowStorageWarnings { get; set; } = true;    // Show when near capacity

    // Processing
    public bool ShowExtractionProgress { get; set; } = true; // Show detailed progress
    public int MaxConcurrentExtractions { get; set; } = 1;   // Parallel PDF processing limit

    // Sync Behavior
    public bool AutoSyncOnTimer { get; set; } = false;       // Enable auto-sync feature
    public int AutoSyncIntervalMinutes { get; set; } = 60;   // Auto-sync interval
    public bool DeleteLocalAfterSync { get; set; } = true;   // Delete local after successful sync
}
```

---

## Storage Schema

### Local JSON Storage Structure

Records are stored in local JSON files following this directory structure:

```
[LocalStoragePath]/
├── records/
│   ├── [GUID-1].json                # Individual record files
│   ├── [GUID-2].json
│   └── ...
├── config/
│   └── application-config.json      # Application configuration
└── index.json                        # Index of all local records (for performance)
```

#### Record File Example

```json
{
  "_id": "507f1f77bcf86cd799439011",
  "localId": "a1b2c3d4-e5f6-47a8-9b0c-1d2e3f4a5b6c",
  "titulo": "Uso e conhecimento tradicional de plantas medicinais no Sertão",
  "autores": ["Giraldi, M.", "Hanazaki, N."],
  "ano": 2010,
  "resumo": "O objetivo desta pesquisa foi...",
  "especies": [
    {
      "vernacular": "maçanilha",
      "nomeCientifico": "Chamomilla recutita",
      "tipoUso": "medicinal"
    }
  ],
  "comunidade": {
    "nome": "Pescadores Artesanais da Praia Branca",
    "localização": "Praia Branca"
  },
  "pais": "Brasil",
  "estado": "SC",
  "municipio": "Florianópolis",
  "local": "Sertão do Ribeirão",
  "bioma": "Mata Atlântica",
  "metodologia": "entrevistas",
  "publicacao": "Acta bot. bras. 24(2): 395-406",
  "syncStatus": "Local",
  "lastSyncedAt": null,
  "syncErrorMessage": null,
  "createdAt": "2025-12-02T10:30:00Z",
  "updatedAt": "2025-12-02T10:30:00Z",
  "extractedAt": "2025-12-02T10:30:00Z"
}
```

#### Configuration File Example

```json
{
  "configurationId": "singleton",
  "ollama": {
    "host": "localhost",
    "port": 11434,
    "model": "mistral",
    "isConfigured": true,
    "extractionPrompt": "Extract ethnobotanical metadata from this paper...",
    "timeoutSeconds": 300,
    "lastConnectionTest": "2025-12-02T10:00:00Z",
    "lastConnectionError": null
  },
  "mongodb": {
    "connectionUri": "mongodb+srv://user:pass@cluster.mongodb.net/",
    "databaseName": "etnopapers",
    "collectionName": "records",
    "isConfigured": true,
    "connectionTimeoutMs": 10000,
    "maxPoolSize": 50,
    "autoRetry": true,
    "lastConnectionTest": "2025-12-02T10:15:00Z",
    "lastConnectionError": null
  },
  "application": {
    "theme": "Light",
    "language": "pt-BR",
    "windowWidth": 1200,
    "windowHeight": 800,
    "windowMaximized": false,
    "localStoragePath": "C:\\Users\\[User]\\AppData\\Local\\EtnoPapers\\data",
    "maxLocalRecords": 1000,
    "showStorageWarnings": true,
    "showExtractionProgress": true,
    "maxConcurrentExtractions": 1,
    "autoSyncOnTimer": false,
    "autoSyncIntervalMinutes": 60,
    "deleteLocalAfterSync": true
  },
  "createdAt": "2025-12-02T09:00:00Z",
  "updatedAt": "2025-12-02T10:30:00Z"
}
```

---

## Validation Rules

### ExtractedRecord Validation

| Field | Required | Min Length | Max Length | Format | Notes |
|-------|----------|-----------|-----------|--------|-------|
| Titulo | Yes | 1 | 500 | String | Normalize case on save |
| Ano | Yes | 1900 | 2100 | Int | Reasonable year range |
| Resumo | Yes | 10 | 10000 | String | Must be in Portuguese |
| Pais | Yes | 2 | 100 | String | Country name |
| Estado | No | 1 | 100 | String | State/region code |
| Municipio | No | 1 | 100 | String | Municipality name |
| Especies | Yes | 1 | 1000 items | Array | At least one species |
| Comunidade | No | - | - | Object | If present, must have Nome |
| Metodologia | No | 1 | 500 | String | Research method description |

### MongoDB Sync Validation

Before syncing a record to MongoDB:

1. ✅ All required fields present and valid
2. ✅ Especies array contains at least one valid entry
3. ✅ Ano is within reasonable range (1900-2100)
4. ✅ Resumo contains valid Portuguese text (optional language detection)
5. ✅ No missing references between entities (e.g., Comunidade is either complete or null)

### Configuration Validation

- OLLAMA settings: Host/port must allow connection or show error
- MongoDB settings: URI must parse and allow connection test
- Storage path: Must be writable directory or show error

---

## Data Migration Strategy

### From Electron Application to Native Windows

**Existing Data Preservation**:

1. **Import from Electron JSON files**:
   - Read existing local records
   - Convert to new schema (add LocalId, SyncStatus, timestamps)
   - Re-validate all records
   - Store in new application data directory

2. **Configuration Migration**:
   - Extract OLLAMA settings from Electron config
   - Extract MongoDB URI if configured
   - Preserve user preferences (theme, window size)
   - Create new ApplicationConfiguration

3. **Rollback Safety**:
   - Backup original Electron data before import
   - Provide export functionality to restore if needed

---

## Performance Considerations

### Local Storage

- **Index File**: Maintain `index.json` with record summary for fast loading
  - Reduces startup time by avoiding full file I/O
  - Updated when records are added/modified/deleted

- **Pagination**: Load records in batches (e.g., 50 at a time)
  - Improves UI responsiveness with large record sets
  - Use VirtualizingStackPanel for list views

- **File Organization**: One record per file
  - Enables atomic updates (single file per record)
  - Simplifies concurrent access
  - Reduces memory footprint per load

### MongoDB Sync

- **Batch Operations**: Sync multiple records in single operation
  - Reduces network round trips
  - Improves performance

- **Connection Pooling**: MongoDB.Driver handles automatically
  - Default pool size: 50 connections
  - Configured in MongoDBSettings.MaxPoolSize

- **Retry Logic**: Automatic retries with exponential backoff
  - Failed syncs don't block UI
  - Status tracked in SyncStatus

---

## Summary

The data model separates concerns into:

1. **Business Entities**: ExtractedRecord, Especie, Comunidade (represent research data)
2. **Configuration**: OLLAMASettings, MongoDBSettings, ApplicationSettings (user preferences)
3. **Infrastructure**: SyncStatus, audit fields (support application behavior)

The schema supports:
- ✅ All mandatory fields from original `docs/estrutura.json`
- ✅ Offline-first with local JSON storage
- ✅ Optional MongoDB sync with status tracking
- ✅ Configuration management for external services
- ✅ Audit trail (creation/modification timestamps)
- ✅ Validation and error handling

