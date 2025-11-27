# Etnopapers - Build from Source

Guia completo para desenvolvedores que desejam compilar o Etnopapers do código-fonte.

## Sumário

1. [Pré-requisitos](#pré-requisitos)
2. [Clonando o Repositório](#clonando-o-repositório)
3. [Setup do Ambiente](#setup-do-ambiente)
4. [Build do Frontend](#build-do-frontend)
5. [Build do Executável](#build-do-executável)
6. [Testes](#testes)
7. [Distribuição](#distribuição)

---

## Pré-requisitos

### Software Necessário

- **Python 3.11+** ([Download](https://www.python.org/downloads/))
- **Node.js 18+** ([Download](https://nodejs.org/))
- **Git** ([Download](https://git-scm.com/downloads))
- **Ollama** ([Download](https://ollama.ai/download))

### Ferramentas de Build

**Windows:**
- Visual Studio Build Tools ou Windows SDK
- PowerShell 5.0+

**macOS:**
- Xcode Command Line Tools: `xcode-select --install`

**Linux (Ubuntu/Debian):**
```bash
sudo apt-get update
sudo apt-get install -y build-essential python3-dev
```

---

## Clonando o Repositório

```bash
# Clone via HTTPS
git clone https://github.com/[seu-usuario]/etnopapers.git
cd etnopapers

# Ou via SSH
git clone git@github.com:[seu-usuario]/etnopapers.git
cd etnopapers
```

---

## Setup do Ambiente

### 1. Backend (Python)

**Criar virtual environment:**

```bash
# Windows
python -m venv venv
venv\Scripts\activate

# macOS/Linux
python3 -m venv venv
source venv/bin/activate
```

**Instalar dependências:**

```bash
pip install -r backend/requirements.txt
```

**Verificar instalação:**

```bash
python -c "import fastapi, uvicorn, pymongo; print('Backend dependencies OK')"
```

### 2. Frontend (Node.js)

**Instalar dependências:**

```bash
cd frontend
npm install
cd ..
```

**Verificar instalação:**

```bash
cd frontend
npm run type-check
cd ..
```

### 3. Ollama

**Instalar e baixar modelo:**

```bash
# Instalar Ollama (ver docs/instalacao-standalone.md)

# Baixar modelo
ollama pull qwen2.5:7b-instruct-q4_K_M

# Verificar
ollama list
```

---

## Build do Frontend

O frontend React precisa ser buildado antes do executável PyInstaller.

```bash
cd frontend

# Build de produção
npm run build

# Verificar output
ls -la dist/
# Deve conter: index.html, assets/, etc.

cd ..
```

**Estrutura esperada:**

```
frontend/dist/
├── index.html
├── assets/
│   ├── index-[hash].js
│   ├── index-[hash].css
│   └── ...
└── favicon.png
```

---

## Build do Executável

### Windows

**Usando script automatizado:**

```batch
build-windows.bat
```

**Ou manual:**

```batch
REM Ativar virtual environment
venv\Scripts\activate

REM Buildar frontend (se não fez ainda)
cd frontend
npm run build
cd ..

REM Buildar executável
pyinstaller build.spec --clean

REM Output em: dist\etnopapers.exe
```

**Verificar build:**

```batch
dir dist\etnopapers.exe
# Tamanho esperado: ~150-200 MB
```

---

### macOS

**Usando script automatizado:**

```bash
chmod +x build-macos.sh
./build-macos.sh
```

**Ou manual:**

```bash
# Ativar virtual environment
source venv/bin/activate

# Buildar frontend (se não fez ainda)
cd frontend
npm run build
cd ..

# Buildar app bundle
pyinstaller build.spec --clean

# Output em: dist/Etnopapers.app
```

**Verificar build:**

```bash
ls -lh dist/Etnopapers.app
# Tamanho esperado: ~150-200 MB
```

---

### Linux

**Usando script automatizado:**

```bash
chmod +x build-linux.sh
./build-linux.sh
```

**Ou manual:**

```bash
# Ativar virtual environment
source venv/bin/activate

# Buildar frontend (se não fez ainda)
cd frontend
npm run build
cd ..

# Buildar executável
pyinstaller build.spec --clean

# Tornar executável
chmod +x dist/etnopapers

# Output em: dist/etnopapers
```

**Verificar build:**

```bash
ls -lh dist/etnopapers
file dist/etnopapers
# Deve mostrar: ELF 64-bit executable
```

---

## Testes

### Testar Backend Standalone

```bash
# Ativar venv
source venv/bin/activate  # Linux/macOS
venv\Scripts\activate     # Windows

# Criar .env temporário para testes
cp .env.example .env
# Editar .env com MONGO_URI válido

# Executar launcher
python backend/launcher.py

# Deve:
# 1. Validar Ollama
# 2. Carregar .env
# 3. Iniciar servidor em http://localhost:8000
# 4. Abrir navegador automaticamente
```

### Testar Executável

**Windows:**

```batch
# Copiar .env para pasta dist
copy .env dist\.env

# Executar
dist\etnopapers.exe
```

**macOS:**

```bash
# Copiar .env para pasta dist
cp .env dist/.env

# Executar
open dist/Etnopapers.app
```

**Linux:**

```bash
# Copiar .env para pasta dist
cp .env dist/.env

# Executar
./dist/etnopapers
```

### Testes Automatizados

**Backend (pytest):**

```bash
# Ativar venv
source venv/bin/activate

# Executar testes
pytest backend/tests/ -v

# Com coverage
pytest backend/tests/ --cov=backend --cov-report=html
```

**Frontend (vitest):**

```bash
cd frontend

# Executar testes
npm run test

# Com UI
npm run test:ui

cd ..
```

---

## Distribuição

### Criar Release

**1. Preparar versão:**

```bash
# Tag no git
git tag -a v1.0.0 -m "Release v1.0.0"
git push origin v1.0.0
```

**2. Buildar em cada plataforma:**

- **Windows**: Build em máquina Windows → `dist/etnopapers.exe`
- **macOS**: Build em máquina macOS → `dist/Etnopapers.app`
- **Linux**: Build em máquina Linux → `dist/etnopapers`

**3. Empacotar:**

**Windows (ZIP):**

```powershell
# Criar estrutura
mkdir Etnopapers-Windows-v1.0.0
copy dist\etnopapers.exe Etnopapers-Windows-v1.0.0\
copy .env.example Etnopapers-Windows-v1.0.0\
copy README.md Etnopapers-Windows-v1.0.0\

# Comprimir
Compress-Archive -Path Etnopapers-Windows-v1.0.0 -DestinationPath Etnopapers-Windows-v1.0.0.zip
```

**macOS (ZIP):**

```bash
# Criar estrutura
mkdir Etnopapers-macOS-v1.0.0
cp -r dist/Etnopapers.app Etnopapers-macOS-v1.0.0/
cp .env.example Etnopapers-macOS-v1.0.0/
cp README.md Etnopapers-macOS-v1.0.0/

# Comprimir
zip -r Etnopapers-macOS-v1.0.0.zip Etnopapers-macOS-v1.0.0
```

**Linux (TAR.GZ):**

```bash
# Criar estrutura
mkdir Etnopapers-Linux-v1.0.0
cp dist/etnopapers Etnopapers-Linux-v1.0.0/
cp .env.example Etnopapers-Linux-v1.0.0/
cp README.md Etnopapers-Linux-v1.0.0/

# Comprimir
tar -czf Etnopapers-Linux-v1.0.0.tar.gz Etnopapers-Linux-v1.0.0
```

**4. GitHub Release:**

```bash
# Usando GitHub CLI (gh)
gh release create v1.0.0 \
  Etnopapers-Windows-v1.0.0.zip \
  Etnopapers-macOS-v1.0.0.zip \
  Etnopapers-Linux-v1.0.0.tar.gz \
  --title "Etnopapers v1.0.0" \
  --notes "Primeira release standalone"

# Ou manualmente em:
# https://github.com/[seu-usuario]/etnopapers/releases/new
```

---

## Otimizações

### Reduzir Tamanho do Executável

**1. UPX Compression (já habilitado em build.spec):**

```python
# Em build.spec, linha:
upx=True
```

**2. Excluir bibliotecas desnecessárias:**

```python
# Em build.spec, adicionar em excludes:
excludes=[
    'pytest',
    'matplotlib',
    'numpy',
    'scipy',
    # ... outras bibliotecas não usadas
]
```

**3. Strip debug symbols (Linux/macOS):**

```python
# Em build.spec:
strip=True
```

### Performance de Build

**Usar cache do PyInstaller:**

```bash
# Build sem --clean para reuso de cache
pyinstaller build.spec
```

**Build paralelo do frontend:**

```bash
# Em package.json, usar:
"build": "vite build --minify esbuild"
```

---

## Troubleshooting

### PyInstaller não encontra módulos

**Erro:** `ModuleNotFoundError` ao executar

**Solução:** Adicionar ao `hiddenimports` em `build.spec`:

```python
hiddenimports=[
    'seu_modulo_faltando',
    # ...
]
```

### Frontend não incluído no executável

**Erro:** 404 ao acessar http://localhost:8000

**Solução:** Verificar `datas` em `build.spec`:

```python
datas=[
    ('frontend/dist', 'frontend/dist'),  # Caminho correto
]
```

E ajustar paths em `backend/main.py` para usar `sys._MEIPASS`.

### Executável muito grande

**Tamanho típico:** 150-200 MB

**Se maior que 300 MB:**

1. Verificar `excludes` em `build.spec`
2. Habilitar UPX compression
3. Remover dependências não usadas

### Build falha no macOS (code signing)

**Erro:** Code signature error

**Solução temporária:**

```bash
# Desabilitar verificação de assinatura (apenas para testes)
sudo spctl --master-disable
```

**Solução permanente:** Assinar com Apple Developer ID

---

## Desenvolvimento Local

Para desenvolvimento sem buildar executável:

```bash
# 1. Ativar venv
source venv/bin/activate

# 2. Copiar .env.example para .env
cp .env.example .env
# Editar .env com configs

# 3. Terminal 1: Backend
python backend/launcher.py

# 4. Terminal 2: Frontend (dev mode)
cd frontend
npm run dev
# Acessa http://localhost:3000
```

---

## CI/CD

Exemplo de GitHub Actions para build automatizado:

```yaml
# .github/workflows/build.yml
name: Build Etnopapers

on:
  push:
    tags:
      - 'v*'

jobs:
  build-windows:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-python@v4
        with:
          python-version: '3.11'
      - uses: actions/setup-node@v3
        with:
          node-version: '18'
      - run: build-windows.bat
      - uses: actions/upload-artifact@v3
        with:
          name: etnopapers-windows
          path: dist/etnopapers.exe

  build-macos:
    runs-on: macos-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-python@v4
        with:
          python-version: '3.11'
      - uses: actions/setup-node@v3
        with:
          node-version: '18'
      - run: chmod +x build-macos.sh && ./build-macos.sh
      - uses: actions/upload-artifact@v3
        with:
          name: etnopapers-macos
          path: dist/Etnopapers.app

  build-linux:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-python@v4
        with:
          python-version: '3.11'
      - uses: actions/setup-node@v3
        with:
          node-version: '18'
      - run: chmod +x build-linux.sh && ./build-linux.sh
      - uses: actions/upload-artifact@v3
        with:
          name: etnopapers-linux
          path: dist/etnopapers
```

---

## Contribuindo

Para contribuir com o desenvolvimento:

1. Fork do repositório
2. Criar branch para feature: `git checkout -b feature/nome-da-feature`
3. Fazer alterações e testar
4. Commit: `git commit -m "Adiciona feature X"`
5. Push: `git push origin feature/nome-da-feature`
6. Abrir Pull Request

---

## Recursos Adicionais

- [PyInstaller Documentation](https://pyinstaller.org/en/stable/)
- [Vite Build Guide](https://vitejs.dev/guide/build.html)
- [FastAPI Deployment](https://fastapi.tiangolo.com/deployment/)
- [Ollama Documentation](https://github.com/ollama/ollama/blob/main/docs/api.md)
