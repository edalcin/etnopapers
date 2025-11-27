# Phase 0: Research & Technology Decisions

**Date**: 2025-11-27
**Status**: Complete – All technology choices researched and documented
**Purpose**: Resolve all NEEDS CLARIFICATION items and document best practices for desktop application bundling with embedded Python backend.

---

## 1. Desktop Framework: Electron vs. Tauri vs. PyQt

### Decision: **Electron** (with Tauri as fallback)

**Rationale**:
- **Existing React codebase**: Electron uses the exact same React + TypeScript stack already in `frontend/`. Zero rewrite needed.
- **Cross-platform consistency**: Single codebase builds identical UI on Windows/macOS/Linux (Chromium-based).
- **Native file dialogs & system integration**: Built-in APIs for file picker, system notifications, app menus.
- **Community & stability**: Massive ecosystem (VS Code, Discord, Slack all use Electron). Well-documented patterns for bundling.
- **Backend integration**: Simple IPC (Inter-Process Communication) or HTTP. Can communicate with embedded Python FastAPI server via localhost.

**Alternatives Considered**:
1. **PyQt/PySide** (Python desktop framework)
   - **Rejected**: Would require rewriting entire React frontend in Python/Qt. Too much refactoring for existing codebase.

2. **Tauri** (Rust-based, lightweight alternative to Electron)
   - **Considered but deferred**: Tauri is 50-70% smaller executables (~50-80 MB vs. 150 MB). However, it has smaller ecosystem and would require Rust integration layer for Python backend. Complexity not worth the 70 MB savings for this phase. Can migrate in future if size becomes critical issue.
   - **Gap**: No mature Tauri + Python FastAPI patterns yet; would require custom IPC boilerplate.

3. **Qt for Python (PyQt/PySide)**
   - **Rejected**: Requires rewriting React components in C++ or Python. Huge refactoring scope.

**Implementation Path**:
- Electron acts as **native container** only (window management, file dialogs, system tray)
- Frontend React app runs inside Electron's embedded Chromium
- **No Electron-specific code in React** – app communicates via standard HTTP to backend (http://localhost:8000)
- Backend (Python FastAPI) runs as subprocess alongside Electron main process
- Electron main process spawns Python executable on app startup, terminates on app close

---

## 2. Python Bundling: PyInstaller vs. cx_Freeze vs. others

### Decision: **PyInstaller 6.x**

**Rationale**:
- **Already used in current codebase**: `build-windows.bat`, `build-macos.sh`, `build-linux.sh` scripts already use PyInstaller.
- **Handles frontend assets**: Can bundle `frontend/dist/` as embedded static files.
- **Cross-platform builds**: Single spec file works for Windows, macOS, Linux with minimal platform-specific hooks.
- **Proven for complex projects**: Successfully bundles FastAPI, PyMongo, pdfplumber, Ollama communication.
- **Size optimization**: With UPX compression and careful dependency exclusion, achievable target of <200 MB per executable.

**Alternatives Considered**:
1. **cx_Freeze** (Python standard approach)
   - **Rejected**: Older, less community support, larger executables (often 50-100 MB larger).

2. **Poetry + PyInstaller** (dependency management)
   - **Considered**: Could improve dependency tracking. Not blocking; existing `requirements.txt` sufficient.

3. **Nuitka** (Python compiler to C++)
   - **Rejected**: Slower compile time, overkill for this use case, harder to debug.

**Implementation Path**:
- Refactor existing PyInstaller spec (`build/pyinstaller.spec`) to:
  1. Bundle `frontend/dist/` directory into executable as `_internal/frontend_assets/`
  2. Add hooks for PyMongo, pdfplumber, instructor (Ollama)
  3. Platform-specific code signing (macOS `.app` packaging, Windows certificate)
- Keep existing build scripts with minor updates

---

## 3. Backend-Frontend Communication: HTTP vs. IPC vs. Subprocess RPC

### Decision: **HTTP (localhost:8000)**

**Rationale**:
- **Simplest integration**: Frontend already calls HTTP APIs (`/api/articles`, `/api/extract/metadata`). Zero changes needed.
- **No platform-specific IPC overhead**: IPC (Inter-Process Communication) requires platform-specific code (Windows named pipes, Unix sockets). HTTP works identically on all three OSes.
- **Existing error handling**: All error handling, auth, CORS already implemented for HTTP.
- **Debugging**: Standard browser DevTools work seamlessly. Can test backend independently with curl/Postman.
- **Loose coupling**: Backend and frontend can be deployed separately (relevant for future server-based deployment).

**Alternatives Considered**:
1. **Electron IPC** (native process communication)
   - **Rejected**: Adds Electron-specific code to React components. Reduces portability (can't easily move frontend to web later).

2. **WebSocket** (for real-time)
   - **Considered but deferred**: Not needed for MVP. Can add in Phase 2 if real-time updates required.

3. **gRPC** (typed RPC)
   - **Rejected**: Overkill for this scale. HTTP + OpenAPI sufficient.

**Implementation Path**:
- Backend: FastAPI already configured to serve on `http://localhost:8000`
- Frontend: Environment variable `REACT_APP_API_URL=http://localhost:8000` (set by launcher.py before starting)
- Electron launcher.py:
  1. Spawn Python subprocess with `--host 127.0.0.1 --port 8000`
  2. Wait for server to respond (health check loop)
  3. Open browser/Electron window to `http://localhost:8000`
  4. On app close: kill Python subprocess

---

## 4. Configuration Persistence: Environment Variables vs. JSON File vs. Registry

### Decision: **Hybrid Approach - Environment Variables + JSON Config File**

**Rationale**:
- **MONGO_URI as environment variable**: Set by launcher before spawning FastAPI. Allows Docker/deployment flexibility.
- **User-writable config file**: Store MongoDB URI locally so app remembers setting across sessions.
- **Cross-platform**: Works on Windows, macOS, Linux without registry/system-specific code.
- **User-friendly**: Researchers can edit `~/.etnopapers/config.json` directly if needed.

**Configuration File Structure**:
```json
{
  "version": "3.0.0",
  "mongoUri": "mongodb://localhost:27017/etnopapers",
  "ollamaUrl": "http://localhost:11434",
  "appVersion": "3.0.0",
  "lastUpdated": "2025-11-27T12:00:00Z"
}
```

**Location**:
- **Windows**: `C:\Users\{username}\AppData\Local\Etnopapers\config.json`
- **macOS**: `~/Library/Application Support/Etnopapers/config.json`
- **Linux**: `~/.config/etnopapers/config.json` (XDG Base Directory spec)

**Implementation Path**:
- `launcher.py` checks for config file on startup
- If missing: show setup dialog (native file picker + text input)
- On setup complete: write config file + environment variables
- On subsequent launches: read config, set environment variables, start FastAPI

---

## 5. Frontend Build & Bundling: Vite vs. Webpack

### Decision: **Vite** (already in use)

**Rationale**:
- **Already integrated**: Frontend is already using Vite (from existing codebase).
- **Fast dev server**: Existing team workflow unaffected.
- **Small, optimized build output**: `npm run build` produces `frontend/dist/` with minimal size (~2-3 MB gzipped).
- **Static asset bundling**: Vite naturally outputs static HTML/JS/CSS ready to embed.

**No changes needed** – keep existing Vite configuration.

**Build Pipeline**:
1. Frontend team runs `npm run build` → generates `frontend/dist/`
2. Build script copies `dist/` into `backend/_static/` directory
3. PyInstaller bundles entire `backend/` directory
4. Backend (FastAPI) serves `frontend/dist/` on GET `/` route

---

## 6. Testing Strategy: Unit vs. Integration vs. E2E

### Decision: **Tiered Testing Approach**

| Layer | Tool | Scope | Requirement |
|-------|------|-------|-------------|
| **Unit** | pytest (backend), Vitest (frontend) | Services, components, utilities | >80% code coverage |
| **Integration** | pytest + mongomock | API endpoints, PDF extraction flow | Critical paths (upload → extract → save) |
| **Desktop** | Electron test utilities | App startup, config dialog, Ollama health check | Smoke tests (app launches, config works) |
| **E2E** | Manual + documentation | Full workflow: download → configure → upload → save | Before release |

**Rationale**:
- Existing test infrastructure (pytest for backend, Vitest for frontend) stays.
- Integration tests verify API contract compliance.
- Desktop-specific tests verify Ollama connectivity and config persistence.
- E2E testing is manual (requires actual Ollama instance, MongoDB connection).

---

## 7. Ollama Integration: Direct HTTP vs. Python Client Library

### Decision: **Direct HTTP via `instructor` library**

**Rationale**:
- **Already in use**: Backend already uses `instructor` for structured Ollama output (Pydantic schema validation).
- **No Ollama SDK dependency**: Lighter weight than `ollama-py` package. Ollama supports standard OpenAI-compatible API.
- **Existing patterns**: `extraction_service.py` already calls Ollama via HTTP POST to `/v1/chat/completions`.

**No changes needed** – instructor library already configured.

---

## 8. Database Backup Format: MongoDB dump vs. Custom JSON

### Decision: **mongodump binary output in ZIP**

**Rationale**:
- **Native MongoDB tool**: `mongodump` is standard MongoDB backup utility. Users can restore with `mongorestore`.
- **Portable**: ZIP archive portable across systems.
- **Integrity checking**: Can verify checksum before download completion.
- **Existing implementation**: `backup_service.py` already implements this.

**Alternative Considered**:
- **JSON export**: More readable, but requires custom schema mapping. `mongodump` is simpler.

**No changes needed** – existing implementation sufficient.

---

## 9. Electron Entry Point: Main Process Architecture

### Decision: **Spawn Python FastAPI as subprocess**

**Implementation**:
```javascript
// Electron main.js
const { spawn } = require('child_process');
const path = require('path');

const pythonPath = path.join(__dirname, 'backend', 'launcher.py');
const pythonProcess = spawn('python', [pythonPath], {
  detached: false,  // Kill subprocess when parent dies
  stdio: ['ignore', 'pipe', 'pipe']  // Capture stdout/stderr for logging
});

// Wait for HTTP server to be ready (health check)
const waitForServer = async () => {
  for (let i = 0; i < 30; i++) {  // Try for 30 seconds
    try {
      const response = await fetch('http://localhost:8000/health');
      if (response.ok) return true;
    } catch (e) {
      await new Promise(r => setTimeout(r, 1000));
    }
  }
  throw new Error('FastAPI server did not start');
};

// Create Electron window
const createWindow = () => {
  const mainWindow = new BrowserWindow({
    width: 1400,
    height: 900,
    webPreferences: { nodeIntegration: false }
  });
  mainWindow.loadURL('http://localhost:8000');
};

app.on('ready', async () => {
  try {
    await waitForServer();
    createWindow();
  } catch (error) {
    dialog.showErrorBox('Startup Error', `Failed to start: ${error.message}`);
    app.quit();
  }
};
```

---

## 10. Code Signing & Distribution Channels

### Decision: **Self-signed (Windows), Notarization-ready (macOS), Unsigned (Linux)**

**Rationale**:
- **Windows**: Code signing for enterprise distribution (can add later). For now, Windows Defender may warn on first run (expected behavior).
- **macOS**: Apple requires notarization for distribution outside App Store. Build script should support this (defer implementation if timeline-constrained).
- **Linux**: No code signing needed. Publish on GitHub Releases.

**Not required for MVP** – can add security hardening in Phase 2.

---

## Phase 0 Conclusion

All NEEDS CLARIFICATION items resolved:

| Item | Decision | Rationale Summary |
|------|----------|-------------------|
| Desktop Framework | Electron | Leverages existing React, cross-platform, native integration |
| Python Bundling | PyInstaller 6.x | Already in codebase, proven for complex projects |
| Backend-Frontend Communication | HTTP (localhost) | Simplest, uses existing API infrastructure |
| Configuration | JSON + env vars | Cross-platform, user-editable, supports Docker |
| Frontend Build | Vite | Already integrated, minimal changes |
| Testing | Tiered (unit + integration + smoke) | Balanced coverage vs. effort |
| Ollama Integration | instructor library + HTTP | Already in use, no changes needed |
| Database Backup | mongodump + ZIP | Standard, portable, no custom code needed |
| Electron Architecture | Subprocess spawning | Clean separation, easy testing/debugging |
| Distribution | Self-signed (v1), notarization-ready (v1) | Good enough for MVP; harden later |

**Ready for Phase 1 (Design & Contracts)**.
