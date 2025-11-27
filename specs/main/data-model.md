# Phase 1: Data Model & Entity Definitions

**Date**: 2025-11-27
**Status**: Complete
**Purpose**: Define all entities, relationships, validation rules, and state transitions needed for standalone desktop application.

---

## Overview

Etnopapers maintains a **document-centric MongoDB schema** with a single core collection (`referencias`) storing complete scientific articles with all metadata embedded. This model prioritizes simplicity, zero-migration schema evolution, and query efficiency over normalization.

**Key Principles**:
- ✅ One query returns complete article with all related data
- ✅ No JOINs needed
- ✅ New fields added without schema migrations
- ✅ Flexible document structure (MongoDB BSON native)

---

## Collections

### 1. `referencias` (Scientific Articles & References)

**Purpose**: Core collection storing all extracted and manually created ethnobotanical research articles.

**Document Structure**:

```json
{
  "_id": ObjectId("507f1f77bcf86cd799439011"),
  "ano": 2010,
  "titulo": "Uso e conhecimento tradicional de plantas medicinais no Sertão",
  "publicacao": "Acta bot. bras. 24(2): 395-406",
  "autores": [
    "Giraldi, M.",
    "Hanazaki, N."
  ],
  "resumo": "O objetivo desta pesquisa foi realizar um estudo etnobotânico das plantas medicinais usadas pela população do Sertão do Ribeirão...",
  "doi": "10.1590/S0102-33062010000200007",
  "url": "https://example.com/article",

  "especies": [
    {
      "vernacular": "maçanilha",
      "nomeCientifico": "Chamomilla recutita",
      "familia": "Asteraceae",
      "status": "validado",
      "gbifId": 3116360
    },
    {
      "vernacular": "hortelã-branca",
      "nomeCientifico": "Mentha sp.",
      "familia": null,
      "status": "não_validado",
      "gbifId": null
    }
  ],

  "tipo_de_uso": "medicinal",
  "usos_especificos": [
    "tratamento de tosse",
    "alívio de dor abdominal",
    "preparação de chá"
  ],

  "metodologia": "entrevistas",
  "descricao_metodologia": "Entrevistas semiestruturadas com 25 habitantes da região",

  "localizacao": {
    "pais": "Brasil",
    "estado": "SC",
    "municipio": "Florianópolis",
    "local": "Sertão do Ribeirão",
    "territorio_indigena": null,
    "coordenadas": {
      "latitude": -27.5951,
      "longitude": -48.5477,
      "precisao": "municipio"
    }
  },

  "bioma": "Mata Atlântica",
  "ecossistema": "floresta tropical subcaducifólia",

  "comunidade": {
    "nome": "Comunidade Ribeirão",
    "tipo": "rural",
    "populacao_estimada": null
  },

  "status": "rascunho",
  "data_criacao": "2025-11-27T12:00:00Z",
  "data_atualizacao": "2025-11-27T14:30:00Z",

  "palavras_chave": [
    "etnobotânica",
    "plantas medicinais",
    "conhecimento tradicional"
  ],

  "notas_internas": "Espécie Mentha precisa validação. Autor confirmado via email."
}
```

**Field Definitions**:

| Field | Type | Required | Description | Validation |
|-------|------|----------|-------------|-----------|
| `_id` | ObjectId | Yes | MongoDB unique identifier | Auto-generated |
| `ano` | Integer | Yes | Publication year | 1900–current year |
| `titulo` | String | Yes | Article title | Max 500 chars, non-empty |
| `publicacao` | String | No | Publication venue (journal, conference) | Max 300 chars |
| `autores` | Array[String] | Yes | Author names | Min 1, max 20 authors |
| `resumo` | String | No | Abstract/summary | Max 5000 chars |
| `doi` | String | No | Digital Object Identifier | Format: `10.XXXX/...`, unique index |
| `url` | String | No | URL to article | Valid HTTPS URL |
| `especies` | Array[Object] | Yes | Species mentioned in article | Min 1 species |
| `especies[].vernacular` | String | Yes | Common/vernacular name | Max 100 chars |
| `especies[].nomeCientifico` | String | Yes | Scientific binomial name | Max 100 chars, format: `Genus species` |
| `especies[].familia` | String | No | Plant family | Max 50 chars |
| `especies[].status` | String | Yes | Validation status | Enum: `validado`, `não_validado`, `parcialmente_validado` |
| `especies[].gbifId` | Integer | No | GBIF species identifier | Reference to external GBIF database |
| `tipo_de_uso` | String | Yes | Primary use category | Enum: `medicinal`, `alimentar`, `ritual`, `construção`, `artesanato`, `outro` |
| `usos_especificos` | Array[String] | No | Specific uses | Max 10 items, max 100 chars each |
| `metodologia` | String | Yes | Research methodology | Enum: `entrevistas`, `observação`, `revisão_bibliográfica`, `experimento`, `misto` |
| `descricao_metodologia` | String | No | Detailed methodology description | Max 1000 chars |
| `localizacao.pais` | String | Yes | Country (Portuguese) | Max 100 chars |
| `localizacao.estado` | String | No | State/province (2-letter code) | Max 2 chars, uppercase |
| `localizacao.municipio` | String | No | Municipality | Max 100 chars |
| `localizacao.local` | String | No | Specific location name | Max 150 chars |
| `localizacao.territorio_indigena` | String | No | Indigenous land name if applicable | Max 150 chars |
| `localizacao.coordenadas.latitude` | Float | No | Latitude | Range: -90 to 90 |
| `localizacao.coordenadas.longitude` | Float | No | Longitude | Range: -180 to 180 |
| `localizacao.coordenadas.precisao` | String | No | Coordinate precision | Enum: `ponto`, `município`, `estado`, `país` |
| `bioma` | String | No | Biome name | Max 100 chars (e.g., "Mata Atlântica", "Cerrado", "Amazônia") |
| `ecossistema` | String | No | Specific ecosystem type | Max 100 chars |
| `comunidade.nome` | String | No | Community name | Max 150 chars |
| `comunidade.tipo` | String | No | Community type | Enum: `urbana`, `rural`, `indígena`, `quilombola`, `outros` |
| `comunidade.populacao_estimada` | Integer | No | Estimated population | Min 0 |
| `status` | String | Yes | Document status | Enum: `rascunho`, `finalizado` |
| `data_criacao` | ISODate | Yes | Creation timestamp | Auto-set on insert |
| `data_atualizacao` | ISODate | Yes | Last update timestamp | Auto-updated on modify |
| `palavras_chave` | Array[String] | No | Keywords for search | Max 20 items |
| `notas_internas` | String | No | Internal notes (not displayed to users) | Max 1000 chars |

**Indexes**:

```javascript
// Create indexes for performance
db.referencias.createIndex({ "doi": 1 }, { unique: true, sparse: true })
db.referencias.createIndex({ "titulo": "text", "resumo": "text", "especies.nomeCientifico": "text" })
db.referencias.createIndex({ "ano": 1 })
db.referencias.createIndex({ "status": 1 })
db.referencias.createIndex({ "tipo_de_uso": 1 })
db.referencias.createIndex({ "localizacao.pais": 1, "localizacao.estado": 1, "localizacao.municipio": 1 })
db.referencias.createIndex({ "data_atualizacao": -1 })
```

**State Transitions**:

```
┌─────────────┐
│ rascunho    │  (New article, being edited, not finalized)
└──────┬──────┘
       │ (User clicks "Save")
       ▼
┌─────────────┐
│ finalizado  │  (Complete, validated metadata ready for publication)
└─────────────┘
       │ (User clicks "Edit" - back to draft)
       ▼
    rascunho
```

---

## Pydantic Schemas (Frontend/Backend Communication)

### `ReferenceData` (Input: PDF Extraction)

```python
class SpeciesData(BaseModel):
    vernacular: str
    nomeCientifico: str
    familia: Optional[str] = None
    status: Literal["validado", "não_validado", "parcialmente_validado"] = "não_validado"
    gbifId: Optional[int] = None

class LocalizacaoData(BaseModel):
    pais: str
    estado: Optional[str] = None
    municipio: Optional[str] = None
    local: Optional[str] = None
    territorio_indigena: Optional[str] = None
    latitude: Optional[float] = None
    longitude: Optional[float] = None

class ComunidadeData(BaseModel):
    nome: Optional[str] = None
    tipo: Optional[Literal["urbana", "rural", "indígena", "quilombola"]] = None
    populacao_estimada: Optional[int] = None

class ReferenceData(BaseModel):
    ano: int
    titulo: str
    publicacao: Optional[str] = None
    autores: List[str]
    resumo: Optional[str] = None
    doi: Optional[str] = None
    url: Optional[str] = None
    especies: List[SpeciesData]
    tipo_de_uso: Literal["medicinal", "alimentar", "ritual", "construção", "artesanato", "outro"]
    usos_especificos: Optional[List[str]] = None
    metodologia: Literal["entrevistas", "observação", "revisão_bibliográfica", "experimento", "misto"]
    descricao_metodologia: Optional[str] = None
    localizacao: LocalizacaoData
    bioma: Optional[str] = None
    ecossistema: Optional[str] = None
    comunidade: Optional[ComunidadeData] = None
    status: Literal["rascunho", "finalizado"] = "rascunho"
    palavras_chave: Optional[List[str]] = None
    notas_internas: Optional[str] = None

    @validator('ano')
    def validate_year(cls, v):
        if not (1900 <= v <= datetime.now().year + 1):
            raise ValueError('Invalid year')
        return v

    @validator('doi')
    def validate_doi(cls, v):
        if v and not re.match(r'^10\.\d{4,}/', v):
            raise ValueError('Invalid DOI format')
        return v
```

### Configuration File Schema

**Location**: User home directory
- Windows: `%APPDATA%\Local\Etnopapers\config.json`
- macOS: `~/Library/Application Support/Etnopapers/config.json`
- Linux: `~/.config/etnopapers/config.json`

**Structure**:

```json
{
  "version": "3.0.0",
  "mongoUri": "mongodb://localhost:27017/etnopapers",
  "ollamaUrl": "http://localhost:11434",
  "appVersion": "3.0.0",
  "lastUpdated": "2025-11-27T12:00:00Z"
}
```

---

## API Request/Response Models

### POST `/api/articles` (Create Article)

**Request**:
```json
{
  "ano": 2010,
  "titulo": "Uso e conhecimento tradicional...",
  "autores": ["Giraldi, M."],
  "especies": [{"vernacular": "maçanilha", "nomeCientifico": "Chamomilla recutita"}],
  "tipo_de_uso": "medicinal",
  "metodologia": "entrevistas",
  "localizacao": {"pais": "Brasil"}
}
```

**Response** (201 Created):
```json
{
  "_id": "507f1f77bcf86cd799439011",
  "ano": 2010,
  "titulo": "Uso e conhecimento tradicional...",
  "status": "rascunho",
  "data_criacao": "2025-11-27T12:00:00Z"
}
```

### GET `/api/articles?limit=20&offset=0&search=medicinal&sort=ano` (List Articles)

**Response**:
```json
{
  "items": [
    {
      "_id": "507f1f77bcf86cd799439011",
      "ano": 2010,
      "titulo": "...",
      "status": "rascunho",
      "especies_count": 5
    }
  ],
  "total": 125,
  "limit": 20,
  "offset": 0
}
```

### POST `/api/extract/metadata` (Upload PDF & Extract)

**Request**:
```
Content-Type: multipart/form-data

file: <binary PDF data>
```

**Response** (200 OK):
```json
{
  "status": "success",
  "data": {
    "titulo": "Extracted from PDF...",
    "autores": ["Author Name"],
    "especies": [...],
    ...
  },
  "extracted_at": "2025-11-27T12:00:00Z"
}
```

**Response** (Ollama unavailable):
```json
{
  "status": "error",
  "error": "Serviço de AI local indisponível. Verifique Ollama ou reinicie o container.",
  "code": "OLLAMA_UNAVAILABLE"
}
```

---

## Data Validation Rules

### Business Logic Validations

1. **Required Species**: Every article must have at least 1 species.
2. **Unique DOI**: If DOI provided, must be unique across database. Non-unique DOI treated as potential duplicate (warning to user).
3. **Valid Year**: Publication year must be between 1900 and current year (+1 for future publications).
4. **Binomial Name Format**: Scientific name should match pattern `Genus species` (capitalized genus, lowercase species).
5. **Coordinates**: If latitude/longitude provided, must be valid ranges (-90 to 90 for lat, -180 to 180 for long).
6. **PDF Size Limit**: File upload limited to 100 MB (configurable via environment variable).
7. **Timezone**: All timestamps stored as UTC (ISODate). Frontend converts to local time for display.

### Data Integrity Checks

Before database backup download:
```python
# Validate all indexes exist
db.referencias.validate()

# Check for orphaned documents
db.referencias.find({ "especies": { $size: 0 } })  # Should be empty

# Verify DOI uniqueness constraint
db.referencias.aggregate([
  { $group: { _id: "$doi", count: { $sum: 1 } } },
  { $match: { count: { $gt: 1 } } }
])  # Should return empty if valid
```

---

## Migration Path (Schema Evolution)

**Zero-migration philosophy**: MongoDB document structure is schema-less. To add new fields:

1. Update Pydantic schema in `backend/models/article.py` (add new field with optional)
2. Update extraction prompt in `backend/services/extraction_service.py` to populate new field
3. New articles created with new field; old articles auto-populate field as `null` (no migration needed)
4. Frontend conditionally displays new field if present (graceful degradation for old docs)
5. Optional: Run script to backfill existing documents with new field

**Example**: Adding `volume_issue` field to articles
```python
# Step 1: Update schema
class ReferenceData(BaseModel):
    volume_issue: Optional[str] = None  # e.g., "24(2)"

# Step 2: Update extraction prompt
# Step 3: Done! Old articles have volume_issue=null, new ones populated
# Step 4: Backfill script (optional)
db.referencias.updateMany(
  { volume_issue: null },
  [ { $set: { volume_issue: "" } } ]
)
```

---

## Database Backup & Restore

### Backup Creation

```python
# backend/services/backup_service.py
def create_backup() -> bytes:
    # Run mongodump
    result = subprocess.run([
        'mongodump',
        f'--uri={MONGO_URI}',
        '--out=./backup_temp'
    ])

    # Create ZIP archive
    shutil.make_archive('backup', 'zip', 'backup_temp')

    # Return ZIP as bytes
    with open('backup.zip', 'rb') as f:
        return f.read()
```

### Restore (User Manual)

```bash
# User extracts backup.zip
unzip backup.zip

# Restore to MongoDB
mongorestore --uri="mongodb://localhost:27017/etnopapers" ./dump
```

---

## Data Size Estimation

| Scenario | Articles | Average Article Size | Total DB Size |
|----------|----------|----------------------|----------------|
| Small research | 100 | 5 KB | 0.5 MB |
| Medium project | 1,000 | 5 KB | 5 MB |
| Large project | 10,000 | 5 KB | 50 MB |
| Very large | 100,000 | 5 KB | 500 MB |

*Estimate: ~5 KB per article (title, authors, species, locations, metadata)*

**Backup size** (mongodump compression): ~30–40% of database size
- 1,000 articles: ~1.5–2 MB backup
- 10,000 articles: ~15–20 MB backup

---

## Phase 1 Conclusion

Data model is **complete, validated, and ready for API contract generation**.

✅ All entities defined with field-level validation
✅ State transitions documented
✅ Pydantic schemas provide type safety
✅ Indexes specified for performance
✅ Backup/restore process defined
✅ Zero-migration schema evolution strategy
✅ Data size estimates provided
