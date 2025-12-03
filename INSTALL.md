# EtnoPapers Installation Guide

**Version**: 1.0.0
**Updated**: December 2024
**Platform**: Windows 10/11 (x64)

---

## Table of Contents

1. [System Requirements](#system-requirements)
2. [Installation Methods](#installation-methods)
3. [Step-by-Step Installation](#step-by-step-installation)
4. [Configuration](#configuration)
5. [Verification](#verification)
6. [Troubleshooting](#troubleshooting)
7. [Uninstallation](#uninstallation)

---

## System Requirements

### Minimum Requirements

| Component | Requirement |
|-----------|------------|
| OS | Windows 10 (Build 1909) or later |
| Architecture | 64-bit (x64) |
| RAM | 4 GB |
| Disk Space | 500 MB |
| .NET Runtime | 8.0 (included in installer) |
| Administrator Rights | Required for installation |

### Recommended Requirements

| Component | Recommendation |
|-----------|---------------|
| OS | Windows 11 |
| RAM | 8 GB |
| Disk Space | 1 GB (with sample data) |
| Processor | Multi-core modern processor |
| Display | 1920x1080 or higher |

### Optional Components

| Component | Purpose | Installation |
|-----------|---------|--------------|
| OLLAMA | AI-powered metadata extraction | Install separately from [ollama.ai](https://ollama.ai) |
| MongoDB | Remote data synchronization | Local installation or Atlas cloud account |
| PostgreSQL | Future support | Not yet required |

---

## Installation Methods

### Method 1: Windows Installer (Recommended)

**Best For:** Most users, automatic updates, system integration

**File**: `EtnoPapers-Setup-1.0.0.msi`

**Advantages:**
- User-friendly setup wizard
- Automatic .NET 8.0 installation
- System integration (Add/Remove Programs)
- Start Menu shortcuts
- Optional desktop shortcut
- Uninstall support

**Installation Time:** 2-5 minutes

### Method 2: Portable ZIP

**Best For:** USB drives, minimal installations, portable systems

**File**: `EtnoPapers-Portable-1.0.0.zip`

**Advantages:**
- No installation required
- Run directly from extracted folder
- Suitable for portable storage
- No registry modifications
- Easy to clean up

**Installation Time:** 1 minute (extraction only)

---

## Step-by-Step Installation

### Using Windows Installer (MSI)

#### 1. Download and Verify

```bash
# Download EtnoPapers-Setup-1.0.0.msi
# Verify file integrity (optional)
certutil -hashfile EtnoPapers-Setup-1.0.0.msi SHA256
# Compare with published checksum in checksums.txt
```

#### 2. Run the Installer

1. Double-click `EtnoPapers-Setup-1.0.0.msi`
2. If prompted by Windows SmartScreen, click "Run anyway"
3. Wait for installer to initialize (~5 seconds)

#### 3. Welcome Screen

- Click "Next >" to continue
- Review license terms
- Click "I Accept" to proceed

#### 4. Installation Directory

**Default**: `C:\Program Files\EtnoPapers`

To change:
1. Click "Browse..."
2. Select desired directory
3. Ensure at least 500 MB free space
4. Click "Next >"

#### 5. Installation Options

Select optional features:
- [ ] Create Desktop Shortcut (recommended)
- [ ] Associate PDF files with EtnoPapers (optional)
- [ ] Add to Start Menu (selected by default)
- [ ] Launch at Windows startup (optional)

Click "Next >" to continue.

#### 6. Review Summary

- Verify installation path
- Confirm selected options
- Click "Install" to begin

#### 7. Installation Progress

Wait for installation to complete. This may take 2-5 minutes depending on system performance.

#### 8. Completion

- Installation complete message appears
- [ ] Optional: "Launch EtnoPapers now" (checked by default)
- Click "Finish"

#### 9. First Launch

When EtnoPapers launches:
1. Initialize database (30 seconds)
2. Create user documents directory
3. Load default configuration
4. Display main window

---

### Using Portable ZIP

#### 1. Download and Extract

```bash
# Download EtnoPapers-Portable-1.0.0.zip
# Extract to desired location
# Example: D:\Applications\EtnoPapers\

Expand-Archive -Path EtnoPapers-Portable-1.0.0.zip -DestinationPath D:\Applications\
```

#### 2. First-Time Setup

1. Navigate to extracted folder
2. Right-click `EtnoPapers.exe`
3. Select "Run as Administrator" (first time only)
4. Accept UAC prompt if prompted
5. Wait for database initialization

#### 3. Create Shortcuts (Optional)

**Windows Start Menu:**
```bash
$desktop = [Environment]::GetFolderPath('Desktop')
$source = "D:\Applications\EtnoPapers\EtnoPapers.exe"
$shortcut = "$desktop\EtnoPapers.lnk"
$shell = New-Object -ComObject WScript.Shell
$link = $shell.CreateShortCut($shortcut)
$link.TargetPath = $source
$link.Save()
```

---

## Configuration

### Initial Setup (First Launch)

#### 1. Main Window Opens

The application loads with default settings. No configuration required to start using basic features.

#### 2. Configure OLLAMA (For PDF Extraction)

OLLAMA must be running before using PDF extraction features.

**Step 1: Install and Run OLLAMA**

```bash
# Download from https://ollama.ai
# Install OLLAMA
# Run OLLAMA server in PowerShell or Command Prompt:
ollama serve
# OLLAMA will start on http://localhost:11434
```

**Step 2: Download a Model**

```bash
# In another PowerShell/Command Prompt window:
ollama pull llama2
# or for better Portuguese support:
ollama pull llama2-uncensored
# Download may take 5-10 minutes and ~6GB space
```

**Step 3: Configure in EtnoPapers**

1. Open EtnoPapers
2. Click "Configurações" (Settings) in navigation menu
3. Find "OLLAMA Configuration" section
4. Enter:
   - **URL**: `http://localhost:11434`
   - **Model**: `llama2` (or other model name)
   - **Custom Prompt**: Leave default or customize
5. Click "Testar Conexão OLLAMA" to verify
6. Status should show green "Conectado"
7. Click "Salvar Configurações"

#### 3. Configure MongoDB (Optional)

MongoDB is only needed if you want to synchronize records to a cloud/remote database.

**Option A: Local MongoDB**

```bash
# Install MongoDB Community Edition
# https://www.mongodb.com/try/download/community

# Start MongoDB service:
net start MongoDB
# or if not installed as service:
mongod --dbpath "C:\path\to\data"

# In EtnoPapers Settings:
MongoDB URI: mongodb://localhost:27017/etnopayers
```

**Option B: MongoDB Atlas (Cloud)**

```bash
# Create MongoDB Atlas account: https://www.mongodb.com/cloud/atlas

# Create a cluster and database
# Get connection string from Atlas dashboard

# In EtnoPapers Settings:
MongoDB URI: mongodb+srv://username:password@cluster.mongodb.net/etnopayers?retryWrites=true&w=majority
```

**Step: Configure in EtnoPapers**

1. Open EtnoPapers
2. Click "Configurações" (Settings)
3. Find "MongoDB Configuration" section
4. Enter your MongoDB URI
5. Click "Testar Conexão MongoDB"
6. Status should show green "Conectado"
7. Click "Salvar Configurações"

#### 4. Language Preference

1. In Settings, find "Application Settings"
2. Select language:
   - Portuguese (pt-BR) - recommended for Brazilian Portuguese UI
   - English (en-US) - for English interface
3. Click "Salvar Configurações"
4. Restart application for changes to take effect

---

## Verification

### Post-Installation Checklist

After installation, verify everything is working:

- [ ] **Application Launches**
  ```bash
  # From Start Menu: EtnoPapers
  # Should launch within 2 seconds
  ```

- [ ] **Main Window Displays**
  - Home page visible
  - Navigation menu accessible
  - No error messages

- [ ] **Database Initialized**
  - Navigate to "Registros" (Records)
  - Empty record list should display
  - No database errors

- [ ] **Settings Work**
  - Click "Configurações"
  - Load default settings
  - No configuration errors

- [ ] **File Locations Exist**
  ```bash
  # Check data directory created:
  %USERPROFILE%\Documents\EtnoPapers
  # Should contain: data.json, logs folder
  ```

### OLLAMA Verification

If configured:

1. In Settings, click "Testar Conexão OLLAMA"
2. Should show "Conectado" (green indicator)
3. If fails, verify OLLAMA is running:
   ```bash
   # Check if OLLAMA is listening
   netstat -ano | findstr 11434
   ```

### MongoDB Verification

If configured:

1. In Settings, click "Testar Conexão MongoDB"
2. Should show "Conectado" (green indicator)
3. If fails, verify MongoDB URI and server running

---

## Troubleshooting

### Common Installation Issues

#### 1. Windows SmartScreen Warning

**Problem**: "Windows Defender SmartScreen prevented an unrecognized app"

**Solution**:
1. Click "More info"
2. Click "Run anyway"
3. This is normal for new applications

#### 2. Insufficient Disk Space

**Problem**: "Insufficient space for installation"

**Solution**:
1. Free up at least 1 GB of disk space
2. Clear temporary files:
   ```bash
   Disk Cleanup
   # or
   cleanmgr
   ```
3. Retry installation

#### 3. Administrator Rights Required

**Problem**: "Access Denied" during installation

**Solution**:
1. Right-click installer
2. Select "Run as administrator"
3. Accept UAC prompt

#### 4. .NET Runtime Installation Fails

**Problem**: "Failed to install .NET 8.0"

**Solution**:
1. Download .NET 8.0 Desktop Runtime from [microsoft.com](https://dotnet.microsoft.com/download/dotnet/8.0)
2. Install manually before running EtnoPapers installer
3. Restart computer
4. Retry EtnoPapers installation

#### 5. Registry Permission Denied

**Problem**: "Cannot write to registry"

**Solution**:
1. Run installer as administrator
2. Disable antivirus temporarily
3. Check if registry key exists:
   ```bash
   # In Command Prompt as Administrator:
   reg query "HKEY_LOCAL_MACHINE\SOFTWARE\EtnoPapers"
   ```

### Common Runtime Issues

#### Application Won't Start

**Symptoms**: EtnoPapers.exe runs but window doesn't appear

**Solutions**:
1. Check logs:
   ```bash
   # View recent log:
   %USERPROFILE%\Documents\EtnoPapers\logs\
   # Look for error details
   ```
2. Verify .NET Runtime:
   ```bash
   dotnet --version
   # Should show version 8.0.0 or later
   ```
3. Check Windows Event Viewer for crashes

#### Database Initialization Fails

**Symptoms**: "Failed to initialize data storage" error

**Solutions**:
1. Verify Documents folder permissions:
   - Right-click `C:\Users\[User]\Documents`
   - Select Properties > Security
   - Ensure your user has Full Control
2. Manual directory creation:
   ```bash
   mkdir "%USERPROFILE%\Documents\EtnoPapers"
   ```
3. Delete and recreate data.json:
   ```bash
   del "%USERPROFILE%\Documents\EtnoPapers\data.json"
   # Restart EtnoPapers to recreate
   ```

#### OLLAMA Connection Fails

**Symptoms**: "Could not connect to OLLAMA at localhost:11434"

**Solutions**:
1. Verify OLLAMA is running:
   ```bash
   # Open Command Prompt
   ollama serve
   # Should show listening on port 11434
   ```
2. Check firewall:
   ```bash
   # Allow OLLAMA through Windows Firewall
   netsh advfirewall firewall add rule name="OLLAMA" dir=in action=allow program="C:\path\to\ollama.exe"
   ```
3. Verify URL in Settings:
   - Should be exactly: `http://localhost:11434`
   - No trailing slashes
4. Check model exists:
   ```bash
   ollama list
   # Should show downloaded models
   ```

#### MongoDB Connection Fails

**Symptoms**: "Could not connect to MongoDB"

**Solutions**:
1. Verify MongoDB is running:
   ```bash
   # Check if mongod process exists:
   tasklist | findstr mongod
   ```
2. Verify URI format:
   ```
   Local: mongodb://localhost:27017/etnopayers
   Atlas: mongodb+srv://user:password@cluster.mongodb.net/dbname
   ```
3. Check credentials (for Atlas):
   - Username and password are URL-encoded
   - Special characters (@, :, /, etc.) must be escaped
4. Test connection manually:
   ```bash
   # Using MongoDB Compass or mongo shell
   mongo "mongodb://localhost:27017/etnopayers"
   ```

#### PDF Extraction Hangs

**Symptoms**: PDF extraction doesn't complete, progress bar stuck

**Solutions**:
1. Check OLLAMA output:
   - Look at OLLAMA console for processing status
   - Large PDFs (>100 pages) may take 2-5 minutes
2. Monitor system resources:
   - Open Task Manager
   - Check if CPU/Memory maxed out
   - Close other applications
3. Try with smaller PDF first
4. Check OLLAMA model size:
   ```bash
   ollama list
   # llama2 model is ~4GB, requires adequate RAM
   ```

### Data and Configuration Issues

#### Lost Configuration/Settings

**Solution**:
```bash
# Settings stored in:
%USERPROFILE%\Documents\EtnoPapers\config.json

# To reset to defaults:
del "%USERPROFILE%\Documents\EtnoPapers\config.json"
# Restart EtnoPapers - will recreate with defaults
```

#### Lost Record Data

**Solution**:
```bash
# Local data stored in:
%USERPROFILE%\Documents\EtnoPapers\data.json

# Backup exists if synced to MongoDB
# To recover from backup:
# 1. Check MongoDB database
# 2. Export records as JSON
# 3. Import back to local storage
```

#### Performance Issues

**Solutions**:
1. Clear logs:
   ```bash
   del "%USERPROFILE%\Documents\EtnoPapers\logs\*.log"
   ```
2. Check disk space:
   ```bash
   # Ensure at least 500MB free
   # Large data.json file slows startup
   # Consider syncing to MongoDB and clearing local data
   ```
3. Monitor memory:
   - Restart application periodically
   - Close other applications
4. Optimize OLLAMA:
   - Use smaller model if available
   - Increase timeout settings if needed

---

## Uninstallation

### Using Windows Installer

1. Open **Settings** > **Apps** > **Installed apps**
2. Find **EtnoPapers** in the list
3. Click the three dots (...)
4. Select **Uninstall**
5. Confirm in the uninstall dialog
6. Click **Uninstall**
7. Follow the uninstall wizard
8. Click **Finish**

**OR using Control Panel:**

1. Open **Control Panel** > **Programs** > **Programs and Features**
2. Find **EtnoPapers 1.0.0**
3. Right-click and select **Uninstall**
4. Follow the uninstall wizard

### Portable Version

1. Delete the extracted folder
2. Delete the shortcut (if created)
3. No registry cleanup needed

### Preserving User Data

**After uninstallation**, user data is preserved in:
```
C:\Users\[User]\Documents\EtnoPapers\
```

To keep this data:
1. Back up this folder before uninstalling
2. Reinstall EtnoPapers
3. Copy backed-up files back to this location

To remove all user data and settings:
```bash
rmdir /s /q "%USERPROFILE%\Documents\EtnoPapers"
```

---

## Getting Help

If you encounter issues not covered in this guide:

1. **Check Documentation**
   - User Guide: `USER_GUIDE.md`
   - Technical Docs: `TECHNICAL.md`
   - Release Notes: `RELEASE_NOTES.md`

2. **Check Logs**
   ```bash
   %USERPROFILE%\Documents\EtnoPapers\logs\
   # Most recent log will have error details
   ```

3. **Report Issues**
   - GitHub Issues: https://github.com/etnopayers/etnopayers/issues
   - Include: OS version, error message, log file

4. **Community Help**
   - GitHub Discussions: https://github.com/etnopayers/etnopayers/discussions
   - Stack Overflow: Tag with `etnopayers`

---

**Happy extracting! 🌿📚**

