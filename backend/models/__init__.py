"""Models package for Etnopapers backend"""

from .article import (
    ArticleRequest,
    ArticleResponse,
    ArticleListResponse,
    DadosEstudoRequest,
    DadosEstudoResponse,
    AuthorInfo,
)

__all__ = [
    "ArticleRequest",
    "ArticleResponse",
    "ArticleListResponse",
    "DadosEstudoRequest",
    "DadosEstudoResponse",
    "AuthorInfo",
]
