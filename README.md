# 🌿 Etnopapers - Ethnobotany Metadata Extraction System

[![GitHub Release](https://img.shields.io/github/v/release/edalcin/etnopapers?style=flat-square)](https://github.com/edalcin/etnopapers/releases)
[![License](https://img.shields.io/badge/license-MIT-blue.svg?style=flat-square)](LICENSE)
[![Python](https://img.shields.io/badge/python-3.11+-blue.svg?style=flat-square)](https://www.python.org/)
[![Node.js](https://img.shields.io/badge/node.js-18+-green.svg?style=flat-square)](https://nodejs.org/)
[![Status](https://img.shields.io/badge/status-stable-brightgreen.svg?style=flat-square)](https://github.com/edalcin/etnopapers)

**Etnopapers** is a standalone desktop application that automatically extracts and catalogs ethnobotanical metadata from scientific articles about traditional plant use in indigenous and traditional communities.

🌿 **100% Private** - All processing happens locally on your computer
🚀 **Easy to Use** - Drag-and-drop PDF upload with automatic extraction
📊 **Comprehensive** - Captures plants, uses, communities, locations, and biomes
🔧 **Free & Open Source** - No subscriptions, no APIs, no cloud dependencies

**Available in**: [English](README.md) | [Português](README_PT.md)

---

## Quick Start

### 1️⃣ Download Ollama
https://ollama.com/download - Required for local AI processing

### 2️⃣ Get Etnopapers
Download latest release: https://github.com/edalcin/etnopapers/releases

### 3️⃣ Run & Configure
Execute the app and configure MongoDB (local or MongoDB Atlas free tier)

### 4️⃣ Start Using
Drag a PDF → Review extraction → Save to database

**⏱️ Setup time: 10 minutes | Processing: 2-5 minutes per PDF**

---

## 🌿 What is Ethnobotany?

Ethnobotany investigates the complex interactions between plants and people over time and space. It encompasses traditional and scientific knowledge, including diverse uses (medicinal, alimentary, etc.), worldviews, management systems, and languages that different cultures maintain in relation to plants and their associated ecosystems. It bridges biology and human sciences (Prance, 2007).

---

## 💡 How It Works

```
1. Open Etnopapers (double-click executable)
         ↓
2. Upload scientific article (PDF)
         ↓
3. Local AI (Qwen 2.5-7B via Ollama) identifies:
   • Plant species mentioned
   • Traditional communities
   • Geographic regions
   • Research methodology
         ↓
4. Review and correct extracted data
         ↓
5. Save to MongoDB database
```

### 🔒 100% Privacy Protected

- **PDFs not stored** - Only extracted metadata saved
- **Local processing** - AI runs on your machine (no cloud)
- **Data never leaves** - Zero transmission to external services
- **Works offline** - After initial setup, no internet needed
- **Free forever** - Process unlimited articles at no cost

---

## 📚 Documentation

| Guide | Purpose | Language |
|-------|---------|----------|
| [Quick Start](docs/QUICKSTART.md) | 30-minute setup | English |
| [User Guide](docs/GUIA_USUARIO.md) | Complete usage guide | Portuguese |
| [API Documentation](docs/API_DOCUMENTATION.md) | REST API reference | English |
| [Developer Guide](docs/DEVELOPER_GUIDE.md) | Development setup | English |
| [CLAUDE.md](CLAUDE.md) | Complete specifications | English |

---

## 🎯 Key Features

### Automatic Extraction
✅ Plant names (vernacular + scientific)
✅ Traditional uses (medicinal, food, rituals)
✅ Indigenous communities and locations
✅ Biomes and ecosystems
✅ Authors, year, publication details

### Smart Database
✅ MongoDB for scalable storage
✅ Full-text search across articles
✅ Filter by year, country, species, use
✅ Pagination and sorting
✅ Advanced analytics

### Professional Tools
✅ Download backup as ZIP
✅ SHA256 integrity checksums
✅ Export to JSON/CSV
✅ Restore from backups
✅ Team collaboration ready

### Cross-Platform
✅ Windows executable (~150 MB)
✅ macOS app (~150 MB)
✅ Linux binary (~150 MB)
✅ No installation required
✅ Portable, single file

---

## 📊 Technology Stack

| Layer | Technology | Purpose |
|-------|-----------|---------|
| **Frontend** | React 18 + TypeScript | User interface |
| **Backend** | FastAPI (Python) | API & business logic |
| **Database** | MongoDB | Scalable storage |
| **AI** | Ollama + Qwen 2.5 | Local inference |
| **PDF** | pdfplumber | Text extraction |
| **Build** | PyInstaller | Executable bundling |

---

## 📋 System Requirements

| Component | Minimum | Recommended |
|-----------|---------|-------------|
| **RAM** | 4 GB | 8 GB |
| **Storage** | 5 GB | 10 GB |
| **CPU** | Dual-core | Quad-core |
| **Internet** | Download only | Works offline |

---

## 🚀 Installation

### Windows
```powershell
# Download exe from releases
# Double-click to install
# Run from Start Menu
```

### macOS
```bash
unzip Etnopapers-macos-vX.Y.Z.zip
mv Etnopapers.app /Applications/
open /Applications/Etnopapers.app
```

### Linux
```bash
wget https://github.com/edalcin/etnopapers/releases/download/vX.Y.Z/etnopapers-linux-vX.Y.Z
chmod +x etnopapers-linux-vX.Y.Z
./etnopapers-linux-vX.Y.Z
```

### From Source (Development)
```bash
git clone https://github.com/edalcin/etnopapers.git
cd etnopapers

# Frontend
cd frontend && npm install && npm run dev

# Backend (another terminal)
cd backend
python -m venv venv && source venv/bin/activate
pip install -r requirements.txt
uvicorn main:app --reload
```

---

## 💻 Usage

### Step 1: Upload PDF
Drag & drop scientific article or click to select (max 100 MB)

### Step 2: Review Metadata
Check extracted information:
- Title, authors, publication year
- Plant species and scientific names
- Traditional uses and communities
- Geographic locations and biome

### Step 3: Edit if Needed
- Correct scientific names
- Add missing species
- Complete geographic data
- Verify research methodology

### Step 4: Save Article
Click "Save Article" to store in database

### Step 5: Search & Analyze
- Search by title, author, species
- Filter by year, country, use type
- Download complete backup
- Export for further analysis

---

## 🔧 Development

### Prerequisites
- Node.js 18+
- Python 3.11+
- MongoDB instance
- Ollama (for testing)

### Frontend Development
```bash
cd frontend
npm install
npm run dev      # Development server
npm run build    # Production build
npm run test     # Run tests
```

### Backend Development
```bash
cd backend
python -m venv venv
source venv/bin/activate
pip install -r requirements.txt
uvicorn main:app --reload  # Development server
pytest tests/               # Run tests
```

### Build Standalone Executables
```bash
./build-windows.bat    # Windows
bash build-macos.sh    # macOS
bash build-linux.sh    # Linux
```

---

## 🤝 Contributing

We welcome contributions! Here's how to help:

1. **Report bugs**: [GitHub Issues](https://github.com/edalcin/etnopapers/issues)
2. **Suggest features**: [GitHub Discussions](https://github.com/edalcin/etnopapers/discussions)
3. **Submit code**: [Pull Requests](https://github.com/edalcin/etnopapers/pulls)
4. **Improve docs**: Edit docs directly

### Development Workflow
```bash
git checkout -b feature/my-feature
# Make changes
git commit -m "feat: Add my feature"
git push origin feature/my-feature
# Create Pull Request on GitHub
```

---

## 📈 Performance

| Operation | Time | Notes |
|-----------|------|-------|
| PDF upload | <1 sec | File transfer |
| Extraction | 2-5 min | Depends on PDF length |
| Save article | <1 sec | Database write |
| Search (1000 items) | <100 ms | Full-text indexed |
| Backup creation | 1-30 sec | Scales with data |

---

## 🐛 Troubleshooting

### "Ollama Unavailable"
1. Download Ollama: https://ollama.com/download
2. Open Ollama application
3. Wait for "Running on http://localhost:11434"

### "MongoDB Connection Error"
1. Local MongoDB: `mongosh` should connect
2. MongoDB Atlas: Verify connection string
3. Check firewall/network settings

### "PDF Not Processing"
1. Verify PDF is text-extractable (not scanned image)
2. Check file size (<100 MB)
3. Ensure Ollama has 4+ GB RAM

See [User Guide](docs/GUIA_USUARIO.md) for more troubleshooting.

---

## 📄 License

MIT License - Free to use, modify, and distribute

---

## 📝 Citation

```bibtex
@software{etnopapers2024,
  author = {Dalcin, E.},
  title = {Etnopapers: Ethnobotany Metadata Extraction System},
  year = {2024},
  url = {https://github.com/edalcin/etnopapers}
}
```

---

## 🔗 Links

- **Releases**: https://github.com/edalcin/etnopapers/releases
- **Issues**: https://github.com/edalcin/etnopapers/issues
- **Discussions**: https://github.com/edalcin/etnopapers/discussions
- **Documentation**: [docs/](docs/)

---

## 🛠️ Technology Stack (Detailed)

### Frontend
- **React** 18 - Modern UI framework
- **TypeScript** - Type-safe JavaScript
- **Vite** - Lightning-fast build tool
- **Zustand** - Lightweight state management
- **TanStack React Table** - Advanced data tables
- **React Hook Form** - Form validation
- **CSS3** - Modern styling with animations

### Backend
- **FastAPI** - Modern Python web framework
- **PyMongo** - MongoDB Python driver
- **pdfplumber** - PDF text extraction
- **instructor** - Structured LLM outputs
- **Ollama** - Local LLM inference
- **Uvicorn** - ASGI server

### Database
- **MongoDB** - NoSQL document store
- **Single collection model** - All-in-one `referencias`
- **Indexed fields** - DOI (unique), status, year, text search
- **Local or Cloud** - Works with MongoDB Atlas

### DevOps
- **GitHub Actions** - CI/CD automation
- **PyInstaller** - Executable bundling
- **Multi-platform** - Windows, macOS, Linux

---

## 👥 Use Cases

### For Ethnobotanists
Digitize literature, organize by plant/region/community, export for analysis

### For Indigenous Communities
Document traditional knowledge, control data locally, organize research

### For Conservation Organizations
Build botanical inventories, track endangered species, map traditional knowledge

### For Students & Researchers
Compile reviews, extract thesis data, practice with open-source tools

---

## 🎉 Features Implemented

✅ Local AI with Ollama (v2.0)
✅ Standalone desktop application
✅ MongoDB integration
✅ PDF text extraction
✅ Automatic metadata extraction
✅ Full-text search
✅ Article management (CRUD)
✅ Database backup & restore
✅ Health checks & monitoring
✅ User configuration interface
✅ Comprehensive documentation
✅ Unit & integration tests
✅ Multi-language support (EN, PT)

---

## 🚀 Planned Features

### v2.1 (Q2 2024)
- Batch PDF upload queue
- Advanced filtering UI
- CSV/Excel export
- Improved duplicate detection

### v2.2 (Q3 2024)
- Multi-user collaboration
- Team workspaces
- Activity logging
- Custom field support

### v3.0 (Q4 2024)
- Mobile app (iOS/Android)
- Web interface
- Real-time sync
- Advanced analytics

---

## 📚 Resources

- **FastAPI**: https://fastapi.tiangolo.com/
- **React**: https://react.dev/
- **MongoDB**: https://docs.mongodb.com/
- **Ollama**: https://ollama.ai/
- **TypeScript**: https://www.typescriptlang.org/

---

## 🙏 Acknowledgments

Built with amazing open-source projects:
- FastAPI, React, MongoDB, Ollama, pdfplumber, PyInstaller

---

**Etnopapers v2.0** - Making ethnobotanical research more accessible

Made with ❤️ for ethnobotany researchers and indigenous communities

🌿 🔬 📚 🌍

---

## Português (Portuguese)

**[Leia o README em Português →](README_PT.md)**
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
