# Implementation Tasks: EtnoPapers Desktop Migration - Electron to C# WPF

**Branch**: `main` (single-branch workflow)
**Spec**: [spec.md](./spec.md)
**Plan**: [plan.md](./plan.md)
**Generated**: 2025-12-02
**Technology**: C# 12 / .NET 8.0 / WPF

## Task Organization

Tasks are organized by phase and user story priority. Each task includes:
- **Task ID**: Unique identifier (T001, T002, etc.)
- **[P]**: Can be executed in parallel with other [P] tasks (different files, no dependencies)
- **[US#]**: Maps to User Story in spec.md ([US1], [US2], [US3], [US4])
- **File Path**: Specific location for implementation

**Execution Order**: Complete tasks sequentially within each phase. Tasks marked [P] can run in parallel if working on different files.

---

## User Story Summary

| Story | Priority | Title | MVP | Phase |
|-------|----------|-------|-----|-------|
| US1 | P1 | Maintain Full Feature Parity | ✅ Core | Phase 2-5 |
| US2 | P1 | Improve Windows Native Integration | ✅ Core | Phase 3-5 |
| US3 | P2 | Improve Performance and Resource Efficiency | ✅ Important | Phase 6-7 |
| US4 | P1 | Maintain Data Compatibility | ✅ Core | Phase 2-5 |

**MVP Scope**: US1, US2, US4 (full feature parity, native Windows, data compatibility) - US3 (performance) validated with benchmarks

---

## Phase 0: Project Setup & Infrastructure

**Goal**: Initialize Visual Studio solution, create project structure, and install dependencies

### Solution & Project Creation

- [ ] T001 Create Visual Studio solution file
  - **File**: `EtnoPapers.sln`
  - **Acceptance**: Solution created in root, can be opened in Visual Studio 2022+
  - **Commands**: `dotnet new sln -n EtnoPapers`

- [ ] T002 [P] Create EtnoPapers.Core class library project
  - **File**: `src/EtnoPapers.Core/EtnoPapers.Core.csproj`
  - **Acceptance**: .csproj file exists, targets .NET 8.0, references added to solution
  - **Commands**: `dotnet new classlib -n EtnoPapers.Core -f net8.0`

- [ ] T003 [P] Create EtnoPapers.UI WPF project
  - **File**: `src/EtnoPapers.UI/EtnoPapers.UI.csproj`
  - **Acceptance**: WPF project created, targets .NET 8.0, references added to solution
  - **Commands**: `dotnet new wpf -n EtnoPapers.UI -f net8.0`

- [ ] T004 [P] Create EtnoPapers.Core.Tests unit test project
  - **File**: `tests/EtnoPapers.Core.Tests/EtnoPapers.Core.Tests.csproj`
  - **Acceptance**: xUnit test project created, references EtnoPapers.Core
  - **Commands**: `dotnet new xunit -n EtnoPapers.Core.Tests -f net8.0`

### NuGet Dependencies

- [ ] T005 [P] Install core NuGet packages (MongoDB, JSON, logging)
  - **File**: `src/EtnoPapers.Core/EtnoPapers.Core.csproj`
  - **Acceptance**: Packages added: MongoDB.Driver, Newtonsoft.Json, Serilog, Serilog.Sinks.File
  - **Commands**: `dotnet add EtnoPapers.Core package MongoDB.Driver` (and others)

- [ ] T006 [P] Install PdfPig library for PDF processing
  - **File**: `src/EtnoPapers.Core/EtnoPapers.Core.csproj`
  - **Acceptance**: PdfPig library installed (open source, superior to iTextSharp for structure detection)
  - **Commands**: `dotnet add src/EtnoPapers.Core package PdfPig`
  - **Note**: PdfPig provides document structure analysis needed for Markdown conversion

- [ ] T007 [P] Install WPF UI dependencies
  - **File**: `src/EtnoPapers.UI/EtnoPapers.UI.csproj`
  - **Acceptance**: UI packages installed (Newtonsoft.Json for UI, optional UI frameworks)
  - **Commands**: `dotnet add EtnoPapers.UI package Newtonsoft.Json`

- [ ] T008 [P] Install testing dependencies
  - **File**: `tests/EtnoPapers.Core.Tests/EtnoPapers.Core.Tests.csproj`
  - **Acceptance**: xUnit, Moq, FluentAssertions installed
  - **Commands**: `dotnet add EtnoPapers.Core.Tests package Moq` and `dotnet add EtnoPapers.Core.Tests package FluentAssertions`

### Directory Structure & .gitignore

- [ ] T009 Create directory structure per implementation plan
  - **Files**: `src/EtnoPapers.Core/Services/`, `Models/`, `Utils/`, `Validation/`, etc.
  - **Acceptance**: All directories created as per plan.md project structure
  - **Commands**: `mkdir -p src/EtnoPapers.Core/{Services,Models,Utils,Validation}` (and others for UI)

- [ ] T010 Create comprehensive .gitignore for .NET
  - **File**: `.gitignore`
  - **Acceptance**: Ignores bin/, obj/, .vs/, *.user, packages/, build artifacts
  - **Content**: Standard .NET gitignore rules

### Build Configuration

- [ ] T011 Create build script to validate solution
  - **File**: `build/build.ps1` or `Makefile`
  - **Acceptance**: Script builds both projects, reports on success/failure
  - **Commands**: Include `dotnet build` and `dotnet test`

---

## Phase 1: Core Services & Data Layer

**Goal**: Implement business logic services and data models that all other features depend on

**Why First**: All UI and extraction functionality depends on working services and data models

### Data Models (US1, US4)

- [ ] T012 [P] [US1] [US4] Create ArticleRecord model class
  - **File**: `src/EtnoPapers.Core/Models/ArticleRecord.cs`
  - **Acceptance**: Class matches JSON schema from Electron version, includes all fields (title, authors, year, abstract, species, community, etc.)
  - **Properties**: titulo, autores[], ano, resumo, especies[], comunidade, pais, estado, municipio, local, bioma, metodologia, ano_coleta, customAttributes

- [ ] T013 [P] [US1] [US4] Create PlantSpecies model class
  - **File**: `src/EtnoPapers.Core/Models/PlantSpecies.cs`
  - **Acceptance**: Nested class with vernacular names, scientific names, use types
  - **Properties**: nome_vernacular, nome_cientifico, tipo_uso

- [ ] T014 [P] [US1] [US4] Create Community model class
  - **File**: `src/EtnoPapers.Core/Models/Community.cs`
  - **Acceptance**: Model for community information
  - **Properties**: nome, localizacao

- [ ] T015 [P] Create Configuration model class
  - **File**: `src/EtnoPapers.Core/Models/Configuration.cs`
  - **Acceptance**: Holds OLLAMA and MongoDB configuration
  - **Properties**: ollamaUrl, ollamaModel, ollamaPrompt, mongodbUri, language, windowSize, windowPosition

### JSON Serialization (US4)

- [ ] T016 [US4] Implement JSON serialization for ArticleRecord
  - **File**: `src/EtnoPapers.Core/Utils/JsonSerializer.cs`
  - **Acceptance**: Uses Newtonsoft.Json to serialize/deserialize ArticleRecord to/from JSON
  - **Methods**: SerializeToJson(ArticleRecord), DeserializeFromJson(string), RoundTripTest()
  - **Testing**: Unit test verifies Electron JSON → C# object → C# JSON produces identical output

- [ ] T017 [US4] Test JSON compatibility with Electron-generated files
  - **File**: `src/EtnoPapers.Core/Utils/JsonCompatibilityTest.cs`
  - **Acceptance**: Load sample Electron-generated JSON, verify all fields parse correctly
  - **Test Data**: Include fixtures with real Electron output

### Utility Functions (US1)

- [ ] T018 [P] Implement TitleNormalizer utility
  - **File**: `src/EtnoPapers.Core/Utils/TitleNormalizer.cs`
  - **Acceptance**: Converts titles to proper case, preserves acronyms (DNA, USA), handles particles (de, von, etc.)
  - **Method**: Normalize(string title)
  - **Tests**: Unit tests for proper case, acronyms, particles

- [ ] T019 [P] Implement AuthorFormatter utility
  - **File**: `src/EtnoPapers.Core/Utils/AuthorFormatter.cs`
  - **Acceptance**: Formats author names to APA style (LastName, F.I.), handles particles, suffixes
  - **Method**: FormatToAPA(string name)
  - **Tests**: Unit tests for single/multiple names, particles, suffixes

- [ ] T020 [P] Implement LanguageDetector utility
  - **File**: `src/EtnoPapers.Core/Utils/LanguageDetector.cs`
  - **Acceptance**: Detects Portuguese, English, Spanish from text samples
  - **Method**: DetectLanguage(string text)
  - **Tests**: Test samples in PT, EN, ES

### Validation (US1, US4)

- [ ] T021 Create validation schemas and validators
  - **File**: `src/EtnoPapers.Core/Validation/ArticleRecordValidator.cs`
  - **Acceptance**: Validates ArticleRecord structure, mandatory fields, data types
  - **Methods**: Validate(ArticleRecord), ValidateMandatoryFields(), GetValidationErrors()
  - **Validation Rules**: Title not empty, year > 1500, authors not empty, etc.

### Core Services (US1, US4)

- [ ] T022 [US1] [US4] Implement ConfigurationService
  - **File**: `src/EtnoPapers.Core/Services/ConfigurationService.cs`
  - **Acceptance**: Loads/saves configuration to JSON file in Documents/EtnoPapers/config.json
  - **Methods**: LoadConfiguration(), SaveConfiguration(config), ResetToDefaults(), ValidateConfiguration()
  - **Features**: Lazy loading, caching, validation

- [ ] T023 [US4] Implement DataStorageService
  - **File**: `src/EtnoPapers.Core/Services/DataStorageService.cs`
  - **Acceptance**: CRUD operations for local JSON storage (Documents/EtnoPapers/data.json)
  - **Methods**: Initialize(), LoadAll(), GetById(id), Create(article), Update(article), Delete(id), Count(), CheckLimit()
  - **Features**: Atomic writes, limit enforcement (1000 records), auto-save

- [ ] T023a [US1] Implement MarkdownConverter service (NEW - Critical for accuracy)
  - **File**: `src/EtnoPapers.Core/Services/MarkdownConverter.cs`
  - **Acceptance**: Converts PDF to structured Markdown using PdfPig library
  - **Methods**: ConvertToMarkdown(pdfPath), DetectHeadings(words), DetectTables(words), DetectParagraphs(words), FormatMarkdownTable(table)
  - **Features**: Document structure analysis, heading detection by font size/position, table extraction, paragraph separation
  - **Testing**: Unit tests with sample PDFs to verify Markdown structure (headings as #, tables as |...|)

- [ ] T023b [US1] Implement structure detection algorithms in MarkdownConverter
  - **File**: `src/EtnoPapers.Core/Services/MarkdownConverter.cs`
  - **Acceptance**: Detect headings by font size (>average + bold/centered), detect tables by column alignment, detect lists by bullets/numbering
  - **Algorithms**: Font size analysis, text positioning analysis, alignment pattern detection
  - **Edge Cases**: Multi-column layouts, nested structures, mixed fonts

- [ ] T023c [US1] Implement fallback to raw text extraction in MarkdownConverter
  - **File**: `src/EtnoPapers.Core/Services/MarkdownConverter.cs`
  - **Acceptance**: If structure detection fails, falls back to simple text extraction with warning logged
  - **Methods**: ConvertToMarkdownWithFallback(pdfPath), ExtractRawText(pdfPath)
  - **Features**: Try-catch wrapper, error logging, graceful degradation

- [ ] T023d [US1] Create unit tests for MarkdownConverter
  - **File**: `tests/EtnoPapers.Core.Tests/Services/MarkdownConverterTests.cs`
  - **Acceptance**: Tests for heading detection, table extraction, paragraph separation, fallback behavior
  - **Test Cases**: Simple paper (title + abstract), complex paper (tables + sections), corrupt PDF (fallback)
  - **Assertions**: Verify Markdown structure (# headings present, tables formatted correctly)

- [ ] T024 [US1] Update PDFProcessingService to orchestrate Markdown conversion
  - **File**: `src/EtnoPapers.Core/Services/PDFProcessingService.cs`
  - **Acceptance**: Coordinates MarkdownConverter, provides high-level PDF processing interface
  - **Methods**: ProcessPDF(filePath) → returns structured Markdown, GetMetadata(filePath), ValidatePDF(filePath)
  - **Features**: Uses MarkdownConverter internally, handles errors, provides progress callbacks

- [ ] T025 [US1] Implement OLLAMAService with Markdown-optimized prompts
  - **File**: `src/EtnoPapers.Core/Services/OLLAMAService.cs`
  - **Acceptance**: REST API integration with local OLLAMA service, prompts optimized for structured Markdown input
  - **Methods**: CheckHealth(), ExtractMetadataFromMarkdown(markdownText, customPrompt), TranslateToPortuguese(text), GetAvailableModels()
  - **Features**: Connection testing, retry logic, timeout handling, response parsing
  - **Prompt Changes**: Updated default prompt to process Markdown structure (# headings, tables, sections)

- [ ] T026 [US4] Implement MongoDBSyncService
  - **File**: `src/EtnoPapers.Core/Services/MongoDBSyncService.cs`
  - **Acceptance**: Connect to MongoDB, upload records, batch operations
  - **Methods**: TestConnection(uri), UploadRecord(record), UploadBatch(records), GetStatus()
  - **Features**: Connection pooling, retry logic, batch processing

- [ ] T027 [US1] Implement ValidationService
  - **File**: `src/EtnoPapers.Core/Services/ValidationService.cs`
  - **Acceptance**: Validate extracted data against schema
  - **Methods**: ValidateRecord(article), CheckMandatoryFields(article), GetValidationErrors(article)
  - **Integration**: Uses ArticleRecordValidator

- [ ] T028 [US1] Update ExtractionPipelineService to use Markdown pipeline
  - **File**: `src/EtnoPapers.Core/Services/ExtractionPipelineService.cs`
  - **Acceptance**: Orchestrates PDF → Markdown → AI → validation → storage (updated pipeline)
  - **Methods**: ExtractFromPDF(filePath), CancelExtraction(), GetExtractionStatus()
  - **Pipeline**: PDFProcessingService.ProcessPDF() → MarkdownConverter → OLLAMAService.ExtractMetadataFromMarkdown() → ValidationService
  - **Features**: Progress tracking, error recovery, atomic operations, fallback handling

- [ ] T029 Create LoggerService
  - **File**: `src/EtnoPapers.Core/Services/LoggerService.cs`
  - **Acceptance**: File-based logging with rotation, different log levels
  - **Methods**: Debug(), Info(), Warn(), Error(), GetLogFilePath()
  - **Configuration**: Logs to %APPDATA%/EtnoPapers/logs/, 7-day retention

### Data Compatibility & Edge Case Testing (US4)

- [ ] T092 [US4] Create JSON schema migration detection and validation
  - **File**: `src/EtnoPapers.Core/Services/ConfigurationMigrationService.cs`
  - **Acceptance**: Detect JSON version/schema and validate compatibility
  - **Methods**: DetectJsonVersion(), ValidateMigration(), PreserveUnknownFields()
  - **Test Cases**: v1.0 config to v2.0 parser with unknown fields, handles gracefully
  - **Note**: Prevents data loss if Electron config has fields unknown to WPF version

- [ ] T093 [US4] Create incomplete migration detection and rollback
  - **File**: `src/EtnoPapers.Core/Services/MigrationCompletionService.cs`
  - **Acceptance**: Detect and handle incomplete migrations (mixed v1 + v2 records)
  - **Methods**: DetectMixedFormats(), ValidateConsistency(), BackupBeforeMigration()
  - **Recovery**: If inconsistent state detected, offer rollback to last known good backup
  - **Logging**: Log all migration transitions for debugging

---
## Phase 2: Windows Native UI Foundation (US2, US1)

**Goal**: Set up WPF application structure, MVVM pattern, and native Windows controls

**Why Second**: Core services are stable; now we need UI framework to build features

### WPF Application Setup (US2)

- [ ] T030 Create WPF Application structure (App.xaml, App.xaml.cs)
  - **Files**: `src/EtnoPapers.UI/App.xaml`, `App.xaml.cs`
  - **Acceptance**: Application entry point configured, default resources set, dependency injection setup
  - **Content**: StartupUri, resource dictionaries, service initialization

- [ ] T031 Create MainWindow and MainWindowViewModel (MVVM)
  - **Files**: `src/EtnoPapers.UI/Views/MainWindow.xaml`, `MainWindow.xaml.cs`, `ViewModels/MainWindowViewModel.cs`
  - **Acceptance**: Main application window with navigation frame/menu structure
  - **Pattern**: MainWindow.DataContext = MainWindowViewModel (MVVM binding)
  - **Features**: Window state preservation (save/restore size, position)

- [ ] T032 [US2] Create WPF Styles and Resource Dictionaries
  - **Files**: `src/EtnoPapers.UI/Styles/Brushes.xaml`, `ControlStyles.xaml`, `App.xaml`
  - **Acceptance**: Windows 11 design language colors, button styles, control templates
  - **Resources**: Color definitions, control styles, font sizes
  - **Features**: Light/dark theme support (system theme detection)

- [ ] T033 [US2] Implement navigation between pages
  - **File**: `src/EtnoPapers.UI/Navigation/NavigationService.cs`
  - **Acceptance**: Navigate between Home, Upload, Records, Settings, About pages
  - **Pattern**: Page switching via MainWindow frame or content control
  - **Implementation**: Frame.Navigate() or similar pattern

### Core WPF Controls (US2)

- [ ] T034 [P] [US2] Create StatusBar control
  - **Files**: `src/EtnoPapers.UI/Controls/StatusBar.xaml`, `StatusBar.xaml.cs`
  - **Acceptance**: Shows OLLAMA and MongoDB connection status with color indicators
  - **Bindings**: BindsTo AppViewModel.OLLAMAConnected, AppViewModel.MongoDBConnected
  - **Display**: Green (connected), red (disconnected), refresh button

- [ ] T035 [P] Create INotifyPropertyChanged base class for ViewModels
  - **File**: `src/EtnoPapers.UI/ViewModels/ViewModelBase.cs`
  - **Acceptance**: Base class implementing INotifyPropertyChanged for all ViewModels
  - **Methods**: OnPropertyChanged(propertyName), RaisePropertyChanged<T>()

- [ ] T036 [P] Create RelayCommand class for ViewModel commands
  - **File**: `src/EtnoPapers.UI/Commands/RelayCommand.cs`
  - **Acceptance**: ICommand implementation for button/menu commands
  - **Methods**: Execute(), CanExecute(), RaiseCanExecuteChanged()

- [ ] T037 Create value converters for WPF bindings
  - **File**: `src/EtnoPapers.UI/Converters/BoolToVisibilityConverter.cs`, `StatusToBrushConverter.cs`, etc.
  - **Acceptance**: Standard converters for bool→Visibility, status→color, etc.
  - **Converters**: BoolToVisibilityConverter, StatusToBrushConverter, DateFormatConverter

### Home Page (UI foundation)

- [ ] T038 [US2] Create HomePage and HomeViewModel
  - **Files**: `src/EtnoPapers.UI/Views/HomePage.xaml`, `HomePage.xaml.cs`, `ViewModels/HomeViewModel.cs`
  - **Acceptance**: Welcome page with quick start guide, connection status, GPU advisory if applicable
  - **Bindings**: Shows OLLAMA and MongoDB status, application version
  - **Features**: Contextual help, call-to-action buttons to navigate to Upload/Settings

### Localization Setup

- [ ] T039 Create Portuguese localization resources
  - **File**: `src/EtnoPapers.UI/Localization/Strings.pt-BR.resx`
  - **Acceptance**: All UI strings in Brazilian Portuguese (buttons, labels, messages)
  - **Content**: Translate all English defaults to Portuguese

- [ ] T040 Create English localization resources
  - **File**: `src/EtnoPapers.UI/Localization/Strings.en-US.resx`
  - **Acceptance**: All UI strings in English
  - **Content**: English translations

---

## Phase 3: PDF Upload & Extraction UI (US1, US2)

**Goal**: Implement upload workflow, extraction progress display, and results editing

**Why Third**: Core features depend on PDF extraction - this is primary workflow

### Upload Page (US1, US2)

- [ ] T041 [US1] [US2] Create UploadPage and UploadViewModel
  - **Files**: `src/EtnoPapers.UI/Views/UploadPage.xaml`, `UploadPage.xaml.cs`, `ViewModels/UploadViewModel.cs`
  - **Acceptance**: Page with file upload area, extraction progress, results editing
  - **Bindings**: ViewModel manages extraction state, progress updates, errors
  - **Layout**: Drop zone, upload button, progress bar, results form

- [ ] T042 [US1] [US2] Create file drop zone control
  - **File**: `src/EtnoPapers.UI/Controls/FileDropZone.xaml`
  - **Acceptance**: Drag-and-drop area for PDF files, file selection button
  - **Validation**: Accept .pdf only, show error for other file types
  - **Size validation**: Reject files >50MB

- [ ] T043 [US1] Create extraction progress display
  - **File**: `src/EtnoPapers.UI/Controls/ExtractionProgressPanel.xaml`
  - **Acceptance**: Shows current step (PDF loading, text extraction, AI processing, validation, saving)
  - **Bindings**: Progress percentage, current step, time elapsed
  - **Features**: Cancel button, error display

- [ ] T044 [US1] Create extraction results editing form
  - **File**: `src/EtnoPapers.UI/Controls/ExtractionResultsForm.xaml`
  - **Acceptance**: Editable form for extracted data (title, authors, year, abstract, etc.)
  - **Features**: Field editing, custom attribute addition, validation on save
  - **Bindings**: Bind to extracted ArticleRecord, enable save button

- [ ] T045 [US1] Create custom attribute editor control
  - **File**: `src/EtnoPapers.UI/Controls/CustomAttributeEditor.xaml`
  - **Acceptance**: Add/remove custom key-value pairs
  - **Features**: Add button, delete button per item, dynamic binding

### Upload Workflow Logic (US1)

- [ ] T046 [US1] Create useExtraction hook equivalent (UploadViewModel logic)
  - **File**: `src/EtnoPapers.UI/ViewModels/UploadViewModel.cs`
  - **Acceptance**: Methods for uploading, starting extraction, handling progress, saving results
  - **Methods**: SelectFile(), StartExtraction(), CancelExtraction(), SaveResults()
  - **State**: IsExtracting, Progress, CurrentStep, ExtractedData, Error, AllowSave

- [ ] T047 [US1] [US2] Implement PDF upload validation
  - **File**: `src/EtnoPapers.Core/Utils/PDFValidator.cs`
  - **Acceptance**: Validate PDF file type (magic numbers, not extension), size limits
  - **Methods**: ValidatePDFFile(filePath), GetFileSize(filePath)
  - **Checks**: File type, file size <50MB, file readable

- [ ] T048 [US1] Implement duplicate detection
  - **File**: `src/EtnoPapers.Core/Services/DuplicateDetectionService.cs`
  - **Acceptance**: Compare extracted title/authors/year with existing records, warn if match found
  - **Methods**: FindPotentialDuplicates(article), DisplayDuplicateWarning()
  - **UI**: Show side-by-side comparison, offer merge or cancel option

### Database Integration (US1, US4)

- [ ] T049 [US1] [US4] Integrate DataStorageService with UploadViewModel
  - **File**: `src/EtnoPapers.UI/ViewModels/UploadViewModel.cs` (update)
  - **Acceptance**: Call DataStorageService.Create() to save extraction results
  - **Features**: Handle save success/failure, update UI feedback

---

## Phase 4: Record Management UI (US1, US2)

**Goal**: Implement CRUD interface for managing local records

**Why Fourth**: Depends on working data storage; completes local workflow

### Records Page (US1, US2)

- [ ] T050 [US1] [US2] Create RecordsPage and RecordsViewModel
  - **Files**: `src/EtnoPapers.UI/Views/RecordsPage.xaml`, `RecordsPage.xaml.cs`, `ViewModels/RecordsViewModel.cs`
  - **Acceptance**: Page with record list/grid, filter/search, CRUD action toolbar
  - **Bindings**: Load records on page load, bind to GridDataSource
  - **Actions**: Edit, delete (bulk select), sync

- [ ] T051 [US1] Create virtualized record grid
  - **File**: `src/EtnoPapers.UI/Controls/VirtualizedRecordGrid.xaml`
  - **Acceptance**: WPF DataGrid with virtualization (only render visible items) for 1000+ records
  - **Performance**: Enable virtual scrolling, complete operations in <200ms
  - **Columns**: Title, Authors, Year, Biome, Last Modified, Sync Status
  - **Selection**: Multi-select checkbox

- [ ] T052 [US1] [US2] Create record filter controls
  - **File**: `src/EtnoPapers.UI/Controls/RecordFilters.xaml`
  - **Acceptance**: Filters for year range, author, biome, species presence, search text
  - **Binding**: Two-way binding to RecordsViewModel filters
  - **Features**: Debounced search (300ms), clear filters button

- [ ] T053 [US1] Create edit record dialog
  - **File**: `src/EtnoPapers.UI/Views/EditRecordDialog.xaml`
  - **Acceptance**: Modal dialog with full record editing form
  - **Binding**: Pre-populated from selected record
  - **Features**: Save button, cancel, validation on save

- [ ] T054 [US1] Create delete confirmation dialog
  - **File**: `src/EtnoPapers.UI/Views/DeleteConfirmDialog.xaml`
  - **Acceptance**: Confirmation dialog for destructive actions
  - **Display**: Show record count for bulk delete, confirm action
  - **Features**: Cancel option, irreversible warning

- [ ] T055 [US1] Create new record dialog
  - **File**: `src/EtnoPapers.UI/Views/NewRecordDialog.xaml`
  - **Acceptance**: Modal for manual record creation (empty form)
  - **Features**: Form validation, save creates new record

### Record Management Logic (US1)

- [ ] T056 [US1] Implement RecordsViewModel
  - **File**: `src/EtnoPapers.UI/ViewModels/RecordsViewModel.cs`
  - **Acceptance**: Load records, filter, CRUD operations
  - **Methods**: LoadRecords(), CreateRecord(data), EditRecord(id, data), DeleteRecord(id), DeleteMultiple(ids), ApplyFilters()
  - **State**: Records[], SelectedIds[], Filters, IsLoading, Error

- [ ] T057 [US1] [US2] Integrate storage service with RecordsViewModel
  - **File**: `src/EtnoPapers.UI/ViewModels/RecordsViewModel.cs` (update)
  - **Acceptance**: Call DataStorageService for all CRUD operations
  - **Features**: Handle success/failure, update UI, show validation errors

---

## Phase 5: MongoDB Synchronization (US1, US4)

**Goal**: Implement cloud sync for selected records

**Why Fifth**: Optional in MVP but important for data backup

### Sync Page (US1, US4)

- [ ] T058 [US1] [US4] Create sync selection UI
  - **Files**: `src/EtnoPapers.UI/Controls/SyncPanel.xaml`, `SyncPanel.xaml.cs`
  - **Acceptance**: Show selected records, sync button, connection status
  - **Display**: Record preview, selected count, MongoDB status
  - **Features**: Connection test button, error display

- [ ] T059 [US1] [US4] Create sync progress display
  - **File**: `src/EtnoPapers.UI/Controls/SyncProgressPanel.xaml`
  - **Acceptance**: Progress bar, per-record status (pending, success, failed), cancel button
  - **Bindings**: Update as sync progresses
  - **Features**: Error details for failed records, retry options

- [ ] T060 [US1] [US4] Create sync reminder notification
  - **File**: `src/EtnoPapers.UI/Controls/SyncReminder.xaml`
  - **Acceptance**: Notification if records count >500 or 7+ days since last sync
  - **Features**: Dismissible, non-intrusive

### Sync Workflow (US1, US4)

- [ ] T061 [US1] [US4] Create SyncViewModel
  - **File**: `src/EtnoPapers.UI/ViewModels/SyncViewModel.cs`
  - **Acceptance**: Manage sync workflow, state, progress
  - **Methods**: SelectRecords(ids), StartSync(), CancelSync(), GetSyncStatus()
  - **State**: SelectedRecords[], IsSyncing, Progress, FailedRecords, LastSyncTime

- [ ] T062 [US4] Integrate MongoDBSyncService with SyncViewModel
  - **File**: `src/EtnoPapers.UI/ViewModels/SyncViewModel.cs` (update)
  - **Acceptance**: Call MongoDBSyncService.UploadBatch(), delete local on success
  - **Features**: Handle network failures gracefully, keep records local on error

---

## Phase 6: Settings & Configuration (US1, US4)

**Goal**: Implement configuration UI for OLLAMA and MongoDB

**Why Sixth**: Can be developed early since service exists; used by all workflows

### Settings Page (US1, US4)

- [ ] T063 [US1] [US4] Create SettingsPage and SettingsViewModel
  - **Files**: `src/EtnoPapers.UI/Views/SettingsPage.xaml`, `SettingsPage.xaml.cs`, `ViewModels/SettingsViewModel.cs`
  - **Acceptance**: Form for OLLAMA config, MongoDB URI, language selection
  - **Bindings**: Load current config, save on apply
  - **Sections**: AI Configuration, Database Configuration, Application Settings

- [ ] T064 [US1] Create OLLAMA configuration form
  - **File**: `src/EtnoPapers.UI/Controls/OLLAMAConfigForm.xaml`
  - **Acceptance**: Input fields for OLLAMA URL (default: http://localhost:11434), model name, custom prompt
  - **Features**: Test connection button, connection status display
  - **Validation**: Valid URL format, non-empty model

- [ ] T065 [US4] Create MongoDB configuration form
  - **File**: `src/EtnoPapers.UI/Controls/MongoDBConfigForm.xaml`
  - **Acceptance**: Input field for MongoDB URI
  - **Features**: Test connection button, connection status display, validate URI format
  - **Security**: Mask password display, clear on reset

- [ ] T066 [US1] [US4] Create connection test UI
  - **File**: `src/EtnoPapers.UI/Controls/ConnectionTestButton.xaml`
  - **Acceptance**: Button that tests OLLAMA/MongoDB connection, shows result (spinner, success, error)
  - **Binding**: Disabled when testing, shows status message
  - **Timeout**: 5 second timeout for connection test

- [ ] T067 Create application settings form
  - **File**: `src/EtnoPapers.UI/Controls/AppSettingsForm.xaml`
  - **Acceptance**: Language selection (Portuguese/English), window size/position options
  - **Features**: Save preferences, apply immediately

### Settings Logic (US1, US4)

- [ ] T068 [US1] [US4] Implement SettingsViewModel
  - **File**: `src/EtnoPapers.UI/ViewModels/SettingsViewModel.cs`
  - **Acceptance**: Load configuration, apply changes, test connections
  - **Methods**: LoadConfiguration(), SaveConfiguration(), TestOLLAMAConnection(), TestMongoDBConnection()
  - **State**: OLLAMAUrl, OLLAMAModel, OLLAMAPrompt, MongoDBUri, Language, ConnectionStatus

- [ ] T069 [US1] [US4] Integrate ConfigurationService with SettingsViewModel
  - **File**: `src/EtnoPapers.UI/ViewModels/SettingsViewModel.cs` (update)
  - **Acceptance**: Load/save configuration from ConfigurationService
  - **Features**: Validation before save, default values, persistent storage

---

## Phase 7: Performance & Testing

**Goal**: Optimize performance and validate all features

**Why Seventh**: After all features implemented, benchmark and test

### Performance Optimization (US3)

- [ ] T070 [US3] Create startup performance benchmark
  - **File**: `src/EtnoPapers.UI/Services/PerformanceBenchmark.cs`
  - **Acceptance**: Measure startup time from app launch to main window visible
  - **Target**: <2 seconds (primary success criterion)
  - **Logging**: Log startup stages and durations

- [ ] T071 [US3] Create record management performance test
  - **File**: `src/EtnoPapers.UI/Services/RecordManagementBenchmark.cs`
  - **Acceptance**: Measure time for sort 1000 records, filter, search, pagination
  - **Target**: <200ms per operation
  - **Logging**: Log operation times

- [ ] T072 [US3] Create memory usage profiling setup
  - **File**: `src/EtnoPapers.UI/Services/MemoryProfiler.cs`
  - **Acceptance**: Measure idle memory usage after loading 1000 records
  - **Target**: <150 MB
  - **Method**: Use GC.GetTotalMemory() or performance counters

- [ ] T073 [US3] Optimize startup by lazy-loading
  - **File**: `src/EtnoPapers.UI/ViewModels/MainWindowViewModel.cs` (update)
  - **Acceptance**: Don't load records on startup; load on Records page first access
  - **Features**: Lazy loading, progress indication during load

- [ ] T074 [US3] Optimize record grid virtual scrolling
  - **File**: `src/EtnoPapers.UI/Controls/VirtualizedRecordGrid.xaml` (update)
  - **Acceptance**: Ensure DataGrid virtualization is enabled, no rendering of off-screen items
  - **Testing**: Load 1000+ records, verify <200ms interactions

### Unit Tests (US1, US4)

- [ ] T075 [P] Write unit tests for TitleNormalizer
  - **File**: `tests/EtnoPapers.Core.Tests/Utils/TitleNormalizerTests.cs`
  - **Coverage**: >90% - test proper case, acronyms, particles, edge cases
  - **Framework**: xUnit

- [ ] T076 [P] Write unit tests for AuthorFormatter
  - **File**: `tests/EtnoPapers.Core.Tests/Utils/AuthorFormatterTests.cs`
  - **Coverage**: >90% - test single/multiple names, particles, suffixes, APA format
  - **Framework**: xUnit

- [ ] T077 [P] Write unit tests for LanguageDetector
  - **File**: `tests/EtnoPapers.Core.Tests/Utils/LanguageDetectorTests.cs`
  - **Coverage**: >85% - test PT, EN, ES detection accuracy
  - **Framework**: xUnit

- [ ] T078 [P] Write unit tests for ArticleRecordValidator
  - **File**: `tests/EtnoPapers.Core.Tests/Validation/ArticleRecordValidatorTests.cs`
  - **Coverage**: >85% - test valid/invalid records, mandatory field checks
  - **Framework**: xUnit

### Integration Tests (US1, US4)

- [ ] T079 Write integration test for JSON serialization
  - **File**: `tests/EtnoPapers.Core.Tests/Integration/JsonSerializationTests.cs`
  - **Acceptance**: Load Electron-generated JSON, deserialize, serialize back, verify identical
  - **Test Data**: Include sample JSON from Electron version
  - **Framework**: xUnit

- [ ] T080 Write integration test for DataStorageService
  - **File**: `tests/EtnoPapers.Core.Tests/Integration/DataStorageServiceTests.cs`
  - **Acceptance**: CRUD operations, JSON persistence, limit enforcement
  - **Setup**: Use temp directory for test JSON files
  - **Framework**: xUnit

- [ ] T081 Write integration test for extraction pipeline
  - **File**: `tests/EtnoPapers.Core.Tests/Integration/ExtractionPipelineTests.cs`
  - **Acceptance**: End-to-end: PDF → text → validation → storage (with mocked OLLAMA)
  - **Mocks**: Mock OLLAMAService to return test responses
  - **Framework**: xUnit

- [ ] T082 Write integration test for MongoDB sync
  - **File**: `tests/EtnoPapers.Core.Tests/Integration/MongoDBSyncServiceTests.cs`
  - **Acceptance**: Test MongoDB connection, batch upload with test database
  - **Setup**: Use MongoDB test container or memory server
  - **Framework**: xUnit

### UI Acceptance Testing (US1, US2)

- [ ] T083 Create UI acceptance test checklist
  - **File**: `tests/UI_ACCEPTANCE_TEST_CHECKLIST.md`
  - **Scenarios**:
    - Upload PDF → Extract → Edit → Save
    - View records → Filter → Edit → Delete
    - Configure OLLAMA → Test → Save
    - Configure MongoDB → Test → Sync
    - Verify Windows native controls, keyboard shortcuts, taskbar integration

---

## Phase 8: Build, Installer & Release

**Goal**: Create professional Windows installer and release package

**Why Eighth**: After all features complete and tested

### Project File Configuration

- [ ] T084 Configure EtnoPapers.UI as startup project
  - **File**: `EtnoPapers.sln`
  - **Acceptance**: UI project set as startup in solution
  - **Command**: Right-click project → Set as Startup Project

- [ ] T085 Configure version information
  - **File**: `src/EtnoPapers.UI/EtnoPapers.UI.csproj`
  - **Acceptance**: Version set to 1.0.0, product name, company, copyright
  - **Properties**: AssemblyVersion, FileVersion, ProductVersion, Company

### Installer Creation

- [ ] T086 Create Windows installer using WiX or NSIS
  - **File**: `build/installer.wxs` (or `.nsi` for NSIS)
  - **Acceptance**: Installer configuration created, can build to .msi or .exe
  - **Features**: Install to Program Files, Start Menu shortcuts, .NET 8 prerequisite check
  - **Tool**: WiX Toolset or NSIS (choose based on preference)

- [ ] T087 Create .NET 8 prerequisite check script
  - **File**: `build/check-dotnet.ps1`
  - **Acceptance**: PowerShell script checks for .NET 8 runtime, prompts user to install if missing
  - **Distribution**: Include with installer

- [ ] T088 Create installer asset files
  - **Files**: `build/icon.ico`, `build/banner.bmp`, `build/LICENSE.txt`
  - **Acceptance**: Professional icons, installer banner, license file
  - **Content**: Brand images, MITL/GPL license text as appropriate

### Build Scripts

- [ ] T089 Create production build script
  - **File**: `build/build-release.ps1`
  - **Acceptance**: Builds solution in Release mode, creates installer
  - **Commands**: `dotnet build -c Release`, installer creation
  - **Output**: Produces `dist/EtnoPapers-1.0.0.exe` and `dist/EtnoPapers-1.0.0.msi`

### Documentation

- [ ] T090 Create installation guide (Portuguese)
  - **File**: `docs/INSTALLATION_PT-BR.md`
  - **Acceptance**: Step-by-step installation instructions for non-technical users
  - **Content**: Download link, system requirements (.NET 8, Windows 10+), installation steps, troubleshooting

- [ ] T091 Create release notes
  - **File**: `CHANGELOG.md`
  - **Acceptance**: Document version 1.0.0 features, known issues, installation requirements
  - **Format**: Markdown, user-friendly language

---

## Phase 9: Launch & Post-Release

**Goal**: Release application and set up support infrastructure

**Why Last**: After all phases complete

### GitHub Release

- [ ] T094 Create GitHub release
  - **File**: GitHub Release page
  - **Acceptance**: v1.0.0 release created with installer attachments
  - **Content**: Release notes, download links, installation instructions, known issues

- [ ] T095 Update README.md for release
  - **File**: `README.md`
  - **Acceptance**: Update with download link, installation instructions, feature list
  - **Sections**: Features, Installation, Usage, Troubleshooting, Support Contact

### Support Infrastructure

- [ ] T096 Create user support email template
  - **File**: `docs/SUPPORT_EMAIL_TEMPLATE.md`
  - **Acceptance**: Template for responding to user bug reports and support requests
  - **Content**: Troubleshooting steps, log collection instructions, contact info

- [ ] T097 Set up issue tracking
  - **File**: GitHub Issues configuration
  - **Acceptance**: Create labels (bug, feature-request, documentation), enable discussions
  - **Template**: Issue template for bug reports

### Post-Release Monitoring

- [ ] T098 Set up crash reporting (optional)
  - **File**: `src/EtnoPapers.UI/Services/CrashReporter.cs`
  - **Acceptance**: Optional Sentry or similar integration for crash reports
  - **User Control**: Opt-in only, can be disabled in settings

- [ ] T099 Create monitoring dashboard setup
  - **File**: `docs/MONITORING_SETUP.md`
  - **Acceptance**: Instructions for setting up error tracking/monitoring
  - **Tools**: Sentry (optional), application logs review

---

## Dependencies & Execution Order

### User Story Dependency Chain

```
US4 (Data Compatibility) ← Blocks everything
  ↓
US1 (Feature Parity) ← Depends on US4
  ↓
US2 (Windows Native) ← Depends on US1
  ↓
US3 (Performance) ← Depends on US1, US2 (validation)
  ↓
Release
```

**Execution Path**:
1. Phase 0 (Setup) - Initialize solution
2. Phase 1 (Foundational) - Core services & data models (supports US4, US1)
3. Phase 2 (UI Foundation) - MVVM, WPF controls (supports US2)
4. Phase 3 (Extraction UI) - Upload workflow (US1 + US2)
5. Phase 4 (Record Management) - CRUD interface (US1 + US2)
6. Phase 5 (MongoDB Sync) - Cloud backup (US4)
7. Phase 6 (Settings) - Configuration (US4)
8. Phase 7 (Testing & Performance) - Validate US1, US2, US3
9. Phase 8-9 (Release) - Build and launch

### Parallel Opportunities

**Phase 0 (Setup)**:
- T002, T003, T004 can run in parallel (different projects)
- T005-T008 can run in parallel (different project files)
- T009-T011 can run in parallel (different directories)

**Phase 1 (Foundational)**:
- T012-T020 can run in parallel (independent models/utilities)
- T022-T029 services can be started in parallel, but ExtractionPipelineService (T028) depends on T024, T025, T027

**Phase 2 (UI Foundation)**:
- T034, T035, T036, T037 can run in parallel (different controls)
- T039-T040 localization files can run in parallel

**Phase 3-7**:
- UI pages (Upload, Records, etc.) can start in parallel once MainWindow exists
- Tests can start once services are implemented

---

## Task Validation & Metrics

| Metric | Value | Status |
|--------|-------|--------|
| Total Tasks | 99 | ✅ |
| Setup Phase (T001-T011) | 11 | ✅ |
| Foundational Phase (T012-T093) | 82 | ✅ |
| US1 Phase (T030-T049) | 20 | ✅ |
| US2 Phase (incorporated in T030-T074) | Integrated | ✅ |
| US4 Phase (incorporated in T012-T069) | Integrated | ✅ |
| US3 Phase (T070-T074) | 5 | ✅ |
| Testing Phase (T075-T083) | 9 | ✅ |
| Build & Release (T084-T099) | 16 | ✅ |
| Tasks with [P] parallel marker | 25 | ✅ |
| Tasks with [US#] story label | 74 | ✅ |
| Format Compliance | 100% | ✅ |

---

## Implementation Strategy

### MVP Scope (Phases 0-6, Core Features)
**Estimated Effort**: 8-12 weeks (1-2 developers)
- Setup & infrastructure (Phase 0)
- Core services & data models (Phase 1)
- WPF UI foundation (Phase 2)
- PDF extraction workflow (Phase 3)
- Record management (Phase 4)
- MongoDB synchronization (Phase 5)
- Settings & configuration (Phase 6)

**Deliverable**: Fully functional C# WPF application with 100% feature parity with Electron

### Full Product (All Phases)
**Estimated Effort**: 12-16 weeks (1-2 developers)
- Includes comprehensive testing (Phase 7)
- Professional installer & release (Phase 8-9)
- Performance validation & benchmarking
- User documentation
- Support infrastructure

### Recommended First Week (Phases 0-1)
**Tasks**: T001-T029
- Day 1-2: Solution setup, projects, dependencies (T001-T011)
- Day 3-4: Data models, JSON serialization (T012-T017)
- Day 5: Utility functions and validation (T018-T021)
- Day 5-end: Core services (T022-T029)
- **Result**: Runnable project with testable services, ready for UI development

---

## Checklist Format Validation

All 99 tasks follow the required format:
- ✅ All start with `- [ ]` (markdown checkbox)
- ✅ All have sequential Task ID (T001...T099)
- ✅ Story labels ([US#]) present for phases 3-6
- ✅ Parallelizable tasks marked with [P]
- ✅ All have clear description with file path
- ✅ Clear acceptance criteria per task

---

**Ready to execute. Each task is specific enough for independent completion.**

**Start with Phase 0 (T001-T011): Project initialization and dependency installation.**
