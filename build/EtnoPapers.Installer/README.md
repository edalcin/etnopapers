# EtnoPapers Windows Installer (WiX Toolset)

This directory contains the Windows MSI installer project for EtnoPapers using WiX Toolset v4.

## Project Structure

```
EtnoPapers.Installer/
├── EtnoPapers.Installer.wixproj    - WiX project configuration
├── Product.wxs                      - Main product definition
├── License.rtf                      - License agreement text
├── Banner.bmp                       - Installer banner image (500x63 pixels)
├── Dialog.bmp                       - Dialog background image (262x392 pixels)
└── README.md                        - This file
```

## Files Description

### EtnoPapers.Installer.wixproj
WiX project file that defines:
- Build configuration (Debug/Release)
- Output name and type (EtnoPapers-Setup-1.0.0.msi)
- Publish directory for application binaries
- Compiler settings and preprocessor variables

### Product.wxs
Main WiX source file containing:
- **T085**: System requirements check (Windows 10+)
- **T086**: Feature definition (application, shortcuts)
- **T087**: User interface configuration (UI dialogs, banner, license)
- **T088**: Installation directories and shortcuts:
  - `C:\Program Files\EtnoPapers` - Main installation folder
  - Start Menu shortcuts in `C:\ProgramData\Microsoft\Windows\Start Menu\Programs\EtnoPapers`
  - Optional desktop shortcut
- **T089**: Registry entries:
  - HKLM\SOFTWARE\EtnoPapers - Application metadata
  - Add/Remove Programs entry
  - PDF file association (optional)
- **T090**: Release build configuration with optimization

### License.rtf
Rich Text Format license file displayed during installation:
- MIT License terms
- Project details and description
- System requirements
- Third-party component acknowledgments

### Branding Files (To be created)

**Banner.bmp** (Required)
- Dimensions: 500 x 63 pixels
- Format: 24-bit BMP
- Used in: Welcome and completion dialogs
- Should contain EtnoPapers logo and branding

**Dialog.bmp** (Required)
- Dimensions: 262 x 392 pixels
- Format: 24-bit BMP
- Used in: License and installation dialogs
- Should contain EtnoPapers logo and branding elements

## Building the Installer

### Prerequisites

1. **WiX Toolset v4**: Install from https://wixtoolset.org/
2. **.NET 8.0 Desktop Runtime**: The application must be built and published first
3. **Visual Studio 2022**: Optional but recommended for editing WiX files

### Build Steps

```powershell
# From the etnopapers root directory

# 1. Publish the application
cd src\EtnoPapers.UI
dotnet publish -c Release -r win-x64 --self-contained

# 2. Build the MSI installer
cd ..\..\build\EtnoPapers.Installer
dotnet build -c Release
# Output: bin\Release\EtnoPapers-Setup-1.0.0.msi

# Or use the automated release script:
cd ..\..
.\build\build-release.ps1 -Version 1.0.0 -Configuration Release
```

### Using build-release.ps1

The PowerShell script in the build directory handles:
1. Cleaning the solution
2. Running all tests
3. Publishing the application
4. Building the WiX project
5. Creating portable ZIP
6. Generating checksums
7. Creating build reports

```powershell
.\build\build-release.ps1 -Version 1.0.0 -Configuration Release -OutputDir ./artifacts
```

## Installation Features

### Main Features
- User-friendly setup wizard
- System requirements validation
- Automatic .NET 8.0 runtime installation (if needed)
- Configurable installation directory
- Optional Start Menu shortcuts
- Optional desktop shortcut
- Optional PDF file associations

### File Structure After Installation

```
C:\Program Files\EtnoPapers\
├── EtnoPapers.exe                 (Main application)
├── EtnoPapers.UI.dll             (UI assembly)
├── EtnoPapers.Core.dll           (Core services)
├── LICENSE.md
├── README.md
└── [Runtime dependencies]

C:\Users\[User]\Documents\EtnoPapers\
├── data.json                      (Local record storage)
├── config.json                    (User settings)
└── logs/                          (Application logs)
```

### Registry Entries

**HKEY_LOCAL_MACHINE\SOFTWARE\EtnoPapers**
- DisplayName: EtnoPapers 1.0.0
- DisplayVersion: 1.0.0
- Publisher: EtnoPapers Project
- InstallLocation: C:\Program Files\EtnoPapers
- URLInfoAbout: https://github.com/etnopayers/etnopayers

**HKEY_CURRENT_USER\Software\EtnoPapers**
- Settings and preferences (created by application)

## Customization

### Changing Branding

1. **Replace Banner.bmp**
   - Create 500x63 pixel image
   - Include EtnoPapers logo
   - Save as 24-bit BMP

2. **Replace Dialog.bmp**
   - Create 262x392 pixel image
   - Include EtnoPapers branding
   - Save as 24-bit BMP

3. **Update License.rtf**
   - Edit License.rtf with compatible RTF editor
   - Maintain RTF format
   - Do not change structure beyond content

### Modifying Features

Edit `Product.wxs` to:
- Add/remove optional features
- Change installation directories
- Modify registry entries
- Update system requirements
- Add new file components

## Troubleshooting

### Build Errors

**Error: "WiX toolset not found"**
- Solution: Install WiX Toolset v4 from https://wixtoolset.org/

**Error: "Cannot find publish directory"**
- Solution: Ensure application is published first:
  ```powershell
  dotnet publish -c Release -r win-x64 --self-contained
  ```

**Error: "Bitmap file not found"**
- Solution: Create Banner.bmp and Dialog.bmp files or disable branding

### Installation Issues

See `INSTALL.md` in the root directory for:
- Installation troubleshooting
- System requirement verification
- Uninstallation procedures
- Post-installation configuration

## Testing

### Pre-Release Testing

1. Build the MSI on clean development system
2. Test installation on Windows 10 VM
3. Test installation on Windows 11 VM
4. Verify all shortcuts created
5. Verify registry entries
6. Test uninstallation
7. Verify clean removal

### Test Checklist

See `RELEASE_CHECKLIST.md` in the root directory for comprehensive testing checklist (T093).

## Versioning

The installer version follows semantic versioning:
- Major.Minor.Patch.Build format
- Current: 1.0.0.0
- Updated in: Product.wxs UpgradeCode and Version attributes
- Updated in: EtnoPapers.Installer.wixproj ProductVersion

## Distribution

The built MSI (`EtnoPapers-Setup-1.0.0.msi`) can be:
1. Downloaded directly from GitHub Releases
2. Distributed via installers on websites
3. Packaged for enterprise deployment
4. Signed with digital certificates for trust

See `RELEASE_CHECKLIST.md` for distribution checklist (T095-T097).

## Support

For issues or questions:
- GitHub Issues: https://github.com/etnopayers/etnopayers/issues
- GitHub Discussions: https://github.com/etnopayers/etnopayers/discussions
- Documentation: `INSTALL.md`, `USER_GUIDE.md`, `TECHNICAL.md`

## Resources

- WiX Toolset Documentation: https://wixtoolset.org/documentation/
- WiX UI Extension Guide: https://wixtoolset.org/docs/wixui/
- Visual Studio Installer Projects: https://docs.microsoft.com/en-us/visualstudio/install/
