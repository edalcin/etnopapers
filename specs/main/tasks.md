# Tasks: Cloud-Based AI Provider Migration

**Input**: Design documents from `/specs/main/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/service-interfaces.ts

**Tests**: Integration tests for cloud AI providers are MANDATORY per Constitution Principle VI (Quality Assurance). Test tasks (T073-T075) are included in Phase 4 (User Story 2) to validate cloud API integration before deployment.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `- [ ] [ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

This is a C# WPF desktop application with the following structure:
- **Core library**: `src/EtnoPapers.Core/` (Models, Services)
- **UI layer**: `src/EtnoPapers.UI/` (Views, ViewModels)
- **Tests**: `tests/` (xUnit test files)

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and dependency setup for cloud AI providers

- [ ] T001 Add Google.Cloud.AIPlatform.V1 NuGet package to src/EtnoPapers.Core/EtnoPapers.Core.csproj
- [ ] T002 [P] Add OpenAI NuGet package to src/EtnoPapers.Core/EtnoPapers.Core.csproj
- [ ] T003 [P] Add Anthropic.SDK NuGet package to src/EtnoPapers.Core/EtnoPapers.Core.csproj
- [ ] T004 Update .gitignore to exclude config.json from AppData/Local/EtnoPapers/
- [ ] T005 [P] Create AIProviderType enum in src/EtnoPapers.Core/Models/AIProviderType.cs

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [ ] T006 Create abstract AIProviderService base class in src/EtnoPapers.Core/Services/AIProviderService.cs
- [ ] T007 Define IAIProvider interface in src/EtnoPapers.Core/Services/IAIProvider.cs
- [ ] T008 [P] Implement DPAPI encryption helper in src/EtnoPapers.Core/Utils/EncryptionHelper.cs
- [ ] T009 [P] Create cloud API error mapping utility in src/EtnoPapers.Core/Utils/CloudErrorMapper.cs
- [ ] T010 Implement exponential backoff retry logic in src/EtnoPapers.Core/Utils/RetryHelper.cs

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Configure Cloud AI Provider (Priority: P1) 🎯 MVP

**Goal**: Enable users to select a cloud AI provider (Gemini, OpenAI, or Anthropic), enter their API key, and save the configuration securely on their local machine.

**Independent Test**: Open the configuration interface, select each provider from the dropdown, enter a test API key, save the configuration, and verify the key is stored locally in a secure configuration file that is excluded from version control.

### Implementation for User Story 1

- [ ] T011 [P] [US1] Refactor Configuration model to add AIProvider property (enum) in src/EtnoPapers.Core/Models/Configuration.cs
- [ ] T012 [P] [US1] Add ApiKey property (string) to Configuration model in src/EtnoPapers.Core/Models/Configuration.cs
- [ ] T013 [US1] Remove OllamaUrl, OllamaModel, OllamaTimeout from Configuration model in src/EtnoPapers.Core/Models/Configuration.cs
- [ ] T014 [US1] Update ConfigurationService.Save to encrypt API key using DPAPI in src/EtnoPapers.Core/Services/ConfigurationService.cs
- [ ] T015 [US1] Update ConfigurationService.Load to decrypt API key using DPAPI in src/EtnoPapers.Core/Services/ConfigurationService.cs
- [ ] T016 [US1] Add validation for API key (non-empty) in ConfigurationService.Save in src/EtnoPapers.Core/Services/ConfigurationService.cs
- [ ] T017 [US1] Update SettingsPage.xaml to add ComboBox for provider selection (Gemini, OpenAI, Anthropic) in src/EtnoPapers.UI/Views/SettingsPage.xaml
- [ ] T018 [US1] Update SettingsPage.xaml to add PasswordBox for API key input in src/EtnoPapers.UI/Views/SettingsPage.xaml
- [ ] T019 [US1] Remove OLLAMA configuration fields from SettingsPage.xaml in src/EtnoPapers.UI/Views/SettingsPage.xaml
- [ ] T020 [US1] Add SelectedProvider property (AIProviderType) to SettingsViewModel in src/EtnoPapers.UI/ViewModels/SettingsViewModel.cs
- [ ] T021 [US1] Add ApiKey property (string) to SettingsViewModel in src/EtnoPapers.UI/ViewModels/SettingsViewModel.cs
- [ ] T022 [US1] Implement provider dropdown selection logic in SettingsViewModel in src/EtnoPapers.UI/ViewModels/SettingsViewModel.cs
- [ ] T023 [US1] Clear API key field when provider changes in SettingsViewModel in src/EtnoPapers.UI/ViewModels/SettingsViewModel.cs
- [ ] T024 [US1] Display masked API key (last 4 characters) when loading existing configuration in SettingsViewModel in src/EtnoPapers.UI/ViewModels/SettingsViewModel.cs
- [ ] T025 [US1] Add validation error message display for empty API key in SettingsViewModel in src/EtnoPapers.UI/ViewModels/SettingsViewModel.cs
- [ ] T026 [US1] Add success message display on save in SettingsViewModel in src/EtnoPapers.UI/ViewModels/SettingsViewModel.cs

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently. Users can configure cloud AI providers with encrypted API keys.

---

## Phase 4: User Story 2 - Process PDF with Cloud AI Provider (Priority: P2)

**Goal**: Enable users to upload a scientific PDF and have its ethnobotanical metadata extracted using the configured cloud AI provider. The extraction process should work identically to the previous OLLAMA-based flow, but use the cloud API instead.

**Independent Test**: Upload a sample ethnobotanical PDF after configuring a valid API key in P1, initiate the extraction process, and verify that metadata is extracted and stored in the local JSON file with the same structure as before.

### Implementation for User Story 2

- [ ] T027 [P] [US2] Implement GeminiService inheriting from AIProviderService in src/EtnoPapers.Core/Services/GeminiService.cs
- [ ] T028 [P] [US2] Implement OpenAIService inheriting from AIProviderService in src/EtnoPapers.Core/Services/OpenAIService.cs
- [ ] T029 [P] [US2] Implement AnthropicService inheriting from AIProviderService in src/EtnoPapers.Core/Services/AnthropicService.cs
- [ ] T030 [US2] Implement ExtractMetadata method for Gemini API in src/EtnoPapers.Core/Services/GeminiService.cs
- [ ] T031 [US2] Implement ExtractMetadata method for OpenAI API in src/EtnoPapers.Core/Services/OpenAIService.cs
- [ ] T032 [US2] Implement ExtractMetadata method for Anthropic API in src/EtnoPapers.Core/Services/AnthropicService.cs
- [ ] T033 [US2] Reuse existing OLLAMA prompt format for Gemini requests in src/EtnoPapers.Core/Services/GeminiService.cs
- [ ] T034 [US2] Reuse existing OLLAMA prompt format for OpenAI requests with system+user messages in src/EtnoPapers.Core/Services/OpenAIService.cs
- [ ] T035 [US2] Reuse existing OLLAMA prompt format for Anthropic requests in src/EtnoPapers.Core/Services/AnthropicService.cs
- [ ] T036 [US2] Set temperature=0.1, top_p=0.3, max_tokens=8000 for all providers in their respective service files
- [ ] T037 [US2] Implement error handling for 401 (invalid key) in src/EtnoPapers.Core/Services/GeminiService.cs
- [ ] T038 [US2] Implement error handling for 429 (rate limit) with exponential backoff in src/EtnoPapers.Core/Services/GeminiService.cs
- [ ] T039 [US2] Implement error handling for 500 (service unavailable) in src/EtnoPapers.Core/Services/GeminiService.cs
- [ ] T040 [US2] Implement error handling for 401, 429, 500 in src/EtnoPapers.Core/Services/OpenAIService.cs
- [ ] T041 [US2] Implement error handling for 401, 429, 500 in src/EtnoPapers.Core/Services/AnthropicService.cs
- [ ] T042 [US2] Map cloud API errors to Portuguese user-friendly messages in src/EtnoPapers.Core/Utils/CloudErrorMapper.cs
- [ ] T043 [US2] Create AIProviderFactory to instantiate provider based on configuration in src/EtnoPapers.Core/Services/AIProviderFactory.cs
- [ ] T044 [US2] Update ExtractionPipelineService to use AIProviderFactory instead of OLLAMAService in src/EtnoPapers.Core/Services/ExtractionPipelineService.cs
- [ ] T045 [US2] Remove OLLAMAService dependency from ExtractionPipelineService in src/EtnoPapers.Core/Services/ExtractionPipelineService.cs
- [ ] T046 [US2] Update progress messages to say "Extracting metadata from cloud AI" in src/EtnoPapers.Core/Services/ExtractionPipelineService.cs
- [ ] T047 [US2] Update UploadViewModel to display cloud-specific error messages in src/EtnoPapers.UI/ViewModels/UploadViewModel.cs
- [ ] T048 [US2] Add error message for missing API key configuration in src/EtnoPapers.UI/ViewModels/UploadViewModel.cs
- [ ] T049 [US2] Update UploadPage.xaml error display for network/API failures in src/EtnoPapers.UI/Views/UploadPage.xaml
- [ ] T050 [US2] Update DI registration in App.xaml.cs to register AIProviderFactory in src/EtnoPapers.UI/App.xaml.cs
- [ ] T051 [US2] Ensure JSON extraction format matches existing estrutura.json schema for all providers

### Testing for User Story 2 (MANDATORY per Constitution VI)

- [ ] T073 [P] [US2] Create GeminiServiceTests.cs in tests/
- [ ] T074 [P] [US2] Create OpenAIServiceTests.cs in tests/
- [ ] T075 [P] [US2] Create AnthropicServiceTests.cs in tests/

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently. Users can configure a provider and process PDFs with cloud AI extraction. Integration tests validate all three cloud providers.

---

## Phase 5: User Story 3 - Migrate Existing OLLAMA References (Priority: P3)

**Goal**: Clean up the codebase to reflect that OLLAMA was used in a previous version but is now deprecated. All documentation, code comments, and configuration files should reflect that cloud AI providers are the current standard.

**Independent Test**: Search the entire codebase for "OLLAMA" references, verify that production code has no OLLAMA functionality, documentation mentions it only as historical context, and all OLLAMA-specific test files are removed.

### Implementation for User Story 3

- [ ] T052 [P] [US3] Delete OLLAMAService.cs from src/EtnoPapers.Core/Services/
- [ ] T053 [P] [US3] Delete OLLAMAServiceTests.cs from tests/
- [ ] T054 [P] [US3] Remove OLLAMA-related imports and dependencies from src/EtnoPapers.Core/Services/ExtractionPipelineService.cs
- [ ] T055 [US3] Update README.md to replace OLLAMA setup instructions with cloud API key acquisition guide (in Brazilian Portuguese)
- [ ] T056 [US3] Add "Previous Versions" section to README.md mentioning OLLAMA as legacy approach (in Brazilian Portuguese)
- [ ] T057 [US3] Update README.md to add API key acquisition links for Gemini, OpenAI, Anthropic (in Brazilian Portuguese)
- [ ] T058 [US3] Update installation guide documentation to remove OLLAMA prerequisites
- [ ] T059 [US3] Update data-model.md to replace OLLAMA references in ExtractionMetadata with generic "AI provider" in specs/main/data-model.md
- [ ] T060 [US3] Update quickstart.md to replace OLLAMA setup with cloud provider configuration in specs/main/quickstart.md
- [ ] T061 [US3] Update service-interfaces.ts to rename IOLLAMAService to IAIProviderService in specs/main/contracts/service-interfaces.ts
- [ ] T062 [US3] Remove OLLAMAConfig interface from service-interfaces.ts in specs/main/contracts/service-interfaces.ts
- [ ] T063 [US3] Add migration detection logic to display banner if legacy config has OllamaUrl in src/EtnoPapers.UI/ViewModels/SettingsViewModel.cs
- [ ] T064 [US3] Display non-intrusive migration banner in settings UI when OLLAMA config detected in src/EtnoPapers.UI/Views/SettingsPage.xaml
- [ ] T065 [US3] Search codebase for remaining "OLLAMA" string references and replace with "Cloud AI" or "AI Provider"
- [ ] T066 [US3] Remove OLLAMA-related code comments from all source files

**Checkpoint**: All user stories should now be independently functional. Codebase is clean of OLLAMA references except in historical context.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories and final validation

- [ ] T067 [P] Add API key acquisition URLs to configuration UI tooltips (Gemini, OpenAI, Anthropic) in src/EtnoPapers.UI/Views/SettingsPage.xaml
- [ ] T068 [P] Add network connectivity check before cloud API calls in src/EtnoPapers.Core/Utils/NetworkHelper.cs
- [ ] T069 Add timeout configuration (default 30s) for cloud API requests across all provider services
- [ ] T070 [P] Update Portuguese error messages for all cloud API scenarios in src/EtnoPapers.Core/Utils/CloudErrorMapper.cs
- [ ] T071 Verify resumo field is always in Brazilian Portuguese across all provider responses (manual check during T077)
- [ ] T072 Add logging for cloud API calls (without exposing API keys) in all provider services
- [ ] T076 Update project documentation in docs/ to reflect cloud AI architecture
- [ ] T077 Run manual integration test with real API keys for all three providers (validate SC-008: 10 consecutive PDFs, edge cases from spec.md lines 64-71, Portuguese resumo field)
- [ ] T078 Verify configuration file encryption and .gitignore exclusion
- [ ] T079 Performance benchmark cloud API vs OLLAMA to confirm 50%+ improvement (SC-005)
- [ ] T080 Code cleanup: remove unused imports and dead code from refactoring

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3+)**: All depend on Foundational phase completion
  - User stories can then proceed in parallel (if staffed)
  - Or sequentially in priority order (P1 → P2 → P3)
- **Polish (Phase 6)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 2 (P2)**: Depends on User Story 1 completion (requires provider configuration to be available)
- **User Story 3 (P3)**: Can start after Foundational (Phase 2) - Independent of US1/US2 (cleanup work)

### Within Each User Story

**User Story 1**:
1. Refactor Configuration model (T011-T013)
2. Update ConfigurationService (T014-T016)
3. Update UI (T017-T019)
4. Update ViewModel (T020-T026)

**User Story 2**:
1. Implement all three provider services in parallel (T027-T029)
2. Add ExtractMetadata methods (T030-T032)
3. Configure prompts and parameters (T033-T036)
4. Add error handling to all providers (T037-T042)
5. Integrate with pipeline (T043-T046)
6. Update UI for cloud errors (T047-T049)
7. Final DI and validation (T050-T051)
8. **MANDATORY**: Integration tests for all three providers (T073-T075)

**User Story 3**:
1. Delete old files in parallel (T052-T054)
2. Update documentation (T055-T062)
3. Add migration logic (T063-T064)
4. Final cleanup (T065-T066)

### Parallel Opportunities

- **Phase 1**: T002 and T003 can run in parallel with T001
- **Phase 2**: T008 and T009 can run in parallel after T007
- **US1**: T011 and T012 can run in parallel
- **US2**: T027, T028, T029 can run in parallel (different provider implementations)
- **US2**: T037-T039 (Gemini), T040 (OpenAI), T041 (Anthropic) are all error handling for different files
- **US2**: T073, T074, T075 can run in parallel (integration tests for different providers)
- **US3**: T052, T053, T054 can run in parallel (deleting different files)
- **Polish**: T067, T068, T070 can run in parallel

---

## Parallel Example: User Story 2 Provider Implementations

```bash
# Launch all three provider service implementations together:
Task T027: "Implement GeminiService inheriting from AIProviderService in src/EtnoPapers.Core/Services/GeminiService.cs"
Task T028: "Implement OpenAIService inheriting from AIProviderService in src/EtnoPapers.Core/Services/OpenAIService.cs"
Task T029: "Implement AnthropicService inheriting from AIProviderService in src/EtnoPapers.Core/Services/AnthropicService.cs"

# Then launch ExtractMetadata implementations together:
Task T030: "Implement ExtractMetadata method for Gemini API"
Task T031: "Implement ExtractMetadata method for OpenAI API"
Task T032: "Implement ExtractMetadata method for Anthropic API"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001-T005)
2. Complete Phase 2: Foundational (T006-T010) - CRITICAL
3. Complete Phase 3: User Story 1 (T011-T026)
4. **STOP and VALIDATE**: Test configuration UI, API key encryption, provider selection
5. Ready for P2 implementation

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready (T001-T010)
2. Add User Story 1 → Test independently → Configuration works (T011-T026)
3. Add User Story 2 → Test independently → Full PDF processing with cloud AI (T027-T051, T073-T075 integration tests)
4. Add User Story 3 → Test independently → Clean codebase (T052-T066)
5. Polish → Final production-ready state (T067-T072, T076-T080)

### Recommended Order

1. **Phase 1 + Phase 2** (10 tasks) - Critical foundation
2. **User Story 1** (16 tasks) - Configuration MVP
3. **User Story 2** (28 tasks including tests) - Core value delivery with quality gates
4. **User Story 3** (15 tasks) - Cleanup
5. **Polish** (11 tasks) - Production hardening

**Total**: 80 tasks

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- CRITICAL: Ensure API keys are NEVER logged or committed to repository
- All Portuguese messages must be in Brazilian Portuguese
- Maintain 100% compatibility with existing JSON data structure (estrutura.json)
