"""Routers package for Etnopapers backend"""

from .articles import router as articles_router
from .species import router as species_router
from .database import router as database_router

__all__ = ["articles_router", "species_router", "database_router"]
