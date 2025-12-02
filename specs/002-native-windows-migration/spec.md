# Feature Specification: Migrate to Native Windows Desktop Application

**Feature Branch**: `002-native-windows-migration`
**Created**: 2025-12-02
**Status**: Draft
**Input**: User description: "Migrate from Electron to native Windows desktop application with improved stability and redesigned UX while preserving core features (PDF extraction, data management, MongoDB sync)"

## User Scenarios & Testing *(mandatory)*

<!--
  IMPORTANT: User stories should be PRIORITIZED as user journeys ordered by importance.
  Each user story/journey must be INDEPENDENTLY TESTABLE - meaning if you implement just ONE of them,
  you should still have a viable MVP (Minimum Viable Product) that delivers value.
  
  Assign priorities (P1, P2, P3, etc.) to each story, where P1 is the most critical.
  Think of each story as a standalone slice of functionality that can be:
  - Developed independently
  - Tested independently
  - Deployed independently
  - Demonstrated to users independently
-->

### User Story 1 - Stable PDF Upload and Processing (Priority: P1)

Researchers can reliably upload PDF documents without application crashes or freezing. The application processes PDFs consistently and extracts metadata through local OLLAMA integration without performance degradation.

**Why this priority**: This is the core value proposition of EtnoPapers. The current Electron implementation has stability issues that prevent users from completing their primary task. A stable, responsive PDF processing pipeline is essential before any other features matter.

**Independent Test**: Can be fully tested by uploading a PDF document, verifying the application remains responsive during processing, and confirming extracted metadata appears in the local database. This demonstrates core stability and functionality.

**Acceptance Scenarios**:

1. **Given** a researcher has a valid PDF document, **When** they upload it through the application UI, **Then** the application processes it without crashing and displays extraction progress
2. **Given** OLLAMA is running on the local machine, **When** a PDF is being processed, **Then** the metadata extraction completes and stores results in the local JSON database
3. **Given** multiple PDFs are queued for processing, **When** the user performs other application actions (navigation, configuration), **Then** the application remains responsive without freezing

---

### User Story 2 - View and Manage Extracted Records (Priority: P1)

Researchers can view, edit, and delete extracted ethnobotanical records in a clean, intuitive interface. The application provides a responsive CRUD interface without lag or stability issues.

**Why this priority**: Data management is equally critical to extraction. Users need to review extracted data, correct errors, and organize records. The current Electron implementation has UI responsiveness issues that make this workflow frustrating.

**Independent Test**: Can be fully tested by viewing a list of extracted records, editing a single record's fields, and deleting a record. This validates the core data management workflow.

**Acceptance Scenarios**:

1. **Given** extracted records exist in the local database, **When** a researcher opens the records view, **Then** all records display in a responsive list with key fields visible
2. **Given** a researcher selects a record, **When** they edit a field (e.g., community name, species list), **Then** changes are saved to the local database immediately
3. **Given** a researcher selects a record, **When** they delete it, **Then** the record is removed from the local database and the UI updates instantly
4. **Given** the user is scrolling through many records, **When** they interact with the UI, **Then** the application remains smooth without lag

---

### User Story 3 - Configure OLLAMA and MongoDB Settings (Priority: P2)

Researchers can configure the local OLLAMA instance connection details, customize the metadata extraction prompt, and configure MongoDB connection parameters (for Atlas or local servers) in a straightforward settings interface.

**Why this priority**: Configuration is necessary for the application to function, but it's typically a one-time or infrequent task. Core extraction and data management take precedence.

**Independent Test**: Can be fully tested by saving OLLAMA connection settings, verifying the connection works, and saving MongoDB credentials. The application should validate connections before persisting settings.

**Acceptance Scenarios**:

1. **Given** a researcher opens the settings interface, **When** they enter OLLAMA host and port, **Then** the application tests the connection and confirms it works
2. **Given** a researcher customizes the metadata extraction prompt, **When** they save it, **Then** the new prompt is stored and used for future extractions
3. **Given** a researcher enters MongoDB credentials (URI), **When** they save, **Then** the connection is validated before storage

---

### User Story 4 - Sync Records to MongoDB (Priority: P2)

Researchers can select one or more extracted records and synchronize them to MongoDB (Atlas or local instance). The application shows sync progress, handles errors gracefully, and deletes local copies after successful upload.

**Why this priority**: MongoDB sync enables backup and enables data sharing/analysis across multiple machines. It's valuable but not essential for the initial migration to work. Researchers can work offline with local storage initially.

**Independent Test**: Can be fully tested by selecting records, clicking sync, observing upload progress, and confirming records appear in MongoDB and are removed from local storage.

**Acceptance Scenarios**:

1. **Given** records exist in the local database and MongoDB is configured, **When** a researcher selects records and initiates sync, **Then** a progress indicator shows upload status
2. **Given** sync completes successfully, **When** the operation finishes, **Then** synced records are removed from local storage and marked as synced
3. **Given** MongoDB connection fails during sync, **When** an error occurs, **Then** the user sees a clear error message and local records remain unchanged

---

### User Story 5 - Professional Windows Application Installer (Priority: P3)

The application is distributed via a professional Windows installer (MSI or equivalent) that allows users to install/uninstall cleanly without manual configuration.

**Why this priority**: Distribution and installation are important for user adoption, but the application core features must work first. This can follow once the native implementation is stable.

**Independent Test**: Can be fully tested by running the installer, verifying the application installs and launches correctly, and confirming uninstall removes all files.

**Acceptance Scenarios**:

1. **Given** the installer is downloaded, **When** the user runs it, **Then** the application installs with minimal user prompts
2. **Given** the application is installed, **When** the user launches it from Start Menu or shortcuts, **Then** it starts reliably
3. **Given** the application is installed, **When** the user uninstalls it, **Then** all files are removed and registry/configuration is cleaned

### Edge Cases

- What happens when OLLAMA is not running when the user attempts to process a PDF? System should display a clear error and guide the user to start OLLAMA.
- What happens when a user uploads a corrupted or non-PDF file? System should validate file format and show an appropriate error.
- What happens when local storage reaches capacity? System should warn the user before blocking uploads and guide them to sync to MongoDB.
- What happens when MongoDB connection fails during sync but some records were already uploaded? System should track partial state and allow retry.
- What happens when the user attempts to extract metadata from a PDF with no readable text (scanned image)? System should handle gracefully and report that extraction failed.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST accept PDF file uploads through a file picker interface
- **FR-002**: System MUST integrate with local OLLAMA instance for metadata extraction
- **FR-003**: System MUST extract and store ethnobotanical metadata according to the defined schema (`docs/estrutura.json`)
- **FR-004**: System MUST provide a list view of all extracted records with pagination or scrolling
- **FR-005**: System MUST allow users to edit individual record fields and persist changes to local storage
- **FR-006**: System MUST allow users to delete records from local storage
- **FR-007**: System MUST provide a settings/configuration interface for OLLAMA connection parameters
- **FR-008**: System MUST allow customization of the metadata extraction prompt in settings
- **FR-009**: System MUST validate OLLAMA connections before persisting configuration
- **FR-010**: System MUST provide a settings interface for MongoDB connection configuration (URI input)
- **FR-011**: System MUST validate MongoDB connections before persisting configuration
- **FR-012**: System MUST support selective sync of records to MongoDB (user selects which records to upload)
- **FR-013**: System MUST delete local copies of records after successful MongoDB sync
- **FR-014**: System MUST show sync progress to the user during MongoDB upload operations
- **FR-015**: System MUST handle sync errors gracefully and allow retry without data loss
- **FR-016**: System MUST store extracted records in local JSON files for offline access
- **FR-017**: System MUST support all mandatory extracted fields defined in `docs/estrutura.json`
- **FR-018**: System MUST maintain application responsiveness during PDF processing and database operations
- **FR-019**: System MUST never crash or freeze during normal user workflows

### Key Entities

- **Extracted Record**: Represents a single research paper's extracted ethnobotanical data, including titulo (title), autores (authors in APA format), ano (year), resumo (abstract in Portuguese), especies (plant species with vernacular/scientific names and use types), geographic data (país, estado, municipio, local, bioma), comunidade (community name/location), metodologia (research methodology), and sync status
- **Configuration**: Stores user preferences including OLLAMA connection details (host, port), MongoDB URI, and custom extraction prompt template
- **Sync Status**: Tracks which records have been synced to MongoDB versus which remain only in local storage

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Application remains responsive (UI remains interactive within 100ms of user action) during PDF processing and database operations
- **SC-002**: Researchers can upload and process PDF documents without application crashes or freezes
- **SC-003**: Application startup time is under 3 seconds on a standard Windows 10/11 machine
- **SC-004**: Researchers can view, edit, and delete 100+ extracted records without performance degradation
- **SC-005**: Metadata extraction from a typical 10-page PDF completes in under 5 minutes when OLLAMA is properly configured
- **SC-006**: MongoDB sync operations show clear progress feedback and complete without data loss
- **SC-007**: 100% of mandatory fields from `docs/estrutura.json` are successfully extracted and editable
- **SC-008**: Application can store at least 1,000 extracted records in local JSON storage before performance warning
- **SC-009**: Researchers complete the typical workflow (upload PDF → review extraction → edit data → optionally sync to MongoDB) in under 10 minutes with 95% success rate

## Assumptions

- OLLAMA is installed separately on the researcher's Windows machine and runs on a configurable host/port (default localhost:11434)
- MongoDB (Atlas or local) credentials will be provided by the researcher during initial configuration
- PDF documents are in standard searchable PDF format (not purely scanned images)
- Local JSON file storage is acceptable for offline work; MongoDB sync is optional
- The current `docs/estrutura.json` schema represents the complete set of required fields to extract
- "Native Windows desktop application" means a self-contained executable that doesn't require Node.js, Electron, or other runtimes; options include C# with WPF/WinUI, C++ with Win32/Qt, or similar frameworks
- The redesigned UX should maintain feature parity with current EtnoPapers but optimize for stability and responsiveness
- Text extraction from PDFs will be handled by an open-source or standard library (details deferred to planning phase)

## Notes

- This specification is technology-agnostic and does not prescribe C# vs. C++ vs. other native Windows technologies. The planning phase will evaluate technology options against project constraints.
- Current Electron implementation stability issues (crashes and freezes) are the primary driver for this migration. The native implementation must resolve these issues as the top priority.
- User interviews during requirements gathering indicated frustration with UI lag and occasional crashes during routine workflows. The redesigned application should emphasize stability and responsiveness.
