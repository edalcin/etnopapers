from typing import Optional, Dict, Any
from difflib import SequenceMatcher
from ..models.reference import Reference, ReferenceCreate
from ..database.connection import get_db

class ArticleService:
    """Service layer for article-specific operations"""
    
    @staticmethod
    def check_duplicate_by_doi(doi: str) -> Optional[Reference]:
        """Check if article with same DOI exists"""
        if not doi:
            return None
        
        db = get_db()
        collection = db['referencias']
        doc = collection.find_one({'doi': doi})
        
        if doc:
            return Reference(**doc)
        return None
    
    @staticmethod
    def check_fuzzy_duplicate(
        titulo: str,
        ano: int,
        primeiro_autor: str,
        similarity_threshold: float = 0.85
    ) -> Optional[Dict[str, Any]]:
        """Check for fuzzy duplicate based on titulo + ano + author"""
        if not titulo or not ano or not primeiro_autor:
            return None
        
        db = get_db()
        collection = db['referencias']
        
        # Find candidates with same year and similar first author
        candidates = list(collection.find({
            'ano': ano,
            'autores': {'$exists': True}
        }))
        
        for candidate in candidates:
            # Check author similarity
            authors = candidate.get('autores', [])
            if not authors:
                continue
            
            author_match = SequenceMatcher(
                None,
                primeiro_autor.lower(),
                authors[0].lower()
            ).ratio() > 0.8
            
            if not author_match:
                continue
            
            # Check title similarity
            title_match = SequenceMatcher(
                None,
                titulo.lower(),
                candidate.get('titulo', '').lower()
            ).ratio()
            
            if title_match >= similarity_threshold:
                return {
                    'duplicate': True,
                    'similarity': title_match,
                    'conflicting_id': str(candidate.get('_id')),
                    'conflicting_article': candidate.get('titulo')
                }
        
        return None
    
    @staticmethod
    def get_conflict_response(
        incoming_article: ReferenceCreate,
        duplicate_info: Dict[str, Any]
    ) -> Dict[str, Any]:
        """Generate conflict response for UI"""
        return {
            'status': 'conflito_duplicado',
            'codigo': 409,
            'mensagem': 'Possível artigo duplicado detectado',
            'detalhes': {
                'tipo': 'fuzzy_match',
                'confianca': round(duplicate_info.get('similarity', 0) * 100, 1),
                'artigo_conflitante_id': duplicate_info.get('conflicting_id'),
                'artigo_conflitante_titulo': duplicate_info.get('conflicting_article'),
                'sugestao': 'Mesclar com artigo existente ou criar novo'
            }
        }
    
    @staticmethod
    def validate_article_data(article: ReferenceCreate) -> tuple[bool, Optional[str]]:
        """Validate article data before saving"""
        if not article.titulo or len(article.titulo.strip()) == 0:
            return False, "Título é obrigatório"
        
        if article.ano and (article.ano < 1900 or article.ano > 2100):
            return False, "Ano inválido"
        
        if article.autores and not isinstance(article.autores, list):
            return False, "Autores deve ser uma lista"
        
        return True, None
