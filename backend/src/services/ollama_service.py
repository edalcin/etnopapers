import os
import httpx
from typing import Optional, Dict, Any

class OllamaService:
    """Service for Ollama AI inference integration"""
    
    @staticmethod
    def get_ollama_url() -> str:
        """Get Ollama service URL from environment"""
        return os.getenv('OLLAMA_URL', 'http://localhost:11434')
    
    @staticmethod
    def get_ollama_model() -> str:
        """Get Ollama model name from environment"""
        return os.getenv('OLLAMA_MODEL', 'qwen2.5:7b-instruct-q4_K_M')
    
    @staticmethod
    async def check_health() -> bool:
        """Check if Ollama service is available"""
        try:
            url = OllamaService.get_ollama_url()
            async with httpx.AsyncClient(timeout=5.0) as client:
                response = await client.get(f"{url}/api/tags")
                return response.status_code == 200
        except Exception as e:
            print(f"Erro ao verificar saúde do Ollama: {e}")
            return False
    
    @staticmethod
    async def generate_completion(
        prompt: str,
        system_prompt: Optional[str] = None
    ) -> str:
        """Generate completion from Ollama"""
        url = OllamaService.get_ollama_url()
        model = OllamaService.get_ollama_model()
        
        messages = []
        if system_prompt:
            messages.append({
                "role": "system",
                "content": system_prompt
            })
        
        messages.append({
            "role": "user",
            "content": prompt
        })
        
        try:
            async with httpx.AsyncClient(timeout=300.0) as client:
                response = await client.post(
                    f"{url}/api/generate",
                    json={
                        "model": model,
                        "prompt": prompt,
                        "stream": False,
                        "temperature": 0.7
                    }
                )
                
                if response.status_code != 200:
                    raise Exception(f"Ollama error: {response.text}")
                
                result = response.json()
                return result.get('response', '')
        except Exception as e:
            raise Exception(f"Erro ao gerar resposta do Ollama: {str(e)}")
    
    @staticmethod
    async def get_available_models() -> list:
        """Get list of available models in Ollama"""
        try:
            url = OllamaService.get_ollama_url()
            async with httpx.AsyncClient(timeout=5.0) as client:
                response = await client.get(f"{url}/api/tags")
                if response.status_code == 200:
                    data = response.json()
                    return data.get('models', [])
        except Exception as e:
            print(f"Erro ao listar modelos: {e}")
        
        return []
