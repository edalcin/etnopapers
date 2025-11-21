"""
Article service layer for business logic
"""

import json
import logging
from typing import List, Optional, Dict, Any
from datetime import datetime
from database.connection import get_db

logger = logging.getLogger(__name__)


class ArticleService:
    """Service for article-related operations"""

    @staticmethod
    def create_article(
        titulo: str,
        ano_publicacao: int,
        autores: List[Dict[str, str]],
        doi: Optional[str] = None,
        resumo: Optional[str] = None,
        status: str = "rascunho",
    ) -> Dict[str, Any]:
        """
        Create a new article

        Args:
            titulo: Article title
            ano_publicacao: Publication year
            autores: List of author information
            doi: Digital Object Identifier
            resumo: Abstract/Summary
            status: Article status (rascunho or finalizado)

        Returns:
            Created article data with ID
        """
        db = get_db()

        try:
            autores_json = json.dumps(autores)

            query = """
                INSERT INTO ArtigosCientificos
                (titulo, doi, ano_publicacao, autores, resumo, status, data_processamento, data_ultima_modificacao)
                VALUES (?, ?, ?, ?, ?, ?, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
            """

            with db.get_cursor() as cursor:
                cursor.execute(
                    query,
                    (titulo, doi, ano_publicacao, autores_json, resumo, status),
                )
                article_id = cursor.lastrowid

            logger.info(f"Article created: {article_id} - {titulo}")
            return ArticleService.get_article_by_id(article_id)

        except Exception as e:
            logger.error(f"Error creating article: {e}")
            raise

    @staticmethod
    def get_article_by_id(article_id: int) -> Dict[str, Any]:
        """Get article by ID"""
        db = get_db()

        query = """
            SELECT id, titulo, doi, ano_publicacao, autores, resumo,
                   status, editado_manualmente, data_processamento, data_ultima_modificacao
            FROM ArtigosCientificos
            WHERE id = ?
        """

        result = db.execute_query(query, (article_id,))

        if not result:
            return None

        row = result[0]
        return {
            "id": row[0],
            "titulo": row[1],
            "doi": row[2],
            "ano_publicacao": row[3],
            "autores": json.loads(row[4]),
            "resumo": row[5],
            "status": row[6],
            "editado_manualmente": bool(row[7]),
            "data_processamento": row[8],
            "data_ultima_modificacao": row[9],
        }

    @staticmethod
    def list_articles(
        page: int = 1,
        page_size: int = 50,
        status: Optional[str] = None,
        search: Optional[str] = None,
    ) -> Dict[str, Any]:
        """
        List articles with pagination and filters

        Args:
            page: Page number (1-based)
            page_size: Items per page
            status: Filter by status (rascunho or finalizado)
            search: Search text in title or authors

        Returns:
            Paginated list of articles
        """
        db = get_db()

        # Build WHERE clause
        where_clauses = []
        params = []

        if status:
            where_clauses.append("status = ?")
            params.append(status)

        if search:
            where_clauses.append("(titulo LIKE ? OR autores LIKE ?)")
            search_term = f"%{search}%"
            params.extend([search_term, search_term])

        where_clause = " AND ".join(where_clauses) if where_clauses else "1=1"

        # Get total count
        count_query = f"SELECT COUNT(*) FROM ArtigosCientificos WHERE {where_clause}"
        count_result = db.execute_query(count_query, tuple(params))
        total = count_result[0][0] if count_result else 0

        # Get paginated results
        offset = (page - 1) * page_size
        list_query = f"""
            SELECT id, titulo, doi, ano_publicacao, autores, resumo,
                   status, editado_manualmente, data_processamento, data_ultima_modificacao
            FROM ArtigosCientificos
            WHERE {where_clause}
            ORDER BY data_processamento DESC
            LIMIT ? OFFSET ?
        """

        results = db.execute_query(list_query, tuple(params + [page_size, offset]))

        articles = []
        for row in results:
            articles.append(
                {
                    "id": row[0],
                    "titulo": row[1],
                    "doi": row[2],
                    "ano_publicacao": row[3],
                    "autores": json.loads(row[4]),
                    "resumo": row[5],
                    "status": row[6],
                    "editado_manualmente": bool(row[7]),
                    "data_processamento": row[8],
                    "data_ultima_modificacao": row[9],
                }
            )

        return {
            "total": total,
            "page": page,
            "page_size": page_size,
            "items": articles,
        }

    @staticmethod
    def update_article(
        article_id: int, **kwargs
    ) -> Dict[str, Any]:
        """
        Update an article

        Args:
            article_id: ID of article to update
            **kwargs: Fields to update

        Returns:
            Updated article data
        """
        db = get_db()

        # Build update query
        allowed_fields = [
            "titulo",
            "doi",
            "ano_publicacao",
            "autores",
            "resumo",
            "status",
        ]
        updates = []
        params = []

        for field, value in kwargs.items():
            if field in allowed_fields:
                if field == "autores" and isinstance(value, list):
                    value = json.dumps(value)
                updates.append(f"{field} = ?")
                params.append(value)

        if not updates:
            return ArticleService.get_article_by_id(article_id)

        params.append(article_id)
        update_query = f"""
            UPDATE ArtigosCientificos
            SET {', '.join(updates)}, data_ultima_modificacao = CURRENT_TIMESTAMP
            WHERE id = ?
        """

        db.execute_update(update_query, tuple(params))
        logger.info(f"Article updated: {article_id}")

        return ArticleService.get_article_by_id(article_id)

    @staticmethod
    def delete_article(article_id: int) -> bool:
        """
        Delete an article

        Args:
            article_id: ID of article to delete

        Returns:
            True if deleted, False if not found
        """
        db = get_db()

        query = "DELETE FROM ArtigosCientificos WHERE id = ?"
        rows_affected = db.execute_update(query, (article_id,))

        if rows_affected > 0:
            logger.info(f"Article deleted: {article_id}")
            return True
        return False
