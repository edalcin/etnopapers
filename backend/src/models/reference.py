from typing import List, Optional
from datetime import datetime
from pydantic import BaseModel, Field
from .species import SpeciesData

class ReferenceBase(BaseModel):
    """Base Reference model with all ethnobotanical fields"""
    titulo: str = Field(..., description="Título do artigo")
    autores: List[str] = Field(default_factory=list, description="Lista de autores")
    ano: Optional[int] = Field(None, description="Ano de publicação")
    publicacao: Optional[str] = Field(None, description="Local/revista de publicação")
    doi: Optional[str] = Field(None, unique=True, description="Digital Object Identifier")
    resumo: Optional[str] = Field(None, description="Resumo/abstract")
    
    # Ethnobotanical data
    especies: List[SpeciesData] = Field(default_factory=list, description="Espécies mencionadas")
    tipo_de_uso: Optional[str] = Field(None, description="Tipo de uso (medicinal, alimentar, etc.)")
    metodologia: Optional[str] = Field(None, description="Metodologia de pesquisa")
    
    # Geographic data
    pais: Optional[str] = Field(None, description="País de estudo")
    estado: Optional[str] = Field(None, description="Estado/província")
    municipio: Optional[str] = Field(None, description="Município")
    local: Optional[str] = Field(None, description="Local específico/comunidade")
    bioma: Optional[str] = Field(None, description="Bioma")
    
    # Status
    status: str = Field(default="rascunho", description="Status do documento (rascunho, finalizado)")

class Reference(ReferenceBase):
    """Reference model with database metadata"""
    id: Optional[str] = Field(None, alias="_id", description="ID do MongoDB")
    criado_em: Optional[datetime] = Field(default_factory=datetime.utcnow)
    atualizado_em: Optional[datetime] = Field(default_factory=datetime.utcnow)
    
    class Config:
        populate_by_name = True
        json_schema_extra = {
            "example": {
                "titulo": "Uso de plantas medicinais no Sertão",
                "autores": ["Silva, J.", "Santos, M."],
                "ano": 2010,
                "publicacao": "Revista Botânica",
                "doi": "10.1234/test",
                "especies": [
                    {
                        "vernacular": "maçanilha",
                        "nomeCientifico": "Chamomilla recutita"
                    }
                ],
                "tipo_de_uso": "medicinal",
                "metodologia": "entrevistas",
                "pais": "Brasil",
                "estado": "SC",
                "municipio": "Florianópolis",
                "local": "Sertão do Ribeirão",
                "bioma": "Mata Atlântica",
                "status": "finalizado"
            }
        }

class ReferenceCreate(ReferenceBase):
    """Model for creating new references"""
    pass

class ReferenceUpdate(BaseModel):
    """Model for updating references"""
    titulo: Optional[str] = None
    autores: Optional[List[str]] = None
    ano: Optional[int] = None
    publicacao: Optional[str] = None
    doi: Optional[str] = None
    resumo: Optional[str] = None
    especies: Optional[List[SpeciesData]] = None
    tipo_de_uso: Optional[str] = None
    metodologia: Optional[str] = None
    pais: Optional[str] = None
    estado: Optional[str] = None
    municipio: Optional[str] = None
    local: Optional[str] = None
    bioma: Optional[str] = None
    status: Optional[str] = None
