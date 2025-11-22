"""
References/Articles router for FastAPI

Handles all reference-related endpoints:
- POST /api/referencias - Create reference
- GET /api/referencias - List references with pagination
- GET /api/referencias/{id} - Get specific reference
- PUT /api/referencias/{id} - Update reference
- DELETE /api/referencias/{id} - Delete reference
- POST /api/referencias/check-duplicate - Check for duplicates
- GET /api/referencias/stats - Get statistics
"""

import logging
from typing import Optional

from fastapi import APIRouter
from fastapi import HTTPException
from fastapi import Query

from backend.models.article import ReferenceData
from backend.models.article import ReferenceListResponse
from backend.models.article import ReferenceResponse
from backend.services.article_service import ArticleService

logger = logging.getLogger(__name__)

router = APIRouter(prefix="/api/referencias", tags=["references"])


@router.post("", response_model=ReferenceResponse, status_code=201)
async def create_reference(reference: ReferenceData):
    """
    Create a new reference/article

    - **ano**: Publication year (required, 1900-2100)
    - **titulo**: Article title (required)
    - **autores**: List of author names (required)
    - **publicacao**: Publication venue (journal, conference, etc.)
    - **resumo**: Abstract (optional)
    - **doi**: Digital Object Identifier (optional)
    - **especies**: Array of species with vernacular and nomeCientifico
    - **tipo_de_uso**: Type of use (medicinal, alimentar, etc.)
    - **metodologia**: Research methodology
    - **pais**, **estado**, **municipio**, **local**, **bioma**: Geographic info
    - **status**: rascunho (draft) or finalizado (completed) - default: rascunho
    """
    try:
        result = ArticleService.create_article(
            ano=reference.ano,
            titulo=reference.titulo,
            autores=reference.autores,
            publicacao=reference.publicacao,
            resumo=reference.resumo,
            doi=reference.doi,
            especies=[e.dict() for e in reference.especies],
            tipo_de_uso=reference.tipo_de_uso,
            metodologia=reference.metodologia,
            pais=reference.pais,
            estado=reference.estado,
            municipio=reference.municipio,
            local=reference.local,
            bioma=reference.bioma,
            status=reference.status,
        )
        return result
    except Exception as e:
        logger.error(f"Error creating reference: {e}")
        raise HTTPException(status_code=500, detail="Error creating reference")


@router.get("", response_model=ReferenceListResponse)
async def list_references(
    page: int = Query(1, ge=1),
    page_size: int = Query(50, ge=1, le=500),
    status: Optional[str] = Query(None),
    search: Optional[str] = Query(None),
    ano: Optional[int] = Query(None, ge=1900, le=2100),
    pais: Optional[str] = Query(None),
):
    """
    List references with pagination and filtering

    - **page**: Page number (1-based), default: 1
    - **page_size**: Items per page (1-500), default: 50
    - **status**: Filter by status (rascunho or finalizado), optional
    - **search**: Search text in title or authors, optional
    - **ano**: Filter by publication year, optional
    - **pais**: Filter by country, optional
    """
    try:
        result = ArticleService.list_articles(
            page=page,
            page_size=page_size,
            status=status,
            search=search,
            ano=ano,
            pais=pais,
        )
        return result
    except Exception as e:
        logger.error(f"Error listing references: {e}")
        raise HTTPException(status_code=500, detail="Error listing references")


@router.get("/{reference_id}", response_model=ReferenceResponse)
async def get_reference(reference_id: str):
    """
    Get a specific reference by ID

    - **reference_id**: MongoDB ObjectId as string
    """
    try:
        result = ArticleService.get_article_by_id(reference_id)
        if not result:
            raise HTTPException(status_code=404, detail="Reference not found")
        return result
    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Error fetching reference: {e}")
        raise HTTPException(status_code=500, detail="Error fetching reference")


@router.put("/{reference_id}", response_model=ReferenceResponse)
async def update_reference(reference_id: str, reference: ReferenceData):
    """
    Update a reference/article

    - **reference_id**: MongoDB ObjectId as string
    - **reference**: Updated reference data
    """
    try:
        result = ArticleService.update_article(
            reference_id,
            ano=reference.ano,
            titulo=reference.titulo,
            autores=reference.autores,
            publicacao=reference.publicacao,
            resumo=reference.resumo,
            doi=reference.doi,
            especies=[e.dict() for e in reference.especies],
            tipo_de_uso=reference.tipo_de_uso,
            metodologia=reference.metodologia,
            pais=reference.pais,
            estado=reference.estado,
            municipio=reference.municipio,
            local=reference.local,
            bioma=reference.bioma,
            status=reference.status,
        )
        if not result:
            raise HTTPException(status_code=404, detail="Reference not found")
        return result
    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Error updating reference: {e}")
        raise HTTPException(status_code=500, detail="Error updating reference")


@router.delete("/{reference_id}", status_code=204)
async def delete_reference(reference_id: str):
    """
    Delete a reference/article

    - **reference_id**: MongoDB ObjectId as string
    """
    try:
        success = ArticleService.delete_article(reference_id)
        if not success:
            raise HTTPException(status_code=404, detail="Reference not found")
        return None
    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Error deleting reference: {e}")
        raise HTTPException(status_code=500, detail="Error deleting reference")


@router.get("/doi/{doi}", response_model=ReferenceResponse)
async def get_by_doi(doi: str):
    """
    Get reference by DOI

    - **doi**: Digital Object Identifier
    """
    try:
        result = ArticleService.get_by_doi(doi)
        if not result:
            raise HTTPException(status_code=404, detail="Reference with this DOI not found")
        return result
    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Error fetching by DOI: {e}")
        raise HTTPException(status_code=500, detail="Error fetching reference")


@router.get("/stats/summary")
async def get_statistics():
    """
    Get statistics about references in the database
    """
    try:
        result = ArticleService.get_statistics()
        return result
    except Exception as e:
        logger.error(f"Error getting statistics: {e}")
        raise HTTPException(status_code=500, detail="Error getting statistics")
