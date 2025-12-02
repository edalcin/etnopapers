/**
 * Preload script for Electron
 * Exposes safe IPC APIs to the renderer process
 */

import { contextBridge, ipcRenderer } from 'electron';

// Expose safe APIs to renderer
contextBridge.exposeInMainWorld('electron', {
  ipc: {
    send: (channel: string, data?: unknown) => ipcRenderer.send(channel, data),
    invoke: (channel: string, data?: unknown) => ipcRenderer.invoke(channel, data),
    on: (channel: string, listener: (event: any, ...args: any[]) => void) =>
      ipcRenderer.on(channel, listener),
    once: (channel: string, listener: (event: any, ...args: any[]) => void) =>
      ipcRenderer.once(channel, listener),
    removeListener: (channel: string, listener: (event: any, ...args: any[]) => void) =>
      ipcRenderer.removeListener(channel, listener),
  },
});
