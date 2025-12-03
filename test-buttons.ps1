Write-Host "=== EtnoPapers Button Test ===" -ForegroundColor Cyan

$exePath = "H:\git\etnopapers\src\EtnoPapers.UI\bin\Release\net8.0-windows\EtnoPapers.UI.exe"
$logPath = "$env:APPDATA\EtnoPapers\exception.log"

# Limpar logs antigos
Remove-Item $logPath -ErrorAction SilentlyContinue

Write-Host "Iniciando aplicação..."
$proc = Start-Process -FilePath $exePath -PassThru

Write-Host "PID: $($proc.Id)"
Write-Host ""
Write-Host "Aguarde 5 segundos e tente clicar nos botões na janela da aplicação..."
Write-Host "Depois pressione ENTER aqui para verificar se os cliques foram registrados"

Read-Host "Pressione ENTER"

Write-Host ""
Write-Host "=== Verificando logs ===" -ForegroundColor Yellow

if (Test-Path $logPath) {
    Write-Host "Logs encontrados:"
    Get-Content $logPath
} else {
    Write-Host "Nenhum erro foi registrado (é um bom sinal!)"
}

# Verificar arquivo de log da aplicação
$appLog = "$env:APPDATA\EtnoPapers\logs\app-*.log"
Write-Host ""
Write-Host "=== Logs da Aplicação ===" -ForegroundColor Yellow
Get-ChildItem (Split-Path $appLog) -Filter "app-*.log" -ErrorAction SilentlyContinue | ForEach-Object {
    Write-Host ""
    Write-Host "Arquivo: $($_.Name)"
    Get-Content $_.FullName
}

# Fechar aplicação
Write-Host ""
Write-Host "Encerrando aplicação..."
$proc.CloseMainWindow()
Start-Sleep -Seconds 2
if (-not $proc.HasExited) {
    $proc.Kill()
}

Write-Host "Teste concluído"
