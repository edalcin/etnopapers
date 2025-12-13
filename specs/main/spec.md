# Feature Specification: Cloud-Based AI Provider Migration

**Feature Branch**: `main` (single-branch workflow)
**Created**: 2025-12-09
**Status**: Draft
**Input**: User description: "Quero refatorar este projeto para usar agentes de intelig�ncia artificial baseados na WEB, via API-KEY. O desempenho com OLLAMA local foi p�ssimo. Fa�a refer�ncia ao OLLAMA apenas como hist�rico de vers�o do projeto. Nesta nova vers�o vamos manter todo fluxo de trabalho, interface etc e vamos mudar apenas a entrega do texto do artigo para o agente de AI externo, n�o mais o OLLAMA. A interface de configura��o agora ir� pedir a chave de API para os agentes 'Gemini', 'OpenAI' e 'Anthropic'. O usu�rio vai escolher o agente, via pulldown, e preencher o valor da chave. Apenas um agente ser� permitido escolher e salvar a chave correspondente no arquivo de configura��o local, que nunca ser� commitado para o reposit�rio p�blico, pois tem informa��o sens�vel. Em resumo, vamos apenas substituir o OLLAMA pelo 'Gemini', 'OpenAI' ou 'Anthropic'. A interface de configura��o e todo o processo de entrega do artigo e recebimento dos dados deve ser ajustado para este novo cen�rio, assim como a interface de configura��o, para receber a chave API do agente escolhido. Atualize o planejamento e as tasks com base nesta nova especifica��o. Atualize tamb�m toda documenta��o, eliminando todos os arquivos desnecess�rios, testes e relacionados ao OLLAMA."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Configure Cloud AI Provider (Priority: P1)

A researcher wants to configure the application to use their preferred cloud AI provider for metadata extraction. They need to select a provider (Gemini, OpenAI, or Anthropic), enter their API key, and save the configuration securely on their local machine.

**Why this priority**: This is the foundational configuration that enables all other functionality. Without a properly configured AI provider, no PDF processing can occur. This represents the minimum viable change to move from OLLAMA to cloud-based AI.

**Independent Test**: Can be fully tested by opening the configuration interface, selecting each provider from the dropdown, entering a test API key, saving the configuration, and verifying the key is stored locally in a secure configuration file that is excluded from version control.

**Acceptance Scenarios**:

1. **Given** the application is installed and no AI provider is configured, **When** the user opens the configuration area, **Then** they see a dropdown list with three options (Gemini, OpenAI, Anthropic) and an empty API key input field
2. **Given** the user has selected "Gemini" from the provider dropdown, **When** they enter their API key and click Save, **Then** the configuration is saved to a local file and a success message is displayed
3. **Given** the user has previously configured OpenAI, **When** they select "Anthropic" from the dropdown, **Then** the API key field is cleared and they can enter a new Anthropic API key
4. **Given** the user tries to save configuration, **When** the API key field is empty, **Then** a validation error message is displayed preventing save
5. **Given** the configuration file exists with an API key, **When** the user reopens the configuration interface, **Then** the previously selected provider is shown in the dropdown and the API key field shows masked characters (for security)

---

### User Story 2 - Process PDF with Cloud AI Provider (Priority: P2)

A researcher wants to upload a scientific PDF and have its ethnobotanical metadata extracted using the configured cloud AI provider. The extraction process should work identically to the previous OLLAMA-based flow, but use the cloud API instead.

**Why this priority**: This is the core value delivery of the application. Once configuration is complete (P1), users need to process PDFs to extract metadata. This validates that the cloud AI migration actually works end-to-end.

**Independent Test**: Can be fully tested by uploading a sample ethnobotanical PDF after configuring a valid API key in P1, initiating the extraction process, and verifying that metadata is extracted and stored in the local JSON file with the same structure as before.

**Acceptance Scenarios**:

1. **Given** a valid AI provider is configured with an active API key, **When** the user uploads a PDF file and initiates extraction, **Then** the PDF text is sent to the cloud AI provider's API for processing
2. **Given** the cloud AI provider returns extracted metadata, **When** the processing completes, **Then** the data is saved to the local JSON file with all mandatory fields (t�tulo, autores, ano, resumo, esp�cies, geographic data, comunidade, metodologia)
3. **Given** the user is processing a PDF, **When** the extraction is in progress, **Then** a progress indicator shows the current status (e.g., "Extracting metadata from cloud AI...")
4. **Given** the cloud AI API returns an error (invalid key, rate limit, network issue), **When** the error occurs, **Then** the user sees a clear error message explaining the issue and the extraction stops gracefully
5. **Given** multiple PDFs are queued for processing, **When** one extraction fails, **Then** the remaining PDFs continue to process independently

---

### User Story 3 - Migrate Existing OLLAMA References (Priority: P3)

A developer or documentation reader wants to understand that OLLAMA was used in a previous version but is now deprecated. All documentation, code comments, and configuration files should reflect that cloud AI providers are the current standard.

**Why this priority**: This is housekeeping work that ensures the codebase is clean and maintainable. It doesn't directly deliver user value but prevents confusion for future developers and users reading documentation.

**Independent Test**: Can be fully tested by searching the entire codebase for "OLLAMA" references, verifying that production code has no OLLAMA functionality, documentation mentions it only as historical context, and all OLLAMA-specific test files are removed.

**Acceptance Scenarios**:

1. **Given** the codebase contains OLLAMA integration code, **When** the migration is complete, **Then** all OLLAMA-specific code files are removed or refactored to use cloud AI providers
2. **Given** README and documentation mention OLLAMA setup instructions, **When** documentation is updated, **Then** OLLAMA is mentioned only in a "Previous Versions" or "Migration History" section, and current setup instructions reference cloud AI providers
3. **Given** configuration files have OLLAMA-related settings, **When** cleanup is complete, **Then** OLLAMA settings are removed and replaced with cloud provider configuration schema
4. **Given** test files exist for OLLAMA integration, **When** cleanup is complete, **Then** OLLAMA-specific test files are deleted and new tests validate cloud AI provider integration

---

### Edge Cases

- What happens when the user's API key expires or becomes invalid during a PDF processing session?
- How does the system handle rate limiting from the cloud AI provider (e.g., max requests per minute)?
- What happens when the user switches AI providers mid-session while there are queued PDFs to process?
- How does the system handle network connectivity loss during API calls?
- What happens when the cloud AI provider returns incomplete or malformed metadata?
- How does the system handle PDFs that exceed the cloud provider's token/size limits?
- What happens if the configuration file is corrupted or manually edited incorrectly?
- How does the system behave if the user tries to process a PDF before configuring any AI provider?

## Requirements *(mandatory)*

### Functional Requirements

#### Configuration Management

- **FR-001**: System MUST provide a configuration interface with a dropdown selector containing exactly three options: "Gemini", "OpenAI", and "Anthropic"
- **FR-002**: System MUST allow the user to enter an API key as text input associated with the selected AI provider
- **FR-003**: System MUST enforce that only one AI provider can be configured and active at any given time
- **FR-004**: System MUST save the selected provider and API key to a local configuration file on the user's machine
- **FR-005**: System MUST exclude the configuration file containing API keys from version control (e.g., via .gitignore)
- **FR-006**: System MUST validate that an API key is provided before allowing configuration to be saved
- **FR-007**: System MUST display the selected provider and mask the API key (e.g., show only last 4 characters or asterisks) when loading an existing configuration
- **FR-008**: System MUST clear the API key input field when the user changes the selected provider in the dropdown

#### PDF Processing with Cloud AI

- **FR-009**: System MUST send extracted PDF text to the configured cloud AI provider's API endpoint using the saved API key
- **FR-010**: System MUST include the ethnobotanical extraction prompt in the API request (same prompt structure previously used with OLLAMA)
- **FR-011**: System MUST parse the cloud AI provider's response and extract metadata into the same JSON structure defined in `docs/estrutura.json`
- **FR-012**: System MUST maintain all mandatory extracted fields: t�tulo, autores, ano, resumo (in Brazilian Portuguese), esp�cies, pais, estado, municipio, local, bioma, comunidade, metodologia
- **FR-013**: System MUST display a progress indicator during cloud API processing with appropriate status messages
- **FR-014**: System MUST handle API errors gracefully and display user-friendly error messages for common issues (invalid key, rate limits, network failures, timeout)
- **FR-015**: System MUST allow processing to continue for remaining PDFs if one extraction fails in a batch
- **FR-016**: System MUST maintain the same CRUD interface and MongoDB synchronization functionality that existed with OLLAMA

#### OLLAMA Deprecation and Cleanup

- **FR-017**: System MUST remove all OLLAMA integration code from the production codebase
- **FR-018**: System MUST update all user-facing documentation (README.md in Brazilian Portuguese) to reference cloud AI providers as the current configuration method
- **FR-019**: System MUST mention OLLAMA only in historical/migration context in documentation (e.g., "Previous versions used OLLAMA...")
- **FR-020**: System MUST remove OLLAMA-specific configuration settings from configuration files
- **FR-021**: System MUST remove OLLAMA-specific test files and replace with cloud AI provider integration tests
- **FR-022**: System MUST update any setup/installation guides to remove OLLAMA prerequisites and replace with cloud API key instructions

### Non-Functional Requirements

- **NFR-001**: API keys MUST be stored securely in a local file that is never committed to the repository
- **NFR-002**: API communication MUST use HTTPS for all cloud provider endpoints
- **NFR-003**: System SHOULD implement retry logic with exponential backoff for transient API failures
- **NFR-004**: Error messages MUST be clear, actionable for non-technical users, and displayed in Brazilian Portuguese
- **NFR-005**: The configuration interface MUST use the same UI framework and design patterns as the existing application

### Key Entities

- **AI Provider Configuration**: Represents the user's cloud AI setup with three attributes: provider name (enum: Gemini, OpenAI, Anthropic), API key (encrypted string), and timestamp of last update. Relationship: One active configuration per application instance.

- **Extracted Metadata Record**: Represents ethnobotanical data extracted from a PDF. Attributes remain unchanged from existing structure (t�tulo, autores, ano, resumo, esp�cies, geographic fields, comunidade, metodologia). Relationship: Created by AI Provider via API call, stored in local JSON, optionally synced to MongoDB.

- **Processing Queue**: Represents PDFs awaiting or undergoing extraction. Attributes: PDF file reference, processing status (pending, in-progress, completed, failed), associated AI provider used, timestamp, error message (if failed). Relationship: Each queue item processes one PDF and creates one Extracted Metadata Record.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can successfully configure any of the three cloud AI providers (Gemini, OpenAI, Anthropic) in under 2 minutes
- **SC-002**: PDF metadata extraction completes with the same accuracy and field completeness as the OLLAMA-based version (100% of mandatory fields populated)
- **SC-003**: API key is never exposed in application logs, version control, or unencrypted storage
- **SC-004**: Users receive clear, actionable error messages for API failures within 5 seconds of error occurrence
- **SC-005**: Extraction processing time is at least 50% faster than the previous OLLAMA local implementation (based on user's reported poor OLLAMA performance)
- **SC-006**: Zero references to OLLAMA exist in production code, and documentation mentions it only in historical context
- **SC-007**: 100% of existing CRUD and MongoDB sync functionality continues to work identically with cloud-based AI extraction
- **SC-008**: Application successfully processes at least 10 consecutive PDFs without degradation or rate limit issues (with valid API keys and standard rate limits)

## Assumptions

1. **API Key Acquisition**: Users are responsible for obtaining their own API keys from Gemini, OpenAI, or Anthropic. The application does not provide key generation or signup flows.

2. **API Response Format**: All three cloud providers return text responses that can be parsed into the existing JSON structure. If structured output (JSON mode) is available, it will be used; otherwise, text parsing logic will extract metadata.

3. **Rate Limits**: Users have API keys with sufficient rate limits for their typical workload. The application will implement basic retry logic but will not manage sophisticated rate limit pooling or queueing across hours/days.

4. **Network Connectivity**: Users have stable internet connection. The application will handle transient failures with retries but is not designed for offline operation.

5. **Prompt Compatibility**: The existing ethnobotanical extraction prompt used with OLLAMA is compatible with cloud AI providers with minimal or no modification.

6. **Cost Awareness**: Users understand that cloud AI providers charge per API call/token, unlike the free local OLLAMA. The application will not track or display cost estimates.

7. **Windows Platform**: Application remains Windows-only as specified in project overview.

8. **Configuration File Location**: Local configuration file will be stored in a standard user application data directory (e.g., AppData/Local or AppData/Roaming on Windows).

9. **Existing Data Compatibility**: All existing JSON files with metadata extracted via OLLAMA remain valid and compatible with the new version.

10. **No Multi-Provider Support**: The requirement explicitly states "only one agent will be allowed" - users cannot configure multiple providers simultaneously or switch between them without reconfiguring.

## Dependencies

### External Dependencies

- **Cloud AI Provider APIs**: Gemini API (Google), OpenAI API (OpenAI Platform), Anthropic API (Claude)
- **Internet Connectivity**: Required for all PDF processing operations
- **User API Keys**: Users must have valid, active API keys from their chosen provider

### Internal Dependencies

- **Existing Configuration System**: Modifications to add provider dropdown and API key input
- **Existing PDF Processing Pipeline**: Extraction logic must be refactored to call cloud APIs instead of local OLLAMA
- **Existing JSON Storage**: Data structure remains unchanged (defined in `docs/estrutura.json`)
- **Existing CRUD Interface**: No changes required, continues to operate on local JSON files
- **Existing MongoDB Sync**: No changes required, continues to sync from local JSON to MongoDB

### Documentation Dependencies

- **README.md**: Must be updated with new cloud AI provider setup instructions (in Brazilian Portuguese)
- **docs/estrutura.json**: No changes required (data structure unchanged)
- **Installation Guide**: Must remove OLLAMA setup steps and add API key acquisition guidance

## Security and Privacy Considerations

1. **API Key Storage**: Configuration file containing API keys must be excluded from version control via .gitignore. File should be stored with appropriate OS-level permissions (user-only read/write).

2. **Key Display**: When configuration is loaded, API keys should be masked in the UI (e.g., show only """""""""" or last 4 characters).

3. **Transmission Security**: All API communications must use HTTPS/TLS encryption.

4. **Error Logging**: API keys must never appear in log files, error messages, or debugging output.

5. **Data Privacy**: PDF content and extracted metadata are transmitted to third-party cloud AI providers. Users should be aware of this in documentation, especially for sensitive research data.

6. **Key Rotation**: Users should be able to update/change API keys at any time without data loss.

## Out of Scope

The following are explicitly NOT included in this feature:

1. **Multiple Provider Support**: Users cannot configure multiple AI providers simultaneously or switch between them without overwriting configuration.

2. **Cost Tracking**: Application will not track API usage costs or display estimated/actual charges.

3. **API Key Validation**: Application will not pre-validate API keys against provider APIs before saving (validation happens on first use).

4. **Offline Mode**: Application requires internet connectivity for PDF processing. No offline fallback is provided.

5. **Provider-Specific Features**: Advanced features unique to specific providers (e.g., OpenAI's function calling, Anthropic's prompt caching) are not exposed in the configuration.

6. **Custom Prompts per Provider**: The same extraction prompt is used for all three providers. No provider-specific prompt customization is available in the UI.

7. **OLLAMA Migration Tool**: No automated migration tool is provided. Users simply reconfigure to a cloud provider and continue working.

8. **Historical OLLAMA Data**: No changes to metadata previously extracted with OLLAMA. All existing JSON records remain valid.
