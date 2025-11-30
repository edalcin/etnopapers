# Etnopapers Developer Guide

**Version**: 2.0
**Language**: English
**Updated**: 2024-01-15

---

## Table of Contents

1. [Development Setup](#development-setup)
2. [Project Architecture](#project-architecture)
3. [Frontend Development](#frontend-development)
4. [Backend Development](#backend-development)
5. [Database Design](#database-design)
6. [Testing](#testing)
7. [Building & Deployment](#building--deployment)
8. [Contributing](#contributing)

---

## Development Setup

### Prerequisites

- **Node.js**: 18.x or higher
- **Python**: 3.11 or higher
- **Git**: Latest version
- **MongoDB**: Local instance or Atlas account
- **Ollama**: https://ollama.com/download

### Initial Setup

```bash
# Clone repository
git clone https://github.com/edalcin/etnopapers.git
cd etnopapers

# Frontend setup
cd frontend
npm install
npm run dev  # Starts on http://localhost:3000

# Backend setup (in another terminal)
cd backend
python -m venv venv

# Activate virtual environment
# Windows:
venv\Scripts\activate
# macOS/Linux:
source venv/bin/activate

# Install dependencies
pip install -r requirements.txt

# Set environment variables
export MONGO_URI="mongodb://localhost:27017/etnopapers"
export OLLAMA_URL="http://localhost:11434"

# Start dev server
uvicorn main:app --reload

# API docs available at: http://localhost:8000/docs
```

---

## Project Architecture

### Directory Structure

```
etnopapers/
├── backend/
│   ├── src/
│   │   ├── models/          # Pydantic schemas
│   │   │   ├── reference.py
│   │   │   └── species.py
│   │   ├── routers/         # FastAPI route handlers
│   │   │   ├── health.py
│   │   │   ├── articles.py
│   │   │   ├── extraction.py
│   │   │   ├── database.py
│   │   │   └── configuration.py
│   │   ├── services/        # Business logic
│   │   │   ├── database_service.py
│   │   │   ├── pdf_service.py
│   │   │   ├── extraction_service.py
│   │   │   ├── search_service.py
│   │   │   ├── backup_service.py
│   │   │   ├── config_service.py
│   │   │   ├── article_service.py
│   │   │   └── ollama_service.py
│   │   ├── database/        # Database connection
│   │   │   └── connection.py
│   │   ├── middleware/      # Middleware (error handling)
│   │   │   └── error_handler.py
│   │   └── config/          # Configuration
│   │       └── logging.py
│   ├── tests/               # Unit & integration tests
│   │   └── test_services.py
│   ├── main.py              # FastAPI app entry point
│   ├── launcher.py          # Desktop app launcher
│   ├── build.spec           # PyInstaller configuration
│   └── requirements.txt      # Python dependencies
│
├── frontend/
│   ├── src/
│   │   ├── components/      # React components
│   │   │   ├── PDFUpload.tsx
│   │   │   ├── MetadataDisplay.tsx
│   │   │   ├── ArticlesTable.tsx
│   │   │   ├── MainLayout.tsx
│   │   │   └── ...
│   │   ├── pages/           # Page components
│   │   │   ├── Home.tsx
│   │   │   └── Settings.tsx
│   │   ├── services/        # API clients
│   │   │   ├── api.ts
│   │   │   ├── healthService.ts
│   │   │   └── configService.ts
│   │   ├── hooks/           # Custom React hooks
│   │   │   └── useOllamaHealth.ts
│   │   ├── store/           # Zustand stores
│   │   │   ├── appStore.ts
│   │   │   ├── configStore.ts
│   │   │   ├── extractionStore.ts
│   │   │   └── articleStore.ts
│   │   ├── __tests__/       # Component tests
│   │   │   └── components.test.tsx
│   │   ├── App.tsx
│   │   └── main.tsx
│   ├── public/
│   │   └── index.html
│   ├── package.json
│   ├── tsconfig.json
│   └── vite.config.ts
│
├── docs/
│   ├── API_DOCUMENTATION.md
│   ├── GUIA_USUARIO.md (Portuguese)
│   └── DEVELOPER_GUIDE.md
│
├── .github/workflows/       # GitHub Actions
│   └── build-release.yml
│
├── build-windows.bat
├── build-macos.sh
├── build-linux.sh
└── CLAUDE.md               # Project instructions
```

### Technology Stack

**Frontend**:
- React 18 with TypeScript
- Vite for build/dev
- Zustand for state management
- TanStack React Table v8
- react-hook-form
- CSS3 with animations

**Backend**:
- FastAPI (Python web framework)
- PyMongo (MongoDB driver)
- pdfplumber (PDF text extraction)
- instructor (Structured LLM outputs)
- Ollama (Local LLM inference)
- Uvicorn (ASGI server)

**Database**:
- MongoDB (NoSQL document store)
- Single collection model (all in `referencias`)
- Indexed fields: DOI (unique), status, year, text search

**Distribution**:
- PyInstaller (executable bundling)
- GitHub Actions (CI/CD)
- Cross-platform: Windows, macOS, Linux

---

## Frontend Development

### Component Structure

Each component should follow this structure:

```typescript
import { useState } from 'react'
import './ComponentName.css'

interface ComponentNameProps {
  onAction?: () => void
}

export default function ComponentName({ onAction }: ComponentNameProps) {
  const [state, setState] = useState(false)

  const handleAction = () => {
    // Action logic
    onAction?.()
  }

  return (
    <div className="component-name">
      {/* JSX */}
    </div>
  )
}
```

### CSS Conventions

- Use BEM naming: `.component-name__element--modifier`
- Mobile-first responsive design
- Variables for colors (defined in index.css)
- Animations defined at component level

**Example CSS**:
```css
.component-name {
  padding: 2rem;
  background: white;
  border-radius: 8px;
}

.component-name__header {
  margin-bottom: 1rem;
}

.component-name__header--active {
  color: #667eea;
}

@media (max-width: 768px) {
  .component-name {
    padding: 1rem;
  }
}
```

### State Management with Zustand

Creating a new store:

```typescript
// src/store/exampleStore.ts
import { create } from 'zustand'

interface ExampleState {
  data: any
  setData: (data: any) => void
  clear: () => void
}

export const useExampleStore = create<ExampleState>((set) => ({
  data: null,
  setData: (data) => set({ data }),
  clear: () => set({ data: null })
}))
```

Using in component:
```typescript
import { useExampleStore } from '../store/exampleStore'

export default function Component() {
  const { data, setData } = useExampleStore()

  return (
    <button onClick={() => setData(newValue)}>
      {data?.name || 'No data'}
    </button>
  )
}
```

### Custom Hooks

Create reusable logic with custom hooks:

```typescript
// src/hooks/useExample.ts
import { useState, useEffect } from 'react'

export function useExample(initialValue: string) {
  const [value, setValue] = useState(initialValue)

  useEffect(() => {
    // Side effects
  }, [value])

  return { value, setValue }
}
```

### API Communication

Use the api.ts service:

```typescript
// src/services/api.ts
export async function fetchArticles(page: number) {
  const response = await fetch(`/api/articles?skip=${page * 20}&limit=20`)
  if (!response.ok) throw new Error('Failed to fetch')
  return response.json()
}
```

---

## Backend Development

### Creating a New Endpoint

1. **Define Pydantic Model** (`src/models/`)

```python
from pydantic import BaseModel
from typing import Optional

class MyDataModel(BaseModel):
    name: str
    description: Optional[str] = None
```

2. **Create Service** (`src/services/my_service.py`)

```python
from typing import Dict, Any

class MyService:
    @staticmethod
    def process_data(data: str) -> Dict[str, Any]:
        # Business logic
        return {"result": processed_data}
```

3. **Create Router** (`src/routers/my_router.py`)

```python
from fastapi import APIRouter, HTTPException
from src.models.my_model import MyDataModel
from src.services.my_service import MyService

router = APIRouter(prefix="/api/my", tags=["my-endpoint"])

@router.post("/process")
async def process(data: MyDataModel):
    try:
        result = MyService.process_data(data.name)
        return {"status": "success", "data": result}
    except Exception as e:
        raise HTTPException(status_code=400, detail=str(e))
```

4. **Register Router** in `main.py`

```python
from src.routers import my_router

app.include_router(my_router.router)
```

### Database Operations

Using PyMongo with MongoDB:

```python
from src.database.connection import DatabaseConnection

# Get database instance
db = DatabaseConnection.get_db()

# Insert document
result = db['referencias'].insert_one({
    'titulo': 'Example',
    'ano': 2020
})

# Find documents
articles = db['referencias'].find({'ano': 2020})

# Update document
db['referencias'].update_one(
    {'_id': ObjectId('...')},
    {'$set': {'status': 'finalizado'}}
)

# Delete document
db['referencias'].delete_one({'_id': ObjectId('...')})
```

### Error Handling

Use FastAPI exceptions with proper status codes:

```python
from fastapi import HTTPException

# 400 Bad Request
raise HTTPException(
    status_code=400,
    detail="Descrição do erro em português"
)

# 404 Not Found
raise HTTPException(
    status_code=404,
    detail="Recurso não encontrado"
)

# 503 Service Unavailable
raise HTTPException(
    status_code=503,
    detail="Serviço indisponível"
)
```

### Logging

Use the configured logging system:

```python
import logging

logger = logging.getLogger(__name__)

logger.info("Operation successful")
logger.warning("Something unusual happened")
logger.error("An error occurred", exc_info=True)
```

---

## Database Design

### Single Collection Model

All data stored in `referencias` collection:

```javascript
{
  "_id": ObjectId,

  // Article metadata
  "titulo": String,
  "autores": [String],
  "ano": Number,
  "publicacao": String,
  "doi": String (unique index),
  "resumo": String,

  // Species information (array)
  "especies": [
    {
      "vernacular": String,
      "nomeCientifico": String,
      "familia": String
    }
  ],

  // Ethnobotanical data
  "tipo_de_uso": String,
  "metodologia": String,

  // Geographic data
  "pais": String,
  "estado": String,
  "municipio": String,
  "local": String,

  // Ecological data
  "bioma": String,
  "comunidade_indigena": String,

  // Status
  "status": String, // 'rascunho' or 'finalizado'

  // Timestamps
  "created_at": ISODate,
  "updated_at": ISODate
}
```

### Indexes

Create indexes for performance:

```python
# In database initialization
db['referencias'].create_index('doi', unique=True)
db['referencias'].create_index('ano')
db['referencias'].create_index('status')
db['referencias'].create_index('pais')
db['referencias'].create_index([('titulo', 'text'), ('autores', 'text')])
```

### Adding New Fields

To add a new field, simply:
1. Update Pydantic model in `src/models/reference.py`
2. Add the field when creating/updating documents
3. Add index if filtering by that field
4. No migration needed (MongoDB is schema-less)

---

## Testing

### Backend Unit Tests

```bash
# Run all tests
pytest

# Run specific test file
pytest tests/test_services.py

# Run with coverage
pytest --cov=src tests/

# Verbose output
pytest -v
```

### Frontend Component Tests

```bash
# Run tests
npm run test

# Watch mode
npm run test:watch

# Coverage
npm run test:coverage
```

### Integration Testing

Test API endpoints:

```bash
# Manual testing with curl
curl -X POST http://localhost:8000/api/extract/metadata \
  -F "file=@test.pdf"

# Using httpie
http POST localhost:8000/api/articles \
  titulo="Test" \
  autores:='["Author"]' \
  ano:=2020
```

### Test Database

Use a separate MongoDB for testing:

```bash
# In tests, use test database
export MONGO_URI="mongodb://localhost:27017/etnopapers_test"
pytest
```

---

## Building & Deployment

### Development Build

```bash
# Frontend
cd frontend
npm run dev    # Development server with hot reload

# Backend
cd backend
uvicorn main:app --reload
```

### Production Build

```bash
# Frontend production build
cd frontend
npm run build
# Output: frontend/dist/

# Backend dependencies
cd backend
pip install -r requirements.txt
```

### Creating Standalone Executables

```bash
# Windows
./build-windows.bat
# Output: dist/etnopapers.exe (~150 MB)

# macOS
bash build-macos.sh
# Output: dist/Etnopapers.app (~150 MB)

# Linux
bash build-linux.sh
# Output: dist/etnopapers (~150 MB)
```

### GitHub Actions Deployment

On tag push, GitHub Actions automatically:
1. Compiles frontend (npm run build)
2. Builds executables for all platforms
3. Creates GitHub release
4. Uploads executables as artifacts

```bash
# Create release
git tag -a v2.0.0 -m "Release version 2.0.0"
git push origin v2.0.0
# Builds trigger automatically
```

---

## Performance Optimization

### Frontend

- Use React.memo() for expensive components
- Implement virtualization for large lists
- Lazy load routes with React.lazy()
- Optimize images and assets

### Backend

- Add caching for GBIF taxonomy queries (30-day TTL)
- Index frequently searched fields
- Use pagination (limit 20-100 per request)
- Async/await for I/O operations

### Database

- Indexes on: doi, status, ano, pais
- Text index on: titulo, autores
- Monitor slow queries in MongoDB logs

---

## Security Considerations

### Current Implementation

✅ **Secure by Design**:
- All processing local (no API keys sent to server)
- MongoDB Atlas supports encrypted connections
- CORS limited to localhost
- No authentication (designed for internal networks)

### Future Enhancements

- [ ] JWT authentication with HttpOnly cookies
- [ ] HTTPS enforcement
- [ ] Rate limiting on download endpoint
- [ ] Audit logging
- [ ] Input sanitization

---

## Troubleshooting

### Frontend Issues

**Port 3000 already in use**:
```bash
# Find and kill process on port 3000
# Windows: netstat -ano | findstr :3000
# macOS/Linux: lsof -i :3000
```

**Module not found errors**:
```bash
npm install
npm run dev
```

### Backend Issues

**Import errors**:
```bash
# Ensure venv is activated
source venv/bin/activate  # or venv\Scripts\activate on Windows
pip install -r requirements.txt
```

**FastAPI not starting**:
```bash
# Check Python version
python --version  # Should be 3.11+

# Check port 8000 availability
# Kill other processes on 8000
```

### MongoDB Issues

**Connection refused**:
```bash
# Verify MongoDB is running
mongosh

# If local MongoDB, restart service
# Windows: net start MongoDB
# macOS: brew services restart mongodb-community
# Linux: sudo systemctl restart mongodb
```

---

## Code Style & Conventions

### Python

- Follow PEP 8
- Type hints on all functions
- Docstrings for public functions
- Use pytest for testing
- Format with black: `black src/`

### TypeScript/React

- Use TypeScript for all files
- Interface names (PascalCase)
- Function names (camelCase)
- Constant names (UPPER_SNAKE_CASE)
- ESLint for style checking

---

## Contributing

1. Fork the repository
2. Create feature branch: `git checkout -b feature/my-feature`
3. Commit changes: `git commit -m "feat: Add my feature"`
4. Push to branch: `git push origin feature/my-feature`
5. Create Pull Request

### Commit Message Format

```
type: subject (max 50 chars)

body (wrap at 72 chars)

footer (issue references)

Examples:
feat: Add Ollama status indicator
fix: Correct MongoDB connection string handling
docs: Update API documentation
test: Add unit tests for extraction service
chore: Update dependencies
```

---

## Resources

- **FastAPI Docs**: https://fastapi.tiangolo.com/
- **React Docs**: https://react.dev/
- **MongoDB Docs**: https://docs.mongodb.com/
- **TypeScript Docs**: https://www.typescriptlang.org/docs/
- **Vite Docs**: https://vitejs.dev/

---

**Version**: 2.0
**Last Updated**: 2024-01-15
**Language**: English
