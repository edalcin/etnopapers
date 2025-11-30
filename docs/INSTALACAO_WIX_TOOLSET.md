# Instalação WiX Toolset - Etnopapers v2.1

**Data**: 2025-11-30
**Instruções**: Para máquina Windows com PowerShell
**Status**: 📋 Pronto para Execução

---

## 🎯 OBJETIVO

Instalar WiX Toolset v3.14+ no Windows para criar MSI Installer profissional para Etnopapers.

---

## 📋 PRÉ-REQUISITOS

- ✅ Windows 10 ou superior
- ✅ PowerShell 5.0+ (abra como Administrador)
- ✅ Chocolatey instalado (ou permissão para instalar)
- ✅ 500 MB espaço em disco
- ✅ Conexão de internet

---

## 🔧 OPÇÃO 1: INSTALAÇÃO VIA CHOCOLATEY (Recomendado)

### Passo 1: Abrir PowerShell como Administrador

```powershell
# Clique com botão direito em PowerShell → "Executar como administrador"
# Verifique se tem privilégios:
Write-Host "Admin: $(([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] 'Administrator'))"
# Deve retornar: Admin: True
```

### Passo 2: Verificar Chocolatey

```powershell
# Verificar se Chocolatey está instalado
choco --version

# Se não estiver, instalar Chocolatey primeiro
# (Copie e execute como uma ÚNICA linha)
Set-ExecutionPolicy Bypass -Scope Process -Force; [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072; iex ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))
```

### Passo 3: Instalar WiX Toolset

```powershell
# Instalar versão estável (v3.14)
choco install wixtoolset -y

# Aguarde 2-3 minutos para completar
# Você verá mensagens de progresso:
# Installing chocolatey package: wixtoolset...
# The install of wixtoolset was successful
```

### Passo 4: Verificar Instalação

```powershell
# Feche e reabra o PowerShell (importante!)
# Depois execute:
wix --version

# Saída esperada:
# wix version 3.14.1.5722 (ou similar)
```

---

## 🔧 OPÇÃO 2: INSTALAÇÃO MANUAL (Sem Chocolatey)

### Passo 1: Download

Acesse uma das URLs:
- **Estável (v3.11)**: https://github.com/wixtoolset/wix3/releases/download/wix3111rtm/wix311.exe
- **Versão 3.14**: https://github.com/wixtoolset/wix3/releases
- **Novo WiX v4**: https://wixtoolset.org/downloads/

### Passo 2: Executar Instalador

```powershell
# Abrir PowerShell como Administrador
# Localizar arquivo baixado (ex: Downloads\wix311.exe)
# Executar:
Start-Process "C:\Users\SEU_USUARIO\Downloads\wix311.exe"

# Seguir wizard de instalação (clique "Next" várias vezes)
# Local padrão: C:\Program Files (x86)\WiX Toolset v3.11\
```

### Passo 3: Adicionar ao PATH

```powershell
# Se PATH não foi adicionado automaticamente:

# 1. Abrir Variáveis de Ambiente
#    Windows + R → "sysdm.cpl" → Aba "Advanced" → Botão "Environment Variables"

# 2. Em "User variables" ou "System variables", encontrar "Path"

# 3. Clicar "Edit" → "New" → Adicionar:
#    C:\Program Files (x86)\WiX Toolset v3.11\bin
#    (ou versão que instalou)

# 4. Clicar OK

# 5. Fechar e reabrir PowerShell

# 6. Verificar:
wix --version
```

---

## 🔧 OPÇÃO 3: INSTALAÇÃO VIA NuGet (Para CI/CD)

Se deseja usar em GitHub Actions ou CI/CD:

```powershell
# Instalar NuGet CLI
nuget install WiX -Version 3.14.0

# Ou adicionar ao projeto:
# No arquivo .csproj ou packages.config:
# <package id="WiX" version="3.14.0" />
```

---

## ✅ VERIFICAR INSTALAÇÃO

### Teste Simples

```powershell
# 1. Verificar versão
wix --version
# Deve retornar algo como: wix version 3.14.1.5722

# 2. Verificar ferramenta candle (compiler)
candle -?
# Deve exibir ajuda do compilador

# 3. Verificar ferramenta light (linker)
light -?
# Deve exibir ajuda do linker

# Resultado esperado: Todas as três acima devem funcionar
```

### Teste Completo (Compilar Exemplo)

```powershell
# Criar diretório de teste
mkdir C:\temp\wix-test
cd C:\temp\wix-test

# Criar arquivo Product.wxs simples
@"
<?xml version="1.0" encoding="UTF-8"?>
<Wix xmlns="http://schemas.microsoft.com/wix/2006/wi">
  <Product
    Id="*"
    Name="Test"
    Language="1033"
    Version="1.0.0.0"
    Manufacturer="Test"
    UpgradeCode="12345678-1234-1234-1234-123456789012">
    <Package InstallerVersion="200" Compressed="yes" />
    <Media Id="1" Cabinet="test.cab" EmbedCab="yes" />
  </Product>
</Wix>
"@ | Out-File Product.wxs

# Compilar
candle Product.wxs -o obj\
light obj\Product.wixobj -o test.msi

# Se bem-sucedido, terá test.msi em C:\temp\wix-test
dir *.msi
```

---

## 🐛 SOLUÇÃO DE PROBLEMAS

### Problema 1: "wix: command not found"

**Solução**:
```powershell
# 1. Feche e reabra PowerShell como Administrador
# 2. Verifique se foi instalado:
$env:Path -split ';' | Select-String 'WiX'

# 3. Se não encontrar, adicione manualmente ao PATH:
[Environment]::SetEnvironmentVariable(
  'Path',
  [Environment]::GetEnvironmentVariable('Path', 'Machine') + ';C:\Program Files (x86)\WiX Toolset v3.11\bin',
  'Machine'
)

# 4. Reinicie o computador (importante!)
```

### Problema 2: "Access Denied" ao instalar

**Solução**:
```powershell
# Abrir PowerShell VERDADEIRAMENTE como Administrador:
# 1. Windows + X
# 2. Clicar "Windows PowerShell (Admin)" (não "Windows Terminal")
# 3. Se pedir confirmação, clicar "Sim"

# Verificar privilégios:
([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] 'Administrator')
# Deve retornar: True
```

### Problema 3: Erro "light.exe not found" ao compilar

**Solução**:
```powershell
# PATH não foi adicionado. Opções:

# A) Usar path completo:
& 'C:\Program Files (x86)\WiX Toolset v3.11\bin\candle.exe' Product.wxs
& 'C:\Program Files (x86)\WiX Toolset v3.11\bin\light.exe' Product.wixobj -o app.msi

# B) Adicionar ao PATH (ver "Problema 1")

# C) Usar build.bat que configura PATH automaticamente
```

### Problema 4: Chocolate não encontrado

**Solução**:
```powershell
# 1. Se Chocolatey não está instalado, instale:
Set-ExecutionPolicy Bypass -Scope Process -Force; [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072; iex ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))

# 2. Feche e reabra PowerShell

# 3. Verifique:
choco --version

# 4. Se ainda não funcionar, use Opção 2 (instalação manual)
```

---

## 📋 CHECKLIST DE INSTALAÇÃO

- [ ] PowerShell aberto como Administrador
- [ ] Chocolatey instalado (verificar com `choco --version`)
- [ ] WiX Toolset instalado (via `choco install wixtoolset -y`)
- [ ] PowerShell fechado e reaberto
- [ ] `wix --version` retorna versão (ex: 3.14.1.5722)
- [ ] `candle -?` exibe ajuda
- [ ] `light -?` exibe ajuda
- [ ] Teste de compilação bem-sucedido (cria .msi)

---

## 🎉 PRÓXIMO PASSO

Após instalação bem-sucedida:

1. ✅ T1.2 concluído (Decidir ferramenta = WiX eleito)
2. ✅ T1.3 próximo (Preparar estrutura de diretórios)

Estrutura a criar em `etnopapers/installer/`:
```
installer/
├── wix/
│   ├── Product.wxs
│   ├── Features.wxs
│   ├── UI.wxs
│   └── Etnopapers.wixproj
├── scripts/
│   ├── check-python.ps1
│   ├── post-install.ps1
│   └── pre-install.ps1
└── build-msi.bat
```

---

## 📚 REFERÊNCIAS

- **WiX v3 Documentation**: https://wixtoolset.org/documentation/
- **WiX Tutorials**: https://wixtoolset.org/documentation/manual/v3/
- **GitHub Releases**: https://github.com/wixtoolset/wix3/releases
- **WiX Community**: https://stackoverflow.com/questions/tagged/wix

---

**Status**: 📋 Pronto para Instalação
**Próximo**: Executar T1.2 em máquina Windows
**Tempo Estimado**: 5-10 minutos (instalação + verificação)
