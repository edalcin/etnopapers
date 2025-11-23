"""
Pytest configuration and fixtures
"""

import os
import pytest

from backend.database.connection import DatabaseConnection
from backend.database.init_db import init_database


@pytest.fixture
def temp_db():
    """Create a temporary MongoDB test database

    Uses MONGO_URI environment variable (no local connections allowed).
    Tests require an explicit MongoDB connection from environment.
    """
    # Get MongoDB URI from environment variable
    test_mongo_uri = os.getenv("MONGO_URI")
    if not test_mongo_uri:
        pytest.skip("MONGO_URI not configured - database tests skipped (configure GitHub secret 'MONGO_URI' to enable)")

    init_database(test_mongo_uri)
    # Reset singleton for testing
    DatabaseConnection._instance = DatabaseConnection(test_mongo_uri)

    # Clean up database before tests
    try:
        db_connection = DatabaseConnection.get_instance(test_mongo_uri)
        # Drop the referencias collection to start fresh
        db_connection.db["referencias"].drop()
    except Exception as e:
        print(f"Cleanup warning: {e}")

    yield test_mongo_uri

    # Cleanup - drop collections after tests
    try:
        db_connection = DatabaseConnection.get_instance(test_mongo_uri)
        # Drop the referencias collection
        db_connection.db["referencias"].drop()
        DatabaseConnection._instance = None
    except Exception as e:
        print(f"Cleanup warning: {e}")


@pytest.fixture
def db(temp_db):
    """Database fixture"""
    return DatabaseConnection.get_instance(temp_db)
