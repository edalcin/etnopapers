@echo off
REM Script para publicar o build Release e copiar para local único
REM Uso: publish-release.bat

echo.
echo ================================
echo  EtnoPapers Release Build Script
echo ================================
echo.

cd /d "%~dp0"

echo [1/3] Compilando projeto...
cd src\EtnoPapers.UI
dotnet build -c Release
if %ERRORLEVEL% NEQ 0 (
    echo Erro na compilacao!
    pause
    exit /b 1
)

echo.
echo [2/3] Publicando...
dotnet publish -c Release --force
if %ERRORLEVEL% NEQ 0 (
    echo Erro na publicacao!
    pause
    exit /b 1
)

echo.
echo [3/3] Copiando para local unico...
powershell -Command "Copy-Item -Path 'bin\Release\net8.0-windows\win-x64\publish\*' -Destination 'bin\Release\publish\' -Recurse -Force" >nul 2>&1

echo.
echo ================================
echo Sucesso! Build em:
echo H:\git\etnopapers\src\EtnoPapers.UI\bin\Release\publish\EtnoPapers.UI.exe
echo ================================
echo.

pause
