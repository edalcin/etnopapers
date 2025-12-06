# Feature Specification: EtnoPapers Desktop Migration - Electron to C# WPF

**Feature Branch**: `main` (single-branch workflow)
**Created**: 2025-12-01 | **Updated**: 2025-12-02
**Status**: Draft
**Input**: User description: "Refactor EtnoPapers from Electron (Node.js/TypeScript) to C# WPF for better Windows desktop integration, improved performance, and native Windows experience. Core functionality (PDF extraction, local record management, MongoDB sync, configuration) remains unchanged."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Maintain Full Feature Parity (Priority: P1)

A researcher using EtnoPapers with Electron expects all existing features to work identically after migration to C# WPF. All functionality for PDF extraction, record management, cloud sync, and configuration must be preserved with identical behavior and user experience.

**Why this priority**: The application must deliver the same value as before migration. Feature parity ensures no loss of capability and justifies the refactoring effort from a user perspective.

**Independent Test**: Can be tested by using the same workflows (extract PDF, view records, edit record, delete record, sync to MongoDB, configure settings) in WPF version and verifying identical results to Electron version.

**Acceptance Scenarios**:

1. **Given** a PDF is uploaded, **When** processing occurs, **Then** PDF is converted to structured Markdown and extracted metadata (title, authors, year, abstract in Portuguese) is accurate without hallucinations
2. **Given** records exist in local storage, **When** user accesses the records management interface, **Then** all records display with identical data and formatting as Electron version
3. **Given** user edits a record, **When** they save changes, **Then** JSON file updates with identical structure and data format as Electron version
4. **Given** user selects records for MongoDB sync, **When** sync occurs, **Then** records upload identically and are deleted from local storage as in Electron version
5. **Given** user configures AI and MongoDB settings, **When** they are saved, **Then** settings persist and behave identically across application restarts as in Electron version

---

### User Story 2 - Improve Windows Native Integration (Priority: P1)

A researcher working on Windows systems expects the EtnoPapers application to feel like a native Windows application with proper Windows integration, native controls, and Windows conventions rather than a web-like interface.

**Why this priority**: Moving to WPF enables true Windows integration, improving perceived application quality and user satisfaction. Native Windows appearance and behavior are fundamental benefits of this migration.

**Independent Test**: Can be tested by verifying Windows-native visual appearance, control behavior, Windows keyboard shortcuts, system tray integration, and compliance with Windows 11 UI conventions.

**Acceptance Scenarios**:

1. **Given** the application launches, **When** user views the interface, **Then** it uses Windows-native controls (buttons, text fields, menus) consistent with Windows 11 design language
2. **Given** user interacts with the application, **When** they use common Windows shortcuts (Ctrl+S, Ctrl+Q, Alt+F4, Tab navigation), **Then** these work intuitively
3. **Given** user minimizes the application, **When** they click the taskbar icon, **Then** window restores with proper state preservation
4. **Given** application has completed operations, **When** processing finishes, **Then** system notifications (if configured) appear using Windows notification system
5. **Given** user is using the application, **When** they access File, Edit, View menus, **Then** menu organization follows Windows conventions

---

### User Story 3 - Improve Performance and Resource Efficiency (Priority: P2)

A researcher working on older hardware or with limited system resources expects the WPF version to use significantly fewer system resources (memory, CPU) compared to Electron, enabling faster startup and smoother operation especially on machines with limited specs.

**Why this priority**: Performance improvement is a major benefit of C# WPF over Electron. While functional parity is critical, performance enables better user experience on diverse hardware. This is important but secondary to feature parity.

**Independent Test**: Can be tested by measuring startup time, memory footprint, CPU usage during PDF extraction, and record management operations, comparing WPF version to Electron version.

**Acceptance Scenarios**:

1. **Given** the application launches on a Windows machine, **When** measuring startup time from EtnoPapers.exe invocation to MainWindow rendered and responsive (cold start, .NET runtime initialization included), **Then** WPF version starts in under 2 seconds measured with System.Diagnostics.Stopwatch (baseline: T070 benchmark)
2. **Given** application is idle (MainWindow loaded, no records loaded), **When** measuring memory usage after 30 seconds via Task Manager or Process.WorkingSet64, **Then** WPF version uses less than 150MB RAM (baseline: T072 benchmark)
3. **Given** user performs record management operations (sorting, filtering, searching 100+ records), **When** measuring responsiveness, **Then** all interactions complete in under 200ms
4. **Given** user uploads and processes a PDF, **When** measuring CPU usage, **Then** WPF version maintains lower CPU utilization during extraction

---

### User Story 4 - Maintain Data Compatibility (Priority: P1)

A researcher who was using the Electron version with existing local JSON data and MongoDB records expects to seamlessly transition to WPF version without any data migration or loss of existing records.

**Why this priority**: Data preservation is critical. Users must be able to migrate to WPF with zero data loss and zero migration effort. This ensures adoption is friction-free.

**Independent Test**: Can be tested by copying Electron version's local JSON file to WPF version's data directory and verifying all records display and function identically.

**Acceptance Scenarios**:

1. **Given** an Electron version has local JSON data, **When** this JSON file is used by WPF version, **Then** all records load and display without corruption or parsing errors
2. **Given** MongoDB contains records synced from Electron version, **When** WPF version connects using same MongoDB URI, **Then** records display identically and can be managed without issues
3. **Given** WPF version uses inherited JSON data from Electron, **When** user adds new records in WPF, **Then** data format matches exactly and mixed records (Electron + WPF created) coexist seamlessly
4. **Given** user moves between Electron and WPF versions, **When** they share the same local JSON file, **Then** both versions read and write identical data without conflicts

---

### Edge Cases

- What happens if JSON configuration file from Electron version has a different schema or unknown fields?
- How does WPF version handle MongoDB connections that were configured in Electron version with potentially different driver versions?
- What happens if WPF and Electron versions run simultaneously on the same machine (shared JSON file)?
- How does the application handle incomplete migrations (some files in Electron format, some in WPF)?

## Requirements *(mandatory)*

### Functional Requirements

#### Feature Parity Requirements

- **FR-001**: System MUST support PDF file uploads identically to Electron version
- **FR-002**: System MUST extract metadata (title, authors, year, abstract) with identical output format as Electron version
- **FR-003**: System MUST store extracted records in identical JSON schema as Electron version
- **FR-004**: System MUST provide complete CRUD interface (Create, Read, Update, Delete) for records matching Electron version behavior
- **FR-005**: System MUST support MongoDB synchronization with identical protocol and behavior as Electron version
- **FR-006**: System MUST persist configuration settings (AI prompt, MongoDB URI) in identical format as Electron version
- **FR-007**: System MUST validate AI and MongoDB connections with identical error messages as Electron version
- **FR-008**: System MUST handle all edge cases (scanned PDFs, duplicate detection, large records, connection failures) identically to Electron version
- **FR-009**: System MUST maintain maximum local record limit enforcement with identical warnings as Electron version
- **FR-010**: System MUST display abstract data in Brazilian Portuguese with identical handling as Electron version

#### PDF Processing and Extraction Accuracy Requirements

- **FR-033**: System MUST convert PDF documents to structured Markdown format before AI extraction to preserve document hierarchy and reduce metadata hallucinations
- **FR-034**: System MUST detect and preserve document structure including headings, sections, tables, and lists during PDF to Markdown conversion
- **FR-035**: System MUST extract metadata from structured Markdown with higher accuracy than raw text extraction
- **FR-036**: System MUST handle scientific papers with complex layouts (multi-column, tables, equations) by converting to clean Markdown representation
- **FR-037**: System MUST provide fallback to raw text extraction if Markdown conversion fails

**Implementation Note**: FR-033 through FR-037 introduce structured Markdown conversion as the solution to reduce AI hallucinations. This architectural layer (using PdfPig library) is a core requirement of this migration, not an optional enhancement. Success of the WPF version depends on accurate Markdown-based extraction matching Electron version output quality.

#### Windows Native Integration Requirements

- **FR-011**: System MUST use Windows-native UI controls throughout the application
- **FR-012**: System MUST respect Windows system colors and themes (light/dark mode support)
- **FR-013**: System MUST support standard Windows keyboard shortcuts (Ctrl+S, Ctrl+Q, Alt+F4, Tab navigation)
- **FR-014**: System MUST implement proper window state preservation (size, position, maximized state) on exit/restart
- **FR-015**: System MUST use Windows file dialogs for file selection (no custom file pickers)
- **FR-016**: System MUST provide menu structure following Windows conventions (File, Edit, View, Help)
- **FR-017**: System MUST display Windows notification alerts for important events when appropriate
- **FR-018**: System MUST support drag-and-drop of PDF files from Windows Explorer

#### Performance Requirements

- **FR-019**: System MUST start in under 2 seconds on modern Windows systems
- **FR-020**: System MUST maintain memory footprint under 150MB during idle state
- **FR-021**: System MUST handle 1000+ local records without performance degradation in record management UI
- **FR-022**: System MUST complete record management operations (sort, filter, search, pagination) in under 200ms
- **FR-023**: System MUST process PDF extraction with CPU efficiency equal to or better than Electron version

#### Data Compatibility Requirements

- **FR-024**: System MUST read and parse JSON configuration files created by Electron version without modification
- **FR-025**: System MUST read and display JSON record files created by Electron version with 100% compatibility
- **FR-026**: System MUST write records in identical JSON format, maintaining compatibility with Electron version
- **FR-027**: System MUST connect to MongoDB instances configured in Electron version using identical connection strings
- **FR-028**: System MUST handle MongoDB document formats created by Electron version without transformation

#### Installation & Distribution Requirements

- **FR-029**: System MUST provide Windows installer (.msi or .exe) for professional deployment
- **FR-030**: Installer MUST handle installation of .NET 8 runtime dependency if not present
- **FR-031**: Installer MUST preserve existing configuration and JSON data during upgrades from Electron version
- **FR-032**: Installer MUST provide uninstall capability that preserves user data

### Key Entities

- **Application State**: Migrated unchanged from Electron
- **Local JSON Storage**: Identical schema and format to Electron version
- **MongoDB Record Format**: Unchanged from Electron version
- **Configuration Schema**: Identical to Electron version (AI prompt, MongoDB URI)
- **User Interface Components**: Functional equivalents using WPF native controls

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: WPF version successfully completes all test cases that passed in Electron version (100% functional parity)
- **SC-002**: WPF version startup time is under 2 seconds (vs. Electron ~5-10 seconds)
- **SC-003**: WPF version idle memory usage is under 150MB (vs. Electron ~300-500MB)
- **SC-004**: Users can migrate from Electron to WPF by simply copying configuration and JSON data files
- **SC-005**: WPF version handles 1000+ local records with record management operations completing in under 200ms
- **SC-006**: 100% of existing JSON data from Electron version displays without corruption in WPF version
- **SC-007**: WPF version looks and feels like a native Windows 10/11 application (user perception test)
- **SC-008**: Application can be installed and run on Windows 10 and later without external dependencies beyond .NET 8 runtime
- **SC-009**: PDF extraction output is identical between Electron and WPF versions when processing same file
- **SC-010**: MongoDB synchronization success rate is maintained at 95% or higher in WPF version

## Out of Scope

- New feature additions during migration (strictly refactoring only)
- UI redesign or significant UX changes (preserve existing user interface layout and flow)
- Changes to core extraction logic or AI prompting (maintain identical behavior)
- Database schema changes (preserve MongoDB document format)
- Support for non-Windows platforms (Windows-only application)
- Simultaneous support for both Electron and WPF versions
- Cross-version data synchronization (users must fully migrate, not run both versions)

## Assumptions

1. Users will migrate to WPF version after it reaches feature parity
2. .NET 8 runtime is acceptable as application dependency
3. Target deployment is Windows 10 and later
4. Visual Studio or build tools are available for compilation
5. Existing JSON data and configuration files are valid and corruption-free
6. MongoDB connection strings remain unchanged during migration
7. OLLAMA integration remains identical to Electron version (no changes to AI extraction)
8. File system permissions are consistent between Electron and WPF versions
9. Windows Defender and antivirus software will recognize WPF executable as legitimate
10. User data directory location can be consistent between versions

## Dependencies

- **.NET 8 Runtime**: Required for WPF application execution
- **Windows 10+**: Target operating system (WPF is Windows-only)
- **OLLAMA**: Unchanged dependency for PDF extraction (pre-installed by user)
- **MongoDB**: Unchanged dependency for cloud sync (Atlas or local)
- **Visual Studio 2022 or Build Tools**: Required for compilation
- **WPF Framework**: Part of .NET 8

## Risks

1. **Testing Coverage**: Ensuring complete feature parity requires comprehensive testing across all features and edge cases
2. **User Migration**: Users familiar with Electron might need guidance to migrate to new version
3. **Third-party Library Availability**: Some Node.js packages may not have direct C# equivalents
4. **PDF Processing Library**: Quality of C# PDF libraries (Spire.Pdf, iTextSharp, etc.) may differ from Node.js libraries
5. **MongoDB Driver Compatibility**: C# MongoDB driver version may need careful selection for compatibility
6. **Build Complexity**: .NET/WPF build process is different from Node.js/TypeScript build
7. **Installer Creation**: Creating professional Windows installer requires additional tooling (WiX, NSIS, etc.)
8. **Performance Regression**: Despite C# advantages, specific operations might perform worse than Electron if not optimized
9. **External API Changes**: OLLAMA and MongoDB API changes could affect both versions identically or differently

## Clarifications

None at this stage. Migration scope is clear: refactor implementation language while maintaining 100% functional equivalence.
