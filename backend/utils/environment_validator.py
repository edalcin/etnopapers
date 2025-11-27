"""
Environment validation utilities for Etnopapers standalone

Valida dependências externas (Ollama, MongoDB) antes de iniciar aplicação.
"""

import os
import urllib.request
import json
from typing import Dict, Any, Optional
from pathlib import Path


def check_ollama_connection(url: str = "http://localhost:11434", timeout: int = 3) -> Dict[str, Any]:
    """
    Verifica se Ollama está acessível

    Args:
        url: URL do Ollama
        timeout: Timeout em segundos

    Returns:
        Dict com status e informações
    """
    try:
        req = urllib.request.Request(f'{url}/api/tags')
        response = urllib.request.urlopen(req, timeout=timeout)
        data = json.loads(response.read())

        return {
            'status': 'ok',
            'accessible': True,
            'url': url,
            'models': data.get('models', []),
            'message': f'Ollama accessible at {url}'
        }

    except urllib.error.URLError as e:
        return {
            'status': 'error',
            'accessible': False,
            'url': url,
            'error': str(e),
            'message': f'Cannot connect to Ollama at {url}'
        }

    except Exception as e:
        return {
            'status': 'error',
            'accessible': False,
            'url': url,
            'error': str(e),
            'message': f'Unexpected error checking Ollama: {str(e)}'
        }


def check_ollama_model(model_name: str, url: str = "http://localhost:11434") -> Dict[str, Any]:
    """
    Verifica se modelo específico está disponível no Ollama

    Args:
        model_name: Nome do modelo (ex: qwen2.5:7b-instruct-q4_K_M)
        url: URL do Ollama

    Returns:
        Dict com status e informações
    """
    try:
        # Primeiro verifica conexão
        connection = check_ollama_connection(url)
        if not connection['accessible']:
            return connection

        # Verifica se modelo está na lista
        models = connection.get('models', [])
        model_names = [m.get('name', '') for m in models]

        if model_name in model_names:
            return {
                'status': 'ok',
                'available': True,
                'model': model_name,
                'message': f'Model {model_name} is available'
            }
        else:
            return {
                'status': 'warning',
                'available': False,
                'model': model_name,
                'available_models': model_names,
                'message': f'Model {model_name} not found. Available models: {", ".join(model_names)}'
            }

    except Exception as e:
        return {
            'status': 'error',
            'available': False,
            'model': model_name,
            'error': str(e),
            'message': f'Error checking model: {str(e)}'
        }


def check_mongodb_connection(uri: str, timeout: int = 5) -> Dict[str, Any]:
    """
    Verifica se MongoDB está acessível

    Args:
        uri: MongoDB URI
        timeout: Timeout em segundos

    Returns:
        Dict com status e informações
    """
    try:
        from pymongo import MongoClient
        from pymongo.errors import ConnectionFailure, ServerSelectionTimeoutError

        # Tentar conectar
        client = MongoClient(uri, serverSelectionTimeoutMS=timeout * 1000)

        # Ping para validar conexão
        client.admin.command('ping')

        # Obter informação do servidor
        server_info = client.server_info()

        return {
            'status': 'ok',
            'accessible': True,
            'uri': _mask_uri_password(uri),
            'version': server_info.get('version', 'unknown'),
            'message': f'MongoDB accessible (version {server_info.get("version", "unknown")})'
        }

    except ConnectionFailure as e:
        return {
            'status': 'error',
            'accessible': False,
            'uri': _mask_uri_password(uri),
            'error': 'Connection failed',
            'message': f'Cannot connect to MongoDB: {str(e)}'
        }

    except ServerSelectionTimeoutError as e:
        return {
            'status': 'error',
            'accessible': False,
            'uri': _mask_uri_password(uri),
            'error': 'Timeout',
            'message': f'MongoDB connection timeout: {str(e)}'
        }

    except Exception as e:
        return {
            'status': 'error',
            'accessible': False,
            'uri': _mask_uri_password(uri),
            'error': str(e),
            'message': f'Error connecting to MongoDB: {str(e)}'
        }


def check_frontend_build(dist_path: Optional[Path] = None) -> Dict[str, Any]:
    """
    Verifica se frontend foi buildado

    Args:
        dist_path: Path para diretório dist (default: frontend/dist)

    Returns:
        Dict com status e informações
    """
    if dist_path is None:
        dist_path = Path(__file__).parent.parent.parent / 'frontend' / 'dist'

    try:
        if not dist_path.exists():
            return {
                'status': 'error',
                'built': False,
                'path': str(dist_path),
                'message': f'Frontend not built: {dist_path} does not exist'
            }

        # Verificar arquivos essenciais
        index_html = dist_path / 'index.html'
        if not index_html.exists():
            return {
                'status': 'error',
                'built': False,
                'path': str(dist_path),
                'message': f'Frontend incomplete: index.html not found'
            }

        # Contar arquivos
        file_count = len(list(dist_path.rglob('*')))

        return {
            'status': 'ok',
            'built': True,
            'path': str(dist_path),
            'files': file_count,
            'message': f'Frontend built successfully ({file_count} files)'
        }

    except Exception as e:
        return {
            'status': 'error',
            'built': False,
            'path': str(dist_path) if dist_path else 'unknown',
            'error': str(e),
            'message': f'Error checking frontend: {str(e)}'
        }


def validate_environment(
    ollama_url: Optional[str] = None,
    ollama_model: Optional[str] = None,
    mongo_uri: Optional[str] = None
) -> Dict[str, Any]:
    """
    Valida ambiente completo

    Args:
        ollama_url: URL do Ollama (default: from env or http://localhost:11434)
        ollama_model: Modelo Ollama (default: from env or qwen2.5:7b-instruct-q4_K_M)
        mongo_uri: MongoDB URI (default: from env)

    Returns:
        Dict com resultados de todas as validações
    """
    # Obter valores de env vars se não fornecidos
    ollama_url = ollama_url or os.getenv('OLLAMA_URL', 'http://localhost:11434')
    ollama_model = ollama_model or os.getenv('OLLAMA_MODEL', 'qwen2.5:7b-instruct-q4_K_M')
    mongo_uri = mongo_uri or os.getenv('MONGO_URI', '')

    results = {
        'ollama': check_ollama_connection(ollama_url),
        'ollama_model': check_ollama_model(ollama_model, ollama_url) if ollama_model else {'status': 'skipped'},
        'mongodb': check_mongodb_connection(mongo_uri) if mongo_uri else {'status': 'skipped', 'message': 'MONGO_URI not configured'},
        'frontend': check_frontend_build(),
    }

    # Calcular status geral
    statuses = [r['status'] for r in results.values()]
    if all(s == 'ok' for s in statuses):
        overall = 'ok'
    elif any(s == 'error' for s in statuses):
        overall = 'error'
    else:
        overall = 'warning'

    results['overall'] = overall

    return results


def print_validation_results(results: Dict[str, Any]):
    """
    Imprime resultados de validação de forma legível

    Args:
        results: Resultado de validate_environment()
    """
    print("=" * 60)
    print("  VALIDAÇÃO DE AMBIENTE")
    print("=" * 60)
    print()

    checks = [
        ('Ollama', 'ollama'),
        ('Ollama Model', 'ollama_model'),
        ('MongoDB', 'mongodb'),
        ('Frontend', 'frontend'),
    ]

    for name, key in checks:
        result = results.get(key, {})
        status = result.get('status', 'unknown')
        message = result.get('message', 'No message')

        if status == 'ok':
            icon = '✓'
            color = ''
        elif status == 'warning':
            icon = '⚠'
            color = ''
        elif status == 'error':
            icon = '✗'
            color = ''
        else:
            icon = '?'
            color = ''

        print(f"{icon} {name:15} {status:10} - {message}")

    print()
    print("=" * 60)
    print(f"  Status Geral: {results.get('overall', 'unknown').upper()}")
    print("=" * 60)


def _mask_uri_password(uri: str) -> str:
    """
    Mascara senha em URI para logs

    Args:
        uri: URI original

    Returns:
        URI com senha mascarada
    """
    if '@' not in uri:
        return uri

    try:
        # Split por @ para separar credenciais
        parts = uri.split('@')
        if len(parts) >= 2:
            creds_and_protocol = parts[0]
            host_and_path = '@'.join(parts[1:])

            # Substituir senha por ***
            if ':' in creds_and_protocol:
                protocol_and_user = creds_and_protocol.rsplit(':', 1)[0]
                return f"{protocol_and_user}:***@{host_and_path}"

        return uri

    except Exception:
        return 'mongodb://***:***@***'


if __name__ == '__main__':
    # Teste standalone
    results = validate_environment()
    print_validation_results(results)
