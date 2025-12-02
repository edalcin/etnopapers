# Service Contracts: EtnoPapers Native Windows Application

**Date**: 2025-12-02
**Feature**: Migrate to Native Windows Desktop Application
**Architecture**: C# WPF with dependency injection

---

## Overview

This document defines the service contracts (interfaces) for core business logic components. These services handle PDF processing, data storage, MongoDB synchronization, and OLLAMA integration.

---

## Core Services

### 1. IPdfProcessingService

Handles PDF file upload, validation, and text extraction.

```csharp
public interface IPdfProcessingService
{
    /// <summary>
    /// Validates that the uploaded file is a valid PDF.
    /// </summary>
    /// <param name="filePath">Full path to the PDF file</param>
    /// <returns>Validation result with error details if invalid</returns>
    Task<ValidationResult> ValidateFileAsync(string filePath);

    /// <summary>
    /// Extracts raw text from a PDF file.
    /// </summary>
    /// <param name="filePath">Full path to the PDF file</param>
    /// <param name="cancellationToken">Cancellation token for long-running operations</param>
    /// <returns>Extracted text content</returns>
    /// <remarks>
    /// - Returns empty string for non-searchable PDFs (scanned images)
    /// - Handles complex layouts and multi-column text
    /// - Preserves line breaks and paragraph structure
    /// </remarks>
    Task<string> ExtractTextAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the text extraction progress for a file being processed.
    /// </summary>
    /// <param name="fileId">Identifier for the file being processed</param>
    /// <returns>Progress percentage (0-100) and status message</returns>
    ProgressUpdate GetProgress(string fileId);

    /// <summary>
    /// Reports progress of PDF extraction to UI.
    /// </summary>
    event EventHandler<ProgressEventArgs> ProgressChanged;
}

public class ValidationResult
{
    public bool IsValid { get; set; }
    public string ErrorMessage { get; set; }
    public int PageCount { get; set; }
    public bool IsSearchable { get; set; }
}

public class ProgressEventArgs : EventArgs
{
    public string FileId { get; set; }
    public int ProgressPercentage { get; set; }
    public string StatusMessage { get; set; }
}
```

---

### 2. IOLLAMAExtractionService

Handles communication with local OLLAMA service for metadata extraction.

```csharp
public interface IOLLAMAExtractionService
{
    /// <summary>
    /// Tests connectivity to the OLLAMA service.
    /// </summary>
    /// <returns>True if OLLAMA is reachable and responding</returns>
    Task<bool> TestConnectionAsync();

    /// <summary>
    /// Sends PDF text to OLLAMA for ethnobotanical metadata extraction.
    /// </summary>
    /// <param name="pdfText">Raw text extracted from PDF</param>
    /// <param name="prompt">Custom extraction prompt (uses default if null)</param>
    /// <param name="cancellationToken">Cancellation token for interrupting long extractions</param>
    /// <returns>Extracted metadata as unstructured text from OLLAMA</returns>
    Task<string> ExtractMetadataAsync(
        string pdfText,
        string prompt = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the default extraction prompt used by the service.
    /// </summary>
    string GetDefaultPrompt();

    /// <summary>
    /// Validates that an extraction prompt is properly formatted.
    /// </summary>
    ValidationResult ValidatePrompt(string prompt);

    /// <summary>
    /// Reports progress of OLLAMA extraction to UI.
    /// </summary>
    event EventHandler<ProgressEventArgs> ExtractionProgress;

    /// <summary>
    /// Reports extraction errors (e.g., OLLAMA unavailable).
    /// </summary>
    event EventHandler<ErrorEventArgs> ExtractionError;
}

public class ErrorEventArgs : EventArgs
{
    public string ErrorCode { get; set; }
    public string ErrorMessage { get; set; }
    public Exception Exception { get; set; }
}
```

---

### 3. IMetadataParsingService

Parses unstructured OLLAMA output into structured ExtractedRecord.

```csharp
public interface IMetadataParsingService
{
    /// <summary>
    /// Parses raw OLLAMA output into a structured ExtractedRecord.
    /// </summary>
    /// <param name="ollamaOutput">Unstructured text from OLLAMA</param>
    /// <returns>Parsed ExtractedRecord with validation errors if present</returns>
    /// <remarks>
    /// Uses heuristics to extract structured fields:
    /// - Title: First substantial sentence
    /// - Authors: Names followed by affiliations
    /// - Year: 4-digit number between 1900-2100
    /// - Species: "comum name (scientific name)" patterns
    /// - Geographic data: Country/state/municipality patterns
    /// </remarks>
    Task<ParsedMetadataResult> ParseMetadataAsync(string ollamaOutput);

    /// <summary>
    /// Validates and normalizes a parsed record.
    /// </summary>
    ValidationResult ValidateRecord(ExtractedRecord record);

    /// <summary>
    /// Gets confidence score for extraction quality (0-100).
    /// </summary>
    /// <remarks>
    /// Based on:
    /// - All mandatory fields present: +20 points each
    /// - Geographic data completeness: +15 points
    /// - Species array populated: +10 points
    /// </remarks>
    int GetConfidenceScore(ExtractedRecord record);
}

public class ParsedMetadataResult
{
    public ExtractedRecord Record { get; set; }
    public List<string> ParseWarnings { get; set; } = new();
    public int ConfidenceScore { get; set; }
}
```

---

### 4. ILocalStorageService

Handles local JSON file storage for offline access.

```csharp
public interface ILocalStorageService
{
    /// <summary>
    /// Saves a record to local JSON storage.
    /// </summary>
    /// <param name="record">Record to save</param>
    /// <returns>Saved record with updated LocalId and timestamps</returns>
    Task<ExtractedRecord> SaveRecordAsync(ExtractedRecord record);

    /// <summary>
    /// Loads a record from local storage by LocalId.
    /// </summary>
    Task<ExtractedRecord> LoadRecordAsync(string localId);

    /// <summary>
    /// Gets all records from local storage with pagination.
    /// </summary>
    /// <param name="pageNumber">Page number (starts at 1)</param>
    /// <param name="pageSize">Records per page (default 50)</param>
    /// <returns>Page of records matching filter criteria</returns>
    Task<PagedResult<ExtractedRecord>> GetRecordsAsync(
        int pageNumber = 1,
        int pageSize = 50,
        RecordFilter filter = null);

    /// <summary>
    /// Deletes a record from local storage.
    /// </summary>
    Task<bool> DeleteRecordAsync(string localId);

    /// <summary>
    /// Updates an existing record.
    /// </summary>
    Task<ExtractedRecord> UpdateRecordAsync(ExtractedRecord record);

    /// <summary>
    /// Gets count of all records in local storage.
    /// </summary>
    Task<int> GetRecordCountAsync();

    /// <summary>
    /// Checks if local storage is approaching capacity.
    /// </summary>
    /// <returns>Usage percentage (0-100) and warning if >80%</returns>
    Task<StorageStatus> GetStorageStatusAsync();

    /// <summary>
    /// Exports all local records to a ZIP file.
    /// </summary>
    Task<string> ExportAllRecordsAsync(string exportPath);

    /// <summary>
    /// Imports records from an exported ZIP file.
    /// </summary>
    Task ImportRecordsAsync(string importPath);
}

public class RecordFilter
{
    public string SearchText { get; set; }          // Full-text search in title/authors/resumo
    public int? YearFrom { get; set; }              // Filter by year range
    public int? YearTo { get; set; }
    public string Pais { get; set; }                // Filter by country
    public SyncStatus? SyncStatus { get; set; }     // Filter by sync status
}

public class PagedResult<T>
{
    public List<T> Items { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
}

public class StorageStatus
{
    public int RecordCount { get; set; }
    public long UsageBytes { get; set; }
    public long CapacityBytes { get; set; }
    public int UsagePercentage { get; set; }
    public bool IsApproachingCapacity => UsagePercentage >= 80;
}
```

---

### 5. IMongoDBSyncService

Handles synchronization with MongoDB.

```csharp
public interface IMongoDBSyncService
{
    /// <summary>
    /// Tests connection to MongoDB with current configuration.
    /// </summary>
    Task<bool> TestConnectionAsync();

    /// <summary>
    /// Syncs selected records to MongoDB.
    /// </summary>
    /// <param name="recordIds">List of LocalIds to sync</param>
    /// <param name="deleteLocalAfterSync">Whether to delete local copies after successful sync</param>
    /// <param name="cancellationToken">For cancelling long operations</param>
    /// <returns>Sync result with success count and errors</returns>
    Task<SyncResult> SyncRecordsAsync(
        List<string> recordIds,
        bool deleteLocalAfterSync = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets sync progress for currently syncing records.
    /// </summary>
    SyncProgress GetSyncProgress();

    /// <summary>
    /// Retrieves records from MongoDB by filter criteria.
    /// </summary>
    Task<List<ExtractedRecord>> QueryRemoteRecordsAsync(RecordFilter filter);

    /// <summary>
    /// Pulls a record from MongoDB and saves locally.
    /// </summary>
    Task<ExtractedRecord> PullRecordAsync(string mongoDbId);

    /// <summary>
    /// Gets total count of records in MongoDB.
    /// </summary>
    Task<int> GetRemoteRecordCountAsync();

    /// <summary>
    /// Reports sync progress to UI.
    /// </summary>
    event EventHandler<SyncProgressEventArgs> ProgressChanged;

    /// <summary>
    /// Reports sync errors.
    /// </summary>
    event EventHandler<SyncErrorEventArgs> SyncError;
}

public class SyncResult
{
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<SyncError> Errors { get; set; } = new();
    public DateTime CompletedAt { get; set; }
    public TimeSpan Duration { get; set; }
}

public class SyncError
{
    public string RecordId { get; set; }
    public string ErrorMessage { get; set; }
    public string ErrorCode { get; set; }
}

public class SyncProgress
{
    public int TotalRecords { get; set; }
    public int CompletedRecords { get; set; }
    public int ProgressPercentage => TotalRecords > 0
        ? (CompletedRecords * 100) / TotalRecords
        : 0;
    public string CurrentRecordId { get; set; }
    public bool IsInProgress { get; set; }
}

public class SyncProgressEventArgs : EventArgs
{
    public SyncProgress Progress { get; set; }
}

public class SyncErrorEventArgs : EventArgs
{
    public SyncError Error { get; set; }
}
```

---

### 6. IConfigurationService

Manages application configuration (OLLAMA, MongoDB, UI settings).

```csharp
public interface IConfigurationService
{
    /// <summary>
    /// Gets the current application configuration.
    /// </summary>
    Task<ApplicationConfiguration> GetConfigurationAsync();

    /// <summary>
    /// Saves the application configuration.
    /// </summary>
    Task SaveConfigurationAsync(ApplicationConfiguration config);

    /// <summary>
    /// Gets OLLAMA-specific settings.
    /// </summary>
    Task<OLLAMASettings> GetOLLAMASettingsAsync();

    /// <summary>
    /// Updates OLLAMA settings and tests connection.
    /// </summary>
    /// <returns>Validation result with connection test outcome</returns>
    Task<ValidationResult> UpdateOLLAMASettingsAsync(OLLAMASettings settings);

    /// <summary>
    /// Gets MongoDB-specific settings.
    /// </summary>
    Task<MongoDBSettings> GetMongoDBSettingsAsync();

    /// <summary>
    /// Updates MongoDB settings and tests connection.
    /// </summary>
    Task<ValidationResult> UpdateMongoDBSettingsAsync(MongoDBSettings settings);

    /// <summary>
    /// Resets configuration to defaults.
    /// </summary>
    Task ResetConfigurationAsync();

    /// <summary>
    /// Gets application settings (UI preferences).
    /// </summary>
    Task<ApplicationSettings> GetApplicationSettingsAsync();

    /// <summary>
    /// Saves application settings.
    /// </summary>
    Task SaveApplicationSettingsAsync(ApplicationSettings settings);
}
```

---

## Service Dependency Injection

### Setup in App.xaml.cs

```csharp
public partial class App : Application
{
    private ServiceProvider _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();

        // Register services
        services.AddSingleton<IConfigurationService, ConfigurationService>();
        services.AddSingleton<ILocalStorageService, LocalStorageService>();
        services.AddScoped<IPdfProcessingService, PdfProcessingService>();
        services.AddScoped<IOLLAMAExtractionService, OLLAMAExtractionService>();
        services.AddScoped<IMetadataParsingService, MetadataParsingService>();
        services.AddScoped<IMongoDBSyncService, MongoDBSyncService>();

        // Register ViewModels
        services.AddScoped<MainWindowViewModel>();
        services.AddScoped<RecordBrowserViewModel>();
        services.AddScoped<SettingsViewModel>();

        // Register Windows
        services.AddScoped<MainWindow>();

        _serviceProvider = services.BuildServiceProvider();

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.DataContext = _serviceProvider.GetRequiredService<MainWindowViewModel>();
        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}
```

---

## Error Handling Contract

All services follow consistent error handling:

### Error Categories

| Category | Exception Type | User Message | Action |
|----------|----------------|-------------|--------|
| **Validation** | ArgumentException | Show validation error in UI | Retry after correction |
| **Not Found** | FileNotFoundException | "Record not found" | Return null or empty |
| **Connection** | HttpRequestException | "Cannot connect to [service]" | Show retry button |
| **Permission** | UnauthorizedAccessException | "Access denied to [resource]" | Show permission error |
| **Storage** | IOException | "Cannot access local storage" | Check disk space |
| **Unexpected** | Exception | "An unexpected error occurred" | Log and show support contact |

### Exception Propagation

- **UI Layer**: Catches all exceptions and displays user-friendly messages
- **Service Layer**: Throws specific exceptions; no UI error handling
- **Infrastructure**: Logs all exceptions; throws service layer exceptions

---

## Async/Await Contract

All I/O operations are async to prevent UI blocking:

```csharp
// ✅ CORRECT: Async methods prevent UI freeze
await pdfService.ExtractTextAsync(filePath);
await storageService.SaveRecordAsync(record);

// ❌ INCORRECT: Blocking calls freeze UI
pdfService.ExtractText(filePath);
storageService.SaveRecord(record);
```

Long-running operations provide progress reporting:

```csharp
// Subscribe to progress updates
pdfService.ProgressChanged += (s, e) =>
{
    progressBar.Value = e.ProgressPercentage;
};

// Perform operation with cancellation support
var cts = new CancellationTokenSource();
await pdfService.ExtractTextAsync(filePath, cts.Token);
```

