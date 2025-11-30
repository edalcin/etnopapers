from typing import Optional
from pydantic import BaseModel, Field

class SpeciesData(BaseModel):
    """Species/plant data model"""
    vernacular: str = Field(..., description="Nome vernacular da espécie")
    nomeCientifico: str = Field(..., description="Nome científico da espécie")
    familia: Optional[str] = Field(None, description="Família botânica")
    validacao_status: Optional[str] = Field(None, description="Status da validação (validado, não_validado)")
    
    class Config:
        json_schema_extra = {
            "example": {
                "vernacular": "maçanilha",
                "nomeCientifico": "Chamomilla recutita",
                "familia": "Asteraceae",
                "validacao_status": "validado"
            }
        }
