# Plano de Otimização do Build Windows - Etnopapers v2.1

**Data**: 2025-11-30
**Prioridade**: 🔴 CRÍTICA
**Objetivo**: Resolver problema de build gigante (39MB), alto consumo de memória e falha no carregamento

---

## 🔴 Problema Identificado

### Sintomas Atuais
- **Tamanho**: Executável único com 39 MB
- **Inicialização**: Falha ao carregar (travamento)
- **Memória**: Consumo excessivo (100%+ RAM)
- **Processamento**: 100% CPU durante carregamento
- **Causa Raiz**: PyInstaller agrupando tudo em um executável monolítico

### Problemas com Abordagem Atual (PyInstaller Monolítico)
1. **Descompactação no tempo de inicialização**
   - PyInstaller desempacota 39 MB em memória antes de executar
   - Causa travamento em máquinas com pouca RAM
   - Arquivo temporário gigante criado em `%APPDATA%`

2. **Dependências Bundled Demais**
   - Toda a venv Python incluída (150+ MB de código objeto)
   - Node.js não necessário em produção (já compilado)
   - Módulos de teste incluídos (pytest, etc.)

3. **Sem Otimizações de Produção**
   - Código Python não compilado para bytecode otimizado
   - Imports desnecessários carregados
   - Nenhuma cache de dependências

---

## ✅ Solução Proposta: MSI Installer + App Modular

### Arquitetura Nova

```
Distribuição Etnopapers v2.1
│
├── Setup.exe (Windows Installer - 5-10 MB)
│   └── Instala:
│       ├── Python 3.11+ (runtime only)
│       ├── Dependências Python (requirements.txt)
│       ├── Frontend (arquivos estáticos)
│       └── Scripts de inicialização
│
└── Etnopapers.exe (Aplicação - 10-15 MB)
    └── Carrega:
        ├── FastAPI backend
        ├── React frontend (estático)
        └── Bibliotecas Python (já em disco via installer)
```

### Benefícios

| Aspecto | Antes (PyInstaller) | Depois (MSI) |
|--------|-------------------|------------|
| **Tamanho Instalado** | 39 MB executável | 5-10 MB setup.exe |
| **Tamanho em Disco** | 39 MB | 80-150 MB (com Python) |
| **Tempo de Inicialização** | 10-30 segundos | 2-5 segundos |
| **RAM na Inicialização** | 100%+ | 20-30% |
| **CPU na Inicialização** | 100% | 10-20% |
| **Atualizações** | Redownload tudo | Apenas app + requisitos |

---

## 📋 Plano de Implementação

### Fase 1: Preparação (1-2 horas)

#### T1.1 - Analisar Distribuição Atual
- [ ] Listar tudo incluído no executável atual (39 MB)
- [ ] Identificar módulos dispensáveis
- [ ] Calcular tamanho de cada dependência
- [ ] Documentar problemas específicos de memória

**Comando para análise**:
```bash
pyinstaller --analyze build.spec
```

#### T1.2 - Escolher Ferramenta MSI
**Opções**:
1. **WiX Toolset** (gratuito, nativo Windows, recomendado)
   - Controle total sobre instalação
   - Pode detectar Python instalado
   - Suporte a rollback automático
   - Integra com Windows Add/Remove Programs

2. **NSIS** (simples, leve)
   - Mais rápido para aprender
   - Menor footprint
   - Menos configuração

3. **Inno Setup** (intermediário)
   - UI amigável
   - Bom para usuários finais
   - Estrutura clara

**Recomendação**: WiX Toolset (produção-ready, mais poderoso)

#### T1.3 - Redesenhar build.spec
```python
# Otimizações:
# 1. Remover venv completamente
# 2. Usar system Python (instalado via MSI)
# 3. Excluir mais módulos desnecessários
# 4. Compilar Python para bytecode otimizado
# 5. Usar "onedir" em vez de "onefile" (mais rápido)
```

---

### Fase 2: Implementação (4-6 horas)

#### T2.1 - Criar build.spec Otimizado para Windows
**Mudanças**:
```python
a = Analysis(
    ['backend/launcher.py'],
    pathex=[str(project_root)],

    # Use system Python - não bundle venv
    # (Será instalado via MSI)

    # Excluir MUITO MAIS
    excludes=[
        'pytest', 'pytest-cov', 'pytest-asyncio',
        'numpy', 'scipy', 'pandas', 'matplotlib',
        'PIL', 'cv2', 'sklearn',
        'jupyter', 'ipython',
        'email', 'http.server',  # Stdlib não usada
        'xml.etree', 'urllib3',  # Substituído por requests
    ],

    # Limpar hidden imports
    hiddenimports=[
        'uvicorn', 'fastapi', 'pydantic',
        'pymongo', 'pdfplumber', 'instructor',
    ],
)

# Usar "onedir" em vez de "onefile"
exe = EXE(
    ...,
    name='etnopapers',
    # IMPORTANTE: NÃO usar "onefile"
    # onefile=False permite acesso mais rápido a arquivos
)
```

**Resultado esperado**: ~15-20 MB

#### T2.2 - Criar Configuração WiX (Product.wxs)
**Estrutura WiX**:
```xml
<?xml version="1.0" encoding="UTF-8"?>
<Wix xmlns="http://schemas.microsoft.com/wix/2006/wi">
    <Product Id="*" Name="Etnopapers" Language="1033"
             Version="2.1.0.0" UpgradeCode="...">

        <Package InstallerVersion="200" Compressed="yes" />

        <!-- Python Runtime (se não instalado) -->
        <Feature Id="PythonRuntime" AllowAbsent="no">
            <!-- Detectar Python 3.11+ -->
            <!-- Se não existir, oferecer download -->
        </Feature>

        <!-- Aplicação -->
        <Feature Id="Application" AllowAbsent="no">
            <!-- Arquivos: etnopapers.exe, backend/, frontend/ -->
            <!-- Atalhos no Menu Iniciar -->
            <!-- Desinstalação limpa -->
        </Feature>

        <!-- Ollama (informação) -->
        <Feature Id="OllamaInfo">
            <!-- Popup: "Ollama não detectado, baixar de ollama.com" -->
        </Feature>
    </Product>
</Wix>
```

#### T2.3 - Implementar Detecção Python
```python
# backend/checks/python_check.py
def check_python_installation():
    """Verifica se Python 3.11+ está instalado no PATH"""
    try:
        result = subprocess.run(
            ['python', '--version'],
            capture_output=True,
            text=True
        )
        version = result.stdout.strip()
        if '3.11' in version or '3.12' in version:
            return True
    except:
        pass
    return False

# Se Python não encontrado, mostrar dialog:
if not check_python_installation():
    show_error_dialog(
        "Python 3.11+ não encontrado",
        "Baixe em: https://www.python.org/downloads/",
        "Ou reinstale o Etnopapers com opção 'Install Python'"
    )
```

#### T2.4 - Criar requirements-windows.txt Otimizado
```
# Apenas as dependências de PRODUÇÃO
fastapi==0.110.0
uvicorn[standard]==0.29.0
pydantic==2.8.0
pymongo==4.5.0
pdfplumber==0.10.3
instructor==1.3.7
requests==2.31.0
python-dotenv==1.0.0
aiofiles==23.2.1  # Para servir arquivos estáticos

# NÃO incluir:
# - pytest (testes, não necessário em produção)
# - pyinstaller (já usado no build)
# - openai (instructor não precisa em produção)
```

#### T2.5 - Criar Script Windows de Instalação (PowerShell)
```powershell
# install.ps1
# 1. Verificar Python
# 2. Criar venv local
# 3. pip install -r requirements-windows.txt
# 4. Copiar frontend/dist
# 5. Registrar atalhos
# 6. Mostrar sucesso
```

#### T2.6 - Criar Launcher Aprimorado
```python
# backend/launcher_v2.py
import sys
import subprocess

def ensure_dependencies():
    """Garante que dependências estão instaladas"""
    try:
        import fastapi
        import uvicorn
    except ImportError:
        show_dialog(
            "Dependências não encontradas",
            "Execute: pip install -r requirements.txt"
        )
        sys.exit(1)

def check_ollama():
    """Verifica se Ollama está disponível"""
    try:
        requests.get('http://localhost:11434/api/tags', timeout=2)
    except:
        show_warning(
            "Ollama não detectado",
            "Baixe em: https://ollama.com/download"
        )

# ... resto do código
```

---

### Fase 3: Automação (2-3 horas)

#### T3.1 - Atualizar build-windows.bat
```batch
@echo off
echo Building Etnopapers v2.1 for Windows...

REM 1. Build frontend
cd frontend
call npm run build
cd ..

REM 2. Build com spec otimizado
pyinstaller build.spec

REM 3. Compilar WiX
cd installer\wix
candle.exe Product.wxs
light.exe Product.wixobj -out ..\..\\dist\Etnopapers-Setup-v2.1.msi

REM 4. Resultat
echo.
echo Done!
echo Output:
echo - dist\etnopapers.exe (aplicação, ~20 MB)
echo - dist\Etnopapers-Setup-v2.1.msi (installer, ~5 MB)
```

#### T3.2 - Criar GitHub Actions Workflow
```yaml
# .github/workflows/build-windows.yml
name: Build Windows Installer

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

      # Instalar WiX
      - run: |
          choco install wixtoolset -y

      # Build
      - run: build-windows.bat

      # Upload
      - uses: actions/upload-artifact@v3
        with:
          name: windows-installer
          path: |
            dist/Etnopapers-Setup-v2.1.msi
            dist/etnopapers.exe
```

---

### Fase 4: Testes e Validação (2-3 horas)

#### T4.1 - Criar Plano de Teste
- [ ] Instalar MSI em máquina Windows limpa
- [ ] Verificar tamanho em disco (esperado: 150-200 MB)
- [ ] Verificar tempo de inicialização (<5 segundos)
- [ ] Verificar uso de RAM inicial (<30%)
- [ ] Testar upload de PDF
- [ ] Testar integração com Ollama
- [ ] Testar desinstalação completa
- [ ] Verificar que nenhum arquivo residual fica em disco

#### T4.2 - Documento de Troubleshooting
```markdown
# Troubleshooting Windows

## "Erro ao carregar aplicação"
- Verificar se Python 3.11+ está em PATH
- Executar repair do installer

## "Ollama não encontrado"
- Baixar Ollama de https://ollama.com/download
- Instalar e iniciar antes de usar Etnopapers

## "Aplicação travando"
- Se ainda usar build antigo, desinstalar e reinstalar
- Limpar %APPDATA%\Etnopapers
```

---

## 📦 Estrutura de Distribuição Final

```
Etnopapers v2.1 Release
├── Etnopapers-Setup-v2.1.msi          (Windows Installer - 5-10 MB)
│   ├── Instala Python 3.11 (se necessário)
│   ├── Instala Etnopapers (~80-150 MB em disco)
│   └── Cria atalhos Menu Iniciar
│
├── Etnopapers-v2.1.zip                (Standalone App - 20 MB)
│   ├── etnopapers.exe
│   ├── backend/
│   ├── frontend/
│   └── requirements.txt
│
├── Ollama-Setup.exe                   (Link/Info - não incluído)
│   └── Informação: "Baixar separadamente"
│
└── README-WINDOWS.md                  (Instruções de instalação)
```

---

## 🎯 Métricas de Sucesso

| Métrica | Antes | Depois | Meta |
|---------|-------|--------|------|
| Tamanho Download | 39 MB | 5-10 MB | ✅ -75% |
| Espaço em Disco | 39 MB | 150-200 MB | ✅ Aceitável |
| Tempo Inicialização | 10-30s | 2-5s | ✅ -80% |
| RAM Inicial | 100%+ | 20-30% | ✅ -70% |
| CPU Inicial | 100% | 10-20% | ✅ -80% |
| Taxa Sucesso | 10% | >95% | ✅ Meta |

---

## 📅 Timeline Estimada

| Fase | Duração | Total |
|------|---------|-------|
| T1 - Preparação | 1-2h | 1-2h |
| T2 - Implementação | 4-6h | 5-8h |
| T3 - Automação | 2-3h | 7-11h |
| T4 - Testes | 2-3h | 9-14h |
| **TOTAL** | | **10-16 horas** |

---

## 🔧 Recursos Necessários

### Ferramentas
- [ ] WiX Toolset 3.14+ (gratuito)
- [ ] PyInstaller 6.14+ (já instalado)
- [ ] Node.js 18+ (já instalado)
- [ ] Python 3.11+ (já instalado)

### Documentação
- [ ] Atualizar GUIA_DESENVOLVEDOR.md
- [ ] Criar WINDOWS_BUILD_GUIDE.md
- [ ] Criar INSTALLATION_GUIDE.md (português)

### Testes
- [ ] Máquina Windows 10/11 limpa
- [ ] Máquina com pouca RAM (2-4 GB)
- [ ] Máquina com internet lenta

---

## ⚠️ Considerações Importantes

### 1. Python Runtime
- **Opção A**: MSI detecta Python instalado globalmente
- **Opção B**: MSI empacota Python portável (vantajoso)
- **Opção C**: Usuário instalará Python antes (desvantajoso)
- **Recomendação**: Opção B (mais amigável)

### 2. Atualizações Futuras
```
v2.1.0 → v2.1.1 (bug fix):
  - Apenas app.exe (~10 MB)
  - Reutiliza Python/dependências instaladas

v2.1 → v3.0 (major):
  - Novo installer full (~15 MB)
  - Upgrade automático suportado via WiX
```

### 3. Assinatura Digital
```
Para distribuição profissional:
- Assinar MSI com certificado de código
- Assinar etnopapers.exe
- Evitar "Windows protected your PC" warnings
```

---

## 📝 Tarefas Imediatas (Next Steps)

1. **Aprovação do Plano** ✅ (Aguardando confirmação)
2. **T1.1**: Análise do executável atual
3. **T1.2**: Escolher WiX Toolset
4. **T1.3**: Começar build.spec otimizado
5. **T2.1**: Implementar novo build.spec

---

**Status**: 📋 Planejamento Completo
**Próximo**: Aguardando aprovação para iniciar implementação
