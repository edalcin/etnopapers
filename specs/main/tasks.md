# Tasks: Standalone Desktop Application with Embedded UI

**Input**: Design documents from `/specs/main/`
**Prerequisites**: plan.md, spec.md
**Status**: Phase 2 - Task Generation Complete

**Organization**: Tasks are grouped by user story (P1, P2, P3) to enable independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Frontend**: `frontend/src/` (React/TypeScript components, hooks, services)
- **Backend**: `backend/` (FastAPI routers, models, services)
- **Tests**: `frontend/tests/`, `backend/tests/`
- **Build**: Platform-specific build scripts at repository root

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and build system configuration

- [ ] T001 Create desktop application directory structure: `launcher.py`, `frontend/`, `backend/` with subdirectories per plan.md
- [ ] T002 [P] Initialize Node.js project in `frontend/` with React 18, TypeScript, Zustand, TanStack React Table, react-hook-form dependencies
- [ ] T003 [P] Initialize Python virtual environment in `backend/` and install FastAPI, PyMongo, pdfplumber, instructor, uvicorn, pytest per requirements.txt
- [ ] T004 [P] Configure Vite build system in `frontend/` for development and production builds
- [ ] T005 [P] Setup PyInstaller build configuration in `build.spec` for Windows, macOS, Linux cross-platform bundling
- [ ] T006 Setup GitHub Actions workflow in `.github/workflows/build-release.yml` for automated cross-platform executable builds on version tags

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [ ] T007 [P] Create MongoDB connection factory in `backend/database/connection.py` with MONGO_URI environment variable support (local or Atlas)
- [ ] T008 [P] Create Pydantic models for Reference entity in `backend/models/reference.py` with all fields: titulo, autores, ano, publicacao, DOI, especies, tipo_de_uso, metodologia, pais, estado, municipio, local, bioma, status
- [ ] T009 [P] Create Pydantic models for Species in `backend/models/species.py` with vernacular and nomeCientifico fields
- [ ] T010 [P] Create Pydantic models for Configuration in `backend/models/configuration.py` for MongoDB URI and Ollama endpoint storage
- [ ] T011 Implement database service layer in `backend/services/database_service.py` with CRUD operations for References (create, read, update, delete, list)
- [ ] T012 Implement Ollama connectivity validation in `backend/services/ollama_service.py` with health check endpoint
- [ ] T013 Create FastAPI application entry point in `backend/main.py` with routing, error handling, and CORS configuration for desktop frontend
- [ ] T014 Create configuration management service in `backend/services/config_service.py` to read/write MongoDB URI and Ollama settings from environment and local files
- [ ] T015 Create error handling and validation middleware in `backend/middleware/error_handler.py` with Portuguese error messages per constitution
- [ ] T016 Setup logging infrastructure in `backend/config/logging.py` with structuring for debugging (no secrets logged)
- [ ] T016.1 [P] Create MongoDB indexes in `backend/database/indexes.py`: unique index on 'doi', index on 'status', index on 'ano', text index on 'titulo' for full-text search. Implement index creation in database initialization
- [ ] T016.2 [P] Implement duplicate detection service in `backend/services/article_service.py`: check DOI unique constraint (return 409 Conflict if exists), fuzzy match fallback on (titulo + ano + primeiro_autor), return duplicate conflict response with merge suggestion for UI

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Download and Run Desktop Application (Priority: P1) 🎯 MVP

**Goal**: Enable researchers to download and run a standalone executable that launches with all UI components visible

**Independent Test**: Download executable, double-click it, verify main UI (PDF upload, article list, configuration) loads within 5 seconds on Windows/macOS/Linux

### Implementation for User Story 1

- [ ] T017 [P] [US1] Create Electron main process in `electron/main.ts` that: spawns FastAPI backend process, creates Electron window, loads React SPA from http://localhost:8000, implements IPC for configuration storage. Create launcher.py to validate Ollama is running before starting FastAPI. Implement FastAPI health check endpoints for backend startup verification
- [ ] T018 [P] [US1] Create main React App component in `frontend/src/App.tsx` with routing and Zustand store initialization for extracted metadata and editor state
- [ ] T019 [P] [US1] Create PDFUpload component in `frontend/src/components/PDFUpload.tsx` with drag-and-drop and file picker interface
- [ ] T020 [P] [US1] Create ArticlesTable component in `frontend/src/components/ArticlesTable.tsx` with TanStack React Table for displaying saved articles (columns: title, authors, year, species count, location)
- [ ] T021 [P] [US1] Create MetadataDisplay component in `frontend/src/components/MetadataDisplay.tsx` with form for showing extracted metadata with Save/Edit/Discard actions
- [ ] T022 [P] [US1] Create ResearcherProfile component in `frontend/src/components/ResearcherProfile.tsx` for optional researcher context input (name, specialization, region)
- [ ] T023 [P] [US1] Create MainLayout component in `frontend/src/components/MainLayout.tsx` with header, navigation, and component placement
- [ ] T024 [US1] Create health check API endpoint in `backend/routers/health.py` at GET `/api/health` that returns application status and Ollama connectivity
- [ ] T025 [US1] Create frontend service in `frontend/src/services/api.ts` with HTTP client for API calls and error handling
- [ ] T026 [US1] Implement static file serving in `backend/main.py` to serve React SPA from `frontend/dist/`
- [ ] T027 Build PyInstaller executables via `build-windows.bat`, `build-macos.sh`, `build-linux.sh` scripts that bundle frontend dist + backend + Python runtime into standalone executables (<200 MB each)

**Checkpoint**: User Story 1 complete - executable downloads and launches with UI visible

---

## Phase 4: User Story 2 - Configure MongoDB Connection on First Run (Priority: P1)

**Goal**: Guide users through MongoDB URI configuration in a desktop dialog during first run

**Independent Test**: Run application without configuration, enter valid MongoDB URI, verify connection, confirm configuration persists on next run

### Implementation for User Story 2

- [ ] T028 [P] [US2] Create APIConfiguration component in `frontend/src/components/APIConfiguration.tsx` with MongoDB URI input field, validation feedback, and save button
- [ ] T029 [P] [US2] Create configuration store in `frontend/src/store/configStore.ts` (Zustand) to manage MongoDB URI and Ollama URL in localStorage
- [ ] T030 [US2] Create configuration API endpoint in `backend/routers/configuration.py`: POST `/api/config/validate-mongo` to test MongoDB connection string
- [ ] T031 [US2] Create configuration API endpoint in `backend/routers/configuration.py`: POST `/api/config/save` to persist MongoDB URI to .env or config file
- [ ] T032 [US2] Create configuration API endpoint in `backend/routers/configuration.py`: GET `/api/config/status` to check if MongoDB is configured and accessible
- [ ] T033 [US2] Modify `launcher.py` to display configuration dialog if no .env file exists, using file I/O to save configuration after validation
- [ ] T034 [US2] Create startup check in `frontend/src/App.tsx` to redirect to APIConfiguration if MongoDB is not configured, then load main interface after success

**Checkpoint**: User Story 2 complete - MongoDB configuration dialog works on first run

---

## Phase 5: User Story 3 - Upload PDF and Extract Metadata (Priority: P1)

**Goal**: Enable researchers to upload PDFs and extract ethnobotanical metadata using local Ollama

**Independent Test**: Upload PDF file, wait for processing, verify extracted metadata displays in editable form, click Save and verify data persists to MongoDB

### Implementation for User Story 3

- [ ] T035 [P] [US3] Create PDF extraction service in `backend/services/pdf_service.py` using pdfplumber to extract text from uploaded PDFs (handle PDFs up to 100 MB, max 30 pages)
- [ ] T036 [P] [US3] Create Ollama metadata extraction service in `backend/services/extraction_service.py` using instructor library to call Ollama's generateContent API with structured output (Pydantic schemas for Reference/Species objects)
- [ ] T037 [P] [US3] Create metadata extraction prompt in `backend/prompts/extraction_prompt.py` that instructs Ollama to extract: titulo, autores, ano, publicacao, DOI, especies (vernacular + nomeCientifico), tipo_de_uso, metodologia, pais, estado, municipio, local, bioma
- [ ] T038 [US3] Create PDF upload API endpoint in `backend/routers/extraction.py`: POST `/api/extract/metadata` that accepts multipart/form-data PDF file and optional researcher profile, returns extracted metadata as JSON
- [ ] T039 [US3] Create extraction result caching in `frontend/src/store/extractionStore.ts` (Zustand) to hold extracted metadata during review/editing
- [ ] T040 [US3] Implement PDF drag-and-drop handler in `frontend/src/components/PDFUpload.tsx` to send file to `/api/extract/metadata` endpoint with loading indicator
- [ ] T041 [US3] Create MetadataDisplay component logic to show extracted fields in editable form (allow user corrections before saving)
- [ ] T042 [US3] Create Save metadata API endpoint in `backend/routers/articles.py`: POST `/api/articles` to persist extracted Reference to MongoDB collection `referencias`
- [ ] T043 [US3] Create Discard action in `frontend/src/components/MetadataDisplay.tsx` to clear extraction store without saving
- [ ] T044 [US3] Implement Ollama health check before PDF processing in `backend/routers/extraction.py`, return clear error "Ollama service unavailable" if not accessible
- [ ] T045 [US3] Add Portuguese error messages and user feedback in extraction workflow (extraction in progress, extraction failed, Ollama unavailable, etc.) with specific behaviors: (a) "PDF em processamento..." during extraction, (b) "Erro ao processar PDF - Verifique se o arquivo é válido" for extraction failure, (c) "Serviço Ollama indisponível - Verifique conexão e tente novamente" for Ollama errors
- [ ] T045.1 [US3] Create extraction accuracy validation test suite in `backend/tests/test_extraction_accuracy.py` with: (a) 50+ ground-truth ethnobotany article dataset (PDFs + expected metadata), (b) automated comparison script measuring accuracy for title (exact match), author count (±1), species count (±1), (c) fail if accuracy <90%, (d) generate accuracy report showing per-field metrics
- [ ] T045.2 [US3] Add PDF validation to `backend/services/pdf_service.py`: check if PDF is text-extractable (magic bytes + pdfplumber validation), reject with error "PDF não contém texto extraível - Use ferramenta OCR externa" if scanned/image-only, prevent Ollama call for invalid PDFs
- [ ] T045.3 [US3] Add file size validation to `frontend/src/components/PDFUpload.tsx`: validate PDF <100 MB before upload, reject >100 MB with error "PDF muito grande (>100 MB) - Divida o arquivo em partes menores", show file size indicator in upload UI
- [ ] T045.4 [US3] Add network error handling to `backend/services/extraction_service.py`: implement 3-retry logic with exponential backoff (1s, 2s, 4s) for network errors during Ollama API calls, show "Erro de rede durante processamento - Verifique conexão e tente novamente" after final retry failure
- [ ] T045.5 [US3] Integrate researcher profile context into extraction: read ResearcherProfile from Zustand store (T022), serialize as optional 'context' field in `/api/extract/metadata` request body, include in Ollama extraction prompt for improved accuracy
- [ ] T045.6 [US3] Implement processing state persistence in `frontend/src/store/extractionStore.ts`: save partial extraction state to IndexedDB when processing begins, show confirmation dialog on app close "Processamento em andamento - Deseja cancelar?", auto-resume draft on next startup, allow user to continue from draft or discard
- [ ] T045.7 [US3] Add OS compatibility validation to `launcher.py` and `electron/main.ts`: detect OS (Windows/macOS/Linux), validate matches executable platform, display error dialog if mismatch "Este executável é para Windows - Baixe a versão adequada para seu sistema em https://github.com/edalcin/etnopapers/releases" and exit gracefully

**Checkpoint**: User Story 3 complete - PDF upload and metadata extraction workflow functional with accuracy validated >90%, edge cases handled gracefully, processing state persisted

---

## Phase 6: User Story 4 - View and Manage Articles (Priority: P2)

**Goal**: Enable researchers to browse, sort, filter, and edit previously extracted articles

**Independent Test**: Verify articles table displays all saved articles, sort by multiple columns works, filter/search reduces results correctly, click article to edit and verify changes persist

### Implementation for User Story 4

- [ ] T046 [P] [US4] Create article retrieval API endpoint in `backend/routers/articles.py`: GET `/api/articles` with optional query parameters (sort, filter, limit, offset) for table pagination/sorting
- [ ] T047 [P] [US4] Create text search service in `backend/services/search_service.py` to filter articles by title, author, species name, or location
- [ ] T048 [US4] Implement ArticlesTable component logic in `frontend/src/components/ArticlesTable.tsx` with TanStack React Table: sortable columns, search box, pagination
- [ ] T049 [US4] Create article detail view component in `frontend/src/components/ArticleDetail.tsx` to display full article metadata with collapsible sections
- [ ] T050 [US4] Create article edit form in `frontend/src/components/ArticleEditor.tsx` with react-hook-form for editing Reference fields (title, authors, species, locations, etc.)
- [ ] T051 [US4] Create article update API endpoint in `backend/routers/articles.py`: PUT `/api/articles/{article_id}` to update Reference document in MongoDB
- [ ] T052 [US4] Create article delete API endpoint in `backend/routers/articles.py`: DELETE `/api/articles/{article_id}` to remove Reference from MongoDB
- [ ] T053 [US4] Add sorting and filtering state management in `frontend/src/store/articleStore.ts` (Zustand) for table state persistence across navigation
- [ ] T054 [US4] Implement debounced search in ArticlesTable to reduce API calls while typing (500ms debounce)
- [ ] T055 [US4] Add pagination state management and API integration for large article lists (10,000+ articles)
- [ ] T055.1 [P] [US4] Create taxonomy validation service in `backend/services/taxonomy_service.py` with GBIF API integration: query GBIF Species API for scientific names, cache results for 30 days in-memory, mark species as "validado" (green) or "não validado" (yellow) based on GBIF response, fallback gracefully if GBIF unavailable
- [ ] T055.2 [US4] Add species validation indicators to `frontend/src/components/ArticlesTable.tsx`: show green checkmark for validated species, yellow warning for unvalidated, allow click-to-validate action with loading spinner, update status on successful validation
- [ ] T055.3 [US4] Implement periodic MongoDB health check in `backend/services/mongodb_service.py`: check connection every 60 seconds, emit status events on disconnect/reconnect, queue pending operations (save, update) for retry on reconnection
- [ ] T055.4 [US4] Add MongoDB status indicator to `frontend/src/components/MainLayout.tsx`: show green dot for connected, red dot for disconnected, yellow dot for connecting, display tooltip with connection details, show "Reconectando com banco de dados..." during retry

**Checkpoint**: User Story 4 complete - Article management table fully functional with taxonomy validation and MongoDB health monitoring

---

## Phase 7: User Story 5 - Download Database Backup (Priority: P2)

**Goal**: Enable researchers to export entire database as portable ZIP backup

**Independent Test**: Click "Download Database Backup" button, verify ZIP file downloads with all articles inside, ZIP can be extracted and inspected

### Implementation for User Story 5

- [ ] T056 [P] [US5] Create database backup service in `backend/services/backup_service.py` that: queries all References from MongoDB, exports to JSON format, creates ZIP archive with timestamp
- [ ] T057 [P] [US5] Create database download API endpoint in `backend/routers/database.py`: GET `/api/database/download` that returns ZIP file with all articles as streamable response
- [ ] T058 [US5] Create backup button in `frontend/src/components/DatabaseDownload.tsx` with download progress indicator and success/error messages
- [ ] T059 [US5] Implement database integrity check in `backup_service.py` before download: verify all documents are valid, verify collection indexes exist
- [ ] T060 [US5] Add checksum validation in backup ZIP file for integrity verification after download

**Checkpoint**: User Story 5 complete - Database backup and export working

---

## Phase 8: User Story 6 - Validate Ollama Connection (Priority: P3)

**Goal**: Check Ollama availability on startup and provide clear user feedback if unavailable

**Independent Test**: Start application without Ollama running, verify clear error message appears, start Ollama and retry, verify processing works

### Implementation for User Story 6

- [ ] T061 [P] [US6] Create Ollama health check API in `backend/routers/health.py`: GET `/api/health/ollama` that checks http://localhost:11434/api/tags endpoint
- [ ] T062 [P] [US6] Create Ollama status check in `launcher.py` during startup: if Ollama unavailable, display dialog with message "Serviço de AI local indisponível. Verifique Ollama ou reinicie o aplicativo."
- [ ] T063 [US6] Create frontend health check hook in `frontend/src/hooks/useOllamaHealth.ts` to periodically check Ollama status (every 30 seconds)
- [ ] T064 [US6] Add Ollama status indicator in `MainLayout.tsx` showing connection status (green/red) with tooltip
- [ ] T065 [US6] Create user-friendly error handling in PDFUpload to prevent processing attempts if Ollama is offline, display actionable error message

**Checkpoint**: User Story 6 complete - Ollama validation and status feedback implemented

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: Quality assurance, documentation, and production readiness

- [ ] T066 [P] Create unit tests for backend services in `backend/tests/test_database_service.py`, `backend/tests/test_extraction_service.py`, `backend/tests/test_pdf_service.py` (target >80% coverage)
- [ ] T067 [P] Create integration tests for critical paths in `backend/tests/test_extraction_workflow.py`: upload PDF → extract → save → query from MongoDB
- [ ] T068 [P] Create component tests for React components in `frontend/tests/PDFUpload.test.tsx`, `frontend/tests/ArticlesTable.test.tsx`, `frontend/tests/MetadataDisplay.test.tsx`
- [ ] T069 Create end-to-end test scenarios in `backend/tests/test_e2e.py` covering: first-run configuration → PDF upload → extraction → save → view → edit → backup download
- [ ] T070 Update API documentation in `backend/docs/API.md` with all endpoints, request/response schemas, error codes (Portuguese)
- [ ] T071 Create user guide in `docs/USER_GUIDE_PT.md` with screenshots and step-by-step instructions for: first run, PDF upload, article management, backup (Portuguese)
- [ ] T072 Create developer guide in `docs/DEVELOPER_GUIDE.md` with: local development setup, architecture overview, extending with new extraction fields, running tests
- [ ] T073 [P] Add TypeScript type checking: `npm run type-check` in frontend with zero errors
- [ ] T074 [P] Add linting: setup ESLint in frontend with formatting rules, `npm run lint` passes
- [ ] T075 [P] Add backend linting: setup flake8/black in backend, `python -m flake8 backend/` and `black backend/` format passes
- [ ] T076 Test all three executable builds (Windows, macOS, Linux) on respective platforms: verify startup time <5 seconds, verify UI loads, verify file picker works, verify all components visible
- [ ] T077 Verify Portuguese localization: all user-facing text, error messages, UI labels are in Brazilian Portuguese (no English in user-visible text)
- [ ] T078 Create security checklist in `docs/SECURITY.md`: (a) HTTPS for external APIs (GBIF/Tropicos), (b) no API keys/secrets in logs, (c) PDF file magic byte validation (reject non-PDF), (d) MongoDB injection prevention via parameterized queries, (e) CORS configuration limited to localhost in Electron, (f) rate limiting on `/api/extract/metadata` (max 10 requests/minute per client), (g) input validation on all endpoints (Pydantic models), (h) document threat model for single-user desktop app
- [ ] T079 Create performance testing script in `backend/tests/test_performance.py`: (a) measure app startup time (target <5s Electron window + FastAPI), (b) PDF extraction time (target <2 minutes for 30 pages), (c) table rendering time (1000 articles <2 seconds), (d) MongoDB query response (<500ms for 10k articles), (e) create benchmark report comparing GPU vs CPU Ollama performance
- [ ] T080 Create deployment guide in `docs/DEPLOYMENT.md` with: (a) system requirements (MongoDB, Ollama, system RAM/disk), (b) environment variable setup (.env template), (c) running from executable (platform-specific instructions), (d) troubleshooting (common issues + solutions), (e) data backup/restore procedures, (f) updating to new versions
- [ ] T080.1 Create `docs/DOCKER_FUTURE.md` documenting deferred Docker deployment approach: (a) rationale (standalone executable MVP prioritized), (b) planned Docker Compose design for Phase 3+ (GPU acceleration, NVIDIA runtime), (c) migration path from standalone to Docker deployment
- [ ] T080.2 [P] Enhance T077 localization verification: create automated test script `frontend/tests/test_localization.ts` that scans all component files and backend error messages for English strings (regex pattern), fail CI/CD if user-visible English detected, exclude code comments and technical terms
- [ ] T080.3 Create `docs/TROUBLESHOOTING.md` with: (a) common error messages and solutions (Portuguese), (b) MongoDB connection issues + fix steps, (c) Ollama not found + fix steps, (d) PDF processing fails + debugging, (e) app crashes + log collection instructions

**Checkpoint**: Application is production-ready, fully tested, documented, and localized

---

## Implementation Strategy

### MVP Scope (Phase 1-5)

**Delivers**: Standalone desktop application with core ethnobotanical metadata extraction workflow

**Includes**:
- User Story 1: Executable download and launch ✅
- User Story 2: MongoDB configuration dialog ✅
- User Story 3: PDF upload and metadata extraction ✅

**Does NOT include**:
- Article management table (US4, Phase 6)
- Database backup (US5, Phase 7)
- Ollama validation (US6, Phase 8)

**Timeline**: Start with Phase 1-5, deploy MVP after Phase 5. User Stories 4-6 can follow in later releases.

### Parallel Execution Opportunities

**Phase 1 (Setup)**:
- T002, T003, T004, T005 can run in parallel (independent build system setup)
- T002, T003 setup happens before T004, T005 (dependency: have dependencies installed before configuring builders)

**Phase 2 (Foundational)**:
- T007-T010 (models/database connection) can run in parallel
- T011-T016 (services) depend on T007-T010, can run in parallel with each other

**Phase 3 (US1)**:
- T018-T023 (frontend components) can run in parallel
- T024-T026 (backend endpoints) can run in parallel with frontend
- T027 (build) depends on all frontend/backend work

**Phase 4 (US2)**:
- T028-T029 (frontend) can run in parallel with T030-T032 (backend)

**Phase 5 (US3)**:
- T035-T037 (extraction services/prompts) can run in parallel
- T040-T041 (frontend upload handler) can run in parallel with backend endpoints

**Phase 9 (Polish)**:
- T066-T075 (testing, linting) can run in parallel

### Dependency Graph

```
T001 (structure) → T002-T005 (setup) → T007-T016.2 (foundation) → T017-T027 (US1) → T028-T034 (US2) → T035-T045.7 (US3) → T046-T055.4 (US4) → T056-T060 (US5) → T061-T065 (US6) → T066-T080.3 (Polish)
```

**Critical Path**: T001 → T007-T016.2 → T017-T027 → T035-T045.7 (defines MVP scope)

**Updated Task Metrics**:
- **Total Tasks**: ~100 (T001-T080.3, including sub-tasks T016.1, T016.2, T045.1-T045.7, T055.1-T055.4, T080.1-T080.3)
- **Phase 1 (Setup)**: 6 tasks
- **Phase 2 (Foundation)**: 12 tasks (including T016.1, T016.2)
- **Phase 3 (US1)**: 11 tasks
- **Phase 4 (US2)**: 7 tasks
- **Phase 5 (US3)**: 19 tasks (including T045.1-T045.7)
- **Phase 6 (US4)**: 18 tasks (including T055.1-T055.4)
- **Phase 7 (US5)**: 5 tasks
- **Phase 8 (US6)**: 5 tasks
- **Phase 9 (Polish)**: 17 tasks (including T080.1-T080.3)

---

## Quality Checkpoints

- **After Phase 1**: Project structure created, all dependencies installed, build system configured
- **After Phase 2**: Database connection working, MongoDB tested, services structure ready
- **After Phase 3 (US1)**: Executable builds successfully, launches, UI visible, no crashes
- **After Phase 4 (US2)**: First-run configuration dialog works, MongoDB connection saves/persists
- **After Phase 5 (US3)**: End-to-end PDF upload → extract → save workflow functional, data persists to MongoDB
- **After Phase 6-8**: All user stories implemented and independently testable
- **After Phase 9**: Tests pass (>80% coverage), linting clean, documentation complete, Portuguese localization verified, performance targets met
