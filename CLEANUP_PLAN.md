# Plano de Limpeza - Arquitetura Standalone

**Data**: 2025-11-28
**Objetivo**: Remover arquivos e diretórios obsoletos na nova arquitetura PyInstaller + Electron
**Política de Documentação**: Manter TUDO em `docs/` - nenhum arquivo será movido

---

## 📊 Análise Completa

### 🟢 MANTER (Necessários para Arquitetura Standalone)

#### Root Level
- ✅ `.gitignore` - Controle de versão
- ✅ `.github/` - CI/CD (GitHub Actions)
- ✅ `.env.example` - Template de configuração
- ✅ `build.spec` - Configuração PyInstaller
- ✅ `build-linux.sh`, `build-macos.sh`, `build-windows.bat` - Scripts de build
- ✅ `pytest.ini` - Configuração de testes
- ✅ `README.md` - Documentação principal
- ✅ `CLAUDE.md` - Instruções para IA

#### Backend
- ✅ `backend/` - Aplicação FastAPI (completa)

#### Frontend
- ✅ `frontend/` - React (completo)

#### Documentação
- ✅ `docs/` - MANTER TUDO (user guides, architecture, etc.)
- ✅ `specs/main/` - Especificações ativas

---

## 🔴 REMOVER (Arquivos Obsoletos)

### 1. Scaffolding de Desenvolvimento
```
❌ etnopaper/                      (branch abandonado)
❌ .benchmarks/                    (diretório vazio)
```

### 2. Especificações Arquivadas
```
❌ specs/archive/                  (v1.0 histórica)
❌ specs/checklists/               (checklists de desenvolvimento)
```

### 3. Arquivo Sensível em Git
```
❌ .env.local                      (credenciais - remover do git)
```

### 4. Relatórios Históricos
```
❌ PHASE1_COMPLETION.md            (mover para docs/)
❌ RELEASES.md                     (mover para docs/)
```

---

## 📋 Plano de Execução

### Fase 1: Remover do Git
```bash
# Arquivo sensível
git rm --cached .env.local

# Scaffolding
git rm -r etnopaper/
git rm -r .benchmarks/
git rm -r specs/archive/
git rm -r specs/checklists/

# Relatórios (mover para docs/)
git mv PHASE1_COMPLETION.md docs/history/PHASE1_COMPLETION.md
git mv RELEASES.md docs/RELEASES.md

git commit -m "chore: Clean up obsolete development scaffolding and move reports to docs/"
```

---

## ✅ Estrutura Final

```
etnopapers/
├── .github/                 ✅ CI/CD
├── backend/                 ✅ FastAPI
├── frontend/                ✅ React
├── docs/                    ✅ Documentação (TUDO aqui)
├── specs/main/              ✅ Especificações ativas
├── build.spec               ✅ PyInstaller
├── build-*.sh/bat           ✅ Build scripts
├── README.md                ✅ Principal
└── CLAUDE.md                ✅ AI instructions
```

**Removido**:
- etnopaper/, .benchmarks/, specs/archive/, specs/checklists/
- .env.local (do git)

**Mantido em docs/**:
- Tudo

---

**Status**: Pronto para execução
