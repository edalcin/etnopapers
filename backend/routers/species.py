"""
Species router for FastAPI

Handles plant species endpoints:
- POST /api/species/validate - Validate a single species
- POST /api/species/validate-bulk - Validate multiple species
- GET /api/species/cache-stats - Get taxonomy cache statistics
- POST /api/species/clear-cache - Clear taxonomy cache
"""

import logging
import asyncio
from fastapi import APIRouter, HTTPException
from models.species import (
    SpeciesValidationRequest,
    SpeciesValidationResponse,
    BulkSpeciesValidationRequest,
    BulkSpeciesValidationResponse,
    TaxonomyCacheStats,
)
from services.taxonomy_service import TaxonomyService

logger = logging.getLogger(__name__)

router = APIRouter(prefix="/api/species", tags=["species"])


@router.post("/validate", response_model=SpeciesValidationResponse)
async def validate_species(request: SpeciesValidationRequest):
    """
    Validate a plant species name

    Uses multi-source validation:
    1. GBIF Species API (primary)
    2. Tropicos API (fallback)

    Returns validated species information with:
    - Scientific name (accepted current name)
    - Botanical family
    - Validation status (validado, nao_validado, ambiguo)
    - Source API (GBIF or Tropicos)
    """
    try:
        if not request.nome_cientifico or len(request.nome_cientifico.strip()) == 0:
            raise HTTPException(
                status_code=400, detail="Nome científico não pode estar vazio"
            )

        result = await TaxonomyService.validate_species(request.nome_cientifico)

        if not result:
            return {
                "nome_cientifico": request.nome_cientifico,
                "status_validacao": "nao_validado",
                "fonte_validacao": None,
            }

        return result

    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Error validating species: {e}")
        raise HTTPException(
            status_code=500, detail="Erro ao validar espécie"
        )


@router.post("/validate-bulk", response_model=BulkSpeciesValidationResponse)
async def validate_multiple_species(request: BulkSpeciesValidationRequest):
    """
    Validate multiple plant species in parallel

    Returns:
    - total: Total number of species submitted
    - validados: Number successfully validated
    - nao_validados: Number that could not be validated
    - resultados: List of validation results
    """
    try:
        if not request.species or len(request.species) == 0:
            raise HTTPException(
                status_code=400, detail="Lista de espécies vazia"
            )

        # Limit to reasonable batch size
        species_list = request.species[:100]

        results = await TaxonomyService.validate_multiple_species(species_list)

        validados = sum(
            1 for r in results if r.get("status_validacao") == "validado"
        )
        nao_validados = len(results) - validados

        return {
            "total": len(species_list),
            "validados": validados,
            "nao_validados": nao_validados,
            "resultados": results,
        }

    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Error validating multiple species: {e}")
        raise HTTPException(
            status_code=500, detail="Erro ao validar espécies"
        )


@router.get("/cache-stats", response_model=TaxonomyCacheStats)
async def get_cache_stats():
    """Get taxonomy cache statistics"""
    try:
        stats = TaxonomyService.get_cache_stats()
        return stats
    except Exception as e:
        logger.error(f"Error getting cache stats: {e}")
        raise HTTPException(
            status_code=500, detail="Erro ao obter estatísticas de cache"
        )


@router.post("/clear-cache")
async def clear_taxonomy_cache():
    """
    Clear all taxonomy cache

    This removes all cached species validations and resets the taxonomy service.
    Use with caution as this will require re-validation of all species.
    """
    try:
        TaxonomyService.clear_cache()
        logger.info("Taxonomy cache cleared by user request")
        return {
            "message": "Cache de taxonomia foi limpo com sucesso",
            "status": "cleared",
        }
    except Exception as e:
        logger.error(f"Error clearing cache: {e}")
        raise HTTPException(
            status_code=500, detail="Erro ao limpar cache"
        )


@router.post("/clear-expired")
async def clear_expired_cache():
    """Clear only expired entries from taxonomy cache"""
    try:
        TaxonomyService.clear_expired_cache()
        stats = TaxonomyService.get_cache_stats()
        logger.info("Expired taxonomy entries cleared")
        return {
            "message": "Entradas expiradas foram removidas",
            "status": "cleared",
            "stats": stats,
        }
    except Exception as e:
        logger.error(f"Error clearing expired cache: {e}")
        raise HTTPException(
            status_code=500, detail="Erro ao limpar cache expirado"
        )
