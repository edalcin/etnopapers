# Plano de Implementação: Migração para AI Local

**Projeto:** Etnopapers - Sistema de Extração de Metadados Etnobotânicos
**Versão:** 2.0 (AI Local)
**Data:** 2025-01-23
**Autor:** Equipe Etnopapers
**Status:** Planejamento

---

## 1. Visão Geral da Migração

### 1.1 Escopo da Mudança

**De (v1.0 - APIs Externas):**
- Frontend gerencia API keys (localStorage)
- Chamadas diretas do navegador para Gemini/ChatGPT/Claude
- Backend sem lógica de AI (apenas CRUD)
- SQLite para persistência
- Docker simples (~180 MB)
- Sem requisitos de GPU

**Para (v2.0 - AI Local):**
- Frontend sem gerenciamento de API keys
- Backend processa PDFs e chama AI local (Ollama)
- Modelo Qwen2.5-7B-Instruct rodando em GPU
- MongoDB para persistência (NoSQL)
- Docker com GPU passthrough (~8.5 GB)
- Requer GPU NVIDIA (6-8 GB VRAM)

### 1.2 Benefícios da Migração

| Aspecto | Antes (APIs Externas) | Depois (AI Local) | Ganho |
|---------|----------------------|-------------------|-------|
| **Privacidade** | Dados enviados para terceiros | Dados nunca saem do servidor | 100% privado |
| **Custo** | Variável (por requisição) | Fixo (investimento inicial GPU) | Economia a longo prazo |
| **Latência** | 2-10s (rede + API) | 1-3s (GPU local) | 2-5x mais rápido |
| **Quota** | Limitado por plano/chave | Ilimitado | Sem restrições |
| **Complexidade** | Gerenciar 3 provedores | 1 modelo único | Mais simples |
| **Offline** | Requer internet | Funciona offline | Independente |
| **UX** | Configurar API key | Upload direto | Sem fricção |

### 1.3 Riscos e Mitigações

| Risco | Impacto | Probabilidade | Mitigação |
|-------|---------|---------------|-----------|
| GPU insuficiente no UNRAID | Alto | Baixo | Verificar specs antes, fallback para Q3 ou modelo menor |
| Qualidade de extração inferior | Médio | Médio | Testes comparativos, ajuste de prompts, fallback para APIs se necessário |
| Tamanho Docker muito grande | Baixo | Baixo | UNRAID moderno suporta facilmente 8.5 GB |
| Complexidade setup GPU | Médio | Médio | Documentação detalhada, scripts automatizados |
| Inferência lenta em CPU | Alto | Baixo | Validar GPU antes de deploy, alertar se caiu para CPU |

---

## 2. Roadmap de Implementação

### Cronograma Geral

```
Fase 0: Infraestrutura        [P0] → 1 dia   (Setup Docker + GPU)
Fase 1: Backend - Extração    [P0] → 2-3 dias (Ollama + Instructor)
Fase 2: Frontend - Integração [P1] → 1-2 dias (Remover API keys, chamar backend)
Fase 3: Testes e Refinamento  [P1] → 2-3 dias (Validação, prompt tuning)
Fase 4: Documentação e Deploy [P2] → 1 dia   (Docs + deploy produção)
────────────────────────────────────────────
TOTAL:                               7-10 dias (1 desenvolvedor full-time)
```

### Dependências Entre Fases

```
Fase 0 (Infra)
    │
    ├─► Fase 1 (Backend)
    │       │
    │       └─► Fase 2 (Frontend)
    │               │
    └───────────────┴─► Fase 3 (Testes)
                            │
                            └─► Fase 4 (Deploy)
```

---

## 3. Fase 0: Infraestrutura (P0)

**Objetivo:** Preparar ambiente Docker com Ollama + MongoDB + GPU passthrough

**Duração:** 1 dia
**Complexidade:** Média
**Bloqueador:** Nenhum

### 3.1 Tasks

- [ ] **Task 0.1:** Criar `docker-compose.yml` atualizado
  - **Descrição:** Definir 3 serviços: MongoDB, Ollama, Etnopapers
  - **Entrega:** Arquivo `docker-compose.yml` funcional
  - **Critério de aceite:** `docker-compose config` valida sem erros
  - **Tempo:** 2h

- [ ] **Task 0.2:** Configurar GPU passthrough para serviços Ollama e Etnopapers
  - **Descrição:** Adicionar `deploy.resources.reservations.devices` com `nvidia` driver
  - **Entrega:** Containers podem acessar GPU via `nvidia-smi`
  - **Critério de aceite:** `docker exec ollama nvidia-smi` exibe GPU info
  - **Tempo:** 1h

- [ ] **Task 0.3:** Definir volumes persistentes
  - **Descrição:** Criar volumes `mongodb_data` e `ollama_models`
  - **Entrega:** Volumes configurados no docker-compose
  - **Critério de aceite:** Dados persistem após `docker-compose down && docker-compose up`
  - **Tempo:** 30min

- [ ] **Task 0.4:** Implementar healthchecks para MongoDB e Ollama
  - **Descrição:** Adicionar `healthcheck` com `mongosh ping` e `curl /api/tags`
  - **Entrega:** Containers só ficam `healthy` quando prontos
  - **Critério de aceite:** `docker-compose ps` mostra status `healthy`
  - **Tempo:** 1h

- [ ] **Task 0.5:** Testar build e startup inicial
  - **Descrição:** Executar `docker-compose build && docker-compose up -d`
  - **Entrega:** Todos os serviços sobem sem erros
  - **Critério de aceite:** 3 containers rodando (mongo, ollama, etnopapers)
  - **Tempo:** 2h (incluindo troubleshooting)

- [ ] **Task 0.6:** Download do modelo Qwen2.5-7B-Instruct-Q4
  - **Descrição:** Executar `docker exec ollama ollama pull qwen2.5:7b-instruct-q4_K_M`
  - **Entrega:** Modelo baixado e armazenado em `ollama_models` volume
  - **Critério de aceite:** `docker exec ollama ollama list` mostra modelo disponível
  - **Tempo:** 30min + 10-20min download (depende da internet)

- [ ] **Task 0.7:** Documentar setup de GPU no UNRAID
  - **Descrição:** Criar `docs/setup-gpu-unraid.md` com passo-a-passo
  - **Entrega:** Documentação completa de instalação do NVIDIA Driver Plugin
  - **Critério de aceite:** Pessoa sem conhecimento prévio consegue seguir o guia
  - **Tempo:** 1h

### 3.2 Entregáveis

- `docker-compose.yml` (v2.0 com Ollama + MongoDB + GPU)
- `docs/setup-gpu-unraid.md` (guia de configuração de GPU)
- Volumes Docker criados e funcionais
- Modelo Qwen2.5 baixado e carregado

### 3.3 Critérios de Sucesso

- [ ] `docker-compose up -d` sobe 3 containers sem erros
- [ ] GPU visível em todos os containers que precisam: `nvidia-smi` funciona
- [ ] MongoDB aceita conexões: `mongosh mongodb://localhost:27017/etnopapers`
- [ ] Ollama responde: `curl http://localhost:11434/api/tags` retorna JSON com modelo
- [ ] Modelo Qwen2.5 listado e pronto para uso

---

## 4. Fase 1: Backend - Extração com AI Local (P0)

**Objetivo:** Implementar serviço de extração usando Ollama + Instructor + Pydantic

**Duração:** 2-3 dias
**Complexidade:** Média-Alta
**Bloqueador:** Fase 0 (Ollama rodando)

### 4.1 Tasks

- [ ] **Task 1.1:** Atualizar `requirements.txt` com novas dependências
  - **Descrição:** Adicionar `instructor==1.7.0`, `openai==1.59.6`, `pdfplumber==0.11.0`
  - **Entrega:** Arquivo `backend/requirements.txt` atualizado
  - **Critério de aceite:** `pip install -r requirements.txt` instala sem erros
  - **Tempo:** 15min

- [ ] **Task 1.2:** Criar schemas Pydantic para metadados
  - **Descrição:** Implementar `SpeciesData` e `ReferenceMetadata` em `backend/models/extraction.py`
  - **Entrega:** Classes Pydantic com validações (Field descriptions, validators)
  - **Critério de aceite:** Schemas instanciam corretamente e validam tipos
  - **Tempo:** 2h

- [ ] **Task 1.3:** Implementar cliente Ollama com Instructor
  - **Descrição:** Criar `OllamaExtractionClient` em `backend/services/extraction_service.py`
  - **Entrega:** Cliente configurado com `instructor.from_openai()` apontando para Ollama
  - **Critério de aceite:** Cliente consegue fazer chamadas ao Ollama e receber JSON estruturado
  - **Tempo:** 3h

- [ ] **Task 1.4:** Construir prompt de extração otimizado
  - **Descrição:** Criar system prompt + user prompt template para extração de etnobotânica
  - **Entrega:** Prompts em português otimizados para Qwen2.5 + JSON schema
  - **Critério de aceite:** Modelo retorna JSON válido seguindo schema Pydantic
  - **Tempo:** 4h (incluindo iterações de teste)

- [ ] **Task 1.5:** Implementar método `extract_metadata()`
  - **Descrição:** Método que recebe PDF text e retorna `ReferenceMetadata` validado
  - **Entrega:** Função completa com tratamento de erros e logging
  - **Critério de aceite:** Extração de PDF de teste retorna metadados completos em <5s
  - **Tempo:** 3h

- [ ] **Task 1.6:** Criar endpoint `/api/extract/metadata`
  - **Descrição:** Implementar router FastAPI em `backend/routers/extraction.py`
  - **Entrega:** Endpoint POST que aceita PDF file + researcher profile
  - **Critério de aceite:** `POST /api/extract/metadata` retorna JSON com metadados
  - **Tempo:** 2h

- [ ] **Task 1.7:** Adicionar extração de texto PDF com pdfplumber
  - **Descrição:** Integrar pdfplumber no endpoint para extrair texto antes de chamar AI
  - **Entrega:** Pipeline: PDF upload → text extraction → AI inference → JSON response
  - **Critério de aceite:** Endpoint processa PDF completo (upload → metadados) em <10s
  - **Tempo:** 2h

- [ ] **Task 1.8:** Implementar validação e error handling
  - **Descrição:** Validar tamanho PDF, tipo de arquivo, erros de inferência, timeouts
  - **Entrega:** Mensagens de erro claras em português para cada caso
  - **Critério de aceite:** Tentativa de upload de arquivo inválido retorna 400 com mensagem clara
  - **Tempo:** 2h

- [ ] **Task 1.9:** Adicionar logging estruturado
  - **Descrição:** Logs de tempo de inferência, erros, texto truncado, modelo usado
  - **Entrega:** Logs informativos em todos os pontos críticos
  - **Critério de aceite:** Logs permitem debug de problemas de extração facilmente
  - **Tempo:** 1h

### 4.2 Entregáveis

- `backend/models/extraction.py` (schemas Pydantic)
- `backend/services/extraction_service.py` (cliente Ollama + lógica de extração)
- `backend/routers/extraction.py` (endpoint FastAPI)
- `backend/requirements.txt` (dependências atualizadas)
- Prompts otimizados para extração etnobotânica

### 4.3 Critérios de Sucesso

- [ ] Endpoint `/api/extract/metadata` funciona end-to-end
- [ ] Tempo de inferência: <5s em GPU (RTX 3060+)
- [ ] JSON retornado sempre válido e seguindo schema Pydantic
- [ ] Pelo menos 80% dos campos preenchidos corretamente em PDFs de teste
- [ ] Erros tratados gracefully com mensagens em português

### 4.4 Exemplo de Request/Response

**Request:**
```bash
curl -X POST http://localhost:8000/api/extract/metadata \
  -F "pdf_file=@artigo_etnobotanica.pdf" \
  -F 'researcher_profile={"name":"Dr. Silva","institution":"UFSC","research_focus":"etnobotânica amazônica"}'
```

**Response (200 OK):**
```json
{
  "metadata": {
    "titulo": "Plantas medicinais utilizadas por comunidades ribeirinhas do Alto Rio Negro",
    "autores": ["Silva, M.A.", "Santos, P.R."],
    "ano": 2020,
    "publicacao": "Acta Amazonica 50(3): 245-256",
    "resumo": "Este estudo registrou o conhecimento tradicional...",
    "doi": "10.1590/1809-4392202000123",
    "especies": [
      {"vernacular": "cipó-alho", "nomeCientifico": "Mansoa alliacea"},
      {"vernacular": "unha-de-gato", "nomeCientifico": "Uncaria tomentosa"}
    ],
    "tipo_de_uso": "medicinal",
    "metodologia": "entrevistas semiestruturadas",
    "pais": "Brasil",
    "estado": "AM",
    "municipio": "São Gabriel da Cachoeira",
    "local": "Comunidade Baniwa do Rio Içana",
    "bioma": "Amazônia"
  },
  "extraction_time_ms": 1234.56,
  "text_length": 12450,
  "species_count": 15
}
```

---

## 5. Fase 2: Frontend - Integração (P1)

**Objetivo:** Simplificar UI removendo API keys e chamando backend local

**Duração:** 1-2 dias
**Complexidade:** Baixa-Média
**Bloqueador:** Fase 1 (Endpoint `/api/extract/metadata` funcionando)

### 5.1 Tasks

- [ ] **Task 2.1:** Remover componente `APIConfiguration`
  - **Descrição:** Deletar `frontend/src/components/APIConfiguration.tsx`
  - **Entrega:** Componente removido + imports limpos
  - **Critério de aceite:** Build do frontend sem erros
  - **Tempo:** 30min

- [ ] **Task 2.2:** Remover gerenciamento de API keys do store
  - **Descrição:** Atualizar Zustand store removendo `apiKey`, `apiProvider`, etc.
  - **Entrega:** Store simplificado apenas com `extractedMetadata` e `researcherProfile`
  - **Critério de aceite:** TypeScript compila sem erros de tipo
  - **Tempo:** 1h

- [ ] **Task 2.3:** Criar serviço `extractionService.ts`
  - **Descrição:** Implementar função `extractMetadataFromPDF()` que chama backend
  - **Entrega:** Serviço TypeScript com tipos completos e error handling
  - **Critério de aceite:** Função retorna `ExtractionResult` tipado
  - **Tempo:** 2h

- [ ] **Task 2.4:** Atualizar componente `PDFUpload.tsx`
  - **Descrição:** Remover lógica de chamada direta a APIs externas, usar `extractionService`
  - **Entrega:** Upload via backend API (multipart/form-data)
  - **Critério de aceite:** Upload de PDF → metadados exibidos em <5s
  - **Tempo:** 2h

- [ ] **Task 2.5:** Atualizar mensagens de UI para AI local
  - **Descrição:** Trocar "Configurando API key..." para "Extraindo com AI local..."
  - **Entrega:** Textos atualizados refletindo AI local (sem menção a APIs externas)
  - **Critério de aceite:** UI não menciona Gemini/ChatGPT/Claude
  - **Tempo:** 1h

- [ ] **Task 2.6:** Adicionar indicador de tempo de inferência
  - **Descrição:** Exibir `extraction_time_ms` na UI após extração
  - **Entrega:** Badge ou tooltip mostrando "Extraído em 1.2s"
  - **Critério de aceite:** Tempo visível na interface
  - **Tempo:** 1h

- [ ] **Task 2.7:** Atualizar info box com vantagens de AI local
  - **Descrição:** Mostrar "✅ Privado ✅ Gratuito ✅ Rápido ✅ Ilimitado"
  - **Entrega:** Info box atualizado com benefícios de AI local
  - **Critério de aceite:** Mensagem clara de valor para usuário
  - **Tempo:** 30min

- [ ] **Task 2.8:** Remover dependências não usadas
  - **Descrição:** Limpar `package.json` removendo clientes de API Gemini/OpenAI/Claude
  - **Entrega:** `package.json` sem dependências desnecessárias
  - **Critério de aceite:** `npm install` não instala pacotes não utilizados
  - **Tempo:** 30min

### 5.2 Entregáveis

- `frontend/src/services/extractionService.ts` (novo)
- `frontend/src/components/PDFUpload.tsx` (atualizado)
- `frontend/src/store/metadataStore.ts` (simplificado)
- ~~`frontend/src/components/APIConfiguration.tsx`~~ (deletado)
- `frontend/package.json` (dependências limpas)

### 5.3 Critérios de Sucesso

- [ ] Build do frontend sem warnings ou erros TypeScript
- [ ] Upload de PDF funciona end-to-end via backend
- [ ] Não há menções a API keys ou provedores externos na UI
- [ ] Tempo de extração exibido corretamente
- [ ] UX mais simples: upload direto sem configuração prévia

---

## 6. Fase 3: Testes e Refinamento (P1)

**Objetivo:** Validar qualidade de extração e otimizar prompts

**Duração:** 2-3 dias
**Complexidade:** Média
**Bloqueador:** Fase 2 (Sistema funcionando end-to-end)

### 6.1 Tasks

- [ ] **Task 3.1:** Criar dataset de teste
  - **Descrição:** Coletar 20 PDFs de etnobotânica com metadados conhecidos (ground truth)
  - **Entrega:** Pasta `tests/fixtures/pdfs/` com 20 artigos + metadados esperados
  - **Critério de aceite:** Cada PDF tem arquivo JSON correspondente com metadados corretos
  - **Tempo:** 3h

- [ ] **Task 3.2:** Implementar testes unitários de `extraction_service`
  - **Descrição:** Criar `tests/backend/test_extraction_service.py` com pytest
  - **Entrega:** Testes para validação de schemas, parsing de JSON, error handling
  - **Critério de aceite:** `pytest tests/backend/test_extraction_service.py -v` passa 100%
  - **Tempo:** 3h

- [ ] **Task 3.3:** Implementar testes de integração do endpoint
  - **Descrição:** Criar `tests/backend/test_extraction_endpoint.py`
  - **Entrega:** Testes end-to-end: upload PDF → verificar resposta JSON
  - **Critério de aceite:** Endpoint retorna 200 e JSON válido para PDFs de teste
  - **Tempo:** 2h

- [ ] **Task 3.4:** Executar extração em todos os 20 PDFs de teste
  - **Descrição:** Rodar extração automatizada e salvar resultados
  - **Entrega:** Arquivo `tests/results/extraction_results.json` com todos os outputs
  - **Critério de aceite:** Todos os 20 PDFs processados sem erros
  - **Tempo:** 1h (execução) + 2h (análise)

- [ ] **Task 3.5:** Calcular métricas de qualidade
  - **Descrição:** Comparar extrações com ground truth, calcular precision/recall por campo
  - **Entrega:** Relatório `tests/results/quality_report.md` com métricas
  - **Critério de aceite:** Acurácia >80% em título, autores, ano; >75% em espécies
  - **Tempo:** 3h

- [ ] **Task 3.6:** Iterar prompts para melhorar qualidade
  - **Descrição:** Ajustar system prompt e user prompt com base em erros identificados
  - **Entrega:** Prompts v2 com melhorias documentadas
  - **Critério de aceite:** Acurácia aumenta >5% após iteração
  - **Tempo:** 4h (múltiplas iterações)

- [ ] **Task 3.7:** Testar performance de inferência
  - **Descrição:** Medir tempo de extração em GPUs diferentes (RTX 3060, 3070, 3080)
  - **Entrega:** Tabela de benchmarks por GPU model
  - **Critério de aceite:** RTX 3060 processa em <5s; RTX 3080 em <2s
  - **Tempo:** 2h

- [ ] **Task 3.8:** Testar casos extremos
  - **Descrição:** PDFs corrompidos, PDFs escaneados, PDFs muito grandes, PDFs sem espécies
  - **Entrega:** Testes automatizados para edge cases
  - **Critério de aceite:** Sistema lida gracefully com todos os casos (erro claro ou extração parcial)
  - **Tempo:** 3h

### 6.2 Entregáveis

- `tests/fixtures/pdfs/` (20 artigos de teste + ground truth)
- `tests/backend/test_extraction_service.py` (testes unitários)
- `tests/backend/test_extraction_endpoint.py` (testes de integração)
- `tests/results/quality_report.md` (relatório de qualidade)
- `docs/benchmarks.md` (performance por GPU)

### 6.3 Critérios de Sucesso

- [ ] Acurácia de extração >80% (média across all campos)
- [ ] Tempo de inferência <5s em RTX 3060
- [ ] 100% dos testes pytest passando
- [ ] Sistema lida com edge cases sem crashes

---

## 7. Fase 4: Documentação e Deploy (P2)

**Objetivo:** Documentar sistema completo e fazer deploy em produção

**Duração:** 1 dia
**Complexidade:** Baixa
**Bloqueador:** Fase 3 (Testes completos)

### 7.1 Tasks

- [ ] **Task 4.1:** Atualizar `README.md` principal
  - **Descrição:** Refletir nova arquitetura com AI local + requisitos de GPU
  - **Entrega:** README com seções: Requisitos, Instalação, Uso, Troubleshooting
  - **Critério de aceite:** Pessoa nova consegue instalar seguindo apenas o README
  - **Tempo:** 2h

- [ ] **Task 4.2:** Atualizar `CLAUDE.md`
  - **Descrição:** Substituir referências a APIs externas por Ollama + Qwen2.5
  - **Entrega:** CLAUDE.md v2.0 com nova arquitetura documentada
  - **Critério de aceite:** Comandos de desenvolvimento corretos para AI local
  - **Tempo:** 1h

- [ ] **Task 4.3:** Criar `docs/quickstart.md`
  - **Descrição:** Guia passo-a-passo para setup completo em UNRAID
  - **Entrega:** Quickstart com screenshots e comandos exatos
  - **Critério de aceite:** Setup completo em <30min seguindo o guia
  - **Tempo:** 2h

- [ ] **Task 4.4:** Criar `docs/troubleshooting.md`
  - **Descrição:** Documentar soluções para problemas comuns (GPU, Ollama, MongoDB)
  - **Entrega:** Troubleshooting com seções por tipo de problema
  - **Critério de aceite:** Cobre 90% dos erros esperados
  - **Tempo:** 2h

- [ ] **Task 4.5:** Criar `docs/architecture.md`
  - **Descrição:** Diagrama de arquitetura + explicação de cada componente
  - **Entrega:** Documentação técnica detalhada com diagramas
  - **Critério de aceite:** Novo desenvolvedor entende sistema lendo esse doc
  - **Tempo:** 2h

- [ ] **Task 4.6:** Deploy em produção no UNRAID
  - **Descrição:** Executar `docker-compose up -d` no servidor de produção
  - **Entrega:** Sistema rodando e acessível via web
  - **Critério de aceite:** Upload de PDF real funciona e extrai metadados
  - **Tempo:** 1h + 1h troubleshooting

- [ ] **Task 4.7:** Criar release notes v2.0
  - **Descrição:** Documentar mudanças, breaking changes, migration guide
  - **Entrega:** `CHANGELOG.md` com release notes v2.0
  - **Critério de aceite:** Usuários sabem o que mudou e como migrar
  - **Tempo:** 1h

### 7.2 Entregáveis

- `README.md` (atualizado para v2.0)
- `CLAUDE.md` (atualizado para v2.0)
- `docs/quickstart.md` (novo)
- `docs/troubleshooting.md` (novo)
- `docs/architecture.md` (novo)
- `CHANGELOG.md` (release notes v2.0)
- Sistema em produção no UNRAID

### 7.3 Critérios de Sucesso

- [ ] Documentação completa e testada
- [ ] Sistema rodando em produção sem erros
- [ ] Primeira extração real bem-sucedida
- [ ] Feedback positivo de usuário teste

---

## 8. Resumo de Esforço e Timeline

### 8.1 Breakdown por Fase

| Fase | Tasks | Tempo Estimado | Complexidade | Prioridade |
|------|-------|----------------|--------------|------------|
| **Fase 0: Infraestrutura** | 7 | 1 dia | Média | P0 |
| **Fase 1: Backend** | 9 | 2-3 dias | Média-Alta | P0 |
| **Fase 2: Frontend** | 8 | 1-2 dias | Baixa-Média | P1 |
| **Fase 3: Testes** | 8 | 2-3 dias | Média | P1 |
| **Fase 4: Documentação** | 7 | 1 dia | Baixa | P2 |
| **TOTAL** | **39 tasks** | **7-10 dias** | - | - |

### 8.2 Timeline Visual

```
Semana 1:
  Seg: Fase 0 (Infra) ████████
  Ter: Fase 1 (Backend) ████████
  Qua: Fase 1 (Backend) ████████
  Qui: Fase 1 (Backend) ████░░░░ | Fase 2 (Frontend) ██░░░░░░
  Sex: Fase 2 (Frontend) ████████

Semana 2:
  Seg: Fase 3 (Testes) ████████
  Ter: Fase 3 (Testes) ████████
  Qua: Fase 3 (Testes) ████░░░░ | Fase 4 (Docs) ████░░░░
  Qui: Fase 4 (Deploy) ████████
  Sex: Buffer/Refinamento ████████

TOTAL: ~10 dias úteis (2 semanas)
```

### 8.3 Recursos Necessários

**Pessoas:**
- 1 Desenvolvedor Full-Stack (Python + React + Docker)

**Hardware:**
- Servidor UNRAID com:
  - GPU NVIDIA (RTX 3060+ com 6-8 GB VRAM)
  - RAM: 16 GB
  - Disco: 50 GB livres
  - NVIDIA Container Toolkit instalado

**Software/Serviços:**
- Docker + Docker Compose
- Acesso à internet (uma vez para download do modelo)
- IDE/Editor (VSCode recomendado)

### 8.4 Custos Estimados

| Item | Custo | Tipo | Observações |
|------|-------|------|-------------|
| **Desenvolvimento** | ~80h × $50/h = $4,000 | Único | 10 dias × 8h/dia |
| **GPU (se necessário)** | $300-500 (RTX 3060) | Único | Apenas se UNRAID não tiver |
| **Disco adicional** | $0-100 | Único | Se precisar expandir |
| **APIs externas** | $0 | - | Não usa mais! |
| **Hosting/Cloud** | $0 | - | Self-hosted no UNRAID |
| **TOTAL** | **$4,000-4,600** | Único | Zero custos recorrentes |

**ROI (Return on Investment):**
- Antes: $0.02-0.05 por artigo (APIs externas) × 1000 artigos = $20-50/mês
- Depois: $0 por artigo (AI local)
- **Payback:** 7-10 meses (baseado em 1000 artigos/mês)

---

## 9. Riscos e Plano de Contingência

### 9.1 Riscos Técnicos

| Risco | Probabilidade | Impacto | Contingência |
|-------|---------------|---------|--------------|
| **GPU insuficiente no UNRAID** | Baixa | Alto | Verificar specs antes; fallback: usar quantização Q3 (3.3 GB) ou NuExtract-tiny (0.5B) |
| **Qualidade de extração <80%** | Média | Médio | Iterar prompts; fallback: manter opção de APIs externas como híbrido |
| **Ollama não suporta Qwen2.5** | Muito Baixa | Alto | Usar modelo alternativo (Mistral-7B ou Sabiá-7B) |
| **Inferência muito lenta (>10s)** | Baixa | Médio | Verificar se GPU está sendo usada; otimizar prompts; cache de respostas |
| **Docker muito grande para UNRAID** | Muito Baixa | Baixo | UNRAID moderno suporta facilmente 10+ GB |

### 9.2 Riscos de Projeto

| Risco | Probabilidade | Impacto | Contingência |
|-------|---------------|---------|--------------|
| **Atraso no cronograma** | Média | Médio | Buffer de 2-3 dias; priorizar P0/P1 tasks |
| **Desenvolvedor indisponível** | Baixa | Alto | Documentação detalhada permite handoff |
| **Requisitos mudam mid-project** | Baixa | Médio | Freeze de features; mudanças para v2.1 |
| **Testes revelam bugs críticos** | Média | Médio | Fase 3 com buffer; fix antes de deploy |

### 9.3 Plano B: Hybrid Approach

Se AI local não atingir qualidade desejada:

**Opção 1: Híbrido (Local + Cloud)**
```python
# Permitir escolha do usuário
if settings.extraction_mode == "local":
    return extract_with_ollama(pdf_text)
elif settings.extraction_mode == "gemini":
    return extract_with_gemini(pdf_text, api_key)
```

**Vantagens:**
- Flexibilidade para usuários
- Fallback se modelo local falhar
- Comparação de qualidade

**Desvantagens:**
- Maior complexidade
- Manter código de APIs externas

---

## 10. Métricas de Sucesso

### 10.1 Métricas Técnicas

| Métrica | Meta | Como Medir |
|---------|------|------------|
| **Tempo de inferência** | <5s (RTX 3060) | `extraction_time_ms` no response |
| **Acurácia de extração** | >80% (média) | Comparar com ground truth em 20 PDFs |
| **Uptime do sistema** | >99% | Monitoramento UNRAID (semana 1 pós-deploy) |
| **Tamanho Docker** | <10 GB | `docker images` + `docker system df` |
| **VRAM usada** | <8 GB | `nvidia-smi` durante inferência |

### 10.2 Métricas de UX

| Métrica | Meta | Como Medir |
|---------|------|------------|
| **Tempo para primeira extração** | <2 min | Cronômetro: acesso → upload → metadados |
| **Taxa de sucesso de upload** | >95% | Logs de erros vs. sucessos |
| **Clareza de mensagens de erro** | Usuário entende problema | Feedback qualitativo |
| **NPS (Net Promoter Score)** | >50 | Survey pós-uso (5 usuários) |

### 10.3 Métricas de Negócio

| Métrica | Meta | Como Medir |
|---------|------|------------|
| **Custo por artigo processado** | $0 | Zero custos de API |
| **Artigos processados/mês** | Ilimitado | Sem quotas |
| **ROI** | Positivo em 12 meses | Custo dev / economia de API |
| **Privacidade** | 100% local | Auditoria: zero requests externos |

---

## 11. Próximos Passos (Pós v2.0)

### 11.1 Melhorias Futuras

**v2.1 (Q2 2025):**
- Fine-tuning do Qwen2.5 em corpus de etnobotânica brasileiro
- Processamento em batch (múltiplos PDFs de uma vez)
- Cache de inferências (PDFs já processados)
- Dashboard de analytics (espécies mais mencionadas, regiões, etc.)

**v2.2 (Q3 2025):**
- Suporte a OCR para PDFs escaneados (Tesseract integration)
- Exportação de dados para CSV/JSON
- API REST pública para integrações
- Sistema de autenticação multi-usuário

**v3.0 (Q4 2025):**
- RAG (Retrieval-Augmented Generation) com banco de conhecimento etnobotânico
- Suporte a múltiplas GPUs para paralelização
- Integração com bases de dados científicas (GBIF, SciELO, etc.)
- Mobile app para coleta de campo

### 11.2 Pesquisa e Experimentação

- **Modelo maior:** Testar Qwen2.5-14B para ver se melhora acurácia
- **Modelo especializado:** Avaliar fine-tuning de NuExtract para português
- **Modelo nativo português:** Comparar com Sabiá-7B após instruction tuning
- **Quantização agressiva:** Testar Q2/IQ2 para reduzir VRAM se necessário
- **Frameworks alternativos:** Avaliar vLLM para produção de alta escala

---

## 12. Conclusão

A migração para AI local com **Ollama + Qwen2.5-7B-Instruct** é viável, bem dimensionada e oferece benefícios claros:

✅ **Viável tecnicamente:** Stack comprovado (Ollama 100K+ deployments, Qwen2.5 state-of-the-art 2025)
✅ **Escopo claro:** 39 tasks bem definidas, 7-10 dias de desenvolvimento
✅ **Riscos gerenciáveis:** Mitigações identificadas, plano B se necessário
✅ **ROI positivo:** Economia de custos de API paga investimento em 7-10 meses
✅ **Privacidade superior:** Dados nunca saem do servidor, 100% local
✅ **UX melhor:** Sem fricção de configurar API keys, upload direto

**Recomendação:** Aprovar migração e iniciar Fase 0 (Infraestrutura).

---

**Próximo Passo:** Revisar este plano com stakeholders → Aprovar → Começar Fase 0

**Responsável:** Equipe Etnopapers
**Data de Aprovação:** _________
**Data de Início:** _________
**Data Alvo de Conclusão:** _________ (10 dias úteis após início)
