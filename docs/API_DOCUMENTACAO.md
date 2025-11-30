# Documentação da API Etnopapers

**Versão**: 2.0
**URL Base**: `http://localhost:8000`
**Formato da API**: RESTful JSON

---

## Índice

1. [Saúde e Status](#saúde-e-status)
2. [Configuração](#configuração)
3. [Extração de Metadados](#extração-de-metadados)
4. [Gerenciamento de Artigos](#gerenciamento-de-artigos)
5. [Operações do Banco de Dados](#operações-do-banco-de-dados)
6. [Tratamento de Erros](#tratamento-de-erros)

---

## Saúde e Status

### Verificar Saúde da API

**Endpoint**: `GET /api/health`

**Descrição**: Verificar se o servidor FastAPI está rodando e respondendo

**Resposta** (200 OK):
```json
{
  "status": "ok",
  "timestamp": "2024-01-15T10:30:45Z",
  "version": "2.0"
}
```

---

### Verificar Saúde do Ollama

**Endpoint**: `GET /api/health/ollama`

**Descrição**: Verificar status do serviço Ollama e modelos disponíveis

**Resposta** (200 OK):
```json
{
  "status": "ok",
  "url": "http://localhost:11434",
  "models_count": 3,
  "models": ["qwen2.5:7b-instruct", "llama2:7b", "mistral:7b"]
}
```

**Resposta** (503 Serviço Indisponível):
```json
{
  "detail": "Ollama indisponível. Por favor, inicie o Ollama."
}
```

---

### Verificar Saúde do MongoDB

**Endpoint**: `GET /api/health/mongodb`

**Descrição**: Verificar status da conexão com MongoDB

**Resposta** (200 OK):
```json
{
  "status": "ok",
  "database": "etnopapers",
  "collections": ["referencias"],
  "articles_count": 150
}
```

**Resposta** (503 Serviço Indisponível):
```json
{
  "detail": "MongoDB indisponível. Verifique MONGO_URI."
}
```

---

## Configuração

### Validar URI MongoDB

**Endpoint**: `POST /api/config/validate-mongo`

**Descrição**: Validar string de conexão MongoDB

**Corpo da Requisição**:
```json
{
  "mongo_uri": "mongodb://localhost:27017/etnopapers"
}
```

**Resposta** (200 OK):
```json
{
  "valid": true,
  "message": "Conexão com MongoDB validada com sucesso",
  "database": "etnopapers",
  "articles_count": 42
}
```

**Resposta** (400 Requisição Inválida):
```json
{
  "detail": "MongoDB URI inválida. Verifique o formato."
}
```

---

### Salvar Configuração MongoDB

**Endpoint**: `POST /api/config/save-mongo`

**Descrição**: Salvar URI MongoDB no arquivo `.env`

**Corpo da Requisição**:
```json
{
  "mongo_uri": "mongodb://localhost:27017/etnopapers"
}
```

**Resposta** (200 OK):
```json
{
  "message": "Configuração MongoDB salva com sucesso",
  "status": "configured"
}
```

---

## Extração de Metadados

### Extrair Metadados de PDF

**Endpoint**: `POST /api/extract/metadata`

**Descrição**: Enviar PDF e extrair metadados etnobotânicos usando Ollama

**Requisição**: Dados multipart form
- `file`: Arquivo PDF (máx 100 MB)

**Exemplo com cURL**:
```bash
curl -X POST "http://localhost:8000/api/extract/metadata" \
  -F "file=@exemplo.pdf"
```

**Resposta** (200 OK):
```json
{
  "titulo": "Etnobotânica das Plantas Medicinais do Sertão",
  "autores": ["Silva, M.", "Santos, J."],
  "ano": 2020,
  "publicacao": "Journal of Ethnobotany, Vol 45(3)",
  "doi": "10.1234/exemplo.2020",
  "resumo": "Esta pesquisa documenta o uso tradicional de plantas medicinais...",
  "especies": [
    {
      "vernacular": "maçanilha",
      "nomeCientifico": "Chamomilla recutita",
      "familia": "Asteraceae"
    },
    {
      "vernacular": "hortelã",
      "nomeCientifico": "Mentha spicata",
      "familia": "Lamiaceae"
    }
  ],
  "tipo_de_uso": "medicinal",
  "metodologia": "entrevistas e observação participante",
  "pais": "Brasil",
  "estado": "CE",
  "municipio": "Fortaleza",
  "bioma": "Caatinga",
  "comunidade_indigena": "Cearenses",
  "status": "rascunho"
}
```

**Resposta** (400 Requisição Inválida):
```json
{
  "detail": "PDF inválido ou não contém texto extraível"
}
```

**Resposta** (503 Serviço Indisponível):
```json
{
  "detail": "Serviço Ollama indisponível. Tente novamente."
}
```

---

## Gerenciamento de Artigos

### Listar Artigos

**Endpoint**: `GET /api/articles`

**Parâmetros de Consulta**:
- `skip`: Número de artigos a pular (padrão: 0)
- `limit`: Número de artigos a retornar (padrão: 20)
- `year`: Filtrar por ano de publicação
- `country`: Filtrar por código de país
- `search`: Busca full-text em título e autores

**Exemplo**:
```
GET /api/articles?skip=0&limit=20&year=2020&country=Brasil
```

**Resposta** (200 OK):
```json
{
  "articles": [
    {
      "_id": "507f1f77bcf86cd799439011",
      "titulo": "Etnobotânica das Plantas Medicinais",
      "autores": ["Silva, J."],
      "ano": 2020,
      "pais": "Brasil",
      "especies_count": 5,
      "status": "finalizado"
    }
  ],
  "total": 150,
  "skip": 0,
  "limit": 20
}
```

---

### Obter Detalhe do Artigo

**Endpoint**: `GET /api/articles/{article_id}`

**Resposta** (200 OK):
```json
{
  "_id": "507f1f77bcf86cd799439011",
  "titulo": "Etnobotânica das Plantas Medicinais do Sertão",
  "autores": ["Silva, M.", "Santos, J."],
  "ano": 2020,
  "publicacao": "Journal of Ethnobotany",
  "resumo": "Documentação de plantas medicinais tradicionais...",
  "especies": [
    {
      "vernacular": "maçanilha",
      "nomeCientifico": "Chamomilla recutita"
    }
  ],
  "tipo_de_uso": "medicinal",
  "pais": "Brasil",
  "estado": "CE",
  "municipio": "Fortaleza",
  "bioma": "Caatinga",
  "status": "finalizado",
  "created_at": "2024-01-15T10:30:45Z",
  "updated_at": "2024-01-15T10:30:45Z"
}
```

---

### Criar Artigo

**Endpoint**: `POST /api/articles`

**Corpo da Requisição**:
```json
{
  "titulo": "Plantas Medicinais Tradicionais",
  "autores": ["Autor 1", "Autor 2"],
  "ano": 2020,
  "publicacao": "Journal of Ethnobotany",
  "especies": [
    {
      "vernacular": "maçanilha",
      "nomeCientifico": "Chamomilla recutita"
    }
  ],
  "tipo_de_uso": "medicinal",
  "pais": "Brasil",
  "estado": "SC",
  "municipio": "Florianópolis",
  "bioma": "Mata Atlântica",
  "status": "rascunho"
}
```

**Resposta** (201 Criado):
```json
{
  "_id": "507f1f77bcf86cd799439011",
  "message": "Artigo criado com sucesso",
  "titulo": "Plantas Medicinais Tradicionais"
}
```

---

### Atualizar Artigo

**Endpoint**: `PUT /api/articles/{article_id}`

**Corpo da Requisição**: Mesmo do Criar Artigo

**Resposta** (200 OK):
```json
{
  "_id": "507f1f77bcf86cd799439011",
  "message": "Artigo atualizado com sucesso",
  "updated_fields": ["titulo", "status"]
}
```

---

### Deletar Artigo

**Endpoint**: `DELETE /api/articles/{article_id}`

**Resposta** (200 OK):
```json
{
  "message": "Artigo deletado com sucesso",
  "deleted_id": "507f1f77bcf86cd799439011"
}
```

---

## Operações do Banco de Dados

### Obter Estatísticas do Banco de Dados

**Endpoint**: `GET /api/database/stats`

**Descrição**: Obter estatísticas resumidas sobre o banco de dados

**Resposta** (200 OK):
```json
{
  "total_articles": 150,
  "database_name": "etnopapers",
  "total_species": 342,
  "countries_count": 12,
  "total_uses": [
    {
      "tipo_de_uso": "medicinal",
      "count": 95
    },
    {
      "tipo_de_uso": "alimentar",
      "count": 55
    }
  ],
  "last_updated": "2024-01-15T10:30:45Z"
}
```

---

### Baixar Backup do Banco de Dados

**Endpoint**: `GET /api/database/download`

**Descrição**: Baixar backup completo do banco de dados como arquivo ZIP

**Resposta** (200 OK):
- Content-Type: `application/zip`
- Content-Disposition: `attachment; filename="etnopapers_backup_2024-01-15.zip"`

**Conteúdo do ZIP**:
- `referencias.json` - Todos os artigos com metadados
- `backup_metadata.json` - Estatísticas e metadados do backup
- `checksum.json` - Checksum SHA256 para verificação de integridade

---

## Tratamento de Erros

### Resposta de Erro Padrão

Todos os erros seguem este formato:

```json
{
  "detail": "Descrição do erro em português",
  "status_code": 400,
  "timestamp": "2024-01-15T10:30:45Z"
}
```

### Códigos HTTP Comuns

| Status | Significado | Exemplo |
|--------|------------|---------|
| 200 | OK | Requisição bem-sucedida |
| 201 | Criado | Novo artigo criado |
| 400 | Requisição Inválida | Arquivo PDF inválido |
| 404 | Não Encontrado | ID de artigo não existe |
| 503 | Serviço Indisponível | Ollama não rodando |

### Exemplos de Respostas de Erro

**400 - Entrada Inválida**:
```json
{
  "detail": "PDF inválido ou não contém texto extraível"
}
```

**404 - Não Encontrado**:
```json
{
  "detail": "Artigo não encontrado"
}
```

**503 - Serviço Indisponível**:
```json
{
  "detail": "Ollama indisponível. Por favor, inicie o Ollama a partir de https://ollama.com/download"
}
```

---

## Autenticação

**Versão Atual**: Sem autenticação (projetado para redes locais)

**Melhoria Futura**: Tokens JWT via HttpOnly cookies

---

## Limitação de Taxa

**Versão Atual**: Sem limitação de taxa

**Recomendação**: Implementar antes da implantação em produção

---

## Configuração CORS

Requisições cross-origin do frontend são permitidas de:
- `http://localhost:3000` (desenvolvimento)
- `http://127.0.0.1:8000` (produção)

---

## Exemplos

### Fluxo Completo: Upload e Salvar Artigo

```bash
# 1. Enviar PDF para extração
curl -X POST "http://localhost:8000/api/extract/metadata" \
  -F "file=@paper.pdf" \
  > extraido.json

# 2. Salvar metadados extraídos como artigo
curl -X POST "http://localhost:8000/api/articles" \
  -H "Content-Type: application/json" \
  -d @extraido.json

# 3. Obter todos os artigos do Brasil
curl "http://localhost:8000/api/articles?country=Brasil&limit=50"

# 4. Baixar backup
curl -X GET "http://localhost:8000/api/database/download" \
  > etnopapers_backup.zip
```

---

## Teste Interativo da API

Acesse a Swagger UI em: `http://localhost:8000/docs`

Ou acesse o ReDoc em: `http://localhost:8000/redoc`

---

## Suporte e Solução de Problemas

**Ollama não respondendo**:
```bash
# Verificar serviço Ollama
curl http://localhost:11434/api/tags

# Reiniciar Ollama:
# Baixe de https://ollama.com/download
```

**Erro de conexão MongoDB**:
```bash
# Verificar se MongoDB está rodando:
mongosh
# Deve conectar sem erros
```

**Extração de PDF falhando**:
- Garanta que o PDF é texto-extraível (não imagem scaneada)
- Arquivo deve ser <100 MB
- Verifique que Ollama tem memória suficiente (4GB+ recomendado)

---

**Última Atualização**: 2024-01-15
**Versão da API**: 2.0
**Versão Python**: 3.11+
**Versão FastAPI**: 0.100+
