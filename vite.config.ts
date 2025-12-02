import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import path from 'path'

export default defineConfig({
  plugins: [react()],
  base: './',
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
      '@shared': path.resolve(__dirname, './src/shared'),
      '@main': path.resolve(__dirname, './src/main'),
      '@renderer': path.resolve(__dirname, './src/renderer'),
    },
  },
  root: path.resolve(__dirname, './src/renderer'),
  build: {
    target: 'ES2022',
    outDir: path.resolve(__dirname, './dist/renderer'),
    sourcemap: true,
    emptyOutDir: true,
  },
  server: {
    port: 5173,
  },
})
