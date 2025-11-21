"""Models package for Etnopapers backend"""

from .article import (
    ArticleRequest,
    ArticleResponse,
    ArticleListResponse,
    DadosEstudoRequest,
    DadosEstudoResponse,
    AuthorInfo,
)
from .duplicate import (
    DuplicateCheckRequest,
    DuplicateArticleResponse,
    DuplicateCheckResponse,
    SimilarArticlesRequest,
)
from .species import (
    SpeciesValidationRequest,
    SpeciesResponse,
    SpeciesValidationResponse,
    BulkSpeciesValidationRequest,
    BulkSpeciesValidationResponse,
    TaxonomyCacheStats,
)

__all__ = [
    "ArticleRequest",
    "ArticleResponse",
    "ArticleListResponse",
    "DadosEstudoRequest",
    "DadosEstudoResponse",
    "AuthorInfo",
    "DuplicateCheckRequest",
    "DuplicateArticleResponse",
    "DuplicateCheckResponse",
    "SimilarArticlesRequest",
    "SpeciesValidationRequest",
    "SpeciesResponse",
    "SpeciesValidationResponse",
    "BulkSpeciesValidationRequest",
    "BulkSpeciesValidationResponse",
    "TaxonomyCacheStats",
]
