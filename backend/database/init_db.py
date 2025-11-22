"""
Database initialization for MongoDB

MongoDB is schema-less, so initialization just creates collections and indexes.
No SQL schema files needed!
Connection URI is configured via MONGO_URI environment variable.
"""

import logging
import os
import sys

# Add project root to path to allow absolute imports
sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__)))))

from backend.database.connection import get_db

logger = logging.getLogger(__name__)


def init_database(mongo_uri: str = None) -> bool:
    """
    Initialize MongoDB database with collections and indexes

    Args:
        mongo_uri: MongoDB connection URI. If None, uses MONGO_URI environment variable
                  (e.g., mongodb://localhost:27017/etnopapers or mongodb+srv://user:pass@cluster.mongodb.net/etnopapers)

    Returns:
        True if successful
    """
    try:
        # Use provided URI or get from environment
        if mongo_uri is None:
            mongo_uri = os.getenv("MONGO_URI", "mongodb://localhost:27017/etnopapers")

        # Initialize database connection
        db = get_db()

        # Verify collections exist
        collections = db.db.list_collection_names()
        expected_collections = [
            "referencias",
        ]

        logger.info(f"MongoDB database initialized")
        logger.info(f"URI: {mongo_uri.split('@')[0]}***@{mongo_uri.split('@')[1] if '@' in mongo_uri else mongo_uri}")
        logger.info(f"Collections: {len(collections)} total")
        logger.info(f"Collections: {', '.join(collections)}")

        for collection_name in expected_collections:
            if collection_name in collections:
                stats = db.get_collection_stats(collection_name)
                logger.info(
                    f"  - {collection_name}: {stats['document_count']} documents"
                )

        logger.info("✓ MongoDB database ready for use (schema-less)")
        return True

    except Exception as e:
        logger.error(f"Failed to initialize database: {e}")
        return False


if __name__ == "__main__":
    logging.basicConfig(level=logging.INFO)
    success = init_database()
    exit(0 if success else 1)
