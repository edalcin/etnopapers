# Windows Installer Tools Comparison

**Date**: 2025-11-30
**Status**: ✅ Complete Analysis
**Decision**: WiX Toolset Selected

---

## 🎯 COMPARISON MATRIX

### 1. WiX Toolset (RECOMMENDED ✅)

**Characteristics**:
- Format: MSI (Microsoft Installer)
- License: Open Source (WiX v3 + WiX v4)
- Language: XML (Product.wxs, Features.wxs)
- Learning curve: Medium (1-2 days)

**Advantages**:
- ✅ Windows official standard (Add/Remove Programs)
- ✅ Complete installation control
- ✅ Automatic Python pre-installation detection
- ✅ Full rollback/uninstall support
- ✅ GitHub Actions integrated (WiX extension)
- ✅ Automatic upgrade (MSI versioning)
- ✅ Digital signature (optional code signing)
- ✅ System requirements detection (OS detection)
- ✅ Professional support & large community
- ✅ Free, no limitations

**Disadvantages**:
- ❌ XML can be verbose (300+ lines for simple app)
- ❌ Steeper learning curve than NSIS
- ❌ Requires compilation (slower than NSIS)
- ❌ Windows only (not cross-platform)

**Installer Size**: 5-10 MB
**Build Time**: 10-20 seconds
**User Experience**: Professional
**Maintenance**: Easy (automatic versioning)

**Example (Product.wxs)**:
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

    <!-- Detect Python 3.11+ -->
    <Property Id="PYTHONINSTALLED" Value="0" />
    <Property Id="PYTHONVERSION" Value="" />

    <Condition Message="Python 3.11+ required. Visit https://python.org">
      <![CDATA[PYTHONINSTALLED = 1]]>
    </Condition>

    <!-- Main feature -->
    <Feature Id="ProductFeature" Title="Etnopapers" Level="1">
      <ComponentRef Id="MainExecutable" />
      <ComponentRef Id="AppShortcut" />
      <ComponentRef Id="StartMenuShortcut" />
    </Feature>

    <!-- Custom UI -->
    <UIRef Id="WixUI_InstallDir" />
    <Property Id="WIXUI_INSTALLDIR" Value="INSTALLFOLDER" />
  </Product>
</Wix>
```

---

### 2. NSIS (Simple Alternative)

**Characteristics**:
- Format: EXE (stand-alone executable)
- License: Open Source
- Language: Script (MakeNSIS)
- Learning curve: Easy (few hours)

**Advantages**:
- ✅ Very simple to use
- ✅ Low learning curve
- ✅ Extensible plugin support
- ✅ Large community
- ✅ Fast compilation (<5 sec)
- ✅ Small EXE output (3-5 MB)

**Disadvantages**:
- ❌ Less professional than MSI
- ❌ Poor "Add/Remove Programs" integration
- ❌ No official rollback support
- ❌ Upgrade requires uninstall + reinstall
- ❌ No automatic versioning
- ❌ Less corporate (higher antivirus block risk)

**Installer Size**: 3-5 MB
**Build Time**: 2-5 seconds
**User Experience**: Semi-professional
**Maintenance**: Medium (manual upgrade)

**Example (install.nsi)**:
```nsis
; Etnopapers Installer
Name "Etnopapers v2.1"
OutFile "Etnopapers-Setup-v2.1.exe"
InstallDir "$PROGRAMFILES\Etnopapers"

; Pages
Page directory
Page instfiles
UninstPage uninstConfirm
UninstPage instfiles

; Installation section
Section "Install Etnopapers"
  SetOutPath "$INSTDIR"
  File "dist\etnopapers.exe"
  File "launcher.py"

  ; Create shortcuts
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

### 3. Inno Setup (Middle Ground)

**Characteristics**:
- Format: EXE (executable)
- License: Open Source (Pascal Script)
- Language: INI-like + Pascal scripting
- Learning curve: Easy-Medium (1 day)

**Advantages**:
- ✅ Visual interface (wizard)
- ✅ Powerful scripting (Pascal)
- ✅ Good balance simplicity/functionality
- ✅ Plugin support
- ✅ Active community
- ✅ Small output (5-7 MB)
- ✅ Moderate "Add/Remove Programs" integration

**Disadvantages**:
- ❌ Less professional than MSI
- ❌ Upgrade requires uninstall + reinstall
- ❌ No automatic versioning
- ❌ Windows only

**Installer Size**: 5-7 MB
**Build Time**: 5-10 seconds
**User Experience**: Good compromise
**Maintenance**: Easy

**Example (Etnopapers.iss)**:
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
    MsgBox('Python 3.11 is required. Visit https://python.org', mbError, MB_OK);
    Result := False;
  end;
  Result := True;
end;
```

---

## 📊 COMPARISON TABLE (Summarized)

| Criterion | WiX | NSIS | Inno Setup |
|-----------|-----|------|-----------|
| **Professionalism** | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐⭐ |
| **Learning Curve** | ⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐ |
| **Complete Control** | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐⭐ |
| **Output Size** | 5-10 MB | 3-5 MB | 5-7 MB |
| **Build Speed** | 🟡 10-20s | ✅ 2-5s | ✅ 5-10s |
| **Automatic Upgrade** | ✅ Native | ❌ Manual | ❌ Manual |
| **Add/Remove Programs** | ✅ Native | 🟡 Partial | ✅ Good |
| **Rollback** | ✅ Native | ❌ No | ❌ No |
| **Code Signing** | ✅ Easy | 🟡 Possible | 🟡 Possible |
| **GitHub Actions** | ✅ WiX Tools | ❌ Manual | ❌ Manual |
| **Community** | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐⭐ |
| **Long-term Maintenance** | ✅ Easy | 🟡 Medium | ✅ Easy |
| **Enterprise Ready** | ⭐⭐⭐⭐⭐ | ⭐⭐ | ⭐⭐⭐ |

---

## 🎯 RECOMMENDATION: WIX TOOLSET ✅

### Why?

1. **Professionalism**: De facto standard for professional Windows applications
   - Microsoft uses WiX for its own products
   - Large companies (Adobe, Intel, etc.) use WiX

2. **Native Features**: Not simulated, truly implemented:
   - Add/Remove Programs integrated
   - Rollback on failure
   - Upgrade without uninstallation
   - Automatic versioning

3. **GitHub Actions**: Official WiX extension for CI/CD
   ```yaml
   - name: Build MSI
     run: wix build installer/wix/ -o dist/Etnopapers-v2.1.msi
   ```

4. **Long-term Investment**: Worth the learning time
   - Once learned, reusable for other projects
   - Actively maintained (WiX v4 released 2023)
   - Growing community

### When to use NSIS or Inno Setup?

- **NSIS**: If maximum simplicity is priority (3-5 MB installer)
- **Inno Setup**: If you want middle ground (easy + professional)
- **WiX**: If you want the best (recommended) ✅

---

## 🔧 WIX TOOLSET INSTALLATION

### Option 1: Chocolatey (Recommended on Windows)

```powershell
# PowerShell as administrator
choco install wixtoolset -y

# Verify installation
wix --version
# Expected output: wix version X.Y.Z
```

### Option 2: Manual Download

```powershell
# Download from: https://github.com/wixtoolset/wix3/releases
# or: https://wixtoolset.org/downloads/

# File: wix311.exe (WiX v3.11 - stable)
# or: wix-vX.Y.Z-windows.zip (WiX v4 - new)

# Install and add to PATH
# C:\Program Files (x86)\WiX Toolset vX\bin
```

### Option 3: NuGet (For CI/CD)

```xml
<!-- packages.config -->
<packages>
  <package id="WiX" version="3.14.0" />
</packages>
```

---

## 📋 NEXT STEP: Create Structure

After choosing WiX, create:

```
etnopapers/
├── installer/
│   ├── wix/
│   │   ├── Product.wxs           # MSI definition
│   │   ├── Features.wxs          # Resources
│   │   ├── UI.wxs                # Interface
│   │   └── Etnopapers.wixproj    # WiX project
│   │
│   ├── scripts/
│   │   ├── check-python.ps1      # Detects Python
│   │   ├── post-install.ps1      # Post-installation
│   │   └── pre-install.ps1       # Pre-checks
│   │
│   └── build-msi.bat              # Build script
│
├── build.spec.optimized           # PyInstaller onedir
├── requirements-windows.txt       # Production deps only
└── build-windows.bat              # Master build script
```

---

## ✅ CONCLUSION

**WiX Toolset is the right choice for Etnopapers v2.1**

Expected final characteristics:
- ✅ Installer MSI 5-10 MB
- ✅ Automatically detects Python
- ✅ Installs app in `C:\Program Files\Etnopapers`
- ✅ Integrates with Add/Remove Programs
- ✅ Upgrade without uninstallation
- ✅ Supports rollback on failure
- ✅ Supports code signing
- ✅ Automated build via GitHub Actions

---

**Status**: ✅ Decision Made - WiX Toolset
**Next**: Install WiX and begin Phase 2 (Implementation)
