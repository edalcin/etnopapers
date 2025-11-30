"""Launcher for Etnopapers desktop application - validates Ollama and starts FastAPI"""
import os
import sys
import subprocess
import time
import requests
from pathlib import Path
from dotenv import load_dotenv

def check_ollama():
    """Check if Ollama service is running with detailed error handling"""
    ollama_url = os.getenv('OLLAMA_URL', 'http://localhost:11434')
    try:
        response = requests.get(f"{ollama_url}/api/tags", timeout=5)
        if response.status_code == 200:
            return True, None
        else:
            return False, f"Ollama returned status {response.status_code}"
    except requests.ConnectionError:
        return False, f"Cannot connect to Ollama at {ollama_url}"
    except requests.Timeout:
        return False, "Ollama connection timeout"
    except Exception as e:
        return False, str(e)

def get_ollama_status():
    """Get detailed Ollama status"""
    ollama_url = os.getenv('OLLAMA_URL', 'http://localhost:11434')
    try:
        response = requests.get(f"{ollama_url}/api/tags", timeout=5)
        if response.status_code == 200:
            data = response.json()
            models = data.get('models', [])
            return {
                'status': 'ok',
                'url': ollama_url,
                'models_count': len(models),
                'models': [m.get('name', 'unknown') for m in models[:3]]
            }
    except Exception as e:
        return {
            'status': 'unavailable',
            'url': ollama_url,
            'error': str(e)
        }

def load_configuration():
    """Load configuration from .env file"""
    config_dir = Path.home() / '.etnopapers'
    env_file = config_dir / '.env'

    if env_file.exists():
        load_dotenv(env_file)
    else:
        # Create default configuration
        config_dir.mkdir(parents=True, exist_ok=True)
        default_mongo_uri = 'mongodb://localhost:27017/etnopapers'
        env_file.write_text(f'MONGO_URI={default_mongo_uri}\n')
        os.environ['MONGO_URI'] = default_mongo_uri

def start_fastapi_server():
    """Start FastAPI server as subprocess"""
    backend_dir = Path(__file__).parent
    
    # Determine if running from built executable or development
    if hasattr(sys, 'frozen'):
        # Running from PyInstaller executable
        python_exe = sys.executable
    else:
        # Development mode
        python_exe = sys.executable
    
    cmd = [python_exe, "-m", "uvicorn", "main:app", "--host", "127.0.0.1", "--port", "8000"]
    
    try:
        proc = subprocess.Popen(
            cmd,
            cwd=str(backend_dir),
            stdout=subprocess.PIPE,
            stderr=subprocess.PIPE
        )
        print(f"FastAPI server started (PID: {proc.pid})")
        return proc
    except Exception as e:
        print(f"Failed to start FastAPI server: {e}")
        return None

def wait_for_fastapi():
    """Wait for FastAPI server to be ready"""
    max_retries = 30
    retry_count = 0
    
    while retry_count < max_retries:
        try:
            response = requests.get("http://127.0.0.1:8000/api/health", timeout=2)
            if response.status_code == 200:
                print("FastAPI server is ready")
                return True
        except Exception:
            retry_count += 1
            time.sleep(1)
    
    return False

def main():
    """Main launcher entry point"""
    print("Iniciando Etnopapers...")

    # Load configuration
    load_configuration()

    # Check Ollama with detailed feedback
    print("Verificando Ollama...")
    is_healthy, error_msg = check_ollama()

    if not is_healthy:
        print("ERRO: Serviço Ollama indisponível.")
        if error_msg:
            print(f"Detalhes: {error_msg}")
        print("Por favor, instale e inicie o Ollama a partir de https://ollama.com/download")
        sys.exit(1)

    # Get and display Ollama status
    status = get_ollama_status()
    print("Ollama encontrado")
    if status.get('status') == 'ok':
        print(f"  - URL: {status.get('url')}")
        print(f"  - Modelos disponiveis: {status.get('models_count')}")
        if status.get('models'):
            print(f"  - Exemplos: {', '.join(status.get('models'))}")
    
    # Start FastAPI server
    print("Iniciando servidor FastAPI...")
    fastapi_proc = start_fastapi_server()
    
    if not fastapi_proc:
        print("ERRO: Falha ao iniciar servidor FastAPI")
        sys.exit(1)
    
    # Wait for FastAPI to be ready
    print("Aguardando FastAPI...")
    if not wait_for_fastapi():
        print("ERRO: FastAPI não respondeu no tempo esperado")
        fastapi_proc.terminate()
        sys.exit(1)
    
    print("Etnopapers pronto!")
    print("Abra o navegador em: http://127.0.0.1:8000")
    
    # Keep process running
    try:
        fastapi_proc.wait()
    except KeyboardInterrupt:
        print("\nEnccerrando Etnopapers...")
        fastapi_proc.terminate()
        fastapi_proc.wait()

if __name__ == "__main__":
    main()
