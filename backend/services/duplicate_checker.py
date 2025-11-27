"""
Duplicate detection service for articles (MongoDB-based)

Implements multi-strategy duplicate detection:
1. Primary: DOI uniqueness (100% confidence)
2. Secondary: Title + Year + Author (fuzzy matching, 95% confidence)

Updated 2025-11-27: Enhanced with Levenshtein distance for fuzzy matching
"""

import logging
from typing import Any
from typing import Dict
from typing import Optional

from Levenshtein import ratio as levenshtein_ratio

from backend.database.connection import get_db
from backend.exceptions import DuplicateDetectionError

logger = logging.getLogger(__name__)


class DuplicateChecker:
    """
    Service for detecting duplicate articles

    Uses two strategies with confidence scoring:
    1. DOI matching (100% confidence if exact)
    2. Fuzzy title + year + author matching (95% confidence if high similarity)
    """

    TITLE_SIMILARITY_THRESHOLD = 0.85  # 85%+ similarity required
    AUTHOR_SIMILARITY_THRESHOLD = 0.90  # 90%+ similarity for author names

    def __init__(self):
        """Initialize duplicate checker"""
        logger.info("DuplicateChecker initialized")

    def find_duplicate(self, metadata_dict: Dict[str, Any]) -> Optional[Dict[str, Any]]:
        """
        Find duplicate article from extracted metadata dict

        Args:
            metadata_dict: Extracted article metadata (from ReferenceData.dict())

        Returns:
            Duplicate article document if found, None otherwise
        """
        try:
            titulo = metadata_dict.get("titulo")
            ano = metadata_dict.get("ano") or metadata_dict.get("ano_publicacao")
            autores = metadata_dict.get("autores", [])
            doi = metadata_dict.get("doi")

            # Strategy 1: Check DOI (if available)
            if doi:
                duplicate = self.check_by_doi(doi)
                if duplicate:
                    logger.info(f"Duplicate found by DOI: {doi}")
                    return duplicate

            # Strategy 2: Check by fuzzy metadata matching
            if titulo and ano:
                duplicate = self.check_by_fuzzy_metadata(titulo, ano, autores)
                if duplicate:
                    logger.info(f"Duplicate found by metadata: {titulo} ({ano})")
                    return duplicate

            return None

        except Exception as e:
            logger.error(f"Error in find_duplicate: {e}")
            raise DuplicateDetectionError(str(e))

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
    def _normalize_string(text: str) -> str:
        """Normalize string for comparison (lowercase, strip)"""
        if not text:
            return ""
        return text.lower().strip()

    @staticmethod
    def _extract_first_author(autores: list) -> Optional[str]:
        """
        Extract first author name from authors list

        Args:
            autores: List of author strings or dictionaries

        Returns:
            First author's name or None
        """
        if not autores or len(autores) == 0:
            return None

        first_author = autores[0]

        # Handle string authors
        if isinstance(first_author, str):
            return first_author.strip()

        # Handle dict authors
        if isinstance(first_author, dict):
            return first_author.get("nome") or first_author.get("sobrenome")

        return None

    def check_by_fuzzy_metadata(
        self, titulo: str, ano_publicacao: int, autores: list
    ) -> Optional[Dict[str, Any]]:
        """
        Check for duplicates using fuzzy matching on title + year + author

        Uses Levenshtein distance for similarity scoring:
        - Title must be 85%+ similar
        - Author must be 90%+ similar
        - Year must be exact match

        Args:
            titulo: Article title
            ano_publicacao: Publication year
            autores: List of authors

        Returns:
            Duplicate document if found, None otherwise
        """
        if not titulo or not ano_publicacao:
            return None

        first_author = self._extract_first_author(autores)
        norm_titulo = self._normalize_string(titulo)

        try:
            db = get_db()
            collection = db.get_collection("referencias")

            # Get all documents from same year
            docs = collection.find({"ano": ano_publicacao})

            for doc in docs:
                doc_titulo = doc.get("titulo", "")
                norm_doc_titulo = self._normalize_string(doc_titulo)

                # Calculate title similarity
                title_similarity = levenshtein_ratio(norm_titulo, norm_doc_titulo)

                if title_similarity >= self.TITLE_SIMILARITY_THRESHOLD:
                    # Check author match if we have one
                    doc_autores = doc.get("autores", [])
                    doc_first_author = self._extract_first_author(doc_autores)

                    if first_author and doc_first_author:
                        author_similarity = levenshtein_ratio(
                            self._normalize_string(first_author),
                            self._normalize_string(doc_first_author),
                        )

                        if author_similarity >= self.AUTHOR_SIMILARITY_THRESHOLD:
                            # Found a match!
                            doc["_id"] = str(doc["_id"])
                            doc["confidence_score"] = (
                                title_similarity + author_similarity
                            ) / 2
                            return doc
                    else:
                        # No author info to verify - high confidence from title alone
                        doc["_id"] = str(doc["_id"])
                        doc["confidence_score"] = title_similarity
                        return doc

            return None

        except Exception as e:
            logger.error(f"Error in fuzzy matching: {e}")
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

            # Check for similar titles (substring match in Python for better performance)
            # Get documents from same year and filter by first word in Python
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

            # Use MongoDB operators for year range, filter by title in Python for simplicity
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
                # Filter by title match in Python for better performance
                if first_word and first_word.lower() in doc.get("titulo", "").lower():
                    doc["_id"] = str(doc["_id"])
                    similar.append(doc)
                    if len(similar) >= limit:
                        break

            return similar

        except Exception as e:
            logger.error(f"Error getting similar articles: {e}")
            return []
