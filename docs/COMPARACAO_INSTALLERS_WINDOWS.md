# Comparação de Ferramentas de Installer Windows

**Data**: 2025-11-30
**Status**: ✅ Análise Completa
**Decisão**: WiX Toolset Eleito

---

## 🎯 MATRIZ DE COMPARAÇÃO

### 1. WiX Toolset (RECOMENDADO ✅)

**Características**:
- Formato: MSI (Microsoft Installer)
- Licença: Open Source (WiX v3 + WiX v4)
- Linguagem: XML (Product.wxs, Features.wxs)
- Curva aprendizado: Média (1-2 dias)

**Vantagens**:
- ✅ Padrão oficial Windows (Add/Remove Programs)
- ✅ Controle total sobre instalação
- ✅ Detecção automática de Python pré-instalado
- ✅ Suporte a rollback/uninstall completo
- ✅ GitHub Actions integrado (WiX extension)
- ✅ Upgrade automático (versionamento MSI)
- ✅ Assinatura digital (code signing opcional)
- ✅ Requisitos de sistema (detecção SO)
- ✅ Suporte profissional & comunidade grande
- ✅ Gratuito, sem limitações

**Desvantagens**:
- ❌ XML pode ser verboso (300+ linhas para app simples)
- ❌ Curva aprendizado maior que NSIS
- ❌ Requer compilação (mais lento que NSIS)
- ❌ Windows apenas (não cross-platform)

**Tamanho Installer**: 5-10 MB
**Tempo Build**: 10-20 segundos
**Experiência Usuário**: Profissional
**Manutenção**: Fácil (versionamento automático)

**Exemplo (Product.wxs)**:
```xml
<?xml version="1.0" encoding="UTF-8"?>
<Wix xmlns="http://schemas.microsoft.com/wix/2006/wi">
  <Product
    Id="*"
    Name="Etnopapers v2.1"
    Language="1033"
    Version="2.1.0.0"
    Manufacturer="Etnopapers"
    UpgradeCode="12345678-1234-1234-1234-123456789012">

    <Package
      InstallerVersion="200"
      Compressed="yes"
      InstallScope="perMachine" />

    <Media Id="1" Cabinet="Etnopapers.cab" EmbedCab="yes" />

    <!-- Detectar Python 3.11+ -->
    <Property Id="PYTHONINSTALLED" Value="0" />
    <Property Id="PYTHONVERSION" Value="" />

    <Condition Message="Python 3.11+ requerido. Acesse https://python.org">
      <![CDATA[PYTHONINSTALLED = 1]]>
    </Condition>

    <!-- Feature principal -->
    <Feature Id="ProductFeature" Title="Etnopapers" Level="1">
      <ComponentRef Id="MainExecutable" />
      <ComponentRef Id="AppShortcut" />
      <ComponentRef Id="StartMenuShortcut" />
    </Feature>

    <!-- UI customizada -->
    <UIRef Id="WixUI_InstallDir" />
    <Property Id="WIXUI_INSTALLDIR" Value="INSTALLFOLDER" />
  </Product>
</Wix>
```

---

### 2. NSIS (Alternativa Simples)

**Características**:
- Formato: EXE (executável stand-alone)
- Licença: Open Source
- Linguagem: Script (MakeNSIS)
- Curva aprendizado: Fácil (poucas horas)

**Vantagens**:
- ✅ Muito simples de usar
- ✅ Curva aprendizado baixa
- ✅ Suporte a plugins extensível
- ✅ Comunidade grande
- ✅ Compile rápido (<5 seg)
- ✅ Output EXE pequeno (3-5 MB)

**Desvantagens**:
- ❌ Menos profissional que MSI
- ❌ Não integra em "Add/Remove Programs" bem
- ❌ Sem suporte oficial rollback
- ❌ Upgrade requer desinstalar + reinstalar
- ❌ Sem versionamento automático
- ❌ Menos corporativo (maior risco de bloqueio por antivírus)

**Tamanho Installer**: 3-5 MB
**Tempo Build**: 2-5 segundos
**Experiência Usuário**: Semi-profissional
**Manutenção**: Média (upgrade manual)

**Exemplo (install.nsi)**:
```nsis
; Etnopapers Installer
Name "Etnopapers v2.1"
OutFile "Etnopapers-Setup-v2.1.exe"
InstallDir "$PROGRAMFILES\Etnopapers"

; Páginas
Page directory
Page instfiles
UninstPage uninstConfirm
UninstPage instfiles

; Secção de instalação
Section "Instalar Etnopapers"
  SetOutPath "$INSTDIR"
  File "dist\etnopapers.exe"
  File "launcher.py"

  ; Criar shortcuts
  CreateDirectory "$SMPROGRAMS\Etnopapers"
  CreateShortcut "$SMPROGRAMS\Etnopapers\Etnopapers.lnk" "$INSTDIR\etnopapers.exe"
  CreateShortcut "$DESKTOP\Etnopapers.lnk" "$INSTDIR\etnopapers.exe"

  ; Registry
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Etnopapers" \
    "DisplayName" "Etnopapers v2.1"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Etnopapers" \
    "UninstallString" "$INSTDIR\uninstall.exe"
SectionEnd
```

---

### 3. Inno Setup (Meio Termo)

**Características**:
- Formato: EXE (executável)
- Licença: Open Source (Pascal Script)
- Linguagem: INI-like + Pascal scripting
- Curva aprendizado: Fácil-Média (1 dia)

**Vantagens**:
- ✅ Interface visual (wizard)
- ✅ Script poderoso (Pascal)
- ✅ Bom balanço simplicidade/funcionalidade
- ✅ Suporte a plugins
- ✅ Comunidade ativa
- ✅ Output pequeno (5-7 MB)
- ✅ Integra moderadamente em Add/Remove Programs

**Desvantagens**:
- ❌ Menos profissional que MSI
- ❌ Upgrade requer desinstalar + reinstalar
- ❌ Sem versionamento automático
- ❌ Windows apenas

**Tamanho Installer**: 5-7 MB
**Tempo Build**: 5-10 segundos
**Experiência Usuário**: Bom meio termo
**Manutenção**: Fácil

**Exemplo (Etnopapers.iss)**:
```ini
[Setup]
AppName=Etnopapers
AppVersion=2.1.0
DefaultDirName={pf}\Etnopapers
DefaultGroupName=Etnopapers
OutputDir=dist\
OutputBaseFilename=Etnopapers-Setup-v2.1

[Files]
Source: "dist\etnopapers.exe"; DestDir: "{app}"
Source: "backend\launcher.py"; DestDir: "{app}"

[Icons]
Name: "{group}\Etnopapers"; Filename: "{app}\etnopapers.exe"
Name: "{desktop}\Etnopapers"; Filename: "{app}\etnopapers.exe"

[Code]
function InitializeSetup(): Boolean;
begin
  if not RegKeyExists(HKLM, 'Software\Python\PythonCore\3.11') then begin
    MsgBox('Python 3.11 é necessário. Acesse https://python.org', mbError, MB_OK);
    Result := False;
  end;
  Result := True;
end;
```

---

## 📊 TABELA COMPARATIVA (Resumida)

| Critério | WiX | NSIS | Inno Setup |
|----------|-----|------|-----------|
| **Profissionalismo** | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐⭐ |
| **Curva Aprendizado** | ⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐ |
| **Controle Total** | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐⭐ |
| **Tamanho Output** | 5-10 MB | 3-5 MB | 5-7 MB |
| **Velocidade Build** | 🟡 10-20s | ✅ 2-5s | ✅ 5-10s |
| **Upgrade Automático** | ✅ Nativo | ❌ Manual | ❌ Manual |
| **Add/Remove Programs** | ✅ Nativo | 🟡 Parcial | ✅ Bom |
| **Rollback** | ✅ Nativo | ❌ Não | ❌ Não |
| **Code Signing** | ✅ Fácil | 🟡 Possível | 🟡 Possível |
| **GitHub Actions** | ✅ WiX Tools | ❌ Manual | ❌ Manual |
| **Comunidade** | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐⭐ |
| **Manutenção LT** | ✅ Fácil | 🟡 Média | ✅ Fácil |
| **Enterprise Ready** | ⭐⭐⭐⭐⭐ | ⭐⭐ | ⭐⭐⭐ |

---

## 🎯 RECOMENDAÇÃO: WiX TOOLSET ✅

### Por quê?

1. **Profissionalismo**: Padrão de facto para aplicações Windows profissionais
   - Microsoft usa WiX para seus próprios produtos
   - Grandes empresas (Adobe, Intel, etc.) usam WiX

2. **Recursos Nativos**: Não simula, implementa nativa:
   - Add/Remove Programs integrado
   - Rollback em caso de falha
   - Upgrade sem desinstalação
   - Versionamento automático

3. **GitHub Actions**: Extensão oficial do WiX para CI/CD
   ```yaml
   - name: Build MSI
     run: wix build installer/wix/ -o dist/Etnopapers-v2.1.msi
   ```

4. **Longo Prazo**: Melhor investimento de tempo
   - Uma vez aprendido, reutilizável para outros projetos
   - Mantido ativamente (WiX v4 lançado em 2023)
   - Comunidade crescente

### Quando usar NSIS ou Inno Setup?

- **NSIS**: Se máxima simplicidade é prioridade (3-5 MB installer)
- **Inno Setup**: Se quer meio termo (fácil + profissional)
- **WiX**: Se quer o melhor (recomendado) ✅

---

## 🔧 INSTALAÇÃO WIX TOOLSET

### Opção 1: Chocolatey (Recomendado no Windows)

```powershell
# PowerShell como administrador
choco install wixtoolset -y

# Verificar instalação
wix --version
# Output esperado: wix version X.Y.Z
```

### Opção 2: Download Manual

```powershell
# Baixar de: https://github.com/wixtoolset/wix3/releases
# ou: https://wixtoolset.org/downloads/

# Arquivo: wix311.exe (WiX v3.11 - estável)
# ou: wix-vX.Y.Z-windows.zip (WiX v4 - novo)

# Instalar e adicionar ao PATH
# C:\Program Files (x86)\WiX Toolset vX\bin
```

### Opção 3: NuGet (Para CI/CD)

```xml
<!-- packages.config -->
<packages>
  <package id="WiX" version="3.14.0" />
</packages>
```

---

## 📋 PRÓXIMO PASSO: T1.3 - Preparar Estrutura

Após decisão de usar WiX, criar:

```
etnopapers/
├── installer/
│   ├── wix/
│   │   ├── Product.wxs           # Definição MSI
│   │   ├── Features.wxs          # Recursos
│   │   ├── UI.wxs                # Interface
│   │   └── Etnopapers.wixproj    # Projeto WiX
│   │
│   ├── scripts/
│   │   ├── check-python.ps1      # Detecta Python
│   │   ├── post-install.ps1      # Configuração pós-instalação
│   │   └── pre-install.ps1       # Checagens pré-instalação
│   │
│   └── build-msi.bat              # Script de build
│
├── build.spec.optimized           # PyInstaller onedir
├── requirements-windows.txt       # Deps produção apenas
└── build-windows.bat              # Build completo
```

---

## ✅ CONCLUSÃO

**WiX Toolset é a escolha certa para Etnopapers v2.1**

Características finais esperadas:
- ✅ Installer MSI 5-10 MB
- ✅ Detecta Python automaticamente
- ✅ Instala app em `C:\Program Files\Etnopapers`
- ✅ Integra em Add/Remove Programs
- ✅ Upgrade sem desinstalação
- ✅ Suporta rollback em caso de falha
- ✅ Suporta code signing
- ✅ Build automatizado via GitHub Actions

---

**Status**: ✅ Decisão Tomada - WiX Toolset
**Próximo**: Instalar WiX e começar Fase 2 (Implementação)
