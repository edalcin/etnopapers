"""
Pydantic models for species-related data
"""

from typing import List
from typing import Optional

from pydantic import BaseModel


class SpeciesValidationRequest(BaseModel):
    """Request model for species validation"""

    nome_cientifico: str


class SpeciesResponse(BaseModel):
    """Response model for a plant species"""

    id: Optional[int] = None
    nome_cientifico: str
    autores_nome_cientifico: Optional[str] = None
    familia_botanica: Optional[str] = None
    nome_aceito_atual: Optional[str] = None
    status_validacao: str
    fonte_validacao: Optional[str] = None


class SpeciesValidationResponse(BaseModel):
    """Response model for species validation result"""

    nome_cientifico: str
    familia_botanica: Optional[str] = None
    nome_aceito_atual: Optional[str] = None
    autores_nome_cientifico: Optional[str] = None
    status_validacao: str  # validado, nao_validado, ambiguo
    fonte_validacao: Optional[str] = None  # GBIF, Tropicos, etc.
    gbif_key: Optional[int] = None
    tropicos_id: Optional[int] = None


class BulkSpeciesValidationRequest(BaseModel):
    """Request model for validating multiple species"""

    species: List[str]


class BulkSpeciesValidationResponse(BaseModel):
    """Response model for bulk validation"""

    total: int
    validados: int
    nao_validados: int
    resultados: List[SpeciesValidationResponse]


class TaxonomyCacheStats(BaseModel):
    """Response model for taxonomy cache statistics"""

    total_entries: int
    valid_entries: int
    expired_entries: int
    cache_ttl_days: int
