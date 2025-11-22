"""Models package for Etnopapers backend"""

from .article import (
    SpeciesData,
    ReferenceData,
    ReferenceResponse,
    ReferenceListResponse,
    ReferenceBulkCreateRequest,
    ReferenceBulkCreateResponse,
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
    # Reference/Article models (simplified)
    "SpeciesData",
    "ReferenceData",
    "ReferenceResponse",
    "ReferenceListResponse",
    "ReferenceBulkCreateRequest",
    "ReferenceBulkCreateResponse",
    # Species validation models
    "SpeciesValidationRequest",
    "SpeciesResponse",
    "SpeciesValidationResponse",
    "BulkSpeciesValidationRequest",
    "BulkSpeciesValidationResponse",
    "TaxonomyCacheStats",
]
