"""
Pydantic models for reference/article data

Simplified denormalized model with all fields embedded in a single document.
Enhanced with community usage tracking and species validation data.
Uses UUID string IDs and camelCase field names for REST API consistency.

Updated 2025-11-27: Added familia, statusValidacao, confianca, usosPorComunidade (Clarificações 2025-11-27)
"""

from typing import List
from typing import Optional

from pydantic import BaseModel
from pydantic import Field


# Nested models for community-based species usage (NEW - Clarificação Q1, Q3)
class ComunidadeUso(BaseModel):
    """Community using a plant species (inline denormalized object)"""

    nome: str = Field(..., min_length=1, description="Community name")
    tipo: str = Field(..., description="Community type: indígena, quilombola, ribeirinha, caiçara, outro")
    país: str = Field(..., description="Country")
    estado: str = Field(..., description="State/province")
    município: str = Field(..., description="Municipality")


class UsoEspeciePorComunidade(BaseModel):
    """Detailed usage of a plant species by a specific community (NEW - Clarificação Q1, Q3)"""

    comunidade: ComunidadeUso = Field(..., description="Community using this plant")
    formaDeUso: Optional[str] = Field(None, description="Form: chá, pó, óleo, infusão, decocção, cataplasma, tinctura, banho")
    tipoDeUso: Optional[str] = Field(None, description="Type: medicinal, alimentar, ritual, cosmético, construção")
    propositoEspecifico: Optional[str] = Field(None, description="Specific purpose: fever, cough, digestion, analgesic, etc.")
    partesUtilizadas: Optional[List[str]] = Field(None, description="Parts used: leaves, roots, bark, flowers, seeds")
    dosagem: Optional[str] = Field(None, description="Dosage if mentioned")
    metodoPreparacao: Optional[str] = Field(None, description="Preparation method if mentioned")
    origem: Optional[str] = Field(None, description="Source/origin of information")


# Updated species data model with validation and usage details
class SpeciesData(BaseModel):
    """Species data embedded in reference document (enhanced)"""

    vernacular: str = Field(..., min_length=1, description="Vernacular/common name")
    nomeCientifico: str = Field(..., min_length=1, description="Scientific name")
    familia: Optional[str] = Field(None, description="Botanical family")
    nomeAceitoValidado: Optional[str] = Field(None, description="Accepted scientific name if validated")
    statusValidacao: Optional[str] = Field(None, description="Validation status: validado or naoValidado (Clarificação Q2)")
    confianca: Optional[str] = Field(None, description="Confidence level: alta, media, baixa (Clarificação Q2)")
    usosPorComunidade: Optional[List[UsoEspeciePorComunidade]] = Field(None, description="Detailed usage by community (Clarificação Q1, Q3)")


class PeriodoEstudo(BaseModel):
    """Study period information (NEW)"""

    dataInicio: Optional[str] = Field(None, description="Study start date (ISO format)")
    dataFim: Optional[str] = Field(None, description="Study end date (ISO format)")


class ReferenceData(BaseModel):
    """Complete reference/article data structure (denormalized)"""

    # Basic article information
    ano: int = Field(..., ge=1900, le=2100, description="Publication year")
    ano_publicacao: Optional[int] = Field(None, ge=1900, le=2100, description="Publication year (alt name for compatibility)")
    titulo: str = Field(..., min_length=1, max_length=500, description="Article title")
    publicacao: Optional[str] = Field(None, description="Publication venue (journal, conference)")
    doi: Optional[str] = Field(None, description="Digital Object Identifier")
    autores: List[str] = Field(..., min_items=1, description="List of author names")
    resumo: Optional[str] = Field(None, description="Article abstract/summary")

    # Species data (embedded)
    especies: List[SpeciesData] = Field(default_factory=list, description="Plant species mentioned")

    # Use and methodology information
    tipoUso: Optional[str] = Field(None, description="Type of use (medicinal, alimentar, etc.)")
    tipo_de_uso: Optional[str] = Field(None, description="Type of use (alternate name for compatibility)")
    metodologia: Optional[str] = Field(None, description="Research methodology")

    # Geographic information (denormalized)
    pais: Optional[str] = Field(None, description="Country")
    estado: Optional[str] = Field(None, description="State/Province")
    municipio: Optional[str] = Field(None, description="Municipality")
    local: Optional[str] = Field(None, description="Specific location/community")
    bioma: Optional[str] = Field(None, description="Biome")

    # Study period (NEW)
    periodoEstudo: Optional[PeriodoEstudo] = Field(None, description="Study period information")

    # Document metadata
    status: Optional[str] = Field(None, description="Document status: rascunho or finalizado")
    data_processamento: Optional[str] = Field(None, description="Processing timestamp")


class ReferenceResponse(BaseModel):
    """Response model for a reference (with UUID ID)"""

    id: str = Field(..., alias="_id", description="UUID string ID")
    ano: int
    titulo: str
    publicacao: Optional[str] = None
    doi: Optional[str] = None
    autores: List[str]
    resumo: Optional[str] = None
    especies: List[SpeciesData] = Field(default_factory=list, description="Plant species mentioned")
    tipoUso: Optional[str] = None
    tipo_de_uso: Optional[str] = None
    metodologia: Optional[str] = None
    pais: Optional[str] = None
    estado: Optional[str] = None
    municipio: Optional[str] = None
    local: Optional[str] = None
    bioma: Optional[str] = None
    periodoEstudo: Optional[PeriodoEstudo] = None
    status: Optional[str] = None
    data_processamento: Optional[str] = None

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
