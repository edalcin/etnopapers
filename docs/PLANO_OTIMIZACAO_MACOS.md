# Plano de Otimização do Build macOS - Etnopapers v2.1

**Data**: 2025-11-30
**Prioridade**: 🟡 ALTA
**Objetivo**: Criar app profissional para macOS com DMG Installer, code signing e suporte a múltiplas arquiteturas (Intel + Apple Silicon)

---

## 🎯 Objetivo Geral

Criar distribuição macOS profissional que:
1. Instala dependências (Python) separadamente
2. Distribui app via DMG Installer (padrão macOS)
3. Suporta Intel (x86_64) e Apple Silicon (arm64)
4. Assinado digitalmente (code signing)
5. Notarizado para passar verificação Gatekeeper
6. Otimizado para performance (startup rápido, pouca RAM)

---

## 🔴 Problemas Atuais (macOS)

Embora menos crítico que Windows, o build atual tem problemas:

- ⚠️ **Tamanho**: ~150-200 MB (grande para download)
- ⚠️ **Inicialização**: 5-10 segundos (lento para app nativo)
- ⚠️ **Memória**: 50-80% consumo inicial (alto)
- ⚠️ **Code Signing**: Não assinado (warnings do Gatekeeper)
- ⚠️ **Notarization**: Não notarizado (possível bloqueia em Sonoma+)
- ⚠️ **Arquitetura**: Apenas Intel, não suporta M1/M2/M3
- ⚠️ **Distribuição**: Apenas app bundle solto, sem installer formal

---

## ✅ Solução Proposta: DMG Installer Profissional

Paralelo ao Windows (MSI), criar DMG Installer para macOS:

### Arquitetura Nova

```
┌─────────────────────────────────────────────────┐
│  Distribuição Etnopapers v2.1 (macOS)           │
├─────────────────────────────────────────────────┤
│                                                 │
│  ┌──────────────────┐  ┌────────────────────┐ │
│  │ Etnopapers.dmg   │  │ Etnopapers-Intel   │ │
│  │ (DMG Installer)  │  │ (App Bundle solto) │ │
│  │ 30-40 MB         │  │ 60-80 MB           │ │
│  └──────────────────┘  └────────────────────┘ │
│         │                                      │
│         ├─ Contém Etnopapers-Universal.app    │
│         ├─ Contém Python 3.11 (opcional)      │
│         ├─ Contém Ollama info                 │
│         └─ Customizado para macOS             │
│                                                │
│  BUILD MATRIX (CI/CD):                         │
│  ├─ Intel (x86_64) → Etnopapers-Intel.dmg    │
│  ├─ Apple Silicon (arm64) → Etnopapers-ARM   │
│  └─ Universal (x86+arm) → Etnopapers.dmg     │
│                                                 │
│  CODE SIGNING & NOTARIZATION:                 │
│  ├─ Assinado com Developer Certificate        │
│  ├─ Notarizado via Apple                      │
│  └─ Stapled (offline verification)            │
│                                                 │
└─────────────────────────────────────────────────┘
```

### Comparação: Atual vs. Proposto

| Aspecto | Atual | Proposto | Melhoria |
|---------|-------|----------|----------|
| **Tamanho** | 150-200 MB | 40-60 MB | ✅ 60% menor |
| **Inicialização** | 5-10s | 2-4s | ✅ 60% rápido |
| **RAM** | 50-80% | 20-30% | ✅ 60% menos |
| **Code Signed** | ❌ | ✅ | ✅ Profissional |
| **Notarized** | ❌ | ✅ | ✅ Seguro |
| **Intel Suporte** | ✅ | ✅ | ✅ |
| **Apple Silicon** | ❌ | ✅ (arm64) | ✅ Nativo M1/M2/M3 |
| **Universal Binary** | ❌ | ✅ (opcional) | ✅ Compatibilidade |
| **DMG Installer** | ❌ | ✅ | ✅ Padrão macOS |

---

## 📋 Estratégia de Implementação

### Fase 1: Preparação (2-3 horas)

#### T1.1 - Analisar App Bundle Atual
- [ ] Medir tamanho atual (150-200 MB)
- [ ] Verificar módulos desnecessários
- [ ] Testar startup em Intel Mac
- [ ] Testar startup em Apple Silicon Mac
- [ ] Medir consumo RAM/CPU

#### T1.2 - Configurar Ferramentas macOS
- [ ] Instalar Xcode Command Line Tools (se não tiver)
- [ ] Verificar certificates disponíveis
- [ ] Setup Keychain com Developer Certificate
- [ ] Registrar no Apple Developer Program (se notarizing)

#### T1.3 - Preparar Estrutura de Build
- [ ] Criar `installer/macos/` directory
- [ ] Criar `build-macos-intel.sh` (x86_64)
- [ ] Criar `build-macos-arm.sh` (arm64)
- [ ] Criar `build-macos-universal.sh` (universal binary)

---

### Fase 2: Implementação (6-8 horas)

#### T2.1 - Criar build.spec.macos Otimizado
```python
# Mudanças vs. build.spec atual:
# 1. Remover venv (Python instalado via installer)
# 2. Excluir módulos desnecessários
# 3. Compilar para bytecode otimizado
# 4. Configurar para macOS bundle
# 5. Code signing hook
```

**Tamanho esperado**: 60-80 MB (vs. 150-200 MB)

#### T2.2 - Implementar Code Signing
```bash
# Script para assinar app bundle
codesign --deep --force --verify --verbose \
    --options=runtime \
    --timestamp \
    --entitlements entitlements.plist \
    dist/Etnopapers.app

# Para universal binary:
lipo -create \
    build/intel/Etnopapers.app \
    build/arm/Etnopapers.app \
    -output dist/Etnopapers-Universal.app
```

#### T2.3 - Criar DMG Installer
```bash
# Usando create-dmg (gratuito):
create-dmg \
    --volname "Etnopapers" \
    --volicon icon.icns \
    --window-pos 100 100 \
    --window-size 500 400 \
    --icon-size 100 \
    --icon "Etnopapers.app" 100 100 \
    --hide-extension "Etnopapers.app" \
    --app-drop-link 400 100 \
    dist/Etnopapers-v2.1.dmg \
    dist/Etnopapers.app
```

#### T2.4 - Implementar Notarization
```bash
# Submeter para Apple para assinatura:
xcrun altool --notarize-app \
    --file dist/Etnopapers-v2.1.dmg \
    --primary-bundle-id com.etnopapers.app \
    -u $APPLE_ID \
    -p $APP_PASSWORD

# Staple (associar notarization ao app):
xcrun stapler staple dist/Etnopapers.app
```

#### T2.5 - Criar Script de Instalação Inteligente
```bash
# installer/macos/install.sh
# 1. Detectar arquitetura (Intel vs. Apple Silicon)
# 2. Verificar Python 3.11+
# 3. Se não tiver, oferecer download
# 4. pip install -r requirements-macos.txt
# 5. Criar shortcut em Applications
# 6. Registrar em Spotlight
```

#### T2.6 - Otimizar requirements-macos.txt
```
# Apenas dependências de produção
fastapi==0.110.0
uvicorn[standard]==0.29.0
pydantic==2.8.0
pymongo==4.5.0
pdfplumber==0.10.3
instructor==1.3.7
requests==2.31.0
python-dotenv==1.0.0
aiofiles==23.2.1
```

---

### Fase 3: Multi-Arquitetura (4-5 horas)

#### T3.1 - Build Intel (x86_64)
- [ ] Forçar Python x86_64
- [ ] Build com `pyinstaller build.spec.macos`
- [ ] Testar em Intel Mac
- [ ] Tamanho esperado: 60-80 MB

#### T3.2 - Build Apple Silicon (arm64)
- [ ] Usar Python arm64 (native)
- [ ] Build com PyInstaller otimizado para arm64
- [ ] Testar em M1/M2/M3 Mac
- [ ] Tamanho esperado: 50-70 MB (menor que Intel)

#### T3.3 - Universal Binary (Opcional)
- [ ] Combinar Intel + arm64 com `lipo`
- [ ] Tamanho: ~80-100 MB (ambas arquiteturas)
- [ ] Vantagem: Funciona em qualquer Mac
- [ ] Desvantagem: Arquivo maior

---

### Fase 4: Automação (3-4 horas)

#### T4.1 - Atualizar build-macos.sh
```bash
#!/bin/bash
# Novo script que:
# 1. Detecta arquitetura
# 2. Build otimizado
# 3. Code signing
# 4. Notarization (opcional, com -n flag)
# 5. Cria DMG
# 6. Verifica tamanho/performance
```

#### T4.2 - GitHub Actions para macOS
```yaml
# .github/workflows/build-macos.yml
name: Build macOS (Intel + Apple Silicon)

on:
  push:
    tags:
      - 'v*'

jobs:
  build-macos-intel:
    runs-on: macos-12  # Intel
    steps:
      - ./build-macos.sh --arch intel

  build-macos-arm:
    runs-on: macos-14  # Apple Silicon (M3)
    steps:
      - ./build-macos.sh --arch arm64

  build-macos-universal:
    runs-on: macos-14  # M3 pode emular Intel
    steps:
      - ./build-macos.sh --arch universal

  upload-release:
    steps:
      - Upload DMGs para GitHub Release
```

#### T4.3 - GitHub Actions para Notarization
```bash
# Notarization pode ser:
# 1. Manual (usuário com Apple Developer Account)
# 2. Automático (via GitHub Actions com secrets)

# Para automático (seguro):
- Armazenar APPLE_ID em GitHub Secrets
- Armazenar APP_PASSWORD em GitHub Secrets
- Script notariza automaticamente ao fazer release
```

---

### Fase 5: Testes (3-4 horas)

#### T5.1 - Plano de Testes macOS
- [ ] Testar em Intel Mac (10.13+)
- [ ] Testar em Apple Silicon Mac (M1/M2/M3)
- [ ] Medir startup (esperado: 2-4 segundos)
- [ ] Medir RAM (esperado: 20-30%)
- [ ] Testar upload PDF
- [ ] Testar integração Ollama
- [ ] Verificar code signing (Gatekeeper)
- [ ] Verificar notarization (Sonoma+)

#### T5.2 - Troubleshooting macOS
```markdown
# Comum em macOS:

## "App não abre - 'Desenvolvedor não identificado'"
- Se code signed: deveria não aparecer
- Se não: User > System Settings > Privacy & Security > Allow

## "App trava no startup"
- Comparar com build anterior
- Verificar se Python está correto (arch -arm64 python3)
- Limpar ~/.cache/Etnopapers

## "Apple Silicon muito lento"
- Verificar se rodando via Rosetta (x86 emulado)
- Usar arm64 native (esperar recompile)
- Performance arm64 nativo é 2-3x melhor

## "Notarization falhou"
- Verificar se DMG está assinado
- Verificar ID do aplicativo
- Esperar 5-10 minutos (Apple processa)
```

#### T5.3 - Guia de Instalação macOS
```markdown
# Instalar Etnopapers no macOS

## 1. Baixar
https://github.com/edalcin/etnopapers/releases
→ Etnopapers-v2.1.dmg

## 2. Instalar
- Duplo-clique em Etnopapers.dmg
- Arraste Etnopapers.app para Applications
- Eject DMG quando terminar

## 3. Primeira Execução
- Abra Finder > Applications > Etnopapers
- macOS pode perguntar sobre permissões (OK)
- Aparecerá tela de configuração

## 4. Instalar Ollama (Obrigatório)
https://ollama.com/download → macOS
```

---

### Fase 6: Documentação (2-3 horas)

#### T6.1 - Atualizar GUIA_DESENVOLVEDOR.md
- [ ] Adicionar seção "macOS Build (Intel + ARM)"
- [ ] Documentar code signing
- [ ] Documentar notarization

#### T6.2 - Criar README-MACOS.md
- [ ] Distribuição macOS (DMG)
- [ ] Suporte multi-arquitetura
- [ ] Processo de build

---

## 📊 Tarefas Totais

| Fase | Tarefas | Duração |
|------|---------|---------|
| 1: Preparação | T1.1 - T1.3 | 2-3h |
| 2: Implementação | T2.1 - T2.6 | 6-8h |
| 3: Multi-Arquitetura | T3.1 - T3.3 | 4-5h |
| 4: Automação | T4.1 - T4.3 | 3-4h |
| 5: Testes | T5.1 - T5.3 | 3-4h |
| 6: Documentação | T6.1 - T6.2 | 2-3h |
| **TOTAL** | **21 tarefas** | **20-27h** |

---

## 🎯 Métricas de Sucesso

| Métrica | Antes | Depois | Meta |
|---------|-------|--------|------|
| **Tamanho** | 150-200 MB | 40-60 MB | ✅ 60% menor |
| **Startup** | 5-10s | 2-4s | ✅ 60% rápido |
| **RAM** | 50-80% | 20-30% | ✅ 60% menos |
| **Code Signed** | ❌ | ✅ | ✅ Profissional |
| **Notarized** | ❌ | ✅ | ✅ Seguro |
| **Intel** | ✅ | ✅ | ✅ Compatível |
| **Apple Silicon** | ❌ | ✅ | ✅ Nativo |
| **DMG Installer** | ❌ | ✅ | ✅ Padrão |

---

## 🔧 Ferramentas Necessárias

| Ferramenta | Tipo | Status |
|-----------|------|--------|
| Xcode Command Line Tools | GRATUITO | Necessário |
| Developer Certificate | PAGO ($99/ano) | Recomendado |
| create-dmg | GRATUITO | Opcional (pode usar Interface) |
| PyInstaller | GRATUITO | Já tem |
| GitHub Actions | GRATUITO | Já tem |

**Nota**: Code signing e notarization requerem Apple Developer Account ($99/ano)

---

## ⚙️ Configurações Específicas macOS

### 1. Entitlements.plist
```xml
<?xml version="1.0" encoding="UTF-8"?>
<plist version="1.0">
<dict>
    <key>com.apple.security.cs.allow-jit</key>
    <true/>
    <key>com.apple.security.cs.allow-dyld-environment-variables</key>
    <true/>
    <key>com.apple.security.cs.allow-unsigned-executable-memory</key>
    <true/>
    <key>com.apple.security.network.client</key>
    <true/>
</dict>
</plist>
```

### 2. Info.plist (macOS specific)
```xml
<key>LSMinimumSystemVersion</key>
<string>10.13</string>

<key>NSMainStoryboardFile</key>
<string></string>

<key>NSHighResolutionCapable</key>
<true/>

<key>NSLocalNetworkUsageDescription</key>
<string>Etnopapers precisa acessar a rede local para Ollama</string>
```

---

## 📅 Timeline Recomendada

- **Semana 1**: Fases 1-2 (Preparação + Implementação)
- **Semana 2**: Fases 3-4 (Multi-Arquitetura + Automação)
- **Semana 3**: Fases 5-6 (Testes + Documentação)

**Total**: 20-27 horas de desenvolvimento

---

## 🚀 Abordagem Integrada Windows + macOS

Este plano de macOS **espelha** o plano do Windows:

| Aspecto | Windows | macOS | Paralelo |
|---------|---------|-------|----------|
| **Problemas** | 39 MB, travamento | 150 MB, lento | Similares |
| **Solução** | MSI Installer | DMG Installer | Paralela |
| **Fases** | 5 fases (10-16h) | 6 fases (20-27h) | Paralelas |
| **Tarefas** | 19 tarefas | 21 tarefas | Similares |
| **Automação** | GitHub Actions | GitHub Actions | Mesma |
| **Código Signing** | Não (Windows) | Sim (macOS) | Diferente |

**Vantagem**: Arquitectura e padrões similares, apenas adaptados para cada SO

---

## 📝 Documentos a Criar

Paralelo aos documentos Windows:

1. **PLANO_OTIMIZACAO_MACOS.md** (este arquivo)
2. **TAREFAS_OTIMIZACAO_MACOS.md** (21 tarefas detalhadas)
3. **VISAO_GERAL_OTIMIZACAO_MACOS.txt** (visão visual)
4. **RESUMO_OTIMIZACAO_MACOS.md** (executivo)
5. **INDICE_OTIMIZACAO_MACOS.md** (navegação)

---

**Status**: 📋 Planejamento Proposto
**Próximo**: Criar TAREFAS_OTIMIZACAO_MACOS.md com 21 tarefas detalhadas
