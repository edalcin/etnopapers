# Test script for compiled EtnoPapers application
$appPath = "H:\git\etnopapers\src\EtnoPapers.UI\bin\Release\net8.0-windows\EtnoPapers.UI.exe"
$logDir = Join-Path $env:APPDATA "EtnoPapers\logs"

Write-Host "=== EtnoPapers Compiled Application Test ===" -ForegroundColor Cyan
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
    $proc = Start-Process -FilePath $appPath -PassThru -WindowStyle Normal -ErrorAction Stop

    Write-Host "Application started with PID: $($proc.Id)" -ForegroundColor Green

    # Wait for app to initialize
    Write-Host "Waiting 10 seconds for initialization..." -ForegroundColor Cyan
    for ($i = 0; $i -lt 10; $i++) {
        Start-Sleep -Seconds 1
        if ($proc.HasExited) {
            Write-Host "Application exited early at second $($i+1)" -ForegroundColor Red
            break
        }
        Write-Host "  $($i+1)s..." -NoNewline
    }
    Write-Host ""

    # Check if process is still running
    if ($proc.HasExited) {
        Write-Host "PROBLEM: Application exited with code: $($proc.ExitCode)" -ForegroundColor Red
        Write-Host ""

        # Check logs
        Write-Host "Checking log files..." -ForegroundColor Yellow
        if (Test-Path $logDir) {
            $logFiles = Get-ChildItem $logDir -Filter "*.log" 2>$null
            if ($logFiles) {
                Write-Host "Log files found:" -ForegroundColor Cyan
                foreach ($log in $logFiles) {
                    Write-Host ""
                    Write-Host "=== $($log.Name) ===" -ForegroundColor Magenta
                    $content = Get-Content $log.FullName
                    Write-Host $content
                }
            } else {
                Write-Host "No log files found in $logDir" -ForegroundColor Yellow
            }
        } else {
            Write-Host "Log directory does not exist: $logDir" -ForegroundColor Yellow
        }
    } else {
        Write-Host "Application is running successfully!" -ForegroundColor Green
        Write-Host "Memory usage: $([math]::Round($proc.WorkingSet / 1MB, 2)) MB" -ForegroundColor Cyan
        Write-Host "Threads: $($proc.Threads.Count)" -ForegroundColor Cyan

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

    # List all log files for review
    Write-Host ""
    Write-Host "=== All Log Files ===" -ForegroundColor Cyan
    if (Test-Path $logDir) {
        Get-ChildItem $logDir -Filter "*.log" | Select-Object Name, Length, LastWriteTime | Format-Table -AutoSize
    } else {
        Write-Host "No log directory found" -ForegroundColor Yellow
    }
}
catch {
    Write-Host "ERROR: Failed to start application" -ForegroundColor Red
    Write-Host "Exception: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "StackTrace: $($_.Exception.StackTrace)" -ForegroundColor Red
}

Write-Host ""
Write-Host "Test completed" -ForegroundColor Cyan
