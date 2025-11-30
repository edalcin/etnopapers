import json
import zipfile
import io
import logging
from datetime import datetime
from typing import Tuple, Optional
from bson import json_util
from ..database.connection import get_db

logger = logging.getLogger(__name__)

class BackupService:
    """Service for database backup and export"""
    
    @staticmethod
    def get_all_articles_for_backup() -> list:
        """Get all articles from database for backup"""
        try:
            db = get_db()
            collection = db['referencias']
            docs = list(collection.find())
            return docs
        except Exception as e:
            logger.error(f"Erro ao buscar artigos para backup: {e}")
            raise
    
    @staticmethod
    def validate_database_integrity() -> Tuple[bool, Optional[str]]:
        """Validate database integrity before backup"""
        try:
            db = get_db()
            
            # Check MongoDB connection
            db.admin.command('ping')
            
            # Check if collection exists
            collections = db.list_collection_names()
            if 'referencias' not in collections:
                return False, "Colecao referencias nao encontrada"
            
            # Count documents
            collection = db['referencias']
            total_docs = collection.count_documents({})
            
            if total_docs == 0:
                logger.warning("Base de dados vazia - nenhum artigo para fazer backup")
            
            return True, None
            
        except Exception as e:
            logger.error(f"Erro ao validar integridade: {e}")
            return False, f"Erro na validacao: {str(e)}"
    
    @staticmethod
    def create_backup_zip() -> Tuple[bytes, str]:
        """Create ZIP file with database backup"""
        try:
            # Validate database integrity
            is_valid, error_msg = BackupService.validate_database_integrity()
            if not is_valid:
                raise ValueError(error_msg or "Database validation failed")
            
            # Get all articles
            articles = BackupService.get_all_articles_for_backup()
            
            # Create ZIP in memory
            zip_buffer = io.BytesIO()
            with zipfile.ZipFile(zip_buffer, 'w', zipfile.ZIP_DEFLATED) as zip_file:
                # Add articles as JSON
                articles_json = json.dumps(
                    articles,
                    default=json_util.default,
                    ensure_ascii=False,
                    indent=2
                )
                
                zip_file.writestr('referencias.json', articles_json)
                
                # Add metadata
                timestamp = datetime.utcnow().isoformat()
                metadata = {
                    'backup_timestamp': timestamp,
                    'total_articles': len(articles),
                    'database_name': 'etnopapers',
                    'format_version': '1.0'
                }
                
                metadata_json = json.dumps(metadata, ensure_ascii=False, indent=2)
                zip_file.writestr('backup_metadata.json', metadata_json)
                
                # Add checksum
                import hashlib
                articles_hash = hashlib.sha256(articles_json.encode()).hexdigest()
                checksum_data = {'sha256': articles_hash, 'timestamp': timestamp}
                checksum_json = json.dumps(checksum_data, indent=2)
                zip_file.writestr('checksum.json', checksum_json)
            
            zip_buffer.seek(0)
            zip_bytes = zip_buffer.getvalue()
            
            # Generate filename with timestamp
            timestamp = datetime.utcnow().strftime('%Y%m%d_%H%M%S')
            filename = f"etnopapers_backup_{timestamp}.zip"
            
            return zip_bytes, filename
            
        except Exception as e:
            logger.error(f"Erro ao criar backup ZIP: {e}")
            raise
    
    @staticmethod
    def restore_from_backup(zip_bytes: bytes) -> Tuple[bool, str]:
        """Restore articles from backup ZIP file"""
        try:
            zip_buffer = io.BytesIO(zip_bytes)
            
            with zipfile.ZipFile(zip_buffer, 'r') as zip_file:
                # Read articles
                if 'referencias.json' not in zip_file.namelist():
                    return False, "Arquivo referencias.json nao encontrado"
                
                articles_data = zip_file.read('referencias.json')
                articles = json.loads(articles_data, object_hook=json_util.object_hook)
                
                # Restore to database
                db = get_db()
                collection = db['referencias']
                
                if not isinstance(articles, list):
                    return False, "Formato invalido do backup"
                
                # Insert restored articles
                if articles:
                    result = collection.insert_many(articles, ordered=False)
                    logger.info(f"Restaurados {len(result.inserted_ids)} artigos")
                    return True, f"Restaurados {len(result.inserted_ids)} artigos"
                else:
                    return True, "Nenhum artigo para restaurar"
            
        except Exception as e:
            logger.error(f"Erro ao restaurar backup: {e}")
            return False, f"Erro na restauracao: {str(e)}"
