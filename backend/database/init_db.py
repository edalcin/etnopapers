"""
Database initialization for Mongita

Mongita is schema-less, so initialization just creates collections and indexes.
No SQL schema files needed!
"""

import sys
import os
import logging

# Add project root to path to allow absolute imports
sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__)))))

from backend.database.connection import get_db

logger = logging.getLogger(__name__)


def init_database(db_path: str = "/data", backend: str = "disk") -> bool:
    """
    Initialize Mongita database with collections and indexes

    Args:
        db_path: Path to database directory
        backend: "disk" (persistent) or "memory" (test)

    Returns:
        True if successful
    """
    try:
        # Initialize database connection
        db = get_db()

        # Verify collections exist
        collections = db.db.list_collection_names()
        expected_collections = [
            "referencias",
            "especies_plantas",
            "comunidades_indígenas",
            "localizacoes",
        ]

        logger.info(f"Database initialized: {db_path}")
        logger.info(f"Backend: {backend}")
        logger.info(f"Collections: {len(collections)} created")
        logger.info(f"Collections: {', '.join(collections)}")

        for collection_name in expected_collections:
            if collection_name in collections:
                stats = db.get_collection_stats(collection_name)
                logger.info(
                    f"  - {collection_name}: {stats['document_count']} documents"
                )

        logger.info("✓ Mongita database ready for use (schema-less)")
        return True

    except Exception as e:
        logger.error(f"Failed to initialize database: {e}")
        return False


if __name__ == "__main__":
    logging.basicConfig(level=logging.INFO)
    success = init_database()
    exit(0 if success else 1)
