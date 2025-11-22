"""
Pydantic models for reference/article data

Simplified denormalized model with all fields embedded in a single document.
"""

from typing import List
from typing import Optional

from pydantic import BaseModel
from pydantic import Field


# Nested models for reference document
class SpeciesData(BaseModel):
    """Species data embedded in reference document"""

    vernacular: str = Field(..., min_length=1, description="Vernacular/common name")
    nomeCientifico: str = Field(..., min_length=1, description="Scientific name")


class ReferenceData(BaseModel):
    """Complete reference/article data structure (denormalized)"""

    # Basic article information
    ano: int = Field(..., ge=1900, le=2100, description="Publication year")
    titulo: str = Field(..., min_length=1, max_length=500, description="Article title")
    publicacao: Optional[str] = Field(None, description="Publication venue (journal, conference)")
    autores: List[str] = Field(..., min_items=1, description="List of author names")
    resumo: Optional[str] = Field(None, description="Article abstract/summary")
    doi: Optional[str] = Field(None, description="Digital Object Identifier")

    # Species data (embedded)
    especies: List[SpeciesData] = Field(default_factory=list, description="Plant species mentioned")

    # Use and methodology information
    tipo_de_uso: Optional[str] = Field(None, description="Type of use (medicinal, alimentar, etc.)")
    metodologia: Optional[str] = Field(None, description="Research methodology")

    # Geographic information (denormalized)
    pais: Optional[str] = Field(None, description="Country")
    estado: Optional[str] = Field(None, description="State/Province")
    municipio: Optional[str] = Field(None, description="Municipality")
    local: Optional[str] = Field(None, description="Specific location/community")
    bioma: Optional[str] = Field(None, description="Biome")

    # Status
    status: str = Field(default="rascunho", pattern="^(rascunho|finalizado)$")


class ReferenceResponse(BaseModel):
    """Response model for a reference (with MongoDB ID)"""

    id: str = Field(..., alias="_id", description="MongoDB ObjectId as string")
    ano: int
    titulo: str
    publicacao: Optional[str] = None
    autores: List[str]
    resumo: Optional[str] = None
    doi: Optional[str] = None
    especies: List[SpeciesData]
    tipo_de_uso: Optional[str] = None
    metodologia: Optional[str] = None
    pais: Optional[str] = None
    estado: Optional[str] = None
    municipio: Optional[str] = None
    local: Optional[str] = None
    bioma: Optional[str] = None
    status: str

    class Config:
        populate_by_name = True  # Allow _id or id


class ReferenceListResponse(BaseModel):
    """Response model for paginated reference list"""

    total: int
    page: int
    page_size: int
    items: List[ReferenceResponse]


class ReferenceBulkCreateRequest(BaseModel):
    """Request model for bulk creating references"""

    references: List[ReferenceData]


class ReferenceBulkCreateResponse(BaseModel):
    """Response model for bulk creation result"""

    created: int
    failed: int
    ids: List[str]
    errors: Optional[List[str]] = None
