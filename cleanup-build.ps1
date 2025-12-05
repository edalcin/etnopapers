# Remove only untracked build artifacts carefully
Write-Host 'Removendo bin/Release raiz se existir...'
if (Test-Path 'H:\git\etnopapers\bin') {
    Remove-Item -Path 'H:\git\etnopapers\bin' -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host 'Removido: bin raiz'
}

if (Test-Path 'H:\git\etnopapers\publish') {
    Remove-Item -Path 'H:\git\etnopapers\publish' -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host 'Removido: publish raiz'
}

if (Test-Path 'H:\git\etnopapers\artifacts') {
    Remove-Item -Path 'H:\git\etnopapers\artifacts' -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host 'Removido: artifacts'
}

if (Test-Path 'H:\git\etnopapers\test-portable') {
    Remove-Item -Path 'H:\git\etnopapers\test-portable' -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host 'Removido: test-portable'
}

Write-Host 'Removendo net8.0-windows (mantendo publish)...'
if (Test-Path 'H:\git\etnopapers\src\EtnoPapers.UI\bin\Release\net8.0-windows') {
    Remove-Item -Path 'H:\git\etnopapers\src\EtnoPapers.UI\bin\Release\net8.0-windows' -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host 'Removido: net8.0-windows'
}

Write-Host ''
Write-Host 'Verificando publish que deve ser mantido:'
if (Test-Path 'H:\git\etnopapers\src\EtnoPapers.UI\bin\Release\publish') {
    Get-ChildItem -Path 'H:\git\etnopapers\src\EtnoPapers.UI\bin\Release\publish' | Select-Object Name
} else {
    Write-Host 'AVISO: publish nao existe!'
}

Write-Host 'Limpeza concluida com sucesso!'
