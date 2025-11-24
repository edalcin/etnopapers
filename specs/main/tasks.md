# Tarefas de Implementação: Etnopapers v2.0

**Funcionalidade**: Sistema de Extração de Metadados com AI Local
**Branch**: main
**Versão**: 2.0 (Local AI + MongoDB)
**Criado**: 2025-11-24
**Status**: Pronto para implementação

---

## Resumo Executivo

Este documento lista todas as tarefas necessárias para implementar o Etnopapers v2.0, organizado por fase de desenvolvimento.

**Arquitetura**: Ollama + Qwen2.5-7B-Instruct (GPU) + MongoDB + FastAPI + React
**Total Estimado**: 26 tarefas, ~7-10 dias com 1 dev
**MVP Crítico**: Tarefas 1-16 (infraestrutura + backend + frontend core)

---

## Fase 0: Setup e Infraestrutura

### TASK-001: Configurar Dockerfile para Single Container (Frontend + Backend + Ollama)

**Prioridade**: P0 (Crítica)
**Pontos**: 3
**Status**: Pendente

**Descrição**: Criar Dockerfile que executa frontend React, backend FastAPI e Ollama em UM ÚNICO container, com MongoDB conectado externamente via MONGO_URI.

**Critérios de Aceitação**:
- [ ] **Dockerfile multi-stage** criado:
  - Stage 1: Build frontend React (npm build)
  - Stage 2: Runtime Python com FastAPI
  - Stage 3: Copy built frontend para FastAPI static folder
  - Stage 4: Ollama setup (download model Qwen2.5-7B-Instruct-Q4)
- [ ] FastAPI serve frontend static files (/)
- [ ] Backend API disponível em /api/* routes
- [ ] Ollama disponível localmente no container (localhost:11434)
- [ ] Environment variables configuráveis:
  - `MONGO_URI` (para conectar MongoDB externo)
  - `OLLAMA_MODEL=qwen2.5:7b-instruct-q4_K_M`
  - `OLLAMA_URL=http://localhost:11434` (interno)
- [ ] Healthcheck validando frontend, backend, e Ollama
- [ ] Container startup script que:
  1. Inicia Ollama em background
  2. Aguarda Ollama healthcheck (max 60s)
  3. Inicia FastAPI server
- [ ] Total image size: ~6.8 GB (Ollama + model + dependencies)
- [ ] `docker run` inicia sistema completo (sem docker-compose)

**Arquivos**:
```
/Dockerfile (completely rewritten for single container)
/entrypoint.sh (container startup script)
/docker-compose.yml (simplified: just for local dev with MongoDB)
```

**Dependências**: Nenhuma

---

### TASK-001b: Configurar UNRAID GPU Passthrough

**Prioridade**: P0 (Crítica)
**Pontos**: 2
**Status**: Pendente

**Descrição**: Documentar e validar configuração de GPU NVIDIA no UNRAID para o container Etnopapers.

**Critérios de Aceitação**:
- [ ] **UNRAID UI Configuration** (no container settings):
  - [ ] Extra Parameters: `--runtime=nvidia`
  - [ ] Environment Variable: `NVIDIA_VISIBLE_DEVICES=all`
- [ ] NVIDIA driver instalado no host UNRAID
- [ ] NVIDIA Container Toolkit instalado no UNRAID
- [ ] `docker exec etnopapers nvidia-smi` funciona dentro do container
- [ ] GPU visível com modelo correto (ex: RTX 3060, 12 GB)
- [ ] Documentação em README.md e DEPLOYMENT.md

**Verificação**:
```bash
# No UNRAID host:
docker exec etnopapers nvidia-smi

# Esperado:
# +----------+----------+----------+
# | NVIDIA GPU: RTX 3060 | 12GB VRAM
# +----------+----------+----------+
```

**Arquivos**:
```
/README.md (UNRAID GPU setup section)
/docs/DEPLOYMENT.md (GPU configuration)
```

**Dependências**: TASK-001 (container deve estar rodando)

---

### TASK-002: Criar MongoDB Schema e Índices

**Prioridade**: P0 (Crítica)
**Pontos**: 3
**Status**: Pendente

**Descrição**: Definir schema MongoDB com 4 collections (referencias, especies_plantas, comunidades_indígenas, localizacoes) e 18 índices.

**Critérios de Aceitação**:
- [ ] `backend/database/schema.py` criado com Pydantic models
- [ ] 4 collections definidas com campos completos
- [ ] 18 índices criados (doi unique, status, ano, text search, etc.)
- [ ] Validações implementadas (year ranges, enum types, etc.)
- [ ] Script de inicialização (`backend/database/init_db.py`)
- [ ] Testes de integridade referencial
- [ ] Documentação de estrutura de dados

**Arquivos**:
```
/backend/database/schema.py
/backend/database/init_db.py
/backend/database/indexes.py
```

**Dependências**: TASK-001, TASK-001b (MongoDB precisa estar rodando externamente)

---

### TASK-003: Validar Setup de Infraestrutura

**Prioridade**: P0 (Crítica)
**Pontos**: 2
**Status**: Pendente

**Descrição**: Testar container Etnopapers single-container com Ollama, frontend, backend, e conexão MongoDB.

**Critérios de Aceitação**:
- [ ] `docker build` completa sem erros (image ~6.8 GB)
- [ ] `docker run` com MONGO_URI e NVIDIA_VISIBLE_DEVICES funciona
- [ ] Ollama healthcheck passa dentro do container (curl localhost:11434/api/tags)
- [ ] Model Qwen2.5-7B-Instruct-Q4 carrega com sucesso
- [ ] `docker exec etnopapers nvidia-smi` mostra GPU NVIDIA
- [ ] FastAPI backend disponível em http://localhost:8000
- [ ] Frontend React servido em http://localhost:8000 (index page)
- [ ] Backend API em http://localhost:8000/api/* routes
- [ ] MongoDB conecta via MONGO_URI (testa com curl + auth se necessário)
- [ ] Container healthcheck inclui frontend, backend, e Ollama
- [ ] Container startup script funciona corretamente:
  - Inicia Ollama em background
  - Aguarda Ollama healthcheck (max 60s)
  - Inicia FastAPI server
  - Serve frontend static files

**Arquivos**:
```
/Dockerfile (validation)
/entrypoint.sh (validation)
/tests/integration/container_setup_test.sh
```

**Dependências**: TASK-001, TASK-001b, TASK-002

---

## Fase 1: Backend - Serviço de Extração

### TASK-004: Criar extraction_service.py

**Prioridade**: P1
**Pontos**: 5
**Status**: Pendente

**Descrição**: Implementar serviço de AI local com Ollama + Instructor.

**Critérios de Aceitação**:
- [ ] `backend/services/extraction_service.py` criado
- [ ] Class `OllamaClient` com métodos:
  - `__init__(url, model)`: Inicializa cliente
  - `extract_metadata(pdf_text, researcher_profile=None)`: Extrai metadados
  - `validate_response(response)`: Valida JSON estruturado
- [ ] Pydantic schemas definidos:
  - `SpeciesData`: Vernacular + scientific name
  - `ReferenceMetadata`: Título, autores, ano, espécies, etc.
- [ ] Instructor + OpenAI client configurado para Ollama
- [ ] Error handling para timeouts, invalid JSON, etc.
- [ ] Retry logic com backoff exponencial (max 3 attempts)
- [ ] Temperature=0.1, top_p=0.9 configurados para determinism
- [ ] Testes unitários implementados

**Arquivos**:
```
/backend/services/extraction_service.py
/backend/models/metadata.py
/backend/tests/test_extraction_service.py
```

**Dependências**: TASK-004

---

### TASK-005: Implementar Pydantic Schemas

**Prioridade**: P1
**Pontos**: 3
**Status**: Pendente

**Descrição**: Definir schemas Pydantic para validação de respostas da IA.

**Critérios de Aceitação**:
- [ ] `SpeciesData`: nome vernacular + científico
- [ ] `ReferenceMetadata`: Todos os campos de artigo científico
- [ ] Validações: ano (1900-2030), DOI format, minúsculas em campos de texto
- [ ] Field descriptions em português
- [ ] Examples para cada schema
- [ ] Serialização/desserialização JSON testada

**Arquivos**:
```
/backend/models/metadata.py
/backend/tests/test_metadata_models.py
```

**Dependências**: TASK-005

---

### TASK-006: Criar POST /api/extract/metadata Endpoint

**Prioridade**: P1
**Pontos**: 5
**Status**: Pendente

**Descrição**: Implementar endpoint principal de extração.

**Critérios de Aceitação**:
- [ ] `backend/routers/extraction.py` criado
- [ ] POST `/api/extract/metadata` endpoint:
  - Request: multipart/form-data (pdf_file, researcher_profile)
  - Response: 200 OK com metadados estruturados
  - Errors: 400 (invalid), 422 (extraction failed), 500 (ollama error)
- [ ] File validation (PDF only, < 50 MB)
- [ ] PDF text extraction com pdfplumber
- [ ] Researcher profile parsing (JSON string → dict)
- [ ] Extraction service integration
- [ ] Response format: {metadata, extraction_time_ms, text_length, species_count}
- [ ] Testes integrais com curl + real PDF

**Arquivos**:
```
/backend/routers/extraction.py
/backend/tests/integration/test_extract_metadata.py
```

**Dependências**: TASK-005, TASK-006

---

### TASK-007: PDF Text Extraction com pdfplumber

**Prioridade**: P1
**Pontos**: 3
**Status**: Pendente

**Descrição**: Implementar extração robusta de texto de PDFs.

**Critérios de Aceitação**:
- [ ] `backend/services/pdf_service.py` criado
- [ ] Função `extract_text_from_pdf(file_path)` retorna texto
- [ ] Detecção de PDFs escaneados (confidence score)
- [ ] Aviso de qualidade quando detectado scanned
- [ ] Tratamento de PDFs corrompidos (exception handling)
- [ ] Page merging com preservação de ordem
- [ ] Limite: máximo 50 páginas (12500 caracteres por página)
- [ ] Testes com PDFs reais (text + scanned)

**Arquivos**:
```
/backend/services/pdf_service.py
/backend/tests/test_pdf_service.py
```

**Dependências**: Nenhuma

---

### TASK-008: Error Handling e Logging

**Prioridade**: P1
**Pontos**: 3
**Status**: Pendente

**Descrição**: Implementar error handling robusto e logging estruturado.

**Critérios de Aceitação**:
- [ ] `backend/exceptions.py` com custom exceptions:
  - `InvalidPDFError`: Arquivo não é PDF
  - `PDFTooLargeError`: > 50 MB
  - `PDFCorruptedError`: Não consegue extrair texto
  - `OllamaTimeoutError`: Inferência timeout (> 60s)
  - `ExtractionValidationError`: JSON inválido
- [ ] Logging com estrutura (timestamp, level, message, context)
- [ ] Mensagens de erro em português
- [ ] Sugestões de solução em mensagens de erro
- [ ] Rate limiting (max 1 extraction/second per IP)
- [ ] Request/response logging (não inclui PDFs)

**Arquivos**:
```
/backend/exceptions.py
/backend/middleware/logging.py
/backend/config.py (logging config)
```

**Dependências**: TASK-007

---

### TASK-009: Testes Backend (Unit + Integration)

**Prioridade**: P1
**Pontos**: 5
**Status**: Pendente

**Descrição**: Implementar suite completa de testes para backend.

**Critérios de Aceitação**:
- [ ] Unit tests para extraction_service (>90% coverage)
- [ ] Unit tests para pdf_service
- [ ] Unit tests para Pydantic models
- [ ] Integration tests para /api/extract/metadata
- [ ] Test fixtures: PDFs de exemplo, mock Ollama responses
- [ ] Test dataset: 5 PDFs com ground truth
- [ ] Testes de erro: invalid PDF, timeout, corrupt file
- [ ] Coverage > 80%
- [ ] `pytest` integrado na CI/CD

**Arquivos**:
```
/backend/tests/conftest.py
/backend/tests/unit/test_*.py
/backend/tests/integration/test_*.py
/backend/tests/fixtures/sample_pdfs/
```

**Dependências**: TASK-007, TASK-008, TASK-009

---

## Fase 2: Frontend - Integração

### TASK-010: Remover APIConfiguration Component

**Prioridade**: P2
**Pontos**: 2
**Status**: Pendente

**Descrição**: Eliminar seleção de provedor de IA e configuração de chave (v1.0 apenas).

**Critérios de Aceitação**:
- [ ] Componente `frontend/src/components/APIConfiguration.tsx` removido
- [ ] Referências ao APIConfiguration removidas de Router
- [ ] Zustand store limpo de `apiProvider` e `apiKey` states
- [ ] Nenhuma tela de configuração inicial
- [ ] Usuário acessa diretamente upload page

**Arquivos**:
```
/frontend/src/components/APIConfiguration.tsx (DELETE)
/frontend/src/router.tsx (update)
/frontend/src/store/useStore.ts (clean API key state)
```

**Dependências**: Nenhuma

---

### TASK-011: Remover Gerenciamento de API Keys do Store

**Prioridade**: P2
**Pontos**: 2
**Status**: Pendente

**Descrição**: Limpar Zustand store de estados de API keys.

**Critérios de Aceitação**:
- [ ] `useStore.ts` removidos states:
  - `apiProvider`
  - `apiKey`
  - `isApiKeyValid`
  - `setApiKey`, `setApiProvider`, `validateApiKey`
- [ ] localStorage limpeza de chaves antigas
- [ ] Manter states para:
  - `extractedMetadata`
  - `drafts`
  - `researcherProfile` (opcional)
- [ ] TypeScript types atualizados

**Arquivos**:
```
/frontend/src/store/useStore.ts (refactor)
/frontend/src/types/state.ts (update)
```

**Dependências**: TASK-011

---

### TASK-012: Criar extractionService.ts

**Prioridade**: P2
**Pontos**: 3
**Status**: Pendente

**Descrição**: Implementar serviço frontend para chamar backend `/api/extract/metadata`.

**Critérios de Aceitação**:
- [ ] `frontend/src/services/extractionService.ts` criado
- [ ] Função `extractMetadata(pdfFile, researcherProfile)`:
  - POST multipart/form-data para `/api/extract/metadata`
  - Retorna metadados estruturados
  - Progress callback para barra de progresso
- [ ] Error handling (400, 422, 500 errors)
- [ ] Timeout handling (60s max)
- [ ] Retry logic para transient failures
- [ ] Request/response logging
- [ ] Testes com mock server

**Arquivos**:
```
/frontend/src/services/extractionService.ts
/frontend/src/tests/services/extractionService.test.ts
```

**Dependências**: TASK-007

---

### TASK-013: Atualizar PDFUpload Component

**Prioridade**: P2
**Pontos**: 3
**Status**: Pendente

**Descrição**: Integrar component upload com novo backend extraction.

**Critérios de Aceitação**:
- [ ] `frontend/src/components/PDFUpload.tsx` atualizado
- [ ] Remove chamadas a externa AI APIs (Gemini/ChatGPT/Claude)
- [ ] Chama `extractionService.extractMetadata()` em vez disso
- [ ] Barra de progresso (upload + extraction)
- [ ] Status messages em português
- [ ] Error messages com sugestões (ex: "PDF corrompido. Tente outro arquivo")
- [ ] Cancel upload button
- [ ] Testes com mock extraction service

**Arquivos**:
```
/frontend/src/components/PDFUpload.tsx (update)
/frontend/src/tests/components/PDFUpload.test.tsx
```

**Dependências**: TASK-013

---

### TASK-014: Atualizar Mensagens de Erro (Português)

**Prioridade**: P2
**Pontos**: 2
**Status**: Pendente

**Descrição**: Localizar todas mensagens de erro para português brasileiro.

**Critérios de Aceitação**:
- [ ] Todas mensagens de erro da UI em português
- [ ] Mensagens backend retornam em português
- [ ] Sugestões de solução incluídas (ex: "Arquivo muito grande. Máximo 50 MB")
- [ ] Evitar jargão técnico ou explicar claramente
- [ ] Testes de mensagens críticas

**Arquivos**:
```
/frontend/src/constants/messages.ts (create)
/backend/exceptions.py (update messages)
/frontend/src/services/*.ts (update error handling)
```

**Dependências**: TASK-014

---

### TASK-015: Remover AI Client Files (v1.0)

**Prioridade**: P2
**Pontos**: 1
**Status**: Pendente

**Descrição**: Deletar arquivos legados de AI providers externos.

**Critérios de Aceitação**:
- [ ] `frontend/src/services/ai/geminiClient.ts` removido
- [ ] `frontend/src/services/ai/openaiClient.ts` removido
- [ ] `frontend/src/services/ai/claudeClient.ts` removido
- [ ] `frontend/src/services/ai/promptBuilder.ts` removido
- [ ] Referências removidas de package.json (axios se não usado)
- [ ] Build sem erros

**Arquivos**:
```
/frontend/src/services/ai/ (DELETE directory)
/frontend/package.json (cleanup deps)
```

**Dependências**: TASK-014

---

## Fase 3: Testing & Refinement

### TASK-016: Criar Test Dataset

**Prioridade**: P3
**Pontos**: 3
**Status**: Pendente

**Descrição**: Preparar 20 PDFs com metadata ground truth para validação.

**Critérios de Aceitação**:
- [ ] 20 PDFs coletados (etnobotânica reais)
- [ ] Ground truth JSON para cada PDF:
  - titulo, autores, ano, DOI
  - especies (vernacular + científico)
  - país, estado, município
  - tipo_de_uso, bioma
- [ ] Distribuição de dificuldade:
  - 5 Easy (clara estrutura, texto bem formatado)
  - 10 Medium (algumas ambiguidades)
  - 5 Hard (tabelas complexas, nomes ambíguos)
- [ ] Armazenado em `/backend/tests/fixtures/test_pdfs/`

**Arquivos**:
```
/backend/tests/fixtures/test_pdfs/ (20 PDFs)
/backend/tests/fixtures/ground_truth.json
```

**Dependências**: TASK-010

---

### TASK-017: Medir Acurácia de Extração

**Prioridade**: P3
**Pontos**: 5
**Status**: Pendente

**Descrição**: Avaliar qualidade de extração contra ground truth.

**Critérios de Aceitação**:
- [ ] Script de avaliação: `backend/scripts/evaluate_extraction.py`
- [ ] Métricas por campo:
  - Título: exact match %
  - Autores: nombre completo correto %
  - Ano: exact match %
  - Espécies: name matching (cientfico vs vernacular)
  - Localização: geographic accuracy
- [ ] Relatório HTML com detalhes por PDF
- [ ] Target: > 80% overall accuracy
- [ ] Identificar campos problemáticos para prompt tuning

**Arquivos**:
```
/backend/scripts/evaluate_extraction.py
/backend/results/evaluation_report.html
```

**Dependências**: TASK-017

---

### TASK-018: Performance Benchmarking

**Prioridade**: P3
**Pontos**: 3
**Status**: Pendente

**Descrição**: Medir velocidade de inferência em diferentes GPUs.

**Critérios de Aceitação**:
- [ ] Benchmark script testa 5-10 PDFs em loop
- [ ] Métricas:
  - Tempo de inferência Ollama
  - Tempo de extração PDF
  - Tempo total end-to-end
- [ ] Testes em:
  - RTX 3060 (target GPU)
  - RTX 4070 (ideal)
  - CPU-only (reference)
- [ ] Resultados documentados em performance_benchmark.md
- [ ] Target: < 3s no RTX 3060+

**Arquivos**:
```
/backend/scripts/benchmark_extraction.py
/docs/performance_benchmark.md
```

**Dependências**: TASK-004

---

### TASK-019: Otimização de Prompts

**Prioridade**: P3
**Pontos**: 5
**Status**: Pendente

**Descrição**: Iterar prompts para melhorar qualidade de extração.

**Critérios de Aceitação**:
- [ ] System prompt refinado (campo descriptions, exemplos)
- [ ] User prompt melhorado (estrutura, hints para campos problemáticos)
- [ ] Testes de variações (5+ versões)
- [ ] Medição de impacto (antes/depois acurácia)
- [ ] Documentação das mudanças em `/backend/prompts/`
- [ ] Target: 85%+ accuracy (vs 80% baseline)

**Arquivos**:
```
/backend/prompts/system_prompt.txt
/backend/prompts/user_prompt_template.txt
/backend/tests/test_prompt_variations.py
```

**Dependências**: TASK-018

---

### TASK-020: Testes de Casos Extremos

**Prioridade**: P3
**Pontos**: 3
**Status**: Pendente

**Descrição**: Validar comportamento em cenários desafiadores.

**Critérios de Aceitação**:
- [ ] PDFs escaneados:
  - Detectados corretamente
  - Aviso de qualidade exibido
  - Ainda extraem algum texto
- [ ] PDFs muito longos (50+ páginas):
  - Truncamento inteligente
  - Sem crash ou timeout
- [ ] PDFs corrompidos:
  - Erro claro ao usuário
  - Não causa problema no server
- [ ] Tabelas complexas:
  - Extração ragioável
  - Nomes científicos identificados mesmo em tabelas
- [ ] Múltiplos idiomas:
  - Português, inglês, espanhol todas processadas

**Arquivos**:
```
/backend/tests/test_edge_cases.py
/backend/tests/fixtures/edge_case_pdfs/
```

**Dependências**: TASK-010

---

### TASK-021: Production Readiness Checklist

**Prioridade**: P3
**Pontos**: 3
**Status**: Pendente

**Descrição**: Validação final antes do deploy.

**Critérios de Aceitação**:
- [ ] All P0/P1 tasks completed
- [ ] Test coverage > 80%
- [ ] No security vulnerabilities (no plaintext secrets, SQL injection, etc.)
- [ ] Performance: < 3s extraction on RTX 3060+
- [ ] Accuracy: > 80% on test dataset
- [ ] Error messages: 100% português
- [ ] Docker build: < 5 min, image < 1 GB
- [ ] `docker-compose up` works end-to-end
- [ ] Healthchecks pass (MongoDB, Ollama, API)
- [ ] Database backup/restore tested

**Arquivos**:
```
/docs/PRODUCTION_CHECKLIST.md
```

**Dependências**: Todas tarefas anteriores

---

## Fase 4: Documentação & Deploy

### TASK-022: Atualizar README (GPU, UNRAID, Troubleshooting)

**Prioridade**: P3
**Pontos**: 3
**Status**: Pendente

**Descrição**: Documentação de setup e uso.

**Critérios de Aceitação**:
- [ ] GPU requirements (RTX 3060+, 6-8 GB VRAM)
- [ ] UNRAID installation steps
- [ ] NVIDIA driver & nvidia-docker setup
- [ ] `docker-compose up` instructions
- [ ] Troubleshooting section:
  - GPU não detectada
  - Modelo não carrega
  - Inferência muito lenta
  - Erro de memória (OOM)
- [ ] Quick start example
- [ ] FAQ português

**Arquivos**:
```
/README.md (rewrite)
```

**Dependências**: TASK-004

---

### TASK-023: Criar DEPLOYMENT.md

**Prioridade**: P3
**Pontos**: 2
**Status**: Pendente

**Descrição**: Guia detalhado de deployment em produção.

**Critérios de Aceitação**:
- [ ] docker-compose.yml explicado (cada serviço)
- [ ] Environment variables documentadas
- [ ] Backup strategy (MongoDB export)
- [ ] Update strategy (git pull + docker-compose rebuild)
- [ ] Monitoring (healthchecks, logs)
- [ ] Scaling (single instance vs multi-instance)
- [ ] Security hardening (CORS, rate limits)

**Arquivos**:
```
/docs/DEPLOYMENT.md (new)
```

**Dependências**: TASK-004

---

### TASK-024: Release Notes v2.0

**Prioridade**: P3
**Pontos**: 2
**Status**: Pendente

**Descrição**: Documentar mudanças v1.0 → v2.0.

**Critérios de Aceitação**:
- [ ] O que é novo em v2.0:
  - AI local (Ollama + Qwen2.5)
  - MongoDB (NoSQL)
  - GPU requirement
  - Sem API keys
- [ ] O que mudou (breaking changes)
- [ ] O que foi removido (v1.0 features)
- [ ] Performance improvements
- [ ] Known limitations

**Arquivos**:
```
/docs/RELEASE_NOTES_v2.0.md (new)
```

**Dependências**: TASK-023, TASK-024

---

### TASK-025: Teste Final em UNRAID

**Prioridade**: P3
**Pontos**: 3
**Status**: Pendente

**Descrição**: Validação end-to-end em servidor UNRAID com GPU.

**Critérios de Aceitação**:
- [ ] `docker-compose up` inicia sem erros no UNRAID
- [ ] GPU visível e utilizada (nvidia-smi)
- [ ] MongoDB persiste dados após reboot
- [ ] Ollama model carrega corretamente
- [ ] Upload PDF → Extração → Salvamento funciona end-to-end
- [ ] Metadados salvos corretamente no MongoDB
- [ ] Performance aceitável (< 3s inferência)
- [ ] Mensagens de erro em português aparecem corretamente
- [ ] Teardown/cleanup funciona (docker-compose down -v)

**Arquivos**:
```
/tests/integration/e2e_unraid_test.sh
```

**Dependências**: Todas tarefas P0, P1

---

## Resumo de Estimativas

### Por Fase

| Fase | Tarefas | Pontos | Horas Estimadas |
|------|---------|--------|-----------------|
| **0: Setup** | 4 | 10 | 20-40h |
| **1: Backend** | 6 | 19 | 38-76h |
| **2: Frontend** | 6 | 13 | 26-52h |
| **3: Testing** | 6 | 22 | 44-88h |
| **4: Docs** | 4 | 10 | 20-40h |
| **Total** | **26** | **74** | **148-296h** |

### Caminho Crítico (MVP)

1. TASK-001 → TASK-002 → TASK-003 → TASK-004 (Setup: 10 pontos)
2. TASK-005 → TASK-006 → TASK-007 → TASK-008 → TASK-009 (Backend: 19 pontos)
3. TASK-011 → TASK-013 → TASK-014 (Frontend: 8 pontos)

**MVP Total**: 37 pontos ≈ 74-148 horas (4-7 dias)

---

## Dependências Visuais

```
TASK-001, TASK-002 (paralelo)
  ↓
TASK-003, TASK-004 (paralelo, bloqueia tudo)
  ├→ TASK-005 → TASK-006 → TASK-007 → TASK-010 (backend)
  │                          ↓
  │                        TASK-008, TASK-009 (paralelo)
  │
  ├→ TASK-011 → TASK-012 → TASK-013 → TASK-014 → TASK-016 (frontend, pode start depois TASK-007)
  │                                       ↓
  │                                     TASK-015
  │
  └→ TASK-017 → TASK-018 → TASK-020 (testing, paralelo com frontend)
                   ↓
                 TASK-019 (paralelo)
                 TASK-021 (paralelo)

TASK-010, TASK-016, TASK-021 → TASK-022 (production checklist)
  ↓
TASK-023 → TASK-024 → TASK-025 (docs, paralelo)
  ↓
TASK-026 (final validation)
```

---

## Próximos Passos

1. ✅ Especificação v2.0 (spec.md)
2. ✅ Plan v2.0 (plan.md)
3. ✅ Tasks v2.0 (tasks.md - THIS FILE)
4. ✅ Constitution v2.0 (.specify/memory/constitution.md)
5. ➡️ **Iniciar TASK-001: Update docker-compose.yml**

---

**Documento gerado**: 2025-11-24
**Versão**: 2.0 (Local AI + MongoDB + GPU)
**Status**: Pronto para implementação
**Estimativa total**: 7-10 dias com 1 desenvolvedor
