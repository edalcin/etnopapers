# Etnopapers Project Constitution

## Core Principles

### I. Privacy-First Architecture
**User API keys are stored ONLY in browser localStorage and never transmitted or persisted on the server.** The frontend makes direct HTTPS calls to AI provider APIs (Gemini, ChatGPT, Claude) using user-provided keys. Backend never handles, logs, or stores API keys under any circumstance.

### II. Data Portability & Locality
**All persistent data lives in a single SQLite database file.** No external dependencies for data storage (no PostgreSQL, MongoDB, or cloud services required). System must operate completely offline except for optional API calls to external taxonomic and AI services. Users can backup, restore, and analyze the database using any standard SQLite tool.

### III. Offline Tolerance & Graceful Degradation
**System continues functioning when external APIs are unavailable.** If GBIF/Tropicos APIs are offline, system allows saving metadata but marks species validation as "não validado". If AI provider is unreachable, user sees clear error with retry option. No data loss or cascade failures from external service outages.

### IV. Simplicity & MVP-First
**Start simple; implement only what's explicitly required in spec. Avoid premature abstraction, unnecessary complexity, or "future-proofing" for unspecified scenarios.** No authentication, multi-user, analytics, or advanced search until explicitly requested. Single container deployment; no microservices.

### V. Portable Docker Deployment
**System packages as single Docker container compatible with standard Docker installations (no GPU, nvidia-docker, or specialized hardware required).** Deployment target: UNRAID servers and standard Linux hosts. Database path configurable via environment variable. Container startup under 10 seconds.

### VI. Portuguese-First Localization
**All user-facing text, documentation, error messages, and UI labels are in Brazilian Portuguese.** Feature spec, implementation docs, and API contracts written in Portuguese. English used only for technical code comments and dependency names where unavoidable.

## Security Requirements

- **Transport Security**: All API calls to external services use HTTPS with certificate validation
- **Input Validation**: Pydantic models validate all backend inputs; reject oversized requests (PDFs > 50 MB)
- **Database Integrity**: SQLite PRAGMA integrity_check required before database downloads; triggers maintain audit trail
- **No Secrets in Logs**: API keys, user data never logged or exposed in error messages
- **CORS Configuration**: Backend accepts requests only from configured frontend origin

## Performance Standards

- **PDF Processing**: Extract text and send to AI within 2 minutes for documents up to 30 pages
- **Table Operations**: Display 1000 articles in < 2 seconds; filter responds in < 500 ms with debounce
- **Taxonomy Caching**: In-memory 30-day cache reduces GBIF API calls; typical query < 100 ms
- **Download**: Database download initiated within 3 seconds; streaming for files > 10 MB

## Quality Gates

### Phase 0 Research
- All NEEDS CLARIFICATION items resolved with evidence-based decisions
- Technology choices documented with rationale and alternatives considered

### Phase 1 Design
- Data model reflects all entities from spec with validation rules
- API contracts cover 100% of functional requirements (RF-001 through RF-068)
- No unresolved ambiguities in architecture or design

### Phase 2 Implementation
- Unit test coverage > 80% for backend services
- Integration tests cover critical paths: upload → extraction → save → download
- Manual testing confirms Portuguese localization is complete and correct
- Database integrity checks pass with test data sets (100-10,000 articles)

## Governance

**This constitution is non-negotiable.** All design, implementation, and testing decisions must comply with these principles. Constitution amendments require:

1. Documentation of the change and rationale
2. Clear migration plan for affected systems
3. Explicit approval before implementation

All PRs and reviews must verify compliance with these principles. Complexity violations (e.g., "we need microservices" or "add Postgres for better performance") must be justified by evidence in the Complexity Tracking section of plan.md.

Development teams should consult `/CLAUDE.md` for implementation guidance and patterns that exemplify these principles.

---

**Version**: 1.0.0 | **Ratified**: 2025-11-20 | **Last Amended**: 2025-11-20
