"""
Mongita database connection management for Etnopapers

Mongita is an embedded MongoDB-compatible document database.
This module provides connection and query management using PyMongo API.
"""

import os
import logging
from pathlib import Path
from typing import Optional
from mongita import MongitaClientDisk, MongitaClientMemory
from bson import ObjectId

logger = logging.getLogger(__name__)


class DatabaseConnection:
    """Manages Mongita database connection and collections"""

    _instance: Optional["DatabaseConnection"] = None
    _db_path: str = "/data"

    def __init__(self, db_path: str = "/data", backend: str = "disk"):
        """
        Initialize Mongita database connection

        Args:
            db_path: Directory path for Mongita files
            backend: "disk" (persistent) or "memory" (test)
        """
        self._db_path = db_path
        self._backend = backend
        self._initialize_db()

    @classmethod
    def get_instance(
        cls, db_path: str = "/data", backend: str = "disk"
    ) -> "DatabaseConnection":
        """Get singleton instance of DatabaseConnection"""
        if cls._instance is None:
            cls._instance = cls(db_path, backend)
        return cls._instance

    def _initialize_db(self):
        """Initialize Mongita client and create database"""
        try:
            # Create directory if using disk backend
            if self._backend == "disk":
                db_dir = Path(self._db_path)
                db_dir.mkdir(parents=True, exist_ok=True)
                self.client = MongitaClientDisk(database_dir=self._db_path)
            else:
                self.client = MongitaClientMemory()

            # Get database
            self.db = self.client["etnopapers"]

            # Initialize collections and indexes
            self._create_collections()

            logger.info(f"Mongita database initialized: {self._db_path}")

        except Exception as e:
            logger.error(f"Failed to initialize Mongita database: {e}")
            raise

    def _create_collections(self):
        """Initialize collections and indexes (Mongita creates collections on first use)"""
        try:
            # Mongita creates collections automatically on first insert/operation
            # We just need to create the indexes for performance

            # Collection: referencias (main documents)
            try:
                self.db["referencias"].create_index("doi", unique=True)
                self.db["referencias"].create_index("status")
                self.db["referencias"].create_index("ano_publicacao")
                self.db["referencias"].create_index("data_processamento")
                self.db["referencias"].create_index([("especies.especie_id", 1)])
                self.db["referencias"].create_index([("comunidades.comunidade_id", 1)])
                self.db["referencias"].create_index([("localizacoes.territorio", 1)])
                logger.info("Indexes created for: referencias")
            except Exception as e:
                logger.warning(f"Index creation for referencias: {e}")

            # Collection: especies_plantas (taxonomy deduplication)
            try:
                self.db["especies_plantas"].create_index("nome_cientifico", unique=True)
                self.db["especies_plantas"].create_index("familia_botanica")
                self.db["especies_plantas"].create_index("status_validacao")
                logger.info("Indexes created for: especies_plantas")
            except Exception as e:
                logger.warning(f"Index creation for especies_plantas: {e}")

            # Collection: comunidades_indígenas
            try:
                self.db["comunidades_indígenas"].create_index("nome")
                self.db["comunidades_indígenas"].create_index("tipo")
                logger.info("Indexes created for: comunidades_indígenas")
            except Exception as e:
                logger.warning(f"Index creation for comunidades_indígenas: {e}")

            # Collection: localizacoes (optional, for large datasets)
            # No indexes needed for localizacoes at this time

            logger.info(
                "Mongita collections ready (auto-created on first use, indexes initialized)"
            )

        except Exception as e:
            logger.error(f"Error initializing collections: {e}")
            raise

    def get_collection(self, collection_name: str):
        """Get a collection from the database"""
        return self.db[collection_name]

    def get_database_info(self) -> dict:
        """Get database statistics"""
        try:
            collections = self.db.list_collection_names()
            collection_info = {}

            for collection_name in collections:
                count = self.db[collection_name].count_documents({})
                collection_info[collection_name] = count

            # Calculate directory size if disk-based
            size_bytes = 0
            if self._backend == "disk":
                db_path = Path(self._db_path)
                if db_path.exists():
                    for file_path in db_path.rglob("*"):
                        if file_path.is_file():
                            size_bytes += file_path.stat().st_size

            return {
                "db_path": self._db_path,
                "backend": self._backend,
                "size_bytes": size_bytes,
                "size_mb": size_bytes / (1024 * 1024),
                "collections": len(collections),
                "collection_info": collection_info,
            }

        except Exception as e:
            logger.error(f"Database info error: {e}")
            raise

    def get_collection_stats(self, collection_name: str) -> dict:
        """Get statistics for a specific collection"""
        try:
            collection = self.db[collection_name]
            count = collection.count_documents({})

            # Note: Mongita doesn't implement list_indexes, so we skip that
            return {
                "collection": collection_name,
                "document_count": count,
            }

        except Exception as e:
            logger.error(f"Collection stats error: {e}")
            raise


# Singleton instance
def get_db() -> DatabaseConnection:
    """Get database connection instance"""
    backend = os.getenv("DATABASE_BACKEND", "disk")
    db_path = os.getenv("DATABASE_PATH", "/data")
    return DatabaseConnection.get_instance(db_path, backend)
