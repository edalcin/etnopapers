# Research & Technology Decisions: EtnoPapers

**Date**: 2025-12-01
**Feature**: Ethnobotanical Metadata Extraction Desktop Application
**Phase**: 0 - Research & Technology Selection

## Executive Summary

This document captures the research and technology decisions for building a Windows desktop application that extracts ethnobotanical metadata from PDF scientific papers using local AI (OLLAMA) and syncs data to MongoDB.

## Critical Technology Decisions

### 1. Desktop Application Framework

**Decision**: Electron with React/TypeScript

**Rationale**:
- **Cross-platform foundation**: While initially Windows-only, Electron provides path to macOS/Linux with minimal changes
- **Modern UI capabilities**: React enables clean, modern interface as required
- **Rich ecosystem**: Extensive libraries for PDF processing, file handling, database connectivity
- **Developer productivity**: TypeScript provides type safety for complex data models
- **Native integration**: Electron provides access to file system, native dialogs, system tray
- **Easy installer**: Electron-builder provides professional Windows installer (.exe, .msi)

**Alternatives Considered**:
- **WPF/WinUI (.NET)**: More native Windows feel, but requires C# expertise and locks into Windows-only
- **Qt/Python**: Good for desktop apps, but less modern UI capabilities and smaller ecosystem for PDF/AI
- **Tauri**: Smaller footprint than Electron, but newer with less mature ecosystem

**Supporting Libraries**:
- `electron`: Core desktop framework
- `electron-builder`: Professional installers
- `react` + `typescript`: UI layer
- `electron-store`: Configuration persistence
- `electron-window-state`: Window state management

---

### 2. PDF Processing

**Decision**: pdf.js (Mozilla) + Custom extraction pipeline

**Rationale**:
- **Pure JavaScript**: Runs in Electron without native dependencies
- **Battle-tested**: Powers Firefox PDF viewer, handles complex PDFs
- **Text extraction**: Reliable text layer extraction for AI processing
- **No server required**: All processing happens locally
- **Active maintenance**: Mozilla-backed, regular updates

**Alternatives Considered**:
- **pdf-parse (Node)**: Simpler API but less robust for complex PDFs
- **Apache PDFBox (Java)**: More features but requires JVM dependency
- **PyPDF2 (Python)**: Would require embedding Python runtime

**Implementation Approach**:
1. Extract full text from PDF using pdf.js
2. Send extracted text to OLLAMA for structured extraction
3. Parse OLLAMA response into data model
4. Validate and present to user for review

---

### 3. Local AI Integration (OLLAMA)

**Decision**: REST API client to local OLLAMA service

**Rationale**:
- **OLLAMA is pre-installed**: Treat as external dependency (user responsibility)
- **HTTP API**: OLLAMA exposes REST endpoints at http://localhost:11434
- **Model agnostic**: Works with any OLLAMA-compatible model
- **Streaming support**: Can show real-time extraction progress
- **Simple integration**: Standard HTTP client (axios)

**API Integration Pattern**:
```typescript
interface OLLAMAClient {
  checkHealth(): Promise<boolean>
  extractMetadata(text: string, prompt: string): Promise<ExtractedData>
  streamExtraction(text: string, prompt: string): AsyncIterator<PartialData>
}
```

**Connection Strategy**:
- Check OLLAMA status on app startup
- Display connection indicator in UI
- Disable PDF upload if OLLAMA unavailable
- Provide helpful error messages with setup instructions

**Prompt Engineering**:
- Store customizable system prompt in configuration
- Default prompt optimized for ethnobotanical extraction
- JSON output format for structured parsing
- Include examples in prompt for few-shot learning

---

### 4. Data Storage

**Decision**: Local JSON file + MongoDB (remote)

**Local Storage**:
- **Library**: `lowdb` (lightweight JSON database)
- **File format**: Single JSON file with array of records
- **Schema validation**: Zod for runtime type checking
- **Location**: User documents folder (e.g., `Documents/EtnoPapers/data.json`)

**Rationale**:
- Simple file-based storage for local-first workflow
- Easy to backup manually (single file)
- Fast read/write for small datasets (< 1000 records)
- No database installation required locally
- Human-readable for troubleshooting

**MongoDB Integration**:
- **Library**: `mongodb` (official Node.js driver)
- **Connection**: User-provided URI (Atlas or local)
- **Schema**: Same structure as local JSON for consistency
- **Sync strategy**: One-way upload (local → MongoDB), then delete local

**Rationale for MongoDB**:
- Industry-standard NoSQL for JSON documents
- Free tier on Atlas suitable for researchers
- Flexible schema matches ethnobotanical data variability
- Good Node.js driver support
- Query capabilities for future analysis

---

### 5. UI Framework & Design

**Decision**: React + Tailwind CSS + shadcn/ui components

**Rationale**:
- **React**: Component-based, large ecosystem, familiar to developers
- **Tailwind CSS**: Utility-first CSS for rapid modern UI development
- **shadcn/ui**: Beautiful, accessible components (not a library, copy-paste approach)
- **Design system**: Consistent, clean, modern aesthetic as required

**Layout Structure**:
- **Sidebar navigation**: Home, Upload, Records, Settings, About
- **Main content area**: Dynamic based on active view
- **Status bar**: OLLAMA and MongoDB connection indicators
- **Notifications**: Toast messages for feedback

**Key UI Components Needed**:
- File upload zone (drag-and-drop)
- Data grid for record management
- Form builder for record editing
- Progress indicators for long operations
- Connection status indicators
- Settings panel

---

### 6. State Management

**Decision**: Zustand (lightweight state manager)

**Rationale**:
- **Simplicity**: Less boilerplate than Redux
- **TypeScript-first**: Excellent type inference
- **Performance**: Only re-renders affected components
- **DevTools**: Integration with Redux DevTools
- **Size**: Small bundle size (~1KB)

**State Stores Needed**:
- `useAppStore`: Global app state (connections, settings)
- `useRecordsStore`: Record collection management
- `useExtractionStore`: PDF extraction progress

**Alternatives Considered**:
- **Redux**: Too much boilerplate for this app size
- **Context API**: Can cause unnecessary re-renders
- **MobX**: More magic, harder to debug

---

### 7. Testing Strategy

**Decision**: Vitest + React Testing Library + Playwright

**Rationale**:
- **Vitest**: Fast, Vite-native test runner with great TypeScript support
- **React Testing Library**: User-centric component testing
- **Playwright**: End-to-end testing for Electron app

**Test Coverage Goals**:
- Unit tests: Data models, validation logic, formatters
- Component tests: UI components in isolation
- Integration tests: OLLAMA client, MongoDB client
- E2E tests: Complete user workflows (upload → extract → save → sync)

**Mock Strategy**:
- Mock OLLAMA API responses for consistent testing
- Mock MongoDB for offline testing
- Test PDF fixtures for extraction testing

---

### 8. Configuration Management

**Decision**: electron-store + JSON schema validation

**Rationale**:
- **electron-store**: Simple key-value config storage
- **Type-safe**: TypeScript interfaces for configuration
- **Defaults**: Sensible defaults for first run
- **Encryption**: Option to encrypt sensitive values (MongoDB URI)

**Configuration Schema**:
```typescript
interface AppConfig {
  ollama: {
    url: string // default: http://localhost:11434
    model: string // default: llama2
    prompt: string // customizable extraction prompt
  }
  mongodb: {
    uri: string | null // user-provided
    database: string // default: etnopapers
    collection: string // default: articles
  }
  storage: {
    maxLocalRecords: number // default: 1000
    autoBackupReminder: boolean // default: true
    reminderInterval: number // days, default: 7
  }
  ui: {
    theme: 'light' | 'dark' // default: light
    language: 'pt-BR' | 'en-US' // default: pt-BR
  }
}
```

---

### 9. Build & Distribution

**Decision**: electron-builder with Windows-specific configuration

**Rationale**:
- **Professional installers**: Creates .exe and .msi files
- **Code signing**: Support for authenticode signing (future)
- **Auto-updater**: Built-in update mechanism (future)
- **Asset compression**: Smaller download size

**Distribution Artifacts**:
- `EtnoPapers-Setup-1.0.0.exe`: NSIS installer (recommended)
- `EtnoPapers-1.0.0.msi`: Windows Installer format
- Portable version (optional): No installation required

**Installation Features**:
- Install to Program Files
- Start menu shortcut
- Desktop shortcut (optional)
- File association for .etnopapers files (future)
- Uninstaller

---

### 10. Error Handling & Logging

**Decision**: winston for logging + Sentry for error tracking (optional)

**Rationale**:
- **winston**: Flexible logging library with multiple transports
- **Sentry**: Cloud error tracking for production issues
- **Local logs**: Help users debug issues
- **Privacy**: No PII in error reports

**Logging Strategy**:
- Application logs: `%APPDATA%/EtnoPapers/logs/`
- Rotation: Keep last 7 days
- Levels: error, warn, info, debug
- Format: JSON for machine parsing, pretty for human reading

**Error Boundaries**:
- React error boundaries to catch UI crashes
- Process error handlers for Electron main process
- User-friendly error messages in UI
- Technical details in logs

---

## Data Flow Architecture

```
┌─────────────┐
│   PDF File  │
└─────┬───────┘
      │
      ▼
┌─────────────────┐
│   pdf.js        │ ← Extract text
│   Extraction    │
└─────┬───────────┘
      │
      ▼
┌─────────────────┐
│   OLLAMA API    │ ← AI extraction with prompt
│   (localhost)   │
└─────┬───────────┘
      │
      ▼
┌─────────────────┐
│   Data Model    │ ← Parse & validate JSON
│   Validation    │
└─────┬───────────┘
      │
      ▼
┌─────────────────┐
│   User Review   │ ← Edit/approve extracted data
│   & Edit        │
└─────┬───────────┘
      │
      ▼
┌─────────────────┐
│   Local JSON    │ ← Save to lowdb
│   Storage       │
└─────┬───────────┘
      │
      ▼ (user-initiated sync)
┌─────────────────┐
│   MongoDB       │ ← Upload selected records
│   (Atlas/local) │
└─────────────────┘
```

---

## Performance Considerations

### PDF Processing
- **Large PDFs**: Stream processing for 100+ page documents
- **Concurrent uploads**: Queue system to prevent memory issues
- **Progress feedback**: Real-time progress bar during extraction

### Local Storage
- **1000 record limit**: Enforced to prevent JSON file bloat
- **Lazy loading**: Virtualized list for record grid
- **Search indexing**: In-memory index for fast filtering

### AI Extraction
- **Timeout**: 60-second limit per PDF
- **Retry logic**: Automatic retry on network errors
- **Streaming**: Use OLLAMA streaming API for responsiveness

---

## Security Considerations

### Data Privacy
- **No telemetry**: No data sent to external services except user-configured MongoDB
- **Local-first**: PDFs processed locally, immediately discarded
- **Encryption**: Optional encryption for MongoDB URI in config

### Input Validation
- **File type checking**: Verify PDF magic numbers
- **Size limits**: Max 50MB per PDF
- **Sanitization**: Sanitize all extracted text before display

### Network Security
- **HTTPS only**: Require HTTPS for MongoDB Atlas
- **Connection validation**: Verify MongoDB certificates
- **Error messages**: Don't leak sensitive connection details

---

## Internationalization (i18n)

**Decision**: react-i18next for Portuguese/English support

**Languages**:
- Portuguese (Brazilian): Primary, default
- English: Secondary, for international collaborators

**Translatable Elements**:
- All UI text
- Error messages
- Validation messages
- Default AI prompts (with language-specific variants)

**Not Translated**:
- Extracted data (stays in source language)
- Technical logs
- Configuration file keys

---

## Future Extensibility

**Planned for Phase 2+**:
- Export to CSV/Excel for analysis
- Batch PDF processing (multiple files at once)
- Custom field templates for different research domains
- OCR support for scanned PDFs
- Integration with reference managers (Zotero, Mendeley)
- Collaborative features (shared MongoDB collections)
- Advanced search and filtering
- Data visualization dashboard

**Architecture Decisions Supporting Future Work**:
- Plugin system for custom extractors
- Modular extraction pipeline
- Extensible data model (custom attributes)
- API-first MongoDB interaction (enables future mobile app)

---

## Development Environment

**Required Tools**:
- Node.js 20 LTS
- pnpm (preferred) or npm
- VS Code with TypeScript, ESLint, Prettier extensions
- Windows 10+ for testing

**Development Workflow**:
1. `pnpm install`: Install dependencies
2. `pnpm dev`: Start Electron in dev mode with hot reload
3. `pnpm test`: Run test suite
4. `pnpm build`: Build production app
5. `pnpm dist`: Create installer packages

---

## Risk Mitigation

| Risk | Mitigation |
|------|------------|
| **OLLAMA compatibility changes** | Abstract OLLAMA client behind interface, version detection |
| **PDF extraction failures** | Comprehensive error handling, user-facing troubleshooting guide |
| **Data loss** | Auto-save, backup reminders, crash recovery |
| **Performance with large datasets** | Enforced limits, virtual scrolling, pagination |
| **MongoDB connection issues** | Offline-first design, retry logic, clear error messages |
| **User configuration errors** | Validation, connection testing, helpful defaults |

---

## Success Metrics

- **Extraction accuracy**: >90% for mandatory fields (measured against test corpus)
- **Extraction speed**: <2 minutes per typical paper (20-30 pages)
- **Installation success**: >95% successful installs on Windows 10+
- **Crash rate**: <1% per session
- **User completion rate**: >80% complete PDF → MongoDB workflow

---

## Conclusion

The technology stack is designed to balance:
- **Developer productivity**: TypeScript, React, modern tooling
- **User experience**: Fast, responsive, offline-capable
- **Maintainability**: Standard libraries, clear architecture
- **Extensibility**: Modular design for future enhancements

All technology choices support the core goal: enable ethnobotany researchers to efficiently extract and manage metadata from scientific papers with minimal technical friction.
