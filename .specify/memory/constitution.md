# Etnopapers Project Constitution

## Core Principles

### I. Privacy-First Architecture & Local AI Inference
**All data processing happens server-side with zero external API calls.** Backend runs Ollama (local AI inference) in container; frontend sends PDFs to backend `/api/extract/metadata` endpoint for processing. Users never provide API keys—system is self-contained. No AI keys, credentials, or sensitive data leave the server. All user data stays on server (UNRAID or local network) and never transmitted to external services.

### II. Data Portability & Locality
**All persistent data lives in a self-contained database with portable backup capability.** System supports MongoDB for scalability and document-centric storage, with automatic backup export functionality (ZIP format). Database connection configurable via MONGO_URI environment variable for local or cloud deployments. System must operate completely offline except for: (a) initial model download (Ollama Qwen2.5), (b) optional GBIF/Tropicos taxonomy validation, and (c) Ollama local inference service. Users can backup MongoDB via native export tools and restore to any MongoDB instance.

### III. Offline Tolerance & Graceful Degradation
**System continues functioning when external APIs are unavailable and provides clear feedback on service status.**

*Local Services*: Ollama inference engine is co-deployed with application and expected to be highly available. If Ollama service fails, user sees clear error message: "Serviço de AI local indisponível. Verifique Ollama ou reinicie o container." Extraction operations gracefully reject with actionable feedback rather than hang or cascade failure.

*External APIs*: If GBIF/Tropicos taxonomy APIs are offline, system allows saving metadata but marks species validation as "não validado" and shows warning: "Validação taxonômica temporariamente indisponível. Dados salvos sem validação."

*No data loss* from service outages. Users can retry taxonomy validation or species correction later.

### IV. Simplicity & MVP-First
**Start simple; implement only what's explicitly required in spec. Avoid premature abstraction, unnecessary complexity, or "future-proofing" for unspecified scenarios.** No authentication, multi-user, analytics, or advanced search until explicitly requested. Single container deployment; no microservices.

### V. Portable Docker Deployment with GPU Acceleration
**System packages as Docker Compose deployment with GPU acceleration for production performance.** Deployment targets: UNRAID servers with NVIDIA GPU (6-8 GB VRAM minimum, e.g., RTX 3060+) and standard Linux hosts with docker-compose and nvidia-docker runtime.

*Hardware requirements*: GPU mandatory for production (acceptable latency 1-3 seconds per article inference). CPU-only operation supported for testing/development (latency 30-60 seconds, not production-ready).

*Configuration*: Database connection (MONGO_URI) and Ollama service URL (OLLAMA_URL) configurable via environment variables. Container startup under 30 seconds (including Ollama health check). Deployment via single `docker-compose up` command.

### VI. Portuguese-First Localization
**All user-facing text, documentation, error messages, and UI labels are in Brazilian Portuguese.** Feature spec, implementation docs, and API contracts written in Portuguese. English used only for technical code comments and dependency names where unavoidable.

## Security Requirements

- **Transport Security**: All API calls to external services use HTTPS with certificate validation
- **Input Validation**: Pydantic models validate all backend inputs; reject oversized requests (PDFs > 50 MB)
- **Database Integrity**: MongoDB collection validation (`db.collection.validate()`) and index verification performed before database downloads; backup ZIP integrity verified via checksum
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

**Version**: 2.0.0 | **Ratified**: 2025-11-20 | **Last Amended**: 2025-11-24 | **Status**: v2.0 (Local AI, MongoDB, GPU-Accelerated)
