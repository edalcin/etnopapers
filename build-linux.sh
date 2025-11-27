#!/bin/bash
# ============================================================================
#  Etnopapers - Build Script for Linux
#
#  Builds standalone executable for Linux (etnopapers)
#
#  Requirements:
#    - Python 3.11+ installed
#    - Node.js 18+ installed
#    - pip and npm in PATH
#
#  Usage:
#    chmod +x build-linux.sh
#    ./build-linux.sh
#
#  Output:
#    dist/etnopapers (standalone executable)
# ============================================================================

set -e  # Exit on error

echo ""
echo "============================================================================"
echo "  Etnopapers - Build for Linux"
echo "============================================================================"
echo ""

# Check Python
if ! command -v python3 &> /dev/null; then
    echo "ERROR: Python 3 not found. Please install Python 3.11+"
    exit 1
fi
echo "[OK] Python installed: $(python3 --version)"

# Check Node.js
if ! command -v node &> /dev/null; then
    echo "ERROR: Node.js not found. Please install Node.js 18+"
    exit 1
fi
echo "[OK] Node.js installed: $(node --version)"

# Check pip
if ! command -v pip3 &> /dev/null; then
    echo "ERROR: pip not found"
    exit 1
fi
echo "[OK] pip installed"

echo ""
echo "============================================================================"
echo "  Step 1/3: Building Frontend"
echo "============================================================================"
echo ""

if [ ! -d "frontend" ]; then
    echo "ERROR: frontend/ directory not found"
    exit 1
fi

cd frontend

echo "Installing frontend dependencies..."
npm install
if [ $? -ne 0 ]; then
    echo "ERROR: npm install failed"
    exit 1
fi

echo "Building frontend..."
npm run build
if [ $? -ne 0 ]; then
    echo "ERROR: npm build failed"
    exit 1
fi

echo "[OK] Frontend built successfully"
cd ..

echo ""
echo "============================================================================"
echo "  Step 2/3: Installing Backend Dependencies"
echo "============================================================================"
echo ""

echo "Installing Python dependencies..."
pip3 install -r backend/requirements.txt
if [ $? -ne 0 ]; then
    echo "ERROR: pip install failed"
    exit 1
fi

echo "[OK] Backend dependencies installed"

echo ""
echo "============================================================================"
echo "  Step 3/3: Building Executable with PyInstaller"
echo "============================================================================"
echo ""

echo "Building etnopapers executable..."
pyinstaller build.spec --clean
if [ $? -ne 0 ]; then
    echo "ERROR: PyInstaller build failed"
    exit 1
fi

# Make executable
chmod +x dist/etnopapers

echo ""
echo "============================================================================"
echo "  BUILD SUCCESSFUL!"
echo "============================================================================"
echo ""
echo "  Executable created: dist/etnopapers"
echo ""
echo "  To run:"
echo "    1. Ensure Ollama is installed and running"
echo "    2. chmod +x dist/etnopapers (if not already executable)"
echo "    3. ./dist/etnopapers"
echo "    4. Configure MongoDB URI on first run"
echo ""
echo "============================================================================"
echo ""
