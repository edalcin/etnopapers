"""
Pytest configuration and fixtures
"""

from pathlib import Path
import pytest
import tempfile

from backend.database.init_db import init_database
from backend.database.connection import DatabaseConnection


@pytest.fixture
def temp_db():
    """Create a temporary test database"""
    with tempfile.TemporaryDirectory() as tmpdir:
        db_path = str(Path(tmpdir) / "test.db")
        init_database(db_path)
        # Reset singleton for testing
        DatabaseConnection._instance = DatabaseConnection(db_path)
        yield db_path
        # Cleanup
        DatabaseConnection._instance = None


@pytest.fixture
def db(temp_db):
    """Database fixture"""
    return DatabaseConnection.get_instance(temp_db)
