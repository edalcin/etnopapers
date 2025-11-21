"""
Duplicate detection service for articles (Mongita-based)

Implements multi-strategy duplicate detection:
1. Primary: DOI uniqueness
2. Secondary: Title + Year + First Author
"""

import logging
from typing import Optional, Dict, Any
from bson import ObjectId
from backend.database.connection import get_db

logger = logging.getLogger(__name__)


class DuplicateChecker:
    """Service for detecting duplicate articles"""

    @staticmethod
    def check_by_doi(doi: str) -> Optional[Dict[str, Any]]:
        """
        Check for duplicate by DOI

        Args:
            doi: Digital Object Identifier

        Returns:
            Existing article if found, None otherwise
        """
        if not doi:
            return None

        db = get_db()
        collection = db.get_collection("referencias")

        try:
            doc = collection.find_one({"doi": doi})

            if doc:
                doc["_id"] = str(doc["_id"])
                doc["tipo_duplicata"] = "doi"
                return doc

            return None

        except Exception as e:
            logger.error(f"Error checking by DOI: {e}")
            return None

    @staticmethod
    def _extract_first_author(autores: list) -> Optional[str]:
        """
        Extract first author name from authors list

        Args:
            autores: List of author dictionaries

        Returns:
            First author's name (nome field) or None
        """
        if not autores or len(autores) == 0:
            return None

        first_author = autores[0]
        if isinstance(first_author, dict):
            return first_author.get("nome") or first_author.get("sobrenome")
        return None

    @staticmethod
    def check_by_metadata(
        titulo: str, ano_publicacao: int, autores: list
    ) -> Optional[Dict[str, Any]]:
        """
        Check for duplicate by title + year + first author

        Uses regex matching for title similarity (case-insensitive)

        Args:
            titulo: Article title
            ano_publicacao: Publication year
            autores: List of authors

        Returns:
            Existing article if found, None otherwise
        """
        if not titulo or not ano_publicacao:
            return None

        first_author = DuplicateChecker._extract_first_author(autores)

        db = get_db()
        collection = db.get_collection("referencias")

        try:
            # Check for exact title match first
            doc = collection.find_one(
                {
                    "titulo": titulo,
                    "ano_publicacao": ano_publicacao,
                }
            )

            if doc:
                existing_autores = doc.get("autores", [])
                existing_first_author = DuplicateChecker._extract_first_author(
                    existing_autores
                )

                # If first author matches or is None, consider it a duplicate
                if not first_author or not existing_first_author or (
                    first_author.lower() == existing_first_author.lower()
                ):
                    doc["_id"] = str(doc["_id"])
                    doc["tipo_duplicata"] = "metadata"
                    return doc

            # Check for similar titles (substring match in Python since Mongita doesn't support $regex)
            # Just get documents from same year and filter by first word in Python
            first_word = titulo.split()[0] if titulo else ""
            if first_word:
                docs = collection.find({"ano_publicacao": ano_publicacao})
                for potential_doc in docs:
                    if first_word.lower() in potential_doc.get("titulo", "").lower():
                        doc = potential_doc
                        break

            if doc:
                existing_autores = doc.get("autores", [])
                existing_first_author = DuplicateChecker._extract_first_author(
                    existing_autores
                )

                if not first_author or not existing_first_author or (
                    first_author.lower() == existing_first_author.lower()
                ):
                    doc["_id"] = str(doc["_id"])
                    doc["tipo_duplicata"] = "metadata"
                    return doc

            return None

        except Exception as e:
            logger.error(f"Error checking by metadata: {e}")
            return None

    @staticmethod
    def check_duplicate(
        titulo: str,
        ano_publicacao: int,
        autores: list,
        doi: Optional[str] = None,
    ) -> Optional[Dict[str, Any]]:
        """
        Check for duplicates using multiple strategies

        Strategy:
        1. Primary: Check DOI (if provided)
        2. Secondary: Check by title + year + first author

        Args:
            titulo: Article title
            ano_publicacao: Publication year
            autores: List of authors
            doi: Digital Object Identifier (optional)

        Returns:
            Duplicate article data if found, None otherwise
        """
        # Strategy 1: DOI (most reliable)
        if doi:
            duplicate = DuplicateChecker.check_by_doi(doi)
            if duplicate:
                logger.info(f"Duplicate found by DOI: {doi}")
                return duplicate

        # Strategy 2: Metadata (title + year + author)
        duplicate = DuplicateChecker.check_by_metadata(titulo, ano_publicacao, autores)
        if duplicate:
            logger.info(f"Duplicate found by metadata: {titulo} ({ano_publicacao})")
            return duplicate

        return None

    @staticmethod
    def get_similar_articles(
        titulo: str, ano_publicacao: int, limit: int = 5
    ) -> list:
        """
        Get potentially similar articles for review

        Args:
            titulo: Article title
            ano_publicacao: Publication year
            limit: Maximum number of results

        Returns:
            List of similar articles
        """
        db = get_db()
        collection = db.get_collection("referencias")

        try:
            # Look for articles within +/- 3 years of publication with similar title
            first_word = titulo.split()[0] if titulo else ""

            # Use Mongita's supported operators for year range, filter by title in Python
            cursor = collection.find(
                {
                    "ano_publicacao": {
                        "$gte": ano_publicacao - 3,
                        "$lte": ano_publicacao + 3,
                    }
                }
            )

            similar = []
            for doc in cursor:
                # Filter by title match in Python since Mongita doesn't support $regex
                if first_word and first_word.lower() in doc.get("titulo", "").lower():
                    doc["_id"] = str(doc["_id"])
                    similar.append(doc)
                    if len(similar) >= limit:
                        break

            return similar

        except Exception as e:
            logger.error(f"Error getting similar articles: {e}")
            return []
