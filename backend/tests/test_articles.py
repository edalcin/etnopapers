"""
Unit tests for reference/article service
"""

import pytest

from backend.services import ArticleService


class TestArticleService:
    """Tests for ArticleService (denormalized model)"""

    def test_create_article(self, db):
        """Test creating a reference"""
        result = ArticleService.create_article(
            ano=2023,
            titulo="Test Article",
            autores=["John Doe", "Jane Smith"],
            doi="10.1234/test",
            resumo="Test abstract",
            especies=[
                {"vernacular": "test plant", "nomeCientifico": "Plantus testis"},
            ],
            pais="Brasil",
            estado="SP",
            status="rascunho",
        )

        assert result is not None
        assert "_id" in result
        assert isinstance(result["_id"], str)
        assert result["titulo"] == "Test Article"
        assert result["ano"] == 2023
        assert result["doi"] == "10.1234/test"
        assert len(result["autores"]) == 2

    def test_get_article_by_id(self, db):
        """Test retrieving a reference by ID"""
        # Create reference first
        created = ArticleService.create_article(
            ano=2023,
            titulo="Get Test",
            autores=["Jane Smith"],
            especies=[],
        )

        # Retrieve it
        result = ArticleService.get_article_by_id(created["_id"])

        assert result is not None
        assert result["titulo"] == "Get Test"
        assert result["ano"] == 2023

    def test_get_nonexistent_article(self, db):
        """Test retrieving a non-existent reference"""
        result = ArticleService.get_article_by_id("507f1f77bcf86cd799439011")
        assert result is None

    def test_list_articles(self, db):
        """Test listing references"""
        # Create multiple references
        for i in range(5):
            ArticleService.create_article(
                ano=2023 + i,
                titulo=f"Article {i}",
                autores=[f"Author{i}"],
                especies=[],
            )

        # List them
        result = ArticleService.list_articles(page=1, page_size=10)

        assert result["total"] == 5
        assert len(result["items"]) == 5
        assert result["page"] == 1

    def test_list_articles_pagination(self, db):
        """Test pagination"""
        # Create 15 references
        for i in range(15):
            ArticleService.create_article(
                ano=2023,
                titulo=f"Article {i}",
                autores=["Author"],
                especies=[],
            )

        # Test first page
        page1 = ArticleService.list_articles(page=1, page_size=10)
        assert len(page1["items"]) == 10
        assert page1["total"] == 15

        # Test second page
        page2 = ArticleService.list_articles(page=2, page_size=10)
        assert len(page2["items"]) == 5

    def test_update_article(self, db):
        """Test updating a reference"""
        created = ArticleService.create_article(
            ano=2023,
            titulo="Original",
            autores=["Author"],
            especies=[],
        )

        updated = ArticleService.update_article(
            created["_id"],
            titulo="Updated Title",
            status="finalizado",
        )

        assert updated["titulo"] == "Updated Title"
        assert updated["status"] == "finalizado"

    def test_delete_article(self, db):
        """Test deleting a reference"""
        created = ArticleService.create_article(
            ano=2023,
            titulo="To Delete",
            autores=["Author"],
            especies=[],
        )

        # Delete it
        result = ArticleService.delete_article(created["_id"])
        assert result is True

        # Verify it's gone
        found = ArticleService.get_article_by_id(created["_id"])
        assert found is None

    def test_delete_nonexistent_article(self, db):
        """Test deleting a non-existent reference"""
        result = ArticleService.delete_article("507f1f77bcf86cd799439011")
        assert result is False

    def test_search_articles(self, db):
        """Test searching references"""
        ArticleService.create_article(
            ano=2023,
            titulo="Python Programming",
            autores=["John"],
            especies=[],
        )

        ArticleService.create_article(
            ano=2023,
            titulo="Java Development",
            autores=["Jane"],
            especies=[],
        )

        # Search for Python
        result = ArticleService.list_articles(search="Python")
        assert result["total"] == 1
        assert "Python" in result["items"][0]["titulo"]

    def test_filter_by_status(self, db):
        """Test filtering by status"""
        ArticleService.create_article(
            ano=2023,
            titulo="Draft Article",
            autores=["Author"],
            especies=[],
            status="rascunho",
        )

        ArticleService.create_article(
            ano=2023,
            titulo="Finalized Article",
            autores=["Author"],
            especies=[],
            status="finalizado",
        )

        # Filter drafts
        drafts = ArticleService.list_articles(status="rascunho")
        assert drafts["total"] == 1
        assert drafts["items"][0]["status"] == "rascunho"

        # Filter finalized
        finalized = ArticleService.list_articles(status="finalizado")
        assert finalized["total"] == 1
        assert finalized["items"][0]["status"] == "finalizado"

    def test_filter_by_year(self, db):
        """Test filtering by year"""
        ArticleService.create_article(
            ano=2020,
            titulo="Old Article",
            autores=["Author"],
            especies=[],
        )

        ArticleService.create_article(
            ano=2023,
            titulo="Recent Article",
            autores=["Author"],
            especies=[],
        )

        # Filter by year
        result = ArticleService.list_articles(ano=2023)
        assert result["total"] == 1
        assert result["items"][0]["ano"] == 2023

    def test_filter_by_country(self, db):
        """Test filtering by country"""
        ArticleService.create_article(
            ano=2023,
            titulo="Brazil Article",
            autores=["Author"],
            pais="Brasil",
            especies=[],
        )

        ArticleService.create_article(
            ano=2023,
            titulo="Colombia Article",
            autores=["Author"],
            pais="Colômbia",
            especies=[],
        )

        # Filter by country
        result = ArticleService.list_articles(pais="Brasil")
        assert result["total"] == 1
        assert result["items"][0]["pais"] == "Brasil"

    def test_with_species(self, db):
        """Test creating reference with species"""
        result = ArticleService.create_article(
            ano=2023,
            titulo="Plants Article",
            autores=["Botanist"],
            especies=[
                {"vernacular": "maçanilha", "nomeCientifico": "Chamomilla recutita"},
                {"vernacular": "hortelã", "nomeCientifico": "Mentha sp."},
            ],
        )

        assert len(result["especies"]) == 2
        assert result["especies"][0]["vernacular"] == "maçanilha"

    def test_with_geographic_data(self, db):
        """Test creating reference with geographic data"""
        result = ArticleService.create_article(
            ano=2023,
            titulo="Geographic Article",
            autores=["Researcher"],
            pais="Brasil",
            estado="SC",
            municipio="Florianópolis",
            local="Sertão do Ribeirão",
            bioma="Mata Atlântica",
            especies=[],
        )

        assert result["pais"] == "Brasil"
        assert result["estado"] == "SC"
        assert result["municipio"] == "Florianópolis"
        assert result["local"] == "Sertão do Ribeirão"
        assert result["bioma"] == "Mata Atlântica"

    def test_get_statistics(self, db):
        """Test getting statistics"""
        # Create references in different years and countries
        for year in [2020, 2021, 2022, 2023]:
            ArticleService.create_article(
                ano=year,
                titulo=f"Article {year}",
                autores=["Author"],
                pais="Brasil",
                especies=[],
                status="finalizado",
            )

        ArticleService.create_article(
            ano=2023,
            titulo="Draft 2023",
            autores=["Author"],
            pais="Colômbia",
            especies=[],
            status="rascunho",
        )

        stats = ArticleService.get_statistics()

        assert stats["total_referencias"] == 5
        assert stats["finalizados"] == 4
        assert stats["rascunhos"] == 1
        assert len(stats["por_ano"]) > 0
