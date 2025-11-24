# Implementation Plan: Etnopapers v2.0 (Local AI + MongoDB)

**Branch**: `main` | **Data**: 2025-11-24 | **Versão**: 2.0 | **Especificação**: `specs/main/spec.md`
**Entrada**: Especificação funcional v2.0 em `/specs/main/spec.md` com 68 requisitos (RF-001 a RF-068)

**Status**: ✅ Plan actualizado para v2.0 (2025-11-24) - Ollama + MongoDB + GPU

---

## Resumo Executivo

**Etnopapers v2.0** é um sistema de extração de metadados de artigos científicos sobre etnobotânica com **Inferência Local de AI** rodando no próprio servidor UNRAID.

### Arquitetura Principal v2.0

- ✅ **AI Local**: Ollama + Qwen2.5-7B-Instruct em GPU NVIDIA (1-3s por artigo)
- ✅ **Privacidade Total**: 100% dos dados permanecem no servidor, zero APIs externas
- ✅ **Custo Zero**: $0 por artigo (vs $0.01-0.05 em v1.0)
- ✅ **Offline-Capable**: Funciona completamente offline após download do modelo
- ✅ **MongoDB**: Banco NoSQL document-centric com backup automático (ZIP)
- ✅ **Sem API Keys**: Sem configuração de chaves no frontend

### Tecnologia

| Camada | Tecnologia | Versão | Notas |
|--------|-----------|--------|-------|
| **AI** | Ollama + Qwen2.5 | Q4 (4.8GB) | GPU NVIDIA obrigatória |
| **Backend** | FastAPI + PyMongo | 3.11 | `/api/extract/metadata` novo |
| **Frontend** | React + TypeScript | 18/5 | Sem API key config |
| **Database** | MongoDB | 5.0+ | 4 collections, 18 índices |
| **Deployment** | Docker Compose | 3 services | MongoDB + Ollama + API |
| **GPU Runtime** | nvidia-docker | latest | GPU passthrough |

---

## Contexto Técnico Detalhado

### Stack & Dependências

**Backend Python**:
```
FastAPI 0.104+ - Web framework assíncrono
PyMongo - Cliente MongoDB
pdfplumber - Extração de texto de PDFs
instructor - Structured outputs com Ollama
openai - Cliente OpenAI-compatible para Ollama
```

**Frontend JavaScript**:
```
React 18+ - UI library
TypeScript 5+ - Tipagem estática
Zustand 4+ - State management
TanStack Table v8 - Tabelas complexas
react-hook-form - Validação de formulários
pdfjs-dist - Extração de PDF no navegador
```

**Serviços**:
```
MongoDB 5.0+ - NoSQL document store
Ollama latest - Local inference framework
NVIDIA GPU Driver - GPU compute
nvidia-docker - GPU passthrough
```

### Restrições de Projeto v2.0

- **GPU obrigatória**: RTX 3060+ (6-8 GB VRAM) para produção
- **Sem autenticação**: Sistema de rede local (UNRAID)
- **Docker Compose**: 3 serviços coordenados
- **MongoDB**: Portabilidade com backup ZIP
- **PDFs < 50 MB**: Limite de upload
- **Ollama local**: Sem fallback para APIs externas

### Metas de Performance

| Métrica | Alvo | Notas |
|---------|------|-------|
| Inferência Ollama (GPU) | 1-3s | Qwen2.5 RTX 3060+ |
| Extração completa | < 1 min | Upload + AI + save |
| Tabela (1000 artigos) | < 2s | Paginação client-side |
| Filtro em tempo real | < 500ms | Debounce 300ms |
| Validação taxonômica | < 100ms | Cache em memória |
| Download MongoDB | < 3s | Backup ZIP streaming |
| Acurácia | > 80% | Campos principais |
| Offline operation | 100% | Após setup |

### Dependências Externas (Opcionais)

- **GBIF Species API**: Validação taxonômica (fallback: Tropicos)
- **Internet**: Uma única vez para download modelo (~4.8 GB)
- **Sem APIs de IA externas**: Ollama substitui Gemini/ChatGPT/Claude

---

## Verificação da Constituição

**Constitution v2.0.0 (2025-11-24)** - ✅ PASS

| Princípio | Status | Verificação |
|-----------|--------|-------------|
| **I. Privacidade** | ✅ PASS | Sem APIs externas, dados locais |
| **II. Portabilidade** | ✅ PASS | MongoDB com backup ZIP |
| **III. Offline Tolerance** | ✅ PASS | Ollama local com fallback claro |
| **IV. Simplicidade** | ✅ PASS | Docker Compose, sem auth |
| **V. GPU Deploy** | ✅ PASS | GPU NVIDIA autorizada, nvidia-docker |
| **VI. Português BR** | ✅ PASS | Todos strings em português |

---

## Estrutura do Projeto

### Diretórios Principais

```
etnopapers/
├── frontend/                    # React 18 + TypeScript
│   ├── src/
│   │   ├── components/         # UI components (sem APIConfiguration)
│   │   ├── pages/              # Upload, History, Drafts, Settings
│   │   ├── services/           # API client (backend calls)
│   │   ├── store/              # Zustand (sem API keys)
│   │   └── types/              # TypeScript interfaces
│   ├── package.json
│   ├── vite.config.ts
│   └── tsconfig.json
│
├── backend/                     # FastAPI + Python 3.11
│   ├── main.py                  # FastAPI app
│   ├── config.py                # Env config (MONGO_URI, OLLAMA_URL)
│   ├── models/                  # Pydantic schemas
│   ├── routers/                 # API endpoints
│   │   ├── articles.py          # CRUD articles
│   │   ├── extraction.py        # NEW: /api/extract/metadata
│   │   ├── species.py           # CRUD species
│   │   ├── regions.py           # Geographic data
│   │   ├── database.py          # Download backup
│   │   └── communities.py       # Communities
│   ├── services/                # Business logic
│   │   ├── extraction_service.py # NEW: Ollama client
│   │   ├── taxonomy_service.py  # GBIF + Tropicos
│   │   ├── article_service.py   # CRUD + duplicates
│   │   └── species_service.py   # Species queries
│   ├── database/
│   │   ├── connection.py        # MongoDB client
│   │   └── schema.py            # Collection schemas + indexes
│   └── requirements.txt
│
├── docker-compose.yml           # MongoDB + Ollama + API
├── Dockerfile                   # Multi-stage build
├── .dockerignore
├── requirements.txt             # Python deps
├── CLAUDE.md                    # v2.0 guidelines
└── README.md                    # v2.0 setup

specs/main/
├── spec.md                      # ✅ v2.0 specification
├── plan.md                      # ✅ THIS FILE (v2.0)
├── tasks.md                     # ✅ v2.0 tasks (regenerated)
├── contracts/
│   └── ai-integration.md        # ✅ v2.0 Ollama spec
├── local-ai-integration.md      # Reference (technical deep-dive)
├── plan-local-ai.md             # Reference (detailed roadmap)
└── archive/v1.0/                # Archived v1.0 docs
    ├── plan-v1.0.md
    ├── tasks-v1.0.md
    └── README.md
```

---

## Phases & Timeline

### Fase 0: Infrastructure Setup (1-2 days)

**Objetivos**: Configurar Docker Compose com 3 serviços, GPU passthrough, e persistência.

**Tarefas**:
1. Atualizar docker-compose.yml:
   - Ollama service com GPU passthrough (nvidia-docker)
   - ollama_models volume (~4.8 GB)
   - Qwen2.5 model download & healthcheck
2. Configurar NVIDIA Container Toolkit
3. Environment variables (MONGO_URI, OLLAMA_URL, OLLAMA_MODEL)
4. Health checks para MongoDB e Ollama

**Saída**: `docker-compose up` inicia 3 serviços com GPU

---

### Fase 1: Backend Extraction Service (2-3 days)

**Objetivos**: Implementar AI local inference e novo endpoint de extração.

**Tarefas**:
1. Create `backend/services/extraction_service.py`:
   - OllamaClient (instructor + Pydantic)
   - Structured output schemas (ReferenceMetadata, SpeciesData)
   - Error handling & retries
2. Create `backend/routers/extraction.py`:
   - POST `/api/extract/metadata` endpoint
   - Multipart/form-data (pdf_file, researcher_profile)
   - PDF text extraction (pdfplumber)
3. Update `backend/config.py`:
   - OLLAMA_URL, OLLAMA_MODEL environment variables
4. Tests:
   - Unit tests para extraction_service
   - Integration tests para endpoint

**Saída**: `/api/extract/metadata` funcional, retorna metadados estruturados

---

### Fase 2: Frontend Integration (1-2 days)

**Objetivos**: Atualizar frontend para chamar backend em vez de APIs externas.

**Tarefas**:
1. Remove `APIConfiguration` component (sem mais API key setup)
2. Remove API key management from Zustand store
3. Create `frontend/src/services/extractionService.ts`:
   - POST to `/api/extract/metadata`
   - Response parsing
   - Error handling
4. Update `PDFUpload` component:
   - Call backend endpoint instead of external AI APIs
5. Update error messages para português
6. Remove external AI client files (geminiClient.ts, openaiClient.ts, claudeClient.ts)

**Saída**: Frontend calls backend `/api/extract/metadata`, works end-to-end

---

### Fase 3: Testing & Refinement (2-3 days)

**Objetivos**: Validar qualidade de extração, performance, e casos extremos.

**Tarefas**:
1. Create test dataset (20 PDFs com ground truth)
2. Measure extraction accuracy by field
3. Performance benchmarking across GPU models
4. Optimize prompts para melhor qualidade
5. Test edge cases:
   - Scanned PDFs (aviso de qualidade)
   - Artigos muito longos (> 20 páginas)
   - PDFs com tabelas complexas
6. Production readiness checks

**Saída**: Quality metrics, performance baselines, prompt optimizations

---

### Fase 4: Documentation & Deploy (1 day)

**Objetivos**: Documentação completa e preparação para produção.

**Tarefas**:
1. Update README:
   - GPU setup instructions (NVIDIA driver, nvidia-docker)
   - UNRAID-specific deployment guide
   - Troubleshooting guide
2. Create DEPLOYMENT.md:
   - docker-compose.yml configuration
   - Environment variables
   - Backup strategy
3. Update CLAUDE.md (already done)
4. Release notes v2.0

**Saída**: Production-ready system on UNRAID with GPU

---

## Roadmap Detalhado

### Sprint 1: Infrastructure (Week 1, Day 1-2)

- [ ] TASK-001: Update docker-compose.yml com Ollama service
- [ ] TASK-002: Configure GPU passthrough (nvidia-docker)
- [ ] TASK-003: Create MongoDB schema e índices
- [ ] TASK-004: Test docker-compose up com healthchecks

**Saída**: 3 serviços rodando (MongoDB, Ollama, API placeholder)

---

### Sprint 2: Backend Extraction (Week 1, Day 3-5)

- [ ] TASK-005: Create extraction_service.py
- [ ] TASK-006: Implement Ollama client com instructor
- [ ] TASK-007: Create /api/extract/metadata endpoint
- [ ] TASK-008: PDF text extraction (pdfplumber)
- [ ] TASK-009: Error handling & structured validation
- [ ] TASK-010: Unit + integration tests

**Saída**: Backend API funcional, local AI working

---

### Sprint 3: Frontend (Week 2, Day 1-2)

- [ ] TASK-011: Remove APIConfiguration component
- [ ] TASK-012: Remove API key management
- [ ] TASK-013: Create extractionService.ts
- [ ] TASK-014: Update PDFUpload component
- [ ] TASK-015: Update error messages (português)
- [ ] TASK-016: Remove old AI client files

**Saída**: Frontend integrado com backend

---

### Sprint 4: Testing & Refinement (Week 2, Day 3-5)

- [ ] TASK-017: Create test dataset (20 PDFs)
- [ ] TASK-018: Extraction accuracy measurement
- [ ] TASK-019: Performance benchmarking
- [ ] TASK-020: Prompt optimization
- [ ] TASK-021: Edge case testing
- [ ] TASK-022: Production readiness checklist

**Saída**: Quality metrics, performance data, optimized prompts

---

### Sprint 5: Documentation (Week 3, Day 1)

- [ ] TASK-023: Update README (GPU, UNRAID, troubleshooting)
- [ ] TASK-024: Create DEPLOYMENT.md
- [ ] TASK-025: Release notes v2.0
- [ ] TASK-026: Final testing on UNRAID

**Saída**: Production-ready v2.0 release

---

## Ordem de Execução (Caminho Crítico)

Para MVP funcional:

1. **Infrastructure** (TASK-001 a 004) → Bloqueia tudo
2. **Backend Extraction** (TASK-005 a 010) → Bloqueia frontend
3. **Frontend** (TASK-011 a 016) → Pode rodar em paralelo com testes
4. **Testing** (TASK-017 a 022) → Valida qualidade
5. **Docs** (TASK-023 a 026) → Final

**Total estimado**: 7-10 dias (com team de 1 dev)

---

## Próximos Passos

1. ✅ Constitution v2.0 aprovada
2. ✅ Plan.md v2.0 finalizado
3. ⏳ Tasks.md v2.0 regeneração
4. ➡️ **Iniciar Fase 0: Infrastructure Setup**
   - Atualizar docker-compose.yml
   - Configurar NVIDIA Container Toolkit
   - Testar Ollama container

---

**Última atualização**: 2025-11-24
**Versão**: 2.0 (Local AI + MongoDB)
**Status**: Pronto para implementação
**Maintainer**: Etnopapers Team
