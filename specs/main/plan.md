# Implementation Plan: Standalone Desktop Application with Embedded UI

**Branch**: `main` | **Date**: 2025-11-27 | **Spec**: `/specs/spec.md`
**Input**: Feature specification from `/specs/spec.md`

**Note**: This plan has completed Phase 0 (research.md) and Phase 1 (data-model.md). Phase 2 tasks will be generated via `/speckit.tasks`.

## Summary

Build a standalone desktop application (Windows, macOS, Linux) that bundles a React UI frontend with a Python FastAPI backend. The application enables ethnobotany researchers to upload PDFs, extract metadata using local Ollama AI inference, manage extracted articles in MongoDB, and download database backups—all without requiring users to manage a terminal, install services, or provide API keys. The architecture uses Electron as the native desktop container with HTTP communication to a locally-spawned FastAPI backend process.

## Technical Context

**Language/Version**:
- Frontend: TypeScript/React 18
- Backend: Python 3.11+ with FastAPI
- Desktop: Electron 30+ (Chromium-based)

**Primary Dependencies**:
- Frontend: React 18, Zustand (state), TanStack React Table (data tables), react-hook-form (forms)
- Backend: FastAPI, PyMongo (MongoDB driver), pdfplumber (PDF text extraction), instructor (Ollama structured output), uvicorn
- Desktop: Electron (main process), PyInstaller (Python bundling), electron-builder (cross-platform packaging)

**Storage**: MongoDB (NoSQL, BSON format) via MONGO_URI environment variable; supports local MongoDB or MongoDB Atlas cloud

**Testing**:
- Backend: pytest (unit/integration tests for FastAPI routers and services)
- Frontend: Jest + React Testing Library (component tests)
- Integration: End-to-end workflow tests (upload → extract → save → query)

**Target Platform**: Windows 10+, macOS 10.13+ (both Intel & Apple Silicon), Linux (glibc 2.29+)

**Project Type**: Desktop application (web-based UI rendered in Electron, backend subprocess)

**Performance Goals**:
- Application startup: <5 seconds
- PDF text extraction: <2 minutes for documents up to 30 pages
- Metadata extraction via Ollama: 1-3 seconds per article (with GPU, ~30-60 seconds CPU-only)
- Article list rendering: <2 seconds for 1000 articles
- Filter/search response: <500 ms with debounce

**Constraints**:
- Executable size: <200 MB per platform
- Memory footprint: <500 MB during operation
- PDF file limit: <100 MB per upload
- Offline capable except for Ollama inference, GBIF/Tropicos taxonomy lookup, and initial Ollama model download
- No external API keys required; users provide only MongoDB URI

**Scale/Scope**:
- Single-user desktop application (no multi-user, no auth)
- ~30 API endpoints (CRUD for articles, configuration, backup, Ollama health check)
- ~10 React components (PDF upload, article list, editor, configuration, backup)
- ~2,000-3,000 lines of backend code (FastAPI routers + services)
- ~1,500-2,000 lines of frontend code (React components)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

**Constitution Compliance Status**: ✅ **PASS** (No violations)

### Principle I: Privacy-First Architecture & Local AI Inference
- ✅ **Status**: Fully compliant
- All data processing happens server-side (Python backend in subprocess)
- Frontend sends PDFs to backend `/api/extract/metadata` endpoint; backend calls local Ollama (no external API calls)
- Users never provide AI keys; system is entirely self-contained
- MongoDB connection configurable via MONGO_URI (local or self-hosted cloud only, no third-party services)

### Principle II: Data Portability & Locality
- ✅ **Status**: Fully compliant
- MongoDB is the single persistent data store; connection via MONGO_URI environment variable
- Database backup export (`/api/database/download`) provides ZIP archive for portability
- Users can backup and restore MongoDB to any MongoDB instance (local or compatible cloud provider)
- System operates offline except for: (a) initial Ollama model download, (b) optional GBIF taxonomy validation, (c) local Ollama inference service

### Principle III: Offline Tolerance & Graceful Degradation
- ✅ **Status**: Fully compliant
- Local Ollama service is co-deployed requirement; if unavailable, clear error: "Serviço de AI local indisponível. Verifique Ollama ou reinicie o aplicativo."
- External APIs (GBIF/Tropicos) optional; species marked as "não validado" if unavailable, no data loss
- Network disconnection handled gracefully with actionable error messages
- No data loss from service outages; users can retry later

### Principle IV: Simplicity & MVP-First
- ✅ **Status**: Fully compliant
- Single container/executable (Electron bundles everything)
- No authentication, multi-user, analytics, or advanced search in Phase 2 tasks
- Pure HTTP communication (no IPC, no gRPC)
- MongoDB single collection (`referencias`) with denormalized documents
- Only explicitly required features in scope; future phases can add complexity

### Principle V: Portable Docker Deployment with GPU Acceleration
- ⚠️ **Status**: Partially applicable; deferred to future phase
- Current Phase 2 focuses on **standalone executable distribution** (PyInstaller), not Docker
- GPU acceleration assumption: Users run Ollama locally with GPU support
- Future Phase 2-3 may add Docker Compose deployment option (GPU-enabled containers), but not required for standalone executable MVP
- **Justification**: Standalone executables (per spec requirement FR-001) are simpler for initial distribution; Docker deployment can follow if user feedback indicates need

### Principle VI: Portuguese-First Localization
- ✅ **Status**: Fully compliant
- All backend error messages, API documentation, and frontend UI labels must be in Brazilian Portuguese
- Code comments in English (standard practice for engineering collaboration)
- API contracts, spec, and docs already written in Portuguese

**Re-check Status** (post-design, Phase 1 complete): ✅ All principles satisfied. No violations detected.

## Project Structure

### Documentation (this feature)

```text
specs/main/
├── spec.md              # Feature specification (complete)
├── plan.md              # This file (implementation plan)
├── research.md          # Phase 0 research (complete)
├── data-model.md        # Phase 1 data model (complete)
├── quickstart.md        # Phase 1 quickstart guide (to be completed)
├── contracts/           # Phase 1 API contracts (to be completed)
│   └── api-rest.yaml    # OpenAPI 3.0 specification
└── tasks.md             # Phase 2 tasks (generated via /speckit.tasks)
```

### Source Code (Repository Root)

**Selected Structure**: Web application with frontend (React/Electron) and backend (FastAPI)

```text
backend/
├── main.py                          # FastAPI entry point (launcher.py spawns this)
├── launcher.py                      # Electron subprocess launcher + health checks
├── requirements.txt                 # Python dependencies
├── build.spec                       # PyInstaller spec for bundling
├── src/
│   ├── models/
│   │   ├── article.py              # Pydantic schemas (Reference, Species, Metadata)
│   │   ├── config.py               # Configuration schemas (MongoURI, OllamaURL)
│   │   └── error.py                # Error response schemas
│   ├── services/
│   │   ├── article_service.py       # CRUD operations for articles
│   │   ├── ollama_service.py        # Ollama AI inference + response parsing
│   │   ├── pdf_service.py           # PDF text extraction (pdfplumber)
│   │   ├── taxonomy_service.py      # GBIF/Tropicos validation with caching
│   │   ├── mongodb_service.py       # MongoDB connection + health checks
│   │   └── backup_service.py        # Database backup and export
│   ├── routers/
│   │   ├── articles.py              # /api/articles endpoints (CRUD)
│   │   ├── metadata.py              # /api/extract/metadata endpoint
│   │   ├── health.py                # /api/health endpoints (Ollama, MongoDB)
│   │   ├── backup.py                # /api/database/download endpoint
│   │   └── config.py                # /api/config endpoints (setup dialog)
│   └── database/
│       ├── connection.py            # MongoDB client + initialization
│       └── indexes.py               # Collection indexes + validation
└── tests/
    ├── unit/
    │   ├── test_services.py         # Service layer unit tests
    │   └── test_models.py           # Pydantic model validation
    └── integration/
        ├── test_api_endpoints.py    # Full endpoint integration tests
        ├── test_mongodb.py          # Database integration tests
        └── test_extraction_flow.py  # End-to-end extraction workflow

frontend/
├── src/
│   ├── components/
│   │   ├── PDFUpload.tsx            # Drag-and-drop PDF upload
│   │   ├── MetadataDisplay.tsx      # Extracted metadata review form
│   │   ├── ManualEditor.tsx         # Full metadata editor
│   │   ├── ArticlesTable.tsx        # Article list with sort/filter
│   │   ├── ConfigurationDialog.tsx  # MongoDB URI setup dialog
│   │   ├── OllamaStatus.tsx         # Ollama connection indicator
│   │   └── DatabaseBackup.tsx       # Download backup button
│   ├── pages/
│   │   ├── Home.tsx                 # Main page (PDF upload + article list)
│   │   ├── Settings.tsx             # Configuration page
│   │   └── ArticleDetail.tsx        # Single article view/edit
│   ├── services/
│   │   ├── api.ts                   # HTTP client (base URLs, interceptors)
│   │   ├── articleService.ts        # API calls for articles
│   │   ├── extractionService.ts     # API calls for metadata extraction
│   │   ├── healthService.ts         # Ollama/MongoDB health checks
│   │   └── configService.ts         # Configuration persistence
│   ├── store/
│   │   └── useAppStore.ts           # Zustand global state (articles, config, editor)
│   ├── types/
│   │   ├── article.ts               # TypeScript interfaces for article data
│   │   └── api.ts                   # API request/response types
│   ├── App.tsx                      # Root component
│   └── index.css                    # Tailwind or CSS modules
├── public/
│   ├── favicon.png                  # Etnopapers logo
│   └── index.html                   # HTML entry point
├── package.json                     # npm dependencies (React, Zustand, TanStack, etc.)
└── tests/
    ├── components/                  # React component tests (Testing Library)
    └── integration/                 # E2E workflow tests

electron/
├── main.ts                          # Electron main process (window management, IPC)
├── preload.ts                       # Preload script (optional, for security)
└── build/
    └── icon.png                     # App icon for Windows/macOS/Linux

build-scripts/
├── build-windows.bat                # Windows executable builder
├── build-macos.sh                   # macOS app bundle builder
├── build-linux.sh                   # Linux binary builder
└── package-windows.ps1              # Code signing and packaging (Windows)

docs/
├── DEVELOPMENT.md                   # Local development setup guide
├── BUILDING.md                      # Instructions for building executables
└── DEPLOYMENT.md                    # Distribution and release workflow

.github/workflows/
└── release.yml                      # GitHub Actions for automated builds + releases
```

**Structure Rationale**:
- **Electron** sits at project root (`electron/`, `build-scripts/`) for visibility
- **Backend** isolated in `backend/` with standard FastAPI structure (models, services, routers)
- **Frontend** isolated in `frontend/` with standard React/TypeScript structure (components, pages, services)
- **Separate concerns**: Backend can be deployed as pure API; frontend as web app; or bundled via Electron
- **Existing codebase compatibility**: Maintains existing `backend/`, `frontend/`, `docs/` directories with new Electron-specific files

## Complexity Tracking

**Status**: ✅ No violations requiring justification

All design decisions align with Constitution principles. The only architectural complexity (Electron + PyInstaller + subprocess communication) is explicitly required by the feature specification (FR-001: standalone executable, FR-020: no browser), not optional enhancement.

| Aspect | Design Choice | Justification | Simpler Alternative Rejected |
|--------|---------------|---------------|------------------------------|
| Desktop Framework | Electron | Reuses existing React codebase; massive ecosystem | PyQt/Tauri would require frontend rewrite or Rust integration |
| Python Bundling | PyInstaller | Already in use; bundles FastAPI + dependencies | cx_Freeze is older, cx_Freeze + Poetry adds complexity |
| IPC | HTTP localhost | Zero code changes to existing FastAPI; works on all OSes | Electron IPC requires React refactoring; reduces portability |
| Database | MongoDB (BSON) | Single collection, no migrations, denormalized docs | Normalization with SQL would require JOIN queries; more schema management |
| State Management | Zustand | Minimal boilerplate; lightweight for single-user app | Redux would add complexity; Context API less suitable for global async state |

**Infrastructure & Deployment**:
- Single executable per OS (PyInstaller), not Docker (per Phase V deferral note)
- GitHub Actions for automated builds (leverages existing CI/CD patterns)
- No authentication layer (out of scope per spec); future phases can add if needed
