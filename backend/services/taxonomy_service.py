"""
Taxonomy validation service

Orchestrates GBIF and Tropicos APIs with in-memory caching
"""

import logging
import json
import asyncio
from typing import Optional, Dict, Any
from pathlib import Path
from datetime import datetime, timedelta
from backend.config import settings
from backend.clients.gbif_client import GBIFClient
from backend.clients.tropicos_client import TropicosClient

logger = logging.getLogger(__name__)


class TaxonomyService:
    """Service for plant species taxonomy validation"""

    CACHE_FILE = Path("data/taxonomy_cache.json")
    CACHE_TTL_DAYS = settings.CACHE_TTL_DAYS
    _cache: Dict[str, Dict[str, Any]] = {}
    _cache_timestamps: Dict[str, str] = {}

    @classmethod
    def _load_cache(cls):
        """Load taxonomy cache from file"""
        try:
            if cls.CACHE_FILE.exists():
                with open(cls.CACHE_FILE, "r", encoding="utf-8") as f:
                    data = json.load(f)
                    cls._cache = data.get("cache", {})
                    cls._cache_timestamps = data.get("timestamps", {})
                    logger.info(f"Loaded taxonomy cache with {len(cls._cache)} entries")
            else:
                logger.info("No taxonomy cache file found, starting fresh")
        except Exception as e:
            logger.error(f"Error loading taxonomy cache: {e}")

    @classmethod
    def _save_cache(cls):
        """Save taxonomy cache to file"""
        try:
            cls.CACHE_FILE.parent.mkdir(parents=True, exist_ok=True)
            data = {
                "cache": cls._cache,
                "timestamps": cls._cache_timestamps,
                "saved_at": datetime.now().isoformat(),
            }
            with open(cls.CACHE_FILE, "w", encoding="utf-8") as f:
                json.dump(data, f, indent=2, ensure_ascii=False)
            logger.debug(f"Saved taxonomy cache with {len(cls._cache)} entries")
        except Exception as e:
            logger.error(f"Error saving taxonomy cache: {e}")

    @classmethod
    def _is_cache_valid(cls, key: str) -> bool:
        """Check if cache entry is still valid"""
        if key not in cls._cache_timestamps:
            return False

        try:
            cached_at = datetime.fromisoformat(cls._cache_timestamps[key])
            age = datetime.now() - cached_at
            return age < timedelta(days=cls.CACHE_TTL_DAYS)
        except Exception:
            return False

    @classmethod
    async def validate_species(
        cls, scientific_name: str
    ) -> Optional[Dict[str, Any]]:
        """
        Validate a plant species name

        Uses multi-strategy approach:
        1. Check cache (valid for 30 days)
        2. Query GBIF API
        3. Fallback to Tropicos API
        4. Mark as "não_validado" if all fail

        Args:
            scientific_name: Scientific name to validate

        Returns:
            Species data with validation status
        """
        if not scientific_name:
            return None

        key = scientific_name.lower().strip()

        # Load cache on first use
        if not cls._cache:
            cls._load_cache()

        # Check cache
        if key in cls._cache and cls._is_cache_valid(key):
            logger.debug(f"Cache hit for '{scientific_name}'")
            return cls._cache[key]

        # Try GBIF first
        logger.info(f"Validating '{scientific_name}' with GBIF...")
        result = await GBIFClient.search_species(scientific_name)

        if result:
            cls._cache[key] = result
            cls._cache_timestamps[key] = datetime.now().isoformat()
            cls._save_cache()
            return result

        # Fallback to Tropicos
        logger.info(f"GBIF failed, trying Tropicos for '{scientific_name}'...")
        result = await TropicosClient.search_species(scientific_name)

        if result:
            cls._cache[key] = result
            cls._cache_timestamps[key] = datetime.now().isoformat()
            cls._save_cache()
            return result

        # All APIs failed
        logger.warning(f"Could not validate '{scientific_name}' with any API")
        result = {
            "nome_cientifico": scientific_name,
            "status_validacao": "nao_validado",
            "fonte_validacao": None,
            "message": "Não foi possível validar com as APIs disponíveis",
        }

        cls._cache[key] = result
        cls._cache_timestamps[key] = datetime.now().isoformat()
        cls._save_cache()

        return result

    @classmethod
    async def validate_multiple_species(
        cls, species_list: list[str]
    ) -> list[Dict[str, Any]]:
        """
        Validate multiple species in parallel

        Args:
            species_list: List of scientific names

        Returns:
            List of validation results
        """
        tasks = [cls.validate_species(name) for name in species_list]
        results = await asyncio.gather(*tasks, return_exceptions=True)

        return [r for r in results if r is not None and not isinstance(r, Exception)]

    @classmethod
    def get_cache_stats(cls) -> Dict[str, Any]:
        """Get cache statistics"""
        if not cls._cache:
            cls._load_cache()

        valid_count = sum(
            1 for key in cls._cache if cls._is_cache_valid(key)
        )

        return {
            "total_entries": len(cls._cache),
            "valid_entries": valid_count,
            "expired_entries": len(cls._cache) - valid_count,
            "cache_ttl_days": cls.CACHE_TTL_DAYS,
        }

    @classmethod
    def clear_cache(cls):
        """Clear all cached taxonomy data"""
        cls._cache.clear()
        cls._cache_timestamps.clear()
        if cls.CACHE_FILE.exists():
            cls.CACHE_FILE.unlink()
        logger.info("Taxonomy cache cleared")

    @classmethod
    def clear_expired_cache(cls):
        """Remove expired entries from cache"""
        if not cls._cache:
            cls._load_cache()

        expired_keys = [
            key for key in cls._cache if not cls._is_cache_valid(key)
        ]

        for key in expired_keys:
            cls._cache.pop(key, None)
            cls._cache_timestamps.pop(key, None)

        if expired_keys:
            cls._save_cache()
            logger.info(f"Removed {len(expired_keys)} expired taxonomy entries")
