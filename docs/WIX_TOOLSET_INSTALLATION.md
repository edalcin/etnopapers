# WiX Toolset Installation Guide - Etnopapers v2.1

**Date**: 2025-11-30
**Instructions**: For Windows machine with PowerShell
**Status**: 📋 Ready for Execution

---

## 🎯 OBJECTIVE

Install WiX Toolset v3.14+ on Windows to create professional MSI Installer for Etnopapers.

---

## 📋 PREREQUISITES

- ✅ Windows 10 or superior
- ✅ PowerShell 5.0+ (run as Administrator)
- ✅ Chocolatey installed (or permission to install)
- ✅ 500 MB disk space
- ✅ Internet connection

---

## 🔧 OPTION 1: INSTALLATION VIA CHOCOLATEY (Recommended)

### Step 1: Open PowerShell as Administrator

```powershell
# Right-click PowerShell → "Run as administrator"
# Verify you have admin privileges:
Write-Host "Admin: $(([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] 'Administrator'))"
# Should return: Admin: True
```

### Step 2: Verify Chocolatey

```powershell
# Check if Chocolatey is installed
choco --version

# If not installed, install Chocolatey first
# (Copy and execute as a SINGLE line)
Set-ExecutionPolicy Bypass -Scope Process -Force; [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072; iex ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))
```

### Step 3: Install WiX Toolset

```powershell
# Install stable version (v3.14)
choco install wixtoolset -y

# Wait 2-3 minutes to complete
# You will see progress messages:
# Installing chocolatey package: wixtoolset...
# The install of wixtoolset was successful
```

### Step 4: Verify Installation

```powershell
# Close and reopen PowerShell (important!)
# Then execute:
wix --version

# Expected output:
# wix version 3.14.1.5722 (or similar)
```

---

## 🔧 OPTION 2: MANUAL INSTALLATION (Without Chocolatey)

### Step 1: Download

Visit one of these URLs:
- **Stable (v3.11)**: https://github.com/wixtoolset/wix3/releases/download/wix3111rtm/wix311.exe
- **Version 3.14**: https://github.com/wixtoolset/wix3/releases
- **New WiX v4**: https://wixtoolset.org/downloads/

### Step 2: Run Installer

```powershell
# Open PowerShell as Administrator
# Locate downloaded file (ex: Downloads\wix311.exe)
# Execute:
Start-Process "C:\Users\YOUR_USER\Downloads\wix311.exe"

# Follow installation wizard (click "Next" multiple times)
# Default location: C:\Program Files (x86)\WiX Toolset v3.11\
```

### Step 3: Add to PATH

```powershell
# If PATH was not added automatically:

# 1. Open Environment Variables
#    Windows + R → "sysdm.cpl" → Tab "Advanced" → Button "Environment Variables"

# 2. In "User variables" or "System variables", find "Path"

# 3. Click "Edit" → "New" → Add:
#    C:\Program Files (x86)\WiX Toolset v3.11\bin
#    (or the version you installed)

# 4. Click OK

# 5. Close and reopen PowerShell

# 6. Verify:
wix --version
```

---

## 🔧 OPTION 3: NUGET INSTALLATION (For CI/CD)

For use in GitHub Actions or CI/CD:

```powershell
# Install NuGet CLI
nuget install WiX -Version 3.14.0

# Or add to project:
# In .csproj or packages.config file:
# <package id="WiX" version="3.14.0" />
```

---

## ✅ VERIFY INSTALLATION

### Simple Test

```powershell
# 1. Check version
wix --version
# Should return something like: wix version 3.14.1.5722

# 2. Check candle tool (compiler)
candle -?
# Should display compiler help

# 3. Check light tool (linker)
light -?
# Should display linker help

# Result expected: All three above should work
```

### Complete Test (Compile Example)

```powershell
# Create test directory
mkdir C:\temp\wix-test
cd C:\temp\wix-test

# Create simple Product.wxs file
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

# Compile
candle Product.wxs -o obj\
light obj\Product.wixobj -o test.msi

# If successful, test.msi will be created in C:\temp\wix-test
dir *.msi
```

---

## 🐛 TROUBLESHOOTING

### Problem 1: "wix: command not found"

**Solution**:
```powershell
# 1. Close and reopen PowerShell as Administrator
# 2. Check if it was installed:
$env:Path -split ';' | Select-String 'WiX'

# 3. If not found, add manually to PATH:
[Environment]::SetEnvironmentVariable(
  'Path',
  [Environment]::GetEnvironmentVariable('Path', 'Machine') + ';C:\Program Files (x86)\WiX Toolset v3.11\bin',
  'Machine'
)

# 4. Restart the computer (important!)
```

### Problem 2: "Access Denied" during installation

**Solution**:
```powershell
# Open PowerShell TRULY as Administrator:
# 1. Windows + X
# 2. Click "Windows PowerShell (Admin)" (not "Windows Terminal")
# 3. If prompted for confirmation, click "Yes"

# Verify privileges:
([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] 'Administrator')
# Should return: True
```

### Problem 3: Error "light.exe not found" during compilation

**Solution**:
```powershell
# PATH was not added. Options:

# A) Use full path:
& 'C:\Program Files (x86)\WiX Toolset v3.11\bin\candle.exe' Product.wxs
& 'C:\Program Files (x86)\WiX Toolset v3.11\bin\light.exe' Product.wixobj -o app.msi

# B) Add to PATH (see "Problem 1")

# C) Use build.bat which sets PATH automatically
```

### Problem 4: Chocolatey not found

**Solution**:
```powershell
# 1. If Chocolatey is not installed, install it:
Set-ExecutionPolicy Bypass -Scope Process -Force; [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072; iex ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))

# 2. Close and reopen PowerShell

# 3. Verify:
choco --version

# 4. If still not working, use Option 2 (manual installation)
```

---

## 📋 INSTALLATION CHECKLIST

- [ ] PowerShell opened as Administrator
- [ ] Chocolatey installed (verify with `choco --version`)
- [ ] WiX Toolset installed (via `choco install wixtoolset -y`)
- [ ] PowerShell closed and reopened
- [ ] `wix --version` returns version (ex: 3.14.1.5722)
- [ ] `candle -?` displays help
- [ ] `light -?` displays help
- [ ] Compilation test successful (creates .msi)

---

## 🎉 NEXT STEP

After successful installation:

1. ✅ Phase 1 Task T1.2 (Decide tool = WiX selected)
2. ✅ Phase 1 Task T1.3 (Prepare structure)

Structure to create in `etnopapers/installer/`:
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

## 📚 REFERENCES

- **WiX v3 Documentation**: https://wixtoolset.org/documentation/
- **WiX Tutorials**: https://wixtoolset.org/documentation/manual/v3/
- **GitHub Releases**: https://github.com/wixtoolset/wix3/releases
- **WiX Community**: https://stackoverflow.com/questions/tagged/wix

---

**Status**: 📋 Ready for Installation
**Next**: Execute in Windows machine
**Estimated Time**: 5-10 minutes (installation + verification)
