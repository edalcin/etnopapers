"""
Tests for duplicate detection service
"""

import pytest
from services import ArticleService, DuplicateChecker


class TestDuplicateChecker:
    """Tests for DuplicateChecker service"""

    def test_check_by_doi_exact_match(self, db):
        """Test DOI duplicate detection - exact match"""
        # Create an article with DOI
        ArticleService.create_article(
            titulo="Original Article",
            ano_publicacao=2023,
            autores=[{"nome": "John"}],
            doi="10.1234/test.001",
        )

        # Check for duplicate with same DOI
        duplicate = DuplicateChecker.check_by_doi("10.1234/test.001")

        assert duplicate is not None
        assert duplicate["titulo"] == "Original Article"
        assert duplicate["doi"] == "10.1234/test.001"
        assert duplicate["tipo_duplicata"] == "doi"

    def test_check_by_doi_no_match(self, db):
        """Test DOI duplicate detection - no match"""
        duplicate = DuplicateChecker.check_by_doi("10.9999/nonexistent")
        assert duplicate is None

    def test_check_by_doi_missing_doi(self, db):
        """Test DOI check with missing DOI"""
        duplicate = DuplicateChecker.check_by_doi(None)
        assert duplicate is None

    def test_check_by_metadata_exact_title(self, db):
        """Test metadata duplicate detection - exact title match"""
        # Create an article
        ArticleService.create_article(
            titulo="Plant Uses in Traditional Medicine",
            ano_publicacao=2023,
            autores=[{"nome": "Alice", "sobrenome": "Smith"}],
        )

        # Check for duplicate with same title, year, and author
        duplicate = DuplicateChecker.check_by_metadata(
            titulo="Plant Uses in Traditional Medicine",
            ano_publicacao=2023,
            autores=[{"nome": "Alice"}],
        )

        assert duplicate is not None
        assert duplicate["titulo"] == "Plant Uses in Traditional Medicine"
        assert duplicate["tipo_duplicata"] == "metadata"

    def test_check_by_metadata_similar_title(self, db):
        """Test metadata duplicate detection - similar title"""
        # Create an article
        ArticleService.create_article(
            titulo="Ethnobotanical Study of the Amazon",
            ano_publicacao=2022,
            autores=[{"nome": "Bob"}],
        )

        # Check with slightly different title but same year and author
        duplicate = DuplicateChecker.check_by_metadata(
            titulo="Ethnobotanical Study of the Amazon Basin",
            ano_publicacao=2022,
            autores=[{"nome": "Bob"}],
        )

        assert duplicate is not None
        assert duplicate["ano_publicacao"] == 2022

    def test_check_by_metadata_different_year_no_match(self, db):
        """Test metadata duplicate detection - different year should not match"""
        # Create an article
        ArticleService.create_article(
            titulo="Common Title",
            ano_publicacao=2020,
            autores=[{"nome": "Author"}],
        )

        # Check with different year
        duplicate = DuplicateChecker.check_by_metadata(
            titulo="Common Title",
            ano_publicacao=2021,
            autores=[{"nome": "Author"}],
        )

        assert duplicate is None

    def test_check_duplicate_primary_strategy(self, db):
        """Test main check_duplicate method - DOI strategy"""
        ArticleService.create_article(
            titulo="Test Article",
            ano_publicacao=2023,
            autores=[{"nome": "Test"}],
            doi="10.1234/primary",
        )

        duplicate = DuplicateChecker.check_duplicate(
            titulo="Test Article",
            ano_publicacao=2023,
            autores=[{"nome": "Test"}],
            doi="10.1234/primary",
        )

        assert duplicate is not None
        assert duplicate["tipo_duplicata"] == "doi"

    def test_check_duplicate_secondary_strategy(self, db):
        """Test main check_duplicate method - metadata strategy"""
        ArticleService.create_article(
            titulo="Secondary Test",
            ano_publicacao=2023,
            autores=[{"nome": "Secondary"}],
        )

        duplicate = DuplicateChecker.check_duplicate(
            titulo="Secondary Test",
            ano_publicacao=2023,
            autores=[{"nome": "Secondary"}],
        )

        assert duplicate is not None
        assert duplicate["tipo_duplicata"] == "metadata"

    def test_check_duplicate_no_false_positives(self, db):
        """Test that different articles are not flagged as duplicates"""
        ArticleService.create_article(
            titulo="First Unique Article",
            ano_publicacao=2023,
            autores=[{"nome": "Author A"}],
        )

        duplicate = DuplicateChecker.check_duplicate(
            titulo="Second Unique Article",
            ano_publicacao=2023,
            autores=[{"nome": "Author B"}],
        )

        assert duplicate is None

    def test_get_similar_articles(self, db):
        """Test finding similar articles"""
        # Create several articles
        for i in range(3):
            ArticleService.create_article(
                titulo=f"Plant Conservation Studies {i}",
                ano_publicacao=2022 + i,
                autores=[{"nome": f"Author{i}"}],
            )

        # Find similar articles
        similar = DuplicateChecker.get_similar_articles(
            titulo="Plant Conservation in the Amazon",
            ano_publicacao=2023,
            limit=5,
        )

        assert len(similar) > 0
        # All results should be within reasonable year range
        for article in similar:
            assert 2020 <= article["ano_publicacao"] <= 2026

    def test_get_similar_articles_limit(self, db):
        """Test similarity search respects limit"""
        # Create 10 articles with similar titles
        for i in range(10):
            ArticleService.create_article(
                titulo=f"Plant Study {i}",
                ano_publicacao=2023,
                autores=[{"nome": "Author"}],
            )

        similar = DuplicateChecker.get_similar_articles(
            titulo="Plant Study",
            ano_publicacao=2023,
            limit=3,
        )

        assert len(similar) <= 3

    def test_extract_first_author(self, db):
        """Test first author extraction"""
        authors = [
            {"nome": "John", "sobrenome": "Doe"},
            {"nome": "Jane", "sobrenome": "Smith"},
        ]

        first = DuplicateChecker._extract_first_author(authors)
        assert first == "John"

    def test_extract_first_author_empty(self, db):
        """Test first author extraction with empty list"""
        first = DuplicateChecker._extract_first_author([])
        assert first is None

    def test_extract_first_author_no_nome(self, db):
        """Test first author extraction without nome field"""
        authors = [{"sobrenome": "Smith"}]
        first = DuplicateChecker._extract_first_author(authors)
        assert first == "Smith"
