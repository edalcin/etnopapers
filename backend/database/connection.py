"""
MongoDB database connection management for Etnopapers

This module provides connection and query management using PyMongo API.
Connection URI is configured via MONGO_URI environment variable.
"""

import logging
import os
from typing import Optional

from pymongo import MongoClient
from pymongo.errors import ConnectionFailure, ServerSelectionTimeoutError

logger = logging.getLogger(__name__)


class DatabaseConnection:
    """Manages MongoDB database connection and collections"""

    _instance: Optional["DatabaseConnection"] = None
    _mongo_uri: str = ""

    def __init__(self, mongo_uri: str):
        """
        Initialize MongoDB database connection

        Args:
            mongo_uri: MongoDB connection URI (e.g., mongodb://localhost:27017/etnopapers
                      or mongodb+srv://user:pass@cluster.mongodb.net/etnopapers)
        """
        self._mongo_uri = mongo_uri
        self._initialize_db()

    @classmethod
    def get_instance(cls, mongo_uri: str) -> "DatabaseConnection":
        """Get singleton instance of DatabaseConnection"""
        if cls._instance is None:
            cls._instance = cls(mongo_uri)
        return cls._instance

    def _initialize_db(self):
        """Initialize MongoDB client and create database"""
        try:
            # Connect to MongoDB
            self.client = MongoClient(self._mongo_uri, serverSelectionTimeoutMS=5000)

            # Verify connection
            self.client.admin.command("ping")

            # Get database (extracted from URI or defaults to 'etnopapers')
            # If URI is mongodb://localhost:27017/etnopapers, database name is 'etnopapers'
            # If URI doesn't specify database, we use 'etnopapers' as default
            db_name = "etnopapers"
            if "/" in self._mongo_uri.split("mongodb+srv://")[-1].split("mongodb://")[-1]:
                # Extract database name from URI if present
                parts = self._mongo_uri.split("/")
                if len(parts) > 1 and parts[-1] and not "?" in parts[-1].split("@")[-1]:
                    db_name = parts[-1].split("?")[0]

            self.db = self.client[db_name]

            # Initialize collections and indexes
            self._create_collections()

            logger.info(f"MongoDB database initialized: {db_name}")

        except (ConnectionFailure, ServerSelectionTimeoutError) as e:
            logger.error(f"Failed to connect to MongoDB: {e}")
            raise
        except Exception as e:
            logger.error(f"Failed to initialize MongoDB database: {e}")
            raise

    def _create_collections(self):
        """Initialize collections and indexes"""
        try:
            # MongoDB creates collections automatically on first insert/operation
            # We just need to create the indexes for performance

            # Single collection: referencias (simplified denormalized model)
            try:
                # Create indexes for filtering and searching
                self.db["referencias"].create_index("doi", unique=True, sparse=True)
                self.db["referencias"].create_index("ano")
                self.db["referencias"].create_index("status")
                self.db["referencias"].create_index("titulo")
                logger.info("Indexes created for: referencias (doi, ano, status, titulo)")
            except Exception as e:
                logger.warning(f"Index creation for referencias: {e}")

            logger.info(
                "MongoDB database ready (single 'referencias' collection, indexes initialized)"
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

            # Get database statistics from MongoDB server
            db_stats = self.db.command("dbStats")
            size_bytes = db_stats.get("dataSize", 0)

            return {
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

            # Get collection statistics from MongoDB
            try:
                col_stats = self.db.command("collStats", collection_name)
                return {
                    "collection": collection_name,
                    "document_count": count,
                    "avg_obj_size": col_stats.get("avgObjSize", 0),
                    "storage_size": col_stats.get("storageSize", 0),
                }
            except Exception:
                # Fallback if collStats not available
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
    mongo_uri = os.getenv("MONGO_URI", "mongodb://localhost:27017/etnopapers")
    return DatabaseConnection.get_instance(mongo_uri)
