"""FastAPI application factory"""
from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from fastapi.staticfiles import StaticFiles
from pathlib import Path
import os
from .routers import health, articles, extraction, configuration, database
from .database.connection import MongoDBConnection

# Create FastAPI application
app = FastAPI(
    title="Etnopapers API",
    description="API for ethnobotany metadata extraction",
    version="2.0.0"
)

# CORS configuration for desktop app
app.add_middleware(
    CORSMiddleware,
    allow_origins=["http://localhost:3000", "http://localhost:8000", "http://127.0.0.1:8000"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Include routers
app.include_router(health.router, prefix="/api", tags=["health"])
app.include_router(articles.router, prefix="/api", tags=["articles"])
app.include_router(extraction.router, prefix="/api", tags=["extraction"])
app.include_router(configuration.router, prefix="/api", tags=["configuration"])
app.include_router(database.router, prefix="/api", tags=["database"])

# Serve React SPA if built
try:
    static_files_path = Path(__file__).parent.parent.parent / "frontend" / "dist"
    if static_files_path.exists():
        app.mount("/", StaticFiles(directory=str(static_files_path), html=True), name="static")
except Exception as e:
    print(f"Note: React SPA not found - {e}")

@app.on_event("startup")
async def startup_event():
    """Initialize on startup"""
    print("Etnopapers API iniciando...")
    try:
        MongoDBConnection.get_connection()
        print("Conectado ao MongoDB com sucesso")
    except Exception as e:
        print(f"Aviso: Erro ao conectar ao MongoDB na inicialização: {e}")

@app.on_event("shutdown")
async def shutdown_event():
    """Cleanup on shutdown"""
    MongoDBConnection.close()
    print("Etnopapers API finalizado")

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(
        app,
        host="127.0.0.1",
        port=8000,
        log_level="info"
    )
