# Advanced debugging script for EtnoPapers
$appPath = "H:\git\etnopapers\src\EtnoPapers.UI\bin\Debug\net8.0-windows\EtnoPapers.UI.exe"

Write-Host "=== EtnoPapers Debug Execution ===" -ForegroundColor Cyan
Write-Host "Path: $appPath"
Write-Host ""

# Try running with environment variable to show debug output
$env:COMPlus_LogFacility = 1
$env:COMPlus_LogLevel = 3
$env:COMPlus_LogToConsole = 1

# Create a wrapper batch file to capture output
$wrapperScript = @"
@echo off
cd /d "H:\git\etnopapers\src\EtnoPapers.UI\bin\Debug\net8.0-windows"
EtnoPapers.UI.exe 2>&1
echo Exit code: %ERRORLEVEL%
pause
"@

$wrapperPath = "$env:TEMP\etnopapers-wrapper.bat"
Set-Content -Path $wrapperPath -Value $wrapperScript

Write-Host "Running with debug output capture..." -ForegroundColor Yellow
Write-Host ""

# Try starting with cmd.exe to capture output better
$output = & cmd.exe /c $wrapperPath 2>&1 | Tee-Object -FilePath "$env:APPDATA\EtnoPapers\debug-output.txt"
Write-Host ""
Write-Host "Output:"
Write-Host $output

# Check for Windows event log entries
Write-Host ""
Write-Host "=== Checking Windows Event Log ===" -ForegroundColor Cyan
$events = Get-EventLog -LogName Application -Newest 20 -ErrorAction SilentlyContinue | Where-Object { $_.Source -like "*EtnoPapers*" -or $_.Message -like "*EtnoPapers*" }
if ($events) {
    foreach ($event in $events) {
        Write-Host ""
        Write-Host "Event: $($event.EventID)" -ForegroundColor Yellow
        Write-Host "Time: $($event.TimeGenerated)"
        Write-Host "Message: $($event.Message)"
    }
} else {
    Write-Host "No EtnoPapers events found in Application log"
}

# Check for crash dump files
Write-Host ""
Write-Host "=== Checking for Crash Dumps ===" -ForegroundColor Cyan
$crashDumps = Get-ChildItem "$env:APPDATA\EtnoPapers" -Filter "*.dmp" -ErrorAction SilentlyContinue
if ($crashDumps) {
    Write-Host "Found crash dumps:"
    foreach ($dump in $crashDumps) {
        Write-Host "  $($dump.Name) - $($dump.Length) bytes"
    }
} else {
    Write-Host "No crash dumps found"
}

# List all files in appdata
Write-Host ""
Write-Host "=== EtnoPapers AppData Contents ===" -ForegroundColor Cyan
Get-ChildItem "$env:APPDATA\EtnoPapers" -Recurse -ErrorAction SilentlyContinue | Select-Object FullName, Length, LastWriteTime | Format-Table -AutoSize

Write-Host ""
Write-Host "Debug completed. Check $env:APPDATA\EtnoPapers\debug-output.txt for full output"
