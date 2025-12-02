#!/usr/bin/env node

/**
 * EtnoPapers - Electron App Launcher
 * Executes Electron directly without pnpm issues
 */

import { spawn } from 'child_process';
import path from 'path';
import fs from 'fs';
import { fileURLToPath } from 'url';

// Get paths
const __filename = fileURLToPath(import.meta.url);
const projectRoot = path.dirname(__filename);
const electronPath = path.join(projectRoot, 'node_modules', '.bin', 'electron');
const mainPath = path.join(projectRoot, 'dist', 'main', 'index.cjs');

console.log('========================================');
console.log('EtnoPapers - Aplicação Nativa Desktop');
console.log('========================================\n');

// Check if main file exists
if (!fs.existsSync(mainPath)) {
  console.error('\n❌ ERRO: Arquivo principal não encontrado!');
  console.error(`Esperado em: ${mainPath}`);
  console.error('\nExecute primeiro: pnpm build:main');
  process.exit(1);
}

// Check if renderer files exist
const rendererPath = path.join(projectRoot, 'dist', 'renderer', 'index.html');
if (!fs.existsSync(rendererPath)) {
  console.error('\n❌ ERRO: Arquivos do renderer não encontrados!');
  console.error(`Esperado em: ${rendererPath}`);
  console.error('\nExecute primeiro: pnpm build:renderer');
  process.exit(1);
}

console.log('✅ Arquivos compilados encontrados');
console.log(`   Main: ${mainPath}`);
console.log(`   Renderer: ${rendererPath}\n`);

// Try direct Electron executable
const electronExe = path.join(projectRoot, 'node_modules', 'electron', 'dist', 'electron.exe');
if (fs.existsSync(electronExe)) {
  console.log(`Iniciando com: ${electronExe}\n`);

  const child = spawn(electronExe, [mainPath], {
    stdio: 'inherit',
    cwd: projectRoot,
  });

  child.on('error', (error) => {
    console.error('❌ Erro ao iniciar Electron:', error);
    process.exit(1);
  });

  child.on('exit', (code) => {
    console.log('\n✅ Aplicação fechada');
    process.exit(code);
  });
} else {
  console.error('❌ Electron executável não encontrado');
  console.error(`Procurou em: ${electronExe}`);
  console.error('\nTente: pnpm install --force');
  process.exit(1);
}
