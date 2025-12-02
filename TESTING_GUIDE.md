# EtnoPapers - Local Testing Guide

## Quick Start

### Prerequisites
- Node.js 18+ (with pnpm package manager)
- OLLAMA installed and running locally (for AI extraction)
- Windows 10+ (for native desktop features)

### 1. Install Dependencies
```bash
cd H:\git\etnopapers
pnpm install
```

### 2. Start OLLAMA Service
In a separate terminal:
```bash
ollama serve
# Listens on http://localhost:11434
# Default model: llama2
```

To verify OLLAMA is running:
```bash
curl http://localhost:11434/api/tags
```

### 3. Run Dev Server (UI Testing)
```bash
pnpm run dev
# Opens Vite dev server on http://localhost:5173
# Hot module reload enabled
# Good for rapid UI development
```

### 4. Build for Production
```bash
pnpm run build
# Outputs to: dist/renderer/
# Ready for Electron packaging
```

## Testing Workflow

### Test 1: UI Navigation & Layout
1. Run `pnpm run dev`
2. Open http://localhost:5173 in browser
3. Test navigation:
   - Click sidebar items (Home, Upload, Records, Settings, About)
   - Verify sidebar collapse/expand works
   - Check responsive layout on different window sizes
4. Verify page content loads for each route

### Test 2: Settings Configuration
1. Navigate to Settings page
2. Scroll to OLLAMA Configuration section
3. Verify current values match defaults:
   - URL: `http://localhost:11434`
   - Model: `llama2`
4. Click "Test OLLAMA Connection" button
   - Should show success message (assuming OLLAMA is running)
   - If OLLAMA not running, should show connection error
5. Scroll to MongoDB Configuration (optional)
   - Leave empty for local-only testing
   - Or provide MongoDB Atlas URI for cloud sync testing
6. Click "Save" buttons - should show success messages
7. Verify settings persist by refreshing page

### Test 3: Home Page
1. Navigate to Home page
2. Verify welcome message displays
3. Test quick action buttons:
   - "Upload PDF" button links to Upload page
   - "View Records" button links to Records page

### Test 4: Upload Page (UI Only)
1. Navigate to Upload page
2. Verify drag-and-drop zone displays
3. (Full extraction requires Electron app - see below)

### Test 5: Records Page
1. Navigate to Records page
2. Should display message "No records yet. Upload a PDF to start."
3. (Full record listing requires Electron app with storage - see below)

### Test 6: About Page
1. Navigate to About page
2. Verify application information displays
3. Check version is 1.0.0

### Test 7: Styling & Responsive Design
1. Open DevTools (F12)
2. Test different breakpoints:
   - Mobile (375px)
   - Tablet (768px)
   - Desktop (1920px)
3. Verify Tailwind CSS styling applies correctly
4. Check dark theme variables work

## Running Full Electron App

Currently the Electron main process needs additional configuration. To run the full app:

### Step 1: Build Renderer
```bash
pnpm run build
```

### Step 2: Compile Main Process
The main process TypeScript files need to be compiled. This requires setting up a build step for `src/main/**/*.ts`:

```bash
# Install esbuild (not in current package.json)
pnpm add -D esbuild

# Add build script to package.json:
"build:main": "esbuild src/main/index.ts --bundle --platform=node --target=node20 --outfile=dist/main/index.js"

# Build main process
pnpm run build:main
```

### Step 3: Run Electron App
```bash
# Requires main.js to exist at dist/main/index.js
electron dist/main/index.js
```

## Testing PDF Extraction (Full App)

Once Electron app is running:

1. **Navigate to Upload page**
2. **Prepare a PDF file** with the following:
   - Extractable text (not scanned/OCR)
   - Ethnobotanical content ideal (mentions plants, communities, etc.)
   - Example: A real academic paper on traditional plant use
3. **Drag & Drop the PDF** onto the drop zone
4. **Watch extraction progress**:
   - Progress bar shows 0-100%
   - Real-time status updates
5. **Verify extraction success**:
   - Success message appears
   - Record appears in Records page
   - Data includes: title, authors, year, abstract, species, location

## Testing Record Management

### View Records
1. Navigate to Records page
2. Should see list of extracted articles
3. Each record shows:
   - Title
   - Authors
   - Publication year
   - Sync status badge (Local/Pending/Synced)

### Search/Filter Records
1. (Requires implementing filter UI in T075)
2. Search by title, author, or year
3. Filter by sync status

### MongoDB Sync (Optional)

1. **Configure MongoDB**:
   - Go to Settings page
   - Enter MongoDB Atlas URI or local server
   - Click "Test Connection"

2. **Upload Records to Cloud**:
   - Select records in Records page
   - Click "Sync" button
   - Monitor sync progress
   - Records should move from "Local" to "Synced" status

## Data Locations

When running Electron app, data is stored in:
- **Windows**: `%APPDATA%\EtnoPapers\`

Subdirectories:
- `data/articles.json` - Local record database
- `etnopapers-config.json` - Application configuration
- `logs/etnopapers.log` - Application logs
- `logs/error.log` - Error logs

## Troubleshooting

### Vite Dev Server Issues

**Error: "port 5173 already in use"**
```bash
# Kill process using port 5173
netstat -ano | findstr :5173
taskkill /PID <PID> /F

# Or use different port
pnpm run dev -- --port 5174
```

**Error: "Module not found"**
- Clear node_modules and reinstall:
```bash
rm -r node_modules pnpm-lock.yaml
pnpm install
```

### OLLAMA Connection Issues

**Error: "Failed to connect to OLLAMA"**
1. Verify OLLAMA is running: `ollama serve`
2. Test connection: `curl http://localhost:11434/api/tags`
3. Verify Settings shows correct URL (http://localhost:11434)
4. Check firewall isn't blocking localhost:11434

**"Model not found" error**
1. Verify model is installed: `ollama list`
2. Pull default model: `ollama pull llama2`
3. Update Settings to use available model

### PDF Extraction Issues

**"Failed to extract text from PDF"**
- PDF might be scanned (image-only)
- Use native digital PDF with text layers
- Try extracting text in PDF viewer first

**"Validation failed: Missing required fields"**
- OLLAMA might not be extracting all required fields
- Try with a more detailed academic paper
- Check OLLAMA model can handle ethnobotanical text

### Build Issues

**Error: "tailwindcss PostCSS plugin"**
- Ensure `@tailwindcss/postcss` is installed:
```bash
pnpm add -D @tailwindcss/postcss
```
- Update `postcss.config.js` to use `@tailwindcss/postcss` plugin

**TypeScript compilation errors**
- Verify `tsconfig.json` paths match your setup
- Check all imports use correct path aliases (@, @shared, etc.)

## Performance Testing

### Storage Performance
```bash
# Default limit: 1000 records
# Test adding many records quickly
# Monitor Records page responsiveness
# Should maintain <200ms interaction time
```

### Extraction Performance
```bash
# Upload multiple PDFs sequentially
# Monitor CPU/memory usage
# Check OLLAMA model isn't maxing out resources
# Expected extraction time: 30-120 seconds per PDF (depends on size)
```

## CI/CD Notes

For automated testing, these commands should pass:
```bash
pnpm install          # Install dependencies
pnpm run build        # Build React app
pnpm run lint         # Check code style
# pnpm run test       # Run unit tests (not yet configured)
# pnpm run test:e2e   # Run E2E tests (not yet configured)
```

## Next Steps

To complete the MVP and enable Electron testing:

1. **Add main process build script** to compile TypeScript
2. **Update package.json** with proper Electron entry point
3. **Configure file picker** for PDF file selection
4. **Implement remaining Phase 4 UI components** (modals, filters)
5. **Add E2E tests** with Playwright
6. **Create Windows installer** with electron-builder

See `specs/main/tasks.md` for full implementation roadmap.
