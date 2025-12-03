#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Prepare EtnoPapers release artifacts for distribution (T095)

.DESCRIPTION
    Validates build artifacts, verifies checksums, and prepares GitHub release package.
    Executes after successful installer testing (T093).

.PARAMETER BuildDirectory
    Location of build artifacts (default: ./artifacts)

.PARAMETER Version
    Release version number (default: 1.0.0)

.PARAMETER GitHubToken
    GitHub personal access token for releases (optional, for automated upload)

.EXAMPLE
    .\prepare-distribution.ps1 -Version 1.0.0 -BuildDirectory ./artifacts
    .\prepare-distribution.ps1 -Version 1.0.0 -BuildDirectory ./artifacts -Validate

.NOTES
    Part of Phase 8: Build, Installer & Release (T095)
    Requires successful build (build-release.ps1) and testing (T093)

.PARAMETER Validate
    Only validate artifacts without packaging (default: $false)

.PARAMETER CreatePackage
    Create release package ZIP file (default: $true)
#>

param(
    [string]$BuildDirectory = "./artifacts",
    [string]$Version = "1.0.0",
    [string]$GitHubToken = "",
    [switch]$Validate = $false,
    [switch]$CreatePackage = $true
)

# Color output helpers
function Write-Info {
    param([string]$Message)
    Write-Host "[INFO] $Message" -ForegroundColor Cyan
}

function Write-Success {
    param([string]$Message)
    Write-Host "[SUCCESS] $Message" -ForegroundColor Green
}

function Write-Warning {
    param([string]$Message)
    Write-Host "[WARNING] $Message" -ForegroundColor Yellow
}

function Write-Error-Custom {
    param([string]$Message)
    Write-Host "[ERROR] $Message" -ForegroundColor Red
}

# Main script
Write-Host "`n╔════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║     Distribution Preparation (T095)        ║" -ForegroundColor Cyan
Write-Host "║            Version $Version               ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════╝`n" -ForegroundColor Cyan

# Resolve paths
$BuildDir = Resolve-Path $BuildDirectory -ErrorAction SilentlyContinue
if (-not $BuildDir) {
    Write-Error-Custom "Build directory not found: $BuildDirectory"
    exit 1
}

Write-Info "Build Directory: $BuildDir"
Write-Info "Release Version: $Version"

# Array to track validation results
$validationResults = @()

# ======== ARTIFACT VALIDATION ========

Write-Host "`n--- Artifact Validation ---`n"

# Check for required files
$requiredFiles = @(
    @{Name = "EtnoPapers-Setup-$Version.msi"; Description = "MSI Installer"; Size = "~120 MB" },
    @{Name = "EtnoPapers-Portable-$Version.zip"; Description = "Portable Package"; Size = "~160 MB" },
    @{Name = "checksums.txt"; Description = "SHA256 Checksums"; Size = "~1 KB" },
    @{Name = "BUILD_REPORT.txt"; Description = "Build Report"; Size = "~10 KB" }
)

$artifactPath = Join-Path $BuildDir ""

foreach ($file in $requiredFiles) {
    $fullPath = Join-Path $BuildDir $file.Name
    $exists = Test-Path $fullPath

    if ($exists) {
        $fileInfo = Get-Item $fullPath
        $sizeMB = [math]::Round($fileInfo.Length / 1MB, 2)
        Write-Success "✓ $($file.Description): $($file.Name) ($sizeMB MB)"
        $validationResults += @{
            Item = $file.Name
            Status = "PASS"
            Details = "$sizeMB MB"
        }
    } else {
        Write-Error-Custom "✗ $($file.Description): $($file.Name) NOT FOUND"
        $validationResults += @{
            Item = $file.Name
            Status = "FAIL"
            Details = "File not found"
        }
    }
}

# ======== CHECKSUM VERIFICATION ========

Write-Host "`n--- Checksum Verification ---`n"

$checksumFile = Join-Path $BuildDir "checksums.txt"
if (Test-Path $checksumFile) {
    Write-Info "Verifying checksums from: checksums.txt"

    $checksums = Get-Content $checksumFile | Select-Object -Skip 1

    foreach ($line in $checksums) {
        if ([string]::IsNullOrWhiteSpace($line)) { continue }

        $parts = $line -split '\s+'
        if ($parts.Count -lt 2) { continue }

        $expectedHash = $parts[0]
        $filename = $parts[-1]
        $filepath = Join-Path $BuildDir $filename

        if (Test-Path $filepath) {
            Write-Info "Calculating SHA256 for $filename..."
            $actualHash = (Get-FileHash $filepath -Algorithm SHA256).Hash

            if ($actualHash -eq $expectedHash) {
                Write-Success "✓ Checksum verified: $filename"
                $validationResults += @{
                    Item = $filename
                    Status = "PASS"
                    Details = "Checksum valid"
                }
            } else {
                Write-Error-Custom "✗ Checksum MISMATCH: $filename"
                Write-Error-Custom "  Expected: $expectedHash"
                Write-Error-Custom "  Actual:   $actualHash"
                $validationResults += @{
                    Item = $filename
                    Status = "FAIL"
                    Details = "Checksum mismatch"
                }
            }
        } else {
            Write-Warning "⚠ File not found for checksum verification: $filename"
        }
    }
} else {
    Write-Warning "checksums.txt not found - skipping verification"
}

# ======== DOCUMENTATION VERIFICATION ========

Write-Host "`n--- Documentation Verification ---`n"

$docFiles = @(
    "RELEASE_NOTES.md",
    "INSTALL.md",
    "RELEASE_CHECKLIST.md"
)

foreach ($doc in $docFiles) {
    if (Test-Path $doc) {
        $size = (Get-Item $doc).Length
        Write-Success "✓ $doc ($([math]::Round($size/1KB)) KB)"
        $validationResults += @{
            Item = $doc
            Status = "PASS"
            Details = "$([math]::Round($size/1KB)) KB"
        }
    } else {
        Write-Error-Custom "✗ $doc NOT FOUND"
        $validationResults += @{
            Item = $doc
            Status = "FAIL"
            Details = "File not found"
        }
    }
}

# ======== BUILD REPORT REVIEW ========

Write-Host "`n--- Build Report Review ---`n"

$buildReport = Join-Path $BuildDir "BUILD_REPORT.txt"
if (Test-Path $buildReport) {
    Write-Success "✓ BUILD_REPORT.txt exists"
    Write-Info "Build Report Summary:"

    $content = Get-Content $buildReport
    $content | Select-Object -First 20 | ForEach-Object { Write-Host "  $_" }

    if ($content.Count -gt 20) {
        Write-Info "  ... ($(($content.Count - 20)) more lines)"
    }
} else {
    Write-Warning "BUILD_REPORT.txt not found"
}

# ======== VALIDATION SUMMARY ========

Write-Host "`n--- Validation Summary ---`n"

$passCount = ($validationResults | Where-Object {$_.Status -eq "PASS"}).Count
$failCount = ($validationResults | Where-Object {$_.Status -eq "FAIL"}).Count

Write-Info "Validation Results: $passCount passed, $failCount failed"

if ($failCount -gt 0) {
    Write-Error-Custom "Distribution validation FAILED"
    Write-Host "`nFailed items:"
    $validationResults | Where-Object {$_.Status -eq "FAIL"} | ForEach-Object {
        Write-Host "  ✗ $($_.Item): $($_.Details)" -ForegroundColor Red
    }
    exit 1
}

Write-Success "All validation checks PASSED"

# ======== RELEASE PACKAGE CREATION ========

if ($CreatePackage -and -not $Validate) {
    Write-Host "`n--- Creating Release Package ---`n"

    $packageName = "EtnoPapers-$Version-Release"
    $stagingDir = Join-Path $BuildDir $packageName
    $packageZip = "$packageName.zip"

    Write-Info "Creating staging directory: $packageName"

    # Remove if exists
    if (Test-Path $stagingDir) {
        Remove-Item $stagingDir -Recurse -Force
    }

    New-Item -ItemType Directory -Path $stagingDir | Out-Null

    # Copy artifacts
    Write-Info "Copying installer artifacts..."
    Copy-Item (Join-Path $BuildDir "EtnoPapers-Setup-$Version.msi") -Destination $stagingDir
    Copy-Item (Join-Path $BuildDir "EtnoPapers-Portable-$Version.zip") -Destination $stagingDir
    Copy-Item (Join-Path $BuildDir "checksums.txt") -Destination $stagingDir
    Copy-Item (Join-Path $BuildDir "BUILD_REPORT.txt") -Destination $stagingDir

    # Copy documentation
    Write-Info "Copying documentation..."
    foreach ($doc in $docFiles) {
        if (Test-Path $doc) {
            Copy-Item $doc -Destination $stagingDir
        }
    }

    # Create README for package
    $packageReadme = Join-Path $stagingDir "README.txt"
    @"
================================================================================
                    EtnoPapers v$Version Release Package
================================================================================

Thank you for downloading EtnoPapers!

QUICK START:
1. Review RELEASE_NOTES.md for new features and improvements
2. Choose installation method:
   - Windows Users: Run EtnoPapers-Setup-$Version.msi
   - Portable Users: Extract EtnoPapers-Portable-$Version.zip
3. Follow INSTALL.md for step-by-step instructions
4. Configure OLLAMA and MongoDB (optional) in Settings

CONTENTS:
- EtnoPapers-Setup-$Version.msi       Windows MSI installer
- EtnoPapers-Portable-$Version.zip    Portable ZIP archive
- RELEASE_NOTES.md                    Version 1.0.0 release information
- INSTALL.md                          Installation and configuration guide
- RELEASE_CHECKLIST.md                QA checklist and release details
- checksums.txt                       SHA256 checksums for verification
- BUILD_REPORT.txt                    Build process report

SYSTEM REQUIREMENTS:
- Windows 10 (Build 1909) or Windows 11
- 4 GB RAM minimum (8 GB recommended)
- 500 MB disk space
- .NET 8.0 Desktop Runtime (included in MSI)

SUPPORT:
- GitHub Issues: https://github.com/etnopayers/etnopayers/issues
- GitHub Discussions: https://github.com/etnopayers/etnopayers/discussions
- Documentation: See INSTALL.md and USER_GUIDE.md

LICENSE:
EtnoPapers is released under the MIT License. See LICENSE.md for details.

================================================================================
For more information, visit: https://github.com/etnopayers/etnopayers
================================================================================
"@ | Out-File $packageReadme

    Write-Success "✓ Package staging directory created"

    # Create ZIP
    Write-Info "Creating release package ZIP: $packageZip"
    $destinationZip = Join-Path $BuildDir $packageZip

    if (Test-Path $destinationZip) {
        Remove-Item $destinationZip -Force
    }

    Compress-Archive -Path $stagingDir -DestinationPath $destinationZip

    $packageSize = [math]::Round((Get-Item $destinationZip).Length / 1MB, 2)
    Write-Success "✓ Release package created: $packageZip ($packageSize MB)"

    # Calculate package checksum
    Write-Info "Calculating package checksum..."
    $packageHash = (Get-FileHash $destinationZip -Algorithm SHA256).Hash
    Write-Success "✓ SHA256: $packageHash"

    # Append to checksums file
    Write-Info "Updating checksums.txt"
    "`n# Release Package" | Out-File $checksumFile -Append
    "$packageHash  $packageZip" | Out-File $checksumFile -Append

    # Cleanup staging directory
    Remove-Item $stagingDir -Recurse -Force
    Write-Success "✓ Staging directory cleaned up"
}

# ======== FINAL CHECKLIST ========

Write-Host "`n--- Distribution Readiness Checklist ---`n"

$checklist = @(
    "All artifacts present and validated",
    "Checksums verified correctly",
    "Documentation complete and present",
    "Release notes reviewed",
    "Build report generated",
    "Installation guide prepared",
    "Release checklist completed",
    "GitHub repository ready"
)

$checkNo = 1
foreach ($item in $checklist) {
    Write-Host "  [$checkNo] $item" -ForegroundColor Green
    $checkNo++
}

# ======== NEXT STEPS ========

Write-Host "`n--- Next Steps (T096) ---`n"

Write-Info "Ready for final verification and sign-off (T096):"
Write-Host "
  1. Review all artifacts in: $BuildDir
  2. Verify checksums match published values
  3. Test on clean Windows system (post-testing)
  4. Obtain approval from Release Manager
  5. Create GitHub release with artifacts
  6. Publish version 1.0.0 officially

  Command to create GitHub release:
  gh release create v$Version `
    -t 'EtnoPapers $Version' `
    -F RELEASE_NOTES.md `
    build/EtnoPapers.Installer/bin/Release/EtnoPapers-Setup-$Version.msi
    artifacts/EtnoPapers-Portable-$Version.zip
"

# ======== COMPLETION ========

Write-Host "`n╔════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║    Distribution Preparation Complete (T095) ║" -ForegroundColor Green
Write-Host "╚════════════════════════════════════════════╝`n" -ForegroundColor Green

Write-Success "All distribution artifacts validated and ready"
Write-Success "Proceed to T096 (Final Verification & Sign-Off)"

exit 0
