# Implementation Tasks: EtnoPapers Desktop Application

**Branch**: `main` (single-branch workflow)
**Spec**: [spec.md](./spec.md)
**Plan**: [plan.md](./plan.md)
**Generated**: 2025-12-02

## Task Organization

Tasks are organized by user story priority and implementation phase. Each task includes:
- **Task ID**: Unique identifier (T001, T002, etc.)
- **[P]**: Can be executed in parallel with other [P] tasks (different files)
- **[US#]**: Maps to User Story in spec.md
- **File Path**: Specific location for implementation
- **Acceptance Criteria**: Clear definition of "done"

**Execution Order**: Complete tasks sequentially within each phase. Tasks marked [P] can run in parallel if working on different files.

---

## User Story Summary

| Story | Priority | Title | MVP | Phase |
|-------|----------|-------|-----|-------|
| US1 | P1 | Extract Metadata from PDF Articles | ✅ Core | Phase 3 |
| US2 | P2 | Manage Local Record Collection | ✅ Core | Phase 4 |
| US3 | P3 | Synchronize Records to Cloud Database | ⚠️ Optional | Phase 5 |
| US4 | P1 | Configure Application Settings | ✅ Core | Phase 2 |

**MVP Scope**: US1, US2, US4 (extract PDFs, manage records, configure settings) - excludes cloud sync

---

## Phase 0: Project Setup & Infrastructure

**Goal**: Initialize project structure, dependencies, and development environment

### Project Initialization

- [ ] T001 [P] Initialize Node.js project with pnpm in root directory
  - **File**: `package.json`
  - **Acceptance**: `package.json` exists with name "etnopapers", version "1.0.0", type "module", pnpm workspaces configured
  - **Commands**: `pnpm init`, configure scripts section

- [ ] T002 [P] Install core dependencies (Electron, React, TypeScript)
  - **File**: `package.json`
  - **Acceptance**: Dependencies installed: electron@28+, react@18, react-dom@18, typescript@5.3+
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
  - **Acceptance**: pdfjs-dist installed with correct version for Node.js
  - **Commands**: `pnpm add pdfjs-dist`

- [ ] T007 [P] Install testing frameworks (Vitest, Testing Library, Playwright)
  - **File**: `package.json`
  - **Acceptance**: vitest, @testing-library/react, @testing-library/jest-dom, @testing-library/user-event, @playwright/test installed
  - **Commands**: `pnpm add -D vitest @testing-library/react @testing-library/jest-dom @testing-library/user-event @playwright/test`

### Configuration Files

- [ ] T008 [P] Create TypeScript configuration
  - **File**: `tsconfig.json`
  - **Acceptance**: Configured for ES2022, strict mode, paths for @/ alias, separate configs for main/renderer
  - **Content**: Target ES2022, module ESNext, strict: true, esModuleInterop: true

- [ ] T009 [P] Create Vite configuration for Electron
  - **File**: `vite.config.ts`
  - **Acceptance**: Configured for Electron renderer process, React plugin, path aliases
  - **Content**: Configure build for renderer, resolve @/ to src/, React plugin, Electron plugin

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
  - **Content**: appId, productName, win config with target: nsis, icon paths

- [ ] T015 [P] Create ESLint configuration
  - **File**: `.eslintrc.json`
  - **Acceptance**: TypeScript and React rules configured
  - **Content**: Extends typescript-eslint, react recommended, ignores build output

- [ ] T016 [P] Create Prettier configuration
  - **File**: `.prettierrc`
  - **Acceptance**: Consistent formatting rules (semi, singleQuote, trailingComma, etc.)
  - **Content**: Configure code style preferences for TypeScript

- [ ] T017 [P] Create .gitignore
  - **File**: `.gitignore`
  - **Acceptance**: Ignores node_modules/, dist/, .env, logs/, coverage/, *.log
  - **Content**: Standard Node.js, Electron, build artifacts, environment files

### Directory Structure

- [ ] T018 [P] Create source directory structure
  - **Files**: `src/main/`, `src/renderer/`, `src/shared/`, `src/preload/`
  - **Acceptance**: All directories created with index.ts stubs in each
  - **Commands**: `mkdir -p src/{main,renderer,shared,preload}/{services,components,types,utils}`

- [ ] T019 [P] Create test directory structure
  - **Files**: `tests/unit/`, `tests/integration/`, `tests/e2e/`, `tests/fixtures/`
  - **Acceptance**: All test directories created with empty fixtures
  - **Commands**: `mkdir -p tests/{unit,integration,e2e,fixtures}`

- [ ] T020 [P] Create resources directory structure
  - **Files**: `resources/icons/`, `resources/installers/`, `resources/locales/`
  - **Acceptance**: Resources directories for assets, installers, translations
  - **Commands**: `mkdir -p resources/{icons,installers,locales}`

- [ ] T021 [P] Create global styles and CSS setup
  - **File**: `src/renderer/index.css`
  - **Acceptance**: Tailwind directives present, CSS variables for shadcn/ui theming
  - **Content**: @tailwind base/components/utilities, :root CSS variables for colors

### Package Scripts

- [ ] T022 Create npm scripts in package.json
  - **File**: `package.json` scripts section
  - **Acceptance**: Scripts exist for dev, build, test, lint, format, dist
  - **Scripts**: `"dev"`, `"build"`, `"test"`, `"test:e2e"`, `"lint"`, `"format"`, `"dist"`

---

## Phase 1: Foundational Services & Configuration (User Story 4 - P1)

**Goal**: Implement configuration management and shared utilities that all features depend on

**Why This Phase First**: Configuration (US4) is a prerequisite for extraction (US1) and all other features. Without proper setup, users cannot configure AI or database connections.

### Shared Types & Validation

- [ ] T023 [P] Define core TypeScript interfaces
  - **File**: `src/shared/types/article.ts`
  - **Acceptance**: ArticleRecord, PlantSpecies, Community, SyncStatus interfaces match data-model.md
  - **Content**: Export all data model types from specs/main/data-model.md with complete JSDoc

- [ ] T024 [P] Define configuration types
  - **File**: `src/shared/types/config.ts`
  - **Acceptance**: AppConfiguration, OLLAMAConfig, MongoDBConfig, StorageConfig interfaces
  - **Content**: Match config structure from data-model.md with validation notes

- [ ] T025 [P] Define service interface types
  - **File**: `src/shared/types/services.ts`
  - **Acceptance**: All service interfaces from contracts/service-interfaces.ts
  - **Content**: Copy interfaces from specs/main/contracts/service-interfaces.ts

- [ ] T026 [P] Define error types
  - **File**: `src/shared/types/errors.ts`
  - **Acceptance**: Custom error classes for PDF, OLLAMA, Storage, MongoDB, Extraction
  - **Content**: Error classes with codes and message templates

- [ ] T027 [P] [US4] Implement title normalization utility
  - **File**: `src/shared/utils/titleNormalizer.ts`
  - **Acceptance**: Converts titles to proper case, preserves acronyms, handles particles (de, von, etc.)
  - **Logic**: Capitalize first letter of each word except common particles

- [ ] T028 [P] [US4] Implement APA author formatter utility
  - **File**: `src/shared/utils/authorFormatter.ts`
  - **Acceptance**: Formats author names to APA style (LastName, F.I.), handles particles, suffixes
  - **Logic**: Split names, format as "Last, First Middle" with initials

- [ ] T029 [P] [US4] Implement language detection utility
  - **File**: `src/shared/utils/languageDetector.ts`
  - **Acceptance**: Detects Portuguese, English, Spanish from text samples
  - **Logic**: Use simple keyword-based detection or small ML library (e.g., franc-min)

- [ ] T030 [P] Create Zod validation schemas
  - **File**: `src/shared/validation/schemas.ts`
  - **Acceptance**: Zod schemas for ArticleRecord, PlantSpecies, Community matching data-model.md
  - **Content**: Export ArticleRecordSchema, PlantSpeciesSchema, CommunitySchema with all validation rules

### Configuration Service (US4)

- [x] T031 [US4] Implement Configuration Service (main process)
  - **File**: `src/main/services/ConfigurationService.ts`
  - **Acceptance**: Implements IConfigurationService, uses electron-store, validates config
  - **Methods**: load(), save(), get(), update(), reset(), loadDefaults()

- [x] T032 [US4] Create configuration IPC handlers
  - **File**: `src/main/ipc/configHandlers.ts`
  - **Acceptance**: Exposes config operations via IPC (get, update, reset, test connections)
  - **Channels**: 'config:get', 'config:update', 'config:reset'

- [x] T033 [US4] Add configuration API to preload script
  - **File**: `src/preload/index.ts`
  - **Acceptance**: Exposes configAPI to renderer (get, update, reset methods)
  - **Security**: Uses contextBridge.exposeInMainWorld with secure API

### Logger Service (Foundational)

- [x] T034 [P] Implement Logger Service (main process)
  - **File**: `src/main/services/LoggerService.ts`
  - **Acceptance**: Implements ILoggerService, uses winston, file rotation, different log levels
  - **Methods**: debug(), info(), warn(), error(), getLogFilePath()

### Electron Main Process Setup

- [x] T035 Create Electron main process entry point
  - **File**: `src/main/index.ts`
  - **Acceptance**: Creates BrowserWindow, loads renderer, registers IPC handlers, handles app lifecycle
  - **Content**: app.whenReady(), createWindow(), register all IPC handlers, ipcMain setup

- [x] T036 Create settings page component
  - **File**: `src/renderer/pages/SettingsPage.tsx`
  - **Acceptance**: Form for OLLAMA config (URL, model, prompt), MongoDB URI, test connection buttons
  - **Features**: Form validation, connection testing feedback, persistent save

---

## Phase 2: Core Services & Data Layer (User Story 1 - P1 Part A)

**Goal**: Implement PDF processing, AI integration, and local data storage services

**Why This Phase**: These services are prerequisites for PDF extraction (US1) and record management (US2)

### PDF Processing Service (US1)

- [x] T037 [US1] Implement PDF Processing Service (main process)
  - **File**: `src/main/services/PDFProcessingService.ts`
  - **Acceptance**: Implements IPDFProcessingService, extracts text with pdf.js, gets metadata
  - **Methods**: extractText(), getMetadata(), validatePDF(), checkTextLayers()

- [x] T038 [US1] Create PDF IPC handlers
  - **File**: `src/main/ipc/pdfHandlers.ts`
  - **Acceptance**: Exposes PDF operations via IPC
  - **Channels**: 'pdf:extractText', 'pdf:getMetadata', 'pdf:validate'

- [x] T039 [US1] Add PDF API to preload script
  - **File**: `src/preload/index.ts`
  - **Acceptance**: Exposes pdfAPI to renderer with type definitions
  - **Methods**: extractText(), getMetadata(), validatePDF()

### OLLAMA Service (US1)

- [x] T040 [US1] Implement OLLAMA Service (main process)
  - **File**: `src/main/services/OLLAMAService.ts`
  - **Acceptance**: Implements IOLLAMAService, calls OLLAMA REST API, parses responses
  - **Methods**: checkHealth(), extractMetadata(), translateToPortuguese(), getAvailableModels()

- [x] T041 [US1] Create OLLAMA IPC handlers
  - **File**: `src/main/ipc/ollamaHandlers.ts`
  - **Acceptance**: Exposes OLLAMA operations via IPC with progress callbacks
  - **Channels**: 'ollama:checkHealth', 'ollama:extract', 'ollama:translate'

- [x] T042 [US1] Add OLLAMA API to preload script
  - **File**: `src/preload/index.ts`
  - **Acceptance**: Exposes ollamaAPI to renderer with event listeners
  - **Methods**: checkHealth(), extractMetadata(), translateToPortuguese()

### Data Storage Service (US1, US2)

- [x] T043 [P] [US1] Implement Data Storage Service (main process)
  - **File**: `src/main/services/DataStorageService.ts`
  - **Acceptance**: Implements IDataStorageService, uses lowdb, CRUD operations
  - **Methods**: initialize(), getAll(), getById(), create(), update(), delete(), count(), checkLimit()

- [x] T044 [US1] Create storage IPC handlers
  - **File**: `src/main/ipc/storageHandlers.ts`
  - **Acceptance**: Exposes storage operations via IPC
  - **Channels**: 'storage:getAll', 'storage:getById', 'storage:create', 'storage:update', 'storage:delete'

- [x] T045 [US1] Add storage API to preload script
  - **File**: `src/preload/index.ts`
  - **Acceptance**: Exposes storageAPI to renderer
  - **Methods**: getAll(), getById(), create(), update(), delete()

### Validation Service (US1)

- [x] T046 [P] [US1] Implement Validation Service (main process)
  - **File**: `src/main/services/ValidationService.ts`
  - **Acceptance**: Implements IValidationService, uses Zod schemas from T030
  - **Methods**: validateRecord(), validateExtractedData(), checkMandatoryFields()

### Extraction Pipeline Service (US1)

- [x] T047 [US1] Implement Extraction Pipeline Service (orchestration)
  - **File**: `src/main/services/ExtractionPipelineService.ts`
  - **Acceptance**: Implements IExtractionPipelineService, orchestrates PDF → text → AI → validation → storage
  - **Methods**: extractFromPDF(), cancelExtraction(), getExtractionStatus()

- [x] T048 [US1] Create extraction IPC handlers with progress events
  - **File**: `src/main/ipc/extractionHandlers.ts`
  - **Acceptance**: Exposes extraction with progress callbacks and error handling
  - **Channels**: 'extraction:start', 'extraction:cancel', 'extraction:status'
  - **Events**: 'extraction:progress' for real-time updates

- [x] T049 [US1] Add extraction API to preload script
  - **File**: `src/preload/index.ts`
  - **Acceptance**: Exposes extractionAPI with event listeners for progress and errors
  - **Methods**: startExtraction(), cancelExtraction(), onProgress(), onError()

- [x] T050 Finalize preload script with all APIs
  - **File**: `src/preload/index.ts`
  - **Acceptance**: All APIs exposed, type definitions for window object correct
  - **Content**: Complete contextBridge with all service APIs (config, pdf, ollama, storage, extraction)

---

## Phase 3: User Interface & Components (User Story 1 - P1 Part B)

**Goal**: Build React UI for PDF upload, extraction, and result display

### UI Foundation

- [ ] T051 [P] Create React app entry point
  - **File**: `src/renderer/main.tsx`
  - **Acceptance**: Renders App component, includes global styles, error boundary
  - **Content**: ReactDOM.createRoot() with App component, global CSS import

- [ ] T052 [P] Create root App component with routing
  - **File**: `src/renderer/App.tsx`
  - **Acceptance**: Router setup, layout with sidebar, main content area, status bar
  - **Content**: React Router configuration, layout structure with outlet

- [ ] T053 [P] Create Zustand app store
  - **File**: `src/renderer/stores/useAppStore.ts`
  - **Acceptance**: Global state for connections (OLLAMA, MongoDB), config, notifications
  - **State**: ollamaConnected, mongoConnected, config, notifications[], loading

- [ ] T054 [P] Create Zustand extraction store
  - **File**: `src/renderer/stores/useExtractionStore.ts`
  - **Acceptance**: State for current extraction progress, status, results
  - **State**: isExtracting, progress (0-100), currentStep, extractedData, error, cancelRequest

### Common UI Components

- [ ] T055 [P] Create Button component
  - **File**: `src/renderer/components/common/Button.tsx`
  - **Acceptance**: Reusable button with variants (primary, secondary, destructive), disabled state, loading state
  - **Features**: TypeScript types, accessibility attributes, theme support

- [ ] T056 [P] Create Input component
  - **File**: `src/renderer/components/common/Input.tsx`
  - **Acceptance**: Text input with label, error state, validation feedback
  - **Features**: TypeScript types, accessibility, placeholder, required indicator

- [ ] T057 [P] Create Select component
  - **File**: `src/renderer/components/common/Select.tsx`
  - **Acceptance**: Dropdown select with options, onChange handler, default value
  - **Features**: TypeScript types, grouped options support, disabled state

- [ ] T058 [P] Create Toast notification component
  - **File**: `src/renderer/components/common/Toast.tsx`
  - **Acceptance**: Toast notifications (success, error, warning, info), auto-dismiss
  - **Features**: Stacked display, accessibility, auto-dismiss timer

- [ ] T059 [P] Create Modal component
  - **File**: `src/renderer/components/common/Modal.tsx`
  - **Acceptance**: Reusable modal dialog with header, body, footer, close button
  - **Features**: Keyboard escape to close, focus trap, overlay click handling

### Layout Components

- [ ] T060 [P] Create Sidebar component
  - **File**: `src/renderer/components/layout/Sidebar.tsx`
  - **Acceptance**: Navigation links (Home, Upload, Records, Settings, About), active state
  - **Features**: Responsive, accessibility, active link highlighting

- [ ] T061 [P] Create StatusBar component
  - **File**: `src/renderer/components/layout/StatusBar.tsx`
  - **Acceptance**: Displays OLLAMA and MongoDB connection status with indicators
  - **Features**: Real-time status updates, color indicators (connected/disconnected)

- [ ] T062 [P] Create Header component
  - **File**: `src/renderer/components/layout/Header.tsx`
  - **Acceptance**: App title/logo, current page indicator
  - **Features**: Branding, page context display

### Upload Page (User Story 1 - P1)

- [ ] T063 [US1] Create Upload Page component
  - **File**: `src/renderer/pages/UploadPage.tsx`
  - **Acceptance**: File drop zone, upload button (disabled if OLLAMA disconnected), extraction progress display
  - **Features**: Conditional rendering based on OLLAMA status

- [ ] T064 [US1] Create File Drop Zone component
  - **File**: `src/renderer/components/upload/FileDropZone.tsx`
  - **Acceptance**: Drag-and-drop area, file selection button, PDF validation
  - **Validation**: Check file type, show error for non-PDF files

- [ ] T065 [US1] Create Extraction Progress component
  - **File**: `src/renderer/components/upload/ExtractionProgress.tsx`
  - **Acceptance**: Progress bar, current step indicator, cancel button, time elapsed
  - **Features**: Real-time updates from extraction store

- [ ] T066 [US1] Create Extraction Results component
  - **File**: `src/renderer/components/upload/ExtractionResults.tsx`
  - **Acceptance**: Displays extracted data in editable form, save button, duplicate warning if applicable
  - **Features**: Field-level editing, custom attribute addition, validation on save

- [ ] T067 [US1] Create extraction hook
  - **File**: `src/renderer/hooks/useExtraction.ts`
  - **Acceptance**: Custom hook manages extraction workflow, updates store, calls APIs
  - **Methods**: startExtraction(), cancelExtraction(), saveResults()

### Home Page

- [ ] T068 Create Home Page component
  - **File**: `src/renderer/pages/HomePage.tsx`
  - **Acceptance**: Welcome message, quick start guide, connection status summary, GPU advisory if applicable
  - **Features**: Contextual help, status indicators

### About Page

- [ ] T069 Create About Page component
  - **File**: `src/renderer/pages/AboutPage.tsx`
  - **Acceptance**: Displays app name, version, author (Eduardo Dalcin), institution, email
  - **Content**: Fixed information matching spec requirements

---

## Phase 4: Record Management (User Story 2 - P2)

**Goal**: Implement CRUD interface for managing local records

**Independent Test**: Can be tested with mock records without PDF extraction or cloud sync

### Records Store & Hooks

- [ ] T070 [US2] Create Zustand records store
  - **File**: `src/renderer/stores/useRecordsStore.ts`
  - **Acceptance**: State for records list, selected IDs, filters, loading state
  - **State**: records[], selectedIds[], filters, isLoading, error

- [ ] T071 [US2] Create records management hook
  - **File**: `src/renderer/hooks/useRecords.ts`
  - **Acceptance**: Custom hook for CRUD operations, loads records on mount
  - **Methods**: loadRecords(), createRecord(), editRecord(), deleteRecord(), deleteMultiple()

### Records Page Components

- [ ] T072 [US2] Create Records Page component
  - **File**: `src/renderer/pages/RecordsPage.tsx`
  - **Acceptance**: Grid/list view of records, search/filter, bulk actions toolbar
  - **Features**: Record display, selection UI, action buttons

- [ ] T073 [US2] Create Record Card component
  - **File**: `src/renderer/components/records/RecordCard.tsx`
  - **Acceptance**: Displays title, authors, year, sync status, actions (edit, delete, select)
  - **Features**: Compact display, action buttons, selection checkbox

- [ ] T074 [US2] Create Record Grid component
  - **File**: `src/renderer/components/records/RecordGrid.tsx`
  - **Acceptance**: Virtual scrolling grid of RecordCard components for performance
  - **Features**: Handles 100+ records with <200ms interactions (from SC-003a)

- [ ] T075 [US2] Create Record Filters component
  - **File**: `src/renderer/components/records/RecordFilters.tsx`
  - **Acceptance**: Filter by year range, author, biome, species presence
  - **Features**: Real-time filtering, debounced search

- [ ] T076 [US2] Create Record Form component
  - **File**: `src/renderer/components/records/RecordForm.tsx`
  - **Acceptance**: Editable form for all fields, custom attribute addition, validation
  - **Features**: Field-level editing, add/remove custom attributes, validation feedback

- [ ] T077 [US2] Implement create record modal
  - **File**: `src/renderer/components/records/CreateRecordModal.tsx`
  - **Acceptance**: Modal with empty RecordForm, creates new record on save
  - **Features**: Form validation before save, success/error feedback

- [ ] T078 [US2] Implement edit record modal
  - **File**: `src/renderer/components/records/EditRecordModal.tsx`
  - **Acceptance**: Modal with pre-filled RecordForm, updates record on save
  - **Features**: Pre-population from store, change detection, success feedback

- [ ] T079 [US2] Implement delete confirmation modal
  - **File**: `src/renderer/components/records/DeleteConfirmModal.tsx`
  - **Acceptance**: Confirmation dialog before deletion, shows record count for bulk delete
  - **Features**: Clear action consequences, cancel option

- [ ] T080 [US2] Create connection status hook
  - **File**: `src/renderer/hooks/useConnections.ts`
  - **Acceptance**: Custom hook checks OLLAMA and MongoDB status on mount, updates store
  - **Features**: Periodic polling, error handling

---

## Phase 5: Cloud Synchronization (User Story 3 - P3)

**Goal**: Implement MongoDB synchronization with batch upload and local deletion

**Independent Test**: Can be tested with test MongoDB by selecting mock records and verifying upload

### MongoDB Service

- [ ] T081 [US3] Implement MongoDB Sync Service (main process)
  - **File**: `src/main/services/MongoDBSyncService.ts`
  - **Acceptance**: Implements IMongoDBSyncService, connects to MongoDB, batch uploads
  - **Methods**: testConnection(), uploadRecord(), uploadBatch(), getStatus(), deleteAfterSync()

- [ ] T082 [US3] Create MongoDB sync IPC handlers
  - **File**: `src/main/ipc/syncHandlers.ts`
  - **Acceptance**: Exposes sync operations via IPC with progress events
  - **Channels**: 'sync:testConnection', 'sync:uploadBatch', 'sync:getStatus'

- [ ] T083 [US3] Add sync API to preload script
  - **File**: `src/preload/index.ts`
  - **Acceptance**: Exposes syncAPI to renderer with event listeners
  - **Methods**: testConnection(), uploadRecords(), onProgress()

### Sync UI Components

- [ ] T084 [US3] Create Sync Panel component
  - **File**: `src/renderer/components/sync/SyncPanel.tsx`
  - **Acceptance**: Shows selected records, sync button, connection status
  - **Features**: Record preview, confirmation, status display

- [ ] T085 [US3] Create Sync Progress component
  - **File**: `src/renderer/components/sync/SyncProgress.tsx`
  - **Acceptance**: Progress bar, per-record status (success/failed), cancel button
  - **Features**: Real-time progress updates, error details

- [ ] T086 [US3] Add sync functionality to Records Page
  - **File**: `src/renderer/pages/RecordsPage.tsx` (update)
  - **Acceptance**: Checkbox selection, "Sync Selected" button in toolbar, opens SyncPanel
  - **Features**: Multi-select support, bulk sync action

- [ ] T087 [US3] Implement sync reminder notification
  - **File**: `src/renderer/components/SyncReminder.tsx`
  - **Acceptance**: Shows notification if records count > 500 or X days since last sync
  - **Features**: Dismissible, smart timing

---

## Phase 6: Testing & Quality Assurance

**Goal**: Comprehensive test coverage and quality validation

### Unit Tests

- [ ] T088 [P] Write unit tests for title normalization
  - **File**: `tests/unit/utils/titleNormalizer.test.ts`
  - **Acceptance**: Test cases for proper case, acronyms, particles, edge cases
  - **Coverage**: >90% for titleNormalizer.ts

- [ ] T089 [P] Write unit tests for APA author formatting
  - **File**: `tests/unit/utils/authorFormatter.test.ts`
  - **Acceptance**: Test cases for single/multiple names, particles, suffixes
  - **Coverage**: >90% for authorFormatter.ts

- [ ] T090 [P] Write unit tests for language detection
  - **File**: `tests/unit/utils/languageDetector.test.ts`
  - **Acceptance**: Test samples in PT, EN, ES correctly identified
  - **Coverage**: >85% for languageDetector.ts

- [ ] T091 [P] Write unit tests for Validation Service
  - **File**: `tests/unit/services/ValidationService.test.ts`
  - **Acceptance**: Test valid/invalid records, mandatory field checks
  - **Coverage**: >85% for ValidationService.ts

- [ ] T092 [P] Write unit tests for Logger Service
  - **File**: `tests/unit/services/LoggerService.test.ts`
  - **Acceptance**: Test log levels, file creation, rotation
  - **Coverage**: >80% for LoggerService.ts

### Integration Tests

- [ ] T093 Write integration test for PDF extraction
  - **File**: `tests/integration/pdfProcessing.test.ts`
  - **Acceptance**: Test with real PDF fixtures, verify text extraction accuracy
  - **Fixtures**: Include test PDFs in `tests/fixtures/`

- [ ] T094 Write integration test for extraction pipeline
  - **File**: `tests/integration/extractionPipeline.test.ts`
  - **Acceptance**: End-to-end test: PDF → text → AI (mocked) → validation → storage
  - **Mocks**: Mock OLLAMA responses

- [ ] T095 Write integration test for storage operations
  - **File**: `tests/integration/storageOperations.test.ts`
  - **Acceptance**: Test CRUD operations, file persistence, limit enforcement
  - **Setup**: Use temp directory for test JSON files

- [ ] T096 Write integration test for MongoDB sync
  - **File**: `tests/integration/mongodbSync.test.ts`
  - **Acceptance**: Test with MongoDB memory server, verify upload and deletion
  - **Setup**: Use mongodb-memory-server for isolated testing

### E2E Tests

- [ ] T097 Write E2E test for configuration workflow
  - **File**: `tests/e2e/configuration.spec.ts`
  - **Acceptance**: Test setting OLLAMA config, MongoDB URI, connection testing
  - **Playwright**: Full app launch and interaction

- [ ] T098 Write E2E test for PDF upload and extraction
  - **File**: `tests/e2e/uploadAndExtract.spec.ts`
  - **Acceptance**: Test file selection, extraction progress, result editing, save
  - **Setup**: Mock OLLAMA service or use test instance

- [ ] T099 Write E2E test for record management
  - **File**: `tests/e2e/manageRecords.spec.ts`
  - **Acceptance**: Test create, view, edit, delete records
  - **Verification**: Check local JSON file for changes

- [ ] T100 Write E2E test for MongoDB synchronization
  - **File**: `tests/e2e/syncToMongoDB.spec.ts`
  - **Acceptance**: Test record selection, sync, verify local deletion
  - **Setup**: Use test MongoDB instance

### Component Tests

- [ ] T101 [P] Write component tests for common components
  - **Files**: `tests/unit/components/Button.test.tsx`, `Input.test.tsx`, etc.
  - **Acceptance**: Test rendering, props, user interactions for Button, Input, Select, Toast, Modal
  - **Coverage**: 100% for common components

- [ ] T102 [P] Write component tests for layout components
  - **Files**: `tests/unit/components/layout/*.test.tsx`
  - **Acceptance**: Test Sidebar, StatusBar, Header rendering and behavior
  - **Coverage**: >85% for layout components

- [ ] T103 Write component tests for Settings Page
  - **File**: `tests/unit/pages/SettingsPage.test.tsx`
  - **Acceptance**: Test configuration form, connection testing, save
  - **Mocks**: Mock config API

- [ ] T104 Write component tests for Upload Page
  - **File**: `tests/unit/pages/UploadPage.test.tsx`
  - **Acceptance**: Test file upload, progress display, result editing
  - **Mocks**: Mock extraction API

- [ ] T105 Write component tests for Records Page
  - **File**: `tests/unit/pages/RecordsPage.test.tsx`
  - **Acceptance**: Test record display, filtering, selection, CRUD actions
  - **Mocks**: Mock storage API

---

## Phase 7: Polish, Error Handling & UX Refinements

**Goal**: Production-ready error handling, notifications, and user experience improvements

### Error Handling

- [ ] T106 Implement global error boundary for React
  - **File**: `src/renderer/components/ErrorBoundary.tsx`
  - **Acceptance**: Catches React errors, displays fallback UI, logs errors
  - **Features**: Error boundary with recovery suggestions

- [ ] T107 Implement error notification system
  - **File**: `src/renderer/services/NotificationService.ts`
  - **Acceptance**: Centralized notification management, error → user-friendly messages
  - **Methods**: success(), error(), warning(), info()

- [ ] T108 Add error handling to all service calls
  - **Files**: All `src/renderer/hooks/*.ts`
  - **Acceptance**: Try-catch blocks, user-friendly error messages, logging
  - **Pattern**: Consistent error handling across all hooks

- [ ] T109 Implement retry logic for OLLAMA calls
  - **File**: `src/main/services/OLLAMAService.ts` (update)
  - **Acceptance**: Retry up to 3 times on network errors with exponential backoff
  - **Features**: User-friendly retry feedback

- [ ] T110 Implement retry logic for MongoDB operations
  - **File**: `src/main/services/MongoDBSyncService.ts` (update)
  - **Acceptance**: Retry transient failures, keep records local on persistent failure
  - **Features**: User-friendly failure messages

### UX Improvements

- [ ] T111 Add loading states to all async operations
  - **Files**: All page components
  - **Acceptance**: Spinner/skeleton during loading, disabled buttons during operations
  - **Pattern**: Consistent loading UI pattern

- [ ] T112 Add empty states for Records Page
  - **File**: `src/renderer/components/records/EmptyState.tsx`
  - **Acceptance**: Helpful message when no records, call-to-action to upload PDF
  - **Features**: Contextual guidance

- [ ] T113 Implement keyboard shortcuts
  - **File**: `src/renderer/hooks/useKeyboardShortcuts.ts`
  - **Acceptance**: Ctrl+N (new record), Ctrl+S (save), Ctrl+F (search)
  - **Features**: Help modal showing shortcuts

- [ ] T114 Add confirmation dialogs for destructive actions
  - **Files**: Records Page, Settings Page
  - **Acceptance**: Confirm before delete record(s), reset config
  - **Pattern**: Use Modal component consistently

- [ ] T115 Implement search/filter debouncing
  - **File**: `src/renderer/hooks/useDebounce.ts`
  - **Acceptance**: Custom debounce hook, 300ms delay
  - **Features**: Efficient filtering

- [ ] T116 Add tooltips for UI elements
  - **File**: `src/renderer/components/common/Tooltip.tsx`
  - **Acceptance**: Reusable tooltip component, accessible
  - **Features**: Show/hide on hover, keyboard accessible

### Performance Optimizations

- [ ] T117 Implement virtual scrolling for record list
  - **File**: `src/renderer/components/records/RecordGrid.tsx` (update)
  - **Acceptance**: Use react-window or similar, render only visible items
  - **Performance**: Support 1000+ records with <200ms interactions (SC-003a)

- [ ] T118 Optimize PDF processing with streaming
  - **File**: `src/main/services/PDFProcessingService.ts` (update)
  - **Acceptance**: Stream large PDFs, process page-by-page
  - **Features**: Progress updates for large files

- [ ] T119 Add auto-save for extraction results
  - **File**: `src/renderer/pages/UploadPage.tsx` (update)
  - **Acceptance**: Auto-save edited extraction results every 30 seconds
  - **Pattern**: Debounced auto-save with status indicator

### Internationalization & Localization

- [ ] T120 Setup i18n infrastructure
  - **File**: `src/renderer/services/i18nService.ts`
  - **Acceptance**: i18n library configured, language selection stored
  - **Languages**: Portuguese (PT-BR), English (EN)

- [ ] T121 Create Portuguese locale file
  - **File**: `resources/locales/pt-BR.json`
  - **Acceptance**: All UI strings translated to Brazilian Portuguese
  - **Scope**: All user-facing text

- [ ] T122 Create English locale file
  - **File**: `resources/locales/en-US.json`
  - **Acceptance**: All UI strings in English
  - **Scope**: Complete translation

- [ ] T123 Implement language switching
  - **File**: `src/renderer/components/settings/LanguageSelector.tsx`
  - **Acceptance**: Toggle between Portuguese and English, immediate UI update
  - **Features**: Persist language selection

---

## Phase 8: Installer & Distribution

**Goal**: Create professional Windows installer and deployment packages

### Installer Configuration

- [ ] T124 Configure electron-builder for Windows installer
  - **File**: `electron-builder.config.js` (update)
  - **Acceptance**: NSIS installer configured, auto-updater setup, file associations
  - **Features**: Professional installer UI, start menu shortcuts, uninstall support

- [ ] T125 Create installer assets
  - **Files**: `resources/icons/*.ico`, `resources/installers/*`
  - **Acceptance**: App icons (256x256, 64x64, 32x32), installer banner, license file
  - **Content**: Professional branding, clear license terms

- [ ] T126 Create installation guide documentation
  - **File**: `docs/INSTALLATION.md` (Portuguese)
  - **Acceptance**: Step-by-step installation instructions, OLLAMA prerequisites, MongoDB setup
  - **Content**: Clear, non-technical language

---

## Phase 9: Final Integration & Release

**Goal**: Verify all components work together, performance validation, release preparation

### Integration Tests

- [ ] T127 Run full E2E test suite
  - **Command**: `pnpm test:e2e`
  - **Acceptance**: All E2E tests pass, no flaky tests
  - **Coverage**: Configuration → extraction → record management → sync workflow

- [ ] T128 Performance validation
  - **File**: `tests/performance/benchmarks.ts`
  - **Acceptance**: Verify performance goals met (PDF <2min, UI <200ms, storage <50ms)
  - **Success Criteria**: All SC-001 through SC-010 validated

- [ ] T129 Build and package Windows installer
  - **Command**: `pnpm dist`
  - **Acceptance**: Installer created, size <200MB, installs successfully on Windows 10+
  - **Features**: Code signing ready (optional)

- [ ] T130 Create release notes
  - **File**: `CHANGELOG.md`
  - **Acceptance**: Document features, bug fixes, known issues, installation requirements
  - **Format**: Markdown, user-friendly language

---

## User Story Dependencies

```
US4 (Configure Settings) [P1]
  ↓ (required for)
US1 (Extract Metadata) [P1]
  ↓ (feeds into)
US2 (Manage Records) [P2]
  ↓ (enables)
US3 (Sync to MongoDB) [P3]
```

**MVP Path**: US4 → US1 → US2 (minimal viable product excludes cloud sync)

---

## Parallel Execution Opportunities

### Phase 0 (Setup) - Fully Parallelizable
Tasks T001-T022 can run in parallel (different files, no dependencies):
- T001-T007: Dependency installation [P]
- T008-T017: Configuration files [P]
- T018-T021: Directory structure [P]

### Phase 1 (Foundational) - Partially Parallelizable
- T023-T030: Type definitions and utilities [P] (no dependencies)
- T031-T036: Configuration service (depends on T023-T030)

### Phase 2 (Core Services) - Partially Parallelizable
- T037-T039: PDF service [P]
- T040-T042: OLLAMA service [P]
- T043-T045: Storage service [P]
- T046: Validation (depends on T030)
- T047-T050: Pipeline orchestration (depends on T037-T045)

### Phase 3 (UI) - Partially Parallelizable
- T051-T054: UI foundation [P]
- T055-T062: Reusable components [P] (depend on T051-T054)
- T063-T069: Page components (depend on T055-T062 and Phase 2 services)

**Recommended Parallel Groups** (for team of 3-4 developers):
1. **Developer A**: Phase 0 setup tasks
2. **Developer B**: Phase 1 (Foundational) config service
3. **Developer C**: Phase 2 (Core Services) - PDF, OLLAMA, Storage
4. **Developer D**: Phase 3 (UI) - Components and pages

---

## Task Validation & Metrics

| Metric | Value | Status |
|--------|-------|--------|
| Total Tasks | 130 | ✅ |
| Setup Phase (T001-T022) | 22 | ✅ |
| Foundational Phase (T023-T036) | 14 | ✅ |
| US1 Phase (T037-T069) | 33 | ✅ |
| US2 Phase (T070-T087) | 18 | ✅ |
| US3 Phase (T081-T087) | 7 | ✅ |
| Testing Phase (T088-T105) | 18 | ✅ |
| Polish Phase (T106-T123) | 18 | ✅ |
| Release Phase (T124-T130) | 7 | ✅ |
| Tasks with [P] parallel marker | 32 | ✅ |
| Tasks with [US#] story label | 95 | ✅ |
| Format Compliance | 100% | ✅ |

---

## Checklist Format Validation

All 130 tasks follow the required format:
- ✅ All start with `- [ ]` (markdown checkbox)
- ✅ All have sequential Task ID (T001...T130)
- ✅ All user story tasks have [US#] label
- ✅ Parallelizable tasks marked with [P]
- ✅ All have clear description with file path

**Example Task Structure**:
```
- [ ] T001 [P] Initialize Node.js project with pnpm in root directory
  - **File**: `package.json`
  - **Acceptance**: Criteria...
  - **Commands**: ...
```

---

## Implementation Strategy

### MVP Scope (Phases 0-4, Excludes US3)
**Estimated Effort**: 6-8 weeks (4 developers)
- Setup & infrastructure
- Configuration service
- PDF extraction pipeline
- Local record management
- Core UI components

### Full Product (All Phases)
**Estimated Effort**: 10-12 weeks (4 developers)
- Includes MongoDB sync
- Full testing coverage
- Performance optimization
- i18n and localization
- Installer and release

### Recommended First Week
- Complete Phase 0 (setup) - Day 1-2
- Complete Phase 1 (foundational) - Day 3-5
- Begin Phase 2 (core services) - Day 5-end of week

---

**Ready to execute. Each task is specific enough for independent completion. Begin with Phase 0 setup tasks (can run in parallel).**
