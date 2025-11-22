# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Etnopapers** is a specialized ethnobotany metadata extraction system designed to automatically extract and catalog information from scientific articles about traditional plant use in indigenous and traditional communities.

**Key Facts:**
- **Current Status**: Design & specification phase (no implementation code yet - only detailed specifications)
- **Frontend**: React 18 + TypeScript (not implemented)
- **Backend**: Python FastAPI (not implemented)
- **Database**: MongoDB (NoSQL with JSON/BSON documents, via MONGO_URI environment variable)
- **Data Model**: Document-centric (references as root documents with aggregated metadata)
- **Deployment**: Docker + Docker Compose for UNRAID servers (lightweight: ~180-200MB)
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
  - `DatabaseDownload`: Download MongoDB database backup (zip archive)

### Backend (Python FastAPI)
- **Structure**: Router-based API with service layer for business logic
- **Database**: MongoDB (NoSQL document store with BSON serialization via PyMongo)
- **Data Layer**: PyMongo API for CRUD operations with MongoDB-style queries
- **Connection**: Via MONGO_URI environment variable (supports local MongoDB or cloud providers like MongoDB Atlas)
- **API Endpoints**: 29 planned endpoints covering articles/references, species, taxonomy validation, locations, communities, drafts, database download
- **Taxonomy Integration**: GBIF API (primary) + Tropicos (fallback) with 30-day in-memory cache
- **Async**: Full async/await support via FastAPI/uvicorn
- **No Authentication**: System designed for open networks (future: add auth layer)

### Database (MongoDB - NoSQL)

**Simplified Document-Centric Model** (single collection with denormalized documents):

A single `referencias` collection stores complete reference documents with all metadata embedded. This model prioritizes simplicity and denormalization over normalization.

**Document Structure Example:**
```json
{
  "_id": "ObjectId",
  "ano": 2010,
  "titulo": "Uso e conhecimento tradicional de plantas medicinais no Sertão",
  "publicacao": "Acta bot. bras. 24(2): 395-406",
  "autores": ["Giraldi, M.", "Hanazaki, N."],
  "resumo": "O objetivo desta pesquisa foi realizar um estudo etnobotânico...",
  "doi": "10.1590/...",
  "especies": [
    {
      "vernacular": "maçanilha",
      "nomeCientifico": "Chamomilla recutita"
    },
    {
      "vernacular": "hortelã-branca",
      "nomeCientifico": "Mentha sp1."
    }
  ],
  "tipo_de_uso": "medicinal",
  "metodologia": "entrevistas",
  "pais": "Brasil",
  "estado": "SC",
  "municipio": "Florianópolis",
  "local": "Sertão do Ribeirão",
  "bioma": "Mata Atlântica",
  "status": "rascunho"
}
```

**Single Collection:**

1. **referencias** - Scientific articles/references (all-in-one collection)
   - `_id`: ObjectId (unique identifier)
   - `ano`: Publication year (indexed for filtering)
   - `titulo`: Article title
   - `publicacao`: Publication venue (journal, conference, etc.)
   - `autores`: Array of author names (strings)
   - `resumo`: Abstract/summary (optional)
   - `doi`: Digital Object Identifier (optional, unique index)
   - `especies`: Array of species objects with `vernacular` and `nomeCientifico`
   - `tipo_de_uso`: Type of use (medicinal, alimentar, etc.)
   - `metodologia`: Research methodology (entrevistas, observação, etc.)
   - `pais`: Country name
   - `estado`: State/province abbreviation
   - `municipio`: Municipality name
   - `local`: Specific location/community name
   - `bioma`: Biome name (Mata Atlântica, Cerrado, etc.)
   - `status`: Document status ('rascunho' or 'finalizado')

**Collection Indexes** (for performance):
- `referencias`: doi (unique), ano, status, titulo (text search friendly)

**Advantages of This Model:**
- ✅ **Simple queries**: All reference data in one document, no JOINs needed
- ✅ **Fast retrieval**: Single find() returns complete reference with all metadata
- ✅ **Schema flexibility**: Add new fields anytime without migrations
- ✅ **Easier extraction**: Direct mapping from AI extraction output to document structure
- ✅ **Smaller database**: No redundant species/location documents

**Estimated Database Sizes (BSON format):**
- 1,000 references: ~1-2 MB (depending on average species count per reference)
- 10,000 references: ~10-20 MB
- 100,000 references: ~100-200 MB

### Docker Setup
```yaml
# Two services: MongoDB + Etnopapers API
# MongoDB Service:
- Port: 27017 (MongoDB server)
- Volume: mongodb_data:/data/db (persistent MongoDB database)
- Image: mongo:7.0
- Health Check: mongosh ping command

# Etnopapers Service:
- Port: 8000 (web app + API)
- Depends on: MongoDB service (healthy condition)
- Base Image: python:3.11-slim (minimal, ~150MB)
- Dependencies: FastAPI, PyMongo (client library)
- Estimated Docker Image Size: ~180-200MB
- Environment (defaults):
  - MONGO_URI=mongodb://mongo:27017/etnopapers (connection to MongoDB service)
  - PORT=8000
  - LOG_LEVEL=info
  - ENVIRONMENT=development
  - TAXONOMY_API_TIMEOUT=5
  - CACHE_TTL_DAYS=30
  - CORS_ORIGINS=http://localhost:3000,http://localhost:8000
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

### Database (MongoDB)
```bash
# MongoDB database is remote/networked (connection via MONGO_URI environment variable)
# Connect to MongoDB using local instance or MongoDB Atlas

# Inspect MongoDB database (from Python)
python -c "
from pymongo import MongoClient

# Connect to MongoDB (use environment variable or local instance)
client = MongoClient('mongodb://localhost:27017/etnopapers')
db = client['etnopapers']

# List all collections
print('Collections:', db.list_collection_names())

# Query examples - get first reference
ref = db['referencias'].find_one()
print('Sample reference:', ref)

# Count documents
print('Total referencias:', db['referencias'].count_documents({}))

# Example: Find all references by year
refs_2010 = list(db['referencias'].find({'ano': 2010}))
print(f'References from 2010: {len(refs_2010)}')

# Example: Search by species name
refs_with_species = list(db['referencias'].find({
    'especies.nomeCientifico': 'Chamomilla recutita'
}))
print(f'References with Chamomilla recutita: {len(refs_with_species)}')
"

# Connection string examples:
# - Local MongoDB: mongodb://localhost:27017/etnopapers
# - MongoDB Atlas: mongodb+srv://user:password@cluster.mongodb.net/etnopapers
# - Docker service: mongodb://mongo:27017/etnopapers
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
- [ ] Setup MongoDB client initialization in backend (MongoClient with MONGO_URI)
- [ ] Create collections: `referencias`, `especies_plantas`, `comunidades_indígenas`, `localizacoes` (optional)
- [ ] Create Pydantic schemas for document validation
- [ ] Implement indexes for performance (DOI, status, ano_publicacao, etc.)
- [ ] Implement backend routers: `/api/referencias` (CRUD), `/api/drafts`, `/api/database/download`
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

2. **MongoDB for NoSQL** (Cloud-Ready):
   - **Zero-Migration Schema Evolution**: New ethnobotanical fields added to code without database migrations
   - **BSON Binary Serialization**: Efficient binary encoding via BSON format
   - **MongoDB Native**: PyMongo API works with local MongoDB or MongoDB Atlas cloud
   - **Flexible Deployment**: Works with local MongoDB server or cloud providers
   - **Environment Configuration**: Connection via MONGO_URI environment variable
   - **Scalability Path**: Can grow from local MongoDB to MongoDB Atlas without code changes
   - **Standards-Based**: Uses standard MongoDB query syntax and operations

3. **Document-Centric Data Model**:
   - **References as Root**: Scientific articles aggregated with species, communities, locations
   - **Nested Objects**: All metadata in single document - one query returns everything
   - **JSON-Native**: BSON natively supports arrays and nested objects (no JSON serialization needed)
   - **Flexible Growth**: New fields added without touching schema or migrations

4. **Collections Instead of Normalized Tables**:
   - `referencias`: Core documents (was 12 separate SQL tables)
   - `especies_plantas`: Taxonomic deduplication (shared across references)
   - `comunidades_indígenas`: Cultural references (shared across articles)
   - `localizacoes`: Geographic data (optional, can be embedded)
   - **Benefits**: Intuitive hierarchy, natural data access, easy schema evolution

5. **Zustand for State Management**: Lightweight, no boilerplate, perfect for storing API keys and editor state.

6. **Direct API Calls**: Frontend makes direct HTTPS calls to Gemini/ChatGPT/Claude APIs, avoiding backend bottleneck.

7. **Researcher Profile Optional**: Personalization improves extraction quality but isn't required for basic operation.

8. **Geographic Flexibility**: Hybrid hierarchy (fixed levels: país→estado→município) + free-form territories (for indigenous lands, quilombos, etc.) accommodates diverse geographic realities.

9. **Taxonomic Caching**: 30-day cache of GBIF validation results reduces API calls and offline-friendly.

10. **Duplicate Detection Multi-Strategy**: DOI uniqueness (MongoDB unique index) + (title+year+author) fallback catches most duplicates without false positives.

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

### Adding a New Document Field (NoSQL)
**No migrations needed!** MongoDB is schema-less. Simply:
1. Update the reference document structure in `backend/models/article.py` (Pydantic schema)
2. Add new field(s) when creating/updating documents in `backend/services/article_service.py`
3. Update any extraction logic to populate the new field
4. Test with pytest
5. For performance: add indexes if filtering by the new field via `db['referencias'].create_index('new_field')` in `backend/database/connection.py`

**Example: Adding a new `palavras_chave` (keywords) field to references**
```python
# In backend/models/article.py (validation schema)
class ReferenceData(BaseModel):
    titulo: str
    autores: List[str]
    especies: List[SpeciesData]
    palavras_chave: Optional[List[str]] = None  # NEW FIELD
    # ... other fields ...

# In backend/services/article_service.py
def create_article(
    titulo: str,
    autores: List[str],
    # ... other params ...
    palavras_chave: Optional[List[str]] = None,  # NEW PARAM
) -> Dict[str, Any]:
    doc = {
        'titulo': titulo,
        'autores': autores,
        'palavras_chave': palavras_chave or [],  # NEW FIELD
        # ... other fields ...
    }
    # Insert document...
```
No migration, no schema update—just add it and deploy!

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

### Backend Won't Connect to Database (MongoDB)
```bash
# Check if MongoDB server is running
# For local MongoDB:
mongosh # Connect to local MongoDB

# For Docker Compose:
docker-compose logs mongo # Check MongoDB service logs
docker-compose ps         # Check if mongo service is running

# Test connection from Python
python -c "
from pymongo import MongoClient
try:
    client = MongoClient('mongodb://localhost:27017/etnopapers', serverSelectionTimeoutMS=5000)
    client.admin.command('ping')
    db = client['etnopapers']
    print('Connected to MongoDB')
    print('Collections:', db.list_collection_names())
except Exception as e:
    print(f'Connection error: {e}')
"

# Verify MONGO_URI environment variable is set:
echo $MONGO_URI

# For Docker, verify the environment variable in docker-compose.yml:
# - MONGO_URI=mongodb://mongo:27017/etnopapers

# Test MongoDB connection from the app container:
docker-compose exec etnopapers python -c "
from pymongo import MongoClient
import os
uri = os.getenv('MONGO_URI')
print(f'MONGO_URI: {uri}')
client = MongoClient(uri)
client.admin.command('ping')
print('Connected!')
"
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
