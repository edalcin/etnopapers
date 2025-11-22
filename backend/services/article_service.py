"""
Article/Reference service layer for business logic

Uses MongoDB for document operations (via PyMongo client).
Simplified denormalized model with single "referencias" collection.
Uses UUID string IDs and camelCase field names.
"""

import logging
import uuid
from typing import Any
from typing import Dict
from typing import List
from typing import Optional

from backend.database.connection import get_db

logger = logging.getLogger(__name__)


class ArticleService:
    """Service for reference/article-related operations (simplified model)"""

    @staticmethod
    def create_article(
        ano: int,
        titulo: str,
        autores: List[str],
        publicacao: Optional[str] = None,
        resumo: Optional[str] = None,
        especies: Optional[List[Dict]] = None,
        tipoUso: Optional[str] = None,
        metodologia: Optional[str] = None,
        pais: Optional[str] = None,
        estado: Optional[str] = None,
        municipio: Optional[str] = None,
        local: Optional[str] = None,
        bioma: Optional[str] = None,
    ) -> Dict[str, Any]:
        """
        Create a new reference/article

        Args:
            ano: Publication year
            titulo: Article title
            autores: List of author names
            publicacao: Publication venue
            resumo: Abstract/summary
            especies: List of species dicts with vernacular and nomeCientifico
            tipoUso: Type of use
            metodologia: Research methodology
            pais: Country
            estado: State
            municipio: Municipality
            local: Specific location
            bioma: Biome

        Returns:
            Created document data with ID
        """
        db = get_db()

        try:
            # Generate UUID for document
            doc_id = str(uuid.uuid4())

            # Build document
            doc = {
                "_id": doc_id,
                "ano": ano,
                "titulo": titulo,
                "autores": autores,
                "publicacao": publicacao,
                "resumo": resumo,
                "especies": especies or [],
                "tipoUso": tipoUso,
                "metodologia": metodologia,
                "pais": pais,
                "estado": estado,
                "municipio": municipio,
                "local": local,
                "bioma": bioma,
            }

            # Insert into collection
            collection = db.get_collection("referencias")
            result = collection.insert_one(doc)
            article_id = str(result.inserted_id)

            logger.info(f"Reference created: {article_id} - {titulo}")
            return ArticleService.get_article_by_id(article_id)

        except Exception as e:
            logger.error(f"Error creating reference: {e}")
            raise

    @staticmethod
    def get_article_by_id(article_id: str) -> Optional[Dict[str, Any]]:
        """Get article/reference by ID (UUID string)"""
        db = get_db()

        try:
            collection = db.get_collection("referencias")
            doc = collection.find_one({"_id": article_id})

            if not doc:
                return None

            return doc

        except Exception as e:
            logger.error(f"Error fetching reference: {e}")
            return None

    @staticmethod
    def list_articles(
        page: int = 1,
        page_size: int = 50,
        search: Optional[str] = None,
        ano: Optional[int] = None,
        pais: Optional[str] = None,
    ) -> Dict[str, Any]:
        """
        List articles/references with pagination and filters

        Args:
            page: Page number (1-based)
            page_size: Items per page
            search: Search text in title or authors
            ano: Filter by year
            pais: Filter by country

        Returns:
            Paginated list of articles
        """
        db = get_db()
        collection = db.get_collection("referencias")

        try:
            # Build query filter
            query = {}

            if ano:
                query["ano"] = ano

            if pais:
                query["pais"] = pais

            # Note: MongoDB $regex requires index, search applied in Python for simplicity
            # Search will be applied in Python after fetching

            # Get all matching documents
            cursor = collection.find(query).sort("ano", -1)

            all_items = []
            for doc in cursor:
                # Apply search filter in Python
                if search:
                    search_lower = search.lower()
                    titulo_match = search_lower in doc.get("titulo", "").lower()
                    # Check if search matches any author
                    autores_match = any(
                        search_lower in (author or "").lower()
                        for author in doc.get("autores", [])
                    )
                    if not (titulo_match or autores_match):
                        continue

                doc["_id"] = str(doc["_id"])
                all_items.append(doc)

            # Get total count after search filter
            total = len(all_items)

            # Apply pagination
            offset = (page - 1) * page_size
            items = all_items[offset : offset + page_size]

            return {
                "total": total,
                "page": page,
                "page_size": page_size,
                "items": items,
            }

        except Exception as e:
            logger.error(f"Error listing references: {e}")
            raise

    @staticmethod
    def update_article(article_id: str, **kwargs) -> Optional[Dict[str, Any]]:
        """
        Update an article/reference

        Args:
            article_id: ID of article to update
            **kwargs: Fields to update

        Returns:
            Updated article data or None if not found
        """
        db = get_db()

        try:
            # Allowed fields for update
            allowed_fields = [
                "ano",
                "titulo",
                "publicacao",
                "autores",
                "resumo",
                "especies",
                "tipoUso",
                "metodologia",
                "pais",
                "estado",
                "municipio",
                "local",
                "bioma",
            ]

            # Build update document
            update_doc = {"$set": {}}
            for field, value in kwargs.items():
                if field in allowed_fields:
                    update_doc["$set"][field] = value

            if not update_doc["$set"]:
                # No valid fields to update
                return ArticleService.get_article_by_id(article_id)

            # Execute update
            collection = db.get_collection("referencias")
            result = collection.update_one({"_id": article_id}, update_doc)

            if result.matched_count == 0:
                return None

            logger.info(f"Reference updated: {article_id}")
            return ArticleService.get_article_by_id(article_id)

        except Exception as e:
            logger.error(f"Error updating reference: {e}")
            raise

    @staticmethod
    def delete_article(article_id: str) -> bool:
        """
        Delete an article/reference

        Args:
            article_id: ID of article to delete

        Returns:
            True if deleted, False if not found
        """
        db = get_db()

        try:
            collection = db.get_collection("referencias")
            result = collection.delete_one({"_id": article_id})

            if result.deleted_count > 0:
                logger.info(f"Reference deleted: {article_id}")
                return True
            return False

        except Exception as e:
            logger.error(f"Error deleting reference: {e}")
            raise

    @staticmethod
    def check_duplicate(
        titulo: str, ano: int, primeiro_autor: str
    ) -> Optional[str]:
        """
        Check for duplicate reference by title, year, and first author

        Returns:
            ID of existing reference or None
        """
        db = get_db()

        try:
            collection = db.get_collection("referencias")
            # Check if first author is in autores list
            doc = collection.find_one(
                {
                    "titulo": titulo,
                    "ano": ano,
                    "autores": primeiro_autor,
                }
            )

            if doc:
                return str(doc["_id"])
            return None

        except Exception as e:
            logger.error(f"Error checking duplicates: {e}")
            return None

    @staticmethod
    def get_statistics() -> Dict[str, Any]:
        """Get statistics about references (processed in Python, not MongoDB)"""
        db = get_db()
        collection = db.get_collection("referencias")

        try:
            total = collection.count_documents({})

            # Count by year (aggregate processed in Python for performance)
            por_ano_dict = {}
            for doc in collection.find({}):
                ano = doc.get("ano")
                if ano:
                    por_ano_dict[ano] = por_ano_dict.get(ano, 0) + 1

            # Sort by year descending
            por_ano = [{"ano": ano, "count": count} for ano, count in sorted(por_ano_dict.items(), reverse=True)]

            # Count by country (process in Python, limit to top 10)
            por_pais_dict = {}
            for doc in collection.find({}):
                pais = doc.get("pais")
                if pais:
                    por_pais_dict[pais] = por_pais_dict.get(pais, 0) + 1

            # Sort by count descending, limit to 10
            por_pais = [
                {"pais": pais, "count": count}
                for pais, count in sorted(por_pais_dict.items(), key=lambda x: x[1], reverse=True)[:10]
            ]

            return {
                "total_referencias": total,
                "por_ano": por_ano,
                "por_pais": por_pais,
            }

        except Exception as e:
            logger.error(f"Error getting statistics: {e}")
            raise
