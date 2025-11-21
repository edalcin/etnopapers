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
    """Create a temporary Mongita test database (in-memory for speed)"""
    # Use in-memory backend for faster tests
    init_database("test_etnopapers", "memory")
    # Reset singleton for testing
    DatabaseConnection._instance = DatabaseConnection("test_etnopapers", "memory")
    yield "test_etnopapers"
    # Cleanup
    DatabaseConnection._instance = None


@pytest.fixture
def db(temp_db):
    """Database fixture"""
    return DatabaseConnection.get_instance(temp_db)
