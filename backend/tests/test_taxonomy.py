"""
Tests for taxonomy validation service
"""

import asyncio

import pytest

from backend.services.taxonomy_service import TaxonomyService


class TestTaxonomyService:
    """Tests for TaxonomyService"""

    def test_cache_stats(self, db):
        """Test getting cache statistics"""
        # Clear cache first
        TaxonomyService.clear_cache()

        stats = TaxonomyService.get_cache_stats()

        assert "total_entries" in stats
        assert "valid_entries" in stats
        assert "expired_entries" in stats
        assert "cache_ttl_days" in stats
        assert stats["total_entries"] == 0

    def test_cache_clear(self, db):
        """Test clearing cache"""
        # Add something to cache manually
        TaxonomyService._cache["test"] = {"nome_cientifico": "Test"}
        TaxonomyService._cache_timestamps["test"] = "2024-01-01T00:00:00"

        assert len(TaxonomyService._cache) > 0

        # Clear cache
        TaxonomyService.clear_cache()

        assert len(TaxonomyService._cache) == 0
        assert len(TaxonomyService._cache_timestamps) == 0

    @pytest.mark.asyncio
    async def test_validate_species_not_found(self, db):
        """Test validating a species that doesn't exist"""
        # Clear cache
        TaxonomyService.clear_cache()

        # This species name probably doesn't exist
        result = await TaxonomyService.validate_species("Xxxxx yyyyyyy zzzzzz")

        assert result is not None
        assert result["status_validacao"] == "nao_validado"

    def test_extract_first_word(self, db):
        """Test extracting first word from species name"""
        name = "Areca catechu"
        first_word = name.split()[0]
        assert first_word == "Areca"

    def test_taxonomy_cache_valid(self, db):
        """Test cache validation logic"""
        from datetime import datetime

        TaxonomyService.clear_cache()

        # Add a recent entry
        test_key = "test_species"
        TaxonomyService._cache[test_key] = {"nome_cientifico": "Test"}
        TaxonomyService._cache_timestamps[test_key] = datetime.now().isoformat()

        assert TaxonomyService._is_cache_valid(test_key) is True

    def test_taxonomy_cache_expired(self, db):
        """Test expired cache detection"""
        from datetime import datetime, timedelta

        TaxonomyService.clear_cache()

        # Add an old entry (31 days ago)
        test_key = "old_species"
        TaxonomyService._cache[test_key] = {"nome_cientifico": "Old"}
        old_date = datetime.now() - timedelta(days=31)
        TaxonomyService._cache_timestamps[test_key] = old_date.isoformat()

        assert TaxonomyService._is_cache_valid(test_key) is False

    def test_clear_expired_cache(self, db):
        """Test clearing only expired entries"""
        from datetime import datetime, timedelta

        TaxonomyService.clear_cache()

        # Add one recent and one old entry
        TaxonomyService._cache["recent"] = {"nome": "Recent"}
        TaxonomyService._cache_timestamps["recent"] = datetime.now().isoformat()

        TaxonomyService._cache["old"] = {"nome": "Old"}
        old_date = datetime.now() - timedelta(days=31)
        TaxonomyService._cache_timestamps["old"] = old_date.isoformat()

        assert len(TaxonomyService._cache) == 2

        # Clear only expired
        TaxonomyService.clear_expired_cache()

        assert len(TaxonomyService._cache) == 1
        assert "recent" in TaxonomyService._cache
        assert "old" not in TaxonomyService._cache
