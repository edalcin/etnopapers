# Implementation Plan: EtnoPapers Desktop Application

**Branch**: `main` (single-branch workflow)
**Date**: 2025-12-01
**Spec**: [spec.md](./spec.md)
**Input**: Feature specification for ethnobotanical metadata extraction desktop application

## Summary

EtnoPapers is a Windows desktop application built with Electron, React, and TypeScript that enables ethnobotany researchers to extract metadata from PDF scientific papers using local AI (OLLAMA), manage records locally, and synchronize to MongoDB for backup and collaboration.

**Core Value Proposition**: Transform manual, time-consuming metadata extraction into an automated, AI-powered workflow that reduces hours of work to minutes while maintaining data quality and researcher control.

**Technical Approach**: Local-first desktop application using Electron for cross-platform desktop capabilities, pdf.js for text extraction, OLLAMA for AI-powered metadata extraction, lowdb for local JSON storage, and MongoDB for cloud backup.

---

## Technical Context

**Language/Version**: TypeScript 5.3+ / Node.js 20 LTS
**Primary Dependencies**:
- **Electron 28+**: Desktop application framework
- **React 18**: UI library
- **pdf.js**: PDF text extraction
- **OLLAMA**: External AI service (localhost REST API)
- **lowdb**: Local JSON database
- **mongodb**: Official MongoDB driver
- **Zustand**: State management
- **Tailwind CSS + shadcn/ui**: UI styling and components

**Storage**:
- **Local**: JSON file via lowdb (`Documents/EtnoPapers/data.json`)
- **Remote**: MongoDB (Atlas cloud or local server)

**Testing**: Vitest (unit/integration), React Testing Library (components), Playwright (E2E)

**Target Platform**: Windows 10+ (primary), with foundation for macOS/Linux future support

**Project Type**: Desktop application (Electron)

**Performance Goals**:
- PDF extraction: <2 minutes for typical 20-30 page paper
- UI responsiveness: <100ms for all interactions
- Local storage operations: <50ms
- MongoDB sync: <5 seconds per record (network dependent)

**Constraints**:
- Windows-only installer initially
- OLLAMA must be pre-installed by user
- Local storage limited to 1000 records
- PDFs must be text-based (no OCR support in v1.0)
- Single user per installation (no multi-user support)

**Scale/Scope**:
- Target users: 100-1000 ethnobotany researchers
- Typical dataset: 50-500 papers per researcher
- Installation base: 100-500 active installations initially
- Expected PDF size: 1-50 MB, 10-100 pages

---

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

**Constitution Status**: The project constitution is currently a template and has not been filled out with specific principles. For this implementation, we adopt standard desktop application development practices:

### Adopted Principles for This Project

1. **User-First Design**
   - Prioritize user experience and workflow efficiency
   - All features must directly support researcher tasks
   - Error messages must be actionable and non-technical

2. **Local-First Architecture**
   - Application works fully offline (except MongoDB sync)
   - Local data is primary; cloud is backup
   - No data sent to external services except user-configured MongoDB

3. **Test-Driven Development (Encouraged)**
   - Critical paths require tests (extraction pipeline, data validation, sync logic)
   - UI components should have basic rendering tests
   - Integration tests for service interactions
   - **Note**: Full TDD (tests before implementation) is encouraged but not strictly enforced for this project

4. **Simplicity & Maintainability**
   - Use standard patterns and libraries
   - Avoid premature optimization
   - Code should be readable and well-documented
   - Complexity requires justification

5. **Privacy & Security**
   - No telemetry or analytics without explicit user consent
   - PDF files never leave user's machine (except as text to local OLLAMA)
   - MongoDB credentials stored locally with optional encryption
   - No unnecessary data collection

### Phase 0 Gate: ✅ **PASSED**
- Technology choices justified in research.md
- All dependencies have clear rationale
- Architecture supports user requirements
- Privacy and security considered

### Phase 1 Gate: ✅ **PASSED**
- Data model matches requirements
- Service interfaces follow separation of concerns
- No unnecessary complexity introduced
- All technical decisions traceable to requirements

---

## Project Structure

### Documentation (this feature)

```text
specs/main/
├── spec.md              # Feature specification
├── plan.md              # This file (implementation plan)
├── research.md          # Technology decisions and rationale
├── data-model.md        # Complete data model with validation
├── quickstart.md        # Developer onboarding guide
├── contracts/           # Service interface contracts
│   └── service-interfaces.ts
└── tasks.md             # Task breakdown (NOT yet created - use /speckit.tasks)
```

### Source Code (repository root)

```text
etnopapers/
├── src/
│   ├── main/                      # Electron main process (Node.js)
│   │   ├── index.ts               # Entry point, window creation
│   │   ├── services/              # Business logic services
│   │   │   ├── PDFProcessingService.ts
│   │   │   ├── OLLAMAService.ts
│   │   │   ├── DataStorageService.ts
│   │   │   ├── MongoDBSyncService.ts
│   │   │   ├── ConfigurationService.ts
│   │   │   ├── ExtractionPipelineService.ts
│   │   │   ├── ValidationService.ts
│   │   │   └── LoggerService.ts
│   │   └── ipc/                   # IPC handlers (expose services to renderer)
│   │       ├── pdfHandlers.ts
│   │       ├── ollamaHandlers.ts
│   │       ├── storageHandlers.ts
│   │       ├── syncHandlers.ts
│   │       └── configHandlers.ts
│   │
│   ├── renderer/                  # React UI (browser environment)
│   │   ├── App.tsx                # Root component with routing
│   │   ├── main.tsx               # React entry point
│   │   ├── components/            # Reusable UI components
│   │   │   ├── layout/
│   │   │   │   ├── Sidebar.tsx
│   │   │   │   ├── StatusBar.tsx
│   │   │   │   └── Header.tsx
│   │   │   ├── common/
│   │   │   │   ├── Button.tsx
│   │   │   │   ├── Input.tsx
│   │   │   │   ├── Select.tsx
│   │   │   │   ├── Toast.tsx
│   │   │   │   └── Modal.tsx
│   │   │   ├── records/
│   │   │   │   ├── RecordCard.tsx
│   │   │   │   ├── RecordGrid.tsx
│   │   │   │   ├── RecordForm.tsx
│   │   │   │   └── RecordFilters.tsx
│   │   │   ├── upload/
│   │   │   │   ├── FileDropZone.tsx
│   │   │   │   ├── ExtractionProgress.tsx
│   │   │   │   └── ExtractionResults.tsx
│   │   │   └── sync/
│   │   │       ├── SyncPanel.tsx
│   │   │       └── SyncProgress.tsx
│   │   ├── pages/                 # Page-level components
│   │   │   ├── HomePage.tsx
│   │   │   ├── UploadPage.tsx
│   │   │   ├── RecordsPage.tsx
│   │   │   ├── SettingsPage.tsx
│   │   │   └── AboutPage.tsx
│   │   ├── stores/                # Zustand state stores
│   │   │   ├── useAppStore.ts
│   │   │   ├── useRecordsStore.ts
│   │   │   └── useExtractionStore.ts
│   │   ├── services/              # Frontend service clients (call IPC)
│   │   │   ├── PDFServiceClient.ts
│   │   │   ├── OLLAMAServiceClient.ts
│   │   │   ├── StorageServiceClient.ts
│   │   │   ├── SyncServiceClient.ts
│   │   │   └── ConfigServiceClient.ts
│   │   ├── hooks/                 # Custom React hooks
│   │   │   ├── useRecords.ts
│   │   │   ├── useExtraction.ts
│   │   │   └── useConnections.ts
│   │   └── utils/                 # UI utility functions
│   │       ├── formatters.ts
│   │       ├── validators.ts
│   │       └── i18n.ts
│   │
│   ├── shared/                    # Shared between main and renderer
│   │   ├── types/                 # TypeScript interfaces
│   │   │   ├── article.ts
│   │   │   ├── config.ts
│   │   │   ├── errors.ts
│   │   │   └── services.ts
│   │   └── utils/                 # Shared utilities
│   │       ├── titleNormalizer.ts
│   │       ├── authorFormatter.ts
│   │       └── languageDetector.ts
│   │
│   └── preload/                   # Electron preload scripts
│       └── index.ts               # Expose IPC to renderer securely
│
├── tests/
│   ├── unit/                      # Unit tests
│   │   ├── services/
│   │   ├── components/
│   │   └── utils/
│   ├── integration/               # Integration tests
│   │   ├── extraction-pipeline.test.ts
│   │   ├── sync-workflow.test.ts
│   │   └── storage-operations.test.ts
│   ├── e2e/                       # Playwright E2E tests
│   │   ├── upload-and-extract.spec.ts
│   │   ├── manage-records.spec.ts
│   │   └── sync-to-mongodb.spec.ts
│   └── fixtures/                  # Test data
│       ├── test-paper.pdf
│       └── mock-extraction.json
│
├── resources/                     # Application resources
│   ├── icons/                     # App icons (Windows ICO, etc.)
│   ├── installers/                # Installer configuration
│   └── locales/                   # i18n translation files
│       ├── pt-BR.json
│       └── en-US.json
│
├── docs/                          # Project documentation
│   ├── estrutura.json             # Data structure reference
│   ├── etnopapers.png             # Application logo
│   ├── etnopapers_white.png       # Logo variant
│   └── promptInicial.txt          # Original project description
│
├── specs/                         # Feature specifications (SpecKit)
│   └── main/                      # Current feature
│
├── .specify/                      # SpecKit framework
├── .claude/                       # Claude Code configuration
├── electron-builder.config.js     # Installer configuration
├── vite.config.ts                 # Vite build configuration
├── vitest.config.ts               # Vitest test configuration
├── playwright.config.ts           # Playwright E2E configuration
├── tsconfig.json                  # TypeScript configuration
├── package.json                   # Dependencies and scripts
├── pnpm-lock.yaml                 # Dependency lock file
├── .gitignore                     # Git ignore rules
├── README.md                      # User documentation (Portuguese)
└── CLAUDE.md                      # Development guide

```

**Structure Decision**: Single project structure (Option 1 from template) with clear separation between Electron main process (`src/main/`) and React renderer process (`src/renderer/`). This matches standard Electron application patterns and keeps the codebase organized by execution context. Shared code lives in `src/shared/` to avoid duplication.

---

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

This section intentionally left empty - no constitution violations to justify.

All architectural decisions follow standard desktop application practices:
- Electron for desktop: Industry standard for cross-platform desktop apps
- Service-oriented architecture: Standard separation of concerns
- IPC communication: Required by Electron architecture
- Local + cloud storage: Matches user requirements for offline work + backup

---

## Implementation Phases Overview

### Phase 0: Research & Technology Decisions ✅ COMPLETE
**Artifact**: `research.md`

Key decisions documented:
- Electron + React + TypeScript stack
- pdf.js for extraction
- OLLAMA REST API integration
- lowdb for local storage
- MongoDB official driver
- Zustand for state management
- Vitest + Playwright for testing

### Phase 1: Design & Contracts ✅ COMPLETE
**Artifacts**: `data-model.md`, `contracts/`, `quickstart.md`

Delivered:
- Complete TypeScript data model with Zod validation
- Service interfaces for all main process services
- IPC contract patterns
- Developer onboarding guide
- Data transformation logic

### Phase 2: Task Breakdown ⏳ NEXT STEP
**Artifact**: `tasks.md` (use `/speckit.tasks` command)

Will break implementation into:
- P1 tasks: Core MVP (extraction + local storage)
- P2 tasks: Record management CRUD
- P3 tasks: MongoDB synchronization
- P1 tasks: Configuration and setup

### Phase 3: Implementation ⏳ PENDING
**Command**: `/speckit.implement`

Execute tasks following TDD principles where applicable.

---

## Architectural Patterns

### Service Layer Pattern

All business logic encapsulated in services with clear interfaces:

```
UI Component → Service Client (renderer) → IPC → Handler → Service (main)
```

**Benefits**:
- Testable in isolation
- Clear boundaries between layers
- Easy to mock for testing
- Can swap implementations

### Local-First Data Flow

```
User Action → Update Local Store → Update UI Immediately → Background Sync (if needed)
```

**Benefits**:
- Fast, responsive UI
- Works offline
- User maintains control
- Sync is explicit user action

### Event-Driven Communication

```
Main Process ↔ IPC Events ↔ Renderer Process
```

**Security**: Preload script exposes only safe, validated APIs to renderer.

---

## Key Technical Challenges & Solutions

### Challenge 1: Large PDF Processing

**Problem**: 100+ page PDFs could block UI and consume memory

**Solution**:
- Stream processing in main process (background)
- Progress updates via IPC events
- Timeout after 60 seconds
- Memory limits enforced
- User can cancel extraction

### Challenge 2: OLLAMA Response Parsing

**Problem**: AI responses may be inconsistent or malformed JSON

**Solution**:
- Retry logic with prompt refinement
- Zod schema validation for all extracted data
- Fallback to manual entry if extraction fails
- Save raw response for debugging
- Confidence scores to flag uncertain extractions

### Challenge 3: MongoDB Connection Reliability

**Problem**: Network issues, authentication failures, timeouts

**Solution**:
- Connection health checks before upload
- Retry logic with exponential backoff
- Keep records local on failure
- Clear error messages with actionable guidance
- Test connection button in settings

### Challenge 4: Abstract Translation

**Problem**: Abstracts must always be in Portuguese, but papers may be in English

**Solution**:
- Language detection heuristics
- If not Portuguese, request translation from OLLAMA
- User can review and edit translation
- Cache translated abstracts to avoid re-translation

### Challenge 5: Data Validation

**Problem**: Extracted data may be incomplete or invalid

**Solution**:
- Zod schemas for runtime validation
- Mandatory field checks before save
- Visual indicators for missing optional fields
- Suggestions for common format errors (e.g., scientific names)
- User can save incomplete records with warnings

---

## Security Considerations

### Data Privacy
- **No external data transmission** except to user-configured MongoDB
- **PDFs processed locally**, never uploaded
- **Immediately delete PDFs** after extraction
- **No telemetry** or usage tracking

### Credential Storage
- MongoDB URI stored in electron-store (encrypted file)
- OLLAMA requires no authentication (localhost only)
- Configuration file permissions restricted to current user

### Input Validation
- File type verification (magic numbers, not just extension)
- Size limits (50 MB max per PDF)
- Sanitize all extracted text before display (prevent XSS)
- Validate MongoDB URIs before connection attempt

### Process Isolation
- Main process (privileged) separated from renderer (untrusted)
- Preload script exposes minimal, validated API surface
- Content Security Policy (CSP) restricts renderer capabilities

---

## Performance Optimizations

### UI Responsiveness
- Virtual scrolling for record lists (render only visible items)
- Debounce search/filter inputs (300ms)
- Lazy load record details on demand
- Pagination for large datasets

### PDF Processing
- Stream text extraction (don't load entire file to memory)
- Worker threads for CPU-intensive operations
- Progress callbacks every 5%
- Cancellable operations

### Local Storage
- Single JSON file for simplicity (lowdb handles atomicity)
- In-memory index for fast searches
- Auto-save with debouncing (30 seconds)
- Compact storage on app exit

### MongoDB Sync
- Batch uploads (up to 10 concurrent)
- Connection pooling
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
- All errors logged to file with full stack trace
- User-facing message shown in UI (simplified)
- Option to view details or copy error for support

**Log Levels**:
- `error`: Unexpected failures requiring user action
- `warn`: Degraded functionality but operation continues
- `info`: Normal operations (startup, config changes, sync)
- `debug`: Detailed trace for troubleshooting

**Log Location**: `%APPDATA%/EtnoPapers/logs/app-YYYY-MM-DD.log`

### Error Recovery

**Graceful Degradation**:
- If OLLAMA unavailable: Show message, disable upload button
- If MongoDB unreachable: Allow local work, show sync unavailable
- If extraction fails: Provide manual entry form
- If validation fails: Highlight errors, allow saving with warnings

---

## Internationalization (i18n)

**Languages**:
- **pt-BR** (Brazilian Portuguese): Default, primary
- **en-US** (English): Secondary, for international users

**Translation Approach**:
- react-i18next for UI strings
- JSON translation files in `resources/locales/`
- Language setting in configuration (persisted)
- Dynamic language switching without restart

**What's Translated**:
- All UI text (buttons, labels, headings)
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

### Unit Tests (Vitest)

**Coverage Goals**: >80% for:
- Utility functions (formatters, validators, normalizers)
- Data model validation logic
- Service business logic (mocked dependencies)

**Example**:
```typescript
describe('normalizeTitle', () => {
  it('converts to proper case', () => {
    expect(normalizeTitle('uso DE plantas')).toBe('Uso de Plantas')
  })

  it('preserves acronyms', () => {
    expect(normalizeTitle('uso de DNA em plantas')).toBe('Uso de DNA em Plantas')
  })
})
```

### Component Tests (React Testing Library)

**Coverage Goals**: All user-facing components

**Test Focus**:
- Rendering with props
- User interactions (clicks, inputs)
- State updates
- Error states

**Example**:
```typescript
describe('RecordCard', () => {
  it('displays article title and authors', () => {
    render(<RecordCard record={mockRecord} />)
    expect(screen.getByText(mockRecord.titulo)).toBeInTheDocument()
    expect(screen.getByText(mockRecord.autores[0])).toBeInTheDocument()
  })
})
```

### Integration Tests (Vitest)

**Coverage Goals**: Critical workflows

**Test Scenarios**:
- End-to-end extraction pipeline (PDF → text → AI → validation → storage)
- MongoDB sync workflow (select → upload → delete local)
- Configuration persistence (save → restart → load)

**Example**:
```typescript
describe('Extraction Pipeline', () => {
  it('extracts metadata from PDF end-to-end', async () => {
    const pdfPath = './fixtures/test-paper.pdf'
    const result = await extractionPipeline.extractFromPDF(pdfPath)

    expect(result.titulo).toBeTruthy()
    expect(result.autores.length).toBeGreaterThan(0)
    expect(result.ano).toBeGreaterThan(1500)
    expect(result.resumo).toBeTruthy()
  })
})
```

### E2E Tests (Playwright)

**Coverage Goals**: Complete user journeys

**Test Scenarios**:
1. **Upload & Extract**: Upload PDF → View extraction → Edit data → Save
2. **Manage Records**: View records → Filter → Edit → Delete
3. **Sync to MongoDB**: Select records → Configure MongoDB → Upload → Verify deletion
4. **Configuration**: Set OLLAMA prompt → Set MongoDB URI → Test connections

**Example**:
```typescript
test('complete extraction workflow', async ({ page }) => {
  await page.goto('/')

  // Upload PDF
  await page.click('[data-testid="upload-page-link"]')
  await page.setInputFiles('input[type="file"]', './fixtures/test-paper.pdf')

  // Wait for extraction
  await page.waitForSelector('[data-testid="extraction-complete"]')

  // Edit extracted data
  await page.fill('[data-testid="titulo-input"]', 'Edited Title')
  await page.click('[data-testid="save-button"]')

  // Verify saved
  await expect(page.locator('[data-testid="success-toast"]')).toBeVisible()
})
```

---

## Build & Distribution

### Development Build

```bash
pnpm dev
```

- Hot reload for renderer
- DevTools enabled
- Source maps for debugging
- Loose error checking

### Production Build

```bash
pnpm build
```

- Minified bundles
- Tree-shaking
- No DevTools
- Strict error checking

### Installer Creation

```bash
pnpm dist
```

**Outputs** (Windows):
- `dist/EtnoPapers-Setup-1.0.0.exe`: NSIS installer (recommended)
- `dist/EtnoPapers-1.0.0.msi`: Windows Installer format
- `dist/EtnoPapers-1.0.0-win.zip`: Portable version

**Installer Features**:
- Install to Program Files
- Start Menu shortcut
- Desktop shortcut (optional)
- Uninstaller
- File associations (future)
- Auto-update capability (future)

### Code Signing (Future)

For production releases, sign Windows installer with Authenticode certificate to avoid SmartScreen warnings.

---

## Deployment & Distribution

### Initial Release (v1.0.0)

**Distribution Method**: Manual download from website/GitHub releases

**Installation Steps for Users**:
1. Download installer (.exe or .msi)
2. Run installer
3. Install OLLAMA separately (prerequisite)
4. Launch EtnoPapers
5. Configure OLLAMA and MongoDB in settings

### Future Releases

**Auto-Update** (v1.1.0+):
- electron-updater for automatic updates
- Check for updates on startup
- Download in background
- Install on restart
- Notify user of new version

### Release Checklist

- [ ] All tests passing
- [ ] Version bumped in package.json
- [ ] CHANGELOG.md updated
- [ ] README.md updated (if needed)
- [ ] Build installers for Windows
- [ ] Test installation on clean Windows machine
- [ ] Create GitHub release with installers attached
- [ ] Update website with download links
- [ ] Announce release to users

---

## Monitoring & Observability

### Logging

**Locations**:
- Application logs: `%APPDATA%/EtnoPapers/logs/`
- Electron logs: Same directory
- Crash reports: `%APPDATA%/EtnoPapers/crashes/`

**Log Rotation**:
- Keep last 7 days
- Max 10 MB per day
- Compress old logs

### Metrics (Optional - Future)

**Privacy-Respecting Metrics** (opt-in):
- Usage patterns (features used, not content)
- Error rates (types, not details)
- Performance metrics (extraction times, crash rates)
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
- Installation guide
- Usage instructions
- Troubleshooting common issues
- Contact information

**For Developers**:
- This implementation plan
- Quickstart guide
- API documentation (JSDoc comments)
- Architecture decision records

### Support Channels

- **Email**: edalcin@jbrj.gov.br (primary contact)
- **GitHub Issues**: Bug reports, feature requests
- **In-App Help**: Links to documentation, troubleshooting guides

### Known Limitations (v1.0.0)

- Windows-only installer
- No OCR for scanned PDFs
- No batch processing (one PDF at a time)
- No collaborative features
- No data visualization
- No export to other formats (only JSON/MongoDB)

---

## Roadmap (Future Versions)

### v1.1.0 - Polish & Refinement
- Auto-update capability
- Performance optimizations
- Bug fixes from user feedback
- Enhanced error messages

### v1.2.0 - Productivity Features
- Batch PDF processing
- Export to CSV/Excel
- Advanced search and filtering
- Custom field templates

### v1.3.0 - Extended Platform Support
- macOS installer
- Linux AppImage
- Shared MongoDB collections (collaboration)

### v2.0.0 - Advanced Features
- OCR for scanned PDFs
- Reference manager integration (Zotero, Mendeley)
- Data visualization dashboard
- Statistical analysis tools
- Mobile companion app

---

## Success Criteria (from Spec)

**Technical Success Metrics**:
- ✅ Extract metadata from PDF in <2 minutes (90% of cases)
- ✅ 90%+ accuracy for mandatory fields
- ✅ Support 100+ local records without degradation
- ✅ 95%+ sync success rate to MongoDB
- ✅ <1% crash rate per session
- ✅ 95%+ successful installations on Windows 10+

**User Success Metrics**:
- ✅ 80%+ task completion rate (upload → extract → save → sync)
- ✅ 90%+ user satisfaction (post-launch survey)
- ✅ 50%+ reduction in manual metadata entry time

---

## Risks & Mitigation

| Risk | Impact | Probability | Mitigation |
|------|--------|-------------|------------|
| **OLLAMA API changes** | High | Medium | Version detection, abstraction layer, fallback prompts |
| **Low extraction accuracy** | High | Medium | Prompt engineering, user review workflow, confidence scores |
| **User config errors** | Medium | High | Validation, connection testing, helpful error messages, defaults |
| **MongoDB connection issues** | Medium | Medium | Offline-first design, retry logic, clear status indicators |
| **Performance with large PDFs** | Medium | Low | Streaming, timeouts, file size limits, progress feedback |
| **Data loss** | High | Low | Auto-save, backup reminders, crash recovery |
| **Installation failures** | Medium | Low | Detailed installer logs, troubleshooting guide, prerequisite checks |
| **Windows version compatibility** | Low | Low | Target Windows 10+, test on multiple versions |

---

## Conclusion

This implementation plan provides a comprehensive blueprint for building EtnoPapers v1.0.0. The architecture balances:

- **User needs**: Fast, reliable metadata extraction with researcher control
- **Technical feasibility**: Proven technologies with clear integration patterns
- **Maintainability**: Standard patterns, clear boundaries, comprehensive tests
- **Extensibility**: Plugin-ready extraction pipeline, custom fields, modular services

**Next Steps**:
1. Run `/speckit.tasks` to generate detailed task breakdown
2. Review tasks and prioritize implementation order
3. Set up development environment (follow quickstart.md)
4. Begin implementation following TDD principles
5. Iterate based on testing and user feedback

**Estimated Timeline**:
- Core MVP (P1): 4-6 weeks
- CRUD + Sync (P2-P3): 3-4 weeks
- Testing + Polish: 2-3 weeks
- **Total**: 9-13 weeks for v1.0.0

This plan is living documentation and will be updated as implementation progresses and requirements evolve.

---

**Plan Complete** ✅

Ready for task breakdown with `/speckit.tasks`.
