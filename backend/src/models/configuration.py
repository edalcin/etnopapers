from pydantic import BaseModel, Field
from typing import Optional

class MongoDBConfig(BaseModel):
    """MongoDB configuration"""
    mongo_uri: str = Field(..., description="MongoDB connection URI")
    
    class Config:
        json_schema_extra = {
            "example": {
                "mongo_uri": "mongodb://localhost:27017/etnopapers"
            }
        }

class OllamaConfig(BaseModel):
    """Ollama configuration"""
    ollama_url: str = Field(default="http://localhost:11434", description="Ollama service URL")
    ollama_model: str = Field(default="qwen2.5:7b-instruct-q4_K_M", description="Ollama model name")
    
    class Config:
        json_schema_extra = {
            "example": {
                "ollama_url": "http://localhost:11434",
                "ollama_model": "qwen2.5:7b-instruct-q4_K_M"
            }
        }

class ApplicationConfig(BaseModel):
    """Complete application configuration"""
    mongodb: MongoDBConfig
    ollama: Optional[OllamaConfig] = None
    
    class Config:
        json_schema_extra = {
            "example": {
                "mongodb": {
                    "mongo_uri": "mongodb://localhost:27017/etnopapers"
                },
                "ollama": {
                    "ollama_url": "http://localhost:11434",
                    "ollama_model": "qwen2.5:7b-instruct-q4_K_M"
                }
            }
        }
