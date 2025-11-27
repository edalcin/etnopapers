# Implementation Plan: Standalone Desktop Application with Embedded UI

**Branch**: `main` | **Date**: 2025-11-27 | **Spec**: [specs/spec.md](../spec.md)
**Input**: Feature specification from `specs/spec.md`

**Note**: This plan addresses a **major architectural shift** from Docker Compose deployment (Constitution) to standalone desktop executables (Specification). This requires justification in Complexity Tracking.

## Summary

Refactor Etnopapers from a Docker Compose containerized backend + browser frontend architecture to **three standalone native executables** (Windows .exe, macOS .app, Linux binary) with completely embedded UI. The application maintains:

- **MongoDB external connection** via MONGO_URI (local or cloud)
- **Local Ollama integration** for GPU-accelerated PDF metadata extraction
- **No browser dependency** – all UI embedded in native application window
- **No terminal/server visible to user** – single click to run

Core workflow: Download executable → Run → Configure MongoDB URI → Upload PDF → Extract metadata with Ollama → View/manage articles → Backup database.

**Technical Approach**:
- **Desktop Framework**: Electron (cross-platform, embedded Chromium, native file dialogs)
- **Backend**: Python FastAPI bundled with PyInstaller
- **Frontend**: React 18 + TypeScript (compiled to static assets, served by embedded backend)
- **Packaging**: PyInstaller wraps everything into single executable per OS

## Technical Context

**Language/Version**:
- Frontend: TypeScript 5.x / React 18 (existing)
- Backend: Python 3.11+ (existing)

**Primary Dependencies**:
- Frontend: Vite, React, Zustand, TanStack React Table
- Desktop: Electron (for native window management) OR Tauri (lightweight Rust alternative)
- Backend: FastAPI, PyMongo, pdfplumber, instructor (Ollama)
- Packaging: PyInstaller 6.x (bundle Python runtime + dependencies)

**Storage**:
- Persistent Config: JSON file in user home directory (`.etnopapers/config.json`)
- Application Data: MongoDB (external, user-configured)
- Temporary: System temp directory for PDFs during processing

**Testing**:
- Backend: pytest (existing)
- Frontend: Vitest + React Testing Library (existing)
- Desktop Integration: Electron test utilities or Playwright
- E2E: Test PDF upload → extraction → save → download flow

**Target Platform**: Windows 10+, macOS 10.13+, Linux glibc 2.29+
**Project Type**: Desktop application (Electron/Tauri frontend + embedded Python backend)
**Performance Goals**:
- Launch: <5 seconds
- PDF extraction: <10 seconds for 5-page document
- Table rendering: <2 seconds for 1000 articles
- Memory: <300 MB RAM for typical workflow

**Constraints**:
- Executable size: <200 MB per OS
- No external API calls (except MongoDB connection string, Ollama local)
- Offline operation: Works completely offline except Ollama inference
- GPU requirement: Ollama leverages GPU if available (not mandatory for app, but recommended for performance)

**Scale/Scope**:
- Single-user per instance (desktop app)
- Typical database: 10,000–100,000 articles
- UI: ~15 screens (configuration, upload, metadata editor, articles table, article detail, settings)

## Constitution Check - ARCHITECTURAL CONFLICT DETECTED

**GATE: FAILED - Requires Complexity Justification**

### Violations

| Constitution Principle | Violation | Impact |
|------------------------|-----------|--------|
| **V. Portable Docker Deployment** | Spec requires standalone executables; Constitution mandates Docker Compose with GPU | Architecture shift from container-based to desktop-native application |
| **Deployment Model** | Constitution: "single `docker-compose up` for UNRAID servers"; Spec: "single executable download" | Completely different deployment model (container vs. binary distribution) |
| **GPU Acceleration** | Constitution requires GPU for production (RTX 3060+, NVIDIA); Desktop app may run on end-user machines without GPU | Performance SLA (1-3s latency) may not be achievable on CPU-only machines |

### Justification Required (Complexity Tracking Below)

This refactoring represents a **conscious architectural pivot**: from a researcher-server deployment model (UNRAID containerized service) to a **researcher-local deployment model** (standalone client application). This is **explicitly requested in the feature spec** and supercedes the Constitution's Docker Compose requirement for this phase.

## Project Structure

### Documentation (this feature)

```text
specs/
├── spec.md              # Feature specification (user stories, requirements, success criteria)
├── plan.md              # This file (implementation architecture + technical decisions)
├── research.md          # Phase 0 output (NEEDS CLARIFICATION resolutions)
├── data-model.md        # Phase 1 output (entity definitions, schema)
├── contracts/           # Phase 1 output (API specifications)
│   └── api-rest.yaml    # OpenAPI 3.0 schema for backend endpoints
├── quickstart.md        # Phase 1 output (setup instructions for developers)
├── checklists/          # Quality gates
│   └── requirements.md  # Specification quality checklist
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code Structure

**Existing structure** (refactored for desktop packaging):

```text
frontend/                          # React 18 + TypeScript SPA
├── src/
│   ├── components/                # UI components (PDFUpload, MetadataDisplay, etc.)
│   ├── pages/                     # Page layouts (Home, Articles, Settings)
│   ├── services/                  # API clients, Ollama integration
│   ├── store/                     # Zustand state (metadata, editor, config)
│   ├── types/                     # TypeScript type definitions
│   └── App.tsx
├── public/                        # Static assets
├── dist/                          # Built SPA (generated by `npm run build`)
├── package.json
├── tsconfig.json
└── vite.config.ts

backend/                           # Python 3.11+ FastAPI
├── main.py                        # Application entry point (serves frontend)
├── launcher.py                    # Startup script for PyInstaller (handles .env config)
├── routers/                       # API endpoint handlers
│   ├── articles.py                # CRUD for referencias collection
│   ├── extract.py                 # PDF upload + metadata extraction endpoint
│   ├── database.py                # Database backup endpoint
│   └── health.py                  # Ollama/MongoDB health checks
├── services/                      # Business logic layer
│   ├── article_service.py
│   ├── extraction_service.py       # Calls Ollama for metadata extraction
│   ├── taxonomy_service.py         # GBIF/Tropicos validation
│   └── backup_service.py           # MongoDB dump + ZIP creation
├── models/                        # Pydantic schemas
│   ├── article.py                 # Reference/Article schema
│   ├── metadata.py                # Extracted metadata structures
│   └── config.py                  # Application configuration
├── db/                            # Database layer
│   └── connection.py              # MongoDB connection management
├── requirements.txt               # Python dependencies
└── tests/
    ├── unit/
    ├── integration/
    └── fixtures/

build/                             # Build artifacts (generated)
├── build-windows.bat              # Windows build script
├── build-macos.sh                 # macOS build script
├── build-linux.sh                 # Linux build script
├── pyinstaller.spec               # PyInstaller configuration
└── scripts/
    ├── setup-env.sh               # Create .env file with MongoDB URI
    └── validate-ollama.py         # Health check for Ollama

dist/                              # Final executables (generated)
├── etnopapers-windows-v3.0.0.exe  (~150 MB)
├── Etnopapers-macos-v3.0.0.zip    (~150 MB)
└── etnopapers-linux-v3.0.0        (~150 MB)
```

**Structure Rationale**:
- **Existing frontend/backend split** preserved (reduces refactoring scope)
- **Frontend compiled to static assets** (dist/ directory) bundled into Python executable
- **Backend serves both API endpoints AND frontend static files** from embedded web server
- **PyInstaller bundles everything**: Python 3.11 runtime + all dependencies + frontend assets
- **launcher.py manages startup**: checks Ollama connectivity, reads/prompts for MongoDB URI, starts FastAPI server on port 8000
- **No Docker, no containers** – everything in single executable per OS

## Complexity Tracking

> **Justification for Constitution Principle Violation (Principle V. Portable Docker Deployment)**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| **Docker Compose → Standalone Executables** | Feature spec explicitly requires "absolutely stand alone" executables with no terminal/server visible to user. Desktop app model is fundamentally different from containerized backend. | Containerized solution requires Docker installation + docker-compose command for end users; violates "single click to run" requirement. End users are researchers without DevOps expertise; Docker adds friction. |
| **GPU Requirement Relaxed** | Specification allows CPU-only operation (higher latency). Desktop users may not have GPUs. Ollama still uses GPU if available; performance degrades gracefully on CPU. | Constitution mandates GPU for production (RTX 3060+). Not all researcher machines have NVIDIA GPUs. Relaxing to "CPU-only with warning" maintains functionality for broader audience while preserving performance path for GPU-enabled systems. |
| **Ollama Local-Only** | Same as Constitution (Principle I). Ollama runs locally on user's machine; no external AI API calls. No new compliance violation. | N/A |
| **MongoDB External** | Same as Constitution (Principle II). Users configure MongoDB URI (local or cloud); app doesn't bundle database. No new compliance violation. | N/A |

**Amended Governance**: This plan supercedes Constitution Principle V (Docker Compose) **for this phase only**. Future phases may return to containerized server deployment for UNRAID. This amended principle applies to current standalone refactoring only.
