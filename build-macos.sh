#!/bin/bash
# ============================================================================
#  Etnopapers - Build Script for macOS
#
#  Builds standalone app bundle for macOS (Etnopapers.app)
#
#  Requirements:
#    - Python 3.11+ installed
#    - Node.js 18+ installed
#    - pip and npm in PATH
#
#  Usage:
#    chmod +x build-macos.sh
#    ./build-macos.sh
#
#  Output:
#    dist/Etnopapers.app (macOS app bundle)
# ============================================================================

set -e  # Exit on error

echo ""
echo "============================================================================"
echo "  Etnopapers - Build for macOS"
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
echo "  Step 3/3: Building App Bundle with PyInstaller"
echo "============================================================================"
echo ""

echo "Building Etnopapers.app..."
pyinstaller build.spec --clean
if [ $? -ne 0 ]; then
    echo "ERROR: PyInstaller build failed"
    exit 1
fi

echo ""
echo "============================================================================"
echo "  BUILD SUCCESSFUL!"
echo "============================================================================"
echo ""
echo "  App bundle created: dist/Etnopapers.app"
echo ""
echo "  To run:"
echo "    1. Ensure Ollama is installed and running"
echo "    2. Open dist/Etnopapers.app"
echo "    3. Configure MongoDB URI on first run"
echo ""
echo "  NOTE: On first run, macOS may ask for permission."
echo "        Go to System Preferences > Security & Privacy to allow."
echo ""
echo "============================================================================"
echo ""
