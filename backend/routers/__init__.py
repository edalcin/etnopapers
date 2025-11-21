"""Routers package for Etnopapers backend"""

from .articles import router as articles_router
from .species import router as species_router

__all__ = ["articles_router", "species_router"]
