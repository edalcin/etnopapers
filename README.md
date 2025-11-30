# 🌿 Etnopapers - Sistema de Extração de Metadados Etnobotânicos

[![GitHub Release](https://img.shields.io/github/v/release/edalcin/etnopapers?style=flat-square)](https://github.com/edalcin/etnopapers/releases)
[![Licença](https://img.shields.io/badge/licença-MIT-blue.svg?style=flat-square)](LICENSE)
[![Python](https://img.shields.io/badge/python-3.11+-blue.svg?style=flat-square)](https://www.python.org/)
[![Node.js](https://img.shields.io/badge/node.js-18+-green.svg?style=flat-square)](https://nodejs.org/)
[![Status](https://img.shields.io/badge/status-estável-brightgreen.svg?style=flat-square)](https://github.com/edalcin/etnopapers)

**Etnopapers** é uma aplicação desktop nativa que automatiza a extração e catalogação de metadados etnobotânicos a partir de artigos científicos sobre o uso tradicional de plantas por comunidades indígenas e tradicionais.

🌿 **100% Privado** - Todo processamento acontece localmente no seu computador
🚀 **Fácil de Usar** - Upload de PDF por arrastar-e-soltar com extração automática
📊 **Abrangente** - Captura plantas, usos, comunidades, localizações e biomas
🔧 **Gratuito e Open Source** - Sem assinaturas, sem APIs externas, sem dependência de nuvem

---

## 🚀 Início Rápido

### 1️⃣ Baixar Ollama
https://ollama.com/download - Necessário para processamento de IA local

### 2️⃣ Obter Etnopapers
Baixe a versão mais recente: https://github.com/edalcin/etnopapers/releases

### 3️⃣ Executar e Configurar
Execute o aplicativo e configure o MongoDB (local ou MongoDB Atlas plano gratuito)

### 4️⃣ Começar a Usar
Arraste um PDF → Revise a extração → Salve no banco de dados

**⏱️ Tempo de configuração: 10 minutos | Processamento: 2-5 minutos por PDF**

---

## 🌿 O que é Etnobotânica?

A etnobotânica investiga as interações complexas entre plantas e pessoas ao longo do tempo e do espaço. Abrange conhecimento tradicional e científico, incluindo usos diversos (medicinal, alimentar, etc.), cosmovisões, sistemas de gestão e línguas que diferentes culturas mantêm em relação às plantas e seus ecossistemas associados. Funciona como ponte entre biologia e ciências humanas (Prance, 2007).

---

## 💡 Como Funciona

```
1. Abra o Etnopapers (duplo-clique no executável)
         ↓
2. Faça upload de artigo científico (PDF)
         ↓
3. IA local (Qwen 2.5-7B via Ollama) identifica:
   • Espécies de plantas mencionadas
   • Comunidades tradicionais
   • Regiões geográficas
   • Metodologia de pesquisa
         ↓
4. Revise e corrija dados extraídos
         ↓
5. Salve no banco de dados MongoDB
```

### 🔒 100% Privacidade Protegida

- **PDFs não armazenados** - Apenas metadados extraídos salvos
- **Processamento local** - IA roda na sua máquina (sem nuvem)
- **Dados nunca saem** - Zero transmissão para serviços externos
- **Funciona offline** - Após setup inicial, sem internet necessária
- **Gratuito para sempre** - Processe artigos ilimitados sem custo

---

## 📚 Documentação

| Guia | Propósito | Linguagem |
|------|----------|----------|
| [Guia Rápido](docs/QUICKSTART.md) | Setup em 30 minutos | Português/English |
| [Guia do Usuário](docs/GUIA_USUARIO.md) | Guia completo de uso | Português |
| [Documentação API](docs/API_DOCUMENTATION.md) | Referência REST API | English |
| [Guia do Desenvolvedor](docs/DEVELOPER_GUIDE.md) | Setup de desenvolvimento | English |
| [CLAUDE.md](CLAUDE.md) | Especificações completas | English |

---

## 🎯 Recursos Principais

### Extração Automática
✅ Nomes de plantas (vernacular + científico)
✅ Usos tradicionais (medicinal, alimentar, rituais)
✅ Comunidades indígenas e localizações
✅ Biomas e ecossistemas
✅ Autores, ano e detalhes de publicação

### Banco de Dados Inteligente
✅ MongoDB para armazenamento escalável
✅ Busca full-text em artigos
✅ Filtrar por ano, país, espécie, uso
✅ Paginação e ordenação
✅ Analytics avançado

### Ferramentas Profissionais
✅ Download de backup como ZIP
✅ Checksums SHA256 de integridade
✅ Export para JSON/CSV
✅ Restaurar de backups
✅ Pronto para colaboração em equipe

### Multi-plataforma
✅ Executável Windows (~150 MB)
✅ App macOS (~150 MB)
✅ Binário Linux (~150 MB)
✅ Sem instalação necessária
✅ Portável, arquivo único

---

## 📊 Stack Tecnológico

| Camada | Tecnologia | Propósito |
|--------|-----------|----------|
| **Frontend** | React 18 + TypeScript | Interface do usuário |
| **Backend** | FastAPI (Python) | API e lógica de negócio |
| **Banco de Dados** | MongoDB | Armazenamento escalável |
| **IA** | Ollama + Qwen 2.5 | Inferência local |
| **PDF** | pdfplumber | Extração de texto |
| **Build** | PyInstaller | Empacotamento executável |

---

## 📋 Requisitos do Sistema

| Componente | Mínimo | Recomendado |
|-----------|--------|-------------|
| **RAM** | 4 GB | 8 GB |
| **Armazenamento** | 5 GB | 10 GB |
| **CPU** | Dual-core | Quad-core |
| **Internet** | Download apenas | Funciona offline |

---

## 🚀 Instalação

### Windows
```powershell
# Baixe o exe das releases
# Duplo-clique para instalar
# Execute do Menu Iniciar
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

### Do Código-Fonte (Desenvolvimento)
```bash
git clone https://github.com/edalcin/etnopapers.git
cd etnopapers

# Frontend
cd frontend && npm install && npm run dev

# Backend (outro terminal)
cd backend
python -m venv venv && source venv/bin/activate
pip install -r requirements.txt
uvicorn main:app --reload
```

---

## 💻 Como Usar

### Passo 1: Upload de PDF
Arraste e solte artigo científico ou clique para selecionar (máx 100 MB)

### Passo 2: Revise Metadados
Verifique informações extraídas:
- Título, autores, ano de publicação
- Espécies de plantas e nomes científicos
- Usos tradicionais e comunidades
- Localizações geográficas e bioma

### Passo 3: Edite se Necessário
- Corrija nomes científicos
- Adicione espécies faltantes
- Complete dados geográficos
- Verifique metodologia de pesquisa

### Passo 4: Salve o Artigo
Clique em "Salvar Artigo" para armazenar no banco de dados

### Passo 5: Busque e Analise
- Busque por título, autor, espécie
- Filtro por ano, país, tipo de uso
- Baixe backup completo
- Exporte para análise posterior

---

## 🔧 Desenvolvimento

### Pré-requisitos
- Node.js 18+
- Python 3.11+
- Instância MongoDB
- Ollama (para testes)

### Desenvolvimento Frontend
```bash
cd frontend
npm install
npm run dev      # Servidor de desenvolvimento
npm run build    # Build de produção
npm run test     # Executar testes
```

### Desenvolvimento Backend
```bash
cd backend
python -m venv venv
source venv/bin/activate
pip install -r requirements.txt
uvicorn main:app --reload  # Servidor de desenvolvimento
pytest tests/               # Executar testes
```

### Construir Executáveis Standalone
```bash
./build-windows.bat    # Windows
bash build-macos.sh    # macOS
bash build-linux.sh    # Linux
```

---

## 🤝 Contribuindo

Bem-vindo a contribuições! Aqui está como ajudar:

1. **Reportar bugs**: [GitHub Issues](https://github.com/edalcin/etnopapers/issues)
2. **Sugerir recursos**: [GitHub Discussions](https://github.com/edalcin/etnopapers/discussions)
3. **Submeter código**: [Pull Requests](https://github.com/edalcin/etnopapers/pulls)
4. **Melhorar docs**: Edite diretamente

### Fluxo de Desenvolvimento
```bash
git checkout -b feature/minha-feature
# Faça alterações
git commit -m "feat: Adicionar minha feature"
git push origin feature/minha-feature
# Criar Pull Request no GitHub
```

---

## 📈 Performance

| Operação | Tempo | Notas |
|----------|-------|-------|
| Upload de PDF | <1 seg | Transferência de arquivo |
| Extração | 2-5 min | Depende do tamanho do PDF |
| Salvar artigo | <1 seg | Escrita no banco de dados |
| Busca (1000 itens) | <100 ms | Busca full-text indexada |
| Criação de backup | 1-30 seg | Escala com quantidade de dados |

---

## 🐛 Solução de Problemas

### "Ollama Indisponível"
1. Baixe Ollama: https://ollama.com/download
2. Abra aplicativo Ollama
3. Aguarde "Running on http://localhost:11434"

### "Erro de Conexão MongoDB"
1. MongoDB local: `mongosh` deve conectar
2. MongoDB Atlas: Verifique string de conexão
3. Verifique firewall/configurações de rede

### "PDF Não Processa"
1. Verifique se PDF é texto-extraível (não imagem scaneada)
2. Verifique tamanho do arquivo (<100 MB)
3. Garanta que Ollama tem 4+ GB de RAM

Veja [Guia do Usuário](docs/GUIA_USUARIO.md) para mais solução de problemas.

---

## 📄 Licença

Licença MIT - Livre para usar, modificar e distribuir

---

## 📝 Citação

```bibtex
@software{etnopapers2024,
  author = {Dalcin, E.},
  title = {Etnopapers: Sistema de Extração de Metadados Etnobotânicos},
  year = {2024},
  url = {https://github.com/edalcin/etnopapers}
}
```

---

## 🔗 Links

- **Releases**: https://github.com/edalcin/etnopapers/releases
- **Issues**: https://github.com/edalcin/etnopapers/issues
- **Discussões**: https://github.com/edalcin/etnopapers/discussions
- **Documentação**: [docs/](docs/)

---

## 🛠️ Stack Tecnológico (Detalhado)

### Frontend
- **React** 18 - Framework UI moderno
- **TypeScript** - JavaScript type-safe
- **Vite** - Ferramenta de build ultra-rápida
- **Zustand** - Gerenciamento de estado leve
- **TanStack React Table** - Tabelas de dados avançadas
- **React Hook Form** - Validação de formulários
- **CSS3** - Estilo moderno com animações

### Backend
- **FastAPI** - Framework web Python moderno
- **PyMongo** - Driver MongoDB para Python
- **pdfplumber** - Extração de texto de PDF
- **instructor** - Saídas estruturadas de LLM
- **Ollama** - Inferência local de LLM
- **Uvicorn** - Servidor ASGI

### Banco de Dados
- **MongoDB** - Banco de dados NoSQL de documentos
- **Modelo single collection** - Tudo em uma `referencias`
- **Campos indexados** - DOI (único), status, ano, busca full-text
- **Local ou nuvem** - Funciona com MongoDB Atlas

### DevOps
- **GitHub Actions** - Automação CI/CD
- **PyInstaller** - Empacotamento executável
- **Multi-plataforma** - Windows, macOS, Linux

---

## 👥 Casos de Uso

### Para Etnobotânicos
Digitalizar literatura, organizar por planta/região/comunidade, exportar para análise

### Para Comunidades Indígenas
Documentar conhecimento tradicional, controlar dados localmente, organizar pesquisa

### Para Organizações de Conservação
Construir inventários botânicos, rastrear espécies ameaçadas, mapear conhecimento tradicional

### Para Estudantes e Pesquisadores
Compilar revisões, extrair dados de tese, praticar com ferramentas open-source

---

## 🎉 Recursos Implementados

✅ IA local com Ollama (v2.0)
✅ Aplicação desktop standalone
✅ Integração MongoDB
✅ Extração de texto de PDF
✅ Extração automática de metadados
✅ Busca full-text
✅ Gerenciamento de artigos (CRUD)
✅ Backup e restauração de banco de dados
✅ Verificações de saúde e monitoramento
✅ Interface de configuração do usuário
✅ Documentação abrangente
✅ Testes unitários e integração
✅ Suporte multi-idioma (PT, EN)

---

## 🚀 Recursos Planejados

### v2.1 (Q2 2024)
- Fila de upload em lote
- UI de filtro avançado
- Export para CSV/Excel
- Detecção de duplicatas aprimorada

### v2.2 (Q3 2024)
- Colaboração multi-usuário
- Workspaces em equipe
- Logging de atividades
- Suporte a campos customizados

### v3.0 (Q4 2024)
- App móvel (iOS/Android)
- Interface web
- Sincronização em tempo real
- Analytics avançado

---

## 📚 Recursos

- **FastAPI**: https://fastapi.tiangolo.com/
- **React**: https://react.dev/
- **MongoDB**: https://docs.mongodb.com/
- **Ollama**: https://ollama.ai/
- **TypeScript**: https://www.typescriptlang.org/

---

## 🙏 Agradecimentos

Construído com projetos open-source incríveis:
- FastAPI, React, MongoDB, Ollama, pdfplumber, PyInstaller

---

**Etnopapers v2.0** - Tornando a pesquisa etnobotânica mais acessível

Feito com ❤️ para pesquisadores de etnobotânica e comunidades indígenas em todo o mundo

🌿 🔬 📚 🌍
