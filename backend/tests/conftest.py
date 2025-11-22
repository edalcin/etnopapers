"""
Pytest configuration and fixtures
"""

import pytest

from backend.database.connection import DatabaseConnection
from backend.database.init_db import init_database


@pytest.fixture
def temp_db():
    """Create a temporary MongoDB test database"""
    # Use test database for testing
    test_mongo_uri = "mongodb://localhost:27017/etnopapers_test"
    init_database(test_mongo_uri)
    # Reset singleton for testing
    DatabaseConnection._instance = DatabaseConnection(test_mongo_uri)
    yield test_mongo_uri
    # Cleanup - drop test database
    try:
        db_connection = DatabaseConnection.get_instance(test_mongo_uri)
        db_connection.client.drop_database("etnopapers_test")
        DatabaseConnection._instance = None
    except Exception as e:
        print(f"Cleanup warning: {e}")


@pytest.fixture
def db(temp_db):
    """Database fixture"""
    return DatabaseConnection.get_instance(temp_db)
