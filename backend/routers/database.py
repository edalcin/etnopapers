"""
Database router for FastAPI

Handles database operations:
- GET /api/database/download - Download SQLite database file
- GET /api/database/info - Get database statistics
"""

import logging
from pathlib import Path
from datetime import datetime
from fastapi import APIRouter, HTTPException
from fastapi.responses import FileResponse
from backend.config import settings
from backend.database.connection import get_db

logger = logging.getLogger(__name__)

router = APIRouter(prefix="/api/database", tags=["database"])


@router.get("/info")
async def get_database_info():
    """Get database statistics and info"""
    try:
        db = get_db()
        info = db.get_database_info()

        return {
            "status": "ok",
            "database": {
                "path": info["db_path"],
                "size_bytes": info["size_bytes"],
                "size_mb": round(info["size_mb"], 2),
                "tables": info["tables"],
                "table_info": info["table_info"],
            },
            "timestamp": datetime.now().isoformat(),
        }
    except Exception as e:
        logger.error(f"Error getting database info: {e}")
        raise HTTPException(
            status_code=500, detail="Erro ao obter informações do banco"
        )


@router.get("/download")
async def download_database():
    """
    Download the complete SQLite database file

    Runs integrity check before returning the file.
    Returns the database as a downloadable file.
    """
    try:
        db = get_db()

        # Run integrity check
        integrity = db.get_integrity_check()
        if integrity and len(integrity) > 0 and integrity[0][0] != "ok":
            logger.error(f"Database integrity check failed: {integrity}")
            raise HTTPException(
                status_code=500,
                detail="Banco de dados corrompido - execute manutenção",
            )

        # Generate filename with timestamp
        timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
        filename = f"etnopapers_{timestamp}.db"

        db_path = Path(settings.DATABASE_PATH)

        if not db_path.exists():
            raise HTTPException(
                status_code=404, detail="Arquivo de banco de dados não encontrado"
            )

        logger.info(f"Database download initiated: {filename}")

        return FileResponse(
            path=db_path,
            filename=filename,
            media_type="application/octet-stream",
            headers={
                "Content-Disposition": f"attachment; filename={filename}",
                "Cache-Control": "no-cache",
            },
        )

    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Error downloading database: {e}")
        raise HTTPException(
            status_code=500, detail="Erro ao baixar banco de dados"
        )


@router.post("/backup")
async def create_backup():
    """
    Create a backup of the database

    Creates a timestamped copy of the database in the data directory.
    """
    try:
        db = get_db()
        db_path = Path(settings.DATABASE_PATH)

        if not db_path.exists():
            raise HTTPException(
                status_code=404, detail="Arquivo de banco de dados não encontrado"
            )

        # Create backup
        timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
        backup_filename = f"backup_etnopapers_{timestamp}.db"
        backup_path = db_path.parent / backup_filename

        # Copy file
        with open(db_path, "rb") as src:
            with open(backup_path, "wb") as dst:
                dst.write(src.read())

        logger.info(f"Database backup created: {backup_filename}")

        return {
            "status": "success",
            "message": "Backup criado com sucesso",
            "filename": backup_filename,
            "path": str(backup_path),
            "timestamp": datetime.now().isoformat(),
        }

    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Error creating backup: {e}")
        raise HTTPException(status_code=500, detail="Erro ao criar backup")
