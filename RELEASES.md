# Etnopapers Release Management Guide

## Overview

Etnopapers distributes as **standalone native executables** for Windows, macOS, and Linux. Releases are automatically built and published using GitHub Actions when you push a version tag.

## Creating a New Release

### Step 1: Update Version Numbers

Before creating a release, update version numbers in relevant files:

```bash
# Frontend version (optional, for consistency)
# Edit frontend/package.json:
{
  "version": "2.0.0"
}

# Backend version (optional)
# Edit backend/__init__.py (if exists) or backend/config.py
```

### Step 2: Tag and Push

Create a Git tag following semantic versioning (v1.0.0, v2.1.0, etc.):

```bash
# Create annotated tag
git tag -a v2.0.0 -m "Release version 2.0.0 - Ollama local AI integration"

# Push tag to GitHub
git push origin v2.0.0
```

This automatically triggers the `.github/workflows/releases.yml` workflow.

### Step 3: Monitor Build Progress

1. Go to: https://github.com/edalcin/etnopapers/actions
2. Find the "Build Cross-Platform Releases" workflow run
3. Check the status of builds for:
   - **Linux**
   - **Windows**
   - **macOS**

Each platform builds in parallel. The workflow will:
- ✅ Install dependencies (Node.js, Python)
- ✅ Build frontend (React → dist/)
- ✅ Install Python packages
- ✅ Package with PyInstaller
- ✅ Upload artifacts to release

### Step 4: Verify Release

Once the workflow completes:

1. Go to: https://github.com/edalcin/etnopapers/releases
2. You should see a new release with:
   - **Linux**: `etnopapers-linux-v2.0.0`
   - **Windows**: `etnopapers-windows-v2.0.0.exe`
   - **macOS**: `Etnopapers-macos-v2.0.0.zip`

The release description includes:
- Download links for each OS
- Prerequisites (Ollama, MongoDB)
- Getting started instructions
- System requirements
- Troubleshooting guide

## Release Workflow Details

### Triggered By

- **Push to tag** matching pattern: `v*` (e.g., `v1.0.0`, `v2.0.0-beta`, `v3.1.2-rc1`)

### Build Matrix

The workflow runs **3 concurrent build jobs**:

| Platform | Runner | Output | Size |
|----------|--------|--------|------|
| **Linux** | `ubuntu-latest` | `etnopapers-linux-vX.Y.Z` | ~150 MB |
| **Windows** | `windows-latest` | `etnopapers-windows-vX.Y.Z.exe` | ~150 MB |
| **macOS** | `macos-latest` | `Etnopapers-macos-vX.Y.Z.zip` | ~150 MB |

### Build Steps (Per Platform)

1. **Checkout** - Clone repository
2. **Setup Node.js 18** - With npm cache
3. **Setup Python 3.11** - With pip cache
4. **Build** - Run platform-specific script:
   - Linux: `bash build-linux.sh`
   - Windows: `.\build-windows.bat`
   - macOS: `bash build-macos.sh`
5. **Verify** - Check artifact exists
6. **Package** - Prepare for release:
   - macOS: Create `.zip` of `.app` bundle
   - Linux/Windows: Rename with version tag
7. **Upload** - Store artifacts (1 day retention)

### Release Creation

After all 3 builds succeed:

1. **Download** all artifacts from build jobs
2. **Create GitHub Release** with:
   - All 3 executables attached
   - Pre-formatted release notes
   - Automatic version detection from tag name
3. **Clean up** - Remove temporary build artifacts

## Distributing Releases

### Users Download From

https://github.com/edalcin/etnopapers/releases

### Installation Instructions (Include in Release Notes)

```
1. Download the executable for your OS:
   - Linux: etnopapers-linux-v2.0.0
   - Windows: etnopapers-windows-v2.0.0.exe
   - macOS: Etnopapers-macos-v2.0.0.zip (extract first)

2. Install Ollama (required):
   https://ollama.com/download

3. Configure MongoDB:
   - Local: Install MongoDB Community Edition
   - Cloud: Create account at mongodb.com/cloud/atlas

4. Run the executable
   - First run shows configuration dialog
   - Enter MongoDB URI
   - App opens at http://localhost:8000
```

## Troubleshooting

### Build Fails: "Build artifact not found"

**Cause**: PyInstaller didn't create executable

**Fix**:
1. Check the build job logs in GitHub Actions
2. Run build locally:
   ```bash
   bash build-linux.sh  # or build-windows.bat / build-macos.sh
   ```
3. Fix any errors and push a new tag

### Build Passes but Release Not Created

**Cause**: "Create GitHub Release" job failed

**Fix**:
1. Check the release job logs
2. Verify `GITHUB_TOKEN` has permission to create releases
3. Check for naming conflicts with existing releases

### Artifact Upload Times Out

**Cause**: Large builds (~150 MB) timing out on upload

**Fix**:
- Already handled in workflow (uploads.retention-days=1)
- Artifacts auto-clean after release creation
- If persistent, increase timeout in workflow

## Legacy Docker Releases

Old Docker-based releases are no longer needed. To clean them up:

### Option 1: Manual Deletion

1. Go to: https://github.com/edalcin/etnopapers/releases
2. Click the release
3. Click "Delete this release" (red button)

### Option 2: Automated Deletion (bash)

```bash
# Requirements: curl, grep, awk
# Linux/macOS only

export GITHUB_TOKEN=your_personal_access_token
bash .github/scripts/delete-docker-releases.sh
```

**Important**: The script identifies Docker releases by checking release notes for "docker" keywords.

## Best Practices

### Version Numbering

Follow [Semantic Versioning](https://semver.org/):
- **MAJOR.MINOR.PATCH** (e.g., 2.0.0)
- **Major**: Breaking changes
- **Minor**: New features (backward compatible)
- **Patch**: Bug fixes

Examples:
- `v1.0.0` - First release
- `v1.1.0` - Added feature X
- `v1.1.1` - Fixed bug Y
- `v2.0.0` - Major refactor (breaking changes)

### Release Notes Template

```markdown
# Etnopapers v2.0.0

## New Features
- Ollama local AI integration (no API keys)
- MongoDB support (local and cloud)
- Metadata extraction from PDF files

## Bug Fixes
- Fixed CORS issues in FastAPI
- Improved error handling for missing Ollama

## Breaking Changes
- Removed cloud API provider support
- Requires local Ollama installation

## Prerequisites
- Ollama: https://ollama.com/download
- MongoDB: Local or Atlas cloud

## Downloads
[Provided automatically by workflow]
```

### Testing Before Release

Before creating a release tag:

```bash
# Run tests
npm test              # Frontend
pytest                # Backend

# Verify locally on at least 2 platforms
bash build-linux.sh
./build-windows.bat  # (on Windows)
bash build-macos.sh  # (on macOS)

# Test the executable
./dist/etnopapers-linux-*
```

## GitHub Actions Configuration

The release workflow is defined in `.github/workflows/releases.yml`:

- **Trigger**: Push to tag matching `v*`
- **Strategy**: Matrix build (Linux, Windows, macOS)
- **Cache**: NPM and pip caches enabled
- **Artifacts**: Temporary 1-day retention
- **Release**: Auto-created with all 3 executables

## Security Notes

The workflow uses:
- `actions/checkout@v4` - Official GitHub action
- `actions/setup-node@v4` - Official GitHub action
- `actions/setup-python@v4` - Official GitHub action
- `softprops/action-gh-release@v1` - Community action (well-maintained)
- `geekyeggo/delete-artifact@v2` - Community action (optional cleanup)

All dependencies are pinned to specific versions for security.

## Support & Feedback

If you encounter issues with releases:

1. Check GitHub Actions logs: https://github.com/edalcin/etnopapers/actions
2. Review troubleshooting section above
3. Open an issue: https://github.com/edalcin/etnopapers/issues
4. Include:
   - OS and version
   - Version of Etnopapers
   - Error message (if any)
   - Steps to reproduce

## Next Steps

1. Create first release: `git tag -a v2.0.0 -m "Initial release" && git push origin v2.0.0`
2. Monitor build in GitHub Actions
3. Download and test executable on each OS
4. Share release URL with users: https://github.com/edalcin/etnopapers/releases
5. Delete any legacy Docker releases (if applicable)

---

**Last Updated**: 2024
**Workflow File**: `.github/workflows/releases.yml`
**Tested Platforms**: Linux (Ubuntu), Windows, macOS
