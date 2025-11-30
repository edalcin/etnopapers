from fastapi import APIRouter, UploadFile, File, HTTPException, Form
from typing import Optional
import logging
from ..services.pdf_service import PDFService
from ..services.extraction_service import ExtractionService
from ..services.database_service import DatabaseService
from ..services.article_service import ArticleService
from ..models.reference import Reference

logger = logging.getLogger(__name__)
router = APIRouter()

@router.post("/extract/metadata", response_model=Reference)
async def extract_metadata(
    file: UploadFile = File(...),
    researcher_name: Optional[str] = Form(None),
    researcher_specialization: Optional[str] = Form(None),
    researcher_region: Optional[str] = Form(None)
):
    """Extract metadata from PDF using Ollama"""
    try:
        # Read file
        file_bytes = await file.read()
        
        # Validate PDF
        is_valid, error_msg = PDFService.validate_pdf(file_bytes, file.filename)
        if not is_valid:
            raise HTTPException(status_code=400, detail=error_msg)
        
        # Check if PDF is text-extractable (not scanned/image-only)
        if not PDFService.is_text_extractable(file_bytes):
            raise HTTPException(
                status_code=400,
                detail="PDF nao contém texto extraível - Use ferramenta OCR externa"
            )
        
        # Extract text from PDF
        pdf_text = PDFService.extract_text(file_bytes)
        
        if not pdf_text or len(pdf_text.strip()) < 100:
            raise HTTPException(
                status_code=400,
                detail="Erro ao processar PDF - Verifique se o arquivo é válido"
            )
        
        # Build researcher profile
        researcher_profile = None
        if researcher_name or researcher_specialization or researcher_region:
            researcher_profile = {
                'name': researcher_name,
                'specialization': researcher_specialization,
                'region': researcher_region
            }
        
        # Extract metadata using Ollama
        extracted_data = await ExtractionService.extract_metadata(
            pdf_text,
            researcher_profile
        )
        
        # Check for duplicates
        duplicate_info = ArticleService.check_fuzzy_duplicate(
            extracted_data.titulo,
            extracted_data.ano or 0,
            extracted_data.autores[0] if extracted_data.autores else ""
        )
        
        if duplicate_info:
            logger.warning(f"Possível duplicado detectado: {extracted_data.titulo}")
        
        # Save to database
        created_article = DatabaseService.create_article(extracted_data)
        
        return created_article
        
    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Erro ao extrair metadados: {e}")
        raise HTTPException(
            status_code=500,
            detail=f"Erro ao processar PDF: {str(e)}"
        )
