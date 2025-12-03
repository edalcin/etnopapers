Write-Host "=== EtnoPapers Launcher ===" -ForegroundColor Cyan
Write-Host ""

$exePath = "H:\git\etnopapers\src\EtnoPapers.UI\bin\Release\net8.0-windows\EtnoPapers.UI.exe"
$logPath = "$env:APPDATA\EtnoPapers\exception.log"

Write-Host "Arquivo: $exePath"
Write-Host "Logs: $logPath"
Write-Host ""

if (-not (Test-Path $exePath)) {
    Write-Host "ERRO: Arquivo executável não encontrado!" -ForegroundColor Red
    exit 1
}

Write-Host "Iniciando aplicação..." -ForegroundColor Yellow

try {
    $proc = Start-Process -FilePath $exePath -PassThru -ErrorAction Stop

    Write-Host "Processo iniciado com PID: $($proc.Id)" -ForegroundColor Green
    Write-Host ""
    Write-Host "A janela da aplicação deve aparecer em alguns segundos..."
    Write-Host ""

    Start-Sleep -Seconds 3

    # Verificar se o processo ainda está rodando
    $proc.Refresh()

    if ($proc.HasExited) {
        Write-Host "ERRO: Processo saiu inesperadamente" -ForegroundColor Red
        Write-Host "Código de saída: $($proc.ExitCode)" -ForegroundColor Red
        Write-Host ""

        # Mostrar logs de erro
        if (Test-Path $logPath) {
            Write-Host "=== Erros Registrados ===" -ForegroundColor Magenta
            Get-Content $logPath
        }
    } else {
        Write-Host "Aplicação rodando normalmente" -ForegroundColor Green
        Write-Host ""
        Write-Host "Pressione CTRL+C para encerrar ou feche a janela da aplicação"

        # Aguardar indefinidamente
        while (-not $proc.HasExited) {
            Start-Sleep -Seconds 1
        }

        Write-Host ""
        Write-Host "Aplicação encerrada" -ForegroundColor Yellow
    }
}
catch {
    Write-Host "ERRO ao iniciar aplicação: $($_.Exception.Message)" -ForegroundColor Red
}
