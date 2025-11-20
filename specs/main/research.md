# Pesquisa Técnica: Etnopapers

**Data**: 2025-11-20 | **Status**: Fase 0 Concluída | **Decisões**: 5 principais

Esta documentação consolida as decisões técnicas tomadas durante a fase de pesquisa, incluindo rationale, alternativas consideradas, e mitigação de riscos.

---

## 1. Extração de Texto de PDF no Navegador

### Decisão
**Usar `pdf.js` para extração nativa de texto + `Tesseract.js` como fallback para OCR em PDFs escaneados.**

### Rationale
- **pdf.js**: Padrão da indústria (usado por Firefox, Chrome). Funciona diretamente no navegador, nenhuma dependência backend.
- **Tesseract.js**: Implementação WASM de código aberto. Detecção automática de qualidade baseada em densidade de texto.
- **Segurança**: Nenhum arquivo PDF enviado ao servidor; processamento 100% local no navegador.

### Implementação

**Detecção de PDFs Escaneados**:
- char_density = total_chars / num_paginas
- Se char_density < 500: provável escaneado; exibir aviso
- Se > 500: texto nativo; processar direto

### Metas de Performance

- Extração PDF (até 30 páginas): < 5 segundos
- OCR com Tesseract.js: < 30 segundos (com Web Worker)

---

## 2. Integração com APIs de IA: Escolha do Provedor Primário

### Decisão
**Anthropic Claude API como provedor primário para chamadas diretas do navegador.**

### Rationale

| Provedor | CORS Direto | Custo | Qualidade Português |
|----------|-------------|-------|-------------------|
| **Claude** | ✅ Completo | $0.00025/1K | Excelente |
| **Gemini** | ⚠️ Limitado | 🆓 Quota | Muito bom |
| **ChatGPT** | ❌ Bloqueado | $0.0015/1K | Excelente |

Claude suporta `anthropic-dangerous-direct-browser-access: true` header para chamadas CORS diretas. Único entre os três provedores.

### Implementação
- Chamadas diretas HTTPS do navegador usando chave do localStorage
- Chave nunca sai do cliente
- Fallback para Gemini se Claude indisponível

### Risks & Mitigations
- Rate limiting (429): Implementar exponential backoff
- API downtime: Mostrar erro clara; permitir retry

---

## 3. Otimização SQLite para Concorrência & Performance

### Decisão
**WAL mode (Write-Ahead Logging) + async write lock pattern.**

### Rationale
- Permite leitores simultâneos + 1 escritor
- Performance 200-500x melhor com índices apropriados
- Padrão moderno para aplicações web

### Índices para Duplicate Detection
```sql
CREATE INDEX idx_artigos_doi ON artigos(doi) WHERE doi IS NOT NULL;
CREATE INDEX idx_artigos_duplicatas 
    ON artigos(titulo, ano_publicacao, autores);
```

### Manutenção
- `PRAGMA analyze(400)` semanalmente
- `PRAGMA optimize` automático
- `PRAGMA integrity_check` antes de downloads

### Resultado Esperado
- Duplicate detection: 1-10ms (100K artigos)
- Tabela com 1000 artigos: < 2 segundos

---

## 4. Integração com APIs de Taxonomia Botânica: GBIF + Tropicos

### Decisão
**Dois-tier cache: In-memory (10K espécies) + persistência JSON. TTL: 30 dias. Fallback: GBIF → Tropicos → "não validado".**

### Implementação
- In-memory cache para 10K espécies (~5 MB)
- Persistência em JSON ao shutdown
- Cache hit rate esperado: ~95% após população inicial

### Padrão Fallback
1. Verificar cache (< 1ms)
2. Consultar GBIF (100-200ms)
3. Se falhar, consultar Tropicos (100-200ms)
4. Se ambas falham, marcar "não validado"

### Taxa de Requisições
- GBIF: ~100 req/sec (generoso)
- Tropicos: 5 seg timeout
- Cache TTL: 30 dias

---

## 5. Docker Multi-Stage Build para Deployment

### Decisão
**Multi-stage: Node builder → Python runtime → Single Alpine container (~280 MB).**

### Estrutura
```
Stage 1: Node 20-alpine → Frontend React build (/dist)
Stage 2: Python 3.11-alpine → Backend + Frontend dist mounted
Result: Single container com frontend + backend
```

### UNRAID Compatibility
- Volume mount: `/mnt/user/appdata/etnopapers` → `/data`
- Healthcheck via curl
- Sem GPU; sem nvidia-docker necessário

### Tamanhos
- Alpine Python 3.11: 50 MB
- Python deps: 80 MB
- Frontend dist: 150 MB
- **Total**: ~280 MB

---

## Alinhamento com Constituição

### Status: ✅ PASSOU EM TODOS OS PRINCÍPIOS

| Princípio | Status | Verificação |
|-----------|--------|-------------|
| I. Privacidade | ✅ | Claude CORS direto; localStorage |
| II. Portabilidade | ✅ | SQLite único; Docker portável |
| III. Offline | ✅ | Cache 30d; graceful degradation |
| IV. Simplicidade | ✅ | MVP apenas; sem auth/multi-user |
| V. Docker | ✅ | Alpine, single container, UNRAID |
| VI. Português | ✅ | Docs em pt-BR; API em português |

---

## Próximos Passos

1. ✅ Phase 0 Research: Concluído
2. ➜ Phase 1 Design: data-model.md, contracts/, quickstart.md
3. ➜ Phase 2 Tasks: /speckit.tasks para TASK-001 a TASK-033
4. ➜ Implementation: Sprint 0 setup

**Conclusão**: 2025-11-20
