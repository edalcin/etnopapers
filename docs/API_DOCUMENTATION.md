# Etnopapers API Documentation

**Version**: 2.0
**Base URL**: `http://localhost:8000`
**API Format**: RESTful JSON

---

## Table of Contents

1. [Health & Status](#health--status)
2. [Configuration](#configuration)
3. [Metadata Extraction](#metadata-extraction)
4. [Articles Management](#articles-management)
5. [Database Operations](#database-operations)
6. [Error Handling](#error-handling)

---

## Health & Status

### Get API Health

**Endpoint**: `GET /api/health`

**Description**: Check if the FastAPI server is running and responsive

**Response** (200 OK):
```json
{
  "status": "ok",
  "timestamp": "2024-01-15T10:30:45Z",
  "version": "2.0"
}
```

---

### Get Ollama Health

**Endpoint**: `GET /api/health/ollama`

**Description**: Check Ollama service status and available models

**Response** (200 OK):
```json
{
  "status": "ok",
  "url": "http://localhost:11434",
  "models_count": 3,
  "models": ["qwen2.5:7b-instruct", "llama2:7b", "mistral:7b"]
}
```

**Response** (503 Service Unavailable):
```json
{
  "detail": "Ollama indisponível. Por favor, inicie o Ollama."
}
```

---

### Get MongoDB Health

**Endpoint**: `GET /api/health/mongodb`

**Description**: Check MongoDB connection status

**Response** (200 OK):
```json
{
  "status": "ok",
  "database": "etnopapers",
  "collections": ["referencias"],
  "articles_count": 150
}
```

**Response** (503 Service Unavailable):
```json
{
  "detail": "MongoDB indisponível. Verifique MONGO_URI."
}
```

---

## Configuration

### Validate MongoDB URI

**Endpoint**: `POST /api/config/validate-mongo`

**Description**: Validate MongoDB connection string

**Request Body**:
```json
{
  "mongo_uri": "mongodb://localhost:27017/etnopapers"
}
```

**Response** (200 OK):
```json
{
  "valid": true,
  "message": "Conexão com MongoDB validada com sucesso",
  "database": "etnopapers",
  "articles_count": 42
}
```

**Response** (400 Bad Request):
```json
{
  "detail": "MongoDB URI inválida. Verifique o formato."
}
```

---

### Save MongoDB Configuration

**Endpoint**: `POST /api/config/save-mongo`

**Description**: Save MongoDB URI to `.env` file

**Request Body**:
```json
{
  "mongo_uri": "mongodb://localhost:27017/etnopapers"
}
```

**Response** (200 OK):
```json
{
  "message": "Configuração MongoDB salva com sucesso",
  "status": "configured"
}
```

---

## Metadata Extraction

### Extract Metadata from PDF

**Endpoint**: `POST /api/extract/metadata`

**Description**: Upload PDF and extract ethnobotanical metadata using Ollama

**Request**: Multipart form data
- `file`: PDF file (max 100 MB)

**cURL Example**:
```bash
curl -X POST "http://localhost:8000/api/extract/metadata" \
  -F "file=@example.pdf"
```

**Response** (200 OK):
```json
{
  "titulo": "Etnobotânica das Plantas Medicinais do Sertão",
  "autores": ["Silva, M.", "Santos, J."],
  "ano": 2020,
  "publicacao": "Journal of Ethnobotany, Vol 45(3)",
  "doi": "10.1234/example.2020",
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

**Response** (400 Bad Request):
```json
{
  "detail": "PDF inválido ou não contém texto extraível"
}
```

**Response** (503 Service Unavailable):
```json
{
  "detail": "Serviço Ollama indisponível. Tente novamente."
}
```

---

## Articles Management

### List Articles

**Endpoint**: `GET /api/articles`

**Query Parameters**:
- `skip`: Number of articles to skip (default: 0)
- `limit`: Number of articles to return (default: 20)
- `year`: Filter by publication year
- `country`: Filter by country code
- `search`: Full-text search in title and authors

**Example**:
```
GET /api/articles?skip=0&limit=20&year=2020&country=Brasil
```

**Response** (200 OK):
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

### Get Article Detail

**Endpoint**: `GET /api/articles/{article_id}`

**Response** (200 OK):
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

### Create Article

**Endpoint**: `POST /api/articles`

**Request Body**:
```json
{
  "titulo": "Plantas Medicinais Tradicionais",
  "autores": ["Author 1", "Author 2"],
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

**Response** (201 Created):
```json
{
  "_id": "507f1f77bcf86cd799439011",
  "message": "Artigo criado com sucesso",
  "titulo": "Plantas Medicinais Tradicionais"
}
```

---

### Update Article

**Endpoint**: `PUT /api/articles/{article_id}`

**Request Body**: Same as Create Article

**Response** (200 OK):
```json
{
  "_id": "507f1f77bcf86cd799439011",
  "message": "Artigo atualizado com sucesso",
  "updated_fields": ["titulo", "status"]
}
```

---

### Delete Article

**Endpoint**: `DELETE /api/articles/{article_id}`

**Response** (200 OK):
```json
{
  "message": "Artigo deletado com sucesso",
  "deleted_id": "507f1f77bcf86cd799439011"
}
```

---

## Database Operations

### Get Database Statistics

**Endpoint**: `GET /api/database/stats`

**Description**: Get summary statistics about the database

**Response** (200 OK):
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

### Download Database Backup

**Endpoint**: `GET /api/database/download`

**Description**: Download complete database backup as ZIP file

**Response** (200 OK):
- Content-Type: `application/zip`
- Content-Disposition: `attachment; filename="etnopapers_backup_2024-01-15.zip"`

**ZIP Contents**:
- `referencias.json` - All articles with metadata
- `backup_metadata.json` - Backup statistics and metadata
- `checksum.json` - SHA256 checksum for integrity verification

---

## Error Handling

### Standard Error Response

All errors follow this format:

```json
{
  "detail": "Descrição do erro em português",
  "status_code": 400,
  "timestamp": "2024-01-15T10:30:45Z"
}
```

### Common HTTP Status Codes

| Status | Meaning | Example |
|--------|---------|---------|
| 200 | OK | Successful request |
| 201 | Created | New article created |
| 400 | Bad Request | Invalid PDF file |
| 404 | Not Found | Article ID doesn't exist |
| 503 | Service Unavailable | Ollama not running |

### Example Error Responses

**400 - Invalid Input**:
```json
{
  "detail": "PDF inválido ou não contém texto extraível"
}
```

**404 - Not Found**:
```json
{
  "detail": "Artigo não encontrado"
}
```

**503 - Service Unavailable**:
```json
{
  "detail": "Ollama indisponível. Por favor, inicie o Ollama a partir de https://ollama.com/download"
}
```

---

## Authentication

**Current Version**: No authentication required (designed for local networks)

**Future Enhancement**: JWT tokens via HttpOnly cookies

---

## Rate Limiting

**Current Version**: No rate limiting

**Recommendation**: Implement before production deployment

---

## CORS Configuration

Frontend cross-origin requests are allowed from:
- `http://localhost:3000` (dev)
- `http://127.0.0.1:8000` (production)

---

## Examples

### Complete Workflow: Upload and Save Article

```bash
# 1. Upload PDF for extraction
curl -X POST "http://localhost:8000/api/extract/metadata" \
  -F "file=@paper.pdf" \
  > extracted.json

# 2. Save extracted metadata as article
curl -X POST "http://localhost:8000/api/articles" \
  -H "Content-Type: application/json" \
  -d @extracted.json

# 3. Get all articles from Brazil
curl "http://localhost:8000/api/articles?country=Brasil&limit=50"

# 4. Download backup
curl -X GET "http://localhost:8000/api/database/download" \
  > etnopapers_backup.zip
```

---

## Interactive API Testing

Access the Swagger UI at: `http://localhost:8000/docs`

Or access the ReDoc at: `http://localhost:8000/redoc`

---

## Support & Troubleshooting

**Ollama not responding**:
```bash
# Check Ollama service
curl http://localhost:11434/api/tags

# Restart Ollama:
# Download from https://ollama.com/download
```

**MongoDB connection error**:
```bash
# Check MongoDB is running:
mongosh
# Should connect without errors
```

**PDF extraction failing**:
- Ensure PDF is text-extractable (not scanned image)
- File should be <100 MB
- Check Ollama has sufficient memory (4GB+ recommended)

---

**Last Updated**: 2024-01-15
**API Version**: 2.0
**Python Version**: 3.11+
**FastAPI Version**: 0.100+
