# Quickstart: Integration Scenarios

**Date**: 2025-12-02
**Feature**: Migrate to Native Windows Desktop Application
**Audience**: Development team starting implementation

---

## Overview

This document provides practical integration scenarios showing how the major components work together to deliver user value.

---

## Scenario 1: Upload PDF and Extract Metadata

**User Goal**: Upload a research paper PDF and extract ethnobotanical metadata.

**Flow**:

```
┌─────────────────────────────────────────────────────────┐
│ User clicks "Upload PDF"                                │
└────────────┬────────────────────────────────────────────┘
             │
             ▼
┌─────────────────────────────────────────────────────────┐
│ UI shows file picker dialog                             │
│ User selects PDF from Documents folder                  │
└────────────┬────────────────────────────────────────────┘
             │
             ▼
┌─────────────────────────────────────────────────────────┐
│ RecordBrowserViewModel calls:                           │
│ await pdfService.ValidateFileAsync(filePath)            │
└────────────┬────────────────────────────────────────────┘
             │
             ├─ Invalid? ─────→ Show error "Not a valid PDF"
             │
             └─ Valid?
                   │
                   ▼
         ┌────────────────────────────────────┐
         │ PDF validation passed              │
         │ Show progress indicator (0%)       │
         └────────┬─────────────────────────┘
                  │
                  ▼
         ┌──────────────────────────────────────────┐
         │ ViewModel calls:                         │
         │ text = await pdfService                  │
         │        .ExtractTextAsync(filePath)       │
         │                                          │
         │ Subscribe to ProgressChanged event:      │
         │ progressBar.Value = e.ProgressPercentage │
         └────────┬─────────────────────────────────┘
                  │
                  ├─ Extraction fails? ──→ Show error
                  │
                  └─ Text extracted?
                        │
                        ▼
              ┌───────────────────────────────────┐
              │ Show progress (50%)               │
              │ Submit text to OLLAMA             │
              └────────┬──────────────────────────┘
                       │
                       ▼
              ┌───────────────────────────────────────────┐
              │ ViewModel calls:                          │
              │ ollamaOutput = await ollamaService        │
              │     .ExtractMetadataAsync(                │
              │         text,                             │
              │         customPrompt,                      │
              │         cancellationToken)                │
              │                                           │
              │ Subscribe to ExtractionProgress event     │
              └────────┬────────────────────────────────┘
                       │
                       ├─ OLLAMA unavailable? ──┐
                       │                        │
                       │                        ▼
                       │               ┌──────────────────┐
                       │               │ Show error with  │
                       │               │ instructions to  │
                       │               │ start OLLAMA     │
                       │               └──────────────────┘
                       │
                       └─ Extraction succeeds?
                             │
                             ▼
                   ┌─────────────────────────────────┐
                   │ Show progress (75%)             │
                   │ Parse OLLAMA output             │
                   └────────┬──────────────────────┘
                            │
                            ▼
                   ┌───────────────────────────────────────────┐
                   │ ViewModel calls:                          │
                   │ parseResult = await                       │
                   │     parsingService.ParseMetadataAsync(    │
                   │         ollamaOutput)                     │
                   │                                           │
                   │ Get confidence score:                     │
                   │ confidence = parsingService               │
                   │     .GetConfidenceScore(record)           │
                   └────────┬──────────────────────────────────┘
                            │
                            ├─ Confidence < 50%? ──→ Show warning
                            │
                            └─ Record parsed?
                                  │
                                  ▼
                        ┌───────────────────────────────┐
                        │ Show progress (90%)           │
                        │ Save to local storage         │
                        └────────┬────────────────────┘
                                 │
                                 ▼
                        ┌──────────────────────────────────┐
                        │ ViewModel calls:                 │
                        │ record = await                   │
                        │     storageService               │
                        │     .SaveRecordAsync(record)     │
                        └────────┬─────────────────────────┘
                                 │
                                 ├─ Save fails? ──→ Show error
                                 │
                                 └─ Saved?
                                      │
                                      ▼
                              ┌──────────────────────┐
                              │ Show success dialog  │
                              │ "Record saved"       │
                              │ Confidence: 87%      │
                              │                      │
                              │ [Edit] [Sync] [OK]   │
                              └──────────────────────┘
```

**Implementation Example**:

```csharp
public class RecordBrowserViewModel : INotifyPropertyChanged
{
    private readonly IPdfProcessingService _pdfService;
    private readonly IOLLAMAExtractionService _ollamaService;
    private readonly IMetadataParsingService _parsingService;
    private readonly ILocalStorageService _storageService;

    public async Task HandleUploadPdfClick()
    {
        var filePath = ShowFilePickerDialog();
        if (filePath == null) return;

        try
        {
            // Step 1: Validate PDF
            var validation = await _pdfService.ValidateFileAsync(filePath);
            if (!validation.IsValid)
            {
                ShowError($"Invalid PDF: {validation.ErrorMessage}");
                return;
            }

            // Step 2: Extract text
            var text = await _pdfService.ExtractTextAsync(filePath);
            if (string.IsNullOrEmpty(text))
            {
                ShowError("No readable text found in PDF (scanned image?)");
                return;
            }

            // Step 3: Extract metadata from OLLAMA
            var ollamaOutput = await _ollamaService.ExtractMetadataAsync(text);

            // Step 4: Parse extracted metadata
            var parseResult = await _parsingService.ParseMetadataAsync(ollamaOutput);
            var record = parseResult.Record;

            // Step 5: Save to local storage
            var savedRecord = await _storageService.SaveRecordAsync(record);

            // Step 6: Show success
            ShowSuccess($"Record saved with {parseResult.ConfidenceScore}% confidence");
        }
        catch (Exception ex)
        {
            ShowError($"Error: {ex.Message}");
        }
    }
}
```

---

## Scenario 2: Edit Extracted Record

**User Goal**: Review extracted metadata and correct any errors.

**Flow**:

```
┌────────────────────────────────────────┐
│ User clicks on record in list view      │
└────────────┬─────────────────────────────┘
             │
             ▼
┌──────────────────────────────────────────────┐
│ RecordBrowserViewModel calls:               │
│ record = await storageService               │
│     .LoadRecordAsync(localId)               │
└────────────┬───────────────────────────────┘
             │
             ▼
┌──────────────────────────────────────────────┐
│ Opens RecordEditorView with loaded record    │
│ Displays all mandatory fields in editable    │
│ TextBox and DataGrid controls                │
└────────────┬───────────────────────────────┘
             │
             ▼
┌──────────────────────────────────────────────┐
│ User edits fields:                           │
│ - Corrects "maçanilha" typo → "maçanilha"  │
│ - Adds missing author name                   │
│ - Adds second species                        │
│ - Updates community name                     │
└────────────┬───────────────────────────────┘
             │
             ▼
┌──────────────────────────────────────────────┐
│ User clicks "Save"                           │
└────────────┬───────────────────────────────┘
             │
             ▼
┌──────────────────────────────────────────────┐
│ ViewModel calls:                             │
│ validation = parsingService                  │
│     .ValidateRecord(editedRecord)            │
└────────────┬───────────────────────────────┘
             │
             ├─ Validation fails? ──→ Show errors in red boxes
             │
             └─ Valid?
                   │
                   ▼
         ┌──────────────────────────────────────┐
         │ ViewModel calls:                     │
         │ await storageService                 │
         │     .UpdateRecordAsync(record)       │
         └────────┬─────────────────────────────┘
                  │
                  ├─ Update fails? ──→ Show error
                  │
                  └─ Success?
                        │
                        ▼
              ┌──────────────────────────────┐
              │ Show success notification    │
              │ "Record updated"             │
              │                              │
              │ Confidence: 92%              │
              └──────────────────────────────┘
```

**Implementation Example**:

```csharp
public class RecordEditorViewModel : INotifyPropertyChanged
{
    private ExtractedRecord _record;
    private readonly IMetadataParsingService _parsingService;
    private readonly ILocalStorageService _storageService;

    public async Task HandleSaveClick()
    {
        try
        {
            // Validate before saving
            var validation = _parsingService.ValidateRecord(_record);
            if (!validation.IsValid)
            {
                ShowValidationErrors(validation.ErrorMessage);
                return;
            }

            // Update record
            await _storageService.UpdateRecordAsync(_record);

            // Recalculate confidence
            var confidence = _parsingService.GetConfidenceScore(_record);

            ShowSuccess($"Record saved. Confidence: {confidence}%");
        }
        catch (Exception ex)
        {
            ShowError($"Save failed: {ex.Message}");
        }
    }
}
```

---

## Scenario 3: Configure OLLAMA Connection

**User Goal**: Provide OLLAMA server details and test the connection.

**Flow**:

```
┌────────────────────────────────────────┐
│ User opens Settings window              │
└────────────┬──────────────────────────┘
             │
             ▼
┌────────────────────────────────────────┐
│ SettingsViewModel loads current config: │
│ config = await configService            │
│     .GetOLLAMASettingsAsync()            │
│                                         │
│ UI shows:                               │
│ Host: localhost                         │
│ Port: 11434                             │
│ Model: mistral                          │
└────────────┬──────────────────────────┘
             │
             ▼
┌────────────────────────────────────────┐
│ User modifies settings:                 │
│ Host: 192.168.1.100                    │
│ Port: 8080                             │
└────────────┬──────────────────────────┘
             │
             ▼
┌────────────────────────────────────────┐
│ User clicks "Test Connection"           │
└────────────┬──────────────────────────┘
             │
             ▼
┌────────────────────────────────────────────┐
│ ViewModel calls:                           │
│ validation = await configService           │
│     .UpdateOLLAMASettingsAsync(             │
│         newSettings)                        │
│                                            │
│ This internally calls:                     │
│ ollamaService.TestConnectionAsync()        │
└────────────┬──────────────────────────────┘
             │
             ├─ Connection fails? ──→ Show error:
             │                       "Cannot connect to
             │                        192.168.1.100:8080"
             │
             └─ Connection succeeds?
                   │
                   ▼
         ┌──────────────────────────────────┐
         │ Show success dialog:             │
         │ "OLLAMA connection successful"   │
         │ "Running: Mistral v2.0"          │
         │ "Models: mistral, llama2, ..."   │
         │                                  │
         │ [Save Settings]                  │
         └──────────────────────────────────┘
```

**Implementation Example**:

```csharp
public class SettingsViewModel : INotifyPropertyChanged
{
    private readonly IConfigurationService _configService;

    public async Task HandleTestOLLAMAConnection()
    {
        try
        {
            var newSettings = new OLLAMASettings
            {
                Host = OLLAMAHost,
                Port = OLLAMAPort,
                Model = OLLAMAModel
            };

            var validation = await _configService
                .UpdateOLLAMASettingsAsync(newSettings);

            if (!validation.IsValid)
            {
                ShowError($"Connection failed: {validation.ErrorMessage}");
                return;
            }

            ShowSuccess("OLLAMA connection successful!");
        }
        catch (Exception ex)
        {
            ShowError($"Error: {ex.Message}");
        }
    }
}
```

---

## Scenario 4: Synchronize Records to MongoDB

**User Goal**: Backup extracted records to MongoDB and remove local copies.

**Flow**:

```
┌───────────────────────────────────────┐
│ User selects records in list:          │
│ □ Paper 1 (2020, 5 species)            │
│ ☑ Paper 2 (2021, 8 species)            │
│ ☑ Paper 3 (2022, 3 species)            │
│ □ Paper 4 (2023, 6 species)            │
└────────────┬─────────────────────────┘
             │
             ▼
┌───────────────────────────────────────┐
│ User clicks "Sync Selected"             │
└────────────┬─────────────────────────┘
             │
             ▼
┌────────────────────────────────────────┐
│ ViewModel calls:                        │
│ selectedIds = GetSelectedRecordIds()    │
│                                        │
│ result = await syncService             │
│     .SyncRecordsAsync(                  │
│         selectedIds,                    │
│         deleteLocalAfterSync: true)     │
└────────────┬─────────────────────────┘
             │
             ▼
┌────────────────────────────────────────┐
│ Show sync progress window:              │
│                                        │
│ Syncing records: 1/2                   │
│ Current: Paper 2...                    │
│ [████████░░░░░░░░░░] 50%               │
│                                        │
│ Status: Uploading to MongoDB            │
│ Speed: 500 KB/s                        │
│                                        │
│ [Cancel]                               │
└────────────┬─────────────────────────┘
             │
             ├─ Subscribe to progress event:
             │   syncService.ProgressChanged += (s, e) =>
             │   {
             │       progressBar.Value = e.Progress
             │           .ProgressPercentage;
             │   }
             │
             └─ Sync completes?
                   │
                   ▼
         ┌──────────────────────────────────┐
         │ Show result dialog:              │
         │ "Sync completed successfully"    │
         │                                  │
         │ Synced: 2 records                │
         │ Failed: 0 records                │
         │ Duration: 1m 23s                 │
         │                                  │
         │ [View Results] [OK]              │
         │                                  │
         │ Local copies deleted.            │
         │ Records backed up to MongoDB.    │
         └──────────────────────────────────┘
```

**Implementation Example**:

```csharp
public class RecordBrowserViewModel : INotifyPropertyChanged
{
    private readonly IMongoDBSyncService _syncService;

    public async Task HandleSyncSelectedRecords()
    {
        try
        {
            var selectedIds = GetSelectedRecordIds();
            if (!selectedIds.Any())
            {
                ShowError("Select records to sync");
                return;
            }

            var syncDialog = new SyncProgressWindow();
            syncDialog.Show();

            // Subscribe to progress updates
            _syncService.ProgressChanged += (s, e) =>
            {
                syncDialog.UpdateProgress(e.Progress);
            };

            _syncService.SyncError += (s, e) =>
            {
                syncDialog.AddErrorMessage(e.Error.ErrorMessage);
            };

            // Execute sync
            var result = await _syncService.SyncRecordsAsync(
                selectedIds,
                deleteLocalAfterSync: true);

            syncDialog.ShowResult(result);
        }
        catch (Exception ex)
        {
            ShowError($"Sync failed: {ex.Message}");
        }
    }
}
```

---

## Scenario 5: Check Storage Status

**User Goal**: Monitor local storage capacity and receive warnings when approaching limit.

**Flow**:

```
┌───────────────────────────────────────────────┐
│ Application startup                            │
│ (OR: Periodically in background)              │
└────────────┬──────────────────────────────────┘
             │
             ▼
┌───────────────────────────────────────────────┐
│ ViewModel calls:                               │
│ storage = await storageService                 │
│     .GetStorageStatusAsync()                    │
│                                               │
│ Display in status bar:                        │
│ "Records: 862/1000 | 86% capacity"             │
└────────────┬──────────────────────────────────┘
             │
             ├─ usage >= 80%?
             │  │
             │  └─ Show warning notification:
             │     "Storage approaching capacity!
             │      Sync records to MongoDB
             │      to free space"
             │
             └─ usage >= 95%?
                │
                └─ Block new uploads:
                   Show error: "Storage full.
                   Sync records before uploading."
```

**Implementation Example**:

```csharp
public class MainWindowViewModel : INotifyPropertyChanged
{
    private readonly ILocalStorageService _storageService;

    public async void CheckStorageStatus()
    {
        var status = await _storageService.GetStorageStatusAsync();

        // Update UI binding
        StorageUsagePercent = status.UsagePercentage;
        RecordCount = status.RecordCount;

        // Show warnings
        if (status.IsApproachingCapacity && status.UsagePercentage < 95)
        {
            ShowWarning($"Storage at {status.UsagePercentage}%. " +
                       "Consider syncing records to MongoDB.");
        }
        else if (status.UsagePercentage >= 95)
        {
            IsUploadEnabled = false;
            ShowError("Storage full. Sync records to continue.");
        }
    }
}
```

---

## Architecture Pattern Summary

### MVVM Architecture

All scenarios follow **Model-View-ViewModel** pattern:

```
View (XAML)
    │
    ├─ Binds to ─→ ViewModel
    │
    └─ User actions
         │
         ▼
    ViewModel
    (RecordBrowserViewModel,
     RecordEditorViewModel,
     SettingsViewModel)
         │
         ├─ Calls ─→ Services
         │          (IPdfProcessingService,
         │           IOLLAMAExtractionService,
         │           ILocalStorageService,
         │           IConfigurationService,
         │           IMongoDBSyncService)
         │
         ├─ Updates ─→ Models
         │            (ExtractedRecord,
         │             ApplicationConfiguration)
         │
         └─ Updates UI ─→ View
            via INotifyPropertyChanged
```

### Key Benefits

- **Testable**: Services can be mocked; ViewModels tested without UI
- **Responsive**: All I/O is async; UI thread never blocks
- **Maintainable**: Clear separation of concerns
- **Reusable**: Same services used by different ViewModels

---

## Error Handling Pattern

All scenarios follow consistent error handling:

```csharp
try
{
    // Perform operation
    var result = await service.DoSomethingAsync();
}
catch (ArgumentException ex)
{
    // Validation error
    ShowValidationError(ex.Message);
}
catch (FileNotFoundException ex)
{
    // Not found error
    ShowError($"File not found: {ex.Message}");
}
catch (HttpRequestException ex)
{
    // Connection error
    ShowError($"Cannot connect: {ex.Message}. Check your internet connection.");
}
catch (Exception ex)
{
    // Unexpected error
    logger.LogError(ex, "Unexpected error");
    ShowError("An unexpected error occurred. Please contact support.");
}
```

---

## Getting Started with Implementation

### Phase 1: Setup Foundation (Week 1)

1. Create solution structure (EtnoPapers.Core, EtnoPapers.Desktop projects)
2. Implement data models (ExtractedRecord, Especie, etc.)
3. Create service interfaces (contracts/services.md)
4. Setup dependency injection in App.xaml.cs

### Phase 2: Core Services (Weeks 2-3)

1. Implement IPdfProcessingService (PdfPig integration)
2. Implement IOLLAMAExtractionService (OllamaSharp integration)
3. Implement IMetadataParsingService (heuristic extraction)
4. Implement ILocalStorageService (JSON file I/O)

### Phase 3: UI & Integration (Week 3-4)

1. Build MainWindow with navigation
2. Implement RecordBrowserViewModel & views
3. Implement RecordEditorViewModel & views
4. Implement SettingsViewModel & views
5. Wire up services to ViewModels

### Phase 4: MongoDB & Polish (Week 4-5)

1. Implement IMongoDBSyncService
2. Implement IConfigurationService
3. Add error dialogs and user feedback
4. Comprehensive testing
5. WiX installer creation

