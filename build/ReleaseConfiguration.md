# EtnoPapers Release Configuration (Phase 8)

**Version**: 1.0.0
**Release Date**: December 2024
**Target Platform**: Windows 10/11 (x64)
**.NET Framework**: .NET 8.0

---

## T084: Version & Assembly Information

**Current Version**: 1.0.0.0
**Assembly Version**: 1.0.0.0
**Product Name**: EtnoPapers
**Company**: EtnoPapers Project

### Assembly Properties

- **Product**: EtnoPapers (Ethnobotanical Paper Analysis Tool)
- **Description**: Automated extraction and cataloging of ethnobotanical metadata from scientific papers about traditional plant use
- **Company**: EtnoPapers Contributors
- **Copyright**: Copyright © 2024 EtnoPapers Contributors
- **Trademark**: EtnoPapers®

### File Versioning Strategy

- **Major Version**: Incremented for significant feature releases
- **Minor Version**: Incremented for feature additions and enhancements
- **Patch Version**: Incremented for bug fixes and patches
- **Build Version**: Auto-incremented per build

---

## T085-T090: Installer Configuration

### System Requirements

**Minimum Requirements:**
- OS: Windows 10 (Build 1909) or Windows 11
- Processor: 1 GHz dual-core processor
- RAM: 4 GB minimum (8 GB recommended)
- Disk Space: 500 MB free space
- .NET Runtime: .NET 8.0 Desktop Runtime (bundled in installer)
- Screen Resolution: 1280x720 minimum (1920x1080 recommended)

**Optional Requirements:**
- OLLAMA: For AI-powered metadata extraction (local installation on localhost:11434)
- MongoDB: For remote synchronization (optional, user provides URI)

### Installation Directories

**Default Installation Path**: `C:\Program Files\EtnoPapers`

**Directory Structure After Installation:**
```
C:\Program Files\EtnoPapers\
├── EtnoPapers.exe                 (Main application)
├── EtnoPapers.UI.dll             (WPF UI assembly)
├── EtnoPapers.Core.dll           (Core services)
├── Dependencies/
│   ├── Newtonsoft.Json.dll
│   ├── MongoDB.Driver.dll
│   ├── Serilog.dll
│   └── iTextSharp.dll
├── License.txt
└── README.md

C:\Users\[User]\Documents\EtnoPapers\    (User data directory)
├── data.json                      (Local record storage)
├── logs/                          (Application logs)
└── config.json                    (User settings)
```

### Application Shortcuts

**Start Menu:**
- Program Files > EtnoPapers > EtnoPapers

**Desktop:**
- EtnoPapers (optional, configurable during installation)

**File Associations:**
- PDF files can be opened with EtnoPapers (optional)

### Registry Entries

**Location**: `HKEY_LOCAL_MACHINE\SOFTWARE\EtnoPapers`

```
- DisplayName: EtnoPapers 1.0.0
- DisplayVersion: 1.0.0
- Publisher: EtnoPapers Project
- UninstallString: C:\Program Files\EtnoPapers\uninstall.exe
- InstallLocation: C:\Program Files\EtnoPapers
- VersionMajor: 1
- VersionMinor: 0
```

**File Association** (Optional):
- `.pdf` associated with `EtnoPapers.exe /open "%1"`

---

## T091: Release Build Configuration

### Build Profile: Release

**MSBuild Configuration:**
```xml
<PropertyGroup Condition="'$(Configuration)|$(Platform)'=='Release|AnyCPU'">
  <DebugType>embedded</DebugType>
  <DebugSymbols>false</DebugSymbols>
  <Optimize>true</Optimize>
  <TieredCompilation>true</TieredCompilation>
  <TieredCompilationQuickJit>true</TieredCompilationQuickJit>
</PropertyGroup>
```

**Build Steps:**
1. Clean solution
2. Restore NuGet packages
3. Run unit tests (90 tests must pass)
4. Build UI project in Release mode
5. Build Core library in Release mode
6. Create self-contained deployment
7. Package binaries
8. Generate installer

**Output Artifacts:**
- `EtnoPapers.exe` (self-contained, ~150MB)
- `EtnoPapers-Setup-1.0.0.msi` (installer, ~120MB)
- `EtnoPapers-Portable-1.0.0.zip` (portable version, ~160MB)
- `release-notes-1.0.0.md` (release documentation)
- `checksums.txt` (SHA256 hashes for distribution)

---

## T092: Release Documentation

### Components

1. **Release Notes** (`RELEASE_NOTES.md`)
   - Version summary
   - New features
   - Bug fixes and improvements
   - Breaking changes (if any)
   - Known issues
   - Installation instructions
   - System requirements

2. **Installation Guide** (`INSTALL.md`)
   - Step-by-step installation
   - Configuration setup (OLLAMA, MongoDB)
   - Troubleshooting common issues
   - Uninstallation instructions
   - Post-installation verification

3. **User Guide** (`USER_GUIDE.md`)
   - Application overview
   - Feature documentation
   - Workflow tutorials
   - Configuration options
   - FAQ and troubleshooting

4. **Technical Documentation** (`TECHNICAL.md`)
   - Architecture overview
   - API documentation
   - Data model specifications
   - Database schema
   - Development environment setup

---

## T093: Installation Testing Checklist

**Test Environment:**
- Windows 10/11 clean installation
- No Visual Studio or development tools
- No .NET SDK (only runtime)

**Installation Tests:**
- [ ] Installer launches without errors
- [ ] System requirements check passes
- [ ] Installation completes successfully
- [ ] Shortcuts created in Start Menu
- [ ] Desktop shortcut created (if selected)
- [ ] Uninstall works cleanly
- [ ] Registry entries correct
- [ ] File associations configured

**Runtime Tests:**
- [ ] Application launches from Start Menu
- [ ] Application launches from desktop shortcut
- [ ] Main window displays correctly
- [ ] All pages load without errors
- [ ] PDF upload works
- [ ] Configuration can be saved
- [ ] Application closes cleanly
- [ ] Logs created in expected location

**Performance Tests:**
- [ ] Startup time < 2 seconds
- [ ] Record list loads 1000 items < 200ms
- [ ] Filtering performance meets targets
- [ ] Memory usage < 150MB idle

---

## T094: Release Checklist

### Pre-Release

- [ ] All 90+ tests passing
- [ ] Code reviewed and approved
- [ ] Version numbers updated
- [ ] Release notes completed
- [ ] Documentation up-to-date
- [ ] Security audit passed
- [ ] Performance benchmarks acceptable
- [ ] Build process verified

### Release

- [ ] Build artifacts created
- [ ] Installer tested on clean system
- [ ] Portable version tested
- [ ] Checksums calculated
- [ ] Release notes published
- [ ] GitHub release created
- [ ] Downloads available
- [ ] Installation guide verified

### Post-Release

- [ ] Monitor for issues
- [ ] Track user feedback
- [ ] Update FAQ with common issues
- [ ] Plan hotfixes if needed
- [ ] Schedule next release

---

## T095-T097: Distribution & Support

### Distribution Channels

1. **GitHub Releases**
   - Location: `https://github.com/etnopayers/etnopayers/releases`
   - Assets: Installer, portable zip, checksums, release notes

2. **Installation Methods**

   **Method 1: Windows Installer (MSI)**
   - User-friendly setup wizard
   - Automatic startup configuration
   - System integration (Add/Remove Programs)
   - Recommended for most users

   **Method 2: Portable ZIP**
   - No installation required
   - Run directly from extracted folder
   - Suitable for USB drives, portable systems
   - All data stored locally

3. **Update Strategy**
   - Manual update checks in Settings
   - In-app notification of new versions
   - One-click upgrade to newer releases
   - Automatic backup of settings before upgrade

### Support Documentation

**Installation Troubleshooting:**
- Windows Defender/Antivirus blocking installation
- Insufficient disk space
- Missing .NET Runtime
- Administrator rights required
- Registry permission issues

**Runtime Troubleshooting:**
- Database connection issues
- OLLAMA service not responding
- MongoDB connection failures
- PDF extraction errors
- File not found errors

**Configuration Help:**
- OLLAMA setup and configuration
- MongoDB URI formatting
- Language and locale settings
- Custom model selection
- Network proxy configuration

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0.0 | Dec 2024 | Initial release - WPF migration complete |

---

## Contact & Support

- **Project Repository**: [GitHub](https://github.com/etnopayers/etnopayers)
- **Issue Tracker**: [GitHub Issues](https://github.com/etnopayers/etnopayers/issues)
- **Documentation**: [GitHub Wiki](https://github.com/etnopayers/etnopayers/wiki)
- **License**: MIT License

