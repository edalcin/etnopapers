# Lista de Tarefas - Otimização do Build macOS (v2.1)

**Objetivo**: Converter para DMG Installer profissional com code signing, notarization e suporte multi-arquitetura
**Prioridade**: 🟡 ALTA
**Tempo Total Estimado**: 20-27 horas
**Data Início**: 2025-11-30

---

## ✅ FASE 1: PREPARAÇÃO (2-3 horas)

### T1.1 - Analisar App Bundle Atual
**Descrição**: Entender tamanho e performance do build atual

**Tarefas**:
- [ ] Medir tamanho atual: `du -sh dist/Etnopapers.app`
- [ ] Testar startup em Intel Mac (medir tempo com `time`)
- [ ] Testar startup em Apple Silicon Mac (m1/m2/m3)
- [ ] Medir consumo RAM: `top -l 1 | grep Etnopapers`
- [ ] Medir consumo CPU: `top -l 1 | grep Etnopapers`
- [ ] Listar módulos Python incluídos: `otool -L dist/Etnopapers.app/Contents/MacOS/Etnopapers`

**Saída Esperada**:
```
Análise do build atual (macOS):
- Tamanho: 150-200 MB
- Startup Intel: 5-10 segundos
- Startup ARM: 3-5 segundos
- RAM inicial: 50-80%
- CPU inicial: 20-50%
- Code signed: NÃO
- Notarized: NÃO
```

**Responsável**: Claude Code
**Duração**: 45 min

---

### T1.2 - Configurar Ferramentas macOS
**Descrição**: Preparar ambiente para code signing e notarization

**Tarefas**:
- [ ] Instalar Xcode Command Line Tools (se necessário)
  ```bash
  xcode-select --install
  ```

- [ ] Verificar certificados disponíveis
  ```bash
  security find-identity -v -p codesigning
  ```

- [ ] Se não tiver Developer Certificate:
  - [ ] Registrar no Apple Developer Program ($99/ano)
  - [ ] Baixar certificado "Developer ID Application"
  - [ ] Importar para Keychain

- [ ] Configurar arquivo de entitlements
  ```bash
  mkdir -p installer/macos
  touch installer/macos/entitlements.plist
  ```

- [ ] Testar code signing em um app de teste

**Saída Esperada**:
```
Certificados disponíveis:
- "Developer ID Application: John Doe (ABC123XYZ)"
```

**Responsável**: Claude Code
**Duração**: 1 hora

---

### T1.3 - Preparar Estrutura de Build
**Descrição**: Criar diretórios e scripts para novo fluxo de build

**Tarefas**:
- [ ] Criar diretórios:
  ```bash
  mkdir -p installer/macos
  mkdir -p build/intel
  mkdir -p build/arm
  mkdir -p build/universal
  ```

- [ ] Criar scripts de build:
  - [ ] `build-macos-intel.sh` (x86_64)
  - [ ] `build-macos-arm.sh` (arm64)
  - [ ] `build-macos-universal.sh` (universal binary)

- [ ] Documentar estrutura em `installer/macos/README.md`

**Estrutura Esperada**:
```
installer/macos/
├── entitlements.plist
├── create-dmg-config.txt
├── post-install.sh
├── check-python.sh
└── README.md
```

**Responsável**: Claude Code
**Duração**: 45 min

---

## ✅ FASE 2: IMPLEMENTAÇÃO (6-8 horas)

### T2.1 - Criar build.spec.macos Otimizado
**Descrição**: Reescrever build.spec removendo dependências desnecessárias

**Mudanças Principais**:

```python
# Usar "onedir" em vez de "onefile"
a = Analysis(
    ...
    # Excluir muito mais para macOS
    excludes=[
        # Testes
        'pytest', 'pytest-cov', 'pytest-asyncio',
        # Ciência de dados
        'numpy', 'scipy', 'pandas', 'matplotlib',
        # Imagem
        'PIL', 'cv2', 'pillow',
        # Jupyter
        'jupyter', 'ipython', 'notebook',
        # Stdlib não usada
        'email', 'http.server', 'ftplib',
        # Web
        'urllib3', 'certifi',
    ],
)

# Compilar para bytecode otimizado
a.optimize = 2  # -O -O para melhor compressão
```

**Tarefas**:
- [ ] Copiar `build.spec` → `build.spec.macos`
- [ ] Remover todas as dependências de teste/ML
- [ ] Adicionar otimizações macOS-específicas
- [ ] Testar build: `pyinstaller build.spec.macos --clean`
- [ ] Medir tamanho: `du -sh dist/Etnopapers.app`
- [ ] Meta: 60-80 MB (redução de ~50%)

**Resultado Esperado**:
```
Antes: 150-200 MB
Depois: 60-80 MB
Redução: 50-60%
```

**Responsável**: Claude Code
**Duração**: 1.5 horas

---

### T2.2 - Implementar Code Signing
**Descrição**: Assinar app bundle com Developer Certificate

**Script**: `installer/macos/sign-app.sh`

```bash
#!/bin/bash

# Variáveis
CERT_NAME="Developer ID Application: Your Name (ABC123XYZ)"
APP_PATH="dist/Etnopapers.app"
ENTITLEMENTS="installer/macos/entitlements.plist"

# 1. Code sign recursively
codesign --deep --force --verify --verbose \
    --options=runtime \
    --timestamp \
    --sign "$CERT_NAME" \
    --entitlements "$ENTITLEMENTS" \
    "$APP_PATH"

# 2. Verify
codesign --verify --verbose "$APP_PATH"

# 3. Check for weak or invalid signatures
codesign --verify --deep --strict "$APP_PATH"

# 4. Display signature info
codesign --display --verbose "$APP_PATH"
```

**Tarefas**:
- [ ] Criar `installer/macos/entitlements.plist`
- [ ] Criar `installer/macos/sign-app.sh`
- [ ] Testar code signing em app teste
- [ ] Verificar que app pode abrir sem warnings

**Entitlements.plist** (exemplo):
```xml
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>com.apple.security.cs.allow-jit</key>
    <true/>
    <key>com.apple.security.cs.allow-dyld-environment-variables</key>
    <true/>
    <key>com.apple.security.network.client</key>
    <true/>
    <key>com.apple.security.network.server</key>
    <true/>
</dict>
</plist>
```

**Responsável**: Claude Code
**Duração**: 1.5 horas

---

### T2.3 - Criar DMG Installer
**Descrição**: Empacotar app em DMG (formato padrão macOS)

**Opções**:

**Opção A: Usando create-dmg (gratuito)**
```bash
# Instalar create-dmg
npm install -g create-dmg

# Criar DMG
create-dmg \
    --volname "Etnopapers" \
    --volicon "frontend/public/favicon.icns" \
    --window-pos 100 100 \
    --window-size 500 400 \
    --icon-size 100 \
    --icon "Etnopapers.app" 100 100 \
    --hide-extension "Etnopapers.app" \
    --app-drop-link 400 100 \
    dist/Etnopapers-v2.1.dmg \
    dist/Etnopapers.app
```

**Opção B: Usando AppleScript (nativo)**
```applescript
-- create-dmg.applescript
on run argv
    set appPath to item 1 of argv
    set dmgPath to item 2 of argv

    -- Criar imagem de disco
    do shell script "hdiutil create -srcfolder " & appPath & " -volname Etnopapers -fs HFS+ -fsargs '-c c=64,a=16,e=16' -format UDRW -o " & dmgPath
end run
```

**Tarefas**:
- [ ] Criar ícone em formato .icns (256x256)
- [ ] Escolher método (create-dmg ou AppleScript)
- [ ] Criar DMG
- [ ] Testar instalação de DMG (abrir, arrastar app)
- [ ] Verificar tamanho: esperado 30-50 MB

**Saída Esperada**:
```
dist/Etnopapers-v2.1.dmg (30-50 MB)
├─ Etnopapers.app
├─ Applications (link simbólico)
└─ background.png (background customizado)
```

**Responsável**: Claude Code
**Duração**: 2 horas

---

### T2.4 - Implementar Notarization
**Descrição**: Submeter DMG para Apple validar e assinar

**Script**: `installer/macos/notarize.sh`

```bash
#!/bin/bash

DMG_PATH="dist/Etnopapers-v2.1.dmg"
BUNDLE_ID="com.etnopapers.app"
APPLE_ID="$1"  # email@icloud.com
APP_PASSWORD="$2"  # App-specific password

echo "Notarizing $DMG_PATH..."

# 1. Submit for notarization
REQUEST_ID=$(xcrun altool --notarize-app \
    --file "$DMG_PATH" \
    --primary-bundle-id "$BUNDLE_ID" \
    -u "$APPLE_ID" \
    -p "$APP_PASSWORD" \
    2>&1 | grep RequestUUID | sed 's/.*RequestUUID = //')

echo "Notarization submitted: $REQUEST_ID"
echo "Waiting for response (5-30 minutes)..."

# 2. Poll for completion
while true; do
    STATUS=$(xcrun altool --notarization-info "$REQUEST_ID" \
        -u "$APPLE_ID" \
        -p "$APP_PASSWORD" 2>&1 | grep Status | sed 's/.*Status: //')

    if [ "$STATUS" = "success" ]; then
        echo "✓ Notarization successful!"

        # 3. Staple (associar notarization)
        xcrun stapler staple "$DMG_PATH"
        echo "✓ Stapled notarization to DMG"
        break
    elif [ "$STATUS" = "invalid" ]; then
        echo "✗ Notarization failed"
        exit 1
    else
        echo "Status: $STATUS... waiting"
        sleep 30
    fi
done
```

**Tarefas**:
- [ ] Criar Apple Developer Account
- [ ] Gerar App-specific password (2FA)
- [ ] Criar script `installer/macos/notarize.sh`
- [ ] Testar notarization (pode levar 10-30 min)
- [ ] Verificar que DMG notarizado (terceira verificação passa)

**Alternativa**: Se sem Apple Developer Account, pular este passo (app vai pedir permissão na primeira execução)

**Responsável**: Claude Code
**Duração**: 1.5 horas (sem testes reais)

---

### T2.5 - Criar Script de Instalação
**Descrição**: Script inteligente para instalar dependências

**Script**: `installer/macos/install.sh`

```bash
#!/bin/bash

echo "Etnopapers macOS Installer"

# 1. Detect architecture
ARCH=$(arch)
echo "Architecture: $ARCH"

# 2. Check Python 3.11+
echo "Checking Python..."
if command -v python3 &> /dev/null; then
    VERSION=$(python3 --version 2>&1 | awk '{print $2}')
    echo "✓ Python $VERSION found"
else
    echo "✗ Python 3.11+ not found"
    echo "Download from: https://www.python.org/downloads/"
    exit 1
fi

# 3. Create venv (opcional)
if [ ! -d "$HOME/.etnopapers/venv" ]; then
    echo "Creating virtual environment..."
    mkdir -p "$HOME/.etnopapers"
    python3 -m venv "$HOME/.etnopapers/venv"
fi

# 4. Install dependencies
echo "Installing Python dependencies..."
source "$HOME/.etnopapers/venv/bin/activate"
pip install -r requirements-macos.txt --quiet

# 5. Create alias/shortcut
echo "Creating shortcut..."
ln -sf "/Applications/Etnopapers.app" "$HOME/Applications Shortcuts/Etnopapers"

# 6. Register in Spotlight
echo "Registering in Spotlight..."
mdimport "/Applications/Etnopapers.app"

# 7. Check Ollama
echo ""
echo "Checking Ollama..."
if curl -s http://localhost:11434/api/tags > /dev/null 2>&1; then
    echo "✓ Ollama found and running"
else
    echo "⚠ Ollama not detected"
    echo "Download from: https://ollama.com/download"
fi

echo ""
echo "✓ Installation complete!"
echo "Open: /Applications/Etnopapers.app"
```

**Tarefas**:
- [ ] Criar `installer/macos/install.sh`
- [ ] Tornar executável: `chmod +x installer/macos/install.sh`
- [ ] Testar em Intel Mac
- [ ] Testar em Apple Silicon Mac
- [ ] Verificar detecção Python

**Responsável**: Claude Code
**Duração**: 1 hora

---

### T2.6 - Otimizar requirements-macos.txt
**Descrição**: Apenas dependências de produção

**Arquivo**: `requirements-macos.txt`

```
# FastAPI + servidor
fastapi==0.110.0
uvicorn[standard]==0.29.0
aiofiles==23.2.1

# Modelos
pydantic==2.8.0
pydantic-settings==2.4.0

# Banco de dados
pymongo==4.5.0

# PDF
pdfplumber==0.10.3

# IA
instructor==1.3.7

# HTTP
requests==2.31.0
httpx==0.27.0

# Ambiente
python-dotenv==1.0.0

# Não incluir: pytest, pyinstaller, numpy, pandas, etc.
```

**Tarefas**:
- [ ] Criar `requirements-macos.txt`
- [ ] Listar apenas dependências de produção
- [ ] Testar: `pip install -r requirements-macos.txt`
- [ ] Medir tamanho após instalação

**Responsável**: Claude Code
**Duração**: 30 min

---

## ✅ FASE 3: MULTI-ARQUITETURA (4-5 horas)

### T3.1 - Build Intel (x86_64)
**Descrição**: Compilar para processadores Intel

**Script**: `build-macos-intel.sh`

```bash
#!/bin/bash

echo "Building Etnopapers for Intel (x86_64)..."

# Force x86_64
export ARCHFLAGS="-arch x86_64"
export arch=x86_64

# Setup Python x86_64 (se em Apple Silicon)
if [ "$(arch)" = "arm64" ]; then
    echo "Running on Apple Silicon, using Rosetta2 for x86_64 build"
    arch -x86_64 /bin/bash << 'EOFBUILD'

    python3 -m venv build/intel/venv
    source build/intel/venv/bin/activate
    pip install -r requirements.txt
    pyinstaller build.spec.macos --clean -d onefile

EOFBUILD
else
    echo "Running on Intel"
    python3 -m venv build/intel/venv
    source build/intel/venv/bin/activate
    pip install -r requirements.txt
    pyinstaller build.spec.macos --clean
fi

echo "✓ Intel build complete"
```

**Tarefas**:
- [ ] Criar `build-macos-intel.sh`
- [ ] Testar em Intel Mac (ou via Rosetta em Apple Silicon)
- [ ] Medir tamanho: esperado 60-80 MB
- [ ] Testar startup: esperado 2-4 segundos
- [ ] Assinar app (T2.2)
- [ ] Criar DMG (T2.3)

**Resultado Esperado**:
```
dist/Etnopapers-Intel-v2.1.dmg (30-40 MB)
```

**Responsável**: Claude Code
**Duração**: 1.5 horas

---

### T3.2 - Build Apple Silicon (arm64)
**Descrição**: Compilar nativo para M1/M2/M3

**Script**: `build-macos-arm.sh`

```bash
#!/bin/bash

echo "Building Etnopapers for Apple Silicon (arm64)..."

# Force arm64
export ARCHFLAGS="-arch arm64"
export arch=arm64

# Setup Python arm64
python3 -m venv build/arm/venv
source build/arm/venv/bin/activate

# Instalar dependências
pip install -r requirements.txt

# Build otimizado para arm64
pyinstaller build.spec.macos --clean

echo "✓ Apple Silicon build complete"
```

**Tarefas**:
- [ ] Criar `build-macos-arm.sh`
- [ ] Executar em Apple Silicon Mac (M1/M2/M3)
- [ ] Medir tamanho: esperado 50-70 MB (menor que Intel)
- [ ] Testar startup: esperado 1-3 segundos (mais rápido)
- [ ] Assinar app
- [ ] Criar DMG

**Resultado Esperado**:
```
dist/Etnopapers-ARM-v2.1.dmg (25-35 MB)
Startup nativo arm64: 30-50% mais rápido que Intel via Rosetta
```

**Responsável**: Claude Code
**Duração**: 1.5 horas

---

### T3.3 - Universal Binary (Opcional)
**Descrição**: Combinar Intel + arm64 em um executável

**Script**: `build-macos-universal.sh`

```bash
#!/bin/bash

echo "Creating universal binary..."

# Combinar com lipo
lipo -create \
    build/intel/dist/Etnopapers.app/Contents/MacOS/Etnopapers \
    build/arm/dist/Etnopapers.app/Contents/MacOS/Etnopapers \
    -output dist/Etnopapers-Universal/Contents/MacOS/Etnopapers

# Copiar restante dos arquivos
cp -r build/arm/dist/Etnopapers.app/Contents/* dist/Etnopapers-Universal/Contents/

# Code sign universal
bash installer/macos/sign-app.sh dist/Etnopapers-Universal

# Create DMG
create-dmg dist/Etnopapers-Universal-v2.1.dmg dist/Etnopapers-Universal

echo "✓ Universal binary created"
```

**Tarefas**:
- [ ] Criar `build-macos-universal.sh`
- [ ] Combinar Intel + arm64
- [ ] Testar em Intel Mac
- [ ] Testar em Apple Silicon Mac
- [ ] Medir tamanho: esperado 80-100 MB (ambas arquiteturas)

**Vantagem**:
- Funciona em qualquer Mac (Intel ou Apple Silicon)
- Nenhuma conversão/emulação necessária

**Desvantagem**:
- Arquivo maior (~80-100 MB)

**Responsável**: Claude Code
**Duração**: 2 horas

---

## ✅ FASE 4: AUTOMAÇÃO (3-4 horas)

### T4.1 - Atualizar build-macos.sh
**Descrição**: Script completo que suporta todas as arquiteturas

**Novo build-macos.sh**:

```bash
#!/bin/bash

ARCH="${1:-universal}"  # intel, arm64, ou universal

case "$ARCH" in
    intel)
        echo "Building for Intel (x86_64)..."
        bash build-macos-intel.sh
        ;;
    arm)
        echo "Building for Apple Silicon (arm64)..."
        bash build-macos-arm.sh
        ;;
    universal)
        echo "Building universal binary..."
        bash build-macos-intel.sh
        bash build-macos-arm.sh
        bash build-macos-universal.sh
        ;;
    *)
        echo "Usage: $0 [intel|arm|universal]"
        exit 1
        ;;
esac

echo "✓ Build complete!"
```

**Tarefas**:
- [ ] Atualizar `build-macos.sh`
- [ ] Testar: `./build-macos.sh intel`
- [ ] Testar: `./build-macos.sh arm`
- [ ] Testar: `./build-macos.sh universal`
- [ ] Adicionar flags: `--sign` para code sign, `--notarize` para notarization

**Responsável**: Claude Code
**Duração**: 1 hora

---

### T4.2 - GitHub Actions para macOS
**Descrição**: Build automático de múltiplas arquiteturas

**Arquivo**: `.github/workflows/build-macos.yml`

```yaml
name: Build macOS (Intel + Apple Silicon)

on:
  push:
    tags:
      - 'v*'

jobs:
  build-macos-intel:
    runs-on: macos-12  # Intel
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-python@v4
        with:
          python-version: '3.11'
      - uses: actions/setup-node@v3
        with:
          node-version: '18'

      - name: Build for Intel
        run: bash build-macos.sh intel

      - name: Upload Intel DMG
        uses: actions/upload-artifact@v3
        with:
          name: macos-intel
          path: dist/Etnopapers-Intel-v*.dmg

  build-macos-arm:
    runs-on: macos-14  # Apple Silicon (M3)
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-python@v4
        with:
          python-version: '3.11'
      - uses: actions/setup-node@v3
        with:
          node-version: '18'

      - name: Build for Apple Silicon
        run: bash build-macos.sh arm

      - name: Upload ARM DMG
        uses: actions/upload-artifact@v3
        with:
          name: macos-arm
          path: dist/Etnopapers-ARM-v*.dmg

  build-macos-universal:
    runs-on: macos-14  # M3 pode emular Intel
    needs: [build-macos-intel, build-macos-arm]
    steps:
      - uses: actions/checkout@v3

      - name: Download Intel build
        uses: actions/download-artifact@v3
        with:
          name: macos-intel

      - name: Download ARM build
        uses: actions/download-artifact@v3
        with:
          name: macos-arm

      - name: Create universal binary
        run: bash build-macos.sh universal

      - name: Notarize (optional)
        if: ${{ secrets.APPLE_ID != '' }}
        env:
          APPLE_ID: ${{ secrets.APPLE_ID }}
          APP_PASSWORD: ${{ secrets.APP_PASSWORD }}
        run: bash installer/macos/notarize.sh $APPLE_ID $APP_PASSWORD

      - name: Upload to release
        uses: softprops/action-gh-release@v1
        with:
          files: |
            dist/Etnopapers-Intel-v*.dmg
            dist/Etnopapers-ARM-v*.dmg
            dist/Etnopapers-v*.dmg
```

**Tarefas**:
- [ ] Criar `.github/workflows/build-macos.yml`
- [ ] Testar workflow localmente (act)
- [ ] Adicionar secrets (se notarizing):
  - [ ] `APPLE_ID` (email)
  - [ ] `APP_PASSWORD` (app-specific password)
- [ ] Push tag de teste: `git tag v2.1.0-test && git push origin v2.1.0-test`
- [ ] Verificar que GitHub Actions constrói DMGs

**Responsável**: Claude Code
**Duração**: 1.5 horas

---

### T4.3 - Configurar Notarization Automática (Opcional)
**Descrição**: Notarizar automaticamente via GitHub Actions

**Tarefas**:
- [ ] Se com Apple Developer Account:
  - [ ] Criar App-specific password
  - [ ] Adicionar como GitHub Secret: `APPLE_ID`, `APP_PASSWORD`
  - [ ] GitHub Actions notariza automaticamente

- [ ] Se sem Apple Developer Account:
  - [ ] Pular este passo (user pode notarizar manualmente depois)
  - [ ] DMG ainda funciona (pede permissão na primeira execução)

**Responsável**: Claude Code (se com account)
**Duração**: 1 hora (setup)

---

## ✅ FASE 5: TESTES (3-4 horas)

### T5.1 - Plano de Testes macOS
**Descrição**: Testes em máquinas reais

**Ambiente de Teste**:
- [ ] Intel Mac (10.13 ou mais novo)
- [ ] Apple Silicon Mac (M1/M2/M3)

**Testes**:

1. **Teste de Instalação**
   - [ ] Fazer download DMG
   - [ ] Duplo-clique em DMG
   - [ ] Arrastar app para Applications
   - [ ] Eject DMG

2. **Teste de Performance**
   - [ ] Startup time: `time /Applications/Etnopapers.app/Contents/MacOS/Etnopapers`
     - Meta: 2-4 segundos (vs. 5-10 antes)
   - [ ] RAM: `top -l 1 | grep Etnopapers`
     - Meta: 20-30% (vs. 50-80% antes)
   - [ ] CPU: `top -l 1 | grep Etnopapers`
     - Meta: 10-20% (vs. 30-50% antes)

3. **Teste de Funcionalidade**
   - [ ] App abre normalmente
   - [ ] Interface React carrega
   - [ ] Ollama detectado (ou aviso)
   - [ ] Upload PDF funciona
   - [ ] MongoDB conecta

4. **Teste de Code Signing**
   - [ ] `codesign --verify -v /Applications/Etnopapers.app`
   - [ ] Deve mostrar "valid on disk" sem warnings

5. **Teste de Notarization** (se notarizado)
   - [ ] `spctl -a -v /Applications/Etnopapers.app`
   - [ ] Deve mostrar "accepted"

6. **Teste de Arquitetura Correta**
   - [ ] Em Intel: `file /Applications/Etnopapers.app/Contents/MacOS/Etnopapers`
     - Deve mostrar "Mach-O i386"
   - [ ] Em ARM: `file /Applications/Etnopapers.app/Contents/MacOS/Etnopapers`
     - Deve mostrar "Mach-O arm64"

**Saída Esperada**:
```
TESTES PASSARAM ✓
- Instalação: OK
- Startup: 2.5s (vs. 7s antes)
- RAM: 25% (vs. 65% antes)
- Code signed: OK
- Notarized: OK
- Intel teste: OK
- ARM teste: OK
```

**Responsável**: Claude Code
**Duração**: 1.5 horas

---

### T5.2 - Guia de Troubleshooting macOS
**Descrição**: Resolver problemas comuns

**Arquivo**: `docs/MACOS_TROUBLESHOOTING.md`

```markdown
# Troubleshooting - Etnopapers macOS

## "App não abre - 'Desenvolvedor não identificado'"

**Se não assinado (expected):**
1. System Settings > Privacy & Security
2. Scroll para "Etnopapers"
3. Clique "Open Anyway"

**Se assinado mas ainda aparece:**
- Verificar assinatura: `codesign --verify -v /Applications/Etnopapers.app`
- Resginar se necessário: `bash installer/macos/sign-app.sh`

## "App trava no startup"

**Verificar:**
- `log stream --predicate 'process == "Etnopapers"'`
- Se com Rosetta2: `arch -arm64 /Applications/Etnopapers.app`

## "Python não encontrado"

**Solução:**
```bash
# Verificar Python
python3 --version

# Se não encontrado, instalar:
# https://www.python.org/downloads/

# Ou usar Homebrew:
brew install python@3.11
```

## "Ollama não conecta"

**Verificar:**
```bash
# 1. Ollama rodando?
curl http://localhost:11434/api/tags

# 2. Não está? Instalar e iniciar
# https://ollama.com/download
```

## "Notarization falhou"

**Causas comuns:**
- DMG não assinado
- ID do bundle incorreto
- Apple ID inválido
- Network issue (tentar depois)

**Solução:**
- Verificar: `codesign --verify -v Etnopapers.dmg`
- Resginar se necessário
- Tentar notarização novamente
```

**Tarefas**:
- [ ] Criar `docs/MACOS_TROUBLESHOOTING.md`
- [ ] Documentar 10+ problemas comuns
- [ ] Incluir comandos de diagnóstico

**Responsável**: Claude Code
**Duração**: 1 hora

---

### T5.3 - Guia de Instalação macOS
**Descrição**: Documentação para usuários finais

**Arquivo**: `docs/GUIA_INSTALACAO_MACOS.md`

```markdown
# Guia de Instalação - Etnopapers macOS

## Passo 1: Baixar
1. Visite: https://github.com/edalcin/etnopapers/releases
2. Baixe: `Etnopapers-v2.1.dmg` (seu Mac Intel ou Apple Silicon)

## Passo 2: Instalar
1. Duplo-clique em `Etnopapers-v2.1.dmg`
2. Arraste `Etnopapers.app` para pasta `Applications`
3. Eject o DMG (arraste para Trash ou Cmd+E)
4. Abra `Applications` no Finder

## Passo 3: Primeira Execução
1. Duplo-clique em `Etnopapers.app`
2. macOS pode perguntar sobre permissões
3. Clique "Open" para confirmar
4. Aparecerá tela de configuração

## Passo 4: Instalar Ollama (Obrigatório)
1. Visite: https://ollama.com/download
2. Baixe para macOS
3. Instale e execute
4. Ollama deve ficar rodando na bandeja do sistema

## Passo 5: Usar Etnopapers
1. Abra `Etnopapers` de Applications
2. Aparecerá interface
3. Faça upload de seu primeiro PDF
4. Pronto!

## Problemas?
Veja: docs/MACOS_TROUBLESHOOTING.md
```

**Tarefas**:
- [ ] Criar `docs/GUIA_INSTALACAO_MACOS.md`
- [ ] Incluir screenshots (se possível)
- [ ] Traduzir para português

**Responsável**: Claude Code
**Duração**: 45 min

---

## ✅ FASE 6: DOCUMENTAÇÃO (2-3 horas)

### T6.1 - Atualizar GUIA_DESENVOLVEDOR.md
**Descrição**: Adicionar instruções novo build para desenvolvedores

**Seções a adicionar**:
- [ ] macOS Build (Intel + ARM)
- [ ] Code Signing
- [ ] Notarization
- [ ] DMG Creation
- [ ] GitHub Actions CI/CD

**Responsável**: Claude Code
**Duração**: 1 hora

---

### T6.2 - Criar README-MACOS.md
**Descrição**: Visão geral do build macOS

**Arquivo**: `installer/README-MACOS.md`

```markdown
# Etnopapers macOS Build (v2.1)

## Arquivos

- **Etnopapers-v2.1.dmg** - DMG Universal (30-40 MB)
  - Funciona em Intel e Apple Silicon
  - Padrão macOS
  - Recomendado

- **Etnopapers-Intel-v2.1.dmg** - Intel apenas (30-40 MB)
  - Para Macs Intel apenas

- **Etnopapers-ARM-v2.1.dmg** - Apple Silicon apenas (25-35 MB)
  - Para M1/M2/M3 Macs apenas

## Build Scripts

- `build-macos.sh` - Build principal
  - `./build-macos.sh intel` - Apenas Intel
  - `./build-macos.sh arm` - Apenas ARM
  - `./build-macos.sh universal` - Universal (padrão)

- `build-macos-intel.sh` - Build Intel detalhado
- `build-macos-arm.sh` - Build ARM detalhado
- `build-macos-universal.sh` - Combinar Intel + ARM

## Code Signing & Notarization

- `installer/macos/sign-app.sh` - Assinar app
- `installer/macos/notarize.sh` - Notarizar DMG
- `installer/macos/entitlements.plist` - Configurações de segurança

## Requisitos

- macOS 10.13 ou mais novo
- Python 3.11+
- Xcode Command Line Tools
- (Opcional) Developer Certificate ($99/ano)

## Tamanho & Performance

| Métrica | Esperado |
|---------|----------|
| DMG Download | 30-40 MB |
| Espaço em Disco | 100-150 MB |
| Startup | 2-4 segundos |
| RAM | 20-30% |
| CPU | 10-20% |
```

**Tarefas**:
- [ ] Criar `installer/README-MACOS.md`
- [ ] Documentar build scripts
- [ ] Documentar code signing

**Responsável**: Claude Code
**Duração**: 1 hora

---

## 📊 RESUMO DE TAREFAS

| Fase | Tarefa | Duração | Status |
|------|--------|---------|--------|
| 1 | T1.1 - Análise | 45 min | ⏳ Pending |
| 1 | T1.2 - Ferramentas | 1h | ⏳ Pending |
| 1 | T1.3 - Estrutura | 45 min | ⏳ Pending |
| 2 | T2.1 - build.spec | 1.5h | ⏳ Pending |
| 2 | T2.2 - Code Signing | 1.5h | ⏳ Pending |
| 2 | T2.3 - DMG Installer | 2h | ⏳ Pending |
| 2 | T2.4 - Notarization | 1.5h | ⏳ Pending |
| 2 | T2.5 - Install Script | 1h | ⏳ Pending |
| 2 | T2.6 - requirements | 30 min | ⏳ Pending |
| 3 | T3.1 - Intel Build | 1.5h | ⏳ Pending |
| 3 | T3.2 - ARM Build | 1.5h | ⏳ Pending |
| 3 | T3.3 - Universal | 2h | ⏳ Pending |
| 4 | T4.1 - build-macos.sh | 1h | ⏳ Pending |
| 4 | T4.2 - GitHub Actions | 1.5h | ⏳ Pending |
| 4 | T4.3 - Notarization Auto | 1h | ⏳ Pending |
| 5 | T5.1 - Testes | 1.5h | ⏳ Pending |
| 5 | T5.2 - Troubleshooting | 1h | ⏳ Pending |
| 5 | T5.3 - Guia Usuário | 45 min | ⏳ Pending |
| 6 | T6.1 - Atualizar Docs | 1h | ⏳ Pending |
| 6 | T6.2 - README-MACOS | 1h | ⏳ Pending |
| **TOTAL** | **21 Tarefas** | **27h** | |

---

## 🎯 Próximos Passos

1. ✅ Revisar este plano
2. ⏳ Iniciar T1.1 - Análise do app atual
3. ⏳ Decidir sobre prioridades
4. ⏳ Começar T2.1 - build.spec otimizado

---

**Status**: 📋 Detalhado e Pronto para Implementação
**Próximo**: Confirmação para iniciar T1.1
