"""
Database initialization script for Etnopapers

This script initializes the SQLite database with the complete schema.
Run once before starting the application.
"""

import sqlite3
import logging
from pathlib import Path

logger = logging.getLogger(__name__)


def init_database(db_path: str = "data/etnopapers.db") -> bool:
    """
    Initialize SQLite database with schema

    Args:
        db_path: Path to database file

    Returns:
        True if successful, False otherwise
    """
    try:
        # Create directory if it doesn't exist
        db_file = Path(db_path)
        db_file.parent.mkdir(parents=True, exist_ok=True)

        # Connect to database
        conn = sqlite3.connect(db_path)
        cursor = conn.cursor()

        # Read schema
        schema_path = Path(__file__).parent / "schema.sql"
        with open(schema_path, "r", encoding="utf-8") as f:
            schema_sql = f.read()

        # Execute schema
        cursor.executescript(schema_sql)

        # Verify tables were created
        cursor.execute(
            "SELECT name FROM sqlite_master WHERE type='table' ORDER BY name;"
        )
        tables = cursor.fetchall()
        table_names = [t[0] for t in tables]

        expected_tables = [
            "ArtigosCientificos",
            "DadosEstudo",
            "Paises",
            "Estados",
            "Municipios",
            "Territorios",
            "ArtigoLocalizacao",
            "EspeciesPlantas",
            "NomesVernaculares",
            "EspecieNomeVernacular",
            "ArtigoEspecie",
            "Comunidades",
        ]

        if not all(t in table_names for t in expected_tables):
            logger.error(f"Missing tables. Found: {table_names}")
            return False

        # Check pragmas
        cursor.execute("PRAGMA foreign_keys;")
        fk_enabled = cursor.fetchone()[0] == 1

        cursor.execute("PRAGMA journal_mode;")
        journal_mode = cursor.fetchone()[0].upper()

        logger.info(f"Database initialized: {db_path}")
        logger.info(f"Tables: {len(table_names)} created")
        logger.info(f"Foreign keys: {'Enabled' if fk_enabled else 'Disabled'}")
        logger.info(f"Journal mode: {journal_mode}")

        conn.commit()
        conn.close()
        return True

    except Exception as e:
        logger.error(f"Failed to initialize database: {e}")
        return False


if __name__ == "__main__":
    logging.basicConfig(level=logging.INFO)
    success = init_database()
    exit(0 if success else 1)
