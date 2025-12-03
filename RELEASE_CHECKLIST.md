# EtnoPapers 1.0.0 Release Checklist

**Release Manager**: ________________
**Release Date**: December 2024
**Version**: 1.0.0
**Build Number**: ________________

---

## Pre-Release Phase

### Code Quality & Testing

- [ ] All 90+ unit tests passing (74 unit + 16 integration tests)
  - [ ] TitleNormalizer: 14/14 tests pass
  - [ ] AuthorFormatter: 21/21 tests pass
  - [ ] LanguageDetector: 20/20 tests pass
  - [ ] ArticleRecordValidator: 19/19 tests pass
  - [ ] JSON Serialization: 7/7 integration tests pass
  - [ ] DataStorageService: 9/9 integration tests pass

- [ ] Code compiles with zero errors
  - [ ] EtnoPapers.Core builds successfully
  - [ ] EtnoPapers.UI builds successfully
  - [ ] EtnoPapers.Core.Tests builds successfully
  - [ ] No unresolved dependencies
  - [ ] No breaking warnings

- [ ] Performance benchmarks acceptable
  - [ ] Startup time: < 2 seconds ✅
  - [ ] Record sorting: < 200ms ✅
  - [ ] Record filtering: < 200ms ✅
  - [ ] Record searching: < 200ms ✅
  - [ ] Memory usage: < 150MB idle ✅

- [ ] Security review completed
  - [ ] No hardcoded credentials
  - [ ] No SQL injection vulnerabilities
  - [ ] Proper input validation throughout
  - [ ] HTTPS for MongoDB Atlas
  - [ ] Local storage properly configured

### Documentation

- [ ] Release Notes completed
  - [ ] Features documented
  - [ ] System requirements listed
  - [ ] Known issues identified
  - [ ] Installation steps clear
  - [ ] Configuration guide included

- [ ] Installation Guide completed
  - [ ] Minimum requirements specified
  - [ ] Step-by-step installation for MSI
  - [ ] Step-by-step installation for portable
  - [ ] Configuration section complete
  - [ ] Troubleshooting section filled out
  - [ ] Verification checklist included

- [ ] User Documentation reviewed
  - [ ] USER_GUIDE.md current
  - [ ] API documentation accurate
  - [ ] Screenshots updated
  - [ ] Video tutorials prepared (if applicable)

- [ ] Technical Documentation reviewed
  - [ ] TECHNICAL.md accurate
  - [ ] Architecture diagrams current
  - [ ] Database schema documented
  - [ ] API contracts defined

### Version Management

- [ ] Version numbers updated
  - [ ] EtnoPapers.UI.csproj: Version 1.0.0 ✅
  - [ ] EtnoPapers.Core.csproj: Version 1.0.0 ✅
  - [ ] AssemblyVersion: 1.0.0.0 ✅
  - [ ] FileVersion: 1.0.0.0 ✅
  - [ ] Release notes mention 1.0.0

- [ ] Assembly metadata complete
  - [ ] Product name: "EtnoPapers"
  - [ ] Company: "EtnoPapers Project"
  - [ ] Copyright: "Copyright © 2024"
  - [ ] Description accurate

### Source Control

- [ ] All changes committed
  - [ ] No uncommitted files
  - [ ] All branches merged to main
  - [ ] Commit messages descriptive

- [ ] Repository tagged
  - [ ] Git tag: v1.0.0 created
  - [ ] Tag message includes release notes link
  - [ ] Tag signed (optional)

---

## Build Phase

### Release Build Execution

- [ ] Clean build performed
  ```bash
  dotnet clean
  dotnet build -c Release
  ```

- [ ] Artifacts generated
  - [ ] EtnoPapers.exe (main application)
  - [ ] EtnoPapers.UI.dll
  - [ ] EtnoPapers.Core.dll
  - [ ] All dependencies resolved
  - [ ] No build warnings (except known issues)

- [ ] Self-contained deployment created
  - [ ] .NET 8.0 runtime included
  - [ ] All dependencies packaged
  - [ ] Executable runs standalone
  - [ ] No external dependencies required

### Installer Creation

- [ ] Windows MSI installer created
  - [ ] File: EtnoPapers-Setup-1.0.0.msi
  - [ ] Size: ~120 MB (expected)
  - [ ] File signature valid
  - [ ] Installer launches without errors

- [ ] Portable ZIP package created
  - [ ] File: EtnoPapers-Portable-1.0.0.zip
  - [ ] Size: ~160 MB (expected)
  - [ ] Contains all necessary files
  - [ ] README.txt included

- [ ] Checksums calculated
  ```bash
  # SHA256 checksums
  certutil -hashfile EtnoPapers-Setup-1.0.0.msi SHA256
  certutil -hashfile EtnoPapers-Portable-1.0.0.zip SHA256
  # Saved to checksums.txt
  ```

---

## Testing Phase

### Installation Testing (MSI)

**Tester**: ________________  **Date**: ________________

#### Clean System Test
- [ ] Test on Windows 10 clean installation
- [ ] Test on Windows 11 clean installation
- [ ] Administrator rights required verified
- [ ] Installation directory configurable
- [ ] Installation time: 2-5 minutes
- [ ] Progress bar displays correctly
- [ ] Completion message shows
- [ ] Application doesn't auto-launch on failure

#### Post-Installation
- [ ] Application appears in Add/Remove Programs
- [ ] Start Menu shortcut created
- [ ] Desktop shortcut created (if selected)
- [ ] Registry entries correct
- [ ] File associations configured (if selected)

#### Uninstallation
- [ ] Uninstall via Add/Remove Programs works
- [ ] All files removed from installation directory
- [ ] Registry entries cleaned up
- [ ] Start Menu shortcuts removed
- [ ] User data preserved in Documents
- [ ] Clean uninstall, no orphaned files

### Installation Testing (Portable)

**Tester**: ________________  **Date**: ________________

- [ ] ZIP extracts successfully
- [ ] All files present after extraction
- [ ] Folder can be placed on USB drive
- [ ] Application runs from any location
- [ ] Settings saved to local folder
- [ ] Clean removal possible (just delete folder)

### Runtime Testing (Windows 10)

**Tester**: ________________  **Date**: ________________

- [ ] Application launches in <2 seconds
- [ ] Main window displays correctly
- [ ] All navigation menu items visible
- [ ] Home page loads without errors
- [ ] "Registros" page works
- [ ] "Sincronização" page accessible
- [ ] "Configurações" loads
- [ ] No crash on rapid page switching

#### Feature Testing
- [ ] PDF upload button works
- [ ] File dialog opens/closes
- [ ] Invalid files rejected with error
- [ ] Settings can be modified
- [ ] Settings persist after restart
- [ ] OLLAMA connection test works (if configured)
- [ ] MongoDB connection test works (if configured)

#### Performance Testing
- [ ] Startup < 2 seconds ✅
- [ ] Record list load < 500ms (empty db)
- [ ] 1000 record load < 500ms
- [ ] Record sort < 200ms
- [ ] Record filter < 200ms
- [ ] Memory <150MB idle
- [ ] No memory leaks on extended use

### Runtime Testing (Windows 11)

**Tester**: ________________  **Date**: ________________

- [ ] Application launches successfully
- [ ] All features work as expected
- [ ] Performance meets targets
- [ ] No Windows 11 specific issues
- [ ] High DPI display scaling works
- [ ] Taskbar integration correct

### Configuration Testing

**Tester**: ________________  **Date**: ________________

- [ ] OLLAMA configuration:
  - [ ] URL field accepts input
  - [ ] Model field accepts input
  - [ ] Connection test works
  - [ ] Status indicator shows correctly
  - [ ] Settings saved on restart

- [ ] MongoDB configuration:
  - [ ] URI field accepts input
  - [ ] Connection test works
  - [ ] Status indicator shows correctly
  - [ ] Invalid URI rejected

- [ ] Language configuration:
  - [ ] Portuguese (pt-BR) selectable
  - [ ] English (en-US) selectable
  - [ ] Language changes on restart
  - [ ] UI displays in selected language

---

## QA Sign-Off

### Test Summary

- **Total Test Cases**: 90+ automated + manual tests
- **Pass Rate**: _____ %
- **Known Issues**: None / List: ________________
- **Critical Issues Found**: None / List: ________________

### Testing Approval

| Role | Name | Signature | Date |
|------|------|-----------|------|
| QA Tester | __________ | __________ | __________ |
| QA Manager | __________ | __________ | __________ |

### Issues Found

| # | Issue | Severity | Resolution | Status |
|---|-------|----------|-----------|--------|
| 1 | | | | |
| 2 | | | | |

---

## Release Phase

### Release Artifacts

- [ ] EtnoPapers-Setup-1.0.0.msi
  - [ ] File present
  - [ ] Size verified (~120 MB)
  - [ ] Checksum calculated
  - [ ] Virus scan passed
  - [ ] Ready for distribution

- [ ] EtnoPapers-Portable-1.0.0.zip
  - [ ] File present
  - [ ] Size verified (~160 MB)
  - [ ] Checksum calculated
  - [ ] Virus scan passed
  - [ ] Ready for distribution

- [ ] Documentation
  - [ ] RELEASE_NOTES.md
  - [ ] INSTALL.md
  - [ ] USER_GUIDE.md
  - [ ] TECHNICAL.md
  - [ ] README.md
  - [ ] LICENSE.md
  - [ ] checksums.txt

### GitHub Release

- [ ] GitHub account verified
- [ ] Repository access confirmed
- [ ] Release created with tag v1.0.0
- [ ] Release notes published
- [ ] Artifacts uploaded
- [ ] Checksums included
- [ ] Release marked as "Latest" (if applicable)

### Distribution Channels

- [ ] GitHub Releases page updated
- [ ] Website download page updated (if applicable)
- [ ] News/announcement posted
- [ ] Documentation links updated
- [ ] Community notification sent

---

## Post-Release Phase

### Monitoring

- [ ] Issue tracker monitored
- [ ] Installation problems tracked
- [ ] Performance issues monitored
- [ ] User feedback collected

**Monitoring Period**: 2 weeks post-release

### Support Readiness

- [ ] Support documentation available
- [ ] FAQ updated with common issues
- [ ] Troubleshooting guide reviewed
- [ ] Support team briefed

### Next Release Planning

- [ ] Release retrospective scheduled
- [ ] Feedback incorporated
- [ ] Next version planned
- [ ] Roadmap updated

---

## Sign-Off

### Release Authorization

| Role | Name | Signature | Date |
|------|------|-----------|------|
| Release Manager | __________ | __________ | __________ |
| Project Lead | __________ | __________ | __________ |
| Technical Lead | __________ | __________ | __________ |

### Release Status

**Overall Status**: ✅ APPROVED / ⚠️ APPROVED WITH ISSUES / ❌ BLOCKED

**Comments/Notes**:
```
_________________________________________________________________

_________________________________________________________________

_________________________________________________________________
```

### Release Date

**Planned Release Date**: December 2024
**Actual Release Date**: ________________
**Release Number**: 1.0.0

---

## Archive

**Release Archive Location**: `https://github.com/etnopayers/etnopayers/releases/tag/v1.0.0`

**Backup Locations**:
- [ ] External drive backup
- [ ] Cloud storage backup
- [ ] Release artifacts archived

---

**Released By**: ________________________
**Date**: ________________
**Version**: 1.0.0

---

For questions or issues, contact the Release Manager.

