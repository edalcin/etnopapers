"""
Etnopapers Backend - Main FastAPI Application
"""

import logging
import json
from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import JSONResponse

from config import settings
from database.connection import get_db
from database.init_db import init_database
from routers import articles_router, species_router, database_router

# Configure logging
logger = logging.getLogger(__name__)

# Initialize database
logger.info("Initializing database...")
init_database(settings.DATABASE_PATH)
db = get_db()
logger.info("Database initialized successfully")

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


@app.on_event("startup")
async def startup_event():
    """Initialize on startup"""
    logger.info(f"Etnopapers API starting - Environment: {settings.ENVIRONMENT}")
    logger.info(f"Database: {settings.DATABASE_PATH}")


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
                "tables": info["tables"],
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
    """API root endpoint"""
    return {
        "message": "Etnopapers API",
        "version": "0.1.0",
        "docs": "/docs",
        "redoc": "/redoc",
        "health": "/health",
    }


if __name__ == "__main__":
    import uvicorn

    uvicorn.run(
        "main:app",
        host=settings.HOST,
        port=settings.PORT,
        reload=settings.ENVIRONMENT == "development",
        log_level=settings.LOG_LEVEL.lower(),
    )
