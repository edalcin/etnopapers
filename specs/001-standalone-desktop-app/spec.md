# Feature Specification: Standalone Desktop Application with Embedded UI

**Feature Directory**: `specs/001-standalone-desktop-app`
**Created**: 2025-11-27
**Status**: Draft
**Working Branch**: `main` (no feature branch)

## User Scenarios & Testing

### User Story 1 - Download and Run Desktop Application (Priority: P1)

A researcher downloads a single executable file for their operating system and runs it directly from their file explorer or command line. The application starts automatically without requiring terminal commands, configuration files, or installation of additional dependencies. The application opens with all necessary UI elements ready to use.

**Why this priority**: This is the foundation of the entire feature. Without a working standalone executable, the application cannot be distributed or used by end users.

**Independent Test**: Can be fully tested by downloading the executable, running it, and verifying the UI loads with all core components visible. This delivers the core value of having a desktop app.

**Acceptance Scenarios**:

1. **Given** a Windows user has downloaded `etnopapers-windows-v3.0.0.exe`, **When** they double-click it, **Then** the application launches with the main UI visible (PDF upload section, article list, configuration) within 5 seconds
2. **Given** a macOS user has downloaded and extracted `Etnopapers-macos-v3.0.0.zip`, **When** they double-click `Etnopapers.app`, **Then** the application launches with the main UI visible within 5 seconds
3. **Given** a Linux user has downloaded `etnopapers-linux-v3.0.0`, **When** they execute it (e.g., `./etnopapers-linux-v3.0.0`), **Then** the application launches with the main UI visible within 5 seconds
4. **Given** the application is running, **When** a user closes the window, **Then** the application terminates cleanly without leaving orphaned processes

---

### User Story 2 - Configure MongoDB Connection on First Run (Priority: P1)

When a user runs the application for the first time, they encounter a setup dialog that guides them through configuring their MongoDB connection. The dialog is part of the desktop application (not a terminal or browser), allows them to enter or paste their MongoDB URI, and validates the connection before saving the configuration.

**Why this priority**: Users need to configure their database connection to use the application. Without this, the system cannot persist data. This must be intuitive and embedded in the UI.

**Independent Test**: Can be tested by running the application without a configuration file, entering a valid MongoDB URI, and verifying the application connects and allows proceeding to the main interface. Delivers the value of easy initial setup.

**Acceptance Scenarios**:

1. **Given** a user runs the application for the first time without a configuration file, **When** the application starts, **Then** a configuration dialog appears asking for MongoDB URI
2. **Given** the configuration dialog is open, **When** the user enters a valid MongoDB connection URI (e.g., `mongodb://localhost:27017/etnopapers`), **Then** the application validates the connection and shows a success confirmation
3. **Given** the user enters an invalid MongoDB URI, **When** they attempt to proceed, **Then** the application displays a clear error message and prevents progression
4. **Given** the user has entered a valid URI, **When** they complete the setup, **Then** the configuration is persisted so subsequent runs do not show the setup dialog

---

### User Story 3 - Upload PDF and Extract Metadata (Priority: P1)

A researcher clicks a button or drags and drops a PDF file into the embedded UI. The application processes the PDF using the local Ollama instance (no external API calls required), extracts ethnobotanical metadata, and displays the results in the application window for review and editing. The entire process happens within the application with no browser involvement.

**Why this priority**: This is the core functional value of the application—extracting ethnobotanical data from PDFs. Without this working seamlessly in the desktop UI, the application serves no purpose.

**Independent Test**: Can be tested by uploading a PDF file, waiting for processing, and verifying extracted metadata appears in the UI. Delivers the primary user value of automated metadata extraction.

**Acceptance Scenarios**:

1. **Given** the application is running with MongoDB configured and Ollama is accessible, **When** a user drags a PDF file onto the designated upload area, **Then** the file is accepted and processing begins
2. **Given** a PDF upload has begun, **When** processing completes, **Then** extracted metadata (title, authors, species, locations, etc.) is displayed in the UI in an editable form
3. **Given** extracted metadata is displayed, **When** the user reviews it and clicks "Save", **Then** the data is persisted to MongoDB
4. **Given** extracted metadata is displayed, **When** the user clicks "Discard", **Then** the data is not saved and the upload form returns to the default state
5. **Given** a PDF file is being processed, **When** Ollama is unavailable or times out, **Then** the application displays a clear error message guiding the user to ensure Ollama is running

---

### User Story 4 - View and Manage Articles (Priority: P2)

The researcher accesses a table or list view of all previously extracted and saved articles. They can sort, filter, and search through articles by title, author, year, species name, or location. They can select an article to view full details, edit metadata, or delete entries.

**Why this priority**: This enables researchers to manage their accumulated data and find articles they've previously processed. It's a core feature but not required for the initial MVP (which focuses on upload and extraction).

**Independent Test**: Can be tested independently by having articles in the database and verifying sort/filter functionality works. Delivers value for long-term data management.

**Acceptance Scenarios**:

1. **Given** the application has articles saved in MongoDB, **When** the user navigates to the "Articles" or "Library" section, **Then** a table displays all articles with sortable columns (title, authors, year, species count, location)
2. **Given** a list of articles is displayed, **When** the user clicks on a column header, **Then** the list sorts by that column
3. **Given** a list of articles is displayed, **When** the user types in a search box, **Then** the list filters to show only articles matching the search term (title, author, species)
4. **Given** an article is selected, **When** the user clicks "Edit", **Then** the metadata editing form opens with all current data populated for modification

---

### User Story 5 - Download Database Backup (Priority: P2)

A researcher can export their entire database (all articles and metadata) as a backup file from within the application UI. The backup is downloaded to their local file system as a ZIP archive containing the database dump, ensuring data portability and backup.

**Why this priority**: Data backup and portability are important for long-term research but not required for initial MVP functionality.

**Independent Test**: Can be tested by clicking a download button and verifying a ZIP file is created with database contents. Delivers the value of data security and portability.

**Acceptance Scenarios**:

1. **Given** the application is running with articles in the database, **When** the user clicks "Download Database Backup", **Then** a file dialog appears prompting them to choose a save location
2. **Given** the user has selected a save location, **When** they confirm the download, **Then** a ZIP file is created containing the database backup and saved to the specified location
3. **Given** a backup has been completed, **When** the user checks the file system, **Then** the ZIP file contains all articles and metadata in a portable format

---

### User Story 6 - Validate Ollama Connection (Priority: P3)

On application startup or when a PDF processing begins, the application checks that Ollama is running and accessible. If Ollama is not available, the application displays a clear message indicating the issue and prevents PDF processing until Ollama is confirmed to be running.

**Why this priority**: This ensures a good user experience when Ollama is not available (prevents confusing error messages during processing). It's a quality-of-life feature, not essential for MVP.

**Independent Test**: Can be tested by starting the app without Ollama running and verifying a clear message appears. Delivers improved user experience.

**Acceptance Scenarios**:

1. **Given** the application is running but Ollama is not accessible on `localhost:11434`, **When** the user attempts to upload a PDF, **Then** a dialog appears stating "Ollama is not running. Please start Ollama and try again"
2. **Given** Ollama is started after the warning appears, **When** the user retries the upload, **Then** processing proceeds normally

---

### Edge Cases

- What happens when a user tries to run the application on an unsupported OS (e.g., attempting to run Windows .exe on Linux)?
- How does the system handle a PDF that cannot be processed (corrupted, scanned image without OCR)?
- What occurs if MongoDB becomes unavailable after the initial configuration?
- How does the system handle very large PDF files (>100 MB)?
- What happens if the user closes the application while a PDF is being processed?
- How does the system behave if the user disconnects from the network while processing?

## Requirements

### Functional Requirements

- **FR-001**: System MUST launch as a standalone executable that does not require terminal/command line interaction
- **FR-002**: System MUST present all user interface elements within a native desktop application window
- **FR-003**: System MUST provide a configuration dialog on first run to collect and validate MongoDB URI
- **FR-004**: System MUST persist MongoDB configuration locally so it is available on subsequent runs
- **FR-005**: System MUST provide a drag-and-drop and/or file picker interface for PDF upload
- **FR-006**: System MUST extract PDF text content and send it to the local Ollama instance for metadata extraction
- **FR-007**: System MUST parse structured metadata output from Ollama (title, authors, species, locations, methodology, etc.)
- **FR-008**: System MUST display extracted metadata in an editable form for user review and correction
- **FR-009**: System MUST persist extracted metadata to MongoDB when user clicks "Save"
- **FR-010**: System MUST discard extracted metadata without saving when user clicks "Discard"
- **FR-011**: System MUST retrieve and display a list/table of all saved articles from MongoDB
- **FR-012**: System MUST support sorting articles by multiple columns (title, authors, year, species)
- **FR-013**: System MUST support filtering/searching articles by text terms
- **FR-014**: System MUST allow users to view, edit, and delete individual article records
- **FR-015**: System MUST export the entire MongoDB database as a ZIP archive backup
- **FR-016**: System MUST validate Ollama connectivity before attempting PDF processing
- **FR-017**: System MUST provide clear, user-friendly error messages for all failure scenarios
- **FR-018**: System MUST be buildable as standalone executables for Windows, macOS, and Linux from a single codebase
- **FR-019**: System MUST NOT require users to install or configure additional services beyond Ollama
- **FR-020**: System MUST NOT open or require a web browser for any functionality
- **FR-021**: System MUST use external MongoDB connection via MONGO_URI environment variable or configuration
- **FR-022**: System MUST use local Ollama instance (via OLLAMA_URL, default http://localhost:11434)

### Key Entities

- **Reference/Article**: The main entity representing an extracted scientific article with fields: title, authors, year, publication, DOI, species list, locations, methodology, biome, usage type, and status
- **Metadata Field**: Individual fields extracted from articles (title, author names, scientific names, locations, etc.)
- **Configuration**: Persisted settings for MongoDB URI and Ollama endpoint
- **Database Backup**: Exported collection of all articles in portable format (ZIP archive)

## Success Criteria

### Measurable Outcomes

- **SC-001**: Application launches from executable and displays main UI within 5 seconds on all three supported operating systems (Windows, macOS, Linux)
- **SC-002**: First-time user completes MongoDB configuration and sees the main interface within 2 minutes
- **SC-003**: PDF file upload and metadata extraction completes within 10 seconds for a typical 5-page ethnobotanical article (when Ollama is responsive)
- **SC-004**: Extracted metadata displays with >90% accuracy for standard fields (title, author count, species count) compared to manual review
- **SC-005**: Users can successfully save, retrieve, and edit previously extracted articles without loss of data
- **SC-006**: Database backup can be exported and the resulting ZIP file contains all articles in a verifiable format
- **SC-007**: Application handles network disconnection and Ollama unavailability without crashing (graceful error messages)
- **SC-008**: All three operating system executables are <200 MB each and can be downloaded and run by end users without installation
- **SC-009**: 95% of users successfully perform the primary workflow (upload PDF → review metadata → save) on first attempt without external help
- **SC-010**: Application closes cleanly without orphaned processes when user quits

## Assumptions

1. **Ollama is pre-installed**: Users are expected to have Ollama installed and running locally. The application will not bundle or manage Ollama installation.
2. **MongoDB access**: Users have access to a MongoDB instance (local or cloud) and know their connection URI.
3. **Model availability**: Ollama has the required model (Qwen2.5-7B-Instruct) already downloaded and available.
4. **Operating system support**: We will support Windows 10+, macOS 10.13+, and modern Linux distributions with glibc 2.29+.
5. **File size limits**: PDFs are expected to be <100 MB each; larger files may timeout during processing.
6. **Network assumptions**: MongoDB connection is stable; temporary network interruptions are handled gracefully.

## Out of Scope

- Multi-user collaboration or authentication
- Real-time sync across multiple computers
- Integration with cloud storage providers
- Advanced NLP tuning or custom model training
- Automatic Ollama installation or version management
- Mobile applications (Windows/macOS/Linux desktop only)
- External API integrations beyond MongoDB and Ollama
