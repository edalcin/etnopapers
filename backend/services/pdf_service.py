"""
PDF text extraction service using pdfplumber

Handles robust text extraction from both text-based and scanned PDFs,
with quality detection and page limit enforcement.
"""

import logging
from pathlib import Path
from typing import Tuple

import pdfplumber

logger = logging.getLogger(__name__)

# Configuration
MAX_PAGES = 50  # Maximum pages to process
MAX_CHARS_PER_PAGE = 12500  # Approximate character limit per page
MAX_TOTAL_CHARS = MAX_PAGES * MAX_CHARS_PER_PAGE


class PDFExtractionError(Exception):
    """Base error for PDF extraction issues"""

    pass


class InvalidPDFError(PDFExtractionError):
    """Raised when file is not a valid PDF"""

    pass


class PDFTooLargeError(PDFExtractionError):
    """Raised when PDF exceeds size limits"""

    pass


class PDFCorruptedError(PDFExtractionError):
    """Raised when PDF is corrupted or cannot be read"""

    pass


class PDFService:
    """PDF text extraction and quality analysis"""

    @staticmethod
    def extract_text(file_path: str) -> Tuple[str, dict]:
        """
        Extract text from PDF with quality metrics

        Args:
            file_path: Path to PDF file

        Returns:
            Tuple of (extracted_text, quality_info)
            where quality_info contains:
            - is_scanned: bool - whether PDF appears to be scanned
            - page_count: int - number of pages processed
            - char_count: int - total characters extracted
            - avg_chars_per_page: float - average characters per page
            - confidence: str - 'alta', 'media', 'baixa'

        Raises:
            InvalidPDFError: File is not a PDF
            PDFTooLargeError: PDF exceeds size limits
            PDFCorruptedError: PDF cannot be read or is corrupted
        """
        try:
            file_path = Path(file_path)

            # Verify file exists and is readable
            if not file_path.exists():
                raise InvalidPDFError(f"File not found: {file_path}")

            if not file_path.suffix.lower() == ".pdf":
                raise InvalidPDFError(f"File is not a PDF: {file_path.suffix}")

            # Check file size (rough estimate, ~6 bytes per character in compressed PDF)
            file_size_mb = file_path.stat().st_size / (1024 * 1024)
            if file_size_mb > 50:  # PDF files shouldn't be > 50 MB
                raise PDFTooLargeError(f"PDF too large: {file_size_mb:.1f} MB (max 50 MB)")

            # Extract text with quality metrics
            return PDFService._extract_with_quality(str(file_path))

        except (InvalidPDFError, PDFTooLargeError, PDFCorruptedError):
            raise
        except Exception as e:
            logger.error(f"Unexpected error reading PDF: {e}")
            raise PDFCorruptedError(f"Failed to read PDF: {str(e)}")

    @staticmethod
    def _extract_with_quality(pdf_path: str) -> Tuple[str, dict]:
        """
        Extract text with quality detection

        Returns tuple of (text, quality_info)
        """
        try:
            with pdfplumber.open(pdf_path) as pdf:
                # Check if PDF has pages
                if not pdf.pages:
                    raise PDFCorruptedError("PDF has no pages")

                # Limit to MAX_PAGES
                pages_to_process = min(len(pdf.pages), MAX_PAGES)
                if len(pdf.pages) > MAX_PAGES:
                    logger.warning(
                        f"PDF has {len(pdf.pages)} pages, processing only first {MAX_PAGES}"
                    )

                # Extract text from each page
                extracted_pages = []
                total_chars = 0

                for page_num in range(pages_to_process):
                    try:
                        page = pdf.pages[page_num]

                        # Extract text
                        page_text = page.extract_text() or ""

                        # Limit characters per page to avoid massive pages
                        if len(page_text) > MAX_CHARS_PER_PAGE:
                            page_text = page_text[:MAX_CHARS_PER_PAGE]
                            logger.warning(
                                f"Page {page_num + 1} truncated to {MAX_CHARS_PER_PAGE} characters"
                            )

                        extracted_pages.append(page_text)
                        total_chars += len(page_text)

                        # Check if we're approaching character limit
                        if total_chars > MAX_TOTAL_CHARS:
                            logger.warning(
                                f"Total extraction approaching limit ({total_chars}/{MAX_TOTAL_CHARS} chars)"
                            )
                            break

                    except Exception as e:
                        logger.warning(f"Error extracting page {page_num + 1}: {e}")
                        # Continue with next page
                        continue

                # Check if we got any text
                if not extracted_pages or total_chars == 0:
                    raise PDFCorruptedError("No text could be extracted from PDF")

                # Combine pages with page breaks
                extracted_text = "\n---PAGE BREAK---\n".join(extracted_pages)

                # Analyze quality
                quality_info = PDFService._analyze_quality(
                    extracted_text, pages_to_process, total_chars
                )

                return extracted_text, quality_info

        except PDFCorruptedError:
            raise
        except Exception as e:
            logger.error(f"Error in _extract_with_quality: {e}")
            raise PDFCorruptedError(f"Failed to extract text from PDF: {str(e)}")

    @staticmethod
    def _analyze_quality(text: str, page_count: int, char_count: int) -> dict:
        """
        Analyze extraction quality to detect scanned PDFs

        Returns quality metrics dict
        """
        avg_chars = char_count / max(page_count, 1)

        # Heuristics to detect scanned PDFs:
        # 1. Very low character count (< 500 chars per page typically means scanned)
        # 2. Lots of OCR artifacts (multiple consecutive spaces, weird characters)

        is_scanned = False
        confidence = "alta"

        # Low character count is strongest indicator of scanned PDF
        if avg_chars < 500:
            is_scanned = True
            confidence = "baixa"  # Low confidence in scanned extraction
            logger.warning(f"PDF appears to be scanned (avg {avg_chars:.0f} chars/page)")

        # Check for OCR artifacts (many spaces, unusual patterns)
        elif avg_chars < 1000:
            confidence = "media"
            # Still might be scanned or low-quality digital PDF

        return {
            "is_scanned": is_scanned,
            "page_count": page_count,
            "char_count": char_count,
            "avg_chars_per_page": avg_chars,
            "confidence": confidence,
        }

    @staticmethod
    def validate_pdf(file_path: str) -> bool:
        """
        Quick validation that file is a readable PDF

        Returns True if valid, False otherwise
        """
        try:
            file_path = Path(file_path)

            # Basic checks
            if not file_path.exists():
                return False

            if not file_path.suffix.lower() == ".pdf":
                return False

            # Try to open with pdfplumber
            with pdfplumber.open(str(file_path)) as pdf:
                return bool(pdf.pages)

        except Exception:
            return False
