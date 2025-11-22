"""
Database router for FastAPI

Handles database operations:
- GET /api/database/download - Download Mongita database backup
- GET /api/database/info - Get database statistics
"""

import logging
from datetime import datetime
from pathlib import Path

from fastapi import APIRouter
from fastapi import HTTPException
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
    Download a backup of the Mongita database

    Creates a zip archive of the database directory and returns it as downloadable file.
    """
    import shutil
    import tempfile

    try:
        db_path = Path(settings.DATABASE_PATH)

        if not db_path.exists():
            raise HTTPException(
                status_code=404, detail="Banco de dados Mongita não encontrado"
            )

        # Generate filename with timestamp
        timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
        backup_filename = f"etnopapers_backup_{timestamp}"

        # Create temporary zip file
        with tempfile.TemporaryDirectory() as temp_dir:
            temp_dir_path = Path(temp_dir)
            zip_path = temp_dir_path / backup_filename

            # Create zip archive of database directory
            shutil.make_archive(
                str(zip_path),
                "zip",
                db_path.parent,
                db_path.name
            )

            zip_file = zip_path.with_suffix(".zip")

            if not zip_file.exists():
                raise HTTPException(
                    status_code=500, detail="Falha ao criar backup do banco de dados"
                )

            logger.info(f"Database backup download initiated: {backup_filename}.zip")

            return FileResponse(
                path=zip_file,
                filename=f"{backup_filename}.zip",
                media_type="application/zip",
                headers={
                    "Content-Disposition": f"attachment; filename={backup_filename}.zip",
                    "Cache-Control": "no-cache",
                },
            )

    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Error downloading database backup: {e}")
        raise HTTPException(
            status_code=500, detail="Erro ao baixar backup do banco de dados"
        )


@router.post("/backup")
async def create_backup():
    """
    Create a backup of the Mongita database

    Creates a timestamped copy of the database directory.
    """
    import shutil

    try:
        db_path = Path(settings.DATABASE_PATH)

        if not db_path.exists():
            raise HTTPException(
                status_code=404, detail="Diretório do banco de dados Mongita não encontrado"
            )

        # Create backup directory
        timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
        backup_dirname = f"backup_etnopapers_{timestamp}"
        backup_path = db_path.parent / backup_dirname

        # Copy entire database directory
        shutil.copytree(db_path, backup_path)

        logger.info(f"Database backup created: {backup_dirname}")

        return {
            "status": "success",
            "message": "Backup criado com sucesso",
            "directory": backup_dirname,
            "path": str(backup_path),
            "timestamp": datetime.now().isoformat(),
        }

    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Error creating backup: {e}")
        raise HTTPException(status_code=500, detail="Erro ao criar backup")
