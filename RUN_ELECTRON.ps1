#!/usr/bin/env pwsh
# EtnoPapers - Rodar Aplicação Electron Nativa
# Script para Windows PowerShell
# Pressione Ctrl+C para encerrar a aplicação

Write-Host "========================================" -ForegroundColor Green
Write-Host "EtnoPapers - Aplicacao Nativa Desktop" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""

# Verificar se está na pasta correta
if (-not (Test-Path "package.json")) {
    Write-Host "ERRO: Execute este script na pasta do projeto" -ForegroundColor Red
    Write-Host "Pasta esperada: H:\git\etnopapers" -ForegroundColor Yellow
    Read-Host "Pressione Enter para sair"
    exit 1
}

# Verificar se node_modules existe
if (-not (Test-Path "node_modules")) {
    Write-Host "Instalando dependencias..." -ForegroundColor Yellow
    & pnpm install
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERRO: Falha ao instalar dependencias" -ForegroundColor Red
        Read-Host "Pressione Enter para sair"
        exit 1
    }
}

Write-Host ""
Write-Host "Compilando aplicacao Electron..." -ForegroundColor Yellow
Write-Host ""

# Compilar main process (output: dist/main/index.cjs)
& pnpm build:main
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERRO: Falha ao compilar main process" -ForegroundColor Red
    Read-Host "Pressione Enter para sair"
    exit 1
}

# Compilar renderer (output: dist/renderer/)
Write-Host ""
Write-Host "Compilando React UI..." -ForegroundColor Yellow
& pnpm build:renderer
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERRO: Falha ao compilar renderer" -ForegroundColor Red
    Read-Host "Pressione Enter para sair"
    exit 1
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "Iniciando aplicacao nativa..." -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Janela da aplicacao abrira em alguns segundos..." -ForegroundColor Cyan
Write-Host "Pressione Ctrl+C para sair." -ForegroundColor Yellow
Write-Host ""

# Rodar Electron direto via Node script
& node start-electron.js

Read-Host "Pressione Enter para sair"
