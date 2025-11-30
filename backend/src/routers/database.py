from fastapi import APIRouter, HTTPException
from fastapi.responses import StreamingResponse
import io
import logging
from ..services.backup_service import BackupService

logger = logging.getLogger(__name__)
router = APIRouter()

@router.get("/database/download")
async def download_database():
    """Download database backup as ZIP file"""
    try:
        # Create backup ZIP
        zip_bytes, filename = BackupService.create_backup_zip()

        # Return as streaming response
        return StreamingResponse(
            iter([zip_bytes]),
            media_type="application/zip",
            headers={"Content-Disposition": f"attachment; filename={filename}"}
        )

    except Exception as e:
        logger.error(f"Erro ao fazer download do backup: {e}")
        raise HTTPException(
            status_code=500,
            detail=f"Erro ao criar backup: {str(e)}"
        )

@router.get("/database/stats")
async def get_database_stats():
    """Get database statistics"""
    try:
        is_valid, error = BackupService.validate_database_integrity()

        if not is_valid:
            raise HTTPException(status_code=500, detail=error or "Database validation failed")

        articles = BackupService.get_all_articles_for_backup()

        return {
            "status": "ok",
            "total_articles": len(articles),
            "database_name": "etnopapers",
            "mensagem": f"Base de dados com {len(articles)} artigos"
        }

    except Exception as e:
        logger.error(f"Erro ao obter stats: {e}")
        raise HTTPException(status_code=500, detail=str(e))
