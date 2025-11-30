import os
from typing import Optional
from pymongo import MongoClient
from pymongo.errors import ConnectionFailure, ServerSelectionTimeoutError

class MongoDBConnection:
    """MongoDB connection factory with MONGO_URI environment variable support"""
    
    _instance: Optional[MongoClient] = None
    _db = None
    
    @classmethod
    def get_connection(cls) -> MongoClient:
        """Get or create MongoDB connection"""
        if cls._instance is None:
            mongo_uri = os.getenv(
                'MONGO_URI',
                'mongodb://localhost:27017/etnopapers'
            )
            try:
                cls._instance = MongoClient(
                    mongo_uri,
                    serverSelectionTimeoutMS=5000,
                    connectTimeoutMS=10000,
                    retryWrites=True
                )
                # Verify connection
                cls._instance.admin.command('ping')
            except (ConnectionFailure, ServerSelectionTimeoutError) as e:
                raise ConnectionError(f"Falha ao conectar ao MongoDB: {str(e)}")
        
        return cls._instance
    
    @classmethod
    def get_database(cls):
        """Get MongoDB database instance"""
        if cls._db is None:
            client = cls.get_connection()
            db_name = os.getenv('MONGO_DB_NAME', 'etnopapers')
            cls._db = client[db_name]
        
        return cls._db
    
    @classmethod
    def close(cls):
        """Close MongoDB connection"""
        if cls._instance is not None:
            cls._instance.close()
            cls._instance = None
            cls._db = None

def get_db():
    """Dependency injection for MongoDB database"""
    return MongoDBConnection.get_database()
