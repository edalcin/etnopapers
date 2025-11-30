# Lista de Tarefas - Otimização do Build Windows (v2.1)

**Objetivo**: Converter de PyInstaller monolítico (39MB) para MSI Installer + App modular
**Prioridade**: 🔴 CRÍTICA
**Tempo Total Estimado**: 10-16 horas
**Data Início**: 2025-11-30

---

## ✅ FASE 1: PREPARAÇÃO (1-2 horas)

### T1.1 - Analisar Distribuição do Executável Atual
**Descrição**: Entender exatamente o que está causando os 39 MB

**Tarefas**:
- [ ] Executar `pyinstaller --analyze build.spec` e salvar output
- [ ] Usar `PyInstaller\utils\win32\wchar_t.py` para listar dependências
- [ ] Verificar tamanho de cada módulo Python incluído
- [ ] Documentar módulos não-essenciais
- [ ] Medir tempo de descompactação em %APPDATA%

**Saída Esperada**:
```
Análise do build.spec:
- Total de módulos: 250+
- Tamanho Python: ~150 MB (descompactado)
- Tamanho React: ~50 MB
- Tamanho dependências: ~100 MB
- Overhead PyInstaller: ~15 MB

Módulos desnecessários:
- pytest (5 MB)
- numpy (30 MB) - não usado
- pandas (20 MB) - não usado
- matplotlib (15 MB) - não usado
```

**Responsável**: Claude Code
**Duração**: 30 min

---

### T1.2 - Decidir Ferramenta de Installer Windows
**Descrição**: Escolher entre WiX, NSIS ou Inno Setup

**Opções Avaliadas**:

| Ferramenta | Tamanho | Curva Aprendizado | Produção | Nota |
|-----------|--------|------------------|----------|------|
| **WiX** | 5-10 MB | Média | ⭐⭐⭐⭐⭐ | **RECOMENDADO** |
| **NSIS** | 3-5 MB | Fácil | ⭐⭐⭐ | Mais simples |
| **Inno Setup** | 5-7 MB | Fácil | ⭐⭐⭐⭐ | Bom meio termo |

**Critérios de Seleção**:
- ✅ WiX eleito por:
  - Controle total sobre instalação
  - Suporte nativo a MSI (padrão Windows)
  - Pode detectar Python pré-instalado
  - Integração Windows Add/Remove Programs
  - Melhor para produção profissional
  - Suporte a rollback/upgrade automático

**Tarefas**:
- [ ] Instalar WiX Toolset 3.14+
- [ ] Verificar `wix --version`
- [ ] Ler documentação WiX (30 min)
- [ ] Criar estrutura de diretórios para MSI

```bash
# Instalação WiX no Windows
choco install wixtoolset -y
# ou baixar de: https://github.com/wixtoolset/wix3/releases
```

**Responsável**: Claude Code
**Duração**: 45 min

---

### T1.3 - Preparar Estrutura para Novo Build
**Descrição**: Criar diretórios e arquivos necessários

**Tarefas**:
- [ ] Criar `installer/` directory
- [ ] Criar `installer/wix/` para arquivos WiX
- [ ] Criar `installer/python/` para Python portável (opcional)
- [ ] Criar `build.spec.optimized` para comparação
- [ ] Documentar estrutura em docs/

**Estrutura**:
```
etnopapers/
├── installer/
│   ├── wix/
│   │   ├── Product.wxs           # Definição MSI
│   │   ├── UI.wxs                # Interface do installer
│   │   └── Features.wxs           # Recursos a instalar
│   │
│   ├── scripts/
│   │   ├── pre-install.ps1       # Checagens antes instalação
│   │   ├── post-install.ps1      # Setup após instalação
│   │   └── check-python.ps1      # Detectar Python
│   │
│   └── README.md                 # Documentação do installer
│
├── build.spec                    # Spec atual (deixar como backup)
├── build.spec.optimized          # Novo spec otimizado
└── build-windows.bat             # Script build atualizado
```

**Responsável**: Claude Code
**Duração**: 30 min

---

## ✅ FASE 2: IMPLEMENTAÇÃO (4-6 horas)

### T2.1 - Criar build.spec Otimizado para Windows
**Descrição**: Reescrever build.spec removendo tudo desnecessário

**Mudanças Principais**:

**1. Usar "onedir" em vez de "onefile"**
```python
# Antes (lento, desempacta 39MB):
exe = EXE(..., onefile=True)

# Depois (rápido, acessa direto em disco):
exe = EXE(..., onefile=False)  # onedir
```

**2. Excluir muito mais módulos**
```python
excludes=[
    # Testes
    'pytest', 'pytest-cov', 'pytest-asyncio',

    # Ciência de dados (não usado)
    'numpy', 'scipy', 'pandas', 'matplotlib',
    'sklearn', 'seaborn', 'plotly',

    # Imagem (não usado)
    'PIL', 'cv2', 'pillow',

    # Jupyter (não usado)
    'jupyter', 'ipython', 'notebook',

    # Standard lib não usada
    'email', 'http.server', 'ftplib',
    'telnetlib', 'nis', 'ossaudiodev',

    # Web (substituído por requests)
    'urllib3', 'certifi',

    # Misc
    'setuptools', 'pkg_resources',
]
```

**3. Limpar hidden imports**
```python
hiddenimports=[
    # Essencial
    'uvicorn.logging',
    'uvicorn.loops.auto',
    'uvicorn.protocols.http.auto',
    'fastapi.responses',
    'fastapi.staticfiles',

    # Banco de dados
    'pymongo',

    # Processamento
    'pdfplumber',

    # IA
    'instructor',
]
```

**4. Compilar bytecode otimizado**
```python
a = Analysis(
    ...
    # Compilar para .pyc otimizado
    compile_to_bytecode=[],
)
```

**Tarefas**:
- [ ] Copiar `build.spec` → `build.spec.optimized`
- [ ] Remover todas as exclusões de teste/ML
- [ ] Remover hiddenimports desnecessários
- [ ] Testar build: `pyinstaller build.spec.optimized --clean`
- [ ] Medir tamanho: `(dir dist\etnopapers).SumFileSize` MB
- [ ] Meta: 15-20 MB (redução de ~50%)

**Resultado Esperado**:
```
Antes: dist/etnopapers.exe (39 MB)
Depois: dist/etnopapers/ (~300 MB total, mas app.exe ~15 MB)
Redução de download: 75%
```

**Responsável**: Claude Code
**Duração**: 1.5 horas

---

### T2.2 - Criar Arquivo WiX Principal (Product.wxs)
**Descrição**: Definir o que o installer faz

**Arquivo**: `installer/wix/Product.wxs`

```xml
<?xml version="1.0" encoding="UTF-8"?>
<Wix xmlns="http://schemas.microsoft.com/wix/2006/wi">
    <Product Id="*"
             Name="Etnopapers"
             Language="1033"
             Version="2.1.0.0"
             Manufacturer="Etnopapers"
             UpgradeCode="12345678-1234-1234-1234-123456789012">

        <Package InstallerVersion="200" Compressed="yes" Platform="x64" />

        <!-- Detectar instalações anteriores -->
        <MajorUpgrade DowngradeErrorMessage=
            "Uma versão mais recente já está instalada." />

        <!-- Pasta de instalação padrão -->
        <InstallScope Scope="perMachine" />
        <Directory Id="ProgramFilesFolder">
            <Directory Id="INSTALLFOLDER" Name="Etnopapers" />
        </Directory>

        <!-- Arquivos a instalar -->
        <Feature Id="ProductFeature" Title="Etnopapers" Level="1">
            <ComponentGroupRef Id="ProductComponents" />
            <Feature Id="OllamaFeature" Title="Ollama (Informação)" Level="1">
                <!-- Mostrar nota sobre Ollama -->
            </Feature>
        </Feature>

        <!-- UI customizada -->
        <UIRef Id="WixUI_Minimal" />
    </Product>
</Wix>
```

**Tarefas**:
- [ ] Criar `installer/wix/Product.wxs`
- [ ] Definir GUID (UpgradeCode)
- [ ] Adicionar recurso para Python 3.11+
- [ ] Adicionar recurso para app Etnopapers
- [ ] Configurar pasta de instalação
- [ ] Adicionar atalhos Menu Iniciar
- [ ] Configurar desinstalação

**Responsável**: Claude Code
**Duração**: 1.5 horas

---

### T2.3 - Criar Arquivo de Componentes WiX (Components.wxs)
**Descrição**: Definir quais arquivos instalar

```xml
<Wix xmlns="http://schemas.microsoft.com/wix/2006/wi">
    <Fragment>
        <DirectoryRef Id="INSTALLFOLDER">

            <!-- Aplicação executável -->
            <Component Id="EtnopapersExe" Guid="...">
                <File Id="EtnopapersExeFile"
                      Source="dist/etnopapers/etnopapers.exe"
                      KeyPath="yes" />
            </Component>

            <!-- Backend -->
            <Directory Id="BackendDir" Name="backend">
                <Component Id="BackendComponent" Guid="...">
                    <File Source="dist/etnopapers/backend/**" />
                </Component>
            </Directory>

            <!-- Frontend -->
            <Directory Id="FrontendDir" Name="frontend">
                <Component Id="FrontendComponent" Guid="...">
                    <File Source="dist/etnopapers/frontend/dist/**" />
                </Component>
            </Directory>

        </DirectoryRef>

        <!-- Shortcut no Menu Iniciar -->
        <DirectoryRef Id="ProgramMenuFolder">
            <Component Id="MenuShortcut" Guid="...">
                <Shortcut Id="Etnopapers" Name="Etnopapers"
                          Target="[INSTALLFOLDER]etnopapers.exe" />
                <RemoveFolder Id="ProgramMenuFolder" On="uninstall" />
                <RegistryValue Root="HKCU"
                               Key="Software\Microsoft\Windows\CurrentVersion\..."
                               Name="installed" Value="1" Type="integer" />
            </Component>
        </DirectoryRef>

        <ComponentGroup Id="ProductComponents">
            <ComponentRef Id="EtnopapersExe" />
            <ComponentRef Id="BackendComponent" />
            <ComponentRef Id="FrontendComponent" />
            <ComponentRef Id="MenuShortcut" />
        </ComponentGroup>
    </Fragment>
</Wix>
```

**Tarefas**:
- [ ] Criar arquivo `Components.wxs`
- [ ] Incluir executável principal
- [ ] Incluir arquivos backend (*.py)
- [ ] Incluir arquivos frontend (estáticos)
- [ ] Criar shortcuts Menu Iniciar/Desktop
- [ ] Adicionar chaves de registro (Add/Remove Programs)

**Responsável**: Claude Code
**Duração**: 1 hora

---

### T2.4 - Criar Script de Verificação Python
**Descrição**: Detectar ou avisar sobre Python

**Arquivo**: `installer/scripts/check-python.ps1`

```powershell
# Detectar Python 3.11+
$pythonPath = Get-Command python.exe 2>$null
if ($pythonPath) {
    $version = python --version 2>&1
    if ($version -match "3.1[1-9]|3.[2-9]") {
        Write-Host "✓ Python encontrado: $version"
        return $true
    }
}

# Python não encontrado - oferecer opções
$result = [Windows.Forms.MessageBox]::Show(
    "Python 3.11+ não encontrado.`n`n" +
    "Escolha:`n" +
    "- [Sim] Baixar Python de python.org`n" +
    "- [Não] Tentar instalar mesmo assim",
    "Python não detectado",
    [Windows.Forms.MessageBoxButtons]::YesNo
)

return $false
```

**Tarefas**:
- [ ] Criar `installer/scripts/check-python.ps1`
- [ ] Testar detecção Python instalado
- [ ] Oferecer download se não encontrado
- [ ] Integrar em Product.wxs

**Responsável**: Claude Code
**Duração**: 45 min

---

### T2.5 - Criar Script Post-Instalação
**Descrição**: Setup após os arquivos serem copiados

**Arquivo**: `installer/scripts/post-install.ps1`

```powershell
# Post-install script
$appDir = "$env:ProgramFiles\Etnopapers"

# 1. Instalar dependências Python
Write-Host "Instalando dependências Python..."
python -m pip install -r "$appDir\requirements-windows.txt" --quiet

# 2. Criar shortcuts
Write-Host "Criando atalhos..."
$shell = New-Object -ComObject WScript.Shell
$shortcut = $shell.CreateShortCut("$env:APPDATA\Microsoft\Windows\Start Menu\Programs\Etnopapers.lnk")
$shortcut.TargetPath = "$appDir\etnopapers.exe"
$shortcut.Save()

# 3. Informar sobre Ollama
Write-Host "Verificando Ollama..."
$ollamaOk = $false
try {
    $response = Invoke-WebRequest -Uri "http://localhost:11434/api/tags" -TimeoutSec 2
    $ollamaOk = $true
    Write-Host "✓ Ollama detectado"
} catch {
    Write-Host "⚠ Ollama não detectado"
    Write-Host "  Baixe em: https://ollama.com/download"
}

Write-Host "Instalação concluída!"
```

**Tarefas**:
- [ ] Criar `installer/scripts/post-install.ps1`
- [ ] Instalar requirements.txt
- [ ] Criar shortcuts
- [ ] Verificar Ollama
- [ ] Mostrar mensagens úteis

**Responsável**: Claude Code
**Duração**: 45 min

---

### T2.6 - Criar requirements-windows.txt Otimizado
**Descrição**: Apenas dependências de produção

**Arquivo**: `requirements-windows.txt`

```
# FastAPI + servidor
fastapi==0.110.0
uvicorn[standard]==0.29.0
aiofiles==23.2.1

# Modelos de dados
pydantic==2.8.0
pydantic-settings==2.4.0

# Banco de dados
pymongo==4.5.0

# Processamento PDF
pdfplumber==0.10.3

# IA/LLM
instructor==1.3.7

# HTTP
requests==2.31.0
httpx==0.27.0

# Ambiente
python-dotenv==1.0.0

# Não incluir dependências de desenvolvimento:
# pytest, pyinstaller, numpy, pandas, etc.
```

**Tarefas**:
- [ ] Criar `requirements-windows.txt` (reduzido)
- [ ] Listar apenas dependências de produção
- [ ] Testar: `pip install -r requirements-windows.txt`
- [ ] Medir tamanho: ~50-80 MB (vs 300+ antes)

**Responsável**: Claude Code
**Duração**: 30 min

---

## ✅ FASE 3: AUTOMAÇÃO (2-3 horas)

### T3.1 - Atualizar build-windows.bat
**Descrição**: Script completo de build para Windows

**Arquivo**: `build-windows.bat` (atualizado)

```batch
@echo off
setlocal enabledelayedexpansion

echo.
echo ============================================
echo   Etnopapers v2.1 - Windows Build
echo ============================================
echo.

REM Verificar Python
python --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: Python não encontrado
    pause
    exit /b 1
)

REM Verificar Node.js
node --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: Node.js não encontrado
    pause
    exit /b 1
)

REM Verificar WiX
where candle.exe >nul 2>&1
if errorlevel 1 (
    echo ERROR: WiX Toolset não encontrado
    echo Install: choco install wixtoolset
    pause
    exit /b 1
)

echo [1/5] Building frontend...
cd frontend
call npm run build
if errorlevel 1 exit /b 1
cd ..
echo ✓ Frontend built

echo.
echo [2/5] Building application (PyInstaller)...
pyinstaller build.spec.optimized --clean
if errorlevel 1 exit /b 1
echo ✓ App built

echo.
echo [3/5] Compiling WiX...
cd installer\wix
call candle.exe Product.wxs Components.wxs UI.wxs -out obj\
if errorlevel 1 exit /b 1
cd ..\..
echo ✓ WiX compiled

echo.
echo [4/5] Linking MSI installer...
cd installer\wix
call light.exe obj\*.wixobj -out ..\..\dist\Etnopapers-Setup-v2.1.msi
if errorlevel 1 exit /b 1
cd ..\..
echo ✓ MSI created

echo.
echo [5/5] Cleanup...
rmdir /s /q installer\wix\obj
echo ✓ Cleanup done

echo.
echo ============================================
echo   BUILD SUCCESSFUL!
echo ============================================
echo.
echo Output:
echo   - dist\Etnopapers-Setup-v2.1.msi (5-10 MB)
echo   - dist\etnopapers\ (app folder)
echo.
pause
```

**Tarefas**:
- [ ] Atualizar `build-windows.bat`
- [ ] Adicionar verificações de ferramentas
- [ ] Testar build completo
- [ ] Verificar saída final

**Responsável**: Claude Code
**Duração**: 1 hora

---

### T3.2 - Criar GitHub Actions Workflow para Windows
**Descrição**: Build automático ao fazer push de tags

**Arquivo**: `.github/workflows/build-windows-msi.yml`

```yaml
name: Build Windows MSI Installer

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

      - name: Install WiX Toolset
        run: choco install wixtoolset -y

      - name: Install Python dependencies
        run: pip install -r requirements.txt

      - name: Build frontend
        run: cd frontend && npm install && npm run build && cd ..

      - name: Build application
        run: pyinstaller build.spec.optimized --clean

      - name: Build MSI installer
        run: .\build-windows.bat

      - name: Upload to release
        uses: softprops/action-gh-release@v1
        with:
          files: |
            dist/Etnopapers-Setup-v2.1.msi
            dist/etnopapers.exe
```

**Tarefas**:
- [ ] Criar `.github/workflows/build-windows-msi.yml`
- [ ] Testar workflow localmente
- [ ] Push tag de teste: `git tag v2.1.0-test && git push origin v2.1.0-test`
- [ ] Verificar que GitHub Actions constrói MSI

**Responsável**: Claude Code
**Duração**: 1 hora

---

## ✅ FASE 4: TESTES E VALIDAÇÃO (2-3 horas)

### T4.1 - Criar Plano de Testes Windows
**Descrição**: Testes em máquina Windows real

**Ambiente de Teste**:
- [ ] Windows 10/11 limpo (VM ou máquina real)
- [ ] Sem Python pré-instalado
- [ ] Sem Ollama instalado
- [ ] RAM limitada (2-4 GB) - teste de estresse

**Testes**:

1. **Teste de Instalação**
   - [ ] Executar Etnopapers-Setup-v2.1.msi
   - [ ] Verificar que Python é instalado (se necessário)
   - [ ] Verificar que arquivos vão para `C:\Program Files\Etnopapers`
   - [ ] Verificar atalhos no Menu Iniciar
   - [ ] Verificar entrada em Add/Remove Programs

2. **Teste de Performance**
   - [ ] Medir tempo de inicialização: `etnopapers.exe` (meta: <5 segundos)
   - [ ] Medir RAM inicial: Task Manager (meta: <30%)
   - [ ] Medir CPU inicial: Task Manager (meta: <20%)
   - [ ] Comparar com build antigo (39 MB)

3. **Teste de Funcionalidade**
   - [ ] Carregar aplicação (deve mostrar interface)
   - [ ] Fazer upload de PDF de teste
   - [ ] Verificar que Ollama é detectado (ou aviso)
   - [ ] Testar integração com MongoDB local
   - [ ] Testar search/filter de artigos

4. **Teste de Desinstalação**
   - [ ] Remover programa via Add/Remove Programs
   - [ ] Verificar que nenhum arquivo fica em `C:\Program Files\`
   - [ ] Verificar que atalhos são removidos
   - [ ] Limpar `%APPDATA%\Etnopapers` (se criado)

5. **Teste de Upgrade**
   - [ ] Instalar v2.1.0
   - [ ] Instalar v2.1.1 (upgrade)
   - [ ] Verificar que dados não são perdidos
   - [ ] Verificar que atalhos continuam funcionando

**Saída Esperada**:
```
TESTE DE PERFORMANCE
====================
Executável antigo (PyInstaller):
- Tamanho: 39 MB
- Tempo inicialização: 15-30 segundos
- RAM inicial: 100%+
- CPU inicial: 100%
- Status: FALHA (travamento)

Novo MSI:
- Tamanho download: 5-10 MB
- Tempo inicialização: 2-5 segundos
- RAM inicial: 20-30%
- CPU inicial: 10-20%
- Status: SUCESSO ✓
```

**Responsável**: Claude Code (em máquina Windows)
**Duração**: 1.5 horas

---

### T4.2 - Criar Documentação de Troubleshooting
**Descrição**: Guia para resolver problemas comuns

**Arquivo**: `docs/WINDOWS_TROUBLESHOOTING.md`

```markdown
# Troubleshooting - Etnopapers Windows

## "MSI não se instala"

**Solução**:
```batch
# Executar como administrador
msiexec /i Etnopapers-Setup-v2.1.msi /l*v install.log
```

## "Python não encontrado após instalação"

**Solução**:
- Reinstalar MSI com opção "Instalar Python"
- Ou instalar Python manualmente de python.org

## "Ollama não funciona"

**Solução**:
- Baixar Ollama de https://ollama.com/download
- Instalar e iniciar Ollama desktop
- Etnopapers deve detectar automaticamente

## "Aplicação travando na inicialização"

Se ainda usar build antigo:
- Desinstalar versão antiga
- Instalar nova versão (MSI)
- Limpar %APPDATA%\Etnopapers

## "MongoDB não encontrado"

**Solução**:
- Usar MongoDB local: https://www.mongodb.com/try/download/community
- Ou MongoDB Atlas (nuvem): https://www.mongodb.com/cloud/atlas
- Configurar MONGO_URI no launcher

## "Erro de permissão"

**Solução**:
- Executar como administrador
- Ou instalar em `C:\Users\[USER]\AppData\Local\` em vez de Program Files
```

**Tarefas**:
- [ ] Criar `docs/WINDOWS_TROUBLESHOOTING.md`
- [ ] Documentar 10+ problemas comuns
- [ ] Incluir comandos de diagnóstico
- [ ] Incluir links para recursos externos

**Responsável**: Claude Code
**Duração**: 45 min

---

### T4.3 - Criar Guia de Instalação do Usuário
**Descrição**: Documentação simples para usuários finais

**Arquivo**: `docs/GUIA_INSTALACAO_WINDOWS.md`

```markdown
# Guia de Instalação - Etnopapers Windows

## Passo 1: Baixar
1. Visite: https://github.com/edalcin/etnopapers/releases
2. Baixe: `Etnopapers-Setup-v2.1.msi`

## Passo 2: Instalar
1. Duplo-clique em `Etnopapers-Setup-v2.1.msi`
2. Clique "Próximo" até concluir
3. Python será instalado automaticamente (se necessário)

## Passo 3: Instalar Ollama (Obrigatório)
1. Visite: https://ollama.com/download
2. Baixe e instale Ollama
3. Abra Ollama (deve ficar na bandeja do sistema)
4. Puxe modelo: `ollama pull qwen2.5:7b-instruct-q4_K_M`

## Passo 4: Usar Etnopapers
1. Abra "Etnopapers" do Menu Iniciar
2. Aguarde carregar (2-5 segundos)
3. Faça upload de seu primeiro PDF
4. Pronto!

## Problemas?
Veja: docs/WINDOWS_TROUBLESHOOTING.md
```

**Tarefas**:
- [ ] Criar `docs/GUIA_INSTALACAO_WINDOWS.md` (português)
- [ ] Incluir screenshots (se possível)
- [ ] Incluir dicas e boas práticas

**Responsável**: Claude Code
**Duração**: 30 min

---

## ✅ FASE 5: DOCUMENTAÇÃO (1-2 horas)

### T5.1 - Atualizar GUIA_DESENVOLVEDOR.md
**Descrição**: Adicionar instruções novo build para desenvolvedores

**Mudanças**:
- [ ] Adicionar seção "Windows Build (MSI)"
- [ ] Documentar novo fluxo: `pyinstaller build.spec.optimized`
- [ ] Documentar como compilar WiX
- [ ] Adicionar troubleshooting de build

**Responsável**: Claude Code
**Duração**: 45 min

---

### T5.2 - Criar README-WINDOWS.md
**Descrição**: Visão geral do novo sistema de build

**Arquivo**: `installer/README.md`

```markdown
# Etnopapers Windows Installer (v2.1)

## Arquivos

- **Etnopapers-Setup-v2.1.msi** - Windows Installer (recomendado)
  - Tamanho: 5-10 MB
  - Instala: Python 3.11+, Etnopapers, Dependências
  - Recomendado para usuários finais

- **etnopapers.exe** - Aplicação standalone
  - Tamanho: 15-20 MB
  - Requer: Python 3.11+ e pip install -r requirements.txt
  - Para desenvolvedores ou users avançados

## Processo de Build

### Para Desenvolvedores

1. **Build da Aplicação**
   ```bash
   pyinstaller build.spec.optimized --clean
   ```

2. **Build do Installer MSI**
   ```bash
   .\build-windows.bat
   ```

3. **Saída**
   - `dist\Etnopapers-Setup-v2.1.msi` (5-10 MB)
   - `dist\etnopapers\` (app folder)

## Estrutura MSI

WiX Toolset é usado para criar o installer. Arquivos:

- `Product.wxs` - Definição principal
- `Components.wxs` - Componentes a instalar
- `UI.wxs` - Interface customizada

## Customizações

### Adicionar arquivo ao installer
Em `Components.wxs`:
```xml
<File Source="seu/arquivo.txt" />
```

### Mudar pasta de instalação
Em `Product.wxs`:
```xml
<Directory Id="ProgramFilesFolder" Name="Program Files">
    <Directory Id="INSTALLFOLDER" Name="Etnopapers" />
</Directory>
```

## Recursos Necessários

- Visual Studio Build Tools (para compilar C++)
- WiX Toolset 3.14+
- Python 3.11+
- Node.js 18+
```

**Tarefas**:
- [ ] Criar `installer/README.md`
- [ ] Documentar estrutura WiX
- [ ] Adicionar exemplos de customização

**Responsável**: Claude Code
**Duração**: 45 min

---

## 📊 RESUMO DE TAREFAS

| Fase | Tarefa | Duração | Status |
|------|--------|---------|--------|
| 1 | T1.1 - Análise | 30 min | ⏳ Pending |
| 1 | T1.2 - Escolher WiX | 45 min | ⏳ Pending |
| 1 | T1.3 - Preparar estrutura | 30 min | ⏳ Pending |
| 2 | T2.1 - build.spec otimizado | 1.5h | ⏳ Pending |
| 2 | T2.2 - Product.wxs | 1.5h | ⏳ Pending |
| 2 | T2.3 - Components.wxs | 1h | ⏳ Pending |
| 2 | T2.4 - check-python.ps1 | 45 min | ⏳ Pending |
| 2 | T2.5 - post-install.ps1 | 45 min | ⏳ Pending |
| 2 | T2.6 - requirements-windows.txt | 30 min | ⏳ Pending |
| 3 | T3.1 - build-windows.bat | 1h | ⏳ Pending |
| 3 | T3.2 - GitHub Actions | 1h | ⏳ Pending |
| 4 | T4.1 - Testes | 1.5h | ⏳ Pending |
| 4 | T4.2 - Troubleshooting | 45 min | ⏳ Pending |
| 4 | T4.3 - Guia usuário | 30 min | ⏳ Pending |
| 5 | T5.1 - Atualizar docs | 45 min | ⏳ Pending |
| 5 | T5.2 - README-WINDOWS | 45 min | ⏳ Pending |
| **TOTAL** | **19 Tarefas** | **14.5 horas** | |

---

## 🎯 Próximos Passos

1. ✅ Revisar este plano
2. ⏳ Iniciar T1.1 - Análise do executável
3. ⏳ Decidir sobre prioridades (pode fazer paralelo)
4. ⏳ Começar T2.1 - build.spec otimizado

---

**Status**: 📋 Detalhado e Pronto para Implementação
**Próximo**: Confirmação para iniciar T1.1
