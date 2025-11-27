"""
Taxonomy validation service

Orchestrates GBIF and Tropicos APIs with in-memory caching
Updated 2025-11-27: Thread-safe in-memory cache with httpx
"""

import logging
import threading
import time
from typing import Any
from typing import Dict
from typing import Optional

import httpx

from backend.exceptions import TaxonomyTimeoutError

logger = logging.getLogger(__name__)


# Configuration
GBIF_API_URL = "https://api.gbif.org/v1/species"
TROPICOS_API_URL = "https://www.tropicos.org/services/json"
API_TIMEOUT = 5  # seconds
CACHE_TTL_DAYS = 30


class TaxonomyCache:
    """Thread-safe in-memory cache with TTL"""

    def __init__(self, ttl_days: int = CACHE_TTL_DAYS):
        self.cache: Dict[str, Dict[str, Any]] = {}
        self.ttl_seconds = ttl_days * 24 * 3600
        self.lock = threading.Lock()

    def get(self, key: str) -> Optional[Dict[str, Any]]:
        """Get value from cache if not expired"""
        with self.lock:
            if key not in self.cache:
                return None

            entry = self.cache[key]
            age = time.time() - entry["timestamp"]

            if age > self.ttl_seconds:
                del self.cache[key]
                logger.debug(f"Cache expired for {key}")
                return None

            logger.debug(f"Cache hit for {key} (age: {age/3600:.1f}h)")
            return entry["data"]

    def set(self, key: str, value: Dict[str, Any]) -> None:
        """Set value in cache with timestamp"""
        with self.lock:
            self.cache[key] = {
                "data": value,
                "timestamp": time.time(),
            }
            logger.debug(f"Cached {key}")

    def cleanup(self) -> None:
        """Remove expired entries"""
        with self.lock:
            now = time.time()
            expired_keys = [
                k
                for k, v in self.cache.items()
                if (now - v["timestamp"]) > self.ttl_seconds
            ]

            for key in expired_keys:
                del self.cache[key]

            if expired_keys:
                logger.info(f"Cleaned {len(expired_keys)} expired cache entries")

    def stats(self) -> Dict[str, Any]:
        """Get cache statistics"""
        with self.lock:
            total = len(self.cache)
            now = time.time()

            age_stats = []
            for entry in self.cache.values():
                age_hours = (now - entry["timestamp"]) / 3600
                age_stats.append(age_hours)

            avg_age = sum(age_stats) / len(age_stats) if age_stats else 0

            return {
                "total_cached": total,
                "average_age_hours": round(avg_age, 1),
                "ttl_days": CACHE_TTL_DAYS,
            }


class TaxonomyService:
    """
    Plant species taxonomy validation service

    Validates scientific names against:
    1. GBIF Species API (primary)
    2. Tropicos API (fallback)
    3. Local cache (30-day TTL)
    """

    def __init__(self):
        self.cache = TaxonomyCache()
        self.http_client = httpx.Client(timeout=API_TIMEOUT)
        logger.info("TaxonomyService initialized")

    def validate_species(self, scientific_name: str) -> Optional[Dict[str, Any]]:
        """
        Validate species scientific name

        Args:
            scientific_name: Scientific name to validate

        Returns:
            Dict with validation results or None
        """
        if not scientific_name or not scientific_name.strip():
            return None

        normalized_name = scientific_name.strip()

        # Check cache first
        cached = self.cache.get(normalized_name)
        if cached:
            cached["source"] = "cache"
            return cached

        # Try GBIF first
        try:
            result = self._validate_gbif(normalized_name)
            if result:
                self.cache.set(normalized_name, result)
                result["source"] = "gbif"
                return result
        except Exception as e:
            logger.warning(f"GBIF validation failed for {normalized_name}: {e}")

        # Fallback to Tropicos
        try:
            result = self._validate_tropicos(normalized_name)
            if result:
                self.cache.set(normalized_name, result)
                result["source"] = "tropicos"
                return result
        except Exception as e:
            logger.warning(f"Tropicos validation failed for {normalized_name}: {e}")

        logger.warning(f"Could not validate {normalized_name}")
        return None

    def _validate_gbif(self, scientific_name: str) -> Optional[Dict[str, Any]]:
        """Validate against GBIF Species API"""
        try:
            response = self.http_client.get(
                f"{GBIF_API_URL}/search",
                params={"q": scientific_name, "limit": 1},
            )
            response.raise_for_status()

            data = response.json()
            results = data.get("results", [])

            if not results:
                logger.debug(f"No GBIF results for {scientific_name}")
                return None

            match = results[0]

            if match.get("matchType") not in ["EXACT", "FUZZY", "HIGHERRANK"]:
                logger.debug(f"GBIF match type not acceptable")
                return None

            accepted_name = match.get("scientificName", scientific_name)
            family = match.get("family")
            is_exact = match.get("matchType") == "EXACT"

            logger.info(f"GBIF validated {scientific_name} → {accepted_name}")

            return {
                "accepted_name": accepted_name,
                "family": family,
                "confidence": "alta" if is_exact else "media",
                "validated": True,
            }

        except httpx.TimeoutException:
            raise TaxonomyTimeoutError()
        except Exception as e:
            logger.warning(f"GBIF error: {e}")
            return None

    def _validate_tropicos(self, scientific_name: str) -> Optional[Dict[str, Any]]:
        """Validate against Tropicos API"""
        try:
            response = self.http_client.get(
                f"{TROPICOS_API_URL}/NameSearch",
                params={"name": scientific_name, "type": "species", "output": "json"},
            )
            response.raise_for_status()

            data = response.json()

            if not isinstance(data, list) or len(data) == 0:
                logger.debug(f"No Tropicos results for {scientific_name}")
                return None

            match = data[0]
            accepted_name = match.get("name", scientific_name)
            family = match.get("family")

            logger.info(f"Tropicos validated {scientific_name} → {accepted_name}")

            return {
                "accepted_name": accepted_name,
                "family": family,
                "confidence": "media",
                "validated": True,
            }

        except httpx.TimeoutException:
            raise TaxonomyTimeoutError()
        except Exception as e:
            logger.warning(f"Tropicos error: {e}")
            return None

    def get_family(self, scientific_name: str) -> Optional[str]:
        """Get family for species"""
        result = self.validate_species(scientific_name)
        return result.get("family") if result else None

    def get_accepted_name(self, scientific_name: str) -> Optional[str]:
        """Get standardized scientific name"""
        result = self.validate_species(scientific_name)
        return result.get("accepted_name", scientific_name) if result else scientific_name

    def cache_stats(self) -> Dict[str, Any]:
        """Get cache statistics"""
        return self.cache.stats()

    def cleanup_cache(self) -> None:
        """Manually trigger cache cleanup"""
        self.cache.cleanup()

    def __del__(self):
        """Cleanup on deletion"""
        try:
            self.http_client.close()
        except:
            pass
