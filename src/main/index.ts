/**
 * Main process entry point for EtnoPapers
 * Handles Electron app lifecycle, window management, and IPC handler registration
 */

import { app, BrowserWindow, Menu } from 'electron';
import path from 'path';
import http from 'http';
import fs from 'fs';
import { isDev } from './utils/isDev';
import { registerConfigHandlers } from './ipc/configHandlers';
import { LoggerService } from './services/LoggerService';

let mainWindow: BrowserWindow | null = null;
const logger = new LoggerService();
let httpServer: http.Server | null = null;

// Disable GPU to avoid crash on some systems (must be before app ready)
app.disableHardwareAcceleration();

/**
 * Start a simple HTTP server to serve renderer files
 */
function startHttpServer(port: number = 3000): Promise<string> {
  return new Promise((resolve, reject) => {
    const rendererDir = path.join(__dirname, '..', 'renderer');
    console.log(`[HTTP Server] Renderer directory: ${rendererDir}`);
    console.log(`[HTTP Server] Directory exists: ${fs.existsSync(rendererDir)}`);

    httpServer = http.createServer((req, res) => {
      let filePath = path.join(rendererDir, req.url === '/' ? 'index.html' : req.url);

      // Prevent directory traversal
      const normalizedPath = path.normalize(filePath);
      if (!normalizedPath.startsWith(rendererDir)) {
        console.warn(`[HTTP Server] Blocked directory traversal: ${req.url}`);
        res.writeHead(403);
        res.end('Forbidden');
        return;
      }

      console.log(`[HTTP Server] GET ${req.url}`);

      fs.readFile(filePath, (err, data) => {
        if (err) {
          console.warn(`[HTTP Server] File not found: ${filePath}`);
          res.writeHead(404);
          res.end('Not Found');
          return;
        }

        // Set correct content type
        const ext = path.extname(filePath);
        let contentType = 'text/plain';
        if (ext === '.html') contentType = 'text/html';
        else if (ext === '.css') contentType = 'text/css';
        else if (ext === '.js') contentType = 'application/javascript';
        else if (ext === '.json') contentType = 'application/json';
        else if (ext === '.png') contentType = 'image/png';
        else if (ext === '.svg') contentType = 'image/svg+xml';

        console.log(`[HTTP Server] Serving ${ext}: ${req.url}`);
        res.writeHead(200, {
          'Content-Type': contentType,
          'Access-Control-Allow-Origin': '*',
          'Cross-Origin-Opener-Policy': 'same-origin',
        });
        res.end(data);
      });
    });

    httpServer.listen(port, 'localhost', () => {
      console.log(`[HTTP Server] Started on http://localhost:${port}`);
      resolve(`http://localhost:${port}`);
    });

    httpServer.on('error', (error) => {
      console.error(`[HTTP Server] Error:`, error);
      reject(error);
    });
  });
}

/**
 * Create main application window
 */
async function createWindow(): Promise<void> {
  mainWindow = new BrowserWindow({
    width: 1200,
    height: 800,
    minWidth: 800,
    minHeight: 600,
    webPreferences: {
      preload: path.join(__dirname, 'preload', 'index.js'),
      contextIsolation: true,
      nodeIntegration: false,
      sandbox: false,
      webSecurity: false,
    },
    icon: path.join(__dirname, '..', '..', 'assets', 'icon.png'),
  });

  let startUrl: string;
  if (isDev) {
    startUrl = 'http://localhost:5173'; // Vite dev server
  } else {
    // Use local HTTP server in production
    startUrl = await startHttpServer(3000);
  }

  mainWindow.loadURL(startUrl);

  if (isDev) {
    mainWindow.webContents.openDevTools();
  }

  mainWindow.on('closed', () => {
    mainWindow = null;
  });

  // Log window creation
  logger.info('Main window created');
}

/**
 * Register all IPC handlers
 */
function registerIpcHandlers(): void {
  logger.info('Registering IPC handlers');
  registerConfigHandlers();
  // Additional handlers will be registered here (PDF, OLLAMA, etc.)
}

/**
 * Create application menu
 */
function createMenu(): void {
  const template: (Electron.MenuItemConstructorOptions | Electron.MenuItem)[] = [
    {
      label: 'File',
      submenu: [
        {
          label: 'Exit',
          accelerator: 'CmdOrCtrl+Q',
          click: () => {
            app.quit();
          },
        },
      ],
    },
    {
      label: 'Edit',
      submenu: [
        { role: 'undo' },
        { role: 'redo' },
        { type: 'separator' },
        { role: 'cut' },
        { role: 'copy' },
        { role: 'paste' },
      ],
    },
    {
      label: 'View',
      submenu: [
        { role: 'reload' },
        { role: 'forceReload' },
        { role: 'toggleDevTools' },
        { type: 'separator' },
        { role: 'resetZoom' },
        { role: 'zoomIn' },
        { role: 'zoomOut' },
        { type: 'separator' },
        { role: 'togglefullscreen' },
      ],
    },
    {
      label: 'Help',
      submenu: [
        {
          label: 'About',
          click: () => {
            logger.info('About dialog clicked');
            // Will show about modal in renderer
          },
        },
      ],
    },
  ];

  const menu = Menu.buildFromTemplate(template);
  Menu.setApplicationMenu(menu);
}

/**
 * App event handlers
 */
app.on('ready', async () => {
  logger.info('EtnoPapers app starting');
  try {
    registerIpcHandlers();
    createMenu();
    await createWindow();
  } catch (error) {
    logger.error('Failed to initialize app', error);
    app.quit();
  }
});

app.on('window-all-closed', () => {
  // On macOS, applications stay active until the user quits explicitly
  if (process.platform !== 'darwin') {
    app.quit();
  }
});

app.on('activate', () => {
  // On macOS, re-create window when dock icon is clicked and app has no windows
  if (mainWindow === null) {
    createWindow();
  }
});

app.on('before-quit', () => {
  logger.info('EtnoPapers app closing');
  // Close HTTP server if it's running
  if (httpServer) {
    httpServer.close();
  }
});

/**
 * Handle uncaught exceptions
 */
process.on('uncaughtException', (error) => {
  logger.error('Uncaught exception', error);
});

/**
 * Log app path
 */
logger.info(`App data directory: ${app.getPath('userData')}`);
logger.info(`Log file path: ${logger.getLogFilePath()}`);
