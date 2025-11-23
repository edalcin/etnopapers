# Integração com AI Local: Etnopapers v2.0

**Funcionalidade**: Sistema de Extração de Metadados de Artigos Etnobotânicos
**Branch**: main
**Criado**: 2025-11-20
**Atualizado**: 2025-01-23 (v2.0 - AI Local)
**Status**: Em Planejamento

---

## Visão Geral

Este documento especifica como o backend do Etnopapers se integra com **Ollama** (framework de inferência local) para extração de metadados de PDFs usando o modelo **Qwen2.5-7B-Instruct** rodando localmente com GPU.

**Mudança Arquitetural v2.0:**
- ❌ **Antes**: Frontend chamava APIs externas (Gemini/ChatGPT/Claude) diretamente
- ✅ **Agora**: Backend processa PDFs e chama AI local (Ollama) no próprio servidor

---

## Arquitetura de Integração v2.0

```
┌─────────────┐
│  Navegador  │
│  (Frontend) │
└──────┬──────┘
       │
       │ 1. POST /api/extract/metadata (multipart/form-data)
       │    - pdf_file: File
       │    - researcher_profile: JSON (opcional)
       ▼
┌─────────────────────────────────┐
│  Backend FastAPI                │
│  ┌───────────────────────────┐  │
│  │ 1. Extract PDF text       │  │ ◄─── pdfplumber
│  │    (pdfplumber)           │  │
│  └───────────┬───────────────┘  │
│              │                  │
│  ┌───────────▼───────────────┐  │
│  │ 2. Construct prompt       │  │
│  │    - System prompt        │  │
│  │    - Researcher context   │  │
│  │    - PDF text             │  │
│  └───────────┬───────────────┘  │
│              │                  │
│  ┌───────────▼───────────────┐  │
│  │ 3. Call Ollama API        │  │
│  │    (Instructor + Pydantic)│  │
│  └───────────┬───────────────┘  │
└──────────────┼──────────────────┘
               │ HTTP POST http://ollama:11434/v1/chat/completions
               ▼
┌────────────────────────────────────┐
│  Ollama Container                  │
│  ┌──────────────────────────────┐  │
│  │ Qwen2.5-7B-Instruct-Q4       │  │ ◄─── GPU NVIDIA (6-8 GB VRAM)
│  │ (~4.8 GB model)              │  │
│  │                              │  │
│  │ - Context: 128K tokens       │  │
│  │ - Inference: 1-3s            │  │
│  │ - Output: Structured JSON    │  │
│  └──────────┬───────────────────┘  │
└─────────────┼──────────────────────┘
              │ JSON response (validated by Pydantic)
              ▼
┌─────────────────────────────────┐
│  Backend FastAPI                │
│  ┌───────────────────────────┐  │
│  │ 4. Validate response      │  │
│  │    (Pydantic schema)      │  │
│  └───────────┬───────────────┘  │
│              │                  │
│  ┌───────────▼───────────────┐  │
│  │ 5. Optional: Taxonomy     │  │
│  │    validation (GBIF API)  │  │
│  └───────────┬───────────────┘  │
│              │                  │
│  ┌───────────▼───────────────┐  │
│  │ 6. Return metadata JSON   │  │
│  └───────────┬───────────────┘  │
└──────────────┼──────────────────┘
               │ 200 OK + JSON
               ▼
┌─────────────┐
│  Frontend   │
│  Exibe      │
│  Metadados  │
└──────┬──────┘
       │ 7. Usuário clica "Salvar"
       ▼
┌─────────────────┐
│  Backend        │
│  MongoDB        │ ◄─── POST /api/articles
└─────────────────┘
```

**Fluxo Detalhado:**
1. Usuário faz upload de PDF no frontend
2. Frontend envia PDF via `POST /api/extract/metadata` (multipart/form-data)
3. Backend extrai texto do PDF com `pdfplumber`
4. Backend constrói prompt estruturado (system + user + researcher context)
5. Backend chama Ollama API local via biblioteca `instructor`
6. Ollama executa inferência com Qwen2.5 em GPU (1-3s)
7. Pydantic valida JSON response automaticamente
8. Backend opcionalmente valida taxonomia (GBIF API)
9. Backend retorna metadados estruturados para frontend
10. Frontend exibe metadados + botões Save/Edit/Discard
11. Usuário salva → frontend POST para `/api/articles` → MongoDB

---

## Ollama API Integration

### Endpoint (Interno)

```
POST http://ollama:11434/v1/chat/completions
```

**Nota**: Endpoint é acessado apenas pelo backend, nunca pelo frontend.

### Cliente Python (Instructor)

```python
import instructor
from openai import OpenAI

# Inicializar cliente Instructor com Ollama
client = instructor.from_openai(
    OpenAI(
        base_url="http://ollama:11434/v1",
        api_key="ollama"  # Dummy key (Ollama não requer auth)
    ),
    mode=instructor.Mode.JSON
)

# Fazer inferência com structured output
metadata = client.chat.completions.create(
    model="qwen2.5:7b-instruct-q4_K_M",
    messages=[
        {"role": "system", "content": system_prompt},
        {"role": "user", "content": user_prompt}
    ],
    response_model=ReferenceMetadata,  # Pydantic schema
    max_tokens=2000,
    temperature=0.1,
    top_p=0.9
)
```

### Pydantic Schema (Structured Output)

```python
from pydantic import BaseModel, Field
from typing import List, Optional

class SpeciesData(BaseModel):
    """Dados de uma espécie mencionada."""
    vernacular: str = Field(
        description="Nome popular/vernacular da planta"
    )
    nomeCientifico: str = Field(
        description="Nome científico no formato 'Gênero espécie'"
    )

class ReferenceMetadata(BaseModel):
    """Schema completo de metadados de referência."""

    # Bibliográficos
    titulo: str = Field(description="Título completo do artigo")
    autores: List[str] = Field(description="Lista de autores")
    ano: int = Field(description="Ano de publicação", ge=1900, le=2030)
    publicacao: str = Field(description="Nome da revista/livro")
    resumo: Optional[str] = Field(default=None, description="Resumo/abstract")
    doi: Optional[str] = Field(default=None, description="DOI do artigo")

    # Etnobotânicos
    especies: List[SpeciesData] = Field(
        description="Espécies vegetais mencionadas",
        min_items=1
    )
    tipo_de_uso: str = Field(
        description="Tipo predominante de uso (medicinal, alimentar, etc.)"
    )
    metodologia: str = Field(
        description="Metodologia da pesquisa"
    )

    # Geográficos
    pais: str = Field(description="País do estudo")
    estado: Optional[str] = Field(default=None, description="Estado/província")
    municipio: Optional[str] = Field(default=None, description="Município")
    local: Optional[str] = Field(default=None, description="Local/comunidade específica")
    bioma: Optional[str] = Field(default=None, description="Bioma")
```

### Prompts de Extração

**System Prompt:**
```
Você é um assistente especializado em extrair metadados estruturados de artigos científicos sobre etnobotânica.

Sua tarefa é analisar o texto de um artigo científico e extrair as seguintes informações no formato JSON:
- Dados bibliográficos (título, autores, ano, publicação, resumo, DOI)
- Espécies vegetais mencionadas (nomes populares e científicos)
- Tipo de uso predominante das plantas
- Metodologia de pesquisa
- Localização geográfica do estudo (país, estado, município, local, bioma)

Regras importantes:
1. Para nomes científicos, use sempre o formato "Gênero espécie" (ex: "Chamomilla recutita")
2. Se uma espécie não tiver nome científico identificado, use "sp." ou "sp1.", "sp2." etc.
3. Para autores, mantenha o formato original do artigo
4. Se alguma informação não estiver disponível, use null para campos opcionais
5. Para biomas, use nomes padronizados: Mata Atlântica, Cerrado, Amazônia, Caatinga, Pampa, Pantanal
6. Extraia TODAS as espécies mencionadas, não apenas as mais importantes
```

**User Prompt Template:**
```
{researcher_context}

Texto do artigo:
{pdf_text}

Extraia os metadados estruturados seguindo o schema JSON fornecido.
```

**Researcher Context (Optional):**
```
Contexto do pesquisador:
- Nome: {researcher_profile.name}
- Instituição: {researcher_profile.institution}
- Foco de pesquisa: {researcher_profile.research_focus}
```

### Response Format

**Success (200 OK):**
```json
{
  "metadata": {
    "titulo": "Uso e conhecimento tradicional de plantas medicinais no Sertão",
    "autores": ["Giraldi, M.", "Hanazaki, N."],
    "ano": 2010,
    "publicacao": "Acta bot. bras. 24(2): 395-406",
    "resumo": "O objetivo desta pesquisa foi realizar um estudo etnobotânico...",
    "doi": "10.1590/...",
    "especies": [
      {
        "vernacular": "maçanilha",
        "nomeCientifico": "Chamomilla recutita"
      },
      {
        "vernacular": "hortelã-branca",
        "nomeCientifico": "Mentha spicata"
      }
    ],
    "tipo_de_uso": "medicinal",
    "metodologia": "entrevistas semiestruturadas",
    "pais": "Brasil",
    "estado": "SC",
    "municipio": "Florianópolis",
    "local": "Sertão do Ribeirão",
    "bioma": "Mata Atlântica"
  },
  "extraction_time_ms": 1234.56,
  "text_length": 12450,
  "species_count": 2
}
```

**Error (400 Bad Request):**
```json
{
  "detail": "Arquivo deve ser um PDF (.pdf)"
}
```

**Error (422 Unprocessable Entity):**
```json
{
  "detail": "Não foi possível extrair texto do PDF. Verifique se o arquivo não está corrompido."
}
```

**Error (500 Internal Server Error):**
```json
{
  "detail": "Erro na extração de metadados: [mensagem de erro específica]"
}
```

---

## Backend Endpoint Specification

### POST /api/extract/metadata

**Descrição**: Extrai metadados de um PDF usando AI local (Ollama + Qwen2.5).

**Request:**
- **Method**: POST
- **Content-Type**: multipart/form-data
- **Body Parameters**:
  - `pdf_file` (File, required): Arquivo PDF do artigo científico (max 50 MB)
  - `researcher_profile` (string, optional): JSON com perfil do pesquisador

**Example Request (curl):**
```bash
curl -X POST http://localhost:8000/api/extract/metadata \
  -F "pdf_file=@artigo_etnobotanica.pdf" \
  -F 'researcher_profile={"name":"Dr. Silva","institution":"UFSC","research_focus":"etnobotânica amazônica"}'
```

**Example Request (JavaScript):**
```javascript
const formData = new FormData();
formData.append('pdf_file', pdfFile);
formData.append('researcher_profile', JSON.stringify({
  name: "Dr. Silva",
  institution: "UFSC",
  research_focus: "etnobotânica amazônica"
}));

const response = await fetch('/api/extract/metadata', {
  method: 'POST',
  body: formData
});

const result = await response.json();
```

**Response Codes:**
- `200 OK`: Extração bem-sucedida
- `400 Bad Request`: Arquivo inválido (não é PDF, muito grande, etc.)
- `422 Unprocessable Entity`: Falha na extração de texto do PDF
- `500 Internal Server Error`: Erro de inferência do modelo

---

## Configuração de Ambiente

### Variáveis de Ambiente Obrigatórias

```bash
# Backend (Etnopapers container)
OLLAMA_URL=http://ollama:11434          # URL do serviço Ollama
OLLAMA_MODEL=qwen2.5:7b-instruct-q4_K_M # Nome do modelo
MONGO_URI=mongodb://mongo:27017/etnopapers
```

### Docker Compose Setup

```yaml
services:
  ollama:
    image: ollama/ollama:latest
    container_name: etnopapers-ollama
    ports:
      - "11434:11434"
    volumes:
      - ollama_models:/root/.ollama
    deploy:
      resources:
        reservations:
          devices:
            - driver: nvidia
              count: 1
              capabilities: [gpu]
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:11434/api/tags"]
      interval: 30s
      timeout: 10s
      retries: 3

  etnopapers:
    build: .
    container_name: etnopapers-api
    ports:
      - "8000:8000"
    depends_on:
      ollama:
        condition: service_healthy
    environment:
      - OLLAMA_URL=http://ollama:11434
      - OLLAMA_MODEL=qwen2.5:7b-instruct-q4_K_M
      - MONGO_URI=mongodb://mongo:27017/etnopapers
    deploy:
      resources:
        reservations:
          devices:
            - driver: nvidia
              count: 1
              capabilities: [gpu]

volumes:
  ollama_models:
```

---

## Performance e Benchmarks

### Tempos de Inferência (GPU)

| GPU | VRAM | Tempo Médio | Observações |
|-----|------|-------------|-------------|
| RTX 4090 | 24 GB | 0.5-1s | Ideal para produção de alta escala |
| RTX 4080 | 16 GB | 1-2s | Excelente performance |
| RTX 3080 | 10 GB | 2-3s | Boa performance |
| **RTX 3060** | **12 GB** | **2-4s** | **Configuração mínima recomendada** |
| RTX 2070 | 8 GB | 4-6s | Aceitável para uso ocasional |

**Notas:**
- Tempos assumem PDFs de ~10-20 páginas
- Primeira inferência é mais lenta (cold start ~5-10s)
- Inferências subsequentes são rápidas (model já em memória)

### Throughput Estimado

| Cenário | Artigos/hora | Observações |
|---------|--------------|-------------|
| Single-user (ocasional) | ~100-500 | Uso típico de pesquisador |
| Single-user (intensivo) | ~500-1000 | Processamento de lote |
| Multi-user (5 users) | ~300-600 | Compartilhado entre pesquisadores |

---

## Qualidade de Extração

### Acurácia Esperada

| Campo | Acurácia Alvo | Métrica |
|-------|---------------|---------|
| Título | >95% | Exato match |
| Autores | >90% | Nome completo correto |
| Ano | >98% | Exato match |
| Espécies (nome científico) | >80% | Nome aceito GBIF |
| Localização (país/estado) | >90% | Match exato ou similar |
| Bioma | >85% | Nome padronizado |

### Fatores que Afetam Qualidade

**Positivos:**
- ✅ PDFs com texto pesquisável (não escaneados)
- ✅ Estrutura de artigo científico padrão
- ✅ Tabelas de espécies bem formatadas
- ✅ Seções claramente delimitadas

**Negativos:**
- ❌ PDFs escaneados sem OCR
- ❌ Tabelas complexas ou imagens
- ❌ Nomes científicos abreviados sem definição
- ❌ Artigos muito longos (>50 páginas)

---

## Troubleshooting

### Problema: Ollama não responde

**Sintoma**: Timeout ao chamar `/api/extract/metadata`

**Solução:**
```bash
# Verificar se Ollama está rodando
docker ps | grep ollama

# Verificar logs do Ollama
docker logs etnopapers-ollama

# Reiniciar Ollama
docker restart etnopapers-ollama

# Testar Ollama diretamente
curl http://localhost:11434/api/tags
```

### Problema: Modelo não carregado

**Sintoma**: Erro "model not found"

**Solução:**
```bash
# Listar modelos disponíveis
docker exec etnopapers-ollama ollama list

# Download manual do modelo
docker exec etnopapers-ollama ollama pull qwen2.5:7b-instruct-q4_K_M

# Verificar download
docker exec etnopapers-ollama ollama list | grep qwen2.5
```

### Problema: Inferência muito lenta (>10s)

**Causa**: GPU não está sendo usada (caiu para CPU)

**Solução:**
```bash
# Verificar se GPU está visível
docker exec etnopapers-ollama nvidia-smi

# Verificar logs de carregamento do modelo
docker logs etnopapers-ollama | grep -i "gpu\|cuda"

# Deve mostrar: "GPU detected: NVIDIA RTX 3060"
```

### Problema: Erro de memória (OOM)

**Causa**: VRAM insuficiente

**Solução:**
```bash
# Opção 1: Usar quantização menor (Q3)
docker exec etnopapers-ollama ollama pull qwen2.5:7b-instruct-q3_K_M

# Atualizar variável de ambiente
# OLLAMA_MODEL=qwen2.5:7b-instruct-q3_K_M

# Opção 2: Usar modelo menor (NuExtract-tiny 0.5B)
docker exec etnopapers-ollama ollama pull nuextract:tiny
```

---

## Comparação: v1.0 (APIs Externas) vs. v2.0 (AI Local)

| Aspecto | v1.0 (Gemini/ChatGPT/Claude) | v2.0 (Ollama + Qwen2.5) |
|---------|------------------------------|--------------------------|
| **Localização** | Frontend chama APIs externas | Backend chama Ollama local |
| **Privacidade** | Dados trafegam pela internet | Dados nunca saem do servidor |
| **Latência** | 2-10s (rede + API) | 1-3s (GPU local) |
| **Custo/artigo** | $0.01-0.05 | $0 |
| **Quota** | Limitada por plano | Ilimitada |
| **Setup** | Configurar API key no frontend | Apenas variáveis de ambiente |
| **Dependência** | Internet sempre | Internet apenas setup inicial |
| **Hardware** | Não requer GPU | GPU NVIDIA obrigatória |
| **Acurácia** | 80-85% | 80-85% (similar) |

---

## Roadmap Futuro

### v2.1 (Planejado)

- [ ] Cache de inferências (evitar reprocessar PDFs idênticos)
- [ ] Batch processing (múltiplos PDFs de uma vez)
- [ ] Streaming de resposta (UI mostra extração em tempo real)
- [ ] Fallback para CPU (se GPU não disponível)

### v2.2 (Planejado)

- [ ] Fine-tuning do Qwen2.5 em corpus etnobotânico
- [ ] Suporte a múltiplos modelos (Mistral, Sabiá, NuExtract)
- [ ] Auto-seleção de modelo por tipo de artigo
- [ ] RAG integration (retrieval-augmented generation)

---

## Referências

- **Ollama**: https://ollama.com
- **Qwen2.5**: https://qwenlm.github.io/blog/qwen2.5/
- **Instructor**: https://python.useinstructor.com/
- **Pydantic**: https://docs.pydantic.dev/
- **GBIF Species API**: https://www.gbif.org/developer/species

---

**Última atualização**: 2025-01-23 (v2.0)
**Responsável**: Equipe Etnopapers
**Contato**: GitHub Issues
