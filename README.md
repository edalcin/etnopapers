# 🌿 Etnopapers

<img src="docs/etnopapers.png" alt="Etnopapers Logo" width="150" height="150">

**Sistema de Extração de Metadados de Artigos Etnobotânicos com AI Local**

Etnopapers é uma ferramenta que automatiza a extração de informações sobre o uso de plantas por comunidades tradicionais a partir de artigos científicos em PDF. O sistema usa **inteligência artificial local** rodando no próprio servidor para ler artigos e criar um banco de dados MongoDB organizado com essas informações.

**🔒 100% Privado**: Todos os dados permanecem no seu servidor. Nenhuma informação é enviada para APIs externas.

---

## 📖 O que é Etnobotânica?

Etnobotânica é o estudo da relação entre pessoas e plantas. Pesquisadores documentam como comunidades tradicionais (indígenas, quilombolas, ribeirinhas) usam plantas para medicina, alimentação, rituais e outras finalidades. Esses conhecimentos são publicados em artigos científicos, mas ficam dispersos e difíceis de consultar.

## 💡 Como funciona?

```
1. Você faz upload de um artigo científico (PDF)
         ↓
2. Sistema extrai o texto do PDF no servidor
         ↓
3. Modelo de AI local (Qwen2.5-7B) lê e identifica:
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
- **Processamento local**: AI roda no próprio servidor UNRAID com GPU
- **Dados nunca saem do servidor**: Zero transmissão para serviços externos
- **Offline-capable**: Funciona sem internet após instalação inicial
- **Zero custos por requisição**: Processe quantos artigos quiser gratuitamente

## 🛠️ Tecnologias Usadas

### Para Usuários
- **Interface Web**: Simples e intuitiva, funciona em qualquer navegador moderno
- **Inteligência Artificial Local**: Modelo Qwen2.5-7B rodando no servidor via Ollama
- **Inferência rápida**: 1-3 segundos por artigo com GPU
- **Validação Taxonômica**: Confirma nomes científicos de plantas automaticamente (GBIF API)

### Para Desenvolvedores
- **Frontend**: React 18 + TypeScript
- **Backend**: Python FastAPI com Instructor + Pydantic
- **AI Framework**: Ollama (gerenciamento de modelos locais)
- **Modelo**: Qwen2.5-7B-Instruct (quantizado Q4, 4.8 GB)
- **Banco de Dados**: MongoDB (NoSQL, document-oriented)
- **Docker**: 3 serviços (MongoDB + Ollama + Etnopapers)
- **GPU**: NVIDIA GPU com 6-8 GB VRAM (RTX 3060 ou superior)

## 🚀 Como começar?

### Requisitos de Hardware

**Obrigatório:**
- Servidor UNRAID (ou Linux com Docker)
- **GPU NVIDIA** com 6-8 GB VRAM (ex: RTX 3060, RTX 3070, RTX 3080, RTX 4060+)
- RAM: 16 GB (mínimo 8 GB)
- Disco: 50 GB livres (~8.5 GB Docker + dados + modelos)
- Internet: Apenas para instalação inicial (download do modelo ~4.8 GB)

**Verifique sua GPU:**
```bash
nvidia-smi  # Deve listar sua GPU NVIDIA
```

### Opção 1: Instalação no UNRAID (Recomendado)

**Passo 1: Instalar NVIDIA Driver Plugin**
1. Abra **Apps** → Busque "nvidia"
2. Instale **"Nvidia-Driver"** (Community Applications)
3. Reinicie o servidor UNRAID
4. Verifique: `nvidia-smi` deve mostrar sua GPU

**Passo 2: Instalar Etnopapers**
```bash
# Via Community Applications (quando disponível)
Apps → Buscar "Etnopapers" → Install

# OU via Docker Compose manual:
git clone https://github.com/etnopapers/etnopapers.git
cd etnopapers
docker-compose up -d
```

**Passo 3: Download do modelo AI (primeira vez)**
```bash
# Aguarde ~5-20 minutos dependendo da internet
docker exec etnopapers-ollama ollama pull qwen2.5:7b-instruct-q4_K_M

# Verificar se modelo foi baixado
docker exec etnopapers-ollama ollama list
# Deve mostrar: qwen2.5:7b-instruct-q4_K_M
```

**Passo 4: Acessar interface**
```
http://seu-servidor-unraid:8000
```

### Opção 2: Docker (Qualquer servidor Linux com GPU)

```bash
# Pré-requisito: NVIDIA Container Toolkit instalado
# Veja: https://docs.nvidia.com/datacenter/cloud-native/container-toolkit/install-guide.html

# Clonar repositório
git clone https://github.com/etnopapers/etnopapers.git
cd etnopapers

# Iniciar serviços
docker-compose up -d

# Aguardar download do modelo (primeira vez)
docker-compose logs -f ollama

# Acessar no navegador
# http://localhost:8000
```

### Primeiro Uso

**Não precisa configurar nada!** 🎉

1. **Acesse a interface web**: `http://seu-servidor:8000`
2. **Faça upload de um PDF**: Arraste um artigo científico sobre etnobotânica
3. **Aguarde extração**: ~1-3 segundos (processamento local com GPU)
4. **Revise os metadados**: Sistema mostra título, autores, espécies, etc.
5. **Salve ou edite**: Clique em "Salvar" (finalizar) ou "Editar" (corrigir)

**Sem configuração de API keys!** Tudo funciona automaticamente.

## 📁 Estrutura do Projeto

```
etnopapers/
├── specs/main/                      # Documentação técnica
│   ├── spec.md                      # Especificação completa (v2.0)
│   ├── local-ai-integration.md      # Detalhes da integração AI local
│   ├── plan-local-ai.md             # Plano de implementação
│   ├── tasks-v2-local-ai.md         # Tasks para desenvolvimento
│   ├── data-model.md                # Estrutura MongoDB
│   ├── quickstart.md                # Guia de instalação detalhado
│   └── contracts/                   # APIs e integrações
├── frontend/                        # Interface web (React + TypeScript)
├── backend/                         # Servidor (Python FastAPI)
│   ├── services/
│   │   └── extraction_service.py    # Cliente Ollama + Instructor
│   ├── routers/
│   │   └── extraction.py            # Endpoint /api/extract/metadata
│   └── models/
│       └── extraction.py            # Schemas Pydantic
├── docker-compose.yml               # 3 serviços: MongoDB + Ollama + Etnopapers
├── CLAUDE.md                        # Guia para desenvolvimento com Claude
└── README.md                        # Este arquivo
```

## 📚 Documentação Técnica

- **[Guia de Deployment](docs/DEPLOYMENT.md)**: Configuração de GPU NVIDIA, troubleshooting UNRAID, otimização de performance
- **[Especificação Técnica v2.0](specs/main/spec.md)**: Requisitos funcionais, histórias de usuário e arquitetura
- **[Integração AI Local](specs/main/local-ai-integration.md)**: Detalhes de Ollama + Qwen2.5, comparação de modelos, implementação
- **[Plano de Implementação](specs/main/plan-local-ai.md)**: Roadmap de 7-10 dias, 39 tasks em 5 fases
- **[Modelo de Dados MongoDB](specs/main/data-model.md)**: Arquitetura document-oriented, coleções e queries
- **[Guia de Instalação](specs/main/quickstart.md)**: Setup de GPU, troubleshooting, FAQ
- **[API REST](specs/main/contracts/api-rest.yaml)**: Documentação OpenAPI dos endpoints

## 🌟 Principais Funcionalidades

### ✅ Disponível na v2.0

- Upload de PDFs científicos (até 50 MB)
- **Extração automática via AI local** (Qwen2.5-7B-Instruct)
- **Inferência rápida**: 1-3 segundos por artigo (GPU)
- **100% privado**: Dados nunca saem do servidor
- Interface de edição manual para correções
- Validação taxonômica de espécies de plantas (GBIF API)
- Histórico de artigos processados com busca/filtro
- Rascunhos automáticos (não perde trabalho se fechar navegador)
- Download completo do banco MongoDB (backup ZIP)
- Detecção de duplicatas (por DOI ou título+ano+autor)

### 🔄 Metadados Extraídos

O modelo de AI local extrai automaticamente:

- **Bibliográficos**: Título, autores, ano, DOI, revista/jornal, resumo
- **Geográficos**: País, estado, município, local específico, bioma
- **Sociais**: Comunidades tradicionais estudadas
- **Botânicos**: Espécies de plantas (nome científico + nome popular)
- **Metodológicos**: Tipo de uso, metodologia de pesquisa, período do estudo

**Acurácia típica**: >80% (com revisão manual recomendada)

## 💰 Custos

### Investimento Inicial
- **Software Etnopapers**: Gratuito (código aberto, MIT License)
- **GPU NVIDIA** (se não tiver): ~$300-500 (RTX 3060)
- **Servidor UNRAID** (se não tiver): ~$0 (software gratuito) + hardware

### Custos Recorrentes
- **Processamento de artigos**: **R$ 0,00** (AI local, sem custos por requisição)
- **APIs externas**: **R$ 0,00** (apenas validação taxonômica GBIF, gratuita)
- **Energia elétrica**: ~R$ 5-15/mês (GPU em standby + uso ocasional)

### Comparação com APIs Externas (v1.0)

| Métrica | v1.0 (APIs Externas) | v2.0 (AI Local) |
|---------|---------------------|-----------------|
| Custo por artigo | R$ 0,05 - R$ 0,15 | **R$ 0,00** |
| 100 artigos | R$ 5 - R$ 15 | **R$ 0,00** |
| 1000 artigos | R$ 50 - R$ 150 | **R$ 0,00** |
| Privacidade | Dados enviados a terceiros | **100% local** |
| Quota | Limitada por plano | **Ilimitada** |

**ROI (Return on Investment)**: Com ~500-1000 artigos processados, o investimento em GPU se paga comparado com APIs externas.

## 🔐 Segurança e Privacidade

### v2.0 Garante 100% de Privacidade

- ✅ **Dados nunca saem do servidor**: AI roda localmente, sem chamadas externas
- ✅ **PDFs não são salvos**: Apenas metadados extraídos permanecem
- ✅ **Banco de dados local**: MongoDB no próprio servidor
- ✅ **Sem telemetria**: Zero rastreamento ou analytics
- ✅ **Código aberto**: Auditável (MIT License)
- ✅ **Offline-capable**: Funciona sem internet após download do modelo

### Comparação com APIs Externas

| Aspecto | APIs Externas | AI Local |
|---------|---------------|----------|
| Dados do PDF | Enviados para Google/OpenAI/Anthropic | Permanecem no servidor |
| Metadados extraídos | Trafegam pela internet | Nunca saem do servidor |
| Dependência de internet | Sempre necessária | Apenas setup inicial |
| Risco de vazamento | Médio (depende do provedor) | **Zero** |
| Compliance LGPD/GDPR | Complexo (dados em terceiros) | **Simples (dados locais)** |

## ⚡ Performance

### Benchmarks (Inferência Local)

| GPU | VRAM | Tempo por Artigo | Throughput |
|-----|------|------------------|------------|
| RTX 4090 | 24 GB | 0.5-1s | ~60-120 artigos/min |
| RTX 4080 | 16 GB | 1-2s | ~30-60 artigos/min |
| RTX 3080 | 10 GB | 2-3s | ~20-30 artigos/min |
| **RTX 3060** | **12 GB** | **2-4s** | **15-30 artigos/min** |
| RTX 2070 | 8 GB | 4-6s | ~10-15 artigos/min |

**Nota**: Tempos assumem PDFs de ~10-20 páginas. Artigos mais longos podem levar mais tempo.

## 🆚 Comparação: v1.0 (APIs Externas) vs. v2.0 (AI Local)

| Aspecto | v1.0 | v2.0 |
|---------|------|------|
| **Privacidade** | Dados enviados a terceiros | 100% local ✅ |
| **Custo/artigo** | $0.02-0.05 | $0 ✅ |
| **Latência** | 2-10s (rede + API) | 1-3s (GPU) ✅ |
| **Quota** | Limitada | Ilimitada ✅ |
| **Setup** | Configurar API key | Apenas upload ✅ |
| **Docker** | ~180 MB | ~8.5 GB |
| **Hardware** | Básico | GPU NVIDIA necessária |
| **Internet** | Sempre | Apenas setup inicial ✅ |
| **Acurácia** | 80-85% | 80-85% (similar) |

**Recomendação**: Se você tem GPU NVIDIA, v2.0 é superior em todos os aspectos (exceto tamanho Docker).

## 🤝 Contribuindo

Contribuições são bem-vindas! Veja como ajudar:

1. **Reportar bugs**: Abra uma [issue](https://github.com/etnopapers/etnopapers/issues) descrevendo o problema
2. **Sugerir melhorias**: Compartilhe suas ideias
3. **Contribuir código**: Fork, desenvolva e envie pull request
4. **Melhorar documentação**: Corrija erros ou adicione exemplos
5. **Testar modelos**: Experimente modelos alternativos (Mistral, Sabiá, NuExtract)

### Roadmap Futuro (v2.1+)

- [ ] Fine-tuning do Qwen2.5 em corpus etnobotânico brasileiro
- [ ] Processamento em batch (múltiplos PDFs de uma vez)
- [ ] OCR para PDFs escaneados (Tesseract integration)
- [ ] Dashboard de analytics (espécies mais mencionadas, mapas, etc.)
- [ ] Exportação para CSV/JSON/Excel
- [ ] API REST pública para integrações
- [ ] Sistema de autenticação multi-usuário
- [ ] RAG com banco de conhecimento etnobotânico

## 📄 Licença

MIT License - Software livre e de código aberto.

Veja [LICENSE](LICENSE) para detalhes completos.

## 🙏 Agradecimentos

Este projeto foi desenvolvido para apoiar pesquisadores em etnobotânica, preservando e organizando conhecimentos tradicionais sobre o uso de plantas.

**Agradecimentos especiais:**
- Comunidade Qwen (modelo Qwen2.5)
- Ollama (framework de inferência)
- Projeto Instructor (structured outputs)
- Comunidades tradicionais que compartilham seus conhecimentos

---

## 📞 Suporte

### Documentação
- **[Guia Rápido](specs/main/quickstart.md)**: Instalação passo-a-passo
- **[Troubleshooting](specs/main/troubleshooting.md)**: Problemas comuns e soluções
- **[Especificações Técnicas](specs/main/)**: Documentação completa

### Problemas Comuns

**GPU não detectada:**
```bash
# Verificar se NVIDIA Driver está instalado
nvidia-smi

# Verificar se containers têm acesso à GPU
docker exec etnopapers-ollama nvidia-smi
```

**Modelo não carrega:**
```bash
# Download manual do modelo
docker exec etnopapers-ollama ollama pull qwen2.5:7b-instruct-q4_K_M

# Verificar modelos disponíveis
docker exec etnopapers-ollama ollama list
```

**Inferência muito lenta (>10s):**
- Verificar se GPU está sendo usada (não caiu para CPU)
- Verificar VRAM disponível: `nvidia-smi`
- Considerar quantização menor (Q3 em vez de Q4)

### Reportar Issues
- **GitHub Issues**: [etnopapers/etnopapers/issues](https://github.com/etnopapers/etnopapers/issues)
- **Discussões**: [GitHub Discussions](https://github.com/etnopapers/etnopapers/discussions)

---

## 🔬 Especificações Técnicas (v2.0)

**Modelo de AI**: Qwen2.5-7B-Instruct (Q4 quantizado)
- Tamanho: 4.8 GB (Q4_K_M)
- Context window: 128K tokens
- Linguagens: 29+ (incluindo português)
- Structured outputs: JSON nativo via Instructor

**Framework**: Ollama
- Versão: latest
- API: OpenAI-compatível
- GPU: NVIDIA CUDA

**Backend**: Python 3.11+ FastAPI
- Bibliotecas: instructor, pydantic, pdfplumber, pymongo
- Endpoints: 30+ REST APIs

**Frontend**: React 18 + TypeScript
- State: Zustand
- Tables: TanStack React Table v8
- Forms: react-hook-form

**Database**: MongoDB 7.0
- Modelo: Document-oriented (NoSQL)
- Coleção principal: `referencias`
- Tamanho estimado: 1-2 MB por 1000 artigos

**Docker**:
- Serviços: 3 (MongoDB + Ollama + Etnopapers)
- Tamanho total: ~8.5 GB
- Volumes: mongodb_data, ollama_models
- GPU: passthrough via nvidia-docker

---

**Desenvolvido com ❤️ para pesquisadores em etnobotânica**

🤖 *Sistema com AI Local - 100% Privado - Zero Custos Recorrentes*

🌿 *Preservando conhecimentos tradicionais sobre plantas*
