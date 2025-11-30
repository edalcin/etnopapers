import logging
from typing import List
from ..database.connection import get_db
from ..models.reference import Reference

logger = logging.getLogger(__name__)

class SearchService:
    """Service for article search and filtering"""
    
    @staticmethod
    def search_articles(query: str, limit: int = 20) -> List[Reference]:
        """Full-text search articles by title, author, species, location"""
        db = get_db()
        collection = db['referencias']
        
        search_filter = {
            '\$or': [
                {'titulo': {'\$regex': query, '\$options': 'i'}},
                {'autores': {'\$regex': query, '\$options': 'i'}},
                {'especies.nomeCientifico': {'\$regex': query, '\$options': 'i'}},
                {'especies.vernacular': {'\$regex': query, '\$options': 'i'}},
                {'local': {'\$regex': query, '\$options': 'i'}},
                {'pais': {'\$regex': query, '\$options': 'i'}},
                {'estado': {'\$regex': query, '\$options': 'i'}},
            ]
        }
        
        try:
            docs = list(collection.find(search_filter).limit(limit))
            return [Reference(**doc) for doc in docs]
        except Exception as e:
            logger.error(f"Erro na busca: {e}")
            return []
    
    @staticmethod
    def filter_by_year(year: int, limit: int = 20) -> List[Reference]:
        """Filter articles by publication year"""
        db = get_db()
        collection = db['referencias']
        
        docs = list(collection.find({'ano': year}).limit(limit))
        return [Reference(**doc) for doc in docs]
    
    @staticmethod
    def filter_by_country(country: str, limit: int = 20) -> List[Reference]:
        """Filter articles by country"""
        db = get_db()
        collection = db['referencias']
        
        docs = list(collection.find({'pais': {'\$regex': country, '\$options': 'i'}}).limit(limit))
        return [Reference(**doc) for doc in docs]
    
    @staticmethod
    def filter_by_use_type(use_type: str, limit: int = 20) -> List[Reference]:
        """Filter articles by type of use"""
        db = get_db()
        collection = db['referencias']
        
        docs = list(collection.find({'tipo_de_uso': {'\$regex': use_type, '\$options': 'i'}}).limit(limit))
        return [Reference(**doc) for doc in docs]
    
    @staticmethod
    def filter_by_species(scientific_name: str, limit: int = 20) -> List[Reference]:
        """Filter articles by species scientific name"""
        db = get_db()
        collection = db['referencias']
        
        docs = list(collection.find({
            'especies.nomeCientifico': {'\$regex': scientific_name, '\$options': 'i'}
        }).limit(limit))
        return [Reference(**doc) for doc in docs]
    
    @staticmethod
    def get_aggregated_stats() -> dict:
        """Get aggregated statistics about articles"""
        db = get_db()
        collection = db['referencias']
        
        total = collection.count_documents({})
        
        # Get unique countries
        countries = list(collection.distinct('pais'))
        
        # Get unique species count
        all_species = list(collection.distinct('especies.nomeCientifico'))
        
        # Get year range
        years = list(collection.distinct('ano'))
        
        # Get use types
        use_types = list(collection.distinct('tipo_de_uso'))
        
        return {
            'total_articles': total,
            'countries': len([c for c in countries if c]),
            'unique_species': len(all_species),
            'year_range': {
                'min': min(years) if years else None,
                'max': max(years) if years else None
            },
            'use_types': use_types
        }
