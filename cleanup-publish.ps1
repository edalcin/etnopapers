# Kill any running EtnoPapers processes
Get-Process | Where-Object { $_.Name -eq 'EtnoPapers.UI' } | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 2

# Remove the exe file if it exists
Remove-Item 'H:\git\etnopapers\src\EtnoPapers.UI\bin\Release\publish\EtnoPapers.UI.exe' -Force -ErrorAction SilentlyContinue
Write-Host 'Killed any running processes and cleaned publish directory'
