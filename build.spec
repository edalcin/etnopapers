# -*- mode: python ; coding: utf-8 -*-
"""
PyInstaller build spec for Etnopapers standalone application

Cria executável único contendo:
- Backend FastAPI
- Frontend React (static files)
- Todas as dependências Python

Uso:
    pyinstaller build.spec

Output:
    dist/etnopapers.exe      (Windows)
    dist/Etnopapers.app      (macOS)
    dist/etnopapers          (Linux)
"""

import sys
from pathlib import Path

# Determinar o path base do projeto
project_root = Path.cwd()

# Análise de dependências
a = Analysis(
    # Entry point - launcher script
    ['backend/launcher.py'],

    # Paths adicionais para busca de módulos
    pathex=[str(project_root)],

    # Binaries externos (nenhum necessário)
    binaries=[],

    # Data files a incluir no executável
    datas=[
        # Frontend static files
        ('frontend/dist', 'frontend/dist'),

        # Backend prompts (templates de extração)
        ('backend/prompts', 'backend/prompts'),
    ],

    # Hidden imports - módulos carregados dinamicamente
    # FastAPI/Uvicorn precisam de imports explícitos
    hiddenimports=[
        # Uvicorn
        'uvicorn.logging',
        'uvicorn.loops',
        'uvicorn.loops.auto',
        'uvicorn.loops.asyncio',
        'uvicorn.protocols',
        'uvicorn.protocols.http',
        'uvicorn.protocols.http.auto',
        'uvicorn.protocols.http.h11_impl',
        'uvicorn.protocols.websockets',
        'uvicorn.protocols.websockets.auto',
        'uvicorn.protocols.websockets.wsproto_impl',
        'uvicorn.lifespan',
        'uvicorn.lifespan.on',

        # FastAPI
        'fastapi.responses',
        'fastapi.staticfiles',

        # Pydantic
        'pydantic',
        'pydantic.fields',
        'pydantic.types',

        # Instructor
        'instructor',

        # OpenAI (usado por instructor)
        'openai',
        'openai.types',

        # Database
        'pymongo',

        # PDF processing
        'pdfplumber',

        # Outros
        'dotenv',
    ],

    # Hook paths customizados
    hookspath=[],

    # Hook config
    hooksconfig={},

    # Runtime hooks
    runtime_hooks=[],

    # Excludes - módulos a NÃO incluir (reduz tamanho)
    excludes=[
        'pytest',
        'pytest-cov',
        'pytest-asyncio',
        'matplotlib',
        'numpy',
        'scipy',
        'pandas',
        'PIL',
    ],

    # Win private assemblies
    win_no_prefer_redirects=False,
    win_private_assemblies=False,

    # Não criar arquivo (usar onefile)
    noarchive=False,

    # Otimizações
    optimize=0,
)

# Python zip file
pyz = PYZ(
    a.pure,
    a.zipped_data,
)

# Executável
exe = EXE(
    pyz,
    a.scripts,
    a.binaries,
    a.zipfiles,
    a.datas,
    [],

    # Nome do executável
    name='etnopapers',

    # Debug mode
    debug=False,

    # Bootloader options
    bootloader_ignore_signals=False,

    # Strip binaries (Linux/macOS - reduz tamanho)
    strip=False,

    # UPX compression (reduz tamanho)
    upx=True,
    upx_exclude=[],

    # Runtime tmpdir
    runtime_tmpdir=None,

    # Console mode (True = mostra terminal, False = windowed)
    console=True,

    # Disable windowed traceback
    disable_windowed_traceback=False,

    # argv emulation (macOS)
    argv_emulation=False,

    # Target architecture (None = current)
    target_arch=None,

    # Code signing identity (macOS)
    codesign_identity=None,

    # Entitlements file (macOS)
    entitlements_file=None,

    # Icon file
    icon=None,  # Pode adicionar: 'frontend/public/favicon.ico'
)

# macOS app bundle (apenas em macOS)
if sys.platform == 'darwin':
    app = BUNDLE(
        exe,
        name='Etnopapers.app',
        icon=None,  # Pode adicionar: 'frontend/public/favicon.icns'
        bundle_identifier='com.etnopapers.app',
        version='1.0.0',
        info_plist={
            'NSPrincipalClass': 'NSApplication',
            'NSHighResolutionCapable': 'True',
            'CFBundleName': 'Etnopapers',
            'CFBundleDisplayName': 'Etnopapers',
            'CFBundleShortVersionString': '1.0.0',
            'CFBundleVersion': '1.0.0',
            'CFBundlePackageType': 'APPL',
            'LSMinimumSystemVersion': '10.13.0',
        },
    )
