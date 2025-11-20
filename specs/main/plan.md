# Implementation Plan: Sistema de Extração de Metadados de Artigos Etnobotânicos (Etnopapers)

**Branch**: `main` | **Data**: 2025-11-20 | **Especificação**: `specs/main/spec.md`
**Entrada**: Especificação funcional completa em `/specs/main/spec.md` com 68 requisitos (RF-001 a RF-068)

**Nota**: Este plano é gerado pelo comando `/speckit.plan` e segue fluxo em `.specify/templates/commands/plan.md`.

## Resumo Executivo

**Etnopapers** é um sistema de código aberto para extração automática de metadados de artigos científicos em PDF sobre etnobotânica. O sistema processa PDFs em navegador, extrai metadados usando APIs de IA (Gemini, ChatGPT, Claude—escolha do usuário), valida nomes científicos de plantas via GBIF/Tropicos, e armazena tudo em banco SQLite portável.

**Abordagem Técnica**:
1. **Frontend React 18 + TypeScript**: Interface web responsiva com upload drag-and-drop, exibição de metadados extraídos, edição manual, tabela com filtros/ordenação
2. **Backend FastAPI**: API REST para CRUD de artigos, duplicatas, validação taxonômica, download do banco
3. **Banco de Dados SQLite**: 12 tabelas normalizadas com hierarquia geográfica (país/estado/município), territórios comunitários, espécies de plantas com nomes vernaculares N:M
4. **Segurança por Design**: Chaves de API armazenadas APENAS no localStorage do navegador; backend nunca toca chaves do usuário
5. **Docker Single-Container**: Deployment simplificado para UNRAID e servidores Linux; sem GPU ou dependências especializadas

**Diferencial**: Combina simplicidade (MVP focado) com flexibilidade (3 provedores de IA, taxonomia plugável, dados portáveis em SQLite).

## Contexto Técnico

### Stack & Versões

**Backend**:
- **Linguagem**: Python 3.11+
- **Framework**: FastAPI 0.104+ (async ASGI, validação Pydantic automática)
- **Servidor**: Uvicorn (incluído no FastAPI)
- **ORM/Queries**: SQLite3 direto com Alembic para migrações

**Frontend**:
- **Framework**: React 18+ com TypeScript 5+
- **Build**: Vite 5+ (dev server rápido, build otimizado)
- **Gerenciamento de Estado**: Zustand 4+ (leve, sem boilerplate)
- **Componentes**: TanStack React Table v8 (tabelas complexas), react-hook-form (formulários)
- **PDF**: pdfjs-dist (extração de texto no navegador), Tesseract.js (OCR para PDFs escaneados)

**Database**:
- **Tipo**: SQLite 3.35+ com WAL (Write-Ahead Logging) para concorrência
- **Tamanho Estimado**: ~100 MB para 10K artigos
- **Migrações**: Alembic com versionamento automático

**Deployment**:
- **Containerização**: Docker (multi-stage build, Alpine Linux, ~300 MB imagem final)
- **Docker Compose**: Dev environment com volume montado em `/data`
- **Alvo**: UNRAID (mnt/user/appdata) e servidores Linux padrão (sem GPU)

### Dependências Críticas (Serviços Externos - Opcionais)
- Google Gemini API (quota gratuita generosa)
- OpenAI ChatGPT API (alta qualidade)
- Anthropic Claude API (melhor CORS)
- GBIF Species API (validação taxonômica)
- Tropicos API (fallback taxonomia)

### Metas de Performance

| Métrica | Alvo |
|---------|------|
| Extração PDF (até 30 pgs) | < 2 min |
| Tabela 1000 artigos | < 2 s |
| Filtro em tempo real | < 500 ms |
| Validação taxonômica (cached) | < 100 ms |
| Download banco | < 3 s |

### Restrições

- Chaves de API NUNCA no servidor (privacidade)
- Sem autenticação (rede local)
- Um container Docker (simplicidade)
- SQLite (portabilidade)
- PDFs < 50 MB

### Escopo

- **Tipo**: Web application (React + FastAPI)
- **Escala**: 1-100K artigos por instância
- **Usuários**: Pesquisadores etnobotânicos individuais
- **Complexidade**: 33 tarefas, 134 pontos = 268-536 horas

## Verificação da Constituição

*GATE: Deve passar antes de Phase 0. Re-verificar após Phase 1.*

### Princípios da Constituição vs. Design Proposto

| Princípio | Status | Verificação |
|-----------|--------|-------------|
| **I. Privacidade em Primeiro Lugar** | ✅ PASS | Chaves de API APENAS em localStorage; backend nunca toca chaves do usuário. Claude API tem CORS completo. |
| **II. Portabilidade de Dados** | ✅ PASS | SQLite como único banco; sem PostgreSQL, MongoDB, ou dependências externas. Usuários podem backup/restore com ferramentas padrão SQLite. |
| **III. Tolerância Offline** | ✅ PASS | Sistema funciona sem GBIF/AI APIs. Marca dados como "não validado" em vez de falhar. |
| **IV. Simplicidade MVP** | ✅ PASS | Sem autenticação, multi-usuário, analytics até serem explicitamente requisitados. Frontend + Backend em um container. |
| **V. Docker Portável** | ✅ PASS | Container único, Alpine, sem GPU. Compatível com Docker padrão e UNRAID. |
| **VI. Português Brasileiro** | ✅ PASS | Todos os strings de usuário, docs, e API em português. Código/comentários em inglês onde necessário. |

### Gatilhos de Qualidade - Fase 0 & 1

**Fase 0 (Research)**:
- [x] Todas as decisões técnicas documentadas (pdf.js, Claude API, SQLite WAL, etc.)
- [x] Alternativas consideradas com rationale
- [x] Riscos identificados e mitigações propostas

**Fase 1 (Design)**:
- [ ] Data model reflete todas as 12 tabelas com validações
- [ ] Contratos API cobrem 100% dos requisitos (RF-001 a RF-068)
- [ ] Sem ambiguidades não resolvidas na arquitetura
- [ ] Diagrama ER e arquitetura de componentes criados

## Estrutura do Projeto

### Documentação (specs/)

```text
specs/main/
├── plan.md              # Este arquivo (output /speckit.plan)
├── research.md          # Phase 0 output: decisões técnicas, rationale, alternativas
├── data-model.md        # Phase 1 output: entidades, schemas, validações
├── quickstart.md        # Phase 1 output: guia setup dev/prod
├── spec.md              # Especificação funcional (INPUT)
├── tasks.md             # Phase 2 output (/speckit.tasks)
├── contracts/           # Phase 1 output
│   ├── api-rest.yaml    # OpenAPI 3.0 com 29 endpoints
│   └── ai-integration.md # Padrões integração Gemini/ChatGPT/Claude
└── [outros docs]        # research.md (análise), data-model.md (SQL detalhado), etc.
```

### Código Fonte (raiz do repositório)

**Decisão de Estrutura**: Web application (Frontend React + Backend FastAPI) em estrutura separada mas unified em um container Docker.

```text
etnopapers/
├── frontend/                    # React 18 + TypeScript
│   ├── src/
│   │   ├── components/         # UI components (PDFUpload, MetadataDisplay, ArticlesTable, etc.)
│   │   ├── pages/              # React Router pages (Upload, History, Settings, Drafts)
│   │   ├── services/           # API clients, PDF extraction, AI integration
│   │   │   ├── ai/             # geminiClient.ts, openaiClient.ts, claudeClient.ts
│   │   │   ├── pdfExtractor.ts # pdf.js + Tesseract.js
│   │   │   └── apiClient.ts    # Backend API calls
│   │   ├── store/              # Zustand store (apiKeys, metadata, drafts)
│   │   ├── types/              # TypeScript interfaces
│   │   ├── utils/              # fileValidation, metadataParser, etc.
│   │   ├── hooks/              # useAutoSave, useArticlesTable, etc.
│   │   └── App.tsx, main.tsx    # Entry points
│   ├── public/                  # Static assets, pdf.worker.js
│   ├── package.json             # React + Vite + dependencies
│   ├── tsconfig.json
│   ├── vite.config.ts
│   └── tests/                   # Jest + React Testing Library

├── backend/                     # FastAPI + Python 3.11
│   ├── main.py                  # FastAPI app, CORS, middleware
│   ├── config.py                # Environment, logging, constants
│   ├── models/                  # Pydantic schemas (ArticleRequest, SpeciesResponse, etc.)
│   ├── routers/                 # API routes
│   │   ├── articles.py          # CRUD articles
│   │   ├── species.py           # CRUD species
│   │   ├── regions.py           # Locations (countries/states/municipalities)
│   │   ├── communities.py       # Traditional communities
│   │   ├── database.py          # Download, info endpoints
│   │   └── export.py            # CSV export
│   ├── services/                # Business logic
│   │   ├── article_service.py   # CRUD + duplicate detection
│   │   ├── taxonomy_service.py  # GBIF + Tropicos integration + cache
│   │   ├── species_service.py   # Species queries
│   │   └── export_service.py    # CSV generation
│   ├── database/
│   │   ├── connection.py        # SQLite3 + WAL config
│   │   ├── schema.sql           # DDL (12 tables, triggers, indexes)
│   │   └── migrations/          # Alembic versions
│   └── tests/                   # pytest
│       ├── test_articles.py
│       ├── test_taxonomy.py
│       ├── test_duplicates.py
│       └── integration/

├── data/                        # SQLite database (runtime)
│   └── etnopapers.db           # Created on first run

├── Dockerfile                   # Multi-stage build
├── docker-compose.yml          # Dev environment
├── .dockerignore
├── requirements.txt            # Python dependencies
├── .gitignore
├── CLAUDE.md                   # Project guidance for Claude Code
└── README.md                   # Portuguese setup guide
```

## Rastreamento de Complexidade

**Status**: ✅ **Sem violações da Constituição**

Todas as decisões de design estão alinhadas com os 6 princípios e restrições da constituição. Nenhuma complexidade adicional necessária ou justificada.

---

## Roadmap de Implementação em Fases

### Fase 0: Pesquisa e Decisões Técnicas (CONCLUÍDA)

**Objetivo**: Resolver todas as incertezas técnicas e documentar decisões com rationale.

**Tarefas Concluídas**:
- [x] Análise pdf.js vs. Tesseract.js (pdf.js para texto nativo, Tesseract.js para OCR)
- [x] Avaliação CORS de provedores IA (Claude > Gemini > ChatGPT para browser direto)
- [x] SQLite WAL optimization research
- [x] GBIF + Tropicos caching patterns
- [x] Docker multi-stage build optimization
- [x] Constituição do projeto criada

**Saída**: `research.md` (já incluído via best-practices-research.md)

---

### Fase 1: Design & Contratos (PRÓXIMA)

**Objetivo**: Gerar modelos de dados, contratos API, e guias de início rápido.

**Tarefas de Design**:
1. **data-model.md**:
   - Diagrama ER (12 tabelas)
   - Schema SQL com validações e triggers
   - Relacionamentos e constraints
   - Índices de performance

2. **contracts/api-rest.yaml**:
   - 29 endpoints REST especificados em OpenAPI 3.0
   - Request/response schemas
   - Códigos de erro esperados
   - Exemplos de uso

3. **contracts/ai-integration.md**:
   - Padrões Gemini, ChatGPT, Claude
   - Prompts JSON estruturado
   - Tratamento de erros e retry logic
   - Validação de respostas

4. **quickstart.md**:
   - Setup dev: Node.js + Python venvs
   - Setup prod: Docker Compose
   - Variáveis de ambiente
   - Testes manuais basic

**Saída**: data-model.md, /contracts/*, quickstart.md

---

### Fase 2: Implementação por Sprints (APÓS PHASE 1)

**Sprint 0 (Setup - 2 semanas)**:
- TASK-001: Estrutura diretórios
- TASK-002: Docker Compose
- TASK-003: Schema SQLite

**Sprint 1 (Backend Core - 2 semanas)**:
- TASK-004: FastAPI base
- TASK-005: Endpoints CRUD
- TASK-006: Detecção duplicatas
- TASK-007: Validação taxonômica

**Sprint 2 (Frontend Core - 2 semanas)**:
- TASK-008: React + TypeScript setup
- TASK-009: Zustand store
- TASK-010: Configuração API key
- TASK-011: Upload PDF
- TASK-012: Extração texto PDF

**Sprint 3 (IA Integration - 2 semanas)**:
- TASK-013: Gemini
- TASK-014: ChatGPT
- TASK-015: Claude
- TASK-016: Exibição metadados
- TASK-017: Edição manual

**Sprint 4 (Interface - 2 semanas)**:
- TASK-018: Auto-save rascunhos
- TASK-019: Tabela artigos
- TASK-020: Download banco

**Sprint 5+ (P2/P3 Melhorias)**:
- TASK-021-029: Validações, PDFs escaneados, endpoints adicionais, etc.

**Saída**: tasks.md com todos os 33 TASKs linkados a RF-xxxxx

---

### Fase 3: Testes e Deploy (APÓS SPRINTS)

**Objectives**:
- Coverage > 80% (backend), E2E testes (frontend)
- Documentação em português 100% completa
- Compatibilidade UNRAID validada

**Saída**: Sistema pronto para produção em container Docker

---

## Próximos Passos

1. ✅ **Constitution criada** → Governa todas as decisões futuras
2. ⏭️ **Phase 1 Design** → Gerar data-model.md, contracts/, quickstart.md
3. ⏭️ **Phase 2 Tasks** → Executar `/speckit.tasks` para gerar tasks.md com 33 tarefas
4. ⏭️ **Implementation** → Começar Sprint 0 com setup base
5. ⏭️ **Testing & Deployment** → Validar com test data, deploy em UNRAID
