# Tasks: Implementação de AI Local (v2.0)

**Projeto:** Etnopapers - Sistema de Extração de Metadados Etnobotânicos
**Versão:** 2.0 (AI Local com Ollama + Qwen2.5-7B-Instruct)
**Data:** 2025-01-23
**Status:** Planejamento

---

## Legenda de Prioridades

- **P0:** Crítico - Bloqueia tudo (deve ser feito primeiro)
- **P1:** Alto - Necessário para MVP
- **P2:** Médio - Importante mas não bloquante
- **P3:** Baixo - Nice to have

## Legenda de Esforço

- **XS:** <2h
- **S:** 2-4h
- **M:** 4-8h (1 dia)
- **L:** 8-16h (2 dias)
- **XL:** 16-24h (3 dias)
- **XXL:** 24h+ (3+ dias)

---

## FASE 0: Infraestrutura Docker + GPU (P0)

**Objetivo:** Preparar ambiente Docker com Ollama, MongoDB e GPU passthrough

| ID | Task | Prioridade | Esforço | Status | Responsável |
|----|------|-----------|---------|--------|-------------|
| 0.1 | Criar `docker-compose.yml` v2.0 com 3 serviços (MongoDB, Ollama, Etnopapers) | P0 | S | ⬜ Pendente | - |
| 0.2 | Configurar GPU passthrough (nvidia driver) para Ollama e Etnopapers | P0 | XS | ⬜ Pendente | - |
| 0.3 | Definir volumes persistentes: `mongodb_data` e `ollama_models` | P0 | XS | ⬜ Pendente | - |
| 0.4 | Implementar healthchecks para MongoDB e Ollama | P0 | XS | ⬜ Pendente | - |
| 0.5 | Testar build e startup inicial (`docker-compose up -d`) | P0 | S | ⬜ Pendente | - |
| 0.6 | Download do modelo Qwen2.5-7B-Instruct-Q4 (~4.8 GB) | P0 | XS | ⬜ Pendente | - |
| 0.7 | Documentar setup de GPU no UNRAID (`docs/setup-gpu-unraid.md`) | P0 | XS | ⬜ Pendente | - |

**Critérios de aceite da Fase 0:**
- [ ] 3 containers rodando sem erros (mongo, ollama, etnopapers)
- [ ] GPU visível em containers via `nvidia-smi`
- [ ] MongoDB aceita conexões
- [ ] Ollama responde em `/api/tags` com modelo listado

**Duração estimada:** 1 dia
**Bloqueadores:** Servidor UNRAID com GPU NVIDIA disponível

---

## FASE 1: Backend - Extração com AI Local (P0)

**Objetivo:** Implementar serviço de extração usando Ollama + Instructor

| ID | Task | Prioridade | Esforço | Status | Responsável |
|----|------|-----------|---------|--------|-------------|
| 1.1 | Atualizar `requirements.txt` (instructor, openai, pdfplumber) | P0 | XS | ⬜ Pendente | - |
| 1.2 | Criar schemas Pydantic (`SpeciesData`, `ReferenceMetadata`) | P0 | S | ⬜ Pendente | - |
| 1.3 | Implementar `OllamaExtractionClient` com Instructor | P0 | S | ⬜ Pendente | - |
| 1.4 | Construir prompts de extração otimizados (system + user) | P0 | M | ⬜ Pendente | - |
| 1.5 | Implementar método `extract_metadata()` | P0 | S | ⬜ Pendente | - |
| 1.6 | Criar endpoint `/api/extract/metadata` (FastAPI) | P0 | S | ⬜ Pendente | - |
| 1.7 | Adicionar extração de texto PDF com pdfplumber | P0 | S | ⬜ Pendente | - |
| 1.8 | Implementar validação e error handling | P0 | S | ⬜ Pendente | - |
| 1.9 | Adicionar logging estruturado | P0 | XS | ⬜ Pendente | - |

**Critérios de aceite da Fase 1:**
- [ ] Endpoint `/api/extract/metadata` funciona end-to-end
- [ ] Tempo de inferência <5s em RTX 3060
- [ ] JSON retornado sempre válido (schema Pydantic)
- [ ] Pelo menos 80% dos campos preenchidos em PDFs de teste
- [ ] Erros tratados com mensagens em português

**Duração estimada:** 2-3 dias
**Bloqueadores:** Fase 0 completa (Ollama rodando)

---

## FASE 2: Frontend - Integração (P1)

**Objetivo:** Simplificar UI removendo API keys e chamando backend

| ID | Task | Prioridade | Esforço | Status | Responsável |
|----|------|-----------|---------|--------|-------------|
| 2.1 | Remover componente `APIConfiguration.tsx` | P1 | XS | ⬜ Pendente | - |
| 2.2 | Remover gerenciamento de API keys do Zustand store | P1 | XS | ⬜ Pendente | - |
| 2.3 | Criar serviço `extractionService.ts` | P1 | S | ⬜ Pendente | - |
| 2.4 | Atualizar `PDFUpload.tsx` para chamar backend | P1 | S | ⬜ Pendente | - |
| 2.5 | Atualizar mensagens de UI para AI local | P1 | XS | ⬜ Pendente | - |
| 2.6 | Adicionar indicador de tempo de inferência | P1 | XS | ⬜ Pendente | - |
| 2.7 | Atualizar info box com vantagens de AI local | P1 | XS | ⬜ Pendente | - |
| 2.8 | Remover dependências não usadas do `package.json` | P1 | XS | ⬜ Pendente | - |

**Critérios de aceite da Fase 2:**
- [ ] Build do frontend sem warnings TypeScript
- [ ] Upload de PDF funciona via backend
- [ ] Não há menções a API keys na UI
- [ ] Tempo de extração exibido
- [ ] UX simplificada (upload direto)

**Duração estimada:** 1-2 dias
**Bloqueadores:** Fase 1 completa (endpoint `/api/extract/metadata`)

---

## FASE 3: Testes e Refinamento (P1)

**Objetivo:** Validar qualidade de extração e otimizar prompts

| ID | Task | Prioridade | Esforço | Status | Responsável |
|----|------|-----------|---------|--------|-------------|
| 3.1 | Criar dataset de teste (20 PDFs + ground truth) | P1 | S | ⬜ Pendente | - |
| 3.2 | Implementar testes unitários (`test_extraction_service.py`) | P1 | S | ⬜ Pendente | - |
| 3.3 | Implementar testes de integração (`test_extraction_endpoint.py`) | P1 | S | ⬜ Pendente | - |
| 3.4 | Executar extração em 20 PDFs de teste | P1 | S | ⬜ Pendente | - |
| 3.5 | Calcular métricas de qualidade (precision/recall) | P1 | S | ⬜ Pendente | - |
| 3.6 | Iterar prompts para melhorar qualidade | P1 | M | ⬜ Pendente | - |
| 3.7 | Testar performance em GPUs diferentes | P1 | S | ⬜ Pendente | - |
| 3.8 | Testar casos extremos (PDFs corrompidos, escaneados, etc.) | P1 | S | ⬜ Pendente | - |

**Critérios de aceite da Fase 3:**
- [ ] Acurácia >80% (média across campos)
- [ ] Tempo de inferência <5s em RTX 3060
- [ ] 100% dos testes pytest passando
- [ ] Sistema lida com edge cases

**Duração estimada:** 2-3 dias
**Bloqueadores:** Fase 2 completa (sistema funcionando end-to-end)

---

## FASE 4: Documentação e Deploy (P2)

**Objetivo:** Documentar sistema e fazer deploy em produção

| ID | Task | Prioridade | Esforço | Status | Responsável |
|----|------|-----------|---------|--------|-------------|
| 4.1 | Atualizar `README.md` para v2.0 (AI local + GPU) | P2 | S | ⬜ Pendente | - |
| 4.2 | Atualizar `CLAUDE.md` (substituir APIs por Ollama) | P2 | XS | ⬜ Pendente | - |
| 4.3 | Criar `docs/quickstart.md` (setup UNRAID step-by-step) | P2 | S | ⬜ Pendente | - |
| 4.4 | Criar `docs/troubleshooting.md` (GPU, Ollama, MongoDB) | P2 | S | ⬜ Pendente | - |
| 4.5 | Criar `docs/architecture.md` (diagramas + explicações) | P2 | S | ⬜ Pendente | - |
| 4.6 | Deploy em produção no UNRAID | P2 | S | ⬜ Pendente | - |
| 4.7 | Criar release notes v2.0 (`CHANGELOG.md`) | P2 | XS | ⬜ Pendente | - |

**Critérios de aceite da Fase 4:**
- [ ] Documentação completa e testada
- [ ] Sistema rodando em produção sem erros
- [ ] Primeira extração real bem-sucedida
- [ ] Feedback positivo de usuário teste

**Duração estimada:** 1 dia
**Bloqueadores:** Fase 3 completa (testes validados)

---

## Resumo por Prioridade

### P0 - Crítico (16 tasks)

**Fase 0: Infraestrutura (7 tasks)**
- 0.1 a 0.7

**Fase 1: Backend (9 tasks)**
- 1.1 a 1.9

**Duração total P0:** 3-4 dias

### P1 - Alto (16 tasks)

**Fase 2: Frontend (8 tasks)**
- 2.1 a 2.8

**Fase 3: Testes (8 tasks)**
- 3.1 a 3.8

**Duração total P1:** 3-5 dias

### P2 - Médio (7 tasks)

**Fase 4: Documentação (7 tasks)**
- 4.1 a 4.7

**Duração total P2:** 1 dia

---

## Timeline Visual

```
Dia 1: ███ P0 Fase 0 (Infra)
Dia 2: ███ P0 Fase 1 (Backend)
Dia 3: ███ P0 Fase 1 (Backend)
Dia 4: ██░ P0 Fase 1 + P1 Fase 2 (Frontend)
Dia 5: ███ P1 Fase 2 (Frontend)
Dia 6: ███ P1 Fase 3 (Testes)
Dia 7: ███ P1 Fase 3 (Testes)
Dia 8: ██░ P1 Fase 3 + P2 Fase 4 (Docs)
Dia 9: ███ P2 Fase 4 (Deploy)
Dia 10: ██ Buffer/Refinamento

TOTAL: 7-10 dias úteis
```

---

## Estimativas de Esforço

| Fase | Tasks | Esforço Total | Duração |
|------|-------|---------------|---------|
| Fase 0 (Infra) | 7 | ~8h | 1 dia |
| Fase 1 (Backend) | 9 | ~20h | 2-3 dias |
| Fase 2 (Frontend) | 8 | ~10h | 1-2 dias |
| Fase 3 (Testes) | 8 | ~20h | 2-3 dias |
| Fase 4 (Docs) | 7 | ~10h | 1 dia |
| **TOTAL** | **39** | **~68h** | **7-10 dias** |

---

## Dependências Críticas

```
Hardware:
  ├─ GPU NVIDIA (RTX 3060+ com 6-8 GB VRAM)
  ├─ RAM: 16 GB
  ├─ Disco: 50 GB livres
  └─ NVIDIA Container Toolkit instalado

Software:
  ├─ Docker + Docker Compose
  ├─ UNRAID 6.10+
  └─ Acesso à internet (download do modelo)

Recursos Humanos:
  └─ 1 Desenvolvedor Full-Stack (Python + React + Docker)
```

---

## Riscos e Mitigações

| Risco | Mitigação |
|-------|-----------|
| GPU insuficiente | Verificar specs antes; fallback Q3 (3.3 GB) ou NuExtract-tiny |
| Qualidade <80% | Iterar prompts; manter híbrido com APIs externas |
| Inferência lenta | Validar GPU usage; otimizar prompts |
| Atrasos | Buffer de 2-3 dias; priorizar P0/P1 |

---

## Próximos Passos

1. ✅ **Planejamento completo** (este documento)
2. ⬜ **Aprovação de stakeholders**
3. ⬜ **Iniciar Fase 0: Infraestrutura**
4. ⬜ **Check-in diário de progresso**
5. ⬜ **Demo ao final de cada fase**

---

**Data de criação:** 2025-01-23
**Última atualização:** 2025-01-23
**Próxima revisão:** Ao final de cada fase
