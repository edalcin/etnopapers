import os
from pathlib import Path
from typing import Optional
from dotenv import load_dotenv, set_key
from ..models.configuration import MongoDBConfig, OllamaConfig

class ConfigService:
    """Service for managing application configuration"""
    
    ENV_FILE = Path.home() / '.etnopapers' / '.env'
    
    @staticmethod
    def ensure_config_dir():
        """Ensure config directory exists"""
        ConfigService.ENV_FILE.parent.mkdir(parents=True, exist_ok=True)
    
    @staticmethod
    def get_mongo_uri() -> str:
        """Get MongoDB URI from environment"""
        load_dotenv(ConfigService.ENV_FILE)
        return os.getenv(
            'MONGO_URI',
            'mongodb://localhost:27017/etnopapers'
        )
    
    @staticmethod
    def get_ollama_url() -> str:
        """Get Ollama URL from environment"""
        load_dotenv(ConfigService.ENV_FILE)
        return os.getenv(
            'OLLAMA_URL',
            'http://localhost:11434'
        )
    
    @staticmethod
    def get_ollama_model() -> str:
        """Get Ollama model from environment"""
        load_dotenv(ConfigService.ENV_FILE)
        return os.getenv(
            'OLLAMA_MODEL',
            'qwen2.5:7b-instruct-q4_K_M'
        )
    
    @staticmethod
    def save_mongo_uri(uri: str) -> bool:
        """Save MongoDB URI to config file"""
        try:
            ConfigService.ensure_config_dir()
            if not ConfigService.ENV_FILE.exists():
                ConfigService.ENV_FILE.touch()
            
            set_key(str(ConfigService.ENV_FILE), 'MONGO_URI', uri)
            os.environ['MONGO_URI'] = uri
            return True
        except Exception as e:
            print(f"Erro ao salvar configuração MongoDB: {e}")
            return False
    
    @staticmethod
    def save_ollama_config(url: str, model: str) -> bool:
        """Save Ollama configuration to config file"""
        try:
            ConfigService.ensure_config_dir()
            if not ConfigService.ENV_FILE.exists():
                ConfigService.ENV_FILE.touch()
            
            set_key(str(ConfigService.ENV_FILE), 'OLLAMA_URL', url)
            set_key(str(ConfigService.ENV_FILE), 'OLLAMA_MODEL', model)
            os.environ['OLLAMA_URL'] = url
            os.environ['OLLAMA_MODEL'] = model
            return True
        except Exception as e:
            print(f"Erro ao salvar configuração Ollama: {e}")
            return False
    
    @staticmethod
    def is_configured() -> bool:
        """Check if application is configured"""
        load_dotenv(ConfigService.ENV_FILE)
        mongo_uri = os.getenv('MONGO_URI')
        return mongo_uri is not None and len(mongo_uri) > 0
    
    @staticmethod
    def load_config_from_env():
        """Load configuration from environment variables"""
        load_dotenv(ConfigService.ENV_FILE)
        load_dotenv()
