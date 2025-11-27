# Etnopapers - Guia de Instalação Standalone

Guia completo para instalação e uso do Etnopapers como aplicação standalone nativa (sem Docker).

## Sumário

1. [Pré-requisitos](#pré-requisitos)
2. [Instalação do Ollama](#instalação-do-ollama)
3. [Download do Etnopapers](#download-do-etnopapers)
4. [Primeira Execução](#primeira-execução)
5. [Configuração do MongoDB](#configuração-do-mongodb)
6. [Uso da Aplicação](#uso-da-aplicação)
7. [Solução de Problemas](#solução-de-problemas)

---

## Pré-requisitos

### Hardware Mínimo
- **CPU**: 4 cores (recomendado: 6+ cores)
- **RAM**: 8 GB (recomendado: 16 GB)
- **Disco**: 10 GB livres
- **GPU**: Nvidia GPU com 4+ GB VRAM (opcional, mas recomendado para melhor performance)

### Software Necessário
1. **Ollama** - Para inferência AI local
2. **MongoDB** - Database (local ou cloud via MongoDB Atlas)

---

## Instalação do Ollama

### Windows

1. Baixar instalador:
   ```
   https://ollama.ai/download/windows
   ```

2. Executar `OllamaSetup.exe`

3. Ollama iniciará automaticamente como serviço do Windows

4. Baixar o modelo de AI:
   ```powershell
   ollama pull qwen2.5:7b-instruct-q4_K_M
   ```

**Verificação:**
```powershell
ollama list
# Deve mostrar qwen2.5:7b-instruct-q4_K_M na lista
```

---

### macOS

1. Baixar Ollama:
   ```
   https://ollama.ai/download/mac
   ```

2. Arrastar `Ollama.app` para pasta Applications

3. Abrir Ollama (ícone aparecerá na barra de menu)

4. Baixar o modelo de AI:
   ```bash
   ollama pull qwen2.5:7b-instruct-q4_K_M
   ```

**Verificação:**
```bash
ollama list
# Deve mostrar qwen2.5:7b-instruct-q4_K_M na lista
```

---

### Linux

1. Instalar Ollama:
   ```bash
   curl -fsSL https://ollama.ai/install.sh | sh
   ```

2. Iniciar serviço:
   ```bash
   sudo systemctl enable ollama
   sudo systemctl start ollama
   ```

3. Baixar o modelo de AI:
   ```bash
   ollama pull qwen2.5:7b-instruct-q4_K_M
   ```

**Verificação:**
```bash
ollama list
# Deve mostrar qwen2.5:7b-instruct-q4_K_M na lista
```

---

## Download do Etnopapers

### Releases Oficiais (Recomendado)

Acesse a página de releases no GitHub:
```
https://github.com/[seu-usuario]/etnopapers/releases
```

Baixe o pacote para seu sistema operacional:

- **Windows**: `Etnopapers-Windows-v1.0.0.zip`
- **macOS**: `Etnopapers-macOS-v1.0.0.zip`
- **Linux**: `Etnopapers-Linux-v1.0.0.tar.gz`

### Extrair Arquivos

**Windows:**
```powershell
# Extrair ZIP
Expand-Archive -Path Etnopapers-Windows-v1.0.0.zip -DestinationPath C:\Etnopapers
```

**macOS/Linux:**
```bash
# ZIP (macOS)
unzip Etnopapers-macOS-v1.0.0.zip -d /Applications

# TAR.GZ (Linux)
tar -xzf Etnopapers-Linux-v1.0.0.tar.gz -C /opt/etnopapers
```

---

## Primeira Execução

### Windows

1. Navegar até pasta extraída
2. Duplo-clique em `etnopapers.exe`
3. Janela de configuração aparecerá
4. Inserir MongoDB URI (ver [Configuração do MongoDB](#configuração-do-mongodb))
5. Clicar em "Salvar e Iniciar"
6. Navegador abrirá automaticamente em `http://localhost:8000`

### macOS

1. Abrir `Etnopapers.app` (Applications ou pasta extraída)
2. **Primeira vez**: macOS pode pedir permissão de segurança
   - Ir em: **System Preferences → Security & Privacy**
   - Clicar em "Open Anyway"
3. Janela de configuração aparecerá
4. Inserir MongoDB URI
5. Clicar em "Salvar e Iniciar"
6. Navegador abrirá automaticamente

### Linux

1. Dar permissão de execução:
   ```bash
   chmod +x etnopapers
   ```

2. Executar:
   ```bash
   ./etnopapers
   ```

3. Janela de configuração aparecerá (ou terminal mostrará prompts)
4. Inserir MongoDB URI
5. Navegador abrirá automaticamente

---

## Configuração do MongoDB

Você tem duas opções para configurar o MongoDB:

### Opção 1: MongoDB Atlas (Cloud - Recomendado)

**Vantagens**: Sem instalação local, backup automático, gratuito até 512 MB

1. Criar conta em [https://www.mongodb.com/cloud/atlas](https://www.mongodb.com/cloud/atlas)

2. Criar cluster gratuito (M0)

3. Criar usuário do database:
   - Database Access → Add New Database User
   - Username: `etnopapers`
   - Password: `[sua-senha-segura]`

4. Permitir acesso de qualquer IP:
   - Network Access → Add IP Address
   - IP: `0.0.0.0/0` (ou seu IP específico para mais segurança)

5. Obter Connection String:
   - Clusters → Connect → Connect your application
   - Copiar URI (formato: `mongodb+srv://...`)

6. **MongoDB URI para o Etnopapers:**
   ```
   mongodb+srv://etnopapers:[sua-senha]@cluster0.xxxxx.mongodb.net/etnopapers?retryWrites=true&w=majority
   ```

---

### Opção 2: MongoDB Local

**Vantagens**: Dados totalmente locais, sem necessidade de internet

**Windows:**
1. Baixar MongoDB Community Server:
   ```
   https://www.mongodb.com/try/download/community
   ```

2. Instalar (deixar opção "Install MongoDB as a Service" marcada)

3. **MongoDB URI:**
   ```
   mongodb://localhost:27017/etnopapers
   ```

**macOS:**
```bash
# Instalar via Homebrew
brew tap mongodb/brew
brew install mongodb-community

# Iniciar serviço
brew services start mongodb-community

# MongoDB URI:
mongodb://localhost:27017/etnopapers
```

**Linux (Ubuntu/Debian):**
```bash
# Adicionar repositório MongoDB
wget -qO - https://www.mongodb.org/static/pgp/server-6.0.asc | sudo apt-key add -
echo "deb [ arch=amd64,arm64 ] https://repo.mongodb.org/apt/ubuntu focal/mongodb-org/6.0 multiverse" | sudo tee /etc/apt/sources.list.d/mongodb-org-6.0.list

# Instalar
sudo apt-get update
sudo apt-get install -y mongodb-org

# Iniciar serviço
sudo systemctl start mongod
sudo systemctl enable mongod

# MongoDB URI:
mongodb://localhost:27017/etnopapers
```

---

## Uso da Aplicação

### Fluxo Básico

1. **Acessar aplicação**: `http://localhost:8000`

2. **Upload de PDF**:
   - Clicar em "Upload" ou arrastar PDF para área
   - Tamanho máximo: 50 MB

3. **Extração Automática**:
   - AI processa PDF (1-5 segundos dependendo da GPU)
   - Metadados são extraídos automaticamente

4. **Revisão e Edição**:
   - Revisar dados extraídos
   - Editar campos conforme necessário
   - Validação taxonômica automática (GBIF)

5. **Salvar**:
   - Clicar em "Salvar" para persistir no MongoDB
   - Status muda de "rascunho" para "finalizado"

6. **Consultar Dados**:
   - Página "Histórico" mostra todos os artigos salvos
   - Filtros por ano, espécie, comunidade, localização

---

## Solução de Problemas

### Erro: "Ollama não está rodando"

**Causa**: Ollama não está acessível

**Solução**:
```bash
# Verificar status
curl http://localhost:11434/api/tags

# Se erro, reiniciar Ollama:
# Windows: Reiniciar serviço "Ollama" no Services
# macOS: Quit e abrir Ollama.app novamente
# Linux: sudo systemctl restart ollama
```

---

### Erro: "Cannot connect to MongoDB"

**Causa**: MongoDB URI inválido ou MongoDB não acessível

**Solução**:
1. Verificar MongoDB URI no arquivo `.env`
2. Testar conexão:
   ```bash
   # MongoDB local
   mongosh mongodb://localhost:27017/etnopapers

   # MongoDB Atlas
   mongosh "mongodb+srv://user:pass@cluster.mongodb.net/etnopapers"
   ```

3. Se erro persiste, recriar arquivo `.env`:
   ```bash
   # Deletar .env
   rm .env

   # Reiniciar Etnopapers
   # GUI de configuração aparecerá novamente
   ```

---

### Erro: "Model not found"

**Causa**: Modelo Ollama não baixado

**Solução**:
```bash
# Baixar modelo
ollama pull qwen2.5:7b-instruct-q4_K_M

# Verificar
ollama list

# Reiniciar Etnopapers
```

---

### Aplicação não abre navegador automaticamente

**Causa**: Navegador padrão não configurado ou firewall

**Solução**:
1. Abrir navegador manualmente
2. Acessar: `http://localhost:8000`

---

### Port 8000 já em uso

**Causa**: Outra aplicação usando porta 8000

**Solução**:
1. Editar arquivo `.env`
2. Alterar `PORT=8000` para outra porta (ex: `PORT=8080`)
3. Reiniciar Etnopapers
4. Acessar: `http://localhost:8080`

---

### GPU não sendo usada (performance ruim)

**Causa**: Ollama não detectando GPU

**Verificação**:
```bash
# Linux
nvidia-smi

# Deve mostrar processo "ollama" usando GPU
```

**Solução**:
- Reinstalar drivers Nvidia
- Verificar instalação CUDA
- Ver logs do Ollama para mais detalhes

---

## Atualizações

Para atualizar para nova versão:

1. Baixar novo release do GitHub
2. Extrair em pasta diferente (ou sobrescrever)
3. **IMPORTANTE**: Copiar arquivo `.env` antigo para nova versão
4. Executar nova versão

**Configuração é preservada** no arquivo `.env`.

---

## Desinstalação

### Windows
1. Deletar pasta `C:\Etnopapers`
2. Deletar arquivo `.env` (se quiser remover configuração)
3. (Opcional) Desinstalar Ollama via "Add or Remove Programs"

### macOS
1. Deletar `Etnopapers.app` de Applications
2. Deletar `.env` do diretório home
3. (Opcional) Desinstalar Ollama arrastando para Trash

### Linux
1. Deletar pasta `/opt/etnopapers` (ou onde instalou)
2. Deletar `.env`
3. (Opcional) Desinstalar Ollama: `sudo apt-get remove ollama`

---

## Suporte

- **Issues**: [https://github.com/[seu-usuario]/etnopapers/issues](https://github.com/[seu-usuario]/etnopapers/issues)
- **Documentação**: [README.md](../README.md)
- **Build from Source**: [docs/build-from-source.md](build-from-source.md)
