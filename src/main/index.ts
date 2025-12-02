/**
 * Main process entry point for EtnoPapers
 * Handles Electron app lifecycle, window management, and IPC handler registration
 */

import { app, BrowserWindow, Menu } from 'electron';
import path from 'path';
import { isDev } from './utils/isDev';
import { registerConfigHandlers } from './ipc/configHandlers';
import { LoggerService } from './services/LoggerService';

let mainWindow: BrowserWindow | null = null;
const logger = new LoggerService();

/**
 * Create main application window
 */
function createWindow(): void {
  mainWindow = new BrowserWindow({
    width: 1200,
    height: 800,
    minWidth: 800,
    minHeight: 600,
    webPreferences: {
      preload: path.join(__dirname, 'preload', 'index.js'),
      contextIsolation: true,
      nodeIntegration: false,
      sandbox: true,
    },
    icon: path.join(__dirname, '..', '..', 'assets', 'icon.png'),
  });

  const startUrl = isDev
    ? 'http://localhost:5173' // Vite dev server
    : `file://${path.join(__dirname, '..', 'renderer', 'index.html')}`; // Production build

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
    createWindow();
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
