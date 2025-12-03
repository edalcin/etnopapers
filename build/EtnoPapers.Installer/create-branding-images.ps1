#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Create branding images for WiX installer

.DESCRIPTION
    Generates minimal valid BMP files for the EtnoPapers installer.
    Creates Banner.bmp (500x63) and Dialog.bmp (262x392).
    These are minimal white backgrounds - replace with proper branded images.

.PARAMETER BannerColor
    Banner background color (default: #F0F0F0 - light gray)

.PARAMETER DialogColor
    Dialog background color (default: #FFFFFF - white)

.EXAMPLE
    .\create-branding-images.ps1
    .\create-branding-images.ps1 -BannerColor "#2E75B6" -DialogColor "#FFFFFF"

.NOTES
    Requires ImageMagick or built-in approach using .NET graphics
#>

param(
    [string]$BannerColor = "#F0F0F0",
    [string]$DialogColor = "#FFFFFF"
)

Write-Host "Creating branding images for EtnoPapers installer..." -ForegroundColor Cyan

$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$bannerPath = Join-Path $scriptPath "Banner.bmp"
$dialogPath = Join-Path $scriptPath "Dialog.bmp"

try {
    # Add System.Drawing assembly
    Add-Type -AssemblyName System.Drawing

    # Create Banner Image (500x63)
    Write-Host "Creating Banner.bmp (500x63)..." -ForegroundColor Yellow

    $bannerBitmap = New-Object System.Drawing.Bitmap(500, 63)
    $bannerGraphics = [System.Drawing.Graphics]::FromImage($bannerBitmap)

    # Fill background with color - use white
    $bannerGraphics.Clear([System.Drawing.Color]::White)

    # Draw light gray background
    $grayBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(240, 240, 240))
    $bannerGraphics.FillRectangle($grayBrush, 0, 0, 500, 63)

    # Add simple text
    $font = New-Object System.Drawing.Font("Segoe UI", 20, [System.Drawing.FontStyle]::Bold)
    $textBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(46, 117, 182))
    $bannerGraphics.DrawString("EtnoPapers 1.0.0", $font, $textBrush, 20, 15)

    $grayBrush.Dispose()
    $textBrush.Dispose()
    $font.Dispose()
    $bannerGraphics.Dispose()
    $bannerBitmap.Save($bannerPath)
    $bannerBitmap.Dispose()

    Write-Host "✓ Banner.bmp created successfully" -ForegroundColor Green

    # Create Dialog Image (262x392)
    Write-Host "Creating Dialog.bmp (262x392)..." -ForegroundColor Yellow

    $dialogBitmap = New-Object System.Drawing.Bitmap(262, 392)
    $dialogGraphics = [System.Drawing.Graphics]::FromImage($dialogBitmap)

    # Fill background with white
    $dialogGraphics.Clear([System.Drawing.Color]::White)

    # Add simple text
    $smallFont = New-Object System.Drawing.Font("Segoe UI", 16, [System.Drawing.FontStyle]::Bold)
    $textBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(46, 117, 182))
    $dialogGraphics.DrawString("EtnoPapers", $smallFont, $textBrush, 20, 150)

    $textBrush.Dispose()
    $smallFont.Dispose()
    $dialogGraphics.Dispose()
    $dialogBitmap.Save($dialogPath)
    $dialogBitmap.Dispose()

    Write-Host "✓ Dialog.bmp created successfully" -ForegroundColor Green

    Write-Host "`n✅ All branding images created!" -ForegroundColor Green
    Write-Host "Note: These are minimal placeholder images." -ForegroundColor Cyan
    Write-Host "Replace them with proper branded images for your installer." -ForegroundColor Cyan

} catch {
    Write-Host "❌ Error creating images: $_" -ForegroundColor Red
    Write-Host $_.ScriptStackTrace -ForegroundColor Red
    exit 1
}
