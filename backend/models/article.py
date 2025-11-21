"""
Pydantic models for article-related data
"""

from typing import Optional, List
from datetime import datetime
from pydantic import BaseModel, Field


class AuthorInfo(BaseModel):
    """Author information"""

    nome: str
    sobrenome: Optional[str] = None
    email: Optional[str] = None


class ArticleRequest(BaseModel):
    """Request model for creating/updating articles"""

    titulo: str = Field(..., min_length=1, max_length=500)
    doi: Optional[str] = None
    ano_publicacao: int = Field(..., ge=1900, le=2100)
    autores: List[AuthorInfo]
    resumo: Optional[str] = None
    status: str = Field(default="rascunho", pattern="^(rascunho|finalizado)$")


class ArticleResponse(BaseModel):
    """Response model for articles"""

    id: int
    titulo: str
    doi: Optional[str] = None
    ano_publicacao: int
    autores: List[AuthorInfo]
    resumo: Optional[str] = None
    status: str
    editado_manualmente: bool
    data_processamento: datetime
    data_ultima_modificacao: datetime

    class Config:
        from_attributes = True


class ArticleListResponse(BaseModel):
    """Response model for article list with pagination"""

    total: int
    page: int
    page_size: int
    items: List[ArticleResponse]


class DadosEstudoRequest(BaseModel):
    """Request model for study data"""

    periodo_inicio: Optional[int] = Field(None, ge=1900, le=2100)
    periodo_fim: Optional[int] = Field(None, ge=1900, le=2100)
    duracao_meses: Optional[int] = None
    metodos_coleta_dados: Optional[str] = None
    tipo_amostragem: Optional[str] = None
    tamanho_amostra: Optional[int] = None
    instrumentos_coleta: Optional[List[str]] = None


class DadosEstudoResponse(BaseModel):
    """Response model for study data"""

    id: int
    artigo_id: int
    periodo_inicio: Optional[int] = None
    periodo_fim: Optional[int] = None
    duracao_meses: Optional[int] = None
    metodos_coleta_dados: Optional[str] = None
    tipo_amostragem: Optional[str] = None
    tamanho_amostra: Optional[int] = None
    instrumentos_coleta: Optional[List[str]] = None

    class Config:
        from_attributes = True
