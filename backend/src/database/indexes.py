from pymongo import ASCENDING, DESCENDING, TEXT
from .connection import get_db

def create_indexes():
    """Create MongoDB indexes for performance"""
    db = get_db()
    collection = db['referencias']
    
    # Drop existing indexes (except default _id)
    try:
        collection.drop_indexes()
    except Exception:
        pass
    
    # Create indexes
    indexes = [
        # Unique index on DOI
        {
            'keys': [('doi', ASCENDING)],
            'kwargs': {'unique': True, 'sparse': True}
        },
        # Index on status
        {
            'keys': [('status', ASCENDING)],
            'kwargs': {}
        },
        # Index on year
        {
            'keys': [('ano', DESCENDING)],
            'kwargs': {}
        },
        # Text index on title
        {
            'keys': [('titulo', TEXT)],
            'kwargs': {}
        },
        # Index on location fields
        {
            'keys': [('pais', ASCENDING), ('estado', ASCENDING)],
            'kwargs': {}
        },
        # Index on creation date for sorting
        {
            'keys': [('criado_em', DESCENDING)],
            'kwargs': {}
        }
    ]
    
    for index in indexes:
        collection.create_index(index['keys'], **index['kwargs'])
    
    print("Índices MongoDB criados com sucesso")

def validate_indexes():
    """Validate that all required indexes exist"""
    db = get_db()
    collection = db['referencias']
    
    index_info = collection.index_information()
    required_indexes = ['doi_1', 'status_1', 'ano_-1', 'titulo_text']
    
    for required in required_indexes:
        if required not in index_info:
            print(f"Aviso: Índice esperado não encontrado: {required}")
            return False
    
    return True
