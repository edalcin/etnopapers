from typing import List, Optional, Dict, Any
from datetime import datetime
from bson import ObjectId
from pymongo import ASCENDING, DESCENDING
from pymongo.errors import DuplicateKeyError
from ..database.connection import get_db
from ..models.reference import Reference, ReferenceCreate, ReferenceUpdate

class DatabaseService:
    """Service layer for database operations"""
    
    COLLECTION_NAME = 'referencias'
    
    @staticmethod
    def _get_collection():
        """Get references collection"""
        db = get_db()
        return db[DatabaseService.COLLECTION_NAME]
    
    @staticmethod
    def create_article(article_data: ReferenceCreate) -> Reference:
        """Create new article in database"""
        collection = DatabaseService._get_collection()
        
        doc = {
            **article_data.dict(exclude_none=True),
            'criado_em': datetime.utcnow(),
            'atualizado_em': datetime.utcnow()
        }
        
        try:
            result = collection.insert_one(doc)
            doc['_id'] = result.inserted_id
            return Reference(**doc)
        except DuplicateKeyError as e:
            raise ValueError(f"Artigo com DOI duplicado: {article_data.doi}")
    
    @staticmethod
    def get_article(article_id: str) -> Optional[Reference]:
        """Get article by ID"""
        collection = DatabaseService._get_collection()
        try:
            doc = collection.find_one({'_id': ObjectId(article_id)})
            if doc:
                return Reference(**doc)
        except Exception:
            pass
        return None
    
    @staticmethod
    def list_articles(
        limit: int = 10,
        skip: int = 0,
        sort_by: str = '_id',
        sort_order: int = DESCENDING
    ) -> tuple[List[Reference], int]:
        """List articles with pagination"""
        collection = DatabaseService._get_collection()
        
        total = collection.count_documents({})
        docs = list(
            collection.find()
            .sort(sort_by, sort_order)
            .skip(skip)
            .limit(limit)
        )
        
        articles = [Reference(**doc) for doc in docs]
        return articles, total
    
    @staticmethod
    def update_article(article_id: str, update_data: ReferenceUpdate) -> Optional[Reference]:
        """Update article"""
        collection = DatabaseService._get_collection()
        
        try:
            update_dict = update_data.dict(exclude_none=True)
            update_dict['atualizado_em'] = datetime.utcnow()
            
            result = collection.find_one_and_update(
                {'_id': ObjectId(article_id)},
                {'$set': update_dict},
                return_document=True
            )
            
            if result:
                return Reference(**result)
        except Exception as e:
            raise ValueError(f"Erro ao atualizar artigo: {str(e)}")
        
        return None
    
    @staticmethod
    def delete_article(article_id: str) -> bool:
        """Delete article"""
        collection = DatabaseService._get_collection()
        
        try:
            result = collection.delete_one({'_id': ObjectId(article_id)})
            return result.deleted_count > 0
        except Exception:
            return False
    
    @staticmethod
    def search_articles(query: str, limit: int = 10) -> List[Reference]:
        """Search articles by title, author, or species"""
        collection = DatabaseService._get_collection()
        
        search_filter = {
            '$or': [
                {'titulo': {'$regex': query, '$options': 'i'}},
                {'autores': {'$regex': query, '$options': 'i'}},
                {'especies.nomeCientifico': {'$regex': query, '$options': 'i'}},
                {'local': {'$regex': query, '$options': 'i'}}
            ]
        }
        
        docs = list(collection.find(search_filter).limit(limit))
        return [Reference(**doc) for doc in docs]
    
    @staticmethod
    def check_duplicate(titulo: str, ano: int, primeiro_autor: str) -> Optional[Reference]:
        """Check for duplicate article using fuzzy matching"""
        collection = DatabaseService._get_collection()
        
        doc = collection.find_one({
            'titulo': {'$regex': titulo[:20], '$options': 'i'},
            'ano': ano,
            'autores': {'$regex': f'^{primeiro_autor}', '$options': 'i'}
        })
        
        if doc:
            return Reference(**doc)
        return None
    
    @staticmethod
    def get_all_articles() -> List[Reference]:
        """Get all articles (for backup)"""
        collection = DatabaseService._get_collection()
        docs = list(collection.find())
        return [Reference(**doc) for doc in docs]
    
    @staticmethod
    def count_articles() -> int:
        """Count total articles"""
        collection = DatabaseService._get_collection()
        return collection.count_documents({})
