# Guia de Início Rápido: Etnopapers

**Funcionalidade**: Sistema de Extração de Metadados de Artigos Etnobotânicos
**Branch**: main
**Criado**: 2025-11-20
**Status**: Em Desenvolvimento

## Visão Geral

Este guia fornece instruções passo-a-passo para configurar e executar o Etnopapers em seu servidor. O processo completo leva aproximadamente **10 minutos**.

**Arquitetura Simplificada**:
- ✅ Docker padrão (sem necessidade de GPU)
- ✅ APIs externas de IA (Gemini, ChatGPT, Claude)
- ✅ Chaves de API fornecidas pelo usuário
- ✅ Banco de dados Mongita (NoSQL, document-oriented, MongoDB-compatible)
- ✅ Interface web responsiva
- ✅ Docker leve: ~180-200MB (40% mais leve que SQL+ORM)

## Pré-requisitos

### Hardware Mínimo

- **CPU**: 2 cores
- **RAM**: 1 GB
- **Disco**: 10 GB de espaço livre
- **Rede**: Conexão à internet para acesso a APIs externas

### Software Necessário

- **Docker**: Versão 20.10 ou superior
- **Docker Compose** (opcional, mas recomendado): Versão 1.29 ou superior
- **Navegador Web**: Chrome, Firefox, Safari ou Edge (versões recentes)

### Conta de API de IA

Você precisará de uma chave de API de **pelo menos um** dos seguintes provedores:

1. **Google Gemini** (recomendado para começar)
   - Gratuito com quota generosa
   - Criar conta: https://makersuite.google.com/app/apikey

2. **OpenAI ChatGPT**
   - Requer créditos pagos após quota inicial
   - Criar conta: https://platform.openai.com/api-keys

3. **Anthropic Claude**
   - Modelos com diferentes níveis de custo
   - Criar conta: https://console.anthropic.com/settings/keys

## Instalação no UNRAID

### Passo 0: Preparar Diretório no Host (IMPORTANTE!)

⚠️ **Antes de instalar o container, você deve criar o diretório de dados no UNRAID:**

1. Acesse o **UNRAID Web UI**
2. Vá para **Main** → **Flash** ou abra Terminal SSH
3. Execute:

```bash
mkdir -p /mnt/user/appdata/etnopapers/
chmod 777 /mnt/user/appdata/etnopapers/
```

**Ou via UNRAID Web UI**:
- Settings → Shares → appdata → Create
- Ou use File Manager para criar pasta `etnopapers`

### Passo 1: Instalar via Community Applications

1. Acesse o **UNRAID Web UI**
2. Navegue para **Apps** → **Community Applications**
3. Busque por **"Etnopapers"**
4. Clique em **Install**
5. Configure as opções:

```
Container Name: etnopapers
Port (WebUI): 8007 → 8000
Path (Database): /mnt/user/appdata/etnopapers → /data
Environment Variables:
  DATABASE_BACKEND=disk
  DATABASE_PATH=/data  # Apontapara volume montado
```

6. Clique em **Apply**

### Passo 2: Acessar a Interface Web

1. Abra navegador e acesse: `http://SEU_IP_UNRAID:8000`
2. Você verá a tela de configuração de API key

### Passo 3: Configurar API Key

1. Selecione o provedor de IA (Gemini, ChatGPT ou Claude)
2. Clique em **"Como obter chave de API"** para instruções
3. Cole sua chave de API no campo
4. Clique em **"Validar e Salvar"**
5. Se válida, você será direcionado para a tela de upload

**Pronto!** Você já pode fazer upload de PDFs.

---

## Instalação Manual com Docker

### Passo 1: Clonar o Repositório (quando disponível)

```bash
git clone https://github.com/etnopapers/etnopapers.git
cd etnopapers
```

### Passo 2: Criar Estrutura de Diretórios

```bash
mkdir -p data
```

### Passo 3: Configurar Variáveis de Ambiente

Crie arquivo `.env` na raiz do projeto:

```env
# Banco de Dados (Mongita - NoSQL)
DATABASE_BACKEND=disk                # disk (persistente) | memory (teste)
DATABASE_PATH=/data/etnopapers       # Diretório onde Mongita armazena documentos BSON

# Porta da aplicação
PORT=8000

# Nível de log (debug|info|warning|error)
LOG_LEVEL=info

# Timeout para API de taxonomia (segundos)
TAXONOMY_API_TIMEOUT=5

# TTL do cache de taxonomia (dias)
CACHE_TTL_DAYS=30
```

**Notas sobre Mongita**:
- `DATABASE_PATH` é um **diretório**, não um arquivo (Mongita cria arquivos binários BSON internamente)
- Mongita auto-cria a estrutura na primeira execução
- Dados são armazenados em formato BSON (20% mais compacto que JSON)

### Passo 4: Executar com Docker Compose

Crie arquivo `docker-compose.yml`:

```yaml
version: '3.8'

services:
  etnopapers:
    image: ghcr.io/edalcin/etnopapers:latest
    container_name: etnopapers
    ports:
      - "8000:8000"
    volumes:
      - ./data:/data
    environment:
      - DATABASE_BACKEND=disk
      - DATABASE_PATH=/data/etnopapers
      - PORT=8000
      - LOG_LEVEL=info
      - TAXONOMY_API_TIMEOUT=5
      - CACHE_TTL_DAYS=30
    restart: unless-stopped
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8000/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 5s
```

Execute:

```bash
docker-compose up -d
```

### Passo 5: Verificar Status

```bash
docker-compose logs -f
```

Você deve ver:

```
INFO:     Started server process
INFO:     Uvicorn running on http://0.0.0.0:8000
INFO:     Database initialized successfully
```

### Passo 6: Acessar Interface

Abra navegador: `http://localhost:8000`

---

## Instalação Manual sem Docker Compose

### Build da Imagem Docker

```bash
docker build -t etnopapers:latest .
```

### Executar Container

```bash
docker run -d \
  --name etnopapers \
  -p 8000:8000 \
  -v $(pwd)/data:/data \
  -e DATABASE_BACKEND=disk \
  -e DATABASE_PATH=/data/etnopapers \
  -e PORT=8000 \
  -e LOG_LEVEL=info \
  --restart unless-stopped \
  ghcr.io/edalcin/etnopapers:latest
```

### Verificar Logs

```bash
docker logs -f etnopapers
```

---

## Primeiro Uso: Configuração de API Key

### Google Gemini (Recomendado)

**Passo a Passo**:

1. Acesse: https://makersuite.google.com/app/apikey
2. Faça login com sua conta Google
3. Clique em **"Create API Key"**
4. Selecione ou crie um projeto do Google Cloud
5. Copie a chave gerada (formato: `AIzaSy...`)
6. No Etnopapers:
   - Selecione **"Google Gemini"**
   - Cole a chave
   - Clique em **"Validar e Salvar"**

**Limites Gratuitos**:
- 60 requests por minuto
- Adequado para uso individual/acadêmico

### OpenAI ChatGPT

**Passo a Passo**:

1. Acesse: https://platform.openai.com/signup
2. Crie uma conta OpenAI
3. Navegue para **API Keys**: https://platform.openai.com/api-keys
4. Clique em **"Create new secret key"**
5. Dê um nome (ex: "Etnopapers")
6. Copie a chave (formato: `sk-...`) - **ela só será exibida uma vez**
7. No Etnopapers:
   - Selecione **"OpenAI ChatGPT"**
   - Cole a chave
   - Clique em **"Validar e Salvar"**

**Custos**:
- gpt-3.5-turbo: ~$0.02 por artigo de 30 páginas
- Requer adicionar créditos após quota inicial gratuita

### Anthropic Claude

**Passo a Passo**:

1. Acesse: https://console.anthropic.com
2. Crie uma conta Anthropic
3. Navegue para **API Keys**: https://console.anthropic.com/settings/keys
4. Clique em **"Create Key"**
5. Dê um nome (ex: "Etnopapers")
6. Copie a chave (formato: `sk-ant-...`)
7. No Etnopapers:
   - Selecione **"Anthropic Claude"**
   - Cole a chave
   - Clique em **"Validar e Salvar"**

**Custos**:
- claude-3-haiku: ~$0.003 por artigo de 30 páginas (mais econômico)
- claude-3-sonnet: ~$0.03 por artigo (melhor qualidade)

---

## Usando o Sistema: Processando Seu Primeiro Artigo

### Visão Geral da Página Principal

Ao acessar o sistema, você verá:

**Seção Superior: Upload de PDF**
- Área de arraste e solte (drag & drop)
- Botão "Selecionar PDF"
- Botão "Download Base de Dados" (no canto superior direito)

**Seção Inferior: Tabela de Artigos Processados**
- Lista de todos os artigos já inseridos no banco
- Colunas: Título, Ano, Autores, Status, Data de Processamento, Nº Espécies
- Campo de busca/filtro no topo da tabela
- Colunas ordenáveis (clique no cabeçalho)
- Paginação automática (50 artigos por página)

Se ainda não houver artigos processados, você verá:
> **"Nenhum artigo processado ainda. Faça upload do primeiro PDF!"**

### Passo 1: Upload de PDF

1. Na tela principal, clique em **"Selecionar PDF"** ou arraste um arquivo
2. Formatos aceitos: `.pdf` (máximo 50 MB)
3. O sistema validará:
   - Formato do arquivo (apenas PDF)
   - Tamanho (até 50 MB)
   - Integridade do PDF

### Passo 2: Processamento Automático

1. Clique em **"Processar"**
2. O sistema irá:
   - Extrair texto do PDF (usando pdf.js)
   - Enviar para API de IA selecionada
   - Processar metadados (1-2 minutos)
3. Barra de progresso indica status

### Passo 3: Revisar Metadados Extraídos

Após processamento, você verá:

**Metadados Extraídos**:
- ✅ Título
- ✅ Autores
- ✅ Ano de publicação
- ✅ Resumo
- ✅ DOI (se disponível)
- ✅ Região do estudo
- ✅ Comunidades tradicionais
- ✅ Espécies de plantas
- ✅ Dados metodológicos

**Campos não extraídos** serão marcados com ⚠️.

### Passo 4: Decidir Ação

Três botões estarão disponíveis:

**"Salvar"**:
- Finaliza e armazena no banco de dados
- Valida taxonomia das espécies
- Status: "finalizado"

**"Editar"**:
- Abre interface de edição
- Permite corrigir/completar metadados
- Ideal para PDFs escaneados ou baixa qualidade

**"Descartar"**:
- Remove dados extraídos
- Solicita confirmação
- Irreversível

**Fechou a janela sem clicar?**
- Dados salvos automaticamente como **rascunho**
- Recuperáveis em "Rascunhos Pendentes"

---

## Interface de Edição Manual

### Quando Usar

- PDF escaneado (baixa qualidade de extração)
- Campos importantes faltando
- Erros na extração de nomes científicos
- Complementar informações

### Como Usar

1. Clique em **"Editar"** após extração
2. Todos os campos são editáveis:
   - Texto simples: título, DOI, resumo
   - Listas: autores, regiões, comunidades
   - Tabelas: espécies de plantas
3. **Validações automáticas**:
   - Ano de publicação (1900-2100)
   - Formato de DOI (10.xxxx/xxxx)
   - Nome científico (binomial: Genus species)
4. Campos editados ficam marcados com 📝
5. Clique em **"Salvar Alterações"** para finalizar

### Validação Taxonômica

Ao adicionar/editar espécie:

1. Digite nome científico (ex: "Uncaria tomentosa")
2. Sistema consulta API de taxonomia (GBIF ou Tropicos)
3. Retorna:
   - ✅ Nome aceito atual
   - ✅ Família botânica
   - ✅ Autores do nome científico
   - ⚠️ Se sinônimo, sugere nome válido
4. Validação salva automaticamente

**Se API estiver offline**:
- Sistema permite salvamento
- Status: "não validado"
- Pode validar posteriormente

---

## Navegando pela Tabela de Artigos

### Visualizar Artigos Processados

A tabela na página principal exibe automaticamente todos os artigos processados.

**Colunas da Tabela**:
- **Título**: Título completo do artigo
- **Ano**: Ano de publicação
- **Autores**: Primeiro autor + "et al." (se múltiplos autores)
- **Status**: Finalizado ou Rascunho
- **Processado em**: Data e hora do processamento
- **Espécies**: Número de espécies de plantas mencionadas

### Ordenar Artigos

Clique no cabeçalho de qualquer coluna para ordenar:

1. **Primeiro clique**: Ordem crescente ↑
2. **Segundo clique**: Ordem decrescente ↓
3. **Terceiro clique**: Remove ordenação

**Colunas ordenáveis**:
- Título (alfabética)
- Ano (numérica)
- Status (alfabética)
- Data de processamento (cronológica)
- Número de espécies (numérica)

### Buscar e Filtrar Artigos

**Campo de Busca** (acima da tabela):

1. Digite qualquer termo no campo de busca
2. Sistema filtra em tempo real (300ms de delay)
3. Busca em todos os campos visíveis:
   - Título
   - Ano
   - Autores
   - Status

**Exemplos**:
- Digite `"2023"` → mostra artigos de 2023
- Digite `"Silva"` → mostra artigos com autor Silva
- Digite `"Amazônia"` → mostra artigos com "Amazônia" no título
- Digite `"rascunho"` → mostra apenas rascunhos

**Limpar Filtro**:
- Clique no ❌ no campo de busca
- Ou apague o texto manualmente

**Sem Resultados**:
Se nenhum artigo corresponder ao filtro, você verá:
> **"Nenhum artigo encontrado com esse filtro"**

### Paginação

A tabela exibe **50 artigos por página**.

**Navegação**:
- **Primeira** página: ⏮️
- **Anterior**: ◀️
- **Próxima**: ▶️
- **Última** página: ⏭️

**Indicador** (abaixo da tabela):
> "Mostrando 1-50 de 237 artigos"

**Ir para página específica**:
- Digite o número da página
- Pressione Enter

### Visualizar Detalhes de um Artigo

1. Clique em qualquer linha da tabela
2. Sistema abre modal/página com todos os metadados:
   - Informações bibliográficas completas
   - Regiões e comunidades
   - Espécies mencionadas com usos e partes utilizadas
   - Dados metodológicos do estudo
3. **Botões de ação**:
   - **"Editar"**: Modificar metadados
   - **"Excluir"**: Remover artigo (solicita confirmação)
   - **"Fechar"**: Voltar para tabela

---

## Download do Banco de Dados

### Como Baixar

**Botão "Download Base de Dados"** (canto superior direito):

1. Clique no botão
2. Sistema:
   - Verifica integridade do banco (PRAGMA integrity_check)
   - Gera arquivo com nome: `etnopapers_YYYYMMDD.db`
   - Inicia download automaticamente
3. Aguarde conclusão (varia conforme tamanho)

**Exemplo de nome**:
- `etnopapers_20251120.db` (processado em 20 de novembro de 2025)

### Tamanho do Arquivo

O tamanho varia conforme número de artigos processados:

| Artigos | Tamanho Estimado |
|---------|------------------|
| 10      | ~200 KB          |
| 100     | ~2 MB            |
| 1.000   | ~20 MB           |
| 10.000  | ~200 MB          |

**Download Demorado?**
- Bancos > 50 MB podem levar alguns minutos
- Aguarde pacientemente
- Progresso exibido pelo navegador

### O que fazer com o arquivo baixado?

**1. Backup Local**
```bash
# Copiar para diretório de backup
cp etnopapers_20251120.db ~/Backups/Etnopapers/
```

**2. Abrir em ferramenta SQLite**

**DB Browser for SQLite** (recomendado):
- Download: https://sqlitebrowser.org/
- Abra o arquivo .db baixado
- Navegue pelas tabelas visualmente
- Execute queries SQL

**DBeaver**:
- Ferramenta universal de banco de dados
- Suporta SQLite e dezenas de outros bancos
- Download: https://dbeaver.io/

**3. Importar em Python/R para Análise**

**Python**:
```python
import sqlite3
import pandas as pd

# Conectar ao banco
conn = sqlite3.connect('etnopapers_20251120.db')

# Carregar artigos em DataFrame
df_artigos = pd.read_sql_query("SELECT * FROM ArtigosCientificos", conn)

# Carregar espécies
df_especies = pd.read_sql_query("SELECT * FROM EspeciesPlantas", conn)

# Análise...
print(df_artigos.describe())

conn.close()
```

**R**:
```r
library(DBI)
library(RSQLite)

# Conectar ao banco
conn <- dbConnect(SQLite(), "etnopapers_20251120.db")

# Carregar tabelas
artigos <- dbReadTable(conn, "ArtigosCientificos")
especies <- dbReadTable(conn, "EspeciesPlantas")

# Análise...
summary(artigos)

dbDisconnect(conn)
```

**4. Compartilhar com Colaboradores**

Envie o arquivo `.db` por:
- Email (se < 25 MB)
- Google Drive / Dropbox
- Pendrive / HD externo

Colaboradores podem importar em suas instâncias do Etnopapers.

### Informações do Banco de Dados

Para verificar informações **antes** de baixar:

**Menu**: **"Configurações"** → **"Info do Banco"**

Exibe:
- **Tamanho**: 2.5 MB (2,621,440 bytes)
- **Total de artigos**: 237
- **Total de espécies**: 156
- **Última modificação**: 2025-11-20 15:30:45
- **Status de integridade**: ✅ OK

---

## Gerenciamento de Rascunhos

### Visualizar Rascunhos

Menu: **"Rascunhos Pendentes"**

Exibe artigos com status "rascunho":
- Salvos automaticamente (janela fechada)
- Salvos manualmente como rascunho
- Indicação de tempo desde criação

### Finalizar Rascunho

1. Clique em rascunho
2. Revise metadados
3. Clique em **"Finalizar"**
4. Status muda para "finalizado"

### Limpeza Automática

Rascunhos com mais de **7 dias** são automaticamente excluídos.

**Para limpar manualmente**:
- Menu: **"Configurações"** → **"Limpar Rascunhos Antigos"**

---

## Configurações Avançadas

### Trocar Provedor de IA

1. Menu: **"Configurações"** → **"API de IA"**
2. Selecione novo provedor
3. Insira nova chave de API
4. Clique em **"Validar e Salvar"**

**Nota**: Chave anterior será substituída (apenas no navegador).

### Visualizar/Remover Chave de API

1. Menu: **"Configurações"** → **"API de IA"**
2. Opções:
   - **"Mostrar Chave"**: Exibe chave armazenada (mascarada)
   - **"Remover Chave"**: Apaga chave do navegador

**Importante**: Se limpar cache do navegador, chave será perdida.

### Configurar Variáveis de Ambiente

Edite arquivo `.env` ou variáveis do Docker:

```bash
# Aumentar timeout para artigos grandes
TAXONOMY_API_TIMEOUT=10

# Aumentar TTL do cache
CACHE_TTL_DAYS=60

# Mudar nível de log para debug
LOG_LEVEL=debug
```

Reinicie container:

```bash
docker-compose restart
```

---

## Backup e Manutenção

### Backup do Banco de Dados (Mongita)

**Localização**: `./data/etnopapers/` (diretório com arquivos BSON)

⚠️ **IMPORTANTE**: Com Mongita, o banco é um **diretório**, não um único arquivo!

**Backup Manual**:

```bash
# Copiar diretório inteiro do banco
cp -r ./data/etnopapers ./backups/etnopapers_$(date +%Y%m%d)

# Ou via tar (para compressão)
tar -czf ./backups/etnopapers_$(date +%Y%m%d).tar.gz ./data/etnopapers/
```

**Backup Automático** (agendado para UNRAID):

1. Criar script `backup.sh`:

```bash
#!/bin/bash
BACKUP_DIR="/mnt/user/backups/etnopapers"
ETNO_DIR="/mnt/user/appdata/etnopapers"
mkdir -p $BACKUP_DIR

# Fazer backup completo do diretório (com compressão)
tar -czf $BACKUP_DIR/etnopapers_$(date +%Y%m%d_%H%M%S).tar.gz $ETNO_DIR/

# Manter apenas últimos 30 backups
ls -t $BACKUP_DIR/etnopapers_*.tar.gz | tail -n +31 | xargs rm -f

echo "Backup Mongita concluído em $(date)" >> $BACKUP_DIR/backup.log
```

2. Agendar no UNRAID User Scripts plugin:
   - Frequência: Diária
   - Horário: 03:00

### Restaurar Backup (Mongita)

```bash
# Parar container
docker-compose down

# Restaurar backup
rm -rf ./data/etnopapers
tar -xzf ./backups/etnopapers_20251120.tar.gz -C ./

# Ou se usou cópia simples:
# rm -rf ./data/etnopapers
# cp -r ./backups/etnopapers_20251120 ./data/etnopapers

# Reiniciar container
docker-compose up -d

# Verificar que Mongita inicializou corretamente
docker-compose logs | grep "Database initialized"
```

### Exportar Dados para CSV/JSON

**Via Interface Web**:

1. Menu: **"Exportar"** → **"CSV"** ou **"JSON"**
2. Selecione dados:
   - Referências (artigos)
   - Espécies
   - Regiões
   - Comunidades
3. Clique em **"Baixar"**

**Via Python** (acesso ao servidor):

```python
# Script para exportar dados de Mongita para CSV
from mongita import MongitaClientDisk
import csv
import json

# Conectar ao banco
client = MongitaClientDisk(database_dir='./data/etnopapers')
db = client['etnopapers']

# Exportar referências para CSV
with open('referencias_export.csv', 'w', newline='', encoding='utf-8') as f:
    writer = csv.DictWriter(f, fieldnames=['_id', 'titulo', 'doi', 'ano_publicacao', 'autores', 'status'])
    writer.writeheader()
    for doc in db['referencias'].find():
        # Converter ObjectId para string
        doc['_id'] = str(doc['_id'])
        doc['autores'] = '|'.join(doc.get('autores', []))
        writer.writerow({k: doc.get(k) for k in writer.fieldnames})

print("Referências exportadas para referencias_export.csv")

# Exportar para JSON (mais completo, preserva estrutura)
with open('referencias_export.json', 'w', encoding='utf-8') as f:
    docs = []
    for doc in db['referencias'].find():
        doc['_id'] = str(doc['_id'])
        docs.append(doc)
    json.dump(docs, f, ensure_ascii=False, indent=2, default=str)

print("Referências exportadas para referencias_export.json")
```

---

## Troubleshooting

### Problema: Container não inicia

**Sintomas**: Docker exibe erro ao executar `docker-compose up`

**Soluções**:

1. Verificar logs:
   ```bash
   docker-compose logs
   ```

2. Verificar permissões do diretório `./data`:
   ```bash
   chmod 755 ./data
   ```

3. Verificar se porta 8000 está disponível:
   ```bash
   netstat -tuln | grep 8000
   ```

### Problema: Diretório de dados não foi criado (Mongita)

**Sintomas**: Container inicia mas gera erro `permission denied` ou dados não persistem

**Causa**: Diretório `/mnt/user/appdata/etnopapers/` não existe no host UNRAID

**Soluções**:

1. **Criar diretório manualmente no UNRAID** (via SSH):
   ```bash
   mkdir -p /mnt/user/appdata/etnopapers/
   chmod 777 /mnt/user/appdata/etnopapers/
   ```

2. **Ou via UNRAID Web UI**:
   - Vá para: **Settings** → **Shares** → **appdata** → **Create**
   - Ou use **File Manager** para criar pasta `etnopapers` dentro de `appdata`

3. **Verificar que volume está mapeado corretamente**:
   ```bash
   docker inspect etnopapers | grep -A 5 Mounts
   # Deve mostrar: /mnt/user/appdata/etnopapers → /data
   ```

4. **Se container já está rodando**:
   ```bash
   # Parar container
   docker stop etnopapers

   # Criar diretório
   mkdir -p /mnt/user/appdata/etnopapers/

   # Reiniciar
   docker start etnopapers
   ```

**Mongita auto-criará** arquivos de banco no diretório na primeira inicialização.

### Problema: API key inválida

**Sintomas**: Mensagem "Chave de API inválida" ao tentar processar PDF

**Soluções**:

1. Verificar se chave foi copiada corretamente (sem espaços)
2. Verificar se chave não expirou (recriar no provedor)
3. Verificar se tem créditos disponíveis (OpenAI/Claude)
4. Testar chave manualmente:

**Gemini**:
```bash
curl -X POST \
  "https://generativelanguage.googleapis.com/v1/models/gemini-pro:generateContent?key=SUA_CHAVE" \
  -H "Content-Type: application/json" \
  -d '{"contents":[{"parts":[{"text":"test"}]}]}'
```

**OpenAI**:
```bash
curl https://api.openai.com/v1/chat/completions \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer SUA_CHAVE" \
  -d '{"model":"gpt-3.5-turbo","messages":[{"role":"user","content":"test"}],"max_tokens":5}'
```

**Claude**:
```bash
curl https://api.anthropic.com/v1/messages \
  -H "Content-Type: application/json" \
  -H "x-api-key: SUA_CHAVE" \
  -H "anthropic-version: 2023-06-01" \
  -d '{"model":"claude-3-haiku-20240307","max_tokens":5,"messages":[{"role":"user","content":"test"}]}'
```

### Problema: Extração muito lenta (>5 minutos)

**Causas**:
- PDF muito grande (>30 páginas)
- API de IA sobrecarregada
- Conexão lenta com internet

**Soluções**:

1. Dividir PDFs grandes em partes menores
2. Tentar outro provedor de IA
3. Verificar velocidade de internet
4. Aumentar timeout no `.env`:
   ```env
   REQUEST_TIMEOUT=120
   ```

### Problema: JSON malformado (erro de parsing)

**Sintomas**: Mensagem "Erro ao processar resposta da IA"

**Causas**:
- IA retornou texto ao invés de JSON
- PDF de baixa qualidade (escaneado)

**Soluções**:

1. Tentar processar novamente (pode funcionar na 2ª tentativa)
2. Usar outro provedor de IA
3. Clicar em "Editar" e preencher manualmente
4. Melhorar qualidade do PDF (re-escanear)

### Problema: Taxonomia não validada

**Sintomas**: Espécies com status "não validado"

**Causas**:
- API de taxonomia (GBIF/Tropicos) offline
- Nome científico incorreto/incompleto
- Espécie não catalogada nas bases

**Soluções**:

1. Verificar conectividade com internet
2. Tentar validar novamente mais tarde
3. Corrigir nome científico manualmente
4. Aceitar status "não validado" temporariamente

### Problema: Banco de dados corrompido ou não responsivo (Mongita)

**Sintomas**: Erros ao salvar referências, interface não carrega, dados perdidos

**Soluções**:

1. **Parar container**:
   ```bash
   docker-compose down
   ```

2. **Fazer backup do banco corrompido** (por segurança):
   ```bash
   cp -r ./data/etnopapers ./data/etnopapers_corrupted_backup
   ```

3. **Limpar cache e reiniciar** (tenta recuperar dados):
   ```bash
   # Se volumes está sujo, tente apenas reiniciar
   docker-compose up -d
   docker-compose logs -f  # Verificar logs
   ```

4. **Se não funcionar, restaurar backup**:
   ```bash
   rm -rf ./data/etnopapers
   tar -xzf ./backups/etnopapers_20251120.tar.gz -C ./
   docker-compose up -d
   ```

5. **Último recurso: reset completo do banco** (⚠️ PERDE TODOS OS DADOS):
   ```bash
   docker-compose down
   rm -rf ./data/etnopapers/*  # Apaga todos os arquivos BSON
   docker-compose up -d        # Mongita recriará estrutura vazia
   ```

**Prevenção**: Faça backups regularmente (veja seção "Backup Automático")

---

## Perguntas Frequentes (FAQ)

### O PDF original fica armazenado no servidor?

**Não.** Apenas metadados são armazenados no banco Mongita (NoSQL). O PDF é processado no navegador, enviado para API de IA, e descartado após extração. O servidor nunca recebe o PDF original.

### Minha chave de API é enviada para o servidor?

**Não.** A chave fica armazenada APENAS no `localStorage` do seu navegador e é usada diretamente pelo frontend para chamar APIs de IA. O backend nunca recebe ou armazena chaves de API.

### Posso usar o sistema offline?

**Não.** O sistema requer conexão à internet para:
- Chamadas às APIs de IA (Gemini/ChatGPT/Claude)
- Validação taxonômica (GBIF/Tropicos)

Porém, após metadados estarem salvos, você pode consultar o histórico offline.

### Quantos artigos posso processar?

**Ilimitado.** O sistema não impõe limites. Restrições vêm das quotas da API de IA escolhida:
- **Gemini**: 60 requests/minuto (gratuito)
- **ChatGPT**: Conforme créditos disponíveis
- **Claude**: Conforme créditos disponíveis

### Posso mudar de provedor de IA após processar artigos?

**Sim.** Artigos já processados permanecem no banco. Você pode trocar de provedor a qualquer momento para novos processamentos.

### Como exportar todos os dados?

**Via CSV**:
- Interface: Menu → Exportar → CSV

**Via Backup do SQLite**:
- Copiar arquivo `./data/etnopapers.db`
- Acessar com qualquer ferramenta SQLite (DB Browser, DBeaver, etc.)

### O sistema funciona com PDFs escaneados?

**Parcialmente.** O sistema tentará processar, mas:
- Qualidade de extração será **reduzida**
- Sistema exibirá **aviso**
- Recomendado usar **interface de edição manual** para corrigir

Para melhores resultados, use PDFs com texto pesquisável.

### Posso processar múltiplos PDFs simultaneamente?

**Não na versão atual.** Sistema processa um PDF por vez. Upload em lote será considerado para versões futuras.

---

## Próximos Passos

Agora que seu sistema está configurado:

1. ✅ Processar artigos de teste
2. ✅ Experimentar diferentes provedores de IA
3. ✅ Configurar backup automático
4. ✅ Explorar interface de edição
5. ✅ Consultar histórico e filtros

**Precisa de Ajuda?**
- Documentação completa: `specs/main/spec.md`
- Modelo de dados: `specs/main/data-model.md`
- API REST: `specs/main/contracts/api-rest.yaml`

---

## Contato e Suporte

**Repositório**: [GitHub - etnopapers/etnopapers](https://github.com/etnopapers/etnopapers)
**Issues**: https://github.com/etnopapers/etnopapers/issues
**Documentação**: https://docs.etnopapers.org

**Contribuições são bem-vindas!**
