"""
Extraction API router - Main endpoint for PDF metadata extraction

Handles PDF file uploads, orchestrates extraction pipeline:
1. PDF upload validation
2. Text extraction from PDF
3. AI metadata extraction (Ollama + Qwen)
4. Taxonomy validation (species names)
5. Duplicate detection
6. Database save
7. Response with quality metrics
"""

import logging
from io import BytesIO

from fastapi import APIRouter, UploadFile, File, Form, HTTPException, BackgroundTasks
from fastapi.responses import JSONResponse

from backend.exceptions import (
    InvalidPDFError,
    PDFTooLargeError,
    PDFCorruptedError,
    ScannedPDFWarning,
    ExtractionValidationError,
    OllamaConnectionError,
    OllamaTimeoutError,
    RateLimitExceededError,
    InternalServerError,
)
from backend.models.article import ReferenceData, ReferenceResponse
from backend.services.pdf_service import PDFService
from backend.services.extraction_service import OllamaClient
from backend.services.taxonomy_service import TaxonomyService
from backend.services.duplicate_checker import DuplicateChecker
from backend.database.connection import get_db

logger = logging.getLogger(__name__)

# Create router
router = APIRouter(prefix="/api", tags=["extraction"])

# Initialize services (lazy - will be created on first use)
_ollama_client = None
_taxonomy_service = None
_duplicate_checker = None


def get_ollama_client() -> OllamaClient:
    """Get or create OllamaClient instance"""
    global _ollama_client
    if _ollama_client is None:
        _ollama_client = OllamaClient()
    return _ollama_client


def get_taxonomy_service() -> TaxonomyService:
    """Get or create TaxonomyService instance"""
    global _taxonomy_service
    if _taxonomy_service is None:
        _taxonomy_service = TaxonomyService()
    return _taxonomy_service


def get_duplicate_checker() -> DuplicateChecker:
    """Get or create DuplicateChecker instance"""
    global _duplicate_checker
    if _duplicate_checker is None:
        _duplicate_checker = DuplicateChecker()
    return _duplicate_checker


@router.post("/extract/metadata")
async def extract_metadata(
    pdf_file: UploadFile = File(..., description="PDF article file"),
    researcher_profile: str = Form(
        None, description="Optional JSON string with researcher context"
    ),
    background_tasks: BackgroundTasks = None,
):
    """
    Extract metadata from PDF file using Ollama AI

    **Request:**
    - `pdf_file`: PDF file (multipart form-data)
    - `researcher_profile`: Optional JSON with researcher context:
      ```json
      {
        "name": "Dr. João Silva",
        "institution": "UFSC",
        "expertise": "Ethnobotany"
      }
      ```

    **Response:**
    ```json
    {
      "status": "success",
      "metadata": {
        "titulo": "Article Title",
        "autores": ["Author 1", "Author 2"],
        "ano": 2020,
        "especies": [...],
        ...
      },
      "stats": {
        "extraction_time_ms": 2500,
        "text_length": 8500,
        "is_scanned": false,
        "confidence": "alta",
        "retry_count": 0,
        "model": "qwen2.5:7b-instruct-q4_K_M"
      },
      "warnings": [
        "⚠️ PDF appears to be scanned. Review carefully."
      ],
      "duplicate_info": {
        "found": true,
        "duplicate_id": "507f1f77bcf86cd799439011",
        "duplicate_title": "Similar article title",
        "confidence": 0.95
      }
    }
    ```

    **Error Responses:**
    - 400: Invalid PDF file
    - 422: Extraction failed or validation error
    - 503: Ollama service unavailable
    - 429: Rate limit exceeded

    **Notes:**
    - First extraction may take 3-8 seconds (model warmup)
    - Subsequent requests: 1-3 seconds
    - Scanned PDFs may have lower quality results
    """
    warnings = []
    stats = {}

    try:
        # Step 1: Validate file
        logger.info(f"Received extraction request for {pdf_file.filename}")

        if not pdf_file.filename.lower().endswith(".pdf"):
            raise InvalidPDFError("Arquivo não é um PDF")

        if pdf_file.size and pdf_file.size > 50 * 1024 * 1024:
            raise PDFTooLargeError(pdf_file.size / (1024 * 1024))

        # Step 2: Read file into memory
        file_content = await pdf_file.read()
        temp_pdf = BytesIO(file_content)

        # Save to temp file for pdfplumber
        import tempfile

        with tempfile.NamedTemporaryFile(suffix=".pdf", delete=False) as tmp:
            tmp.write(file_content)
            temp_path = tmp.name

        # Step 3: Extract text from PDF
        logger.info("Extracting text from PDF")
        try:
            pdf_text, quality_info = PDFService.extract_text(temp_path)
            stats.update(quality_info)
        except (InvalidPDFError, PDFTooLargeError, PDFCorruptedError) as e:
            raise e
        finally:
            import os

            try:
                os.unlink(temp_path)
            except:
                pass

        # Check for scanned PDF quality warning
        if quality_info["is_scanned"]:
            warnings.append("⚠️ PDF escaneado detectado. A qualidade da extração pode estar reduzida. Revise com atenção.")
            logger.warning(f"Scanned PDF detected (avg {quality_info['avg_chars_per_page']:.0f} chars/page)")

        # Step 4: Parse researcher profile if provided
        researcher_context = None
        if researcher_profile:
            try:
                import json

                researcher_context = json.loads(researcher_profile)
                logger.info(f"Using researcher context: {researcher_context.get('name', 'Unknown')}")
            except json.JSONDecodeError:
                logger.warning("Invalid researcher_profile JSON")

        # Step 5: Extract metadata with Ollama
        logger.info("Extracting metadata with Ollama")
        try:
            ollama = get_ollama_client()

            # Test connection first
            ollama.validate_connection()

            # Extract metadata
            metadata, extraction_stats = ollama.extract_metadata(
                pdf_text, researcher_profile=researcher_context
            )
            stats.update(extraction_stats)

        except OllamaConnectionError as e:
            logger.error(f"Ollama connection failed: {e}")
            raise HTTPException(
                status_code=503,
                detail=e.to_dict(),
            )
        except OllamaTimeoutError as e:
            logger.error(f"Ollama timeout: {e}")
            raise HTTPException(
                status_code=503,
                detail=e.to_dict(),
            )
        except ExtractionValidationError as e:
            logger.error(f"Extraction validation failed: {e}")
            raise HTTPException(
                status_code=422,
                detail=e.to_dict(),
            )

        # Step 6: Validate species and get taxonomic information
        logger.info(f"Found {len(metadata.especies or [])} species. Validating...")
        if metadata.especies:
            try:
                taxonomy_service = get_taxonomy_service()

                for species in metadata.especies:
                    # Validate scientific name
                    result = taxonomy_service.validate_species(species.nomeCientifico)

                    if result:
                        species.nomeAceitoValidado = result.get("accepted_name")
                        species.statusValidacao = "validado" if result.get("validated") else "naoValidado"
                        species.confianca = result.get("confidence", "baixa")
                        species.familia = result.get("family") or species.familia
                        logger.info(
                            f"Validated: {species.nomeCientifico} → {species.nomeAceitoValidado}"
                        )

            except Exception as e:
                logger.warning(f"Taxonomy validation error: {e}")
                warnings.append(f"⚠️ Erro ao validar nomes científicos: {str(e)}")

        # Step 7: Check for duplicates
        logger.info("Checking for duplicates")
        duplicate_info = None
        try:
            duplicate_checker = get_duplicate_checker()
            duplicate = duplicate_checker.find_duplicate(metadata.dict())

            if duplicate:
                duplicate_info = {
                    "found": True,
                    "duplicate_id": str(duplicate.get("_id", "")),
                    "duplicate_title": duplicate.get("titulo", ""),
                    "duplicate_status": duplicate.get("status", ""),
                    "confidence": duplicate.get("confidence_score", 0),
                }
                warnings.append(
                    f"⚠️ Artigo duplicado encontrado: {duplicate_info['duplicate_title']}"
                )
                logger.info(f"Duplicate detected with confidence {duplicate_info['confidence']}")

        except Exception as e:
            logger.warning(f"Duplicate detection error: {e}")
            warnings.append("⚠️ Erro ao verificar duplicatas")

        # Step 8: Return response
        logger.info("Extraction successful. Returning response.")

        response_data = {
            "status": "success",
            "metadata": metadata.dict(exclude_none=True),
            "stats": stats,
            "warnings": warnings,
        }

        if duplicate_info:
            response_data["duplicate_info"] = duplicate_info

        return JSONResponse(status_code=200, content=response_data)

    except HTTPException:
        raise

    except (InvalidPDFError, PDFTooLargeError, PDFCorruptedError) as e:
        logger.error(f"PDF error: {e}")
        raise HTTPException(
            status_code=400,
            detail=e.to_dict(),
        )

    except RateLimitExceededError as e:
        logger.warning(f"Rate limit: {e}")
        raise HTTPException(
            status_code=429,
            detail=e.to_dict(),
        )

    except Exception as e:
        logger.error(f"Unexpected error: {e}", exc_info=True)
        error = InternalServerError(str(e))
        raise HTTPException(
            status_code=500,
            detail=error.to_dict(),
        )


@router.get("/extract/health")
async def extraction_health():
    """
    Health check for extraction service

    Verifies:
    - Ollama connectivity
    - Model availability
    - Database connectivity

    Returns status and diagnostics
    """
    try:
        # Check Ollama
        ollama = get_ollama_client()
        ollama.validate_connection()
        ollama_status = "healthy"
        ollama_error = None
    except Exception as e:
        ollama_status = "unhealthy"
        ollama_error = str(e)

    try:
        # Check Database
        db = get_db()
        db.get_database_info()
        db_status = "healthy"
        db_error = None
    except Exception as e:
        db_status = "unhealthy"
        db_error = str(e)

    overall_status = "healthy" if (ollama_status == "healthy" and db_status == "healthy") else "degraded"

    return JSONResponse(
        status_code=200 if overall_status == "healthy" else 503,
        content={
            "status": overall_status,
            "services": {
                "ollama": {
                    "status": ollama_status,
                    "error": ollama_error,
                },
                "database": {
                    "status": db_status,
                    "error": db_error,
                },
            },
        },
    )
