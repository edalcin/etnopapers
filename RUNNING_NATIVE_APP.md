# EtnoPapers - Guia de Execução da Aplicação Nativa Windows

Este guia descreve como executar a aplicação EtnoPapers como uma aplicação nativa Windows usando Electron.

## ⚠️ Pré-requisitos

Antes de executar a aplicação, certifique-se de ter:

- **Node.js 20+**: [Download](https://nodejs.org/)
- **pnpm 10.16.1+**: `npm install -g pnpm`
- **Electron v28**: Instalado automaticamente via `pnpm install`
- **Python 3.9+** (opcional): Para construir módulos nativos se necessário

## 🚀 Execução Rápida

### Opção 1: Script Windows (Recomendado para Usuários)

#### CMD (Prompt de Comando)
```bash
RUN_ELECTRON.bat
```

#### PowerShell
```powershell
.\RUN_ELECTRON.ps1
```

Esses scripts automaticamente:
1. Verificam dependências (`node_modules`)
2. Compilam o processo principal TypeScript → CommonJS
3. Compilam a interface React
4. Iniciam a aplicação nativa

### Opção 2: Linha de Comando (Desenvolvedor)

```bash
# Instalar dependências (uma única vez)
pnpm install

# Compilar código
pnpm build

# Iniciar aplicação
pnpm start
```

Ou com launcher direto:

```bash
# Compilar e rodar com script launcher (contorna problemas pnpm)
pnpm build && node start-electron.js
```

## 📋 Estrutura da Compilação

### Processo Principal (Main Process)
- **Fonte**: `src/main/index.ts` (TypeScript)
- **Compilado para**: `dist/main/index.cjs` (CommonJS)
- **Builder**: esbuild
- **Tamanho**: ~2.4 MB

Comando de compilação:
```bash
pnpm build:main
```

### Interface (Renderer)
- **Fonte**: `src/renderer/` (React + TypeScript)
- **Compilado para**: `dist/renderer/` (HTML + JS + CSS)
- **Builder**: Vite
- **Tamanho**: ~228 KB (71 KB gzip)

Comando de compilação:
```bash
pnpm build:renderer
```

### Ambos
```bash
pnpm build
```

## 🔧 Modo Desenvolvimento (Com Hot Reload)

Para desenvolvimento com recompilação automática:

```bash
pnpm dev
```

Isso executa em paralelo:
- **Dev Server Vite**: http://localhost:5173 (React UI com hot reload)
- **esbuild Watch**: Recompila TypeScript do processo principal automaticamente

Quando atualizar código, a interface React recarrega instantaneamente!

## 📝 Scripts Disponíveis

| Script | Descrição |
|--------|-----------|
| `pnpm dev` | Modo desenvolvimento com hot reload |
| `pnpm dev:renderer` | Só dev server Vite (http://localhost:5173) |
| `pnpm dev:main` | Só compilação esbuild com watch |
| `pnpm build` | Compila para produção (main + renderer) |
| `pnpm build:main` | Compila só processo principal |
| `pnpm build:renderer` | Compila só interface |
| `pnpm start` | Inicia aplicação (requer build prévio) |
| `pnpm start:dev` | Dev + start (não recomendado) |
| `pnpm lint` | Verifica código (ESLint) |
| `pnpm format` | Formata código (Prettier) |

## 🐛 Troubleshooting

### Problema: "Electron failed to install correctly"

**Solução**: Executar manualmente o script de instalação:
```bash
node node_modules/electron/install.js
```

### Problema: Não consegue encontrar Electron binários

**Solução**: Reinstalar com force:
```bash
pnpm install --force
```

### Problema: "Cannot find module" ou erros de importação

**Solução**: Limpar e reinstalar:
```bash
rm -r node_modules pnpm-lock.yaml
pnpm install
pnpm build
pnpm start
```

### Problema: Porta 5173 já em uso (desenvolvimento)

**Solução**: Matar processo na porta:
```bash
netstat -ano | findstr :5173
taskkill /PID <PID> /F
```

### Problema: Aplicação inicia mas não carrega UI

**Certifique-se de que**:
1. Build do renderer completou: `ls dist/renderer/index.html`
2. Build do main completou: `ls dist/main/index.cjs`
3. Não há erros no console da aplicação

### Problema: Erros de OLLAMA durante operações

**Certifique-se de que**:
1. OLLAMA está rodando: `ollama serve`
2. Modelo está disponível: `ollama pull llama2`
3. Endpoint configurado em Configurações → OLLAMA URI

## 📊 Performance

- **Tempo de compilação (dev)**: ~200ms (Vite) + ~100ms (esbuild)
- **Tempo de compilação (build)**: ~1s total
- **Tamanho bundle renderer**: 228 KB (71 KB comprimido)
- **Tamanho bundle main**: 2.4 MB (CommonJS bundled)
- **Tempo de startup**: ~3-5 segundos (dependendo do sistema)
- **Tempo de load UI**: <500ms (produção)

## 🔒 Segurança

A aplicação usa:
- **Context Isolation**: Ativado (sandbox seguro)
- **Node Integration**: Desativado (sem acesso direto ao Node)
- **Preload Script**: Gerencia comunicação IPC segura
- **Process Sandbox**: Ativado (isolamento de processo)

## 📁 Arquivos Importantes

```
etnopapers/
├── start-electron.js           # Launcher direto (ESM)
├── RUN_ELECTRON.bat            # Script Windows CMD
├── RUN_ELECTRON.ps1            # Script Windows PowerShell
├── package.json                # Configuração (type: "module")
├── tsconfig.json               # TypeScript
├── vite.config.ts              # Vite bundler
├── electron-builder.config.js  # Configuração do instalador
│
├── src/
│   ├── main/                   # Processo principal (Electron)
│   ├── renderer/               # Interface React
│   ├── preload/                # Script de contexto isolado
│   └── shared/                 # Código compartilhado
│
└── dist/
    ├── main/index.cjs          # Processo compilado (CommonJS)
    └── renderer/               # Interface compilada
```

## 🎯 Fluxo de Execução

```
RUN_ELECTRON.bat / RUN_ELECTRON.ps1
        ↓
pnpm build:main (src/main/index.ts → dist/main/index.cjs)
        ↓
pnpm build:renderer (src/renderer/ → dist/renderer/)
        ↓
node start-electron.js (ou pnpm start)
        ↓
Electron.exe carrega dist/main/index.cjs
        ↓
Cria janela e carrega:
  - Em dev: http://localhost:5173
  - Em produção: file://dist/renderer/index.html
```

## 💡 Dicas

1. **Desenvolvimento**: Use `pnpm dev` para hot reload automático
2. **Teste rápido**: `pnpm build && node start-electron.js` (mais rápido que pnpm start)
3. **Debug**: Pressione F12 na aplicação para abrir DevTools
4. **Linting**: Execute `pnpm lint` antes de commitar
5. **Tipo-verificação**: TypeScript é verificado durante build/dev

## 🚪 Próximos Passos

1. **Executar a aplicação**: Use `RUN_ELECTRON.bat` ou `RUN_ELECTRON.ps1`
2. **Testar funcionalidades**:
   - Ir para "Upload" e fazer upload de PDF
   - Configurar OLLAMA URI em "Configurações"
   - Revisar registros extraídos
   - Sincronizar com MongoDB (se configurado)

3. **Criar instalador Windows**:
   ```bash
   pnpm dist
   ```
   Isso cria um instalador `.exe` em `dist/` (requer Electron configurado corretamente)

## 📞 Suporte

Para problemas:
1. Verifique os logs em `~/.config/EtnoPapers/` (local dos dados)
2. Execute `pnpm lint` para verificar erros de código
3. Consulte COMMANDS.md para mais detalhes
4. Abra issue em: https://github.com/edalcin/etnopapers/issues

---

**Versão**: 1.0.0
**Última atualização**: 2025-12-02
**Plataforma**: Windows 10/11 (64-bit)
