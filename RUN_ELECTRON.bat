@echo off
REM EtnoPapers - Rodar Aplicação Electron Nativa
REM Script para Windows (CMD)
REM Pressione Ctrl+C para encerrar a aplicação

echo ========================================
echo EtnoPapers - Aplicacao Nativa Desktop
echo ========================================
echo.

REM Verificar se está na pasta correta
if not exist "package.json" (
    echo ERRO: Execute este script na pasta do projeto
    echo Pasta esperada: H:\git\etnopapers
    pause
    exit /b 1
)

REM Verificar se node_modules existe
if not exist "node_modules" (
    echo Instalando dependencias...
    call pnpm install
    if errorlevel 1 (
        echo ERRO: Falha ao instalar dependencias
        pause
        exit /b 1
    )
)

echo.
echo Compilando aplicacao Electron...
echo.

REM Compilar main process (output: dist/main/index.cjs)
call pnpm build:main
if errorlevel 1 (
    echo ERRO: Falha ao compilar main process
    pause
    exit /b 1
)

REM Compilar renderer (output: dist/renderer/)
echo.
echo Compilando React UI...
call pnpm build:renderer
if errorlevel 1 (
    echo ERRO: Falha ao compilar renderer
    pause
    exit /b 1
)

echo.
echo ========================================
echo Iniciando aplicacao nativa...
echo ========================================
echo.
echo Janela da aplicacao abrira em alguns segundos...
echo Pressione Ctrl+C para sair.
echo.

REM Rodar Electron direto via Node script
call node start-electron.js

pause
