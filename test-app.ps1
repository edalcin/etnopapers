# Test script for portable EtnoPapers application
$appPath = "H:\git\etnopapers\test-portable\publish\EtnoPapers.UI.exe"
$logDir = Join-Path $env:APPDATA "EtnoPapers\logs"

Write-Host "=== EtnoPapers Portable Application Test ===" -ForegroundColor Cyan
Write-Host "Application Path: $appPath"
Write-Host "Log Directory: $logDir"
Write-Host ""

# Verify file exists
if (-not (Test-Path $appPath)) {
    Write-Host "ERROR: Application executable not found!" -ForegroundColor Red
    exit 1
}

Write-Host "Launching application..." -ForegroundColor Yellow

# Start the application with error handling
try {
    $proc = Start-Process -FilePath $appPath -PassThru -WindowStyle Normal

    Write-Host "Application started with PID: $($proc.Id)" -ForegroundColor Green

    # Wait for app to initialize
    Write-Host "Waiting 8 seconds for initialization..." -ForegroundColor Cyan
    Start-Sleep -Seconds 8

    # Check if process is still running
    if ($proc.HasExited) {
        Write-Host "PROBLEM: Application exited with code: $($proc.ExitCode)" -ForegroundColor Red
        Write-Host ""

        # Check logs
        Write-Host "Checking log files..." -ForegroundColor Yellow
        $logFiles = Get-ChildItem $logDir -Filter "*.log" 2>$null
        if ($logFiles) {
            Write-Host "Log files found:" -ForegroundColor Cyan
            foreach ($log in $logFiles) {
                Write-Host ""
                Write-Host "=== $($log.Name) ===" -ForegroundColor Magenta
                Get-Content $log.FullName -Tail 50
            }
        } else {
            Write-Host "No log files found" -ForegroundColor Yellow
        }
    } else {
        Write-Host "Application is running successfully!" -ForegroundColor Green
        Write-Host "Memory usage: $([math]::Round($proc.WorkingSet / 1MB, 2)) MB" -ForegroundColor Cyan

        # Ask user input
        Write-Host ""
        Write-Host "Press ENTER to close the application..." -ForegroundColor Yellow
        Read-Host

        # Close gracefully
        $proc.CloseMainWindow()
        Start-Sleep -Seconds 2

        if (-not $proc.HasExited) {
            $proc.Kill()
            Write-Host "Application killed" -ForegroundColor Yellow
        } else {
            Write-Host "Application closed gracefully" -ForegroundColor Green
        }
    }
}
catch {
    Write-Host "ERROR: Failed to start application" -ForegroundColor Red
    Write-Host "Exception: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "Test completed" -ForegroundColor Cyan
