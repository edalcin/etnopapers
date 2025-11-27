# Developer Quickstart Guide

**Version**: 3.0.0
**Target**: Standalone Desktop Application with Embedded UI
**Audience**: Developers building the Etnopapers desktop application

---

## Prerequisites

### System Requirements

- **Node.js**: 18.x or 20.x (for frontend build)
- **Python**: 3.11 or 3.12 (for backend)
- **Git**: For version control
- **MongoDB**: Local instance (default `localhost:27017`) or cloud connection (MongoDB Atlas)
- **Ollama**: Installed and running on `localhost:11434` with `qwen2.5:7b-instruct-q4_K_M` model downloaded
- **npm**: 9.x or higher (comes with Node.js)
- **pip**: Python package manager

### Development Tools (Optional but Recommended)

- **VS Code** with Python and Prettier extensions
- **MongoDB Compass** for database inspection
- **Postman** or **curl** for API testing

---

## Project Structure Overview

```
etnopapers/
├── frontend/                  # React 18 + TypeScript SPA
│   ├── src/components/        # UI components
│   ├── src/pages/            # Page layouts
│   ├── src/store/            # Zustand state management
│   └── package.json
├── backend/                   # Python FastAPI server
│   ├── main.py               # Entry point
│   ├── launcher.py           # Desktop app startup wrapper
│   ├── routers/              # API endpoints
│   ├── services/             # Business logic
│   ├── models/               # Pydantic schemas
│   └── requirements.txt
├── build/                     # Build scripts
│   ├── build-windows.bat
│   ├── build-macos.sh
│   └── build-linux.sh
└── specs/                     # Feature specifications
    ├── spec.md               # User requirements
    ├── plan.md               # Architecture & tech decisions
    ├── research.md           # Technology research
    ├── data-model.md         # Database schema
    └── contracts/api-rest.yaml
```

---

## Development Workflow

### 1. Initial Setup

#### Clone Repository

```bash
git clone https://github.com/edalcin/etnopapers.git
cd etnopapers
```

#### Backend Setup

```bash
# Create virtual environment
python -m venv venv

# Activate virtual environment
# On Windows:
venv\Scripts\activate
# On macOS/Linux:
source venv/bin/activate

# Install dependencies
pip install -r backend/requirements.txt
```

#### Frontend Setup

```bash
# Install Node dependencies
npm install --prefix frontend

# Verify Node.js and npm versions
node --version  # Should be 18.x or 20.x
npm --version   # Should be 9.x or higher
```

#### Environment Configuration

Create `.env` file in project root:

```bash
# MongoDB connection
MONGO_URI=mongodb://localhost:27017/etnopapers

# Ollama endpoint
OLLAMA_URL=http://localhost:11434
OLLAMA_MODEL=qwen2.5:7b-instruct-q4_K_M

# Backend server (for development)
API_HOST=0.0.0.0
API_PORT=8000
API_LOG_LEVEL=debug
```

### 2. Development Server (Full Stack)

#### Terminal 1: Start Backend

```bash
# Activate virtual environment
source venv/bin/activate  # or venv\Scripts\activate on Windows

# Start FastAPI server (with auto-reload)
uvicorn backend.main:app --reload --host 0.0.0.0 --port 8000

# You should see:
# INFO:     Uvicorn running on http://0.0.0.0:8000
# INFO:     Application startup complete
```

#### Terminal 2: Start Frontend Dev Server

```bash
# Install and start Vite dev server
npm run dev --prefix frontend

# You should see:
# ➜  Local:   http://localhost:5173/
# ➜  press h to show help
```

#### Terminal 3: Test API Connectivity

```bash
# Verify backend is running
curl http://localhost:8000/health

# Expected response:
# {"status":"healthy","backend":"ok","mongodb":"ok","ollama":"ok","timestamp":"2025-11-27T12:00:00Z"}
```

**Open Browser**: Navigate to `http://localhost:5173`

---

## Backend Development

### API Endpoints (Development)

| Endpoint | Method | Purpose |
|----------|--------|---------|
| `/health` | GET | System health check |
| `/api/articles` | GET | List articles |
| `/api/articles` | POST | Create article |
| `/api/articles/{id}` | GET | Get article details |
| `/api/articles/{id}` | PUT | Update article |
| `/api/articles/{id}` | DELETE | Delete article |
| `/api/extract/metadata` | POST | Upload PDF & extract |
| `/api/database/download` | GET | Download database backup |
| `/docs` | GET | Swagger UI (API documentation) |
| `/openapi.json` | GET | OpenAPI schema |

### Testing Backend

#### Run Unit Tests

```bash
# Activate virtual environment
source venv/bin/activate

# Run all tests
pytest backend/tests -v

# Run specific test file
pytest backend/tests/test_article_service.py -v

# Run with coverage
pytest backend/tests --cov=backend
```

#### Test API Endpoints with curl

```bash
# Test health endpoint
curl http://localhost:8000/health | jq

# Test list articles
curl http://localhost:8000/api/articles | jq

# Test create article
curl -X POST http://localhost:8000/api/articles \
  -H "Content-Type: application/json" \
  -d '{
    "ano": 2010,
    "titulo": "Test Article",
    "autores": ["Test Author"],
    "especies": [{"vernacular": "teste", "nomeCientifico": "Test species"}],
    "tipo_de_uso": "medicinal",
    "metodologia": "entrevistas",
    "localizacao": {"pais": "Brasil"}
  }' | jq

# Test PDF extraction (requires PDF file)
curl -X POST http://localhost:8000/api/extract/metadata \
  -F "file=@/path/to/sample.pdf" | jq
```

#### Debug with IDE

**VS Code Debugging** (`launch.json`):

```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": "FastAPI",
      "type": "python",
      "request": "launch",
      "module": "uvicorn",
      "args": ["backend.main:app", "--reload"],
      "jinja": true,
      "justMyCode": true
    }
  ]
}
```

---

## Frontend Development

### Component Structure

**Location**: `frontend/src/components/`

| Component | Purpose |
|-----------|---------|
| `PDFUpload.tsx` | File picker + drag-and-drop |
| `MetadataDisplay.tsx` | Show extracted metadata |
| `MetadataEditor.tsx` | Edit metadata form |
| `ArticlesTable.tsx` | List & filter articles |
| `ConfigDialog.tsx` | MongoDB URI setup |
| `ArticleDetail.tsx` | Single article view |

### Styling

- **Framework**: Tailwind CSS (existing)
- **Component UI**: Custom Tailwind + Shadcn (if used)

### State Management

**Zustand Store** (`frontend/src/store/`):

```typescript
// Access state in components
import { useStore } from '@/store'

export function MyComponent() {
  const metadata = useStore((state) => state.metadata)
  const setMetadata = useStore((state) => state.setMetadata)

  return (
    <div>
      <button onClick={() => setMetadata({...})}>Update</button>
    </div>
  )
}
```

### Testing Frontend

#### Run Component Tests

```bash
# Run Vitest
npm run test --prefix frontend

# Watch mode
npm run test:watch --prefix frontend

# Coverage
npm run test:coverage --prefix frontend
```

#### Testing Example

```typescript
// frontend/src/components/__tests__/PDFUpload.test.tsx
import { render, screen } from '@testing-library/react'
import PDFUpload from '@/components/PDFUpload'

test('renders file upload button', () => {
  render(<PDFUpload />)
  expect(screen.getByText(/upload pdf/i)).toBeInTheDocument()
})
```

---

## Building for Production

### 1. Build Frontend Assets

```bash
# Generate optimized production build
npm run build --prefix frontend

# Output: frontend/dist/
# Files: index.html, assets/*, etc.
```

### 2. Build Desktop Executables

#### On Windows

```bash
# Run build script
.\build\build-windows.bat

# Output: dist\etnopapers-windows-v3.0.0.exe
```

#### On macOS

```bash
# Run build script
bash build/build-macos.sh

# Output: dist/Etnopapers-macos-v3.0.0.zip
```

#### On Linux

```bash
# Run build script
bash build/build-linux.sh

# Output: dist/etnopapers-linux-v3.0.0
```

### Build Process

Each build script:
1. Cleans previous builds
2. Runs `npm run build` (frontend → dist/)
3. Installs backend dependencies
4. Runs PyInstaller with custom spec
5. Outputs platform-specific executable

---

## Debugging

### Backend Logs

```bash
# Set log level
export API_LOG_LEVEL=debug  # or info, warning, error

# Tail logs in real-time
tail -f backend.log
```

### Frontend DevTools

**Browser DevTools** (Ctrl+Shift+I in desktop app):
- Console: See React errors
- Network: Monitor API calls
- Application: Inspect state, localStorage

### MongoDB Debugging

```bash
# Connect to MongoDB
mongosh mongodb://localhost:27017/etnopapers

# List all articles
db.referencias.find().pretty()

# Count articles
db.referencias.countDocuments()

# Find by title
db.referencias.findOne({ titulo: /test/i })

# Check indexes
db.referencias.getIndexes()
```

### Ollama Health Check

```bash
# Test Ollama is running
curl http://localhost:11434/api/tags

# Test model availability
curl http://localhost:11434/api/generate \
  -H "Content-Type: application/json" \
  -d '{
    "model": "qwen2.5:7b-instruct-q4_K_M",
    "prompt": "Hello"
  }' | jq
```

---

## Common Development Tasks

### Add New API Endpoint

1. **Create router** (`backend/routers/my_feature.py`):
```python
from fastapi import APIRouter

router = APIRouter()

@router.get("/my-endpoint")
async def my_endpoint():
    return {"message": "Hello"}
```

2. **Register in main.py**:
```python
from backend.routers import my_feature
app.include_router(my_feature.router, prefix="/api")
```

3. **Test in browser/curl**:
```bash
curl http://localhost:8000/api/my-endpoint
```

### Add New Database Field

1. **Update schema** (`backend/models/article.py`):
```python
class ReferenceData(BaseModel):
    new_field: Optional[str] = None
```

2. **Update extraction prompt** (`backend/services/extraction_service.py`):
```python
# Add to Ollama prompt
"Extract the new_field if present..."
```

3. **No migration needed!** MongoDB is schema-less

### Add New React Component

1. **Create component file** (`frontend/src/components/MyComponent.tsx`):
```typescript
export default function MyComponent() {
  return <div>My Component</div>
}
```

2. **Use in page** (`frontend/src/pages/Home.tsx`):
```typescript
import MyComponent from '@/components/MyComponent'

export default function Home() {
  return <MyComponent />
}
```

---

## Troubleshooting

### "Module not found" errors (Python)

```bash
# Reinstall dependencies
pip install -r backend/requirements.txt

# Verify virtual environment is activated
which python  # Should show venv path
```

### "Cannot find module" errors (TypeScript)

```bash
# Clear node_modules and reinstall
rm -rf frontend/node_modules frontend/package-lock.json
npm install --prefix frontend
```

### Ollama connection fails

```bash
# Check Ollama is running
ps aux | grep ollama

# Start Ollama if not running
ollama serve

# Test connectivity
curl http://localhost:11434/api/tags
```

### MongoDB connection fails

```bash
# Check MongoDB is running
mongosh --eval "db.adminCommand('ping')"

# Start MongoDB if not running (macOS with Homebrew)
brew services start mongodb-community

# Verify connection string in .env
echo $MONGO_URI
```

### Port already in use (8000 or 5173)

```bash
# Find process using port
lsof -i :8000  # For backend
lsof -i :5173  # For frontend

# Kill process
kill -9 <PID>
```

---

## Next Steps

1. **Read the specification**: `specs/spec.md` for user requirements
2. **Review the architecture**: `specs/plan.md` for technical decisions
3. **Understand the data model**: `specs/data-model.md` for database schema
4. **Check API contracts**: `specs/contracts/api-rest.yaml` for endpoint specs
5. **Start coding**: Pick a task from `specs/tasks.md` and implement

---

## Resources

- **FastAPI Docs**: https://fastapi.tiangolo.com/
- **React Docs**: https://react.dev/
- **MongoDB Docs**: https://docs.mongodb.com/
- **Ollama Docs**: https://ollama.ai/
- **Tailwind CSS**: https://tailwindcss.com/

---

**Questions?** Check existing GitHub issues or create a new one.
