# Tarefas de Implementação: Etnopapers

**Funcionalidade**: Sistema de Extração de Metadados de Artigos Etnobotânicos
**Branch**: main
**Criado**: 2025-11-20
**Status**: Planejamento Concluído

## Visão Geral

Este documento contém todas as tarefas necessárias para implementar o sistema Etnopapers, organizadas por fases e priorizadas conforme as histórias de usuário (P1, P2, P3).

**Metodologia de Estimativa**:
- **Pontos**: Complexidade relativa (1, 2, 3, 5, 8, 13)
- **1 ponto** ≈ 2-4 horas de desenvolvimento
- **Total estimado**: 89 pontos ≈ 178-356 horas

---

## Fase 0: Setup e Infraestrutura

### TASK-001: Configurar Estrutura do Projeto

**Prioridade**: P0 (Crítica)
**Pontos**: 2
**Status**: Pendente

**Descrição**:
Criar estrutura de diretórios e arquivos de configuração base para frontend e backend.

**Critérios de Aceitação**:
- [ ] Estrutura de diretórios criada: `/frontend`, `/backend`, `/data`
- [ ] `package.json` do frontend configurado com React + TypeScript
- [ ] `requirements.txt` do backend configurado com FastAPI
- [ ] `.gitignore` configurado (node_modules, __pycache__, *.db, .env)
- [ ] `README.md` na raiz com instruções básicas
- [ ] `.env.example` com variáveis de ambiente documentadas

**Arquivos Criados**:
```
/frontend/package.json
/frontend/tsconfig.json
/backend/requirements.txt
/backend/main.py (esqueleto)
/.gitignore
/.env.example
```

**Dependências**: Nenhuma

---

### TASK-002: Configurar Docker e Docker Compose

**Prioridade**: P0 (Crítica)
**Pontos**: 3
**Status**: Pendente

**Descrição**:
Criar Dockerfile multi-stage e docker-compose.yml para ambiente de desenvolvimento e produção.

**Critérios de Aceitação**:
- [ ] Dockerfile multi-stage (frontend builder + backend runtime)
- [ ] Imagem base Alpine Linux
- [ ] docker-compose.yml funcional
- [ ] Volume `/data` mapeado para persistência SQLite
- [ ] Variáveis de ambiente configuradas
- [ ] `docker-compose up` inicia sistema com sucesso
- [ ] Acesso via http://localhost:8000 funcionando

**Arquivos Criados**:
```
/Dockerfile
/docker-compose.yml
/.dockerignore
```

**Dependências**: TASK-001

**Referência**: specs/main/research.md:170-217

---

### TASK-003: Criar Schema do Banco SQLite

**Prioridade**: P0 (Crítica)
**Pontos**: 5
**Status**: Pendente

**Descrição**:
Implementar schema completo do banco SQLite com 8 tabelas, triggers e índices.

**Critérios de Aceitação**:
- [ ] Script SQL com todas as 8 tabelas criado
- [ ] Triggers para auditoria implementados (atualizar_modificacao_artigo, marcar_edicao_manual)
- [ ] Todos os índices criados (18 índices incluindo idx_artigos_duplicatas)
- [ ] View vw_artigos_completos criada
- [ ] Script de migração com Alembic configurado
- [ ] Testes de integridade referencial passando

**Arquivos Criados**:
```
/backend/database/schema.sql
/backend/database/migrations/001_initial_schema.py
/backend/database/init_db.py
```

**Dependências**: TASK-002

**Referência**: specs/main/data-model.md:56-520

---

## Fase 1: MVP - Funcionalidades Essenciais (P1)

### TASK-004: Implementar Backend FastAPI Base

**Prioridade**: P1
**Pontos**: 3
**Status**: Pendente

**Descrição**:
Criar aplicação FastAPI básica com conexão SQLite, CORS e estrutura de rotas.

**Critérios de Aceitação**:
- [ ] Aplicação FastAPI inicializada
- [ ] Conexão com SQLite funcionando
- [ ] CORS configurado para aceitar requests do frontend
- [ ] Endpoint `/health` retornando status
- [ ] SQLite em modo WAL (Write-Ahead Logging)
- [ ] Logging configurado
- [ ] Pydantic models para validação criados

**Arquivos Criados**:
```
/backend/main.py
/backend/database/connection.py
/backend/models/article.py
/backend/config.py
```

**Dependências**: TASK-003

**Referência**: specs/main/research.md:52-76

---

### TASK-005: Implementar Endpoints CRUD de Artigos

**Prioridade**: P1
**Pontos**: 5
**Status**: Pendente

**Descrição**:
Criar endpoints REST para criar, ler, atualizar e deletar artigos científicos.

**Critérios de Aceitação**:
- [ ] POST /api/articles (criar artigo)
- [ ] GET /api/articles (listar com paginação)
- [ ] GET /api/articles/{id} (obter artigo específico)
- [ ] PUT /api/articles/{id} (atualizar artigo)
- [ ] DELETE /api/articles/{id} (excluir artigo)
- [ ] Validação de dados com Pydantic
- [ ] Retorno de erros apropriados (400, 404, 422)
- [ ] Testes unitários com pytest

**Arquivos Criados**:
```
/backend/routers/articles.py
/backend/services/article_service.py
/backend/tests/test_articles.py
```

**Dependências**: TASK-004

**Referência**: specs/main/contracts/api-rest.yaml:51-235

---

### TASK-006: Implementar Detecção de Duplicatas

**Prioridade**: P1
**Pontos**: 5
**Status**: Pendente

**Descrição**:
Implementar lógica de detecção de artigos duplicados usando DOI ou título+ano+autor.

**Critérios de Aceitação**:
- [ ] Função de verificação de duplicatas implementada
- [ ] Verificação por DOI (primária) funcional
- [ ] Verificação por título+ano+autor (secundária) funcional
- [ ] Query otimizada com índice idx_artigos_duplicatas
- [ ] Endpoint retorna informações do artigo duplicado encontrado
- [ ] Testes com casos de duplicatas reais
- [ ] Testes com casos de não-duplicatas (falsos positivos)

**Arquivos Criados**:
```
/backend/services/duplicate_checker.py
/backend/tests/test_duplicates.py
```

**Dependências**: TASK-005

**Referência**: specs/main/spec.md:163-169, specs/main/data-model.md:124-156

---

### TASK-007: Implementar Validação Taxonômica (GBIF/Tropicos)

**Prioridade**: P1
**Pontos**: 5
**Status**: Pendente

**Descrição**:
Integrar APIs de taxonomia botânica (GBIF primária, Tropicos fallback) para validar nomes científicos.

**Critérios de Aceitação**:
- [ ] Cliente HTTP para GBIF Species API implementado
- [ ] Cliente HTTP para Tropicos API implementado (fallback)
- [ ] Lógica de fallback funcional (GBIF → Tropicos)
- [ ] Cache em memória implementado (dict Python)
- [ ] Persistência do cache em JSON ao shutdown
- [ ] Timeout de 5 segundos configurado
- [ ] Tratamento de APIs offline (marca como "não validado")
- [ ] Testes com espécies reais

**Arquivos Criados**:
```
/backend/services/taxonomy_service.py
/backend/clients/gbif_client.py
/backend/clients/tropicos_client.py
/backend/tests/test_taxonomy.py
```

**Dependências**: TASK-005

**Referência**: specs/main/spec.md:144-147, specs/main/research.md:157-175

---

### TASK-008: Configurar Frontend React + TypeScript

**Prioridade**: P1
**Pontos**: 3
**Status**: Pendente

**Descrição**:
Inicializar aplicação React com TypeScript, configurar roteamento e estrutura de componentes.

**Critérios de Aceitação**:
- [ ] Aplicação React criada com Vite
- [ ] TypeScript configurado
- [ ] React Router configurado
- [ ] Estrutura de pastas organizada (components, pages, services, types)
- [ ] ESLint e Prettier configurados
- [ ] Tema básico/CSS global definido
- [ ] Hot reload funcionando

**Arquivos Criados**:
```
/frontend/src/App.tsx
/frontend/src/main.tsx
/frontend/src/router.tsx
/frontend/vite.config.ts
/frontend/.eslintrc.json
```

**Dependências**: TASK-001

**Referência**: specs/main/research.md:14-50

---

### TASK-009: Implementar Gestão de Estado (Zustand)

**Prioridade**: P1
**Pontos**: 3
**Status**: Pendente

**Descrição**:
Criar store Zustand para gerenciar estado global da aplicação (API keys, metadados, drafts).

**Critérios de Aceitação**:
- [ ] Store Zustand criado com interface AppState
- [ ] State para API keys (provider, key, isValid)
- [ ] State para upload e processamento
- [ ] State para metadados extraídos e editados
- [ ] State para rascunhos
- [ ] Actions para todas as operações
- [ ] TypeScript types completos
- [ ] Persistência no localStorage (API keys)

**Arquivos Criados**:
```
/frontend/src/store/useStore.ts
/frontend/src/types/state.ts
```

**Dependências**: TASK-008

**Referência**: specs/main/research.md:291-319

---

### TASK-010: Implementar Configuração de API Key

**Prioridade**: P1
**Pontos**: 3
**Status**: Pendente

**Descrição**:
Criar interface para usuário selecionar provedor de IA e configurar API key.

**Critérios de Aceitação**:
- [ ] Componente de seleção de provedor (Gemini/ChatGPT/Claude)
- [ ] Input de API key com validação
- [ ] Botão "Validar e Salvar"
- [ ] Validação assíncrona da chave (chamada de teste)
- [ ] Feedback visual (loading, sucesso, erro)
- [ ] Armazenamento no localStorage
- [ ] Modal com instruções de como obter chave
- [ ] Links para páginas de criação de chave

**Arquivos Criados**:
```
/frontend/src/components/ApiKeySetup.tsx
/frontend/src/services/apiKeyValidator.ts
/frontend/src/components/ProviderSelector.tsx
```

**Dependências**: TASK-009

**Referência**: specs/main/spec.md:31-33, specs/main/contracts/ai-integration.md:151-227

---

### TASK-011: Implementar Upload de PDF

**Prioridade**: P1
**Pontos**: 3
**Status**: Pendente

**Descrição**:
Criar componente de upload de PDF com drag-and-drop e validações.

**Critérios de Aceitação**:
- [ ] Área de drag-and-drop com react-dropzone
- [ ] Botão "Selecionar PDF"
- [ ] Validação de formato (.pdf apenas)
- [ ] Validação de tamanho (máximo 50 MB)
- [ ] Preview do arquivo selecionado
- [ ] Barra de progresso durante processamento
- [ ] Mensagens de erro claras
- [ ] Cancelamento de upload

**Arquivos Criados**:
```
/frontend/src/components/PdfUpload.tsx
/frontend/src/utils/fileValidation.ts
```

**Dependências**: TASK-010

**Referência**: specs/main/spec.md:34

---

### TASK-012: Implementar Extração de Texto do PDF (pdf.js)

**Prioridade**: P1
**Pontos**: 5
**Status**: Pendente

**Descrição**:
Usar pdf.js para extrair texto do PDF no navegador antes de enviar para API de IA.

**Critérios de Aceitação**:
- [ ] pdf.js integrado e configurado
- [ ] Worker do pdf.js configurado
- [ ] Função extractTextFromPDF implementada
- [ ] Extração página por página funcionando
- [ ] Tratamento de PDFs corrompidos
- [ ] Tratamento de PDFs escaneados (aviso de qualidade)
- [ ] Progress callback para barra de progresso
- [ ] Testes com PDFs reais (texto e escaneados)

**Arquivos Criados**:
```
/frontend/src/services/pdfExtractor.ts
/frontend/public/pdf.worker.js
```

**Dependências**: TASK-011

**Referência**: specs/main/contracts/ai-integration.md:571-591

---

### TASK-013: Implementar Integração com Google Gemini

**Prioridade**: P1
**Pontos**: 5
**Status**: Pendente

**Descrição**:
Integrar Google Gemini API para extração de metadados do texto do PDF.

**Critérios de Aceitação**:
- [ ] Cliente HTTP para Gemini API implementado
- [ ] Prompt template estruturado em português
- [ ] Função extractWithGemini funcionando
- [ ] Parsing de JSON da resposta
- [ ] Tratamento de erros (401, 429, 500)
- [ ] Retry logic com backoff exponencial
- [ ] Timeout de 60 segundos
- [ ] Validação do JSON retornado

**Arquivos Criados**:
```
/frontend/src/services/ai/geminiClient.ts
/frontend/src/services/ai/promptBuilder.ts
/frontend/src/utils/metadataParser.ts
```

**Dependências**: TASK-012

**Referência**: specs/main/contracts/ai-integration.md:19-105

---

### TASK-014: Implementar Integração com OpenAI ChatGPT

**Prioridade**: P1
**Pontos**: 3
**Status**: Pendente

**Descrição**:
Integrar OpenAI ChatGPT API como opção alternativa de IA.

**Critérios de Aceitação**:
- [ ] Cliente HTTP para OpenAI API implementado
- [ ] Sistema de mensagens (system + user) configurado
- [ ] Função extractWithChatGPT funcionando
- [ ] Parsing de JSON da resposta
- [ ] Compatível com mesmo prompt template do Gemini
- [ ] Tratamento de erros OpenAI-specific
- [ ] Testes com gpt-3.5-turbo

**Arquivos Criados**:
```
/frontend/src/services/ai/openaiClient.ts
```

**Dependências**: TASK-013

**Referência**: specs/main/contracts/ai-integration.md:107-179

---

### TASK-015: Implementar Integração com Anthropic Claude

**Prioridade**: P1
**Pontos**: 3
**Status**: Pendente

**Descrição**:
Integrar Anthropic Claude API como terceira opção de IA.

**Critérios de Aceitação**:
- [ ] Cliente HTTP para Claude API implementado
- [ ] Sistema de mensagens + system prompt configurado
- [ ] Função extractWithClaude funcionando
- [ ] Header anthropic-version configurado
- [ ] Compatível com mesmo prompt template
- [ ] Testes com claude-3-haiku-20240307

**Arquivos Criados**:
```
/frontend/src/services/ai/claudeClient.ts
```

**Dependências**: TASK-013

**Referência**: specs/main/contracts/ai-integration.md:181-253

---

### TASK-016: Implementar Exibição de Metadados Extraídos

**Prioridade**: P1
**Pontos**: 5
**Status**: Pendente

**Descrição**:
Criar interface para exibir metadados extraídos pela IA com opções "Salvar", "Editar", "Descartar".

**Critérios de Aceitação**:
- [ ] Componente de exibição de metadados estruturado
- [ ] Campos agrupados por categoria (bibliográficos, geográficos, botânicos, metodológicos)
- [ ] Indicação visual de campos não extraídos (⚠️)
- [ ] Botão "Salvar" (finaliza no BD)
- [ ] Botão "Editar" (abre interface de edição)
- [ ] Botão "Descartar" (com confirmação)
- [ ] Exibição de lista de espécies de plantas
- [ ] Exibição de regiões e comunidades

**Arquivos Criados**:
```
/frontend/src/components/MetadataDisplay.tsx
/frontend/src/components/FieldGroup.tsx
```

**Dependências**: TASK-015

**Referência**: specs/main/spec.md:35-39

---

### TASK-017: Implementar Interface de Edição Manual

**Prioridade**: P1
**Pontos**: 8
**Status**: Pendente

**Descrição**:
Criar formulário completo para edição manual de todos os metadados extraídos.

**Critérios de Aceitação**:
- [ ] Formulário com react-hook-form implementado
- [ ] Todos os campos editáveis (texto, número, data, arrays)
- [ ] Validações de campo (ano 1900-2100, formato DOI)
- [ ] Campos de lista dinâmica (autores, espécies)
- [ ] Botões "Adicionar" e "Remover" para listas
- [ ] Auto-save com debounce de 2 segundos
- [ ] Indicação visual de campos editados (📝)
- [ ] Botão "Salvar Alterações"
- [ ] Botão "Cancelar"

**Arquivos Criados**:
```
/frontend/src/components/MetadataEditor.tsx
/frontend/src/components/forms/DynamicListField.tsx
/frontend/src/hooks/useAutoSave.ts
```

**Dependências**: TASK-016

**Referência**: specs/main/spec.md:69-73

---

### TASK-018: Implementar Auto-Save de Rascunhos

**Prioridade**: P1
**Pontos**: 3
**Status**: Pendente

**Descrição**:
Implementar salvamento automático como rascunho quando usuário fecha janela sem salvar.

**Critérios de Aceitação**:
- [ ] Listener `beforeunload` configurado
- [ ] POST para backend com status "rascunho"
- [ ] Dados salvos no BD automaticamente
- [ ] Rascunho recuperável na próxima sessão
- [ ] Timeout de 7 dias para limpeza automática
- [ ] Seção "Rascunhos Pendentes" na interface
- [ ] Botão "Finalizar Rascunho"

**Arquivos Criados**:
```
/frontend/src/hooks/useAutoSaveDraft.ts
/frontend/src/components/DraftsList.tsx
```

**Dependências**: TASK-017

**Referência**: specs/main/spec.md:40, 161-162

---

### TASK-019: Implementar Tabela de Artigos com TanStack Table

**Prioridade**: P1
**Pontos**: 8
**Status**: Pendente

**Descrição**:
Criar tabela de artigos processados com ordenação, filtros e paginação usando TanStack Table.

**Critérios de Aceitação**:
- [ ] Tabela com 6 colunas implementada (título, ano, autores, status, data, espécies)
- [ ] Ordenação por coluna (crescente/decrescente)
- [ ] Filtro global com busca em tempo real
- [ ] Debounce de 300ms no filtro
- [ ] Paginação client-side (50 itens/página)
- [ ] Navegação de páginas (primeira, anterior, próxima, última)
- [ ] Indicador "Mostrando X-Y de Z artigos"
- [ ] Mensagem quando vazio: "Nenhum artigo processado ainda"
- [ ] Clique em linha abre detalhes do artigo
- [ ] Performance < 2s para 1000 artigos

**Arquivos Criados**:
```
/frontend/src/components/ArticlesTable.tsx
/frontend/src/hooks/useArticlesTable.ts
/frontend/src/types/article.ts
```

**Dependências**: TASK-009

**Referência**: specs/main/spec.md:170-179, specs/main/research.md:348-415

---

### TASK-020: Implementar Download do Banco de Dados

**Prioridade**: P1
**Pontos**: 5
**Status**: Pendente

**Descrição**:
Implementar endpoint backend e botão frontend para download completo do arquivo SQLite.

**Critérios de Aceitação**:
- [ ] Endpoint GET /api/database/download implementado
- [ ] PRAGMA integrity_check antes de envio
- [ ] FileResponse com streaming
- [ ] Nome do arquivo: etnopapers_YYYYMMDD.db
- [ ] Headers Content-Disposition configurados
- [ ] Botão "Download Base de Dados" no frontend
- [ ] Download via fetch + blob + createElement('a')
- [ ] Feedback visual (toast/notificação)
- [ ] Tratamento de erros (500 se integridade falhar)
- [ ] Endpoint GET /api/database/info com estatísticas

**Arquivos Criados**:
```
/backend/routers/database.py
/frontend/src/components/DatabaseDownload.tsx
/frontend/src/services/databaseService.ts
```

**Dependências**: TASK-005

**Referência**: specs/main/spec.md:175-179, specs/main/research.md:417-511

---

## Fase 2: Melhorias e Validações (P2)

### TASK-021: Implementar Validação de Uploads

**Prioridade**: P2
**Pontos**: 3
**Status**: Pendente

**Descrição**:
Adicionar validações robustas para arquivos não-PDF, PDFs corrompidos e mensagens de erro claras.

**Critérios de Aceitação**:
- [ ] Rejeição de arquivos não-PDF com mensagem clara
- [ ] Detecção de PDFs corrompidos
- [ ] Mensagem de erro em português
- [ ] Sugestões de solução nos erros
- [ ] Validação de tamanho (50 MB) com mensagem clara
- [ ] Testes com diversos formatos inválidos

**Arquivos Criados/Modificados**:
```
/frontend/src/utils/fileValidation.ts (modificar)
/frontend/src/components/ErrorMessage.tsx (criar)
```

**Dependências**: TASK-011

**Referência**: specs/main/spec.md:54-56

---

### TASK-022: Implementar Detecção de PDFs Escaneados

**Prioridade**: P2
**Pontos**: 3
**Status**: Pendente

**Descrição**:
Detectar PDFs escaneados (imagem) e exibir aviso de qualidade reduzida.

**Critérios de Aceitação**:
- [ ] Heurística para detectar PDFs escaneados (baixo ratio de texto)
- [ ] Aviso visual: "PDF escaneado detectado - qualidade de extração pode estar reduzida"
- [ ] Redirecionamento automático para interface de edição após extração
- [ ] Recomendação de revisão manual
- [ ] Testes com PDFs escaneados reais

**Arquivos Criados/Modificados**:
```
/frontend/src/utils/pdfQualityDetector.ts (criar)
/frontend/src/services/pdfExtractor.ts (modificar)
```

**Dependências**: TASK-012

**Referência**: specs/main/spec.md:131-132

---

### TASK-023: Implementar Interface de Detecção de Duplicatas

**Prioridade**: P2
**Pontos**: 5
**Status**: Pendente

**Descrição**:
Criar modal/interface para exibir duplicata detectada e permitir escolha (Descartar/Sobrescrever).

**Critérios de Aceitação**:
- [ ] Modal de duplicata com informações do artigo existente
- [ ] Exibir: título, data de processamento, status
- [ ] Botão "Descartar" (fecha modal, descarta novos dados)
- [ ] Botão "Sobrescrever" (substitui registro existente)
- [ ] Confirmação antes de sobrescrever
- [ ] Integração com detecção backend (TASK-006)
- [ ] Mensagem clara e amigável

**Arquivos Criados**:
```
/frontend/src/components/DuplicateModal.tsx
```

**Dependências**: TASK-006, TASK-016

**Referência**: specs/main/spec.md:57-61, 166-169

---

### TASK-024: Implementar Endpoints de Espécies

**Prioridade**: P2
**Pontos**: 3
**Status**: Pendente

**Descrição**:
Criar endpoints REST para consulta de espécies de plantas.

**Critérios de Aceitação**:
- [ ] GET /api/species (listar com filtros)
- [ ] GET /api/species/{id} (obter espécie específica com artigos)
- [ ] Filtros: família, status de validação, busca por nome
- [ ] Paginação implementada
- [ ] Response inclui artigos que mencionam a espécie

**Arquivos Criados**:
```
/backend/routers/species.py
/backend/services/species_service.py
```

**Dependências**: TASK-005

**Referência**: specs/main/contracts/api-rest.yaml:271-323

---

### TASK-025: Implementar Endpoints de Regiões e Comunidades

**Prioridade**: P2
**Pontos**: 3
**Status**: Pendente

**Descrição**:
Criar endpoints REST para consulta de regiões de estudo e comunidades tradicionais.

**Critérios de Aceitação**:
- [ ] GET /api/regions (listar regiões)
- [ ] GET /api/regions/{id} (obter região com artigos e comunidades)
- [ ] GET /api/communities (listar comunidades)
- [ ] GET /api/communities/{id} (obter comunidade com artigos)
- [ ] Filtros por país, estado, tipo de comunidade

**Arquivos Criados**:
```
/backend/routers/regions.py
/backend/routers/communities.py
/backend/services/region_service.py
/backend/services/community_service.py
```

**Dependências**: TASK-005

**Referência**: specs/main/contracts/api-rest.yaml:371-465

---

## Fase 3: Funcionalidades Avançadas (P3)

### TASK-026: Implementar Histórico e Detalhes de Artigos

**Prioridade**: P3
**Pontos**: 5
**Status**: Pendente

**Descrição**:
Criar página de histórico com filtros avançados e visualização detalhada de artigos.

**Critérios de Aceitação**:
- [ ] Página /history com lista de artigos
- [ ] Filtros: ano (slider), status, busca textual
- [ ] Modal de detalhes ao clicar em artigo
- [ ] Exibição completa de metadados
- [ ] Botões "Editar" e "Excluir" com confirmação
- [ ] Navegação de volta para lista

**Arquivos Criados**:
```
/frontend/src/pages/History.tsx
/frontend/src/components/ArticleDetails.tsx
/frontend/src/components/FilterPanel.tsx
```

**Dependências**: TASK-019

**Referência**: specs/main/spec.md:85-88

---

### TASK-027: Implementar Página de Rascunhos

**Prioridade**: P3
**Pontos**: 3
**Status**: Pendente

**Descrição**:
Criar página dedicada para gerenciar rascunhos pendentes.

**Critérios de Aceitação**:
- [ ] Página /drafts com lista de rascunhos
- [ ] Indicação de tempo desde criação
- [ ] Botão "Finalizar" para cada rascunho
- [ ] Botão "Limpar Rascunhos Antigos" (>7 dias)
- [ ] Contador de rascunhos pendentes

**Arquivos Criados**:
```
/frontend/src/pages/Drafts.tsx
```

**Dependências**: TASK-018

**Referência**: specs/main/quickstart.md:589-605

---

### TASK-028: Implementar Configurações e Gerenciamento de API Keys

**Prioridade**: P3
**Pontos**: 3
**Status**: Pendente

**Descrição**:
Criar página de configurações para gerenciar API keys e preferências.

**Critérios de Aceitação**:
- [ ] Página /settings
- [ ] Visualizar chave armazenada (mascarada)
- [ ] Trocar de provedor de IA
- [ ] Atualizar chave de API
- [ ] Remover chave do localStorage
- [ ] Botão "Testar Chave"
- [ ] Seção "Info do Banco" com estatísticas

**Arquivos Criados**:
```
/frontend/src/pages/Settings.tsx
/frontend/src/components/ApiKeyManager.tsx
/frontend/src/components/DatabaseInfo.tsx
```

**Dependências**: TASK-010

**Referência**: specs/main/quickstart.md:639-671

---

### TASK-029: Implementar Exportação para CSV

**Prioridade**: P3
**Pontos**: 3
**Status**: Pendente

**Descrição**:
Adicionar funcionalidade de exportar dados para CSV (artigos, espécies, regiões, comunidades).

**Critérios de Aceitação**:
- [ ] Botão "Exportar para CSV" no histórico
- [ ] Seleção de tabela a exportar
- [ ] Geração de CSV no backend
- [ ] Download automático
- [ ] Headers em português
- [ ] Encoding UTF-8 com BOM (compatibilidade Excel)

**Arquivos Criados**:
```
/backend/routers/export.py
/backend/services/csv_exporter.py
/frontend/src/components/ExportButton.tsx
```

**Dependências**: TASK-024, TASK-025

---

## Fase 4: Testes, Documentação e Deploy

### TASK-030: Implementar Testes de Integração

**Prioridade**: P2
**Pontos**: 8
**Status**: Pendente

**Descrição**:
Criar suite completa de testes de integração para backend e frontend.

**Critérios de Aceitação**:
- [ ] Testes de integração backend com pytest
- [ ] Testes E2E frontend com Playwright/Cypress
- [ ] Testes de fluxo completo (upload → extração → salvamento)
- [ ] Testes de detecção de duplicatas
- [ ] Testes de validação taxonômica
- [ ] Coverage > 80%
- [ ] CI/CD configurado (GitHub Actions)

**Arquivos Criados**:
```
/backend/tests/integration/
/frontend/tests/e2e/
/.github/workflows/ci.yml
```

**Dependências**: Todas as tarefas P1

---

### TASK-031: Documentar API com Swagger/OpenAPI

**Prioridade**: P3
**Pontos**: 2
**Status**: Pendente

**Descrição**:
Garantir que documentação automática do FastAPI está completa e acessível.

**Critérios de Aceitação**:
- [ ] Todos os endpoints documentados com docstrings
- [ ] Swagger UI acessível em /docs
- [ ] ReDoc acessível em /redoc
- [ ] Exemplos de request/response
- [ ] Descrições em português

**Arquivos Modificados**:
```
/backend/routers/*.py (adicionar docstrings)
```

**Dependências**: TASK-005, TASK-024, TASK-025

---

### TASK-032: Otimizar Performance Frontend

**Prioridade**: P3
**Pontos**: 5
**Status**: Pendente

**Descrição**:
Implementar otimizações de performance para tabela e interface.

**Critérios de Aceitação**:
- [ ] Code splitting com lazy loading
- [ ] Memoização de componentes com React.memo
- [ ] Debounce em filtros (implementado)
- [ ] Virtualização de listas se necessário
- [ ] Bundle size < 500 KB
- [ ] Lighthouse score > 90

**Arquivos Modificados**:
```
/frontend/src/router.tsx (lazy loading)
/frontend/src/components/*.tsx (React.memo)
```

**Dependências**: TASK-019

**Referência**: specs/main/research.md:329-346

---

### TASK-033: Preparar para Produção

**Prioridade**: P0 (antes de deploy)
**Pontos**: 5
**Status**: Pendente

**Descrição**:
Configurar variáveis de ambiente, segurança e preparar para deploy.

**Critérios de Aceitação**:
- [ ] Variáveis de ambiente em produção configuradas
- [ ] HTTPS configurado
- [ ] Rate limiting implementado
- [ ] Logs estruturados
- [ ] Backup automático do SQLite configurado
- [ ] Documentação de deploy atualizada
- [ ] Health checks configurados

**Arquivos Criados/Modificados**:
```
/backend/middleware/rate_limiter.py
/backend/config.py (prod settings)
/docs/deployment.md
```

**Dependências**: Todas as tarefas P1

---

## Resumo de Estimativas

### Por Prioridade

| Prioridade | Tarefas | Pontos | Horas Estimadas |
|------------|---------|--------|-----------------|
| P0         | 3       | 10     | 20-40h          |
| P1         | 17      | 76     | 152-304h        |
| P2         | 5       | 17     | 34-68h          |
| P3         | 8       | 31     | 62-124h         |
| **Total**  | **33**  | **134**| **268-536h**    |

### Por Fase

| Fase              | Tarefas | Pontos | Horas Estimadas |
|-------------------|---------|--------|-----------------|
| 0: Setup          | 3       | 10     | 20-40h          |
| 1: MVP (P1)       | 17      | 76     | 152-304h        |
| 2: Melhorias (P2) | 5       | 17     | 34-68h          |
| 3: Avançado (P3)  | 5       | 21     | 42-84h          |
| 4: Testes/Deploy  | 3       | 15     | 30-60h          |
| **Total**         | **33**  | **139**| **278-556h**    |

### Caminho Crítico (MVP)

Para ter um MVP funcional, as seguintes tarefas são críticas:

1. TASK-001 → TASK-002 → TASK-003 (Setup: 10 pontos)
2. TASK-004 → TASK-005 → TASK-006 → TASK-007 (Backend: 18 pontos)
3. TASK-008 → TASK-009 → TASK-010 (Frontend Base: 9 pontos)
4. TASK-011 → TASK-012 → TASK-013 (Upload e IA: 13 pontos)
5. TASK-016 → TASK-017 → TASK-018 (Interface: 16 pontos)
6. TASK-019 → TASK-020 (Tabela e Download: 13 pontos)

**Total MVP**: 79 pontos ≈ 158-316 horas

---

## Ordem Recomendada de Execução

### Sprint 1 (Setup - 2 semanas)
1. TASK-001: Estrutura do projeto
2. TASK-002: Docker
3. TASK-003: Schema SQLite
4. TASK-004: Backend FastAPI base
5. TASK-008: Frontend React base

### Sprint 2 (Backend Core - 2 semanas)
6. TASK-005: Endpoints CRUD
7. TASK-006: Detecção de duplicatas
8. TASK-007: Validação taxonômica
9. TASK-009: Zustand store

### Sprint 3 (Upload e IA - 2 semanas)
10. TASK-010: Configuração API key
11. TASK-011: Upload PDF
12. TASK-012: Extração texto PDF
13. TASK-013: Integração Gemini
14. TASK-014: Integração ChatGPT
15. TASK-015: Integração Claude

### Sprint 4 (Interface - 2 semanas)
16. TASK-016: Exibição metadados
17. TASK-017: Edição manual
18. TASK-018: Auto-save rascunhos
19. TASK-019: Tabela artigos

### Sprint 5 (Melhorias - 1-2 semanas)
20. TASK-020: Download banco
21. TASK-021: Validação uploads
22. TASK-022: PDFs escaneados
23. TASK-023: Interface duplicatas

### Sprint 6+ (Funcionalidades Avançadas)
24-33: Tarefas P2 e P3 conforme necessidade

---

## Notas de Implementação

### Dependências Externas
- APIs de IA: Chaves fornecidas pelo usuário
- GBIF API: Sem autenticação
- Tropicos API: Requer registro gratuito

### Tecnologias Principais
- **Frontend**: React 18, TypeScript 5, TanStack Table 8, Zustand 4
- **Backend**: Python 3.11, FastAPI 0.104, SQLite 3.35+
- **Docker**: Alpine Linux, multi-stage build
- **Testes**: Pytest (backend), Playwright (frontend)

### Considerações de Performance
- Tabela: Paginação client-side (50 itens)
- Filtro: Debounce de 300ms
- Download: Streaming com FileResponse
- Cache: Taxonomia em memória (10K espécies ≈ 5MB)

### Segurança
- API keys: Apenas localStorage (nunca servidor)
- CORS: Configurado para frontend
- Rate limiting: 1 download/minuto por IP
- Validação: Pydantic models no backend

---

## Próximos Passos

1. Revisar estimativas com equipe
2. Priorizar tarefas conforme necessidades
3. Começar pelo Sprint 1 (Setup)
4. Realizar reuniões de retrospectiva ao final de cada sprint
5. Ajustar prioridades conforme feedback do usuário

---

**Documentação gerada em**: 2025-11-20
**Baseado em**: specs/main/spec.md, research.md, data-model.md, contracts/, quickstart.md
