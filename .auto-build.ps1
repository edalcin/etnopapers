# Auto-build script for EtnoPapers
# This script is called automatically after code changes
$ErrorActionPreference = 'Stop'

# Use the user's dotnet installation (has SDK access)
$dotnetExe = "C:\Users\EDalcin\.dotnet\dotnet.exe"

if (-not (Test-Path $dotnetExe)) {
    Write-Error "dotnet.exe not found"
    exit 1
}

# Set environment
$env:DOTNET_CLI_TELEMETRY_OPTOUT = "1"
$env:DOTNET_NOLOGO = "1"

# Change to project root
Set-Location "D:\git\etnopapers"

# Build Release configuration
Write-Host "Building EtnoPapers Release..." -ForegroundColor Cyan

& $dotnetExe build src/EtnoPapers.UI/EtnoPapers.UI.csproj -c Release --nologo -v quiet

if ($LASTEXITCODE -eq 0) {
    $exePath = "src\EtnoPapers.UI\bin\Release\net8.0-windows\EtnoPapers.UI.exe"
    if (Test-Path $exePath) {
        $fileInfo = Get-Item $exePath
        Write-Host "✓ Build successful: $($fileInfo.LastWriteTime)" -ForegroundColor Green
        exit 0
    }
}

Write-Host "✗ Build failed" -ForegroundColor Red
exit 1
