"""
Etnopapers Standalone Launcher

Inicializa aplicação standalone, valida dependências, e abre navegador automaticamente.

Responsabilidades:
1. Validar se Ollama está rodando
2. Carregar configuração de .env ou solicitar via GUI
3. Iniciar servidor FastAPI
4. Abrir navegador automaticamente

Uso:
    python backend/launcher.py
    ou
    ./etnopapers (executável PyInstaller)
"""

import os
import sys
import time
import webbrowser
import urllib.request
from pathlib import Path


def validate_ollama() -> bool:
    """
    Valida se Ollama está rodando

    Returns:
        True se Ollama está acessível, False caso contrário
    """
    try:
        req = urllib.request.Request('http://localhost:11434/api/tags')
        urllib.request.urlopen(req, timeout=3)
        print("✓ Ollama detectado e rodando em http://localhost:11434")
        return True
    except Exception as e:
        print(f"✗ Ollama não detectado: {e}")
        return False


def load_or_prompt_config() -> bool:
    """
    Carrega configuração de .env ou solicita via GUI

    Returns:
        True se configuração carregada com sucesso
    """
    env_file = Path('.env')

    if env_file.exists():
        print(f"✓ Configuração encontrada em {env_file}")
        # Carregar variáveis de ambiente
        with open(env_file, 'r', encoding='utf-8') as f:
            for line in f:
                line = line.strip()
                if line and not line.startswith('#') and '=' in line:
                    key, value = line.split('=', 1)
                    os.environ[key.strip()] = value.strip()

        # Validar que MONGO_URI está presente
        if not os.getenv('MONGO_URI'):
            print("✗ MONGO_URI não encontrado no .env")
            print("  Abrindo janela de configuração...")
            from backend.gui.config_dialog import show_config_dialog
            show_config_dialog()
            return load_or_prompt_config()  # Recursivo após salvar

        print(f"✓ MONGO_URI configurado")
        return True

    # Mostrar GUI de configuração se .env não existe
    print("⚠ Primeira execução - configuração necessária")
    from backend.gui.config_dialog import show_config_dialog
    show_config_dialog()
    return load_or_prompt_config()  # Recursivo após salvar


def open_browser_delayed(url: str, delay: float = 2.0):
    """
    Abre navegador após delay

    Args:
        url: URL para abrir
        delay: Tempo de espera em segundos
    """
    time.sleep(delay)
    print(f"✓ Abrindo navegador em {url}")
    webbrowser.open(url)


def main():
    """Ponto de entrada principal do launcher"""
    print("=" * 60)
    print("  ETNOPAPERS - Sistema de Extração de Metadados Etnobotânicos")
    print("=" * 60)
    print()

    # 1. Validar Ollama
    print("[1/3] Validando Ollama...")
    if not validate_ollama():
        print()
        print("ERRO: Ollama não está rodando!")
        print()
        print("Por favor, inicie o Ollama antes de executar o Etnopapers:")
        print("  - Windows: Ollama.exe (inicia automaticamente após instalação)")
        print("  - macOS: Abrir Ollama.app (fica na barra de menu)")
        print("  - Linux: sudo systemctl start ollama")
        print()
        print("Download: https://ollama.ai/download")
        print()
        input("Pressione Enter para sair...")
        sys.exit(1)

    # 2. Carregar/solicitar configuração
    print()
    print("[2/3] Carregando configuração...")
    if not load_or_prompt_config():
        print("✗ Falha ao carregar configuração")
        sys.exit(1)

    # 3. Iniciar servidor FastAPI
    print()
    print("[3/3] Iniciando servidor...")
    print()

    # Setar variáveis de ambiente padrão se não existirem
    os.environ.setdefault('OLLAMA_URL', 'http://localhost:11434')
    os.environ.setdefault('OLLAMA_MODEL', 'qwen2.5:7b-instruct-q4_K_M')
    os.environ.setdefault('PORT', '8000')
    os.environ.setdefault('HOST', '0.0.0.0')
    os.environ.setdefault('ENVIRONMENT', 'production')
    os.environ.setdefault('LOG_LEVEL', 'info')

    # Importar depois de setar env vars
    import uvicorn
    from backend.main import app

    # Configurar URL do servidor
    port = int(os.getenv('PORT', 8000))
    server_url = f'http://localhost:{port}'

    print(f"✓ Servidor iniciando em {server_url}")
    print()
    print("=" * 60)
    print("  Aplicação pronta! O navegador abrirá automaticamente.")
    print("  Para encerrar: Feche esta janela ou pressione Ctrl+C")
    print("=" * 60)
    print()

    # Abrir navegador em thread separada
    import threading
    browser_thread = threading.Thread(
        target=open_browser_delayed,
        args=(server_url, 2.0),
        daemon=True
    )
    browser_thread.start()

    # Iniciar uvicorn
    try:
        uvicorn.run(
            app,
            host=os.getenv('HOST', '0.0.0.0'),
            port=port,
            log_level=os.getenv('LOG_LEVEL', 'info').lower()
        )
    except KeyboardInterrupt:
        print()
        print("✓ Servidor encerrado")
        sys.exit(0)
    except Exception as e:
        print()
        print(f"✗ Erro ao iniciar servidor: {e}")
        input("Pressione Enter para sair...")
        sys.exit(1)


if __name__ == '__main__':
    main()
