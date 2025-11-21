"""
Duplicate detection service for articles

Implements multi-strategy duplicate detection:
1. Primary: DOI uniqueness
2. Secondary: Title + Year + First Author
"""

import json
import logging
from typing import Optional, Dict, Any
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
        query = """
            SELECT id, titulo, doi, ano_publicacao, autores,
                   status, data_processamento, data_ultima_modificacao
            FROM ArtigosCientificos
            WHERE doi = ? AND doi IS NOT NULL
            LIMIT 1
        """

        results = db.execute_query(query, (doi,))

        if results:
            row = results[0]
            return {
                "id": row[0],
                "titulo": row[1],
                "doi": row[2],
                "ano_publicacao": row[3],
                "autores": json.loads(row[4]),
                "status": row[5],
                "data_processamento": row[6],
                "data_ultima_modificacao": row[7],
                "tipo_duplicata": "doi",
            }

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

        Uses fuzzy matching for title similarity (exact match on year and first author)

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

        # Query with title similarity (using LIKE for substring matching)
        # In production, could use FTS5 (Full Text Search) for better matching
        query = """
            SELECT id, titulo, doi, ano_publicacao, autores,
                   status, data_processamento, data_ultima_modificacao
            FROM ArtigosCientificos
            WHERE ano_publicacao = ?
            AND (
                LOWER(titulo) = LOWER(?)
                OR (
                    LOWER(titulo) LIKE LOWER('%' || ? || '%')
                    AND LENGTH(titulo) BETWEEN LENGTH(?) - 10 AND LENGTH(?) + 10
                )
            )
            ORDER BY
                CASE WHEN LOWER(titulo) = LOWER(?) THEN 0 ELSE 1 END,
                ABS(LENGTH(titulo) - LENGTH(?))
            LIMIT 1
        """

        params = (
            ano_publicacao,
            titulo,
            titulo.split()[0] if titulo else "",  # First word for fuzzy match
            titulo,
            titulo,
            titulo,
            titulo,
        )

        results = db.execute_query(query, params)

        if results:
            row = results[0]
            existing_autores = json.loads(row[4])
            existing_first_author = DuplicateChecker._extract_first_author(
                existing_autores
            )

            # If first author matches or is None, consider it a duplicate
            if not first_author or not existing_first_author or (
                first_author.lower() == existing_first_author.lower()
            ):
                return {
                    "id": row[0],
                    "titulo": row[1],
                    "doi": row[2],
                    "ano_publicacao": row[3],
                    "autores": existing_autores,
                    "status": row[5],
                    "data_processamento": row[6],
                    "data_ultima_modificacao": row[7],
                    "tipo_duplicata": "metadata",
                }

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

        # Look for articles within +/- 3 years of publication
        query = """
            SELECT id, titulo, doi, ano_publicacao, autores,
                   status, data_processamento
            FROM ArtigosCientificos
            WHERE ano_publicacao BETWEEN ? AND ?
            AND (
                LOWER(titulo) LIKE LOWER('%' || ? || '%')
                OR LOWER(titulo) LIKE LOWER('%' || ? || '%')
            )
            ORDER BY
                CASE WHEN LOWER(titulo) LIKE LOWER('%' || ? || '%') THEN 0 ELSE 1 END,
                ABS(ano_publicacao - ?)
            LIMIT ?
        """

        first_word = titulo.split()[0] if titulo else ""
        second_word = titulo.split()[1] if len(titulo.split()) > 1 else ""

        results = db.execute_query(
            query,
            (
                ano_publicacao - 3,
                ano_publicacao + 3,
                first_word,
                second_word,
                titulo,
                ano_publicacao,
                limit,
            ),
        )

        similar = []
        for row in results:
            similar.append(
                {
                    "id": row[0],
                    "titulo": row[1],
                    "doi": row[2],
                    "ano_publicacao": row[3],
                    "autores": json.loads(row[4]),
                    "status": row[5],
                    "data_processamento": row[6],
                }
            )

        return similar
