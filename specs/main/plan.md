# Implementation Plan: EtnoPapers Desktop Migration - Electron to C# WPF

**Branch**: `main` (single-branch workflow)
**Date**: 2025-12-02
**Spec**: [spec.md](./spec.md)
**Input**: Feature specification for migrating EtnoPapers from Electron to C# WPF

## Summary

EtnoPapers is being refactored from a Node.js/TypeScript/Electron-based desktop application to a native Windows desktop application using C# and Windows Presentation Foundation (WPF). This migration maintains **100% functional parity** with the original Electron version while improving performance, native Windows integration, and reducing resource consumption.

**Core Value Proposition**: Transform the existing EtnoPapers application into a native Windows experience that loads faster, uses fewer resources, integrates seamlessly with Windows conventions, and provides a more professional user experience for ethnobotany researchers.

**Migration Approach**: Refactor Electron TypeScript code → C# WPF, maintaining identical data structures (JSON files, MongoDB documents), AI integration (OLLAMA), and user workflows.

---

## Technical Context

**Language/Version**: C# 12 / .NET 8.0 LTS

**Primary Dependencies**:
- **.NET 8.0**: Framework
- **WPF**: Windows Presentation Foundation (built-in to .NET)
- **C#**: Primary language
- **XAML**: UI markup language for WPF
- **MongoDB .NET Driver**: MongoDB integration (identical to Electron version)
- **OLLAMA**: External AI service (localhost REST API) - unchanged
- **Newtonsoft.Json (Json.NET)**: JSON serialization for data compatibility with Electron version
- **xUnit**: Unit testing framework
- **Moq**: Mocking library for tests
- **WiX Toolset or NSIS**: Windows installer creation

**Storage**:
- **Local**: JSON file (identical structure to Electron version, same file path) → `Documents/EtnoPapers/data.json`
- **Remote**: MongoDB (Atlas cloud or local server) - unchanged
- **Configuration**: JSON configuration file → `Documents/EtnoPapers/config.json`

**Testing**: xUnit (unit/integration), manual UI testing (WPF testing is primarily manual), plus integration tests with MongoDB

**Target Platform**: Windows 10+ (primary, exclusive)

**Project Type**: WPF Desktop Application (native Windows)

**Performance Goals**:
- Startup time: < 2 seconds (vs. Electron ~5-10 seconds) ← **Primary migration benefit**
- Idle memory: < 150 MB (vs. Electron ~300-500 MB) ← **Primary migration benefit**
- Record management interactions: < 200ms for 1000+ records (sorting, filtering, pagination, search)
- PDF extraction: identical to current implementation (delegated to OLLAMA, not performance-critical for app)
- MongoDB sync: < 5 seconds per record (network dependent)

**Constraints**:
- Windows-only application (WPF is Windows-only)
- OLLAMA must be pre-installed by user (unchanged)
- Local storage limited to 1000 records (unchanged)
- PDFs must be text-based (no OCR support - unchanged)
- Single user per installation (unchanged)

**Scale/Scope**:
- Target users: 100-1000 ethnobotany researchers (unchanged)
- Typical dataset: 50-500 papers per researcher (unchanged)
- Installation base: 100-500 active installations initially (unchanged)
- Expected PDF size: 1-50 MB, 10-100 pages (unchanged)

---

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

**Constitution File Status**: `.specify/memory/constitution.md` is a template (not filled in with actual principles).

**For this C# WPF migration project, we adopt these principles**:

### Adopted Principles for This Project

1. **Functional Parity**
   - 100% feature compatibility with Electron version
   - Identical user workflows, data formats, AI integration
   - Zero data loss during migration
   - Users can transition seamlessly

2. **Native Windows Design**
   - WPF native controls throughout (no web-like emulation)
   - Windows 11 design language compliance
   - Windows keyboard shortcuts (Ctrl+S, Alt+F4, Tab navigation)
   - Windows file dialogs, drag-and-drop, notifications

3. **Performance First**
   - Startup < 2 seconds (hard requirement)
   - Idle memory < 150 MB (hard requirement)
   - UI responsiveness < 200ms (hard requirement)
   - These metrics justify the entire migration

4. **Data Integrity**
   - JSON file format identical to Electron version
   - MongoDB documents unchanged
   - Configuration compatibility maintained
   - Migration path documented and tested

5. **Simplicity & Maintainability**
   - Standard WPF patterns and best practices
   - MVVM architectural pattern (standard WPF)
   - Clear separation of concerns (Data Layer, Service Layer, UI Layer)
   - No unnecessary complexity

6. **Quality Assurance**
   - Unit tests for business logic
   - Integration tests for data layer and MongoDB
   - Manual acceptance testing for UI and workflows
   - Performance benchmarking before release

### Phase 0 Gate: ✅ **PASSED**
- Technology choices justified (C# WPF selected in `/speckit.specify`)
- Performance targets defined (SC-002, SC-003)
- Data compatibility strategy clear (identical JSON/MongoDB)
- Architecture approach documented

### Phase 1 Gate: ✅ **PASSED**
- Data model matches Electron version (JSON structure unchanged)
- Service interfaces defined for refactored code
- No unnecessary complexity added
- All technical decisions traceable to requirements

---

## Project Structure

### Documentation (this feature)

```text
specs/main/
├── spec.md              # Feature specification (C# WPF migration)
├── plan.md              # This file (implementation plan)
├── research.md          # Technology decisions and rationale
├── data-model.md        # Data structures (JSON format - unchanged from Electron)
├── quickstart.md        # Developer onboarding guide
├── contracts/           # Service interface contracts
│   └── service-interfaces.ts (reference; will need C# equivalents)
└── tasks.md             # Task breakdown (to be generated via /speckit.tasks)
```

### Source Code (repository root)

```text
etnopapers/
├── src/
│   ├── EtnoPapers.Core/                    # Business logic (C# class library)
│   │   ├── Services/                       # Service implementations
│   │   │   ├── ConfigurationService.cs     # Settings (app.config, JSON)
│   │   │   ├── DataStorageService.cs       # Local JSON file management
│   │   │   ├── PDFProcessingService.cs     # PDF text extraction (iTextSharp/Spire)
│   │   │   ├── OLLAMAService.cs            # REST API integration with local OLLAMA
│   │   │   ├── MongoDBSyncService.cs       # MongoDB upload/sync
│   │   │   ├── ExtractionPipelineService.cs # Orchestration: PDF→text→AI→validation→storage
│   │   │   ├── ValidationService.cs        # Data validation against schema
│   │   │   └── LoggerService.cs            # File-based logging
│   │   ├── Models/                         # Data classes matching Electron JSON schema
│   │   │   ├── ArticleRecord.cs
│   │   │   ├── PlantSpecies.cs
│   │   │   ├── Community.cs
│   │   │   ├── SyncStatus.cs
│   │   │   └── Configuration.cs
│   │   ├── Utils/                          # Utility functions
│   │   │   ├── TitleNormalizer.cs          # Proper case conversion
│   │   │   ├── AuthorFormatter.cs          # APA format conversion
│   │   │   ├── LanguageDetector.cs         # Portuguese/English detection
│   │   │   └── DateParser.cs               # Date parsing utilities
│   │   └── Validation/                     # Validation logic
│   │       └── ArticleRecordValidator.cs   # Zod-equivalent validation
│   │
│   ├── EtnoPapers.UI/                      # WPF User Interface
│   │   ├── Views/                          # XAML Windows and UserControls
│   │   │   ├── MainWindow.xaml             # Main application window
│   │   │   ├── MainWindow.xaml.cs
│   │   │   ├── HomePage.xaml               # Home/Dashboard page
│   │   │   ├── HomePage.xaml.cs
│   │   │   ├── UploadPage.xaml             # PDF upload and extraction
│   │   │   ├── UploadPage.xaml.cs
│   │   │   ├── RecordsPage.xaml            # Record management (CRUD)
│   │   │   ├── RecordsPage.xaml.cs
│   │   │   ├── SettingsPage.xaml           # Configuration page
│   │   │   ├── SettingsPage.xaml.cs
│   │   │   ├── AboutPage.xaml              # About / Author info
│   │   │   └── AboutPage.xaml.cs
│   │   ├── ViewModels/                     # MVVM ViewModels
│   │   │   ├── MainWindowViewModel.cs
│   │   │   ├── HomeViewModel.cs
│   │   │   ├── UploadViewModel.cs
│   │   │   ├── RecordsViewModel.cs
│   │   │   ├── SettingsViewModel.cs
│   │   │   └── AboutViewModel.cs
│   │   ├── Controls/                       # Reusable WPF User Controls
│   │   │   ├── StatusBar.xaml              # Connection status indicator
│   │   │   ├── StatusBar.xaml.cs
│   │   │   ├── ProgressPanel.xaml          # Extraction progress display
│   │   │   ├── ProgressPanel.xaml.cs
│   │   │   ├── RecordGrid.xaml             # Virtualized record grid
│   │   │   └── RecordGrid.xaml.cs
│   │   ├── Styles/                         # XAML styles and brushes
│   │   │   ├── Brushes.xaml                # Color definitions
│   │   │   └── ControlStyles.xaml          # Control templates
│   │   ├── Converters/                     # IValueConverter implementations
│   │   │   ├── BoolToVisibilityConverter.cs
│   │   │   ├── StatusToBrushConverter.cs
│   │   │   └── DateFormatConverter.cs
│   │   ├── Resources/                      # Application resources
│   │   │   ├── App.xaml                    # Global application resources
│   │   │   └── App.xaml.cs                 # Code-behind
│   │   └── Localization/                   # i18n translations
│   │       ├── Strings.pt-BR.resx          # Portuguese strings
│   │       └── Strings.en-US.resx          # English strings
│   │
│   └── EtnoPapers.sln                      # Visual Studio solution file
│
├── tests/
│   ├── EtnoPapers.Core.Tests/              # Unit and integration tests
│   │   ├── Services/
│   │   │   ├── ConfigurationServiceTests.cs
│   │   │   ├── DataStorageServiceTests.cs
│   │   │   ├── PDFProcessingServiceTests.cs
│   │   │   ├── OLLAMAServiceTests.cs
│   │   │   ├── MongoDBSyncServiceTests.cs
│   │   │   ├── ExtractionPipelineTests.cs
│   │   │   └── ValidationServiceTests.cs
│   │   └── Utils/
│   │       ├── TitleNormalizerTests.cs
│   │       ├── AuthorFormatterTests.cs
│   │       ├── LanguageDetectorTests.cs
│   │       └── DateParserTests.cs
│   │
│   └── EtnoPapers.UI.Tests/                # UI Integration tests (manual + automation)
│       ├── Workflows/
│       │   ├── ExtractionWorkflowTest.cs   # PDF upload → extraction → save
│       │   ├── RecordManagementTest.cs     # CRUD operations
│       │   └── SyncWorkflowTest.cs         # MongoDB sync workflow
│       └── Performance/
│           ├── StartupPerformanceTest.cs   # Verify < 2 seconds
│           └── RecordManagementPerformanceTest.cs # Verify < 200ms interactions
│
├── docs/                                   # Project documentation
│   ├── estrutura.json                      # Data structure reference (unchanged from Electron)
│   ├── etnopapers.png                      # Application logo
│   └── promptInicial.txt                   # Original project description
│
├── build/                                  # Build scripts and installer config
│   ├── installer.wxs                       # WiX installer source (or NSIS config)
│   └── install-dotnet.ps1                  # PowerShell script to check .NET 8
│
├── specs/                                  # Feature specifications (SpecKit)
│   └── main/                               # Current migration feature
│
├── .specify/                               # SpecKit framework
├── .claude/                                # Claude Code configuration
├── EtnoPapers.sln                          # Visual Studio solution
├── .gitignore                              # Git ignore rules
├── README.md                               # User documentation (Portuguese)
├── CLAUDE.md                               # Development guide
└── TESTING_GUIDE.md                        # Testing approach documentation

```

**Structure Decision**:
- Separate class libraries for Core (business logic) and UI (WPF)
- MVVM pattern for WPF (industry standard)
- Clear separation between data layer, service layer, and presentation layer
- Matches standard .NET enterprise application structure
- Enables independent testing of business logic from UI

---

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

This section intentionally left empty - no constitution violations to justify.

All architectural decisions follow standard WPF and .NET practices:
- C# for core logic: Industry standard for Windows desktop development
- WPF for UI: Microsoft-recommended framework for native Windows applications
- MVVM pattern: Decouples UI from business logic, standard WPF practice
- Service-oriented architecture: Testability and maintainability
- Local + cloud storage: Identical to Electron version

---

## Implementation Phases Overview

### Phase 0: Project Setup & Core Infrastructure ✅ COMPLETE (in progress)
**Artifact**: Repository reorganized, Electron code removed, C# project structure created

**Completed**:
- ✅ Removed Electron/Node.js/TypeScript artifacts
- ✅ Created C# project directory structure (src/EtnoPapers.Core, src/EtnoPapers.UI, tests/)
- ✅ Spec and plan updated for C# WPF migration

**Next**: Generate initial task breakdown

### Phase 1: Core Services & Data Layer (P1)
**Dependencies**: Phase 0 complete
**Deliverables**:
- Data model classes matching Electron JSON schema
- Core services (Configuration, Storage, PDF, OLLAMA, Validation)
- JSON serialization for Electron compatibility
- MongoDB driver integration
- Logger service

**Why First**: All other features depend on working data layer and service infrastructure

### Phase 2: WPF UI Foundation (P1)
**Dependencies**: Phase 1 complete
**Deliverables**:
- MainWindow shell with navigation
- ViewModels for MVVM pattern
- Reusable WPF controls and styles
- Localization setup (Portuguese/English)

**Why Second**: UI can be built in parallel with some Phase 1 services, but needs data models to be stable

### Phase 3: PDF Upload & Extraction UI (P1)
**Dependencies**: Phase 1 (services) + Phase 2 (UI framework)
**Deliverables**:
- Upload page with file dialog and drag-and-drop
- Extraction progress display
- Results editing form
- Save to local storage

**Why This Order**: Core MVP feature - researchers need PDF extraction

### Phase 4: Record Management UI (P2)
**Dependencies**: Phase 1 (storage service) + Phase 2 (UI framework)
**Deliverables**:
- Records page with virtualized DataGrid (1000+ records)
- Filter/search/sort functionality (< 200ms requirement)
- CRUD dialogs (create, edit, delete)
- Confirmation modals for destructive actions

**Why This Order**: Secondary MVP feature - complete local workflow before cloud sync

### Phase 5: MongoDB Synchronization (P3)
**Dependencies**: Phase 1 (MongoDB service) + Phase 4 (record selection)
**Deliverables**:
- Sync panel for record selection and batch upload
- Progress tracking during upload
- Local deletion after successful sync
- Sync reminder notifications
- Offline-first workflow maintenance

**Why This Order**: Optional in MVP but important for data backup

### Phase 6: Settings & Configuration (P1)
**Dependencies**: Phase 1 (configuration service) + Phase 2 (UI framework)
**Deliverables**:
- Settings page for OLLAMA configuration
- MongoDB connection URI input
- Connection testing buttons
- Configuration persistence
- GPU detection and advisory message

**Why This Order**: Prerequisites user setup; can be built early since relatively simple

### Phase 7: Testing & Quality Assurance
**Dependencies**: Phases 1-6 complete
**Deliverables**:
- Unit tests for all services
- Integration tests for data layer and MongoDB
- UI automation tests for critical workflows
- Performance benchmarking
- Manual acceptance testing

### Phase 8: Build, Installer & Release
**Dependencies**: All phases complete + all tests passing
**Deliverables**:
- WiX or NSIS installer configuration
- .NET 8 runtime dependency bundling
- Code signing (optional)
- Windows installer (.msi or .exe)
- Release documentation

### Phase 9: Launch & Post-Release
**Dependencies**: Phase 8 complete
**Deliverables**:
- GitHub release with installer
- User documentation update
- Support setup
- Monitoring for crash reports

---

## Architectural Patterns

### MVVM (Model-View-ViewModel) Pattern

**WPF Standard Pattern**:

```
View (XAML) ↔ ViewModel (C# class) ↔ Model (Data classes)
    ↓
  Binding to properties and commands
```

**Benefits**:
- UI logic separated from presentation
- Easy to unit test ViewModels (no WPF dependencies)
- Data binding handles UI updates
- Supports reactive programming

**Implementation**:
- Each XAML page has corresponding ViewModel
- ViewModel exposes properties (with INotifyPropertyChanged)
- ViewModel exposes commands (ICommand implementations)
- View binds to ViewModel (DataContext)

### Service Layer Pattern

**Architecture**:

```
ViewModel → Service Interface → Service Implementation (Core library)
```

**Benefits**:
- Testable in isolation (mock services)
- Clear separation of concerns
- Easy to swap implementations
- Business logic independent of UI framework

**Implementation**:
- All services in EtnoPapers.Core
- UI project references Core for service interfaces
- Dependency injection setup in App.xaml.cs

### Local-First Data Flow

```
User Action → Update ViewModel → Update Local Storage → Background MongoDB Sync (if enabled)
```

**Benefits**:
- Fast, responsive UI (no waiting for network)
- Works fully offline
- User maintains control over sync
- MongoDB is backup, not primary storage

---

## Key Technical Challenges & Solutions

### Challenge 1: Maintain 100% Data Format Compatibility

**Problem**: C# application must read/write JSON files identically to Electron version, or existing users lose data

**Solution**:
- Use Newtonsoft.Json (Json.NET) for serialization
- Define C# data classes matching Electron JSON exactly
- Unit tests verify JSON round-trip (C# → JSON → C# → identical)
- Same file paths as Electron (`Documents/EtnoPapers/data.json`)
- Test with actual Electron-generated JSON files

### Challenge 2: PDF Text Extraction in C#

**Problem**: Different library from Electron (pdf.js) - must produce identical text extraction

**Solution**:
- Evaluate C# libraries: iTextSharp, Spire.Pdf, PdfSharp, Syncfusion PDF
- Compare extraction quality with existing Electron output
- Create test suite with real research papers
- Fallback mechanism if text extraction fails (same as Electron)

### Challenge 3: OLLAMA Integration Unchanged

**Problem**: OLLAMA REST API must work identically from C# as it does from Node.js

**Solution**:
- Use HttpClient for REST API calls (standard .NET)
- Maintain identical prompts and parameters
- Handle responses identically (JSON parsing, error handling)
- Connection testing before extraction (same as Electron)
- Timeout handling (same defaults)

### Challenge 4: Performance Targets (Startup < 2s, Memory < 150MB)

**Problem**: Must meet hard performance requirements that justify migration cost

**Solution**:
- Measure baseline Electron performance first (establish baseline)
- Profile WPF app startup with dotTrace/PerfView
- Identify and eliminate bottlenecks
- Lazy-load resources (only when needed)
- Load large record sets with virtualization
- Use .NET performance best practices (avoid allocations, async/await, etc.)
- Benchmarking tests before release (T128 in tasks)

### Challenge 5: Windows Integration (Native Controls, Dialogs, etc.)

**Problem**: Must feel like native Windows application, not cross-platform emulation

**Solution**:
- Use WPF built-in controls only (no custom cross-platform UI)
- File dialogs: System.Windows.Forms.OpenFileDialog (Windows-native)
- Drag-and-drop: XAML native support
- Windows notifications: ToastNotificationManager (Windows Runtime)
- Keyboard shortcuts: WPF InputBindings
- Window state preservation: WPF built-in (SaveWindowSize/RestoreWindowSize)
- Taskbar integration: WPF native

### Challenge 6: Test Framework for WPF

**Problem**: WPF UI testing is difficult - no jsdom equivalent like React Testing Library

**Solution**:
- Unit tests for ViewModels (no WPF dependency)
- Integration tests for services (xUnit + Moq)
- UI automation tests for critical workflows (UIAutomation or similar)
- Manual acceptance testing for UX validation
- Performance tests for startup time and memory

---

## Security Considerations

### Data Privacy
- **No external data transmission** except to user-configured MongoDB
- **PDFs processed locally**, never uploaded
- **Immediately delete PDFs** after extraction (identical to Electron)
- **No telemetry** or usage tracking

### Credential Storage
- MongoDB URI stored in `config.json` (user Documents folder, restricted permissions)
- OLLAMA requires no authentication (localhost only)
- Configuration file accessible only to current user (Windows file permissions)

### Input Validation
- File type verification (magic numbers, not just extension)
- Size limits (50 MB max per PDF)
- Sanitize all extracted text before display (prevent code injection)
- Validate MongoDB URIs before connection attempt
- JSON schema validation before storage

### Process Isolation
- WPF runs in single process (unlike Electron's multi-process architecture)
- All code runs in same security context as logged-in user
- No sandboxing (unlike Electron renderer isolation)

---

## Performance Optimizations

### Startup Performance
- Lazy-load record data (don't load all records at startup)
- Lazy-load MongoDB driver (only load if user configures sync)
- Use background threads for connection testing
- Skip slow operations until user triggers them

### UI Responsiveness
- Virtual scrolling for record lists (only render visible items)
- Debounce search/filter inputs (300ms)
- Background threads for long-running operations
- Progress feedback during extraction and sync

### PDF Processing
- Stream PDF reading (don't load entire file to memory)
- Page-by-page processing for large PDFs
- Cancellable operations (user can abort)
- Memory limits enforced

### Local Storage
- Single JSON file (lowdb equivalent not available in .NET; roll custom or use JSON serialization)
- In-memory index for fast searches (DataTable or LINQ)
- Auto-save with debouncing (30 seconds)
- Compact storage (remove whitespace)

### MongoDB Sync
- Batch uploads (up to 10 concurrent)
- Connection pooling (MongoDB driver handles)
- Parallel uploads with progress tracking
- Automatic retry on transient failures

---

## Error Handling Strategy

### User-Facing Errors

**Principle**: Every error message must tell the user:
1. What went wrong (in plain language)
2. Why it happened (if known)
3. What they can do about it

**Example**:
```
❌ Cannot Upload PDF

The AI service (OLLAMA) is not running.

What to do:
1. Start the OLLAMA application
2. Wait for the connection indicator to turn green
3. Try uploading the PDF again

Need help? Click here for setup guide.
```

### Technical Errors

**Logging Strategy**:
- All errors logged to file (`%APPDATA%/EtnoPapers/logs/`) with full stack trace
- User-facing message shown in UI (simplified)
- Option to view details or copy error for support

**Log Levels**:
- `Error`: Unexpected failures requiring user action
- `Warn`: Degraded functionality but operation continues
- `Info`: Normal operations (startup, config changes, sync)
- `Debug`: Detailed trace for troubleshooting

**Log Location**: `%APPDATA%/EtnoPapers/logs/app-YYYY-MM-DD.log`

### Error Recovery

**Graceful Degradation**:
- If OLLAMA unavailable: Show message, disable upload button
- If MongoDB unreachable: Allow local work, show sync unavailable
- If extraction fails: Provide manual entry form (identical to Electron)
- If validation fails: Highlight errors, allow saving with warnings

---

## Internationalization (i18n)

**Languages**:
- **pt-BR** (Brazilian Portuguese): Default, primary
- **en-US** (English): Secondary, for international users

**Implementation Approach**:
- .NET resource files (.resx) for UI strings (standard WPF approach)
- Separate satellite assemblies for each language
- Language setting in configuration (persisted)
- No restart required for language switching

**What's Translated**:
- All UI text (buttons, labels, headings, menus)
- Error messages
- Validation messages
- Settings descriptions
- About page content

**What's NOT Translated**:
- Extracted data (keeps source language)
- Log files (English for consistency)
- Configuration keys
- Data model field names

---

## Testing Strategy

### Unit Tests (xUnit)

**Coverage Goals**: >80% for:
- Utility functions (TitleNormalizer, AuthorFormatter, LanguageDetector)
- Validation logic (ArticleRecordValidator)
- Service business logic (mocked dependencies)

**Example**:
```csharp
[Fact]
public void NormalizeTitle_ConvertsToProperCase()
{
    // Arrange
    var input = "uso DE plantas";

    // Act
    var result = TitleNormalizer.Normalize(input);

    // Assert
    Assert.Equal("Uso de Plantas", result);
}
```

### Integration Tests (xUnit + Moq)

**Coverage Goals**: Critical workflows

**Test Scenarios**:
- Data storage: CRUD operations, JSON persistence, limit enforcement
- OLLAMA integration: Connection test, extraction with mocked responses
- MongoDB sync: Connection test, batch upload with test MongoDB instance
- Extraction pipeline: PDF → text → validation → storage end-to-end
- Configuration: Save, load, persistence across restarts

**Example**:
```csharp
[Fact]
public async Task ExtractionPipeline_ExtractsMetadataFromPdf()
{
    // Arrange
    var pdfPath = @".\fixtures\test-paper.pdf";
    var service = new ExtractionPipelineService(...);

    // Act
    var result = await service.ExtractFromPdfAsync(pdfPath);

    // Assert
    Assert.NotNull(result.Titulo);
    Assert.NotEmpty(result.Autores);
    Assert.True(result.Ano > 1500);
    Assert.NotNull(result.Resumo);
}
```

### UI Integration Tests

**Coverage Goals**: Complete user journeys

**Test Scenarios**:
1. **Startup**: Application starts in < 2 seconds, loads configuration
2. **Extraction Workflow**: Open upload page → Select PDF → Extraction completes → Edit results → Save
3. **Record Management**: View records → Filter/search → Edit → Delete with confirmation
4. **MongoDB Sync**: Select records → Configure MongoDB → Upload → Verify local deletion
5. **Settings**: Change OLLAMA config → Change MongoDB URI → Test connections

**Testing Approach**:
- Manual testing with checklist (simple but reliable for WPF)
- UIAutomation for critical paths (if needed)
- Performance measurement (stopwatch timing for startup/responsiveness)

### Performance Tests

**Benchmarks**:
- Startup time: Measure from application launch to main window visible
- Memory usage: Measure after loading 1000 records
- Record management responsiveness: Sort 1000 records, measure time
- PDF extraction: Extract text from 50-page PDF, measure time

---

## Build & Distribution

### Development Build

```bash
dotnet build
```

- Debug symbols enabled
- Optimizations disabled
- Full error checking
- Verbose logging

### Production Build

```bash
dotnet publish -c Release -r win-x64
```

- Optimizations enabled
- Debug symbols stripped
- Minimal binary size
- IL trimming for smaller output

### Installer Creation

```bash
# Using WiX Toolset
wix build installer.wxs -o dist\

# OR using NSIS (more common for .NET)
makensis installer.nsi
```

**Outputs** (Windows):
- `dist/EtnoPapers-1.0.0-Setup.exe`: NSIS installer (recommended)
- `dist/EtnoPapers-1.0.0.msi`: Windows Installer format
- `dist/EtnoPapers-1.0.0-portable.zip`: Portable version (no installation)

**Installer Features**:
- Install to Program Files
- Start Menu shortcut
- Desktop shortcut (optional)
- Uninstaller
- Check for .NET 8 runtime (install if missing)
- File associations (optional for future)
- Auto-update capability (future)

### Code Signing (Future)

For production releases, sign Windows installer with Authenticode certificate to avoid SmartScreen warnings.

---

## Deployment & Distribution

### Initial Release (v1.0.0)

**Distribution Method**: Manual download from GitHub releases

**Installation Steps for Users**:
1. Download installer (.exe or .msi)
2. Run installer (automatically checks for .NET 8)
3. Launch EtnoPapers
4. Configure OLLAMA (prerequisite: already installed)
5. Configure MongoDB in settings (optional)

### Future Releases

**Auto-Update** (v1.1.0+):
- WinUpdate or custom update mechanism
- Check for updates on startup
- Download in background
- Install on restart
- Notify user of new version

### Release Checklist

- [ ] All tests passing (unit, integration, UI)
- [ ] Performance benchmarks met (startup < 2s, memory < 150MB)
- [ ] Version bumped in AssemblyInfo.cs and .csproj
- [ ] CHANGELOG.md updated
- [ ] README.md updated (if needed)
- [ ] Build Windows installers (.exe and .msi)
- [ ] Test installation on clean Windows 10 machine
- [ ] Test with existing Electron-generated JSON files
- [ ] Create GitHub release with installers attached
- [ ] Update website with download links
- [ ] Announce release to users

---

## Monitoring & Observability

### Logging

**Locations**:
- Application logs: `%APPDATA%/EtnoPapers/logs/`
- Crash reports: `%APPDATA%/EtnoPapers/crashes/`

**Log Rotation**:
- Keep last 7 days
- Max 10 MB per day
- Compress old logs

**Log Access**:
- Users can view logs through Help menu
- Logs included in error reports for support

### Metrics (Optional - Future)

**Privacy-Respecting Metrics** (opt-in):
- Usage patterns (features used, not content)
- Error rates (types, not details)
- Performance metrics (startup times, crash rates)
- No PII collected

### Error Tracking (Optional - Future)

**Sentry Integration** (opt-in):
- Crash reports
- Unhandled exceptions
- Performance monitoring
- User can disable in settings

---

## Maintenance & Support

### Documentation

**For Users** (README.md in Portuguese):
- Installation guide (how to install .NET 8, OLLAMA, the app)
- Usage instructions (identical to Electron version)
- Troubleshooting common issues
- Contact information (edalcin@jbrj.gov.br)

**For Developers**:
- This implementation plan
- Quickstart guide (how to set up dev environment)
- API documentation (XML comments in code)
- Architecture decision records

### Support Channels

- **Email**: edalcin@jbrj.gov.br (primary contact)
- **GitHub Issues**: Bug reports, feature requests
- **In-App Help**: Links to documentation, troubleshooting guides

### Known Limitations (v1.0.0)

- Windows-only installer (no macOS/Linux)
- No OCR for scanned PDFs (unchanged from Electron)
- No batch processing (one PDF at a time)
- No collaborative features
- No data visualization
- No export to other formats (only JSON/MongoDB)

---

## Roadmap (Future Versions)

### v1.1.0 - Polish & Refinement
- Auto-update capability
- Performance optimizations (if benchmarks not met)
- Bug fixes from user feedback
- Enhanced error messages
- Additional language support (Spanish, etc.)

### v1.2.0 - Productivity Features
- Batch PDF processing
- Export to CSV/Excel
- Advanced search and filtering
- Custom field templates
- Saved search filters

### v1.3.0 - Extended Platform Support
- macOS version (via MAUI or Qt)
- Linux AppImage
- Shared MongoDB collections (basic collaboration)

### v2.0.0 - Advanced Features
- OCR for scanned PDFs
- Reference manager integration (Zotero, Mendeley)
- Data visualization dashboard
- Statistical analysis tools
- Mobile companion app

---

## Success Criteria (from Spec)

**Migration Success Metrics**:
- ✅ SC-001: WPF version successfully completes all test cases (100% functional parity)
- ✅ SC-002: Startup time < 2 seconds (vs. Electron ~5-10 seconds)
- ✅ SC-003: Idle memory < 150 MB (vs. Electron ~300-500 MB)
- ✅ SC-004: Users can migrate from Electron to WPF by copying JSON/config files
- ✅ SC-005: WPF handles 1000+ records with operations < 200ms
- ✅ SC-006: 100% of Electron-generated JSON loads without corruption
- ✅ SC-007: WPF version looks and feels like native Windows 10/11 application
- ✅ SC-008: Application installs and runs on Windows 10+ with .NET 8
- ✅ SC-009: PDF extraction output identical between Electron and WPF
- ✅ SC-010: MongoDB sync success rate ≥ 95%

---

## Risks & Mitigation

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| **PDF library quality differs from pdf.js** | High | Medium | Test with diverse PDFs, fallback to manual entry, choose library with good reviews |
| **Performance targets not met** | High | Low | Profiling early, optimization expertise, budget extra time for tuning |
| **JSON compatibility issues** | High | Low | Unit tests for round-trip, test with real Electron data, schema validation |
| **OLLAMA integration breaks** | Medium | Low | Version detection, abstraction layer, fallback prompts, detailed error messages |
| **MongoDB driver version incompatibility** | Medium | Low | Pin stable versions, test with multiple MongoDB versions, abstraction layer |
| **User migration friction** | Medium | Medium | Clear upgrade instructions, data backup before migration, support channel |
| **WPF adoption/hiring challenges** | Low | Low | C# has large developer pool, WPF documentation available, training resources |
| **Performance regression in database access** | Medium | Low | Benchmarking before release, optimize queries, lazy-load data |

---

## Conclusion

This implementation plan provides a comprehensive blueprint for migrating EtnoPapers from Electron to C# WPF while maintaining 100% functional parity. The architecture balances:

- **User needs**: Fast, responsive application that feels native to Windows
- **Technical feasibility**: Proven technologies (.NET 8, WPF, MongoDB driver)
- **Performance goals**: Startup < 2s, memory < 150MB (primary migration benefits)
- **Data integrity**: Identical JSON/MongoDB compatibility, zero data loss
- **Maintainability**: Standard patterns, clear architecture, comprehensive tests
- **Extensibility**: Modular services, plugin-ready extraction pipeline

**Next Steps**:
1. Run `/speckit.tasks` to generate detailed task breakdown for C# WPF
2. Review and prioritize tasks
3. Set up Visual Studio development environment
4. Begin implementation following Phase 0 setup tasks
5. Iterate based on testing and performance benchmarking

**Estimated Timeline**:
- Phase 0-2 (Setup + Core services): 3-4 weeks
- Phase 3-5 (UI + Features): 4-6 weeks
- Phase 6-7 (Settings + Testing): 2-3 weeks
- Phase 8-9 (Release): 1-2 weeks
- **Total**: 10-15 weeks for v1.0.0 with team of 1-2 developers

This plan is living documentation and will be updated as implementation progresses and requirements evolve.

---

**Plan Ready** ✅

Ready for task generation with `/speckit.tasks`.
