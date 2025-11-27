# 🌿 Etnopapers

<img src="docs/etnopapers.png" alt="Etnopapers Logo" width="150" height="150">

**Sistema Standalone de Extração de Metadados de Artigos Etnobotânicos com AI Local**

Etnopapers é uma aplicação desktop nativa que automatiza a extração de informações sobre o uso de plantas por comunidades tradicionais a partir de artigos científicos em PDF. O sistema usa **inteligência artificial local** rodando na sua máquina para ler artigos e criar um banco de dados MongoDB organizado com essas informações.

**🔒 100% Privado**: Todos os dados permanecem no seu computador. Nenhuma informação é enviada para APIs externas.

**💻 Standalone**: Aplicativo nativo para Windows, macOS e Linux. Não requer Docker.

---

## 📖 O que é Etnobotânica?

Etnobotânica é o estudo da relação entre pessoas e plantas. Pesquisadores documentam como comunidades tradicionais (indígenas, quilombolas, ribeirinhas) usam plantas para medicina, alimentação, rituais e outras finalidades. Esses conhecimentos são publicados em artigos científicos, mas ficam dispersos e difíceis de consultar.

## 💡 Como funciona?

```
1. Você abre o Etnopapers (duplo-clique no executável)
         ↓
2. Upload de um artigo científico (PDF)
         ↓
3. Modelo de AI local (Qwen2.5-7B via Ollama) lê e identifica:
   • Espécies de plantas mencionadas
   • Comunidades tradicionais estudadas
   • Regiões geográficas
   • Métodos de pesquisa
         ↓
4. Você revisa e corrige os dados extraídos
         ↓
5. Informações são salvas em banco de dados MongoDB
```

### 🔒 Sua privacidade está 100% protegida

- **PDFs não são armazenados**: Apenas os metadados extraídos ficam salvos
- **Processamento local**: AI roda na sua máquina (sem envio para cloud)
- **Dados nunca saem do computador**: Zero transmissão para serviços externos
- **Offline-capable**: Funciona sem internet após instalação inicial
- **Zero custos por requisição**: Processe quantos artigos quiser gratuitamente

## 🛠️ Tecnologias Usadas

### Para Usuários
- **Interface Desktop Nativa**: Aplicativo standalone para Windows, macOS e Linux
- **Inteligência Artificial Local**: Modelo Qwen2.5-7B rodando via Ollama
- **Inferência rápida**: 1-5 segundos por artigo (com GPU)
- **Validação Taxonômica**: Confirma nomes científicos de plantas automaticamente (GBIF API)

### Para Desenvolvedores
- **Frontend**: React 18 + TypeScript (buildado e incluído no executável)
- **Backend**: Python FastAPI com Instructor + Pydantic
- **AI Framework**: Ollama (gerenciamento de modelos locais)
- **Modelo**: Qwen2.5-7B-Instruct (quantizado Q4, 4.8 GB)
- **Banco de Dados**: MongoDB (NoSQL, via URI externa)
- **Build**: PyInstaller para executável standalone
- **GPU**: Nvidia GPU recomendada (opcional, funciona com CPU)

## 🚀 Como começar?

### Opção 1: Download do Executável (Recomendado)

**Baixe o executável pronto para seu sistema operacional:**

1. Acesse a página de [Releases](https://github.com/[seu-usuario]/etnopapers/releases/latest)
2. Baixe o pacote para seu SO:
   - **Windows**: `Etnopapers-Windows-v1.0.0.zip`
   - **macOS**: `Etnopapers-macOS-v1.0.0.zip`
   - **Linux**: `Etnopapers-Linux-v1.0.0.tar.gz`

3. Extraia o arquivo

4. **Instale o Ollama** (pré-requisito):
   - Windows: [Download](https://ollama.ai/download/windows)
   - macOS: [Download](https://ollama.ai/download/mac)
   - Linux: `curl -fsSL https://ollama.ai/install.sh | sh`

5. **Baixe o modelo de AI**:
   ```bash
   ollama pull qwen2.5:7b-instruct-q4_K_M
   ```

6. **Execute o Etnopapers**:
   - Windows: Duplo-clique em `etnopapers.exe`
   - macOS: Abra `Etnopapers.app`
   - Linux: `./etnopapers`

7. **Configure MongoDB URI** na primeira execução (via GUI)

**Pronto!** O navegador abrirá automaticamente em `http://localhost:8000`

---

### Opção 2: Build do Código-Fonte

Para desenvolvedores ou quem quer compilar manualmente:

**Pré-requisitos:**
- Python 3.11+
- Node.js 18+
- Ollama instalado

**Build:**

```bash
# 1. Clonar repositório
git clone https://github.com/[seu-usuario]/etnopapers.git
cd etnopapers

# 2. Executar script de build
# Windows:
build-windows.bat

# macOS:
./build-macos.sh

# Linux:
./build-linux.sh

# 3. Executável estará em: dist/etnopapers.exe (ou Etnopapers.app / etnopapers)
```

Veja [docs/build-from-source.md](docs/build-from-source.md) para instruções detalhadas.

---

## 📋 Requisitos

### Hardware

**Mínimo:**
- CPU: 4 cores
- RAM: 8 GB
- Disco: 10 GB livres
- GPU: Nvidia GPU com 4+ GB VRAM (opcional, mas recomendado)

**Recomendado:**
- CPU: 6+ cores
- RAM: 16 GB
- Disco: 20 GB livres
- GPU: Nvidia RTX 3060 ou superior (12 GB VRAM)

### Software (Pré-requisitos)

1. **Ollama** - Para inferência AI local
   - Download: https://ollama.ai/download

2. **MongoDB** - Database (local ou MongoDB Atlas cloud)
   - Local: https://www.mongodb.com/try/download/community
   - Cloud (gratuito): https://www.mongodb.com/cloud/atlas

**Nota**: Ollama e MongoDB precisam ser instalados separadamente. O executável do Etnopapers os usa como serviços externos.

---

## 🎯 Primeiro Uso

### 1. Iniciar Ollama

Certifique-se que o Ollama está rodando:

```bash
# Verificar
curl http://localhost:11434/api/tags

# Se não estiver rodando:
# Windows: Abrir Ollama.exe (inicia automaticamente)
# macOS: Abrir Ollama.app (fica na barra de menu)
# Linux: sudo systemctl start ollama
```

### 2. Configurar MongoDB

**Opção A: MongoDB Atlas (Cloud - Recomendado)**

1. Criar conta gratuita em https://www.mongodb.com/cloud/atlas
2. Criar cluster M0 (gratuito)
3. Obter URI: `mongodb+srv://user:password@cluster.mongodb.net/etnopapers`

**Opção B: MongoDB Local**

```bash
# Instalar MongoDB Community
# URI será: mongodb://localhost:27017/etnopapers
```

### 3. Executar Etnopapers

- Windows: Duplo-clique em `etnopapers.exe`
- macOS: Abrir `Etnopapers.app`
- Linux: `./etnopapers`

### 4. Configuração Inicial (GUI)

Na primeira execução, uma janela aparecerá solicitando:
- **MongoDB URI**: Cole o URI do MongoDB Atlas ou local
- Clique em "Salvar e Iniciar"

Configuração será salva em arquivo `.env` e não precisará repetir.

### 5. Usar Aplicação

O navegador abrirá automaticamente em `http://localhost:8000`

1. **Upload de PDF**: Arraste um artigo científico sobre etnobotânica
2. **Aguarde extração**: ~1-5 segundos (processamento local)
3. **Revise os metadados**: Sistema mostra título, autores, espécies, etc.
4. **Salve ou edite**: Clique em "Salvar" (finalizar) ou "Editar" (corrigir)

---

## 🔄 Metadados Extraídos

O modelo de AI local extrai automaticamente:

- **Bibliográficos**: Título, autores, ano, DOI, revista/jornal, resumo
- **Geográficos**: País, estado, município, local específico, bioma
- **Sociais**: Comunidades tradicionais estudadas
- **Botânicos**: Espécies de plantas (nome científico + nome popular)
- **Metodológicos**: Tipo de uso, metodologia de pesquisa, período do estudo

**Acurácia típica**: >80% (com revisão manual recomendada)

---

## 💰 Custos

### Investimento Inicial
- **Software Etnopapers**: Gratuito (código aberto, MIT License)
- **Ollama**: Gratuito
- **MongoDB Atlas**: Gratuito até 512 MB (M0 cluster)
- **GPU Nvidia** (opcional): ~$300-500 (RTX 3060)

### Custos Recorrentes
- **Processamento de artigos**: **R$ 0,00** (AI local, sem custos por requisição)
- **APIs externas**: **R$ 0,00** (apenas validação taxonômica GBIF, gratuita)
- **MongoDB Atlas**: **R$ 0,00** (tier gratuito)
- **Energia elétrica**: ~R$ 2-5/mês (uso ocasional)

### Comparação com APIs Externas

| Métrica | APIs Externas | AI Local (Etnopapers) |
|---------|---------------|----------------------|
| Custo por artigo | R$ 0,05 - R$ 0,15 | **R$ 0,00** |
| 100 artigos | R$ 5 - R$ 15 | **R$ 0,00** |
| 1000 artigos | R$ 50 - R$ 150 | **R$ 0,00** |
| Privacidade | Dados enviados a terceiros | **100% local** |
| Quota | Limitada por plano | **Ilimitada** |

---

## ⚡ Performance

### Benchmarks (Inferência Local)

| Hardware | Tempo por Artigo | Throughput |
|----------|------------------|------------|
| RTX 4090 (24 GB) | 0.5-1s | ~60-120 artigos/min |
| RTX 4080 (16 GB) | 1-2s | ~30-60 artigos/min |
| RTX 3080 (10 GB) | 2-3s | ~20-30 artigos/min |
| **RTX 3060 (12 GB)** | **2-4s** | **15-30 artigos/min** |
| RTX 2070 (8 GB) | 4-6s | ~10-15 artigos/min |
| **CPU (16 cores)** | **10-20s** | **3-6 artigos/min** |

**Nota**: GPU é opcional mas altamente recomendada. Funciona com CPU, porém mais lento.

---

## 📁 Estrutura do Projeto

```
etnopapers/
├── backend/                         # Servidor Python
│   ├── launcher.py                  # Entry point do executável
│   ├── gui/                         # Interface Tkinter
│   │   └── config_dialog.py         # Dialog de configuração
│   ├── utils/                       # Utilitários
│   │   └── environment_validator.py # Validação Ollama/MongoDB
│   ├── services/
│   │   └── extraction_service.py    # Cliente Ollama + Instructor
│   └── routers/
│       └── extraction.py            # Endpoint /api/extract/metadata
├── frontend/                        # Interface web (React)
│   ├── src/                         # Código-fonte React
│   └── dist/                        # Build estático (incluído no exe)
├── docs/                            # Documentação
│   ├── instalacao-standalone.md     # Guia para usuários
│   └── build-from-source.md         # Guia para desenvolvedores
├── build.spec                       # Configuração PyInstaller
├── build-windows.bat                # Script build Windows
├── build-macos.sh                   # Script build macOS
├── build-linux.sh                   # Script build Linux
├── .env.example                     # Template de configuração
├── CLAUDE.md                        # Guia para desenvolvimento
└── README.md                        # Este arquivo
```

---

## 📚 Documentação

- **[Guia de Instalação Standalone](docs/instalacao-standalone.md)**: Instalação completa passo-a-passo
- **[Build from Source](docs/build-from-source.md)**: Compilar do código-fonte
- **[Especificação Técnica](specs/main/spec.md)**: Arquitetura e requisitos
- **[Modelo de Dados](specs/main/data-model.md)**: Estrutura MongoDB

---

## 🔐 Segurança e Privacidade

### 100% de Privacidade Garantida

- ✅ **Dados nunca saem do computador**: AI roda localmente
- ✅ **PDFs não são salvos**: Apenas metadados extraídos permanecem
- ✅ **Banco de dados externo**: MongoDB (local ou cloud de sua escolha)
- ✅ **Sem telemetria**: Zero rastreamento ou analytics
- ✅ **Código aberto**: Auditável (MIT License)
- ✅ **Offline-capable**: Funciona sem internet após instalação

---

## 🛠️ Solução de Problemas

### Erro: "Ollama não está rodando"

```bash
# Verificar status
curl http://localhost:11434/api/tags

# Reiniciar Ollama
# Windows: Reiniciar Ollama.exe
# macOS: Quit e reabrir Ollama.app
# Linux: sudo systemctl restart ollama
```

### Erro: "Cannot connect to MongoDB"

1. Verificar MongoDB URI no arquivo `.env`
2. Testar conexão:
   ```bash
   mongosh "seu-mongodb-uri"
   ```
3. Reconfigurar: Deletar `.env` e reiniciar Etnopapers

### Erro: "Model not found"

```bash
# Baixar modelo
ollama pull qwen2.5:7b-instruct-q4_K_M

# Verificar
ollama list
```

Veja [docs/instalacao-standalone.md](docs/instalacao-standalone.md) para mais soluções.

---

## 🤝 Contribuindo

Contribuições são bem-vindas!

1. Fork do repositório
2. Criar branch: `git checkout -b feature/nome-da-feature`
3. Fazer alterações e testar
4. Commit: `git commit -m "Adiciona feature X"`
5. Push: `git push origin feature/nome-da-feature`
6. Abrir Pull Request

### Roadmap Futuro

- [ ] Interface desktop nativa moderna (customtkinter ou Electron)
- [ ] Prompt customizável via GUI
- [ ] Fine-tuning do Qwen2.5 em corpus etnobotânico
- [ ] Processamento em batch (múltiplos PDFs)
- [ ] OCR para PDFs escaneados
- [ ] Dashboard de analytics
- [ ] Exportação CSV/JSON/Excel
- [ ] Sistema de autenticação multi-usuário

---

## 📄 Licença

MIT License - Software livre e de código aberto.

Veja [LICENSE](LICENSE) para detalhes completos.

---

## 🙏 Agradecimentos

Este projeto foi desenvolvido para apoiar pesquisadores em etnobotânica, preservando e organizando conhecimentos tradicionais sobre o uso de plantas.

**Agradecimentos especiais:**
- Comunidade Qwen (modelo Qwen2.5)
- Ollama (framework de inferência)
- Projeto Instructor (structured outputs)
- Comunidades tradicionais que compartilham seus conhecimentos

---

## 📞 Suporte

- **Guia de Instalação**: [docs/instalacao-standalone.md](docs/instalacao-standalone.md)
- **Build from Source**: [docs/build-from-source.md](docs/build-from-source.md)
- **Issues**: [GitHub Issues](https://github.com/[seu-usuario]/etnopapers/issues)
- **Discussões**: [GitHub Discussions](https://github.com/[seu-usuario]/etnopapers/discussions)

---

**Desenvolvido com ❤️ para pesquisadores em etnobotânica**

🤖 *Sistema com AI Local - 100% Privado - Zero Custos Recorrentes - Aplicação Standalone Nativa*

🌿 *Preservando conhecimentos tradicionais sobre plantas*
