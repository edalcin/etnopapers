from fastapi import APIRouter, HTTPException, Query
from typing import Optional, List
from ..models.reference import Reference, ReferenceCreate, ReferenceUpdate
from ..services.database_service import DatabaseService
from ..services.article_service import ArticleService
from ..services.search_service import SearchService

router = APIRouter()

@router.post("/articles", response_model=Reference)
async def create_article(article: ReferenceCreate):
    """Create new article"""
    # Validate article data
    is_valid, error_msg = ArticleService.validate_article_data(article)
    if not is_valid:
        raise HTTPException(status_code=400, detail=error_msg)
    
    # Check for duplicate by DOI
    if article.doi:
        existing = ArticleService.check_duplicate_by_doi(article.doi)
        if existing:
            raise HTTPException(
                status_code=409,
                detail={
                    "mensagem": "Artigo com este DOI já existe",
                    "artigo_existente_id": str(existing.id)
                }
            )
    
    # Check for fuzzy duplicates
    if article.autores and len(article.autores) > 0:
        duplicate_info = ArticleService.check_fuzzy_duplicate(
            article.titulo,
            article.ano or 0,
            article.autores[0]
        )
        
        if duplicate_info:
            # Return conflict response but don't block creation
            # Frontend can decide to merge or create new
            pass
    
    # Create article in database
    try:
        created_article = DatabaseService.create_article(article)
        return created_article
    except ValueError as e:
        raise HTTPException(status_code=400, detail=str(e))

@router.get("/articles", response_model=dict)
async def list_articles(
    limit: int = Query(10, ge=1, le=100),
    skip: int = Query(0, ge=0),
    sort_by: str = Query("_id"),
    order: str = Query("desc")
):
    """List articles with pagination"""
    sort_order = -1 if order == "desc" else 1
    articles, total = DatabaseService.list_articles(
        limit=limit,
        skip=skip,
        sort_by=sort_by,
        sort_order=sort_order
    )
    
    return {
        "artigos": articles,
        "total": total,
        "pagina": skip // limit + 1 if limit > 0 else 1,
        "limite": limit
    }

@router.get("/articles/{article_id}", response_model=Reference)
async def get_article(article_id: str):
    """Get article by ID"""
    article = DatabaseService.get_article(article_id)
    if not article:
        raise HTTPException(status_code=404, detail="Artigo não encontrado")
    return article

@router.put("/articles/{article_id}", response_model=Reference)
async def update_article(article_id: str, update_data: ReferenceUpdate):
    """Update article"""
    article = DatabaseService.update_article(article_id, update_data)
    if not article:
        raise HTTPException(status_code=404, detail="Artigo não encontrado")
    return article

@router.delete("/articles/{article_id}")
async def delete_article(article_id: str):
    """Delete article"""
    success = DatabaseService.delete_article(article_id)
    if not success:
        raise HTTPException(status_code=404, detail="Artigo não encontrado")
    return {"mensagem": "Artigo deletado com sucesso"}

@router.get("/articles/search", response_model=List[Reference])
async def search_articles(q: str = Query(..., min_length=2)):
    """Search articles by title, author, species, location"""
    articles = SearchService.search_articles(q)
    return articles

@router.get("/articles/filter/year")
async def filter_by_year(year: int = Query(...)):
    """Filter articles by publication year"""
    articles = SearchService.filter_by_year(year)
    return {"artigos": articles, "ano": year, "total": len(articles)}

@router.get("/articles/filter/country")
async def filter_by_country(country: str = Query(...)):
    """Filter articles by country"""
    articles = SearchService.filter_by_country(country)
    return {"artigos": articles, "pais": country, "total": len(articles)}

@router.get("/articles/filter/species")
async def filter_by_species(species: str = Query(...)):
    """Filter articles by species scientific name"""
    articles = SearchService.filter_by_species(species)
    return {"artigos": articles, "especie": species, "total": len(articles)}

@router.get("/articles/stats")
async def get_article_stats():
    """Get aggregated statistics about articles"""
    stats = SearchService.get_aggregated_stats()
    return stats
