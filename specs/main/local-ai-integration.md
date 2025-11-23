# Especificação: Integração de AI Local para Extração de Metadados

**Versão:** 2.0
**Data:** 2025-01-23
**Status:** Planejamento
**Autor:** Equipe Etnopapers

---

## 1. Visão Geral

Esta especificação detalha a refatoração do sistema Etnopapers para utilizar um modelo de AI local (self-hosted) em vez de APIs externas (Gemini, ChatGPT, Claude) para extração de metadados de artigos científicos de etnobotânica.

### 1.1 Motivação

**Problemas com APIs Externas:**
- ❌ Dependência de chaves de API fornecidas pelo usuário
- ❌ Custos por requisição (variáveis conforme uso)
- ❌ Latência de rede para APIs externas
- ❌ Preocupações com privacidade (dados enviados para terceiros)
- ❌ Limite de requisições/quota por chave
- ❌ Complexidade de gerenciar múltiplos provedores

**Vantagens da AI Local:**
- ✅ **Privacidade total:** Dados nunca saem do servidor UNRAID
- ✅ **Custo fixo:** Investimento inicial em GPU, zero custo por requisição
- ✅ **Sem limites de quota:** Extrair quantos artigos quiser
- ✅ **Latência consistente:** Inferência local (1-3 segundos)
- ✅ **Simplicidade:** Um único modelo, sem gerenciar múltiplas APIs
- ✅ **Controle total:** Versão do modelo fixada, resultados reproduzíveis
- ✅ **Offline-capable:** Funciona sem conexão à internet

### 1.2 Requisitos do Sistema

**Hardware Alvo: Servidor UNRAID com GPU NVIDIA**
- **GPU:** NVIDIA RTX 3060 ou superior (6-8 GB VRAM mínimo)
- **RAM:** 16 GB recomendado (8 GB mínimo)
- **Disco:** +10 GB para Docker images e modelos
- **CPU:** Moderno multi-core (inferência auxiliar)

**Software:**
- Docker + Docker Compose
- NVIDIA Container Toolkit (GPU passthrough para Docker)
- UNRAID 6.10+ (suporte nativo a GPU passthrough)

---

## 2. Arquitetura da Solução

### 2.1 Stack Tecnológica Escolhida

**Recomendação Final: Ollama + Qwen2.5-7B-Instruct**

| Componente | Tecnologia | Justificativa |
|------------|-----------|---------------|
| **Modelo de AI** | Qwen2.5-7B-Instruct (Q4 quantizado) | Melhor balanço: português excelente + JSON nativo + 4.8 GB |
| **Framework de Inferência** | Ollama | Simplicidade máxima, API OpenAI-compatível, 1-command setup |
| **Biblioteca de Integração** | Instructor + Pydantic | Outputs estruturados garantidos, validação automática |
| **Extração de Texto PDF** | pdfplumber | Melhor qualidade de texto que PyPDF2 |
| **Banco de Dados** | MongoDB | Mantido (sem alterações) |
| **Backend** | FastAPI | Mantido (novos endpoints de extração) |
| **Frontend** | React + TypeScript | Simplificado (remove gerenciamento de API keys) |

### 2.2 Comparação de Modelos Avaliados

| Modelo | Tamanho (Q4) | VRAM | Português | JSON | Vantagem Principal |
|--------|--------------|------|-----------|------|-------------------|
| **Qwen2.5-7B-Instruct** ⭐ | 4.8 GB | 6-8 GB | ⭐⭐⭐⭐⭐ | Nativo | Melhor balanço geral |
| NuExtract-1.5 | 4.5 GB | 6-8 GB | ⭐⭐⭐⭐ | Especializado | Purpose-built para extração |
| Sabiá-7B | 4.5 GB | 6-8 GB | ⭐⭐⭐⭐⭐ | Médio | Nativo em português |
| Mistral-7B-Instruct | 4.1 GB | 6-8 GB | ⭐⭐⭐⭐⭐ | Bom | Grande comunidade |
| Gemma 2-9B | 5.5 GB | 8-10 GB | ⭐⭐⭐ | Bom | Backing do Google |
| NuExtract-tiny | 1.2 GB | 2-4 GB | ⭐⭐⭐ | Especializado | Menor tamanho (fallback) |

**Escolha: Qwen2.5-7B-Instruct-Q4** porque:
1. Treinado em 29+ idiomas incluindo português (performance MMLU 81.6%)
2. Suporte nativo a JSON Schema (outputs estruturados garantidos)
3. Context window de 128K tokens (artigos longos)
4. Quantização Q4 reduz de 17 GB para 4.8 GB sem perda significativa de qualidade
5. Atualizado em 2025 (modelo recente, arquitetura moderna)
6. Comprovado em tarefas de extração científica

### 2.3 Diagrama de Arquitetura

```
┌─────────────────────────────────────────────────────────────────┐
│                        SERVIDOR UNRAID                          │
│  ┌────────────────────────────────────────────────────────┐     │
│  │                   DOCKER COMPOSE                       │     │
│  │                                                        │     │
│  │  ┌──────────────┐  ┌──────────────┐  ┌─────────────┐  │     │
│  │  │   MongoDB    │  │    Ollama    │  │ Etnopapers  │  │     │
│  │  │              │  │              │  │   Backend   │  │     │
│  │  │ Port: 27017  │  │ Port: 11434  │  │ Port: 8000  │  │     │
│  │  │              │  │              │  │             │  │     │
│  │  │ Volume:      │  │ Volume:      │  │ FastAPI     │  │     │
│  │  │ mongodb_data │  │ ollama_models│  │ + Instructor│  │     │
│  │  └──────────────┘  └──────────────┘  └─────────────┘  │     │
│  │         │                 │                  │         │     │
│  │         │                 │                  │         │     │
│  │         │          ┌──────┴──────┐           │         │     │
│  │         │          │  GPU NVIDIA │           │         │     │
│  │         │          │  (6-8 GB)   │◄──────────┤         │     │
│  │         │          └─────────────┘           │         │     │
│  │         │                                     │         │     │
│  │         └─────────────────────────────────────┘         │     │
│  │                                                          │     │
│  └──────────────────────────────────────────────────────────┘     │
│                                                                   │
│  Volumes Persistentes:                                            │
│  • mongodb_data: Banco de dados MongoDB                           │
│  • ollama_models: Modelos de AI (Qwen2.5-7B-Instruct-Q4)          │
│                                                                   │
└───────────────────────────────────────────────────────────────────┘
                                  ▲
                                  │
                                  │ HTTP
                                  │
                      ┌───────────┴──────────┐
                      │   Frontend React     │
                      │   (Navegador)        │
                      │                      │
                      │ • Upload PDF         │
                      │ • Exibir metadados   │
                      │ • Editar/Salvar      │
                      └──────────────────────┘
```

### 2.4 Fluxo de Extração de Metadados (Novo)

```
1. FRONTEND (React)
   │
   ├─► Usuário faz upload de PDF
   │
   └─► POST /api/extract/metadata
       {
         "pdf_file": "<base64 ou multipart>",
         "researcher_profile": {...}
       }

2. BACKEND (FastAPI)
   │
   ├─► Extrai texto do PDF com pdfplumber
   │   (Validações: tamanho <50 MB, tipo PDF, não corrompido)
   │
   ├─► Constrói prompt de extração:
   │   • Instrução de sistema (JSON schema)
   │   • Contexto do pesquisador
   │   • Texto do PDF (primeiras ~8000 palavras)
   │
   └─► POST http://ollama:11434/v1/chat/completions
       {
         "model": "qwen2.5:7b-instruct-q4_K_M",
         "messages": [...],
         "response_model": ReferenceMetadata,
         "temperature": 0.1
       }

3. OLLAMA (Inferência Local)
   │
   ├─► Carrega Qwen2.5-7B-Instruct-Q4 na GPU
   │   (Auto-offload para GPU, ~6 GB VRAM)
   │
   ├─► Executa inferência (~1-3 segundos)
   │   (Gera JSON estruturado seguindo schema Pydantic)
   │
   └─► Retorna JSON validado

4. BACKEND (Validação)
   │
   ├─► Pydantic valida schema automaticamente
   │   (Garante tipos corretos, campos obrigatórios)
   │
   ├─► Validação taxonômica (GBIF API)
   │   (Opcional: valida nomes científicos)
   │
   └─► Retorna para frontend
       {
         "metadata": {...},
         "confidence": 0.85,
         "extraction_time_ms": 1234
       }

5. FRONTEND (Exibição)
   │
   ├─► Exibe metadados extraídos
   │
   └─► Usuário pode:
       • SALVAR → POST /api/articles (status: finalizado)
       • EDITAR → Abre ManualEditor
       • DESCARTAR → Limpa estado
```

### 2.5 Docker Compose (Atualizado)

```yaml
version: '3.8'

services:
  mongodb:
    image: mongo:7.0
    container_name: etnopapers-mongodb
    ports:
      - "27017:27017"
    volumes:
      - mongodb_data:/data/db
    healthcheck:
      test: ["CMD", "mongosh", "--eval", "db.adminCommand('ping')"]
      interval: 10s
      timeout: 5s
      retries: 5
    restart: unless-stopped

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
    restart: unless-stopped

  etnopapers:
    build: .
    container_name: etnopapers-api
    ports:
      - "8000:8000"
    depends_on:
      mongodb:
        condition: service_healthy
      ollama:
        condition: service_healthy
    environment:
      - MONGO_URI=mongodb://mongodb:27017/etnopapers
      - OLLAMA_URL=http://ollama:11434
      - OLLAMA_MODEL=qwen2.5:7b-instruct-q4_K_M
      - PORT=8000
      - LOG_LEVEL=info
      - ENVIRONMENT=production
      - TAXONOMY_API_TIMEOUT=5
      - CACHE_TTL_DAYS=30
      - CORS_ORIGINS=http://localhost:3000,http://localhost:8000
    volumes:
      - ./data:/data
    deploy:
      resources:
        reservations:
          devices:
            - driver: nvidia
              count: 1
              capabilities: [gpu]
        limits:
          memory: 8G
    restart: unless-stopped

volumes:
  mongodb_data:
    driver: local
  ollama_models:
    driver: local

networks:
  default:
    name: etnopapers-network
```

---

## 3. Especificação de Implementação

### 3.1 Backend: Novo Serviço de Extração

**Arquivo:** `backend/services/extraction_service.py`

```python
"""
Serviço de extração de metadados usando Ollama + Qwen2.5-7B-Instruct.
"""

import instructor
from openai import OpenAI
from pydantic import BaseModel, Field, validator
from typing import List, Optional
import os
import time
import logging

logger = logging.getLogger(__name__)

# ==================== SCHEMAS PYDANTIC ====================

class SpeciesData(BaseModel):
    """Dados de uma espécie mencionada no artigo."""
    vernacular: str = Field(
        description="Nome popular/vernacular da planta (ex: 'maçanilha', 'hortelã-branca')"
    )
    nomeCientifico: str = Field(
        description="Nome científico no formato 'Gênero espécie' (ex: 'Chamomilla recutita', 'Mentha sp1.')"
    )

    @validator('nomeCientifico')
    def validate_scientific_name(cls, v):
        """Valida formato básico do nome científico."""
        if not v or len(v.split()) < 1:
            raise ValueError("Nome científico deve ter pelo menos um termo")
        return v.strip()


class ReferenceMetadata(BaseModel):
    """Schema completo de metadados de uma referência bibliográfica."""

    # Metadados bibliográficos
    titulo: str = Field(description="Título completo do artigo/livro")
    autores: List[str] = Field(
        description="Lista de autores (formato: 'Sobrenome, Inicial.' ou 'Nome Completo')"
    )
    ano: int = Field(
        description="Ano de publicação (4 dígitos)",
        ge=1900, le=2030
    )
    publicacao: str = Field(
        description="Nome da revista, livro ou conferência onde foi publicado"
    )
    resumo: Optional[str] = Field(
        default=None,
        description="Resumo ou abstract do artigo"
    )
    doi: Optional[str] = Field(
        default=None,
        description="Digital Object Identifier (DOI) do artigo"
    )

    # Dados etnobotânicos
    especies: List[SpeciesData] = Field(
        description="Lista de espécies vegetais mencionadas no artigo",
        min_items=1
    )
    tipo_de_uso: str = Field(
        description="Tipo predominante de uso (ex: 'medicinal', 'alimentar', 'construção', 'ritual')"
    )
    metodologia: str = Field(
        description="Metodologia da pesquisa (ex: 'entrevistas', 'observação participante', 'revisão bibliográfica')"
    )

    # Localização geográfica
    pais: str = Field(description="País onde o estudo foi realizado")
    estado: Optional[str] = Field(
        default=None,
        description="Estado ou província (sigla ou nome completo)"
    )
    municipio: Optional[str] = Field(
        default=None,
        description="Município ou cidade"
    )
    local: Optional[str] = Field(
        default=None,
        description="Localidade ou comunidade específica (ex: 'Sertão do Ribeirão', 'Aldeia Guarani')"
    )
    bioma: Optional[str] = Field(
        default=None,
        description="Bioma principal (ex: 'Mata Atlântica', 'Cerrado', 'Amazônia', 'Caatinga')"
    )

    @validator('ano')
    def validate_year(cls, v):
        """Valida se o ano é razoável."""
        current_year = 2030  # Upper bound
        if v < 1900 or v > current_year:
            raise ValueError(f"Ano deve estar entre 1900 e {current_year}")
        return v


# ==================== CLIENTE OLLAMA ====================

class OllamaExtractionClient:
    """Cliente para extração de metadados usando Ollama."""

    def __init__(self):
        self.ollama_url = os.getenv("OLLAMA_URL", "http://ollama:11434")
        self.model_name = os.getenv("OLLAMA_MODEL", "qwen2.5:7b-instruct-q4_K_M")

        # Inicializa cliente Instructor com backend Ollama
        self.client = instructor.from_openai(
            OpenAI(
                base_url=f"{self.ollama_url}/v1",
                api_key="ollama"  # Dummy key (Ollama não requer autenticação)
            ),
            mode=instructor.Mode.JSON
        )

        logger.info(f"OllamaExtractionClient initialized: {self.ollama_url} | Model: {self.model_name}")

    def extract_metadata(
        self,
        pdf_text: str,
        researcher_profile: Optional[dict] = None
    ) -> tuple[ReferenceMetadata, float]:
        """
        Extrai metadados estruturados de um artigo científico.

        Args:
            pdf_text: Texto extraído do PDF
            researcher_profile: Perfil do pesquisador (opcional, para contexto)

        Returns:
            Tupla (ReferenceMetadata, tempo_de_inferência_ms)

        Raises:
            Exception: Se a extração falhar
        """

        # Limita tamanho do texto para caber no context window
        MAX_CHARS = 12000  # ~3000 tokens (safe para context de 128K)
        truncated_text = pdf_text[:MAX_CHARS]
        if len(pdf_text) > MAX_CHARS:
            logger.warning(f"PDF text truncated: {len(pdf_text)} -> {MAX_CHARS} chars")

        # Constrói contexto do pesquisador
        researcher_context = ""
        if researcher_profile:
            researcher_context = f"""
Contexto do pesquisador:
- Nome: {researcher_profile.get('name', 'N/A')}
- Instituição: {researcher_profile.get('institution', 'N/A')}
- Foco de pesquisa: {researcher_profile.get('research_focus', 'N/A')}
"""

        # Prompt de extração
        system_prompt = """Você é um assistente especializado em extrair metadados estruturados de artigos científicos sobre etnobotânica.

Sua tarefa é analisar o texto de um artigo científico e extrair as seguintes informações no formato JSON:
- Dados bibliográficos (título, autores, ano, publicação, resumo, DOI)
- Espécies vegetais mencionadas (nomes populares e científicos)
- Tipo de uso predominante das plantas
- Metodologia de pesquisa
- Localização geográfica do estudo (país, estado, município, local, bioma)

Regras importantes:
1. Para nomes científicos, use sempre o formato "Gênero espécie" (ex: "Chamomilla recutita", "Mentha spicata")
2. Se uma espécie não tiver nome científico identificado, use "sp." ou "sp1.", "sp2." etc.
3. Para autores, mantenha o formato original do artigo
4. Se alguma informação não estiver disponível, use null para campos opcionais
5. Para biomas, use nomes padronizados: Mata Atlântica, Cerrado, Amazônia, Caatinga, Pampa, Pantanal
6. Extraia TODAS as espécies mencionadas, não apenas as mais importantes"""

        user_prompt = f"""{researcher_context}

Texto do artigo:
{truncated_text}

Extraia os metadados estruturados seguindo o schema JSON fornecido."""

        # Executa inferência
        start_time = time.time()
        try:
            metadata = self.client.chat.completions.create(
                model=self.model_name,
                messages=[
                    {"role": "system", "content": system_prompt},
                    {"role": "user", "content": user_prompt}
                ],
                response_model=ReferenceMetadata,
                max_tokens=2000,
                temperature=0.1,  # Baixa temperatura para consistência
                top_p=0.9
            )

            inference_time_ms = (time.time() - start_time) * 1000

            logger.info(f"Extraction successful: {len(metadata.especies)} species, {inference_time_ms:.0f}ms")

            return metadata, inference_time_ms

        except Exception as e:
            logger.error(f"Extraction failed: {str(e)}")
            raise


# ==================== SINGLETON ====================

_extraction_client: Optional[OllamaExtractionClient] = None

def get_extraction_client() -> OllamaExtractionClient:
    """Retorna singleton do cliente de extração."""
    global _extraction_client
    if _extraction_client is None:
        _extraction_client = OllamaExtractionClient()
    return _extraction_client
```

### 3.2 Backend: Novo Endpoint de API

**Arquivo:** `backend/routers/extraction.py`

```python
"""
Rotas de API para extração de metadados com AI local.
"""

from fastapi import APIRouter, File, UploadFile, Form, HTTPException, status
from pydantic import BaseModel
from typing import Optional
import pdfplumber
import io
import logging

from ..services.extraction_service import get_extraction_client, ReferenceMetadata

logger = logging.getLogger(__name__)
router = APIRouter(prefix="/api/extract", tags=["extraction"])


# ==================== SCHEMAS ====================

class ResearcherProfile(BaseModel):
    """Perfil do pesquisador para contexto de extração."""
    name: Optional[str] = None
    institution: Optional[str] = None
    research_focus: Optional[str] = None


class ExtractionResponse(BaseModel):
    """Resposta da extração de metadados."""
    metadata: ReferenceMetadata
    extraction_time_ms: float
    text_length: int
    species_count: int


# ==================== ENDPOINTS ====================

@router.post("/metadata", response_model=ExtractionResponse)
async def extract_metadata_from_pdf(
    pdf_file: UploadFile = File(..., description="Arquivo PDF do artigo científico"),
    researcher_profile: Optional[str] = Form(None, description="Perfil do pesquisador (JSON)")
):
    """
    Extrai metadados estruturados de um artigo científico em PDF usando AI local.

    **Requisitos:**
    - Arquivo PDF válido
    - Tamanho máximo: 50 MB
    - Contém texto extraível (não apenas imagens)

    **Retorna:**
    - Metadados estruturados (título, autores, espécies, localização, etc.)
    - Tempo de inferência em milissegundos
    - Estatísticas da extração

    **Erros:**
    - 400: PDF inválido ou muito grande
    - 422: Falha na extração de texto
    - 500: Erro de inferência do modelo
    """

    # Validação: tipo de arquivo
    if not pdf_file.filename.lower().endswith('.pdf'):
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail="Arquivo deve ser um PDF (.pdf)"
        )

    # Validação: tamanho máximo
    MAX_SIZE_MB = 50
    content = await pdf_file.read()
    size_mb = len(content) / (1024 * 1024)
    if size_mb > MAX_SIZE_MB:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail=f"Arquivo muito grande: {size_mb:.1f} MB (máximo: {MAX_SIZE_MB} MB)"
        )

    # Extração de texto com pdfplumber
    try:
        pdf_text = ""
        with pdfplumber.open(io.BytesIO(content)) as pdf:
            for page in pdf.pages:
                text = page.extract_text()
                if text:
                    pdf_text += text + "\n\n"

        if not pdf_text.strip():
            raise HTTPException(
                status_code=status.HTTP_422_UNPROCESSABLE_ENTITY,
                detail="Não foi possível extrair texto do PDF. Verifique se o arquivo não está corrompido ou protegido."
            )

        logger.info(f"PDF text extracted: {len(pdf_text)} chars from {pdf_file.filename}")

    except Exception as e:
        logger.error(f"PDF extraction error: {str(e)}")
        raise HTTPException(
            status_code=status.HTTP_422_UNPROCESSABLE_ENTITY,
            detail=f"Erro ao extrair texto do PDF: {str(e)}"
        )

    # Parse do perfil do pesquisador (se fornecido)
    researcher_dict = None
    if researcher_profile:
        import json
        try:
            researcher_dict = json.loads(researcher_profile)
        except json.JSONDecodeError:
            logger.warning("Invalid researcher profile JSON, ignoring")

    # Extração de metadados com AI local
    try:
        client = get_extraction_client()
        metadata, inference_time_ms = client.extract_metadata(
            pdf_text=pdf_text,
            researcher_profile=researcher_dict
        )

        return ExtractionResponse(
            metadata=metadata,
            extraction_time_ms=inference_time_ms,
            text_length=len(pdf_text),
            species_count=len(metadata.especies)
        )

    except Exception as e:
        logger.error(f"Model inference error: {str(e)}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Erro na extração de metadados: {str(e)}"
        )
```

### 3.3 Frontend: Componente de Upload Simplificado

**Arquivo:** `frontend/src/components/PDFUpload.tsx` (Atualizado)

```typescript
import React, { useState } from 'react';
import { useDropzone } from 'react-dropzone';
import { toast } from 'react-hot-toast';
import { useMetadataStore } from '../store/metadataStore';
import { extractMetadataFromPDF } from '../services/extractionService';

export const PDFUpload: React.FC = () => {
  const [isExtracting, setIsExtracting] = useState(false);
  const [progress, setProgress] = useState(0);
  const { setExtractedMetadata, researcherProfile } = useMetadataStore();

  const onDrop = async (acceptedFiles: File[]) => {
    if (acceptedFiles.length === 0) return;

    const file = acceptedFiles[0];

    // Validação de tamanho
    const maxSizeMB = 50;
    const sizeMB = file.size / (1024 * 1024);
    if (sizeMB > maxSizeMB) {
      toast.error(`Arquivo muito grande: ${sizeMB.toFixed(1)} MB (máximo: ${maxSizeMB} MB)`);
      return;
    }

    setIsExtracting(true);
    setProgress(0);

    try {
      // Simula progresso durante upload
      setProgress(30);

      // Chama API backend para extração
      const result = await extractMetadataFromPDF(file, researcherProfile);

      setProgress(100);

      // Salva metadados no store
      setExtractedMetadata(result.metadata);

      toast.success(
        `Extração concluída! ${result.species_count} espécies encontradas (${result.extraction_time_ms.toFixed(0)}ms)`,
        { duration: 5000 }
      );
    } catch (error: any) {
      console.error('Extraction error:', error);
      toast.error(error.message || 'Erro ao extrair metadados do PDF');
    } finally {
      setIsExtracting(false);
      setProgress(0);
    }
  };

  const { getRootProps, getInputProps, isDragActive } = useDropzone({
    onDrop,
    accept: { 'application/pdf': ['.pdf'] },
    maxFiles: 1,
    disabled: isExtracting
  });

  return (
    <div className="pdf-upload">
      <div
        {...getRootProps()}
        className={`dropzone ${isDragActive ? 'active' : ''} ${isExtracting ? 'disabled' : ''}`}
      >
        <input {...getInputProps()} />

        {isExtracting ? (
          <div className="extracting-state">
            <div className="spinner" />
            <p>Extraindo metadados com AI local...</p>
            <div className="progress-bar">
              <div className="progress-fill" style={{ width: `${progress}%` }} />
            </div>
            <small>Aguarde, processando o PDF (~1-3 segundos)</small>
          </div>
        ) : (
          <div className="idle-state">
            <svg className="upload-icon" /* ... */ />
            <p>
              {isDragActive
                ? 'Solte o arquivo PDF aqui'
                : 'Arraste um PDF ou clique para selecionar'}
            </p>
            <small>Máximo: 50 MB | Extração local com AI (privada e gratuita)</small>
          </div>
        )}
      </div>

      {!isExtracting && (
        <div className="info-box">
          <h4>ℹ️ Extração com AI Local</h4>
          <ul>
            <li>✅ Totalmente privado (dados não saem do servidor)</li>
            <li>✅ Sem custos por requisição</li>
            <li>✅ Sem limites de quota</li>
            <li>✅ Inferência rápida (~1-3 segundos)</li>
          </ul>
        </div>
      )}
    </div>
  );
};
```

**Arquivo:** `frontend/src/services/extractionService.ts` (Novo)

```typescript
/**
 * Serviço de extração de metadados via API backend.
 */

export interface SpeciesData {
  vernacular: string;
  nomeCientifico: string;
}

export interface ReferenceMetadata {
  titulo: string;
  autores: string[];
  ano: number;
  publicacao: string;
  resumo?: string;
  doi?: string;
  especies: SpeciesData[];
  tipo_de_uso: string;
  metodologia: string;
  pais: string;
  estado?: string;
  municipio?: string;
  local?: string;
  bioma?: string;
}

export interface ExtractionResult {
  metadata: ReferenceMetadata;
  extraction_time_ms: number;
  text_length: number;
  species_count: number;
}

export interface ResearcherProfile {
  name?: string;
  institution?: string;
  research_focus?: string;
}

/**
 * Extrai metadados de um PDF usando a API backend (AI local).
 */
export async function extractMetadataFromPDF(
  pdfFile: File,
  researcherProfile?: ResearcherProfile
): Promise<ExtractionResult> {
  const formData = new FormData();
  formData.append('pdf_file', pdfFile);

  if (researcherProfile) {
    formData.append('researcher_profile', JSON.stringify(researcherProfile));
  }

  const response = await fetch('/api/extract/metadata', {
    method: 'POST',
    body: formData
  });

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.detail || 'Erro ao extrair metadados');
  }

  return response.json();
}
```

---

## 4. Tamanhos Estimados do Docker

| Componente | Tamanho | Observações |
|------------|---------|-------------|
| **MongoDB** | ~700 MB | Imagem oficial mongo:7.0 |
| **Ollama (base)** | ~2 GB | Inclui CUDA runtime |
| **Qwen2.5-7B-Instruct-Q4** | ~4.8 GB | Modelo quantizado (Q4_K_M) |
| **Backend Python** | ~800 MB | FastAPI + Instructor + pdfplumber |
| **Frontend (build)** | ~10 MB | React production build (gzip) |
| **Sistema base** | ~200 MB | Alpine/slim base images |
| **TOTAL** | **~8.5 GB** | Estimativa conservadora |

**Otimizações Possíveis:**
- **Usar Q3 quantização:** -1.5 GB (total: ~7 GB)
- **Multi-stage Docker build:** -200 MB
- **Usar NuExtract-tiny 0.5B:** -4 GB (total: ~4.5 GB, mas menor qualidade)
- **Modelo sob demanda:** Baixar modelo em primeiro uso (~3 GB menos na imagem)

**Recomendação:** Manter Q4 (4.8 GB) para melhor qualidade. Total de **~8.5 GB** é aceitável para UNRAID moderno.

---

## 5. Requisitos de Hardware

### 5.1 GPU NVIDIA (Obrigatório)

| Modelo de GPU | VRAM | Status | Performance Esperada |
|---------------|------|--------|---------------------|
| RTX 4090 | 24 GB | ✅ Excelente | ~0.5-1s por extração |
| RTX 4080 | 16 GB | ✅ Ótimo | ~1-2s por extração |
| RTX 4070 | 12 GB | ✅ Ótimo | ~1-2s por extração |
| RTX 3090 | 24 GB | ✅ Excelente | ~1-2s por extração |
| RTX 3080 | 10 GB | ✅ Bom | ~2-3s por extração |
| RTX 3070 | 8 GB | ✅ Bom | ~2-3s por extração |
| **RTX 3060** | **12 GB** | ✅ **Recomendado mínimo** | ~2-4s por extração |
| RTX 2080 Ti | 11 GB | ✅ Aceitável | ~3-5s por extração |
| RTX 2070 | 8 GB | ⚠️ Limite | ~4-6s por extração |
| GTX 1080 Ti | 11 GB | ⚠️ Limite | ~5-7s por extração (GPU antiga) |
| GTX 1660 | 6 GB | ❌ Insuficiente | VRAM insuficiente para Q4 |

**Nota:** Qwen2.5-7B-Instruct-Q4 requer **6-8 GB VRAM**. Para GPUs com 8 GB exatos, pode haver lentidão se outros processos usarem VRAM.

### 5.2 Outros Requisitos

| Componente | Mínimo | Recomendado | Observações |
|------------|--------|-------------|-------------|
| **RAM** | 8 GB | 16 GB | Backend + MongoDB + Ollama |
| **Disco (SSD)** | 20 GB | 50 GB | Docker images + modelos + dados |
| **CPU** | 4 cores | 8+ cores | Para extração de texto PDF e MongoDB |
| **Rede** | 10 Mbps | 100 Mbps | Download de modelos (uma vez) |

---

## 6. Configuração Inicial (Setup)

### 6.1 Pré-requisitos no UNRAID

```bash
# 1. Instalar NVIDIA GPU Driver Plugin (Community Applications)
# Navegue para: Apps → Search "nvidia" → Install "Nvidia-Driver"

# 2. Instalar NVIDIA Container Toolkit
docker run --rm --gpus all nvidia/cuda:12.0-base nvidia-smi

# Verificar se GPU está visível
nvidia-smi

# 3. Verificar Docker Compose instalado
docker-compose --version
```

### 6.2 Download do Modelo Qwen2.5

**Opção 1: Download manual antes do build** (Recomendado)
```bash
# Dentro do servidor UNRAID (via SSH)
docker run --rm -v ollama_models:/root/.ollama --gpus all ollama/ollama:latest \
  ollama pull qwen2.5:7b-instruct-q4_K_M

# Aguardar download (~4.8 GB, pode levar 5-20 minutos dependendo da internet)
```

**Opção 2: Download no primeiro uso**
- Modelo será baixado automaticamente no primeiro request de extração
- Desvantagem: Primeira extração será muito lenta (~5-20 minutos)

### 6.3 Build e Deploy

```bash
# Clone do repositório
cd /mnt/user/appdata/etnopapers
git clone https://github.com/seu-usuario/etnopapers.git
cd etnopapers

# Build da aplicação
docker-compose build

# Inicia todos os serviços
docker-compose up -d

# Verifica logs
docker-compose logs -f etnopapers
docker-compose logs -f ollama

# Verifica se Ollama está rodando
curl http://localhost:11434/api/tags

# Testa extração via API
curl -X POST http://localhost:8000/api/extract/metadata \
  -F "pdf_file=@sample.pdf"
```

---

## 7. Migração do Sistema Atual

### 7.1 O que REMOVE

**Frontend:**
- ❌ Componente `APIConfiguration` (seleção de provedor AI)
- ❌ Input de API keys (Gemini, OpenAI, Claude)
- ❌ Armazenamento de keys em `localStorage`
- ❌ Funções de chamada direta para APIs externas
- ❌ Tratamento de erros de quota/rate limit de APIs

**Backend:**
- ❌ Nenhuma remoção (backend atual não gerencia AI)

**Documentação:**
- ❌ Seções sobre como obter API keys externas
- ❌ Comparação de custos de APIs externas
- ❌ Instruções de configuração de provedores AI

### 7.2 O que ADICIONA

**Frontend:**
- ✅ Novo serviço `extractionService.ts` (chamada para backend)
- ✅ Atualizar `PDFUpload.tsx` (upload via backend API)
- ✅ Mensagens de feedback específicas para AI local
- ✅ Indicador de tempo de inferência (~1-3s)

**Backend:**
- ✅ Novo serviço `extraction_service.py` (cliente Ollama + Instructor)
- ✅ Novo router `extraction.py` (endpoint `/api/extract/metadata`)
- ✅ Dependências: `instructor`, `pdfplumber`, `openai`
- ✅ Variáveis de ambiente: `OLLAMA_URL`, `OLLAMA_MODEL`

**Docker:**
- ✅ Novo serviço `ollama` no `docker-compose.yml`
- ✅ GPU passthrough para `ollama` e `etnopapers`
- ✅ Volume `ollama_models` para persistir modelos
- ✅ Healthcheck para Ollama

**Documentação:**
- ✅ Guia de setup de GPU no UNRAID
- ✅ Instruções de download de modelo
- ✅ Troubleshooting de inferência local
- ✅ Atualizar `CLAUDE.md`, `spec.md`, `quickstart.md`

### 7.3 O que MANTÉM (Sem Alterações)

- ✅ MongoDB e schema de dados
- ✅ Componentes de edição manual (`ManualEditor`)
- ✅ Componentes de visualização (`ArticlesTable`, `MetadataDisplay`)
- ✅ Validação taxonômica (GBIF API)
- ✅ Download de database
- ✅ Sistema de drafts

---

## 8. Testes e Validação

### 8.1 Testes de Inferência

```python
# backend/tests/test_extraction_service.py

import pytest
from ..services.extraction_service import get_extraction_client

SAMPLE_PDF_TEXT = """
Uso e conhecimento tradicional de plantas medicinais no Sertão da Ribeira

Autores: Giraldi, M.; Hanazaki, N.

Publicado em: Acta bot. bras. 24(2): 395-406, 2010

Resumo: O objetivo desta pesquisa foi realizar um estudo etnobotânico...

Foram identificadas as seguintes espécies:
- Maçanilha (Chamomilla recutita)
- Hortelã-branca (Mentha spicata)
- Boldo (Plectranthus barbatus)

Localização: Sertão do Ribeirão, Florianópolis, SC, Brasil
Bioma: Mata Atlântica
Metodologia: Entrevistas semiestruturadas
"""

def test_extraction_basic():
    """Testa extração básica de metadados."""
    client = get_extraction_client()
    metadata, inference_time = client.extract_metadata(SAMPLE_PDF_TEXT)

    assert metadata.titulo is not None
    assert len(metadata.autores) >= 1
    assert metadata.ano == 2010
    assert len(metadata.especies) >= 3
    assert metadata.pais == "Brasil"
    assert metadata.estado == "SC"
    assert metadata.bioma == "Mata Atlântica"
    assert inference_time > 0

def test_extraction_species_names():
    """Verifica formato dos nomes científicos."""
    client = get_extraction_client()
    metadata, _ = client.extract_metadata(SAMPLE_PDF_TEXT)

    for species in metadata.especies:
        assert species.nomeCientifico is not None
        assert len(species.nomeCientifico.split()) >= 1  # Pelo menos 1 palavra
        assert species.vernacular is not None

def test_extraction_performance():
    """Testa se inferência é razoavelmente rápida."""
    client = get_extraction_client()
    _, inference_time = client.extract_metadata(SAMPLE_PDF_TEXT)

    # Deve completar em menos de 10 segundos (GPU)
    assert inference_time < 10000  # ms
```

### 8.2 Testes de Integração (API)

```bash
# Teste completo end-to-end
curl -X POST http://localhost:8000/api/extract/metadata \
  -F "pdf_file=@tests/fixtures/sample_article.pdf" \
  -F 'researcher_profile={"name":"Dr. Silva","institution":"UFSC","research_focus":"ethnobotany"}' \
  | jq .

# Esperado: JSON com metadados estruturados
{
  "metadata": {
    "titulo": "Uso e conhecimento tradicional...",
    "autores": ["Giraldi, M.", "Hanazaki, N."],
    "ano": 2010,
    "especies": [
      {"vernacular": "maçanilha", "nomeCientifico": "Chamomilla recutita"},
      ...
    ],
    ...
  },
  "extraction_time_ms": 1234.56,
  "text_length": 12345,
  "species_count": 15
}
```

---

## 9. Troubleshooting

### 9.1 GPU Não Detectada

**Sintoma:** `docker-compose up` falha com erro de GPU

**Solução:**
```bash
# Verificar se nvidia-smi funciona no host
nvidia-smi

# Verificar se NVIDIA Container Toolkit está instalado
docker run --rm --gpus all nvidia/cuda:12.0-base nvidia-smi

# Se falhar, reinstalar NVIDIA Driver Plugin no UNRAID:
# Apps → Nvidia-Driver → Force Update
```

### 9.2 Modelo Não Carrega

**Sintoma:** Erro "model not found" ou timeout na primeira inferência

**Solução:**
```bash
# Download manual do modelo
docker exec -it etnopapers-ollama ollama pull qwen2.5:7b-instruct-q4_K_M

# Verificar se modelo foi baixado
docker exec -it etnopapers-ollama ollama list
```

### 9.3 Inferência Muito Lenta (>10s)

**Causas possíveis:**
1. **GPU não está sendo usada:** Verificar logs do Ollama para "CUDA" ou "GPU"
2. **VRAM insuficiente:** Modelo caiu para CPU (muito mais lento)
3. **CPU inferência:** Ollama não detectou GPU

**Solução:**
```bash
# Verificar logs do Ollama
docker logs etnopapers-ollama

# Deve mostrar:
# "GPU detected: NVIDIA RTX 3060"
# "Loading model to GPU..."

# Se não, verificar deploy.resources no docker-compose.yml
```

### 9.4 Erro de Memória (OOM)

**Sintoma:** Container Ollama reinicia ou erro "CUDA out of memory"

**Causa:** VRAM da GPU insuficiente

**Solução:**
```bash
# Opção 1: Usar quantização menor (Q3 em vez de Q4)
docker exec -it etnopapers-ollama ollama pull qwen2.5:7b-instruct-q3_K_M

# Atualizar variável de ambiente OLLAMA_MODEL no docker-compose.yml
environment:
  - OLLAMA_MODEL=qwen2.5:7b-instruct-q3_K_M

# Opção 2: Usar modelo menor (NuExtract-tiny 0.5B)
docker exec -it etnopapers-ollama ollama pull nuextract:tiny
```

---

## 10. Roadmap de Implementação

### Fase 1: Infraestrutura (Prioridade 0)
- [ ] Atualizar `docker-compose.yml` com serviço Ollama + GPU
- [ ] Criar Dockerfile multi-stage otimizado
- [ ] Testar GPU passthrough no UNRAID
- [ ] Documentar setup de GPU no quickstart.md

**Estimativa:** 1 dia
**Depende de:** Acesso ao servidor UNRAID com GPU

### Fase 2: Backend - Extração (Prioridade 0)
- [ ] Implementar `extraction_service.py` com Instructor + Pydantic
- [ ] Criar schemas `ReferenceMetadata` e `SpeciesData`
- [ ] Implementar cliente Ollama com retry e error handling
- [ ] Adicionar endpoint `/api/extract/metadata` em `extraction.py`
- [ ] Testar extração com PDFs de exemplo
- [ ] Ajustar prompts para melhor qualidade de extração

**Estimativa:** 2-3 dias
**Depende de:** Fase 1 (Ollama rodando)

### Fase 3: Frontend - Integração (Prioridade 1)
- [ ] Remover componente `APIConfiguration` e gerenciamento de keys
- [ ] Atualizar `PDFUpload.tsx` para chamar backend API
- [ ] Criar serviço `extractionService.ts`
- [ ] Atualizar UI para mostrar tempo de inferência
- [ ] Remover dependências de APIs externas (OpenAI client, etc.)
- [ ] Testar fluxo completo: upload → extração → exibição

**Estimativa:** 1-2 dias
**Depende de:** Fase 2 (API backend funcionando)

### Fase 4: Testes e Refinamento (Prioridade 1)
- [ ] Criar testes unitários para `extraction_service.py`
- [ ] Criar testes de integração para `/api/extract/metadata`
- [ ] Testar com 20+ PDFs reais de etnobotânica
- [ ] Medir acurácia da extração (comparar com extração manual)
- [ ] Ajustar prompts para melhorar qualidade
- [ ] Otimizar temperatura e top_p para consistência

**Estimativa:** 2-3 dias
**Depende de:** Fase 3 (Sistema funcionando)

### Fase 5: Documentação e Deploy (Prioridade 2)
- [ ] Atualizar `CLAUDE.md` com nova arquitetura
- [ ] Atualizar `spec.md` removendo APIs externas
- [ ] Atualizar `quickstart.md` com setup de GPU
- [ ] Criar guia de troubleshooting detalhado
- [ ] Atualizar `README.md` com nova proposta de valor
- [ ] Deploy em produção no UNRAID

**Estimativa:** 1 dia
**Depende de:** Fase 4 (Testes completos)

**TOTAL ESTIMADO:** 7-10 dias de desenvolvimento

---

## 11. Métricas de Sucesso

### 11.1 Performance

| Métrica | Meta | Medição |
|---------|------|---------|
| **Tempo de inferência** | <5 segundos/artigo | Via `extraction_time_ms` na resposta |
| **Throughput** | ≥1 artigo/minuto | Testes de carga com múltiplos PDFs |
| **Tamanho Docker** | <10 GB total | `docker images` + `docker system df` |
| **VRAM usada** | <8 GB | `nvidia-smi` durante inferência |
| **Disponibilidade** | >99% uptime | Monitoramento UNRAID |

### 11.2 Qualidade de Extração

| Campo | Acurácia Meta | Método de Validação |
|-------|---------------|---------------------|
| **Título** | >95% | Comparação com extração manual |
| **Autores** | >90% | Comparação com extração manual |
| **Ano** | >98% | Comparação com extração manual |
| **Espécies (nomes científicos)** | >80% | Validação taxonômica GBIF |
| **Localização (país/estado)** | >90% | Comparação com extração manual |
| **Bioma** | >85% | Comparação com extração manual |

**Processo de Validação:**
1. Extrair metadados de 50 artigos com AI
2. Comparar com extração manual (ground truth)
3. Calcular precision, recall e F1-score por campo
4. Iterar prompts se acurácia <80%

---

## 12. Alternativas e Fallbacks

### 12.1 Modelo Alternativo: NuExtract-1.5

Se Qwen2.5 não atingir acurácia desejada:

```bash
# Download NuExtract-1.5 (especializado em extração)
docker exec -it etnopapers-ollama ollama pull nuextract:latest

# Atualizar variável de ambiente
OLLAMA_MODEL=nuextract:latest
```

**Vantagens:**
- Purpose-built para extração estruturada
- Comprovado em papers científicos (MeXtract)

**Desvantagens:**
- Suporte a português inferior ao Qwen2.5

### 12.2 Modelo Alternativo: Sabiá-7B

Se português nativo for crítico:

```bash
# Download Sabiá-7B (nativo em português)
docker exec -it etnopapers-ollama ollama pull sabia:7b

# Requer prompt engineering (não tem instruction tuning)
```

### 12.3 Hybrid Approach (APIs Externas + Local)

**Permitir escolha do usuário:**
```python
# settings: { extraction_mode: "local" | "gemini" | "openai" }

if settings.extraction_mode == "local":
    return extract_with_ollama(pdf_text)
elif settings.extraction_mode == "gemini":
    return extract_with_gemini(pdf_text, api_key)
```

**Vantagem:** Flexibilidade para usuários sem GPU ou que preferem APIs externas

---

## 13. Conclusão

A migração para AI local com **Ollama + Qwen2.5-7B-Instruct-Q4** oferece:

✅ **Privacidade total:** Dados nunca saem do servidor
✅ **Custo zero recorrente:** Investimento único em GPU
✅ **Simplicidade:** Uma stack unificada, sem gerenciar múltiplas APIs
✅ **Performance:** Inferência local rápida (1-3s)
✅ **Qualidade:** Modelo state-of-the-art 2025 com excelente suporte a português
✅ **Escalabilidade:** Processar quantos artigos quiser sem limites

**Investimento estimado:**
- **Desenvolvimento:** 7-10 dias (1 desenvolvedor full-time)
- **Hardware:** RTX 3060+ (se ainda não disponível no UNRAID)
- **Disco:** +10 GB para Docker/modelos
- **Complexidade:** Média (requer conhecimento de Docker + GPU passthrough)

**Retorno sobre investimento:**
- **Break-even:** Imediato (sem custos de API)
- **Privacidade:** Inestimável para dados sensíveis de pesquisa
- **Experiência do usuário:** Superior (sem configuração de API keys)

Esta arquitetura posiciona o Etnopapers como uma solução **self-contained, privacy-first** para catalogação etnobotânica, eliminando dependências externas e custos recorrentes.
