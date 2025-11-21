"""
Unit tests for article service and endpoints
"""

import pytest
from services import ArticleService


class TestArticleService:
    """Tests for ArticleService"""

    def test_create_article(self, db):
        """Test creating an article"""
        result = ArticleService.create_article(
            titulo="Test Article",
            ano_publicacao=2023,
            autores=[{"nome": "John", "sobrenome": "Doe"}],
            doi="10.1234/test",
            resumo="Test abstract",
        )

        assert result is not None
        assert result["id"] > 0
        assert result["titulo"] == "Test Article"
        assert result["ano_publicacao"] == 2023
        assert result["doi"] == "10.1234/test"

    def test_get_article_by_id(self, db):
        """Test retrieving an article by ID"""
        # Create article first
        created = ArticleService.create_article(
            titulo="Get Test",
            ano_publicacao=2023,
            autores=[{"nome": "Jane", "sobrenome": "Smith"}],
        )

        # Retrieve it
        result = ArticleService.get_article_by_id(created["id"])

        assert result is not None
        assert result["titulo"] == "Get Test"

    def test_get_nonexistent_article(self, db):
        """Test retrieving a non-existent article"""
        result = ArticleService.get_article_by_id(99999)
        assert result is None

    def test_list_articles(self, db):
        """Test listing articles"""
        # Create multiple articles
        for i in range(5):
            ArticleService.create_article(
                titulo=f"Article {i}",
                ano_publicacao=2023 + i,
                autores=[{"nome": f"Author{i}"}],
            )

        # List them
        result = ArticleService.list_articles(page=1, page_size=10)

        assert result["total"] == 5
        assert len(result["items"]) == 5
        assert result["page"] == 1

    def test_list_articles_pagination(self, db):
        """Test pagination"""
        # Create 15 articles
        for i in range(15):
            ArticleService.create_article(
                titulo=f"Article {i}",
                ano_publicacao=2023,
                autores=[{"nome": "Author"}],
            )

        # Test first page
        page1 = ArticleService.list_articles(page=1, page_size=10)
        assert len(page1["items"]) == 10
        assert page1["total"] == 15

        # Test second page
        page2 = ArticleService.list_articles(page=2, page_size=10)
        assert len(page2["items"]) == 5

    def test_update_article(self, db):
        """Test updating an article"""
        created = ArticleService.create_article(
            titulo="Original",
            ano_publicacao=2023,
            autores=[{"nome": "Author"}],
        )

        updated = ArticleService.update_article(
            created["id"],
            titulo="Updated Title",
            status="finalizado",
        )

        assert updated["titulo"] == "Updated Title"
        assert updated["status"] == "finalizado"

    def test_delete_article(self, db):
        """Test deleting an article"""
        created = ArticleService.create_article(
            titulo="To Delete",
            ano_publicacao=2023,
            autores=[{"nome": "Author"}],
        )

        # Delete it
        result = ArticleService.delete_article(created["id"])
        assert result is True

        # Verify it's gone
        found = ArticleService.get_article_by_id(created["id"])
        assert found is None

    def test_delete_nonexistent_article(self, db):
        """Test deleting a non-existent article"""
        result = ArticleService.delete_article(99999)
        assert result is False

    def test_search_articles(self, db):
        """Test searching articles"""
        ArticleService.create_article(
            titulo="Python Programming",
            ano_publicacao=2023,
            autores=[{"nome": "John"}],
        )

        ArticleService.create_article(
            titulo="Java Development",
            ano_publicacao=2023,
            autores=[{"nome": "Jane"}],
        )

        # Search for Python
        result = ArticleService.list_articles(search="Python")
        assert result["total"] == 1
        assert "Python" in result["items"][0]["titulo"]

    def test_filter_by_status(self, db):
        """Test filtering by status"""
        ArticleService.create_article(
            titulo="Draft Article",
            ano_publicacao=2023,
            autores=[{"nome": "Author"}],
            status="rascunho",
        )

        ArticleService.create_article(
            titulo="Final Article",
            ano_publicacao=2023,
            autores=[{"nome": "Author"}],
            status="finalizado",
        )

        # Filter by status
        result = ArticleService.list_articles(status="finalizado")
        assert result["total"] == 1
        assert result["items"][0]["status"] == "finalizado"
