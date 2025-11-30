from fastapi import APIRouter, HTTPException
from pymongo import MongoClient
from pymongo.errors import ConnectionFailure, ServerSelectionTimeoutError
from ..models.configuration import MongoDBConfig, OllamaConfig
from ..services.config_service import ConfigService
from ..services.ollama_service import OllamaService

router = APIRouter()

@router.post("/config/validate-mongo")
async def validate_mongo_connection(config: MongoDBConfig):
    """Validate MongoDB connection string"""
    try:
        # Test the connection
        client = MongoClient(
            config.mongo_uri,
            serverSelectionTimeoutMS=5000,
            connectTimeoutMS=10000
        )
        
        # Try to ping the database
        client.admin.command('ping')
        client.close()
        
        return {
            "status": "ok",
            "mensagem": "Conexão com MongoDB validada com sucesso",
            "valido": True
        }
    except (ConnectionFailure, ServerSelectionTimeoutError) as e:
        return {
            "status": "erro",
            "mensagem": f"Falha ao conectar ao MongoDB: {str(e)}",
            "valido": False,
            "detalhes": str(e)
        }, 400
    except Exception as e:
        return {
            "status": "erro",
            "mensagem": f"Erro ao validar conexão: {str(e)}",
            "valido": False,
            "detalhes": str(e)
        }, 400

@router.post("/config/save")
async def save_configuration(config: MongoDBConfig):
    """Save MongoDB configuration to local config file"""
    try:
        # Validate before saving
        client = MongoClient(
            config.mongo_uri,
            serverSelectionTimeoutMS=5000,
            connectTimeoutMS=10000
        )
        client.admin.command('ping')
        client.close()
        
        # Save configuration
        success = ConfigService.save_mongo_uri(config.mongo_uri)
        
        if success:
            return {
                "status": "ok",
                "mensagem": "Configuração salva com sucesso",
                "salvo": True
            }
        else:
            return {
                "status": "erro",
                "mensagem": "Falha ao salvar configuração",
                "salvo": False
            }, 500
    except Exception as e:
        return {
            "status": "erro",
            "mensagem": f"Erro ao salvar configuração: {str(e)}",
            "salvo": False,
            "detalhes": str(e)
        }, 400

@router.get("/config/status")
async def get_config_status():
    """Get application configuration status"""
    ConfigService.load_config_from_env()
    is_configured = ConfigService.is_configured()
    
    if is_configured:
        mongo_uri = ConfigService.get_mongo_uri()
        # Don't expose the full URI for security
        uri_preview = mongo_uri[:50] + "..." if len(mongo_uri) > 50 else mongo_uri
        
        return {
            "configurado": True,
            "mongodb": {
                "configurado": True,
                "uri_preview": uri_preview
            },
            "mensagem": "Aplicação completamente configurada"
        }
    else:
        return {
            "configurado": False,
            "mongodb": {
                "configurado": False
            },
            "mensagem": "Configuração necessária"
        }, 200

@router.post("/config/ollama")
async def save_ollama_configuration(config: OllamaConfig):
    """Save Ollama configuration"""
    try:
        # Validate Ollama connection
        import httpx
        async with httpx.AsyncClient(timeout=5.0) as client:
            response = await client.get(f"{config.ollama_url}/api/tags")
            if response.status_code != 200:
                return {
                    "status": "erro",
                    "mensagem": "Ollama não respondeu",
                    "valido": False
                }, 400
        
        # Save configuration
        success = ConfigService.save_ollama_config(config.ollama_url, config.ollama_model)
        
        if success:
            return {
                "status": "ok",
                "mensagem": "Configuração Ollama salva com sucesso",
                "salvo": True
            }
        else:
            return {
                "status": "erro",
                "mensagem": "Falha ao salvar configuração Ollama",
                "salvo": False
            }, 500
    except Exception as e:
        return {
            "status": "erro",
            "mensagem": f"Erro ao salvar configuração Ollama: {str(e)}",
            "salvo": False
        }, 400

@router.get("/config/ollama/models")
async def get_available_ollama_models():
    """Get list of available Ollama models"""
    try:
        models = await OllamaService.get_available_models()
        return {
            "status": "ok",
            "modelos": models,
            "modelo_recomendado": "qwen2.5:7b-instruct-q4_K_M"
        }
    except Exception as e:
        return {
            "status": "erro",
            "mensagem": f"Erro ao obter modelos: {str(e)}",
            "modelos": []
        }, 400
