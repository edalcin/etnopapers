"""
Articles router for FastAPI

Handles all article-related endpoints:
- POST /api/articles - Create article
- GET /api/articles - List articles with pagination
- GET /api/articles/{id} - Get specific article
- PUT /api/articles/{id} - Update article
- DELETE /api/articles/{id} - Delete article
- POST /api/articles/check-duplicate - Check for duplicates
- GET /api/articles/{id}/similar - Get similar articles
"""

import logging
from typing import Optional

from fastapi import APIRouter
from fastapi import HTTPException
from fastapi import Query

from backend.models import ArticleListResponse
from backend.models import ArticleRequest
from backend.models import ArticleResponse
from backend.models import DuplicateArticleResponse
from backend.models import DuplicateCheckRequest
from backend.models import DuplicateCheckResponse
from backend.models import SimilarArticlesRequest
from backend.services import ArticleService
from backend.services import DuplicateChecker

logger = logging.getLogger(__name__)

router = APIRouter(prefix="/api/articles", tags=["articles"])


@router.post("", response_model=ArticleResponse, status_code=201)
async def create_article(article: ArticleRequest):
    """
    Create a new article

    - **titulo**: Article title (required)
    - **ano_publicacao**: Publication year (1900-2100, required)
    - **autores**: List of authors with name, surname, email (required)
    - **doi**: Digital Object Identifier (optional)
    - **resumo**: Article abstract (optional)
    - **status**: rascunho (draft) or finalizado (completed) - default: rascunho
    """
    try:
        result = ArticleService.create_article(
            titulo=article.titulo,
            ano_publicacao=article.ano_publicacao,
            autores=[a.dict() for a in article.autores],
            doi=article.doi,
            resumo=article.resumo,
            status=article.status,
        )
        return result
    except Exception as e:
        logger.error(f"Error creating article: {e}")
        raise HTTPException(status_code=500, detail="Error creating article")


@router.get("", response_model=ArticleListResponse)
async def list_articles(
    page: int = Query(1, ge=1),
    page_size: int = Query(50, ge=1, le=500),
    status: Optional[str] = Query(None),
    search: Optional[str] = Query(None),
):
    """
    List articles with pagination and filtering

    - **page**: Page number (1-based), default: 1
    - **page_size**: Items per page (1-500), default: 50
    - **status**: Filter by status (rascunho or finalizado), optional
    - **search**: Search text in title or authors, optional
    """
    try:
        result = ArticleService.list_articles(
            page=page,
            page_size=page_size,
            status=status,
            search=search,
        )
        return result
    except Exception as e:
        logger.error(f"Error listing articles: {e}")
        raise HTTPException(status_code=500, detail="Error listing articles")


@router.get("/{article_id}", response_model=ArticleResponse)
async def get_article(article_id: str):
    """Get a specific article by ID"""
    try:
        article = ArticleService.get_article_by_id(article_id)
        if not article:
            raise HTTPException(status_code=404, detail="Article not found")
        return article
    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Error getting article: {e}")
        raise HTTPException(status_code=500, detail="Error getting article")


@router.put("/{article_id}", response_model=ArticleResponse)
async def update_article(article_id: str, article: ArticleRequest):
    """
    Update an article

    - **titulo**: Article title
    - **ano_publicacao**: Publication year
    - **autores**: List of authors
    - **doi**: Digital Object Identifier
    - **resumo**: Article abstract
    - **status**: Article status
    """
    try:
        # Check if article exists
        existing = ArticleService.get_article_by_id(article_id)
        if not existing:
            raise HTTPException(status_code=404, detail="Article not found")

        result = ArticleService.update_article(
            article_id,
            titulo=article.titulo,
            ano_publicacao=article.ano_publicacao,
            autores=[a.dict() for a in article.autores],
            doi=article.doi,
            resumo=article.resumo,
            status=article.status,
        )
        return result
    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Error updating article: {e}")
        raise HTTPException(status_code=500, detail="Error updating article")


@router.delete("/{article_id}", status_code=204)
async def delete_article(article_id: str):
    """Delete an article by ID"""
    try:
        # Check if article exists
        existing = ArticleService.get_article_by_id(article_id)
        if not existing:
            raise HTTPException(status_code=404, detail="Article not found")

        ArticleService.delete_article(article_id)
        return None
    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Error deleting article: {e}")
        raise HTTPException(status_code=500, detail="Error deleting article")


@router.post("/check-duplicate", response_model=DuplicateCheckResponse)
async def check_duplicate(request: DuplicateCheckRequest):
    """
    Check if an article is a duplicate of an existing one

    Uses multi-strategy detection:
    1. Primary: DOI uniqueness
    2. Secondary: Title + Year + First Author

    Returns:
    - is_duplicate: True if duplicate found
    - duplicate: Details of the duplicate article if found
    - similar_articles: List of potentially similar articles for review
    """
    try:
        duplicate = DuplicateChecker.check_duplicate(
            titulo=request.titulo,
            ano_publicacao=request.ano_publicacao,
            autores=[a.dict() for a in request.autores],
            doi=request.doi,
        )

        if duplicate:
            similar = DuplicateChecker.get_similar_articles(
                titulo=request.titulo,
                ano_publicacao=request.ano_publicacao,
                limit=3,
            )

            return {
                "is_duplicate": True,
                "duplicate": duplicate,
                "message": f"Artigo duplicado detectado: {duplicate['titulo']} ({duplicate['ano_publicacao']})",
                "similar_articles": similar,
            }

        return {
            "is_duplicate": False,
            "duplicate": None,
            "message": "Nenhum artigo duplicado detectado",
            "similar_articles": None,
        }

    except Exception as e:
        logger.error(f"Error checking duplicate: {e}")
        raise HTTPException(status_code=500, detail="Error checking duplicate")


@router.get("/{article_id}/similar", response_model=list[DuplicateArticleResponse])
async def get_similar_articles(article_id: str, limit: int = Query(5, ge=1, le=20)):
    """
    Get articles similar to the specified article

    - **article_id**: ID of the article to find similar to
    - **limit**: Maximum number of similar articles (1-20), default: 5

    Returns list of similar articles ordered by relevance
    """
    try:
        article = ArticleService.get_article_by_id(article_id)
        if not article:
            raise HTTPException(status_code=404, detail="Article not found")

        similar = DuplicateChecker.get_similar_articles(
            titulo=article["titulo"],
            ano_publicacao=article["ano_publicacao"],
            limit=limit,
        )

        return similar

    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Error getting similar articles: {e}")
        raise HTTPException(status_code=500, detail="Error getting similar articles")
