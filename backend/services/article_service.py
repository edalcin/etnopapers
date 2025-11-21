"""
Article/Reference service layer for business logic

Uses Mongita (MongoDB-compatible) for document operations.
Articles are now called "referencias" (references) in the Mongita model.
"""

import logging
from typing import List, Optional, Dict, Any
from datetime import datetime
from bson import ObjectId
from backend.database.connection import get_db

logger = logging.getLogger(__name__)


class ArticleService:
    """Service for reference/article-related operations"""

    @staticmethod
    def create_article(
        titulo: str,
        ano_publicacao: int,
        autores: List[str],
        doi: Optional[str] = None,
        resumo: Optional[str] = None,
        status: str = "rascunho",
        metadata_estudo: Optional[Dict] = None,
        especies: Optional[List[Dict]] = None,
        comunidades: Optional[List[Dict]] = None,
        localizacoes: Optional[List[Dict]] = None,
    ) -> Dict[str, Any]:
        """
        Create a new reference/article

        Args:
            titulo: Article title
            ano_publicacao: Publication year
            autores: List of author names
            doi: Digital Object Identifier
            resumo: Abstract/Summary
            status: Article status (rascunho or finalizado)
            metadata_estudo: Study metadata (dict)
            especies: Associated species (list)
            comunidades: Associated communities (list)
            localizacoes: Associated locations (list)

        Returns:
            Created document data with ID
        """
        db = get_db()

        try:
            # Build document
            doc = {
                "titulo": titulo,
                "ano_publicacao": ano_publicacao,
                "autores": autores,
                "resumo": resumo,
                "status": status,
                "data_processamento": datetime.utcnow(),
                "data_ultima_modificacao": datetime.utcnow(),
                "editado_manualmente": False,
                "metadata_estudo": metadata_estudo or {},
                "especies": especies or [],
                "comunidades": comunidades or [],
                "localizacoes": localizacoes or [],
            }

            # Add DOI only if provided (to maintain unique index)
            if doi:
                doc["doi"] = doi

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
        """Get article/reference by ID"""
        db = get_db()

        try:
            # Convert string ID to ObjectId
            try:
                obj_id = ObjectId(article_id)
            except:
                return None

            collection = db.get_collection("referencias")
            doc = collection.find_one({"_id": obj_id})

            if not doc:
                return None

            # Convert ObjectId to string for JSON serialization
            doc["_id"] = str(doc["_id"])
            return doc

        except Exception as e:
            logger.error(f"Error fetching reference: {e}")
            return None

    @staticmethod
    def list_articles(
        page: int = 1,
        page_size: int = 50,
        status: Optional[str] = None,
        search: Optional[str] = None,
        ano: Optional[int] = None,
    ) -> Dict[str, Any]:
        """
        List articles/references with pagination and filters

        Args:
            page: Page number (1-based)
            page_size: Items per page
            status: Filter by status (rascunho or finalizado)
            search: Search text in title or authors
            ano: Filter by year

        Returns:
            Paginated list of articles
        """
        db = get_db()
        collection = db.get_collection("referencias")

        try:
            # Build query filter
            query = {}

            if status:
                query["status"] = status

            if ano:
                query["ano_publicacao"] = ano

            if search:
                # Search in titulo or autores using regex (case-insensitive)
                query["$or"] = [
                    {"titulo": {"$regex": search, "$options": "i"}},
                    {"autores": {"$regex": search, "$options": "i"}},
                ]

            # Get total count
            total = collection.count_documents(query)

            # Get paginated results
            offset = (page - 1) * page_size
            cursor = (
                collection.find(query)
                .sort("data_processamento", -1)
                .skip(offset)
                .limit(page_size)
            )

            items = []
            for doc in cursor:
                doc["_id"] = str(doc["_id"])
                items.append(doc)

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
            # Convert string ID to ObjectId
            try:
                obj_id = ObjectId(article_id)
            except:
                return None

            # Allowed fields for update
            allowed_fields = [
                "titulo",
                "doi",
                "ano_publicacao",
                "autores",
                "resumo",
                "status",
                "metadata_estudo",
                "especies",
                "comunidades",
                "localizacoes",
                "editado_manualmente",
            ]

            # Build update document
            update_doc = {"$set": {}}
            for field, value in kwargs.items():
                if field in allowed_fields:
                    update_doc["$set"][field] = value

            # Always update modification timestamp
            update_doc["$set"]["data_ultima_modificacao"] = datetime.utcnow()

            if not update_doc["$set"]:
                # No valid fields to update
                return ArticleService.get_article_by_id(article_id)

            # Execute update
            collection = db.get_collection("referencias")
            result = collection.update_one({"_id": obj_id}, update_doc)

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
            # Convert string ID to ObjectId
            try:
                obj_id = ObjectId(article_id)
            except:
                return False

            collection = db.get_collection("referencias")
            result = collection.delete_one({"_id": obj_id})

            if result.deleted_count > 0:
                logger.info(f"Reference deleted: {article_id}")
                return True
            return False

        except Exception as e:
            logger.error(f"Error deleting reference: {e}")
            raise

    @staticmethod
    def get_by_doi(doi: str) -> Optional[Dict[str, Any]]:
        """Get reference by DOI"""
        db = get_db()

        try:
            collection = db.get_collection("referencias")
            doc = collection.find_one({"doi": doi})

            if not doc:
                return None

            doc["_id"] = str(doc["_id"])
            return doc

        except Exception as e:
            logger.error(f"Error fetching by DOI: {e}")
            return None

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
                    "ano_publicacao": ano,
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
        """Get statistics about references"""
        db = get_db()
        collection = db.get_collection("referencias")

        try:
            total = collection.count_documents({})
            finalizados = collection.count_documents({"status": "finalizado"})
            rascunhos = collection.count_documents({"status": "rascunho"})

            # Count by year (aggregation pipeline)
            por_ano = list(
                collection.aggregate(
                    [
                        {
                            "$group": {
                                "_id": "$ano_publicacao",
                                "count": {"$sum": 1},
                            }
                        },
                        {"$sort": {"_id": -1}},
                    ]
                )
            )

            return {
                "total_referencias": total,
                "finalizados": finalizados,
                "rascunhos": rascunhos,
                "por_ano": por_ano,
            }

        except Exception as e:
            logger.error(f"Error getting statistics: {e}")
            raise
