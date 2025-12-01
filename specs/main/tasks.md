# Implementation Tasks: EtnoPapers Desktop Application

**Branch**: `main` (single-branch workflow)
**Spec**: [spec.md](./spec.md)
**Plan**: [plan.md](./plan.md)
**Generated**: 2025-12-01

## Task Organization

Tasks are organized by user story priority and implementation phase. Each task includes:
- **Task ID**: Unique identifier (T001, T002, etc.)
- **[P]**: Can be executed in parallel with other [P] tasks (different files)
- **[US#]**: Maps to User Story in spec.md
- **File Path**: Specific location for implementation
- **Acceptance Criteria**: Clear definition of "done"

**Execution Order**: Complete tasks sequentially within each phase. Tasks marked [P] can run in parallel if working on different files.

---

## Phase 0: Project Setup & Infrastructure

**Goal**: Initialize project structure, dependencies, and development environment

### Project Initialization

- [ ] T001 [P] Initialize Node.js project with pnpm in root directory
  - **File**: `package.json`
  - **Acceptance**: `package.json` exists with name "etnopapers", version "1.0.0", type "module"
  - **Commands**: `pnpm init`, configure scripts section

- [ ] T002 [P] Install core dependencies (Electron, React, TypeScript)
  - **File**: `package.json`
  - **Acceptance**: Dependencies installed: electron@28+, react@18, typescript@5.3+
  - **Commands**: `pnpm add electron react react-dom`, `pnpm add -D typescript @types/react @types/react-dom @types/node`

- [ ] T003 [P] Install build tools (Vite, electron-builder)
  - **File**: `package.json`
  - **Acceptance**: Dev dependencies include vite, electron-builder, @vitejs/plugin-react
  - **Commands**: `pnpm add -D vite electron-builder @vitejs/plugin-react`

- [ ] T004 [P] Install UI dependencies (Tailwind CSS, shadcn/ui prerequisites)
  - **File**: `package.json`
  - **Acceptance**: tailwindcss, postcss, autoprefixer, class-variance-authority, clsx installed
  - **Commands**: `pnpm add -D tailwindcss postcss autoprefixer`, `pnpm add class-variance-authority clsx tailwind-merge`

- [ ] T005 [P] Install state management and utilities (Zustand, lowdb, MongoDB driver)
  - **File**: `package.json`
  - **Acceptance**: zustand, lowdb, mongodb, zod, axios, uuid installed
  - **Commands**: `pnpm add zustand lowdb mongodb zod axios uuid`

- [ ] T006 [P] Install PDF processing libraries (pdf.js)
  - **File**: `package.json`
  - **Acceptance**: pdfjs-dist installed
  - **Commands**: `pnpm add pdfjs-dist`

- [ ] T007 [P] Install testing frameworks (Vitest, Testing Library, Playwright)
  - **File**: `package.json`
  - **Acceptance**: vitest, @testing-library/react, @testing-library/user-event, @playwright/test installed
  - **Commands**: `pnpm add -D vitest @testing-library/react @testing-library/jest-dom @testing-library/user-event @playwright/test`

### Configuration Files

- [ ] T008 [P] Create TypeScript configuration
  - **File**: `tsconfig.json`
  - **Acceptance**: Configured for ES2022, strict mode, paths for @/ alias, separate configs for main/renderer
  - **Content**: Target ES2022, module ESNext, strict: true, esModuleInterop: true

- [ ] T009 [P] Create Vite configuration for Electron
  - **File**: `vite.config.ts`
  - **Acceptance**: Configured for Electron renderer process, React plugin, path aliases
  - **Content**: Configure build for renderer, resolve @/ to src/, React plugin

- [ ] T010 [P] Create Vitest configuration
  - **File**: `vitest.config.ts`
  - **Acceptance**: Extends vite.config, configured for jsdom environment, setupFiles
  - **Content**: environment: 'jsdom', setupFiles: './tests/setup.ts'

- [ ] T011 [P] Create Playwright configuration
  - **File**: `playwright.config.ts`
  - **Acceptance**: Configured for Electron app testing, testDir: './tests/e2e'
  - **Content**: Use electron context, baseURL for app

- [ ] T012 [P] Create Tailwind CSS configuration
  - **File**: `tailwind.config.js`
  - **Acceptance**: Content paths include src/, theme extensions for shadcn/ui
  - **Content**: Configure content paths, extend theme with CSS variables

- [ ] T013 [P] Create PostCSS configuration
  - **File**: `postcss.config.js`
  - **Acceptance**: Includes tailwindcss and autoprefixer plugins
  - **Content**: plugins: [tailwindcss, autoprefixer]

- [ ] T014 [P] Create electron-builder configuration
  - **File**: `electron-builder.config.js`
  - **Acceptance**: Configured for Windows installer (NSIS), app metadata, file associations
  - **Content**: appId, productName, win config with target: nsis

- [ ] T015 [P] Create ESLint configuration
  - **File**: `.eslintrc.json`
  - **Acceptance**: TypeScript and React rules configured
  - **Content**: Extends typescript-eslint, react recommended

- [ ] T016 [P] Create Prettier configuration
  - **File**: `.prettierrc`
  - **Acceptance**: Consistent formatting rules (semi, singleQuote, etc.)
  - **Content**: Configure code style preferences

- [ ] T017 [P] Create .gitignore
  - **File**: `.gitignore`
  - **Acceptance**: Ignores node_modules/, dist/, .env, logs/, coverage/
  - **Content**: Standard Node.js, Electron, build artifacts

### Directory Structure

- [ ] T018 [P] Create source directory structure
  - **Files**: `src/main/`, `src/renderer/`, `src/shared/`, `src/preload/`
  - **Acceptance**: All directories created, empty index files in each
  - **Commands**: `mkdir -p src/{main,renderer,shared,preload}/{services,components,types,utils}`

- [ ] T019 [P] Create test directory structure
  - **Files**: `tests/unit/`, `tests/integration/`, `tests/e2e/`, `tests/fixtures/`
  - **Acceptance**: All test directories created
  - **Commands**: `mkdir -p tests/{unit,integration,e2e,fixtures}`

- [ ] T020 [P] Create resources directory structure
  - **Files**: `resources/icons/`, `resources/installers/`, `resources/locales/`
  - **Acceptance**: Resources directories for assets, installers, translations
  - **Commands**: `mkdir -p resources/{icons,installers,locales}`

- [ ] T021 [P] Create global styles and CSS setup
  - **File**: `src/renderer/index.css`
  - **Acceptance**: Tailwind directives, CSS variables for shadcn/ui theming
  - **Content**: @tailwind base/components/utilities, :root CSS variables

### Package Scripts

- [ ] T022 Create npm scripts in package.json
  - **File**: `package.json` scripts section
  - **Acceptance**: Scripts for dev, build, test, lint, format, dist
  - **Scripts**: "dev", "build", "test", "test:e2e", "lint", "format", "dist"

---

## Phase 1: Core Services & Data Layer (User Story 4 & 1 - P1)

**Goal**: Implement configuration, PDF processing, AI integration, and data storage services

### Shared Types & Utilities

- [ ] T023 [P] [US4] Define core TypeScript interfaces
  - **File**: `src/shared/types/article.ts`
  - **Acceptance**: ArticleRecord, PlantSpecies, Community interfaces match data-model.md
  - **Content**: Export all data model types from specs/main/data-model.md

- [ ] T024 [P] [US4] Define configuration types
  - **File**: `src/shared/types/config.ts`
  - **Acceptance**: AppConfiguration, OLLAMAConfig, MongoDBConfig, StorageConfig interfaces
  - **Content**: Match config structure from data-model.md

- [ ] T025 [P] [US4] Define service interface types
  - **File**: `src/shared/types/services.ts`
  - **Acceptance**: All service interfaces from contracts/service-interfaces.ts
  - **Content**: Copy interfaces from specs/main/contracts/service-interfaces.ts

- [ ] T026 [P] [US4] Define error types
  - **File**: `src/shared/types/errors.ts`
  - **Acceptance**: Custom error classes for PDF, OLLAMA, Storage, MongoDB, Extraction
  - **Content**: Error classes with codes matching service-interfaces.ts

- [ ] T027 [P] [US1] Implement title normalization utility
  - **File**: `src/shared/utils/titleNormalizer.ts`
  - **Acceptance**: Converts titles to proper case, preserves acronyms, handles particles
  - **Test**: Unit tests in `tests/unit/utils/titleNormalizer.test.ts`

- [ ] T028 [P] [US1] Implement APA author formatter utility
  - **File**: `src/shared/utils/authorFormatter.ts`
  - **Acceptance**: Formats author names to APA style (LastName, F.I.)
  - **Test**: Unit tests cover name variations, particles, suffixes

- [ ] T029 [P] [US1] Implement language detection utility
  - **File**: `src/shared/utils/languageDetector.ts`
  - **Acceptance**: Detects Portuguese, English, Spanish from text samples
  - **Test**: Unit tests with sample text in different languages

### Configuration Service

- [ ] T030 [US4] Implement Configuration Service (main process)
  - **File**: `src/main/services/ConfigurationService.ts`
  - **Acceptance**: Implements IConfigurationService, uses electron-store, validates config
  - **Methods**: load(), save(), get(), update(), reset()
  - **Test**: Unit tests in `tests/unit/services/ConfigurationService.test.ts`

- [ ] T031 [US4] Create configuration IPC handlers
  - **File**: `src/main/ipc/configHandlers.ts`
  - **Acceptance**: Exposes config operations via IPC (get, update, reset)
  - **Channels**: 'config:get', 'config:update', 'config:reset'

- [ ] T032 [US4] Add configuration API to preload script
  - **File**: `src/preload/index.ts`
  - **Acceptance**: Exposes configAPI to renderer (get, update, reset methods)
  - **Security**: Uses contextBridge.exposeInMainWorld

### PDF Processing Service

- [ ] T033 [US1] Implement PDF Processing Service (main process)
  - **File**: `src/main/services/PDFProcessingService.ts`
  - **Acceptance**: Implements IPDFProcessingService, extracts text with pdf.js, gets metadata
  - **Methods**: extractText(), getMetadata(), validatePDF()
  - **Test**: Integration tests with fixture PDFs in `tests/integration/pdfProcessing.test.ts`

- [ ] T034 [US1] Create PDF IPC handlers
  - **File**: `src/main/ipc/pdfHandlers.ts`
  - **Acceptance**: Exposes PDF operations via IPC
  - **Channels**: 'pdf:extractText', 'pdf:getMetadata', 'pdf:validate'

- [ ] T035 [US1] Add PDF API to preload script
  - **File**: `src/preload/index.ts`
  - **Acceptance**: Exposes pdfAPI to renderer
  - **Methods**: extractText(), getMetadata(), validatePDF()

### OLLAMA Service

- [ ] T036 [US1] Implement OLLAMA Service (main process)
  - **File**: `src/main/services/OLLAMAService.ts`
  - **Acceptance**: Implements IOLLAMAService, calls OLLAMA REST API, parses responses
  - **Methods**: checkHealth(), extractMetadata(), translateToPortuguese(), getAvailableModels()
  - **Test**: Unit tests with mocked axios in `tests/unit/services/OLLAMAService.test.ts`

- [ ] T037 [US1] Create OLLAMA IPC handlers
  - **File**: `src/main/ipc/ollamaHandlers.ts`
  - **Acceptance**: Exposes OLLAMA operations via IPC
  - **Channels**: 'ollama:checkHealth', 'ollama:extract', 'ollama:translate'

- [ ] T038 [US1] Add OLLAMA API to preload script
  - **File**: `src/preload/index.ts`
  - **Acceptance**: Exposes ollamaAPI to renderer
  - **Methods**: checkHealth(), extractMetadata(), translateToPortuguese()

### Data Storage Service

- [ ] T039 [P] [US1] Implement Data Storage Service (main process)
  - **File**: `src/main/services/DataStorageService.ts`
  - **Acceptance**: Implements IDataStorageService, uses lowdb, CRUD operations
  - **Methods**: initialize(), getAll(), getById(), create(), update(), delete(), count()
  - **Test**: Integration tests in `tests/integration/storageOperations.test.ts`

- [ ] T040 [US1] Create storage IPC handlers
  - **File**: `src/main/ipc/storageHandlers.ts`
  - **Acceptance**: Exposes storage operations via IPC
  - **Channels**: 'storage:getAll', 'storage:getById', 'storage:create', 'storage:update', 'storage:delete'

- [ ] T041 [US1] Add storage API to preload script
  - **File**: `src/preload/index.ts`
  - **Acceptance**: Exposes storageAPI to renderer
  - **Methods**: getAll(), getById(), create(), update(), delete()

### Validation Service

- [ ] T042 [P] [US1] Implement Validation Service (main process)
  - **File**: `src/main/services/ValidationService.ts`
  - **Acceptance**: Implements IValidationService, uses Zod schemas
  - **Methods**: validateRecord(), validateExtractedData(), checkMandatoryFields()
  - **Test**: Unit tests with valid/invalid data samples

- [ ] T043 [P] [US1] Create Zod validation schemas
  - **File**: `src/shared/validation/schemas.ts`
  - **Acceptance**: Zod schemas for ArticleRecord, PlantSpecies, Community matching data-model.md
  - **Content**: Export ArticleRecordSchema, PlantSpeciesSchema, CommunitySchema

### Extraction Pipeline Service

- [ ] T044 [US1] Implement Extraction Pipeline Service (orchestration)
  - **File**: `src/main/services/ExtractionPipelineService.ts`
  - **Acceptance**: Implements IExtractionPipelineService, orchestrates PDF → text → AI → validation → storage
  - **Methods**: extractFromPDF(), cancelExtraction(), getExtractionStatus()
  - **Test**: Integration tests full pipeline in `tests/integration/extractionPipeline.test.ts`

- [ ] T045 [US1] Create extraction IPC handlers with progress events
  - **File**: `src/main/ipc/extractionHandlers.ts`
  - **Acceptance**: Exposes extraction with progress callbacks
  - **Channels**: 'extraction:start', 'extraction:cancel', 'extraction:status'
  - **Events**: 'extraction:progress' for real-time updates

- [ ] T046 [US1] Add extraction API to preload script
  - **File**: `src/preload/index.ts`
  - **Acceptance**: Exposes extractionAPI with event listeners
  - **Methods**: startExtraction(), cancelExtraction(), onProgress()

### Logger Service

- [ ] T047 [P] Implement Logger Service (main process)
  - **File**: `src/main/services/LoggerService.ts`
  - **Acceptance**: Implements ILoggerService, uses winston, file rotation
  - **Methods**: debug(), info(), warn(), error(), getLogFilePath()
  - **Test**: Unit tests verify log file creation and rotation

### Electron Main Process Setup

- [ ] T048 Create Electron main process entry point
  - **File**: `src/main/index.ts`
  - **Acceptance**: Creates BrowserWindow, loads renderer, registers IPC handlers, handles app lifecycle
  - **Content**: app.whenReady(), createWindow(), register all IPC handlers

- [ ] T049 Finalize preload script
  - **File**: `src/preload/index.ts`
  - **Acceptance**: All APIs exposed, type definitions for window object
  - **Content**: Complete contextBridge with all service APIs

---

## Phase 2: User Interface & Components (User Story 4 & 1 - P1 MVP)

**Goal**: Build React UI for configuration, PDF upload, extraction, and result display

### UI Foundation

- [ ] T050 [P] Create React app entry point
  - **File**: `src/renderer/main.tsx`
  - **Acceptance**: Renders App component, includes global styles
  - **Content**: ReactDOM.render(<App />), import index.css

- [ ] T051 [P] Create root App component with routing
  - **File**: `src/renderer/App.tsx`
  - **Acceptance**: Router setup, layout with sidebar, main content area, status bar
  - **Content**: React Router, layout structure

- [ ] T052 [P] Create Zustand app store
  - **File**: `src/renderer/stores/useAppStore.ts`
  - **Acceptance**: Global state for connections (OLLAMA, MongoDB), config, notifications
  - **State**: ollamaConnected, mongoConnected, config, notifications[]

- [ ] T053 [P] Create Zustand extraction store
  - **File**: `src/renderer/stores/useExtractionStore.ts`
  - **Acceptance**: State for current extraction progress, status, results
  - **State**: isExtracting, progress, currentStep, extractedData, error

### Common UI Components

- [ ] T054 [P] Create Button component
  - **File**: `src/renderer/components/common/Button.tsx`
  - **Acceptance**: Reusable button with variants (primary, secondary, destructive), disabled state
  - **Test**: Component test in `tests/unit/components/Button.test.tsx`

- [ ] T055 [P] Create Input component
  - **File**: `src/renderer/components/common/Input.tsx`
  - **Acceptance**: Text input with label, error state, validation
  - **Test**: Component test for rendering and onChange

- [ ] T056 [P] Create Select component
  - **File**: `src/renderer/components/common/Select.tsx`
  - **Acceptance**: Dropdown select with options, onChange handler
  - **Test**: Component test for selection

- [ ] T057 [P] Create Toast notification component
  - **File**: `src/renderer/components/common/Toast.tsx`
  - **Acceptance**: Toast notifications (success, error, warning, info), auto-dismiss
  - **Test**: Component test for variants and dismissal

- [ ] T058 [P] Create Modal component
  - **File**: `src/renderer/components/common/Modal.tsx`
  - **Acceptance**: Reusable modal dialog with header, body, footer, close button
  - **Test**: Component test for open/close

### Layout Components

- [ ] T059 [P] Create Sidebar component
  - **File**: `src/renderer/components/layout/Sidebar.tsx`
  - **Acceptance**: Navigation links (Home, Upload, Records, Settings, About), active state
  - **Test**: Component test for navigation

- [ ] T060 [P] Create StatusBar component
  - **File**: `src/renderer/components/layout/StatusBar.tsx`
  - **Acceptance**: Displays OLLAMA and MongoDB connection status with indicators
  - **Test**: Component test for connection states

- [ ] T061 [P] Create Header component
  - **File**: `src/renderer/components/layout/Header.tsx`
  - **Acceptance**: App title/logo, current page indicator
  - **Test**: Component test for rendering

### Settings Page (User Story 4 - P1)

- [ ] T062 [US4] Create Settings Page component
  - **File**: `src/renderer/pages/SettingsPage.tsx`
  - **Acceptance**: Form for OLLAMA config (URL, model, prompt), MongoDB URI, test connection buttons
  - **Test**: Component test for form submission

- [ ] T063 [US4] Implement OLLAMA configuration section
  - **File**: `src/renderer/components/settings/OLLAMAConfig.tsx`
  - **Acceptance**: Input fields for URL, model, prompt; test connection button shows status
  - **Test**: Component test for connection testing

- [ ] T064 [US4] Implement MongoDB configuration section
  - **File**: `src/renderer/components/settings/MongoDBConfig.tsx`
  - **Acceptance**: Input for URI, database, collection; test connection button
  - **Test**: Component test for validation and testing

- [ ] T065 [US4] Implement storage configuration section
  - **File**: `src/renderer/components/settings/StorageConfig.tsx`
  - **Acceptance**: Display max records limit, current count, utilization %
  - **Test**: Component test for display

- [ ] T066 [US4] Create connection status hook
  - **File**: `src/renderer/hooks/useConnections.ts`
  - **Acceptance**: Custom hook checks OLLAMA and MongoDB status on mount, updates store
  - **Test**: Hook test with mocked APIs

### Upload Page (User Story 1 - P1)

- [ ] T067 [US1] Create Upload Page component
  - **File**: `src/renderer/pages/UploadPage.tsx`
  - **Acceptance**: File drop zone, upload button (disabled if OLLAMA disconnected), extraction progress display
  - **Test**: Component test for rendering and disabled state

- [ ] T068 [US1] Create File Drop Zone component
  - **File**: `src/renderer/components/upload/FileDropZone.tsx`
  - **Acceptance**: Drag-and-drop area, file selection button, PDF validation
  - **Test**: Component test for file selection and validation

- [ ] T069 [US1] Create Extraction Progress component
  - **File**: `src/renderer/components/upload/ExtractionProgress.tsx`
  - **Acceptance**: Progress bar, current step indicator, cancel button
  - **Test**: Component test for progress updates

- [ ] T070 [US1] Create Extraction Results component
  - **File**: `src/renderer/components/upload/ExtractionResults.tsx`
  - **Acceptance**: Displays extracted data in editable form, save button
  - **Test**: Component test for data display and editing

- [ ] T071 [US1] Create extraction hook
  - **File**: `src/renderer/hooks/useExtraction.ts`
  - **Acceptance**: Custom hook manages extraction workflow, updates store
  - **Methods**: startExtraction(), cancelExtraction(), saveResults()
  - **Test**: Hook test with mocked extraction API

### About Page

- [ ] T072 Create About Page component
  - **File**: `src/renderer/pages/AboutPage.tsx`
  - **Acceptance**: Displays app name, version, author (Eduardo Dalcin), institution, email
  - **Test**: Component test for information display

### Home Page

- [ ] T073 Create Home Page component
  - **File**: `src/renderer/pages/HomePage.tsx`
  - **Acceptance**: Welcome message, quick start guide, connection status summary
  - **Test**: Component test for rendering

---

## Phase 3: Record Management (User Story 2 - P2)

**Goal**: Implement CRUD interface for managing local records

### Records Store & Hooks

- [ ] T074 [US2] Create Zustand records store
  - **File**: `src/renderer/stores/useRecordsStore.ts`
  - **Acceptance**: State for records list, selected IDs, filters, loading state
  - **State**: records[], selectedIds[], filters, isLoading
  - **Actions**: setRecords(), addRecord(), updateRecord(), deleteRecord(), toggleSelection()

- [ ] T075 [US2] Create records management hook
  - **File**: `src/renderer/hooks/useRecords.ts`
  - **Acceptance**: Custom hook for CRUD operations, loads records on mount
  - **Methods**: loadRecords(), createRecord(), editRecord(), deleteRecord(), deleteMultiple()
  - **Test**: Hook test with mocked storage API

### Records Page Components

- [ ] T076 [US2] Create Records Page component
  - **File**: `src/renderer/pages/RecordsPage.tsx`
  - **Acceptance**: Grid/list view of records, search/filter, bulk actions toolbar
  - **Test**: Component test for rendering records

- [ ] T077 [US2] Create Record Card component
  - **File**: `src/renderer/components/records/RecordCard.tsx`
  - **Acceptance**: Displays title, authors, year, actions (edit, delete, select)
  - **Test**: Component test for card display and interactions

- [ ] T078 [US2] Create Record Grid component
  - **File**: `src/renderer/components/records/RecordGrid.tsx`
  - **Acceptance**: Virtual scrolling grid of RecordCard components
  - **Test**: Component test for virtual scrolling

- [ ] T079 [US2] Create Record Filters component
  - **File**: `src/renderer/components/records/RecordFilters.tsx`
  - **Acceptance**: Filter by year range, author, biome, species presence
  - **Test**: Component test for filter application

- [ ] T080 [US2] Create Record Form component
  - **File**: `src/renderer/components/records/RecordForm.tsx`
  - **Acceptance**: Editable form for all fields, custom attribute addition, validation
  - **Test**: Component test for form submission and validation

- [ ] T081 [US2] Implement create record modal
  - **File**: `src/renderer/components/records/CreateRecordModal.tsx`
  - **Acceptance**: Modal with empty RecordForm, creates new record on save
  - **Test**: Component test for record creation

- [ ] T082 [US2] Implement edit record modal
  - **File**: `src/renderer/components/records/EditRecordModal.tsx`
  - **Acceptance**: Modal with pre-filled RecordForm, updates record on save
  - **Test**: Component test for record update

- [ ] T083 [US2] Implement delete confirmation modal
  - **File**: `src/renderer/components/records/DeleteConfirmModal.tsx`
  - **Acceptance**: Confirmation dialog before deletion, shows record count
  - **Test**: Component test for confirmation flow

---

## Phase 4: Cloud Synchronization (User Story 3 - P3)

**Goal**: Implement MongoDB synchronization with batch upload and local deletion

### MongoDB Service

- [ ] T084 [US3] Implement MongoDB Sync Service (main process)
  - **File**: `src/main/services/MongoDBSyncService.ts`
  - **Acceptance**: Implements IMongoDBSyncService, connects to MongoDB, batch uploads
  - **Methods**: testConnection(), uploadRecord(), uploadBatch(), getStatus()
  - **Test**: Integration tests with test MongoDB in `tests/integration/mongodbSync.test.ts`

- [ ] T085 [US3] Create MongoDB sync IPC handlers
  - **File**: `src/main/ipc/syncHandlers.ts`
  - **Acceptance**: Exposes sync operations via IPC
  - **Channels**: 'sync:testConnection', 'sync:uploadBatch', 'sync:getStatus'

- [ ] T086 [US3] Add sync API to preload script
  - **File**: `src/preload/index.ts`
  - **Acceptance**: Exposes syncAPI to renderer
  - **Methods**: testConnection(), uploadRecords(), onProgress()

### Sync UI Components

- [ ] T087 [US3] Create Sync Panel component
  - **File**: `src/renderer/components/sync/SyncPanel.tsx`
  - **Acceptance**: Shows selected records, sync button, connection status
  - **Test**: Component test for sync initiation

- [ ] T088 [US3] Create Sync Progress component
  - **File**: `src/renderer/components/sync/SyncProgress.tsx`
  - **Acceptance**: Progress bar, per-record status (success/failed), cancel button
  - **Test**: Component test for progress display

- [ ] T089 [US3] Add sync functionality to Records Page
  - **File**: `src/renderer/pages/RecordsPage.tsx` (update)
  - **Acceptance**: Checkbox selection, "Sync Selected" button in toolbar, opens SyncPanel
  - **Test**: Integration test for sync workflow

- [ ] T090 [US3] Implement sync reminder notification
  - **File**: `src/renderer/components/SyncReminder.tsx`
  - **Acceptance**: Shows notification if records count > threshold or X days since last sync
  - **Test**: Component test for reminder conditions

---

## Phase 5: Testing & Quality Assurance

**Goal**: Comprehensive test coverage and quality validation

### Unit Tests

- [ ] T091 [P] Write unit tests for title normalization
  - **File**: `tests/unit/utils/titleNormalizer.test.ts`
  - **Acceptance**: Test cases for proper case, acronyms, particles, edge cases
  - **Coverage**: >90% for titleNormalizer.ts

- [ ] T092 [P] Write unit tests for APA author formatting
  - **File**: `tests/unit/utils/authorFormatter.test.ts`
  - **Acceptance**: Test cases for single/multiple names, particles, suffixes
  - **Coverage**: >90% for authorFormatter.ts

- [ ] T093 [P] Write unit tests for language detection
  - **File**: `tests/unit/utils/languageDetector.test.ts`
  - **Acceptance**: Test samples in PT, EN, ES correctly identified
  - **Coverage**: >85% for languageDetector.ts

- [ ] T094 [P] Write unit tests for Validation Service
  - **File**: `tests/unit/services/ValidationService.test.ts`
  - **Acceptance**: Test valid/invalid records, mandatory field checks
  - **Coverage**: >85% for ValidationService.ts

- [ ] T095 [P] Write unit tests for Logger Service
  - **File**: `tests/unit/services/LoggerService.test.ts`
  - **Acceptance**: Test log levels, file creation, rotation
  - **Coverage**: >80% for LoggerService.ts

### Integration Tests

- [ ] T096 Write integration test for PDF extraction
  - **File**: `tests/integration/pdfProcessing.test.ts`
  - **Acceptance**: Test with real PDF fixtures, verify text extraction
  - **Fixtures**: Include test PDFs in `tests/fixtures/`

- [ ] T097 Write integration test for extraction pipeline
  - **File**: `tests/integration/extractionPipeline.test.ts`
  - **Acceptance**: End-to-end test: PDF → text → AI (mocked) → validation → storage
  - **Mocks**: Mock OLLAMA responses

- [ ] T098 Write integration test for storage operations
  - **File**: `tests/integration/storageOperations.test.ts`
  - **Acceptance**: Test CRUD operations, file persistence, limit enforcement
  - **Setup**: Use temp directory for test JSON files

- [ ] T099 Write integration test for MongoDB sync
  - **File**: `tests/integration/mongodbSync.test.ts`
  - **Acceptance**: Test with MongoDB memory server, verify upload and deletion
  - **Setup**: Use mongodb-memory-server for isolated testing

### E2E Tests

- [ ] T100 Write E2E test for configuration workflow
  - **File**: `tests/e2e/configuration.spec.ts`
  - **Acceptance**: Test setting OLLAMA config, MongoDB URI, connection testing
  - **Playwright**: Full app launch and interaction

- [ ] T101 Write E2E test for PDF upload and extraction
  - **File**: `tests/e2e/uploadAndExtract.spec.ts`
  - **Acceptance**: Test file selection, extraction progress, result editing, save
  - **Setup**: Mock OLLAMA service or use test instance

- [ ] T102 Write E2E test for record management
  - **File**: `tests/e2e/manageRecords.spec.ts`
  - **Acceptance**: Test create, view, edit, delete records
  - **Verification**: Check local JSON file for changes

- [ ] T103 Write E2E test for MongoDB synchronization
  - **File**: `tests/e2e/syncToMongoDB.spec.ts`
  - **Acceptance**: Test record selection, sync, verify local deletion
  - **Setup**: Use test MongoDB instance

### Component Tests

- [ ] T104 [P] Write component tests for all common components
  - **Files**: `tests/unit/components/Button.test.tsx`, `Input.test.tsx`, etc.
  - **Acceptance**: Test rendering, props, user interactions for Button, Input, Select, Toast, Modal
  - **Coverage**: 100% for common components

- [ ] T105 [P] Write component tests for layout components
  - **Files**: `tests/unit/components/layout/*.test.tsx`
  - **Acceptance**: Test Sidebar, StatusBar, Header rendering and behavior
  - **Coverage**: >85% for layout components

- [ ] T106 Write component tests for Settings Page
  - **File**: `tests/unit/pages/SettingsPage.test.tsx`
  - **Acceptance**: Test configuration form, connection testing, save
  - **Mocks**: Mock config API

- [ ] T107 Write component tests for Upload Page
  - **File**: `tests/unit/pages/UploadPage.test.tsx`
  - **Acceptance**: Test file upload, progress display, result editing
  - **Mocks**: Mock extraction API

- [ ] T108 Write component tests for Records Page
  - **File**: `tests/unit/pages/RecordsPage.test.tsx`
  - **Acceptance**: Test record display, filtering, selection, CRUD actions
  - **Mocks**: Mock storage API

---

## Phase 6: Polish, Error Handling & UX Refinements

**Goal**: Production-ready error handling, notifications, and user experience improvements

### Error Handling

- [ ] T109 Implement global error boundary for React
  - **File**: `src/renderer/components/ErrorBoundary.tsx`
  - **Acceptance**: Catches React errors, displays fallback UI, logs errors
  - **Test**: Component test with error-throwing child

- [ ] T110 Implement error notification system
  - **File**: `src/renderer/services/NotificationService.ts`
  - **Acceptance**: Centralized notification management, error → user-friendly messages
  - **Methods**: success(), error(), warning(), info()

- [ ] T111 Add error handling to all service calls
  - **Files**: All `src/renderer/hooks/*.ts`
  - **Acceptance**: Try-catch blocks, user-friendly error messages, logging
  - **Pattern**: Consistent error handling pattern across all hooks

- [ ] T112 Implement retry logic for OLLAMA calls
  - **File**: `src/main/services/OLLAMAService.ts` (update)
  - **Acceptance**: Retry up to 3 times on network errors with exponential backoff
  - **Test**: Unit test for retry behavior

- [ ] T113 Implement retry logic for MongoDB operations
  - **File**: `src/main/services/MongoDBSyncService.ts` (update)
  - **Acceptance**: Retry transient failures, keep records local on persistent failure
  - **Test**: Unit test for retry and failure handling

### UX Improvements

- [ ] T114 Add loading states to all async operations
  - **Files**: All page components
  - **Acceptance**: Spinner/skeleton during loading, disabled buttons during operations
  - **Pattern**: Consistent loading UI pattern

- [ ] T115 Add empty states for Records Page
  - **File**: `src/renderer/components/records/EmptyState.tsx`
  - **Acceptance**: Helpful message when no records, call-to-action to upload PDF
  - **Test**: Component test for rendering

- [ ] T116 Implement keyboard shortcuts
  - **File**: `src/renderer/hooks/useKeyboardShortcuts.ts`
  - **Acceptance**: Ctrl+N (new record), Ctrl+S (save), Ctrl+F (search)
  - **Test**: Hook test for shortcut handlers

- [ ] T117 Add confirmation dialogs for destructive actions
  - **Files**: Records Page, Settings Page
  - **Acceptance**: Confirm before delete record(s), reset config
  - **Pattern**: Use Modal component consistently

- [ ] T118 Implement search/filter debouncing
  - **File**: `src/renderer/hooks/useDebounce.ts`
  - **Acceptance**: Custom debounce hook, 300ms delay
  - **Test**: Hook test for debounce timing

- [ ] T119 Add tooltips for UI elements
  - **File**: `src/renderer/components/common/Tooltip.tsx`
  - **Acceptance**: Reusable tooltip component, accessible
  - **Test**: Component test for show/hide

### Performance Optimizations

- [ ] T120 Implement virtual scrolling for record list
  - **File**: `src/renderer/components/records/RecordGrid.tsx` (update)
  - **Acceptance**: Use react-window or similar, render only visible items
  - **Test**: Performance test with 1000+ records

- [ ] T121 Optimize PDF processing with streaming
  - **File**: `src/main/services/PDFProcessingService.ts` (update)
  - **Acceptance**: Stream large PDFs, process page-by-page
  - **Test**: Integration test with large PDF (100+ pages)

- [ ] T122 Add auto-save for extraction results
  - **File**: `src/renderer/pages/UploadPage.tsx` (update)
  - **Acceptance**: Auto-save edited extraction results every 30 seconds
  - **Pattern**: Debounced auto-save

---

## Phase 7: Internationalization & Localization

**Goal**: Portuguese and English language support

### i18n Setup

- [ ] T123 [P] Install and configure react-i18next
  - **File**: `package.json`, `src/renderer/i18n/config.ts`
  - **Acceptance**: i18next configured, language detection, fallback to pt-BR
  - **Commands**: `pnpm add react-i18next i18next`

- [ ] T124 [P] Create Portuguese translation file
  - **File**: `resources/locales/pt-BR.json`
  - **Acceptance**: All UI strings translated to Brazilian Portuguese
  - **Content**: JSON with all text keys and translations

- [ ] T125 [P] Create English translation file
  - **File**: `resources/locales/en-US.json`
  - **Acceptance**: All UI strings in English
  - **Content**: JSON with all text keys and translations

- [ ] T126 Add language switcher to Settings Page
  - **File**: `src/renderer/pages/SettingsPage.tsx` (update)
  - **Acceptance**: Dropdown to select pt-BR or en-US, persists in config
  - **Test**: Component test for language switching

- [ ] T127 Wrap all UI text with i18n translation
  - **Files**: All component files
  - **Acceptance**: Use `t()` function for all user-facing text
  - **Pattern**: Consistent translation key naming

---

## Phase 8: Build, Distribution & Documentation

**Goal**: Production build, Windows installer, and user documentation

### Build Configuration

- [ ] T128 Configure production build optimization
  - **File**: `vite.config.ts` (update)
  - **Acceptance**: Minification, tree-shaking, source maps for production
  - **Config**: build.minify, build.sourcemap, build.rollupOptions

- [ ] T129 Configure electron-builder for Windows
  - **File**: `electron-builder.config.js` (update)
  - **Acceptance**: NSIS installer config, app metadata, icon, file associations
  - **Content**: win.target, win.icon, files to include/exclude

- [ ] T130 [P] Create application icons
  - **Files**: `resources/icons/icon.ico`, `icon.png` (multiple sizes)
  - **Acceptance**: Windows ICO file (16x16 to 256x256), PNG variants
  - **Tools**: Use icon generator or design tool

### Testing & Validation

- [ ] T131 Run full test suite and fix failing tests
  - **Command**: `pnpm test`
  - **Acceptance**: All unit, integration, component tests passing
  - **Coverage**: Overall coverage >80%

- [ ] T132 Run E2E test suite
  - **Command**: `pnpm test:e2e`
  - **Acceptance**: All E2E tests passing on Windows 10/11
  - **Environment**: Test on clean Windows install

- [ ] T133 Run linting and fix issues
  - **Command**: `pnpm lint`
  - **Acceptance**: No ESLint errors, warnings addressed or suppressed with justification
  - **Pattern**: Consistent code style

- [ ] T134 Run type checking
  - **Command**: `pnpm type-check`
  - **Acceptance**: No TypeScript errors
  - **Verification**: tsc --noEmit passes

### Build & Distribution

- [ ] T135 Build production version
  - **Command**: `pnpm build`
  - **Acceptance**: Builds successfully, output in dist/
  - **Verification**: Manual smoke test of built app

- [ ] T136 Create Windows installer
  - **Command**: `pnpm dist`
  - **Acceptance**: Generates .exe and .msi installers in dist/
  - **Test**: Install on clean Windows machine

- [ ] T137 Test installer on Windows 10 and Windows 11
  - **Acceptance**: Installs successfully, creates shortcuts, app launches
  - **Environment**: Test on multiple Windows versions

### Documentation

- [ ] T138 Update README.md (Portuguese)
  - **File**: `README.md` (already created, verify completeness)
  - **Acceptance**: Installation instructions, usage guide, screenshots, troubleshooting
  - **Content**: Verify all sections are complete and accurate

- [ ] T139 Create CHANGELOG.md
  - **File**: `CHANGELOG.md`
  - **Acceptance**: Version 1.0.0 entry with all features
  - **Format**: Keep a Changelog format

- [ ] T140 Create user guide documentation
  - **File**: `docs/USER_GUIDE.md` (Portuguese)
  - **Acceptance**: Step-by-step guide for all features, screenshots
  - **Content**: Configuration, upload, extraction, records, sync

- [ ] T141 Create developer documentation
  - **File**: `docs/DEVELOPER.md`
  - **Acceptance**: Architecture overview, setup guide, contribution guidelines
  - **Content**: Link to quickstart.md, architecture diagrams

---

## Phase 9: Final Validation & Release Preparation

**Goal**: Pre-release validation and deployment readiness

### Pre-Release Checklist

- [ ] T142 Verify all functional requirements (FR-001 to FR-041)
  - **Acceptance**: Manual testing of each FR from spec.md
  - **Documentation**: Checklist tracking each FR tested

- [ ] T143 Verify all user stories and acceptance scenarios
  - **Acceptance**: Manual testing of all acceptance scenarios from spec.md
  - **Documentation**: Checklist tracking each scenario

- [ ] T144 Verify edge cases handling
  - **Acceptance**: Test all 10 edge cases from spec.md
  - **Documentation**: Results for each edge case

- [ ] T145 Performance validation
  - **Acceptance**: Verify performance goals (extraction <2min, UI <100ms, storage <50ms)
  - **Tools**: Performance profiling, timing measurements

- [ ] T146 Security validation
  - **Acceptance**: Verify no PDFs stored, config encryption, input validation
  - **Review**: Security checklist from plan.md

- [ ] T147 Accessibility validation
  - **Acceptance**: Keyboard navigation works, screen reader compatible
  - **Tools**: Lighthouse, axe DevTools

### Release Artifacts

- [ ] T148 Create release notes
  - **File**: `docs/RELEASE_NOTES_v1.0.0.md`
  - **Acceptance**: Features, known limitations, upgrade instructions
  - **Format**: Markdown with clear sections

- [ ] T149 Prepare GitHub release
  - **Acceptance**: Tag v1.0.0, release notes, attach installers
  - **Artifacts**: .exe, .msi, checksums

- [ ] T150 Create installation troubleshooting guide
  - **File**: `docs/TROUBLESHOOTING.md` (Portuguese)
  - **Acceptance**: Common issues and solutions, FAQ
  - **Content**: OLLAMA setup, MongoDB connection, Windows firewall

---

## Task Summary

**Total Tasks**: 150

### By Phase:
- **Phase 0** (Setup & Infrastructure): 22 tasks
- **Phase 1** (Core Services & Data): 26 tasks
- **Phase 2** (UI & Components - MVP): 24 tasks
- **Phase 3** (Record Management): 10 tasks
- **Phase 4** (Cloud Sync): 7 tasks
- **Phase 5** (Testing & QA): 18 tasks
- **Phase 6** (Polish & Error Handling): 14 tasks
- **Phase 7** (i18n): 5 tasks
- **Phase 8** (Build & Distribution): 14 tasks
- **Phase 9** (Final Validation): 10 tasks

### By User Story:
- **US1** (Extract Metadata): 28 tasks
- **US2** (Manage Records): 10 tasks
- **US3** (Cloud Sync): 7 tasks
- **US4** (Configuration): 11 tasks
- **Infrastructure & Testing**: 94 tasks

### Estimated Effort:
- **Setup & Core Services**: 3-4 weeks
- **UI & MVP Features**: 3-4 weeks
- **Record Management & Sync**: 2-3 weeks
- **Testing & Polish**: 2-3 weeks
- **Documentation & Release**: 1 week
- **Total**: 11-15 weeks

---

## Next Steps

1. **Review this task list** with the team/stakeholders
2. **Prioritize** any adjustments or additional tasks
3. **Set up development environment** using `specs/main/quickstart.md`
4. **Begin implementation** with Phase 0 (Project Setup)
5. **Run `/speckit.implement`** when ready to start guided implementation

---

## Implementation Notes

- **Parallel Execution**: Tasks marked [P] can be worked on simultaneously by different team members
- **Testing**: Write tests alongside implementation (TDD encouraged)
- **Code Review**: Each completed task should be reviewed before moving to dependent tasks
- **Documentation**: Update inline documentation (JSDoc) as you implement
- **Git Commits**: Commit after each task completion with message format: "feat(T###): [description]"

**Ready for implementation!** 🚀
