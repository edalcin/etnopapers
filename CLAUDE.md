# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Etnopapers** is a specialized ethnobotany metadata extraction system designed to automatically extract and catalog information from scientific articles about traditional plant use in indigenous and traditional communities.

**Key Facts:**
- **Current Status**: Design & specification phase (no implementation code yet - only detailed specifications)
- **Frontend**: React 18 + TypeScript (not implemented)
- **Backend**: Python FastAPI (not implemented)
- **Database**: SQLite with 12 normalized tables
- **Deployment**: Docker + Docker Compose for UNRAID servers
- **AI Integration**: Frontend-driven extraction using user-provided API keys (Gemini, ChatGPT, or Claude)

## Architecture Overview

### Frontend (React + TypeScript)
- **State Management**: Zustand for lightweight global state (API keys, extracted metadata, editor state)
- **Tables**: TanStack React Table v8 for articles list (sortable, filterable, paginated)
- **Forms**: react-hook-form for metadata editing with validation
- **PDF Handling**: pdfjs-dist for text extraction, pdf-lib for PDF validation
- **Security Model**: API keys stored ONLY in browser localStorage, never sent to backend
- **Key Components**:
  - `PDFUpload`: Drag-and-drop PDF upload (<50 MB)
  - `APIConfiguration`: Select AI provider + input API key
  - `MetadataDisplay`: Show extracted metadata with three actions (Save/Edit/Discard)
  - `ManualEditor`: Edit and correct extracted data
  - `ArticlesTable`: Browse all articles with sort/filter/pagination
  - `ResearcherProfile`: Optional personalization for extraction context
  - `DatabaseDownload`: Download complete SQLite backup

### Backend (Python FastAPI)
- **Structure**: Router-based API with service layer for business logic
- **Database**: Direct SQLite3 with Alembic migrations
- **API Endpoints**: 29 planned endpoints covering articles, species, taxonomy validation, locations, communities, drafts, database download
- **Taxonomy Integration**: GBIF API (primary) + Tropicos (fallback) with 30-day in-memory cache
- **Async**: Full async/await support via FastAPI/uvicorn
- **No Authentication**: System designed for open networks (future: add auth layer)

### Database (SQLite)
**12 Tables (normalized design):**

1. **ArtigosCientificos** - Scientific articles (main entity)
   - Fields: id, titulo, doi (unique), ano_publicacao, autores (JSON), resumo, status ('rascunho'|'finalizado'), editado_manualmente, data_processamento, data_ultima_modificacao
   - Duplicate detection via DOI or (titulo + ano_publicacao + first_author)

2. **DadosEstudo** - Study methodology (1:1 with articles)
   - Fields: artigo_id, periodo_inicio, periodo_fim, duracao_meses, metodos_coleta_dados, tipo_amostragem, tamanho_amostra, instrumentos_coleta (JSON)

3-5. **Paises** / **Estados** / **Municipios** - Geographic hierarchy (3 levels)

6. **Territorios** - Community territories (non-hierarchical)
   - Example: "Terra Indígena Yanomami", "Quilombo Ivaporunduva"

7. **ArtigoLocalizacao** - Link articles to locations (polymorphic: municipio_id XOR territorio_id)

8. **EspeciesPlantas** - Plant species (scientific names are unique keys)
   - Fields: id, nome_cientifico (unique), autores_nome_cientifico, familia_botanica, nome_aceito_atual, sinonimo_de_id, status_validacao, fonte_validacao, usos_reportados (JSON)

9. **NomesVernaculares** - Common/folk names for plants (supports homonomy)

10. **EspecieNomeVernacular** - N:M relationship (especie ↔ vernacular names with confidence levels)

11. **ArtigoEspecie** - N:M relationship (articles ↔ species with context of use)

12. **Comunidades** - Traditional communities
    - Types: indígena, quilombola, ribeirinha, caiçara, seringueira, pantaneira, outro

**Key Features:**
- 18 indexes for query optimization
- Triggers for auto-updating `data_ultima_modificacao` and marking manual edits
- View `vw_artigos_completos` for aggregated data
- Foreign keys with CASCADE/RESTRICT constraints
- CHECK constraints for valid value ranges (e.g., year ranges, status values)
- JSON columns for variable-length arrays (autores, instrumentos_coleta, parte_planta_utilizada)

**Estimated Database Sizes:**
- 1,000 articles: ~1 MB
- 10,000 articles: ~10 MB
- 100,000 articles: ~100 MB

### Docker Setup
```yaml
# Single service (etnopapers)
- Port: 8000 (web app + API)
- Volume: ./data:/data (persistent SQLite)
- Environment:
  - DATABASE_PATH=/data/etnopapers.db
  - PORT=8000
  - LOG_LEVEL=info
  - TAXONOMY_API_TIMEOUT=5
  - CACHE_TTL_DAYS=30
```

## Development Commands

### Frontend
```bash
npm install              # Install dependencies
npm run dev             # Start dev server (http://localhost:3000)
npm run type-check      # TypeScript type checking
npm run build           # Production build
npm run test            # Run tests (Jest + React Testing Library)
npm run lint            # Linting (ESLint)
```

### Backend
```bash
python -m venv venv                    # Create virtual environment
source venv/bin/activate               # Activate (Linux/Mac)
venv\Scripts\activate                  # Activate (Windows)

pip install -r requirements.txt        # Install dependencies
uvicorn main:app --reload              # Dev server (http://localhost:8000)
uvicorn main:app --reload --log-level debug  # Dev server with debug logging

# API documentation: http://localhost:8000/docs (Swagger UI)
# OpenAPI schema: http://localhost:8000/openapi.json

pytest                                 # Run tests
pytest -v                              # Verbose test output
pytest tests/routers/test_articles.py  # Single test file
```

### Database
```bash
# Running migrations (Alembic)
alembic upgrade head                   # Apply all pending migrations
alembic downgrade -1                   # Rollback one migration
alembic revision --autogenerate -m "description"  # Generate migration from models

# SQLite inspection (from Python)
sqlite3 data/etnopapers.db
  > PRAGMA integrity_check;             # Check database integrity
  > .tables                             # List all tables
  > .schema ArtigosCientificos          # View table schema
  > .quit                               # Exit
```

### Docker
```bash
docker-compose build                   # Build image
docker-compose up -d                   # Start services (detached)
docker-compose logs -f                 # Follow logs
docker-compose down                    # Stop services
docker-compose down -v                 # Stop services and remove volumes

# Build for production
docker build -t etnopapers:latest .
docker run -d --name etnopapers -p 8000:8000 -v $(pwd)/data:/data etnopapers:latest
```

## Testing Strategy

### Frontend
- **Unit tests**: Jest for utilities and hooks
- **Component tests**: React Testing Library for UI components
- **Integration tests**: Test AI extraction flow end-to-end

### Backend
- **Unit tests**: pytest for service layer functions
- **Integration tests**: pytest for API endpoints with test database
- **Database tests**: Schema integrity checks, constraint validation

## Implementation Checklist (Priority Order)

### Phase 0: Infrastructure (P0)
- [ ] Create directory structure (frontend/, backend/, data/, migrations/)
- [ ] Setup Dockerfile with multi-stage build
- [ ] Setup docker-compose.yml
- [ ] Configure backend requirements.txt and frontend package.json

### Phase 1: Core API & Database (P0-P1)
- [ ] Create SQLite schema with all 12 tables, indexes, triggers, constraints
- [ ] Setup Alembic migrations
- [ ] Implement backend routers: `/api/articles` (CRUD), `/api/drafts`, `/api/database/download`
- [ ] Implement database service layer (CRUD operations, duplicate detection)
- [ ] Implement taxonomy service (GBIF/Tropicos API integration with caching)

### Phase 2: Frontend Upload & Display (P1)
- [ ] Setup React project structure, Zustand store, TypeScript config
- [ ] Implement `PDFUpload` component with drag-and-drop
- [ ] Implement `APIConfiguration` component (provider selection, key input/validation)
- [ ] Implement direct AI API calls (Gemini → OpenAI → Claude)
- [ ] Implement `MetadataDisplay` component with Save/Edit/Discard actions
- [ ] Implement `ArticlesTable` component with sort/filter/pagination

### Phase 3: Manual Editing & Management (P2)
- [ ] Implement `ManualEditor` component with form validation
- [ ] Implement draft auto-save and recovery
- [ ] Implement duplicate detection and conflict resolution
- [ ] Implement taxonomic validation UI feedback

### Phase 4: Advanced Features (P2-P3)
- [ ] Implement `ResearcherProfile` personalization
- [ ] Implement database integrity checks before download
- [ ] Add search/filter by community, location, species
- [ ] Add data export options (CSV, JSON)
- [ ] Add analytics dashboard (articles per year, species per community, etc.)

## Key Design Decisions

1. **Frontend-Driven AI Extraction**: API keys never leave the browser. Backend only manages metadata persistence. This eliminates key management overhead and privacy concerns.

2. **SQLite for Portability**: Single-file database makes backup, distribution, and local analysis trivial. No server required.

3. **Zustand for State Management**: Lightweight, no boilerplate, perfect for storing API keys and editor state.

4. **Direct API Calls**: Frontend makes direct HTTPS calls to Gemini/ChatGPT/Claude APIs, avoiding backend bottleneck.

5. **Researcher Profile Optional**: Personalization improves extraction quality but isn't required for basic operation.

6. **Geographic Flexibility**: Hybrid hierarchy (fixed levels: país→estado→município) + free-form territories (for indigenous lands, quilombos, etc.) accommodates diverse geographic realities.

7. **Taxonomic Caching**: 30-day cache of GBIF validation results reduces API calls and offline-friendly.

8. **Duplicate Detection Multi-Strategy**: DOI uniqueness + (title+year+author) fallback catches most duplicates without false positives.

9. **Polymorphic Location Link**: Articles can link to either municipal/state level OR community territories, not both - maintains data integrity.

10. **JSON Columns for Flexibility**: Arrays stored as JSON (autores, instrumentos_coleta, usos_reportados) avoid unnormalized design while staying flexible.

## AI Provider Integration

### Supported Providers

**Google Gemini** (recommended for starting)
- Free quota: 60 requests/minute
- Cost: ~$0.00025 per 1K characters (after quota)
- Good Portuguese language support
- Endpoint: `POST https://generativelanguage.googleapis.com/v1/models/gemini-pro:generateContent`

**OpenAI ChatGPT**
- Model: gpt-3.5-turbo
- Cost: $0.0015 per 1K input tokens (~$0.02 per article)
- High-quality extraction
- Endpoint: `POST https://api.openai.com/v1/chat/completions`

**Anthropic Claude**
- Model: claude-3-haiku (cheapest) or claude-3-sonnet
- Cost: $0.00025 per 1K tokens (Haiku) - most economical
- Excellent for long/complex documents
- Endpoint: `POST https://api.anthropic.com/v1/messages`

### Extraction Prompt Structure
- System prompt defines JSON schema for output (guaranteed structured data)
- User prompt includes extracted PDF text + researcher profile context
- Response parsed as JSON and validated against schema before saving

## Documentation References

- **Complete Spec**: `specs/main/spec.md` - All functional requirements, user stories, acceptance criteria
- **Data Model**: `specs/main/data-model.md` - Full SQL schema with 30+ example queries
- **Installation Guide**: `specs/main/quickstart.md` - UNRAID & Docker setup, first-use walkthrough
- **Roadmap**: `specs/main/plan.md` - Project phases and timeline
- **Implementation Tasks**: `specs/main/tasks.md` - Tasks by priority (P0-P3) with effort estimates
- **Technical Decisions**: `specs/main/research.md` - Architecture rationale and code examples
- **API Specification**: `specs/main/contracts/api-rest.yaml` - OpenAPI 3.0 spec for all 29 endpoints
- **AI Integration Guide**: `specs/main/contracts/ai-integration.md` - AI API integration patterns

## Common Patterns

### Adding a New API Endpoint
1. Define Pydantic schema in `backend/models/`
2. Create router function in `backend/routers/`
3. Implement service layer in `backend/services/`
4. Call service from router with error handling
5. Test with pytest
6. Document in OpenAPI spec (`specs/main/contracts/api-rest.yaml`)

### Adding a Database Table
1. Define table in schema migration (`backend/migrations/versions/`)
2. Run migration: `alembic upgrade head`
3. Create Pydantic schema in `backend/models/`
4. Create service methods for CRUD
5. Create router endpoint
6. Add indexes and constraints as needed

### Extracting Metadata from PDF
1. Frontend uses `pdfjs-dist` to extract text
2. Frontend constructs extraction prompt with researcher profile context
3. Frontend makes direct HTTPS call to selected AI API
4. Frontend parses JSON response
5. Frontend sends to backend `/api/articles` POST endpoint

### Validating Plant Species Names
1. Backend receives `nome_cientifico` in article POST request
2. Backend queries GBIF Species API
3. If found: return standardized name + family + accepted status
4. If not found: try Tropicos fallback
5. If both fail: save as "não validado", user can re-validate later
6. Cache result for 30 days to reduce API calls

## Troubleshooting

### Frontend Dev Server Won't Start
```bash
# Clear node_modules and reinstall
rm -rf node_modules package-lock.json
npm install
npm run dev
```

### Backend Won't Connect to Database
```bash
# Check if database file exists
ls -la data/etnopapers.db

# Check database integrity
sqlite3 data/etnopapers.db "PRAGMA integrity_check;"

# Recreate from migration
rm data/etnopapers.db
alembic upgrade head
```

### API Key Validation Fails
- Verify key format is correct for selected provider
- Check if API key has sufficient quota/credits
- Verify network connectivity to provider's API
- Check browser localStorage for key storage issues

### Docker Build Fails
- Ensure Node.js version matches `frontend/package.json` engines
- Ensure Python version matches `backend/requirements.txt`
- Check Docker build output for dependency resolution errors
- Try `docker-compose build --no-cache` to skip cached layers

### Slow Taxonomy Validation
- Check if GBIF API is responding: `curl https://api.gbif.org/v1/species/search?q=Areca`
- Check cache hit rate in logs
- Consider increasing `CACHE_TTL_DAYS` environment variable
- Fallback to Tropicos if GBIF is slow

## Notes for Future Development

- **Authentication**: Currently no auth layer. Future implementation should use JWT tokens stored in HttpOnly cookies.
- **Rate Limiting**: Plan to add rate limiting on database download endpoint to prevent abuse.
- **Logging**: Implement structured logging (Winston/Pino for Node, loguru for Python) with request tracing.
- **Monitoring**: Setup error tracking (Sentry) and APM (New Relic/DataDog) for production.
- **Localization**: App is designed for Portuguese speakers, but strings should be i18n-ready for future multi-language support.
- **Advanced Search**: Current filter is basic substring matching. Future: full-text search with SQL FTS5 or Elasticsearch.
- **Analytics Dashboard**: Planned feature to visualize article counts, species coverage, geographic distribution.
- **Batch Upload**: Currently single PDF per session. Future: queue multiple PDFs for processing.
- **Community Collaboration**: Current design is single-user per server. Future: add multi-user with sharing/collaboration features.
