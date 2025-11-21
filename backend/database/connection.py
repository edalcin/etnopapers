"""
SQLite database connection management for Etnopapers
"""

import sqlite3
import logging
from contextlib import contextmanager
from typing import Optional
from pathlib import Path

logger = logging.getLogger(__name__)


class DatabaseConnection:
    """Manages SQLite database connections"""

    _instance: Optional["DatabaseConnection"] = None
    _db_path: str = "data/etnopapers.db"

    def __init__(self, db_path: str = "data/etnopapers.db"):
        self._db_path = db_path
        self._initialize_db()

    @classmethod
    def get_instance(cls, db_path: str = "data/etnopapers.db") -> "DatabaseConnection":
        """Get singleton instance of DatabaseConnection"""
        if cls._instance is None:
            cls._instance = cls(db_path)
        return cls._instance

    def _initialize_db(self):
        """Initialize database if needed"""
        try:
            # Create directory if needed
            db_file = Path(self._db_path)
            db_file.parent.mkdir(parents=True, exist_ok=True)

            # Test connection
            conn = self.get_connection()
            cursor = conn.cursor()

            # Enable foreign keys
            cursor.execute("PRAGMA foreign_keys = ON;")

            # Set WAL mode
            cursor.execute("PRAGMA journal_mode = WAL;")

            cursor.execute("PRAGMA synchronous = NORMAL;")
            cursor.execute("PRAGMA cache_size = -64000;")

            conn.commit()
            conn.close()

            logger.info(f"Database initialized: {self._db_path}")

        except Exception as e:
            logger.error(f"Failed to initialize database: {e}")
            raise

    def get_connection(self) -> sqlite3.Connection:
        """Get a new database connection"""
        conn = sqlite3.connect(self._db_path)
        conn.row_factory = sqlite3.Row
        return conn

    @contextmanager
    def get_cursor(self):
        """Context manager for database cursor"""
        conn = self.get_connection()
        cursor = conn.cursor()
        try:
            yield cursor
            conn.commit()
        except Exception as e:
            conn.rollback()
            logger.error(f"Database error: {e}")
            raise
        finally:
            cursor.close()
            conn.close()

    def execute_query(self, query: str, params: tuple = None):
        """Execute a SELECT query"""
        try:
            with self.get_cursor() as cursor:
                if params:
                    cursor.execute(query, params)
                else:
                    cursor.execute(query)
                return cursor.fetchall()
        except Exception as e:
            logger.error(f"Query error: {e}")
            raise

    def execute_update(self, query: str, params: tuple = None) -> int:
        """Execute INSERT/UPDATE/DELETE query"""
        try:
            with self.get_cursor() as cursor:
                if params:
                    cursor.execute(query, params)
                else:
                    cursor.execute(query)
                return cursor.rowcount
        except Exception as e:
            logger.error(f"Update error: {e}")
            raise

    def get_integrity_check(self) -> list:
        """Run PRAGMA integrity_check"""
        try:
            conn = self.get_connection()
            cursor = conn.cursor()
            cursor.execute("PRAGMA integrity_check;")
            result = cursor.fetchall()
            conn.close()
            return result
        except Exception as e:
            logger.error(f"Integrity check error: {e}")
            raise

    def get_database_info(self) -> dict:
        """Get database statistics"""
        try:
            conn = self.get_connection()
            cursor = conn.cursor()

            # Get database size
            db_size = Path(self._db_path).stat().st_size if Path(self._db_path).exists() else 0

            # Get table counts
            cursor.execute(
                "SELECT name FROM sqlite_master WHERE type='table' ORDER BY name;"
            )
            tables = [t[0] for t in cursor.fetchall()]

            table_info = {}
            for table in tables:
                cursor.execute(f"SELECT COUNT(*) FROM {table};")
                count = cursor.fetchone()[0]
                table_info[table] = count

            conn.close()

            return {
                "db_path": self._db_path,
                "size_bytes": db_size,
                "size_mb": db_size / (1024 * 1024),
                "tables": len(tables),
                "table_info": table_info,
            }

        except Exception as e:
            logger.error(f"Database info error: {e}")
            raise


# Singleton instance
def get_db() -> DatabaseConnection:
    """Get database connection instance"""
    return DatabaseConnection.get_instance()
