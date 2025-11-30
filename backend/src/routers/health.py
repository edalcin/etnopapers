from fastapi import APIRouter, Depends
from ..services.ollama_service import OllamaService
from ..database.connection import get_db

router = APIRouter()

@router.get("/health")
async def health_check():
    """Health check endpoint"""
    return {
        "status": "ok",
        "versao": "2.0.0",
        "aplicacao": "Etnopapers"
    }

@router.get("/health/ollama")
async def ollama_health():
    """Check Ollama service health"""
    is_healthy = await OllamaService.check_health()
    
    if is_healthy:
        return {
            "status": "ok",
            "servico": "Ollama",
            "url": OllamaService.get_ollama_url(),
            "modelo": OllamaService.get_ollama_model()
        }
    else:
        return {
            "status": "indisponivel",
            "servico": "Ollama",
            "mensagem": "Serviço Ollama não encontrado"
        }, 503

@router.get("/health/mongodb")
async def mongodb_health():
    """Check MongoDB connection health"""
    try:
        db = get_db()
        db.admin.command('ping')
        return {
            "status": "ok",
            "servico": "MongoDB",
            "database": db.name
        }
    except Exception as e:
        return {
            "status": "indisponivel",
            "servico": "MongoDB",
            "mensagem": str(e)
        }, 503
