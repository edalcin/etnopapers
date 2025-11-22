"""
Etnopapers Backend - Main FastAPI Application
"""

import logging
from pathlib import Path

from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import FileResponse
from fastapi.responses import JSONResponse
from fastapi.staticfiles import StaticFiles

from backend.config import settings
from backend.database.connection import get_db
from backend.database.init_db import init_database
from backend.routers import articles_router
from backend.routers import database_router
from backend.routers import species_router

# Configure logging
logger = logging.getLogger(__name__)

# Initialize MongoDB database
logger.info("Initializing MongoDB database...")
init_database(settings.MONGO_URI)
db = get_db()
logger.info("MongoDB database initialized successfully")

# Create FastAPI app
app = FastAPI(
    title="Etnopapers API",
    description="Sistema de Extração de Metadados de Artigos Etnobotânicos",
    version="0.1.0",
    docs_url="/docs",
    redoc_url="/redoc",
    openapi_url="/openapi.json",
)

# Configure CORS
app.add_middleware(
    CORSMiddleware,
    allow_origins=settings.CORS_ORIGINS,
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Include routers
app.include_router(articles_router)
app.include_router(species_router)
app.include_router(database_router)

# Serve frontend static files
frontend_dist = Path(__file__).parent.parent / "frontend" / "dist"
if frontend_dist.exists():
    app.mount("/static", StaticFiles(directory=str(frontend_dist)), name="static")
    logger.info(f"Frontend static files mounted from {frontend_dist}")


@app.on_event("startup")
async def startup_event():
    """Initialize on startup"""
    logger.info(f"Etnopapers API starting - Environment: {settings.ENVIRONMENT}")
    logger.info(f"Database: MongoDB")


@app.on_event("shutdown")
async def shutdown_event():
    """Cleanup on shutdown"""
    logger.info("Etnopapers API shutting down")


@app.get("/health")
async def health_check():
    """Health check endpoint"""
    try:
        # Test database connection
        info = db.get_database_info()
        return {
            "status": "healthy",
            "version": "0.1.0",
            "environment": settings.ENVIRONMENT,
            "database": {
                "size_mb": info["size_mb"],
                "collections": info["collections"],
            },
        }
    except Exception as e:
        logger.error(f"Health check failed: {e}")
        return JSONResponse(
            status_code=503,
            content={"status": "unhealthy", "error": str(e)},
        )


@app.get("/")
async def root():
    """Root endpoint - serve frontend"""
    frontend_dist = Path(__file__).parent.parent / "frontend" / "dist"
    index_file = frontend_dist / "index.html"

    if index_file.exists():
        return FileResponse(str(index_file))

    # Fallback to API info if frontend not built
    return {
        "message": "Etnopapers API",
        "version": "0.1.0",
        "docs": "/docs",
        "redoc": "/redoc",
        "health": "/health",
    }


@app.get("/{path:path}")
async def serve_spa(path: str):
    """Serve SPA - fallback to index.html for client-side routing"""
    # Don't intercept API routes or special paths
    if path.startswith("api/") or path in ["docs", "redoc", "openapi.json", "health"]:
        return {"error": "Not found"}

    frontend_dist = Path(__file__).parent.parent / "frontend" / "dist"
    file_path = frontend_dist / path

    if file_path.exists() and file_path.is_file():
        return FileResponse(str(file_path))

    # Fallback to index.html for SPA routing
    index_file = frontend_dist / "index.html"
    if index_file.exists():
        return FileResponse(str(index_file))

    return {"error": "Not found"}


if __name__ == "__main__":
    import uvicorn

    uvicorn.run(
        "main:app",
        host=settings.HOST,
        port=settings.PORT,
        reload=settings.ENVIRONMENT == "development",
        log_level=settings.LOG_LEVEL.lower(),
    )
