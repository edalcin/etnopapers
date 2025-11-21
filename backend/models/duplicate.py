"""
Pydantic models for duplicate detection
"""

from typing import Optional, List
from datetime import datetime
from pydantic import BaseModel
from .article import AuthorInfo


class DuplicateCheckRequest(BaseModel):
    """Request model for checking duplicates"""

    titulo: str
    ano_publicacao: int
    autores: List[AuthorInfo]
    doi: Optional[str] = None


class DuplicateArticleResponse(BaseModel):
    """Response model for a duplicate article"""

    id: int
    titulo: str
    doi: Optional[str] = None
    ano_publicacao: int
    autores: List[AuthorInfo]
    status: str
    data_processamento: datetime
    data_ultima_modificacao: datetime
    tipo_duplicata: str  # "doi" or "metadata"


class DuplicateCheckResponse(BaseModel):
    """Response model for duplicate check"""

    is_duplicate: bool
    duplicate: Optional[DuplicateArticleResponse] = None
    message: str
    similar_articles: Optional[List[DuplicateArticleResponse]] = None


class SimilarArticlesRequest(BaseModel):
    """Request model for finding similar articles"""

    titulo: str
    ano_publicacao: int
    limit: int = 5
