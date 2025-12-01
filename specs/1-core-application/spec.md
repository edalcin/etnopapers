# Feature Specification: EtnoPapers - Ethnobotanical Metadata Extraction Desktop Application

**Feature Branch**: `main` (single-branch workflow)
**Created**: 2025-12-01
**Status**: Draft
**Input**: User description: "Desktop application for Windows that automates extraction and cataloging of ethnobotanical metadata from scientific papers about traditional plant use by indigenous and traditional communities"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Extract Metadata from PDF Articles (Priority: P1)

A researcher has a scientific paper in PDF format about traditional plant use. They need to extract structured data including article metadata (title, authors, year, abstract) and ethnobotanical information (species, communities, locations, methodology).

**Why this priority**: This is the core value proposition of the application. Without PDF extraction capability, the application cannot fulfill its primary purpose. This forms the minimum viable product.

**Independent Test**: Can be fully tested by uploading a single PDF file and verifying that extracted data matches expected structured format with mandatory fields (title, authors, year, abstract in Portuguese).

**Acceptance Scenarios**:

1. **Given** the AI service is connected and running, **When** researcher uploads a scientific paper PDF, **Then** system extracts and displays all mandatory fields (title with normalized case, authors in APA format, year, abstract in Brazilian Portuguese)
2. **Given** a PDF is being processed, **When** extraction is in progress, **Then** system displays real-time processing status and progress information
3. **Given** extraction completes successfully, **When** results are presented, **Then** researcher can immediately review and edit all extracted fields
4. **Given** extracted data is displayed, **When** researcher reviews the data, **Then** optional fields (species, communities, locations, biome, methodology) are also populated when available in the source document

---

### User Story 2 - Manage Local Record Collection (Priority: P2)

A researcher needs to review, edit, and manage all previously extracted records before syncing them to the cloud database. They want to create, view, update, and delete records as needed.

**Why this priority**: Once extraction works, researchers need to manage and refine their data locally before committing to the external database. This enables quality control and data curation.

**Independent Test**: Can be tested by creating, viewing, editing, and deleting mock records without requiring PDF extraction or cloud sync functionality.

**Acceptance Scenarios**:

1. **Given** local records exist, **When** researcher opens the records view, **Then** system displays all previously extracted records with key identifying information
2. **Given** a record is selected, **When** researcher clicks edit, **Then** all fields become editable including the ability to add custom attributes
3. **Given** researcher modifies a record, **When** they save changes, **Then** local JSON file is updated with new data
4. **Given** researcher wants to remove incorrect data, **When** they delete a record, **Then** it is removed from local storage immediately
5. **Given** researcher wants to manually add data, **When** they create a new record, **Then** system provides an empty form accepting all standard and custom fields

---

### User Story 3 - Synchronize Records to Cloud Database (Priority: P3)

A researcher has curated a collection of local records and wants to upload them to the MongoDB database for long-term storage, backup, and multi-device access.

**Why this priority**: While important for data persistence and collaboration, cloud sync is not required for the core extraction and curation workflows. Researchers can work offline and sync later.

**Independent Test**: Can be tested with a test MongoDB instance by selecting mock records and verifying successful upload and local deletion.

**Acceptance Scenarios**:

1. **Given** MongoDB connection is configured, **When** researcher selects records for upload, **Then** system displays selected count and confirmation prompt
2. **Given** records are marked for sync, **When** upload process begins, **Then** each record is sent to MongoDB and status is reported
3. **Given** a record uploads successfully, **When** confirmation is received from MongoDB, **Then** record is deleted from local JSON file
4. **Given** upload fails for a record, **When** error occurs, **Then** record remains in local storage and error details are displayed
5. **Given** researcher has not synced recently, **When** they open the application, **Then** system reminds them to sync for data safety and performance

---

### User Story 4 - Configure Application Settings (Priority: P1)

A researcher needs to configure the AI extraction service and database connection before using the application. Configuration must persist across sessions.

**Why this priority**: Configuration is a prerequisite for all other functionality. Without proper setup, extraction and sync features cannot operate. This is part of the MVP.

**Independent Test**: Can be tested by entering configuration values, restarting the application, and verifying persistence and connection status.

**Acceptance Scenarios**:

1. **Given** first-time application launch, **When** researcher accesses settings, **Then** system provides fields for AI prompt configuration and MongoDB URI
2. **Given** configuration values are entered, **When** researcher saves settings, **Then** values persist in local configuration file
3. **Given** AI service is installed and running, **When** application starts, **Then** connection status is displayed (connected/disconnected)
4. **Given** MongoDB URI is provided, **When** researcher tests connection, **Then** system reports success or specific error message
5. **Given** AI service is not available, **When** researcher attempts PDF upload, **Then** upload is disabled with clear message about AI requirement

---

### Edge Cases

- What happens when a PDF contains no extractable text (scanned images without OCR)?
- How does the system handle extremely large PDFs (100+ pages)?
- What happens when the AI extraction returns incomplete data (missing mandatory fields)?
- How does the system behave when local storage reaches the maximum record limit?
- What happens when MongoDB connection is lost during sync operation?
- How does the system handle PDFs in languages other than Portuguese/English?
- What happens when a user tries to upload a non-PDF file?
- How does the system handle concurrent extraction requests?
- What happens when the AI service crashes during extraction?
- How does the system handle duplicate records (same article uploaded twice)?

## Requirements *(mandatory)*

### Functional Requirements

#### PDF Processing & Extraction

- **FR-001**: System MUST accept PDF file uploads through a drag-and-drop or file selection interface
- **FR-002**: System MUST discard uploaded PDF files immediately after processing (no local storage of PDFs)
- **FR-003**: System MUST display real-time processing status during extraction with informative messages
- **FR-004**: System MUST validate that AI service is connected before allowing PDF uploads
- **FR-005**: System MUST extract mandatory fields: title, authors, year, abstract
- **FR-006**: System MUST normalize title text to proper case formatting
- **FR-007**: System MUST format author names according to APA citation standard
- **FR-008**: System MUST ensure abstract is extracted or translated to Brazilian Portuguese
- **FR-009**: System SHOULD extract optional fields when available: publication source, plant species (vernacular and scientific names), plant uses, studied communities, geographic data (country, state, municipality, location, biome), methodology
- **FR-010**: System MUST handle extraction errors gracefully with user-friendly error messages

#### Data Storage & Management

- **FR-011**: System MUST store all extracted records in a local JSON file matching the defined schema structure
- **FR-012**: System MUST generate unique identifiers for each record
- **FR-013**: System MUST enforce maximum record limit for local storage
- **FR-014**: System MUST prevent new PDF uploads when local storage limit is reached
- **FR-015**: System MUST display warning messages as local storage approaches capacity limit

#### Record Management Interface

- **FR-016**: System MUST provide a complete CRUD interface (Create, Read, Update, Delete) for managing records
- **FR-017**: System MUST display all locally stored records in an organized, searchable view
- **FR-018**: System MUST allow editing of all record fields, including adding custom attributes
- **FR-019**: System MUST allow manual creation of new records without PDF upload
- **FR-020**: System MUST confirm destructive actions (delete) before execution
- **FR-021**: System MUST allow users to select multiple records for batch operations

#### Cloud Synchronization

- **FR-022**: System MUST allow users to mark individual or multiple records for MongoDB upload
- **FR-023**: System MUST upload marked records to configured MongoDB instance (Atlas or local)
- **FR-024**: System MUST delete successfully uploaded records from local JSON storage
- **FR-025**: System MUST retain records locally if upload fails and report specific error
- **FR-026**: System MUST provide sync status feedback (success, failure, in-progress) for each record
- **FR-027**: System MUST display reminders to users about syncing for data safety and performance

#### Configuration & Settings

- **FR-028**: System MUST provide configuration interface for AI prompt customization
- **FR-029**: System MUST provide configuration interface for MongoDB connection URI
- **FR-030**: System MUST persist configuration settings across application sessions
- **FR-031**: System MUST validate MongoDB connection and display status (connected/error)
- **FR-032**: System MUST validate AI service availability and display status (connected/error)
- **FR-033**: System MUST display connection status for both AI and MongoDB services on application startup

#### User Interface & Experience

- **FR-034**: System MUST provide a simple, clean, and modern interface design
- **FR-035**: System MUST include an "About" menu option displaying author information: Eduardo Dalcin, Instituto de Pesquisas Jardim Botânico do Rio de Janeiro, edalcin@jbrj.gov.br
- **FR-036**: System MUST display informational message about GPU benefits for AI processing performance
- **FR-037**: System MUST provide clear visual feedback for all long-running operations
- **FR-038**: System MUST display helpful error messages with actionable guidance

#### Installation & Distribution

- **FR-039**: System MUST provide a professional Windows installer package
- **FR-040**: Installer MUST handle all necessary dependencies and configurations
- **FR-041**: Installer SHOULD provide clear instructions about OLLAMA prerequisite

### Key Entities

- **Article Record**: Represents a scientific paper with extracted metadata
  - Unique identifier
  - Mandatory fields: title (normalized), authors (APA format array), year, abstract (Portuguese)
  - Optional fields: publication source, methodology
  - Sync status (local, synced, pending)

- **Plant Species**: Represents plant species mentioned in articles
  - Vernacular name (common name)
  - Scientific name
  - Use type (medicinal, food, construction, etc.)

- **Community**: Represents the indigenous or traditional community studied
  - Community name
  - Location description

- **Geographic Data**: Represents location information
  - Country, state, municipality, specific location
  - Biome classification

- **Configuration**: Represents application settings
  - AI prompt template
  - MongoDB connection URI
  - Maximum local storage limit

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Researchers can extract all mandatory metadata from a scientific paper PDF in under 2 minutes
- **SC-002**: System successfully extracts mandatory fields (title, authors, year, abstract) from 90% of uploaded PDFs on first attempt
- **SC-003**: Researchers can manage (view, edit, delete) 100+ local records without performance degradation
- **SC-004**: System successfully syncs selected records to MongoDB with 95% success rate when network is available
- **SC-005**: 90% of researchers can complete initial setup (AI and database configuration) within 5 minutes
- **SC-006**: System prevents data loss by blocking PDF uploads when storage limit is reached
- **SC-007**: Researchers can add custom fields to records, enabling flexibility for diverse research needs
- **SC-008**: Application provides clear feedback for all connection status (AI and MongoDB), reducing support requests by 70%
- **SC-009**: Installation process completes successfully on Windows 10+ systems without manual dependency resolution
- **SC-010**: 95% of extracted abstracts are correctly presented in Brazilian Portuguese

## Out of Scope

- Multi-user collaboration features (sharing records between researchers)
- Built-in PDF viewer or annotation tools
- Automatic PDF download from online sources
- Statistical analysis or data visualization features
- Export to formats other than JSON and MongoDB
- Mobile or web versions of the application
- Automatic translation of full articles (only abstracts)
- OCR capabilities for scanned documents
- Version control or change tracking for edited records
- Batch PDF processing (processing multiple PDFs simultaneously)
- Integration with reference managers (Zotero, Mendeley, etc.)
- Automatic plant species name validation against botanical databases

## Assumptions

1. Target users have basic computer literacy and understand file system operations
2. Users have administrative privileges to install software on their Windows machines
3. Users are willing to install and configure OLLAMA separately before using the application
4. Users have internet connectivity for MongoDB synchronization (Atlas)
5. Scientific papers are primarily in English or Portuguese
6. PDF files are text-based (not scanned images requiring OCR)
7. Users understand the importance of regular backups via MongoDB sync
8. MongoDB Atlas free tier or local MongoDB installation is sufficient for user needs
9. OLLAMA models suitable for text extraction are available and documented
10. Maximum local storage limit of 1000 records is sufficient before requiring sync

## Dependencies

- **OLLAMA**: Local AI service must be pre-installed by user for extraction functionality
- **MongoDB**: Either MongoDB Atlas (cloud) or local MongoDB instance for data persistence
- **Windows OS**: Application targets Windows 10 and above
- **Network Connection**: Required for MongoDB synchronization (not required for extraction or local management)
- **GPU (Optional)**: Recommended for improved AI processing performance

## Risks

1. **AI Extraction Accuracy**: Quality of extracted data depends on OLLAMA model performance and PDF text quality
2. **User Configuration Complexity**: Setting up OLLAMA and MongoDB may be challenging for non-technical researchers
3. **Data Loss**: If users don't sync regularly and lose their device, local data is irrecoverable
4. **Performance**: Large PDFs or low-end hardware may result in slow extraction times
5. **Language Support**: Extraction accuracy may vary significantly for non-Portuguese/English papers
6. **Dependency Management**: OLLAMA updates may break compatibility with the application
