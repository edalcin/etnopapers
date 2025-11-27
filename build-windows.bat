@echo off
REM ============================================================================
REM  Etnopapers - Build Script for Windows
REM
REM  Builds standalone executable for Windows (Etnopapers.exe)
REM
REM  Requirements:
REM    - Python 3.11+ installed
REM    - Node.js 18+ installed
REM    - pip and npm in PATH
REM
REM  Usage:
REM    build-windows.bat
REM
REM  Output:
REM    dist/etnopapers.exe (standalone executable)
REM ============================================================================

echo.
echo ============================================================================
echo   Etnopapers - Build for Windows
echo ============================================================================
echo.

REM Check Python 3.12
set PYTHON_PATH=C:\Users\EDalcin\AppData\Local\Programs\Python\Python312\python.exe
if not exist "%PYTHON_PATH%" (
    echo ERROR: Python 3.12 not found at %PYTHON_PATH%
    echo Please install Python 3.12 from https://www.python.org/downloads/
    pause
    exit /b 1
)
"%PYTHON_PATH%" --version
echo [OK] Python 3.12 installed

REM Check Node.js
node --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: Node.js not found. Please install Node.js 18+
    pause
    exit /b 1
)
echo [OK] Node.js installed

REM Check npm
npm --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: npm not found
    pause
    exit /b 1
)
echo [OK] npm installed

echo.
echo ============================================================================
echo   Step 1/3: Building Frontend
echo ============================================================================
echo.

cd frontend
if errorlevel 1 (
    echo ERROR: frontend/ directory not found
    pause
    exit /b 1
)

echo Installing frontend dependencies...
call npm install
if errorlevel 1 (
    echo ERROR: npm install failed
    cd ..
    pause
    exit /b 1
)

echo Building frontend...
call npm run build
if errorlevel 1 (
    echo ERROR: npm build failed
    cd ..
    pause
    exit /b 1
)

echo [OK] Frontend built successfully
cd ..

echo.
echo ============================================================================
echo   Step 2/3: Installing Backend Dependencies
echo ============================================================================
echo.

echo Installing Python dependencies...
"%PYTHON_PATH%" -m pip install -r backend\requirements.txt
if errorlevel 1 (
    echo ERROR: pip install failed
    pause
    exit /b 1
)

echo [OK] Backend dependencies installed

echo.
echo ============================================================================
echo   Step 3/3: Building Executable with PyInstaller
echo ============================================================================
echo.

echo Building etnopapers.exe...
"%PYTHON_PATH%" -m PyInstaller build.spec --clean
if errorlevel 1 (
    echo ERROR: PyInstaller build failed
    pause
    exit /b 1
)

echo.
echo ============================================================================
echo   BUILD SUCCESSFUL!
echo ============================================================================
echo.
echo   Executable created: dist\etnopapers.exe
echo.
echo   To run:
echo     1. Ensure Ollama is installed and running
echo     2. Double-click dist\etnopapers.exe
echo     3. Configure MongoDB URI on first run
echo.
echo ============================================================================
echo.

pause
