# Cleanup: Remove Legacy Docker Releases

This guide explains how to delete legacy Docker releases from your GitHub repository.

## Background

Etnopapers v2.0 moved from Docker-based distribution to standalone native executables (Windows, macOS, Linux) using PyInstaller. Any Docker releases from v1.x should be cleaned up.

## Method 1: Manual Deletion via Web UI (Recommended)

### Steps

1. Go to: **https://github.com/edalcin/etnopapers/releases**

2. Look for releases with Docker-related keywords in the title or description:
   - "docker"
   - "container"
   - "image"
   - Any v1.x releases (if they used Docker)

3. For each legacy Docker release:
   - Click the **three dots (⋮)** menu in the top-right corner
   - Select **"Delete this release"**
   - Confirm deletion

4. Done! The legacy releases are removed.

## Method 2: Automated Deletion via GitHub CLI

### Prerequisites

- GitHub CLI installed: https://cli.github.com
- Authenticated: `gh auth login`

### Steps

```bash
# Clone or navigate to the repository
cd etnopapers

# List all releases
gh release list

# Delete specific release by tag
gh release delete v1.0.0 --yes
gh release delete v1.1.0 --yes
gh release delete v1.2.0 --yes

# Or delete multiple releases matching a pattern
for release in $(gh release list --limit 100 | grep docker | awk '{print $1}'); do
  echo "Deleting $release..."
  gh release delete "$release" --yes
done
```

## Method 3: Automated Deletion via Script

### Steps

1. Get a Personal Access Token (PAT):
   - Go to: https://github.com/settings/tokens
   - Click "Generate new token" → "Generate new token (classic)"
   - Select scopes:
     - `public_repo` (public repositories)
     - `repo` (private repositories - if applicable)
   - Copy the token

2. Run the cleanup script:

```bash
# Linux/macOS
export GITHUB_TOKEN="your_personal_access_token_here"
bash .github/scripts/delete-docker-releases.sh

# Windows (PowerShell)
$env:GITHUB_TOKEN = "your_personal_access_token_here"
bash .github/scripts/delete-docker-releases.sh
```

The script will:
- Fetch all releases from the repository
- Check each release's description for Docker keywords
- Delete matching releases automatically
- Report which releases were deleted

## Verification

After cleanup, verify that only standalone executable releases remain:

```bash
gh release list
```

Expected remaining releases (after v2.0.0+):
- `v2.0.0` → `etnopapers-linux-v2.0.0`, `etnopapers-windows-v2.0.0.exe`, `Etnopapers-macos-v2.0.0.zip`
- `v2.1.0` → (same naming pattern)
- etc.

## Safety Notes

⚠️ **Warning**: Deleting releases cannot be undone. Make sure you have:

- [ ] Verified which releases are Docker-based
- [ ] Backed up any release notes (they're archived in Git history anyway)
- [ ] Confirmed no users actively download those releases

## What's Next

Once legacy Docker releases are cleaned up:

1. ✅ Future releases automatically use the new GitHub Actions workflow
2. ✅ Only standalone native executables are provided
3. ✅ Users get executables for their OS directly
4. ✅ No Docker/container complexity for end users

## Need Help?

If you have questions:
- Check GitHub release documentation: https://docs.github.com/en/repositories/releasing-projects-on-github/managing-releases-in-a-repository
- Review the `RELEASES.md` file for release management guide
- Open an issue: https://github.com/edalcin/etnopapers/issues

---

**Script Location**: `.github/scripts/delete-docker-releases.sh`
**Workflow Definition**: `.github/workflows/releases.yml`
**Release Guide**: `RELEASES.md`
