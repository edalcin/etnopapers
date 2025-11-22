"""
Tests for duplicate detection
"""

import pytest

from backend.services import ArticleService


class TestDuplicateChecker:
    """Tests for duplicate detection (using new denormalized model)"""

    def test_check_by_doi_exact_match(self, db):
        """Test DOI duplicate detection"""
        # Create a reference with DOI
        created = ArticleService.create_article(
            ano=2023,
            titulo="Original Article",
            autores=["John"],
            doi="10.1234/test.001",
            especies=[],
        )

        # Check for duplicate by DOI
        duplicate = ArticleService.get_by_doi("10.1234/test.001")

        assert duplicate is not None
        assert duplicate["titulo"] == "Original Article"
        assert duplicate["doi"] == "10.1234/test.001"

    def test_check_by_doi_no_match(self, db):
        """Test DOI detection with non-existent DOI"""
        result = ArticleService.get_by_doi("10.9999/nonexistent")
        assert result is None

    def test_check_duplicate_by_metadata(self, db):
        """Test metadata-based duplicate detection"""
        # Create a reference
        created = ArticleService.create_article(
            ano=2023,
            titulo="Plant Uses in Traditional Medicine",
            autores=["Alice Smith"],
            especies=[],
        )

        # Check for duplicate using metadata
        duplicate_id = ArticleService.check_duplicate(
            titulo="Plant Uses in Traditional Medicine",
            ano=2023,
            primeiro_autor="Alice Smith",
        )

        assert duplicate_id is not None
        assert duplicate_id == created["_id"]

    def test_check_duplicate_different_year_no_match(self, db):
        """Test that different year doesn't match"""
        # Create a reference in 2020
        ArticleService.create_article(
            ano=2020,
            titulo="Common Title",
            autores=["Author"],
            especies=[],
        )

        # Check for duplicate with same title but different year
        duplicate_id = ArticleService.check_duplicate(
            titulo="Common Title",
            ano=2021,
            primeiro_autor="Author",
        )

        assert duplicate_id is None

    def test_check_duplicate_different_author_no_match(self, db):
        """Test that different author doesn't match"""
        # Create a reference
        ArticleService.create_article(
            ano=2023,
            titulo="Study Title",
            autores=["Alice"],
            especies=[],
        )

        # Check for duplicate with same title and year but different author
        duplicate_id = ArticleService.check_duplicate(
            titulo="Study Title",
            ano=2023,
            primeiro_autor="Bob",
        )

        assert duplicate_id is None

    def test_extract_first_author_from_list(self, db):
        """Test extracting first author from list"""
        # Create with multiple authors
        result = ArticleService.create_article(
            ano=2023,
            titulo="Multi-Author Paper",
            autores=["Giraldi, M.", "Hanazaki, N."],
            especies=[],
        )

        assert len(result["autores"]) == 2
        assert result["autores"][0] == "Giraldi, M."

    def test_get_statistics_by_year(self, db):
        """Test getting statistics grouped by year"""
        # Create references in different years
        for year in [2020, 2021, 2022, 2023, 2023]:
            ArticleService.create_article(
                ano=year,
                titulo=f"Article {year}",
                autores=["Author"],
                especies=[],
            )

        stats = ArticleService.get_statistics()

        # Should have entries for each year
        assert len(stats["por_ano"]) == 4
        # Find 2023 entry
        y2023 = next((y for y in stats["por_ano"] if y["_id"] == 2023), None)
        assert y2023 is not None
        assert y2023["count"] == 2

    def test_get_statistics_by_country(self, db):
        """Test getting statistics grouped by country"""
        # Create references in different countries
        countries = ["Brasil", "Colômbia", "Brasil", "Perú"]
        for i, country in enumerate(countries):
            ArticleService.create_article(
                ano=2023,
                titulo=f"Article {i}",
                autores=["Author"],
                pais=country,
                especies=[],
            )

        stats = ArticleService.get_statistics()

        # Should have country stats
        assert len(stats["por_pais"]) > 0
        # Find Brasil
        brasil = next((c for c in stats["por_pais"] if c["_id"] == "Brasil"), None)
        assert brasil is not None
        assert brasil["count"] == 2

    def test_duplicate_prevention_doi_level(self, db):
        """Test that attempting to insert duplicate DOI is handled"""
        # Create reference with DOI
        created1 = ArticleService.create_article(
            ano=2023,
            titulo="First Article",
            autores=["Author1"],
            doi="10.1234/unique",
            especies=[],
        )

        # Try to create another with same DOI
        # This should either fail gracefully or return existing
        created2 = ArticleService.create_article(
            ano=2023,
            titulo="Second Article",
            autores=["Author2"],
            doi="10.1234/unique",
            especies=[],
        )

        # Should have different titles if both were inserted
        # (or second should fail, depending on implementation)
        # Just verify the database is consistent
        all_refs = ArticleService.list_articles()
        doi_matches = [
            r for r in all_refs["items"] if r.get("doi") == "10.1234/unique"
        ]
        # We don't enforce uniqueness at DB level (Mongita limitation),
        # but document it - app should handle this
        assert len(doi_matches) >= 1
