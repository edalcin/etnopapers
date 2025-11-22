"""
Database router for FastAPI

Handles database operations:
- GET /api/database/info - Get database statistics
- GET /api/database/download - Download MongoDB database backup (JSON export)
- POST /api/database/backup - Create MongoDB backup
"""

import json
import logging
import tempfile
from datetime import datetime
from pathlib import Path

from fastapi import APIRouter
from fastapi import HTTPException
from fastapi.responses import FileResponse

from backend.database.connection import get_db

logger = logging.getLogger(__name__)

router = APIRouter(prefix="/api/database", tags=["database"])


@router.get("/info")
async def get_database_info():
    """Get MongoDB database statistics and info"""
    try:
        db = get_db()
        info = db.get_database_info()

        return {
            "status": "ok",
            "database": {
                "type": "MongoDB",
                "size_bytes": info["size_bytes"],
                "size_mb": round(info["size_mb"], 2),
                "collections": info["collections"],
                "collection_info": info["collection_info"],
            },
            "timestamp": datetime.now().isoformat(),
        }
    except Exception as e:
        logger.error(f"Error getting database info: {e}")
        raise HTTPException(
            status_code=500, detail="Erro ao obter informações do banco de dados"
        )


@router.get("/download")
async def download_database():
    """
    Download a backup of the MongoDB database as JSON export

    Exports all collections as a single JSON file.
    """
    import shutil

    try:
        db = get_db()

        # Generate filename with timestamp
        timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
        backup_filename = f"etnopapers_backup_{timestamp}"

        # Create temporary directory for backup
        with tempfile.TemporaryDirectory() as temp_dir:
            temp_dir_path = Path(temp_dir)
            json_file = temp_dir_path / f"{backup_filename}.json"

            # Export all collections to JSON
            backup_data = {}
            collections = db.db.list_collection_names()

            for collection_name in collections:
                collection = db.db[collection_name]
                docs = list(collection.find())

                # Convert ObjectId to string for JSON serialization
                for doc in docs:
                    if "_id" in doc:
                        doc["_id"] = str(doc["_id"])

                backup_data[collection_name] = docs

            # Write to JSON file
            with open(json_file, "w", encoding="utf-8") as f:
                json.dump(backup_data, f, indent=2, ensure_ascii=False, default=str)

            if not json_file.exists():
                raise HTTPException(
                    status_code=500, detail="Falha ao criar backup do banco de dados"
                )

            logger.info(f"Database backup export created: {backup_filename}.json")

            return FileResponse(
                path=json_file,
                filename=f"{backup_filename}.json",
                media_type="application/json",
                headers={
                    "Content-Disposition": f"attachment; filename={backup_filename}.json",
                    "Cache-Control": "no-cache",
                },
            )

    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Error downloading database backup: {e}")
        raise HTTPException(
            status_code=500, detail="Erro ao exportar banco de dados"
        )


@router.post("/backup")
async def create_backup():
    """
    Create a backup of the MongoDB database

    Exports all collections as JSON format (same as /download endpoint).
    Returns the exported data directly.
    """
    try:
        db = get_db()
        timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")

        # Export all collections to JSON
        backup_data = {}
        collections = db.db.list_collection_names()

        for collection_name in collections:
            collection = db.db[collection_name]
            docs = list(collection.find())

            # Convert ObjectId to string for JSON serialization
            for doc in docs:
                if "_id" in doc:
                    doc["_id"] = str(doc["_id"])

            backup_data[collection_name] = docs

        logger.info(f"Database backup created at {timestamp}")

        return {
            "status": "success",
            "message": "Backup criado com sucesso",
            "timestamp": timestamp,
            "collections": list(backup_data.keys()),
            "total_documents": sum(len(docs) for docs in backup_data.values()),
            "data": backup_data,
        }

    except Exception as e:
        logger.error(f"Error creating backup: {e}")
        raise HTTPException(
            status_code=500,
            detail="Erro ao criar backup do banco de dados"
        )
