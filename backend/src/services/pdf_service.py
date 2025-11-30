import io
from pathlib import Path
from typing import Optional
import pdfplumber
import logging

logger = logging.getLogger(__name__)

class PDFService:
    """Service for PDF text extraction"""
    
    MAX_FILE_SIZE = 100 * 1024 * 1024  # 100 MB
    MAX_PAGES = 30
    
    @staticmethod
    def validate_pdf(file_bytes: bytes, filename: str) -> tuple[bool, Optional[str]]:
        """Validate PDF file"""
        # Check file size
        if len(file_bytes) > PDFService.MAX_FILE_SIZE:
            return False, "PDF muito grande (>100 MB)"
        
        # Check magic bytes (PDF signature)
        if not file_bytes.startswith(b'%PDF'):
            return False, "PDF nao contém assinatura valida"
        
        # Check file extension
        if not filename.lower().endswith('.pdf'):
            return False, "Arquivo nao é um PDF"
        
        return True, None
    
    @staticmethod
    def is_text_extractable(file_bytes: bytes) -> bool:
        """Check if PDF has text (not scanned/image-only)"""
        try:
            pdf_file = io.BytesIO(file_bytes)
            with pdfplumber.open(pdf_file) as pdf:
                # Check if PDF has any text in first 3 pages
                for page_num in range(min(3, len(pdf.pages))):
                    text = pdf.pages[page_num].extract_text()
                    if text and len(text.strip()) > 50:
                        return True
            return False
        except Exception as e:
            logger.error(f"Erro ao verificar texto do PDF: {e}")
            return False
    
    @staticmethod
    def extract_text(file_bytes: bytes, max_pages: int = None) -> str:
        """Extract all text from PDF"""
        if max_pages is None:
            max_pages = PDFService.MAX_PAGES
        
        try:
            pdf_file = io.BytesIO(file_bytes)
            text_content = []
            
            with pdfplumber.open(pdf_file) as pdf:
                # Check page count
                if len(pdf.pages) > max_pages:
                    logger.warning(f"PDF has {len(pdf.pages)} pages, extracting first {max_pages}")
                
                # Extract text from each page
                for page_num, page in enumerate(pdf.pages[:max_pages]):
                    text = page.extract_text()
                    if text:
                        text_content.append(f"--- Página {page_num + 1} ---")
                        text_content.append(text)
            
            return "
".join(text_content)
        except Exception as e:
            logger.error(f"Erro ao extrair texto do PDF: {e}")
            raise ValueError(f"Erro ao processar PDF: {str(e)}")
    
    @staticmethod
    def extract_pdf_metadata(file_bytes: bytes) -> dict:
        """Extract PDF metadata (title, author, etc)"""
        try:
            pdf_file = io.BytesIO(file_bytes)
            with pdfplumber.open(pdf_file) as pdf:
                metadata = pdf.metadata or {}
                return {
                    'title': metadata.get('Title', ''),
                    'author': metadata.get('Author', ''),
                    'subject': metadata.get('Subject', ''),
                    'pages': len(pdf.pages)
                }
        except Exception as e:
            logger.error(f"Erro ao extrair metadados do PDF: {e}")
            return {'pages': 0}
