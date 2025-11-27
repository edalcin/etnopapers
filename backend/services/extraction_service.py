"""
AI-based metadata extraction service using Ollama + Instructor

Provides structured extraction of ethnobotanical metadata from PDF text
using local AI inference with Qwen2.5-7B model via Ollama.

Uses instructor library for guaranteed structured outputs with Pydantic validation.
"""

import logging
import time
from typing import Optional

import instructor
from openai import OpenAI, APIConnectionError, APITimeoutError

from backend.exceptions import (
    OllamaConnectionError,
    OllamaTimeoutError,
    OllamaModelNotFoundError,
    ExtractionValidationError,
)
from backend.models.article import (
    ComunidadeUso,
    UsoEspeciePorComunidade,
    SpeciesData,
    ReferenceData,
)

logger = logging.getLogger(__name__)

# Configuration
DEFAULT_OLLAMA_URL = "http://localhost:11434"
DEFAULT_MODEL = "qwen2.5:7b-instruct-q4_K_M"
DEFAULT_TIMEOUT = 60  # seconds
MAX_RETRIES = 3
BACKOFF_FACTOR = 2


class OllamaExtractionResponse(ReferenceData):
    """Output model for extraction results (extends ReferenceData)"""

    pass


class OllamaClient:
    """
    Ollama AI client for ethnobotanical metadata extraction

    Handles:
    - Connection to local Ollama service
    - Structured output generation via instructor
    - Retry logic with exponential backoff
    - Error handling and recovery
    """

    def __init__(
        self,
        ollama_url: str = DEFAULT_OLLAMA_URL,
        model: str = DEFAULT_MODEL,
        timeout: int = DEFAULT_TIMEOUT,
    ):
        """
        Initialize Ollama client

        Args:
            ollama_url: Ollama service URL (e.g., http://localhost:11434)
            model: Model name (e.g., qwen2.5:7b-instruct-q4_K_M)
            timeout: Request timeout in seconds
        """
        self.ollama_url = ollama_url
        self.model = model
        self.timeout = timeout

        # Initialize OpenAI-compatible client for Ollama
        self.client = OpenAI(
            api_key="ollama",  # Ollama doesn't require API key, but client needs one
            base_url=f"{ollama_url}/v1",
        )

        # Patch client with instructor for structured outputs
        self.client = instructor.patch(
            self.client,
            mode=instructor.Mode.JSON,  # JSON mode for Ollama compatibility
        )

        logger.info(f"OllamaClient initialized: {ollama_url} | Model: {model}")

    def extract_metadata(
        self,
        pdf_text: str,
        researcher_profile: Optional[dict] = None,
        max_chars: int = 10000,
    ) -> tuple[ReferenceData, dict]:
        """
        Extract structured metadata from PDF text using Ollama

        Args:
            pdf_text: Extracted text from PDF
            researcher_profile: Optional researcher context (name, institution, etc.)
            max_chars: Maximum characters to process (truncate if longer)

        Returns:
            Tuple of (extracted_metadata, stats)
            where stats contains:
            - extraction_time_ms: time taken for extraction
            - text_length: length of input text
            - retry_count: number of retries needed
            - model: model used
            - url: Ollama URL used

        Raises:
            OllamaConnectionError: Cannot connect to Ollama
            OllamaTimeoutError: Extraction took too long
            ExtractionValidationError: Response failed validation
        """
        # Truncate text if too long
        if len(pdf_text) > max_chars:
            logger.warning(f"PDF text truncated from {len(pdf_text)} to {max_chars} chars")
            pdf_text = pdf_text[:max_chars]

        # Record start time
        start_time = time.time()
        retry_count = 0

        # Retry loop with exponential backoff
        for attempt in range(MAX_RETRIES):
            try:
                retry_count = attempt
                logger.info(f"Extraction attempt {attempt + 1}/{MAX_RETRIES}")

                # Build system prompt
                system_prompt = self._build_system_prompt(researcher_profile)

                # Build user message with PDF text
                user_message = f"""Extraia os metadados do seguinte texto de artigo científico:

---TEXT START---
{pdf_text}
---TEXT END---

Por favor, extraia e estruture todas as informações disponíveis no formato JSON esperado."""

                # Call Ollama with structured output
                extraction = self.client.messages.create(
                    model=self.model,
                    response_model=OllamaExtractionResponse,
                    max_retries=0,  # We handle retries ourselves
                    messages=[
                        {"role": "system", "content": system_prompt},
                        {"role": "user", "content": user_message},
                    ],
                    temperature=0.1,  # Low temperature for deterministic output
                    top_p=0.9,  # Nucleus sampling for diversity without randomness
                    timeout=self.timeout,
                )

                # Calculate statistics
                elapsed_time = time.time() - start_time
                stats = {
                    "extraction_time_ms": int(elapsed_time * 1000),
                    "text_length": len(pdf_text),
                    "retry_count": retry_count,
                    "model": self.model,
                    "url": self.ollama_url,
                }

                logger.info(
                    f"Extraction successful in {stats['extraction_time_ms']}ms "
                    f"(attempt {attempt + 1})"
                )

                return extraction, stats

            except APITimeoutError as e:
                logger.warning(f"Timeout on attempt {attempt + 1}: {e}")
                if attempt < MAX_RETRIES - 1:
                    # Exponential backoff
                    wait_time = BACKOFF_FACTOR ** attempt
                    logger.info(f"Waiting {wait_time}s before retry...")
                    time.sleep(wait_time)
                else:
                    raise OllamaTimeoutError(self.timeout)

            except APIConnectionError as e:
                logger.error(f"Connection error: {e}")
                if "Connection refused" in str(e) or "connect" in str(e).lower():
                    raise OllamaConnectionError(self.ollama_url)
                else:
                    raise

            except Exception as e:
                # Handle validation or other errors
                error_msg = str(e)
                logger.error(f"Extraction error on attempt {attempt + 1}: {error_msg}")

                # Check if it's a model availability issue
                if "model" in error_msg.lower() and "not found" in error_msg.lower():
                    raise OllamaModelNotFoundError(self.model)

                # For validation errors, try again if retries left
                if attempt < MAX_RETRIES - 1:
                    wait_time = BACKOFF_FACTOR ** attempt
                    logger.info(f"Waiting {wait_time}s before retry...")
                    time.sleep(wait_time)
                else:
                    raise ExtractionValidationError(error_msg)

        # Should not reach here, but handle just in case
        raise ExtractionValidationError(
            f"Failed after {MAX_RETRIES} attempts - check logs for details"
        )

    def validate_connection(self) -> bool:
        """
        Test connection to Ollama service

        Returns True if connection successful, raises exception otherwise
        """
        try:
            # Try to list available models
            response = self.client.models.list()
            logger.info(f"Ollama connection successful. Available models: {len(response.data)}")
            return True

        except APIConnectionError as e:
            logger.error(f"Cannot connect to Ollama at {self.ollama_url}: {e}")
            raise OllamaConnectionError(self.ollama_url)

        except Exception as e:
            logger.error(f"Ollama validation error: {e}")
            raise

    @staticmethod
    def _build_system_prompt(researcher_profile: Optional[dict] = None) -> str:
        """
        Build system prompt for metadata extraction

        Args:
            researcher_profile: Optional dict with researcher context
                - name: Researcher name
                - institution: Institution
                - expertise: Research area

        Returns:
            System prompt string in Portuguese
        """
        base_prompt = """Você é um especialista em etnobotânica e processamento de texto científico.

Sua tarefa é extrair e estruturar metadados de artigos científicos sobre uso de plantas por comunidades tradicionais.

IMPORTANTE:
1. Extraia TODOS os campos disponíveis no documento
2. Se um campo não estiver disponível, deixe como null/vazio
3. Para espécies, capture:
   - Nome vernacular (comum)
   - Nome científico
   - Família botânica (se mencionada)
4. Para uso de plantas por comunidade, capture:
   - Nome da comunidade
   - Tipo de comunidade (indígena, quilombola, ribeirinha, caiçara, outro)
   - Localização (país, estado, município)
   - Forma de uso (chá, pó, óleo, infusão, etc.)
   - Tipo de uso (medicinal, alimentar, ritual, cosmético, construção)
   - Propósito específico (febre, tosse, digestão, etc.)
   - Partes utilizadas (folhas, raízes, cascas, etc.)
   - Dosagem (se mencionada)
   - Método de preparação (se mencionado)
5. Para cada espécie, pode haver múltiplos usos por comunidades diferentes
6. Capture informações geográficas (país, estado, município, localidades)
7. Identifique comunidades estudadas (indígenas, quilombolas, ribeirinhas, caiçaras)
8. Capture período do estudo (datas início e fim, se disponíveis)
9. Não consolide informações de comunidades diferentes na mesma entrada
10. Retorne resultado estruturado como JSON

Responda APENAS com o JSON estruturado, sem explicações adicionais."""

        # Add researcher profile context if provided
        if researcher_profile:
            profile_text = "\n\nCONTEXTO DO PESQUISADOR:"
            if researcher_profile.get("name"):
                profile_text += f"\nPesquisador: {researcher_profile['name']}"
            if researcher_profile.get("institution"):
                profile_text += f"\nInstituição: {researcher_profile['institution']}"
            if researcher_profile.get("expertise"):
                profile_text += f"\nEspecialidade: {researcher_profile['expertise']}"

            base_prompt += profile_text

        return base_prompt

    @staticmethod
    def _validate_response(response: OllamaExtractionResponse) -> bool:
        """
        Validate extracted response has required fields

        Args:
            response: Extraction response

        Returns:
            True if valid, raises ExtractionValidationError otherwise
        """
        # Check required fields
        if not response.titulo or not response.titulo.strip():
            raise ExtractionValidationError("Campo obrigatório faltando: título")

        if not response.ano or response.ano < 1900 or response.ano > 2100:
            raise ExtractionValidationError("Campo inválido: ano deve estar entre 1900-2100")

        if not response.autores or len(response.autores) == 0:
            raise ExtractionValidationError("Campo obrigatório faltando: autores")

        return True
