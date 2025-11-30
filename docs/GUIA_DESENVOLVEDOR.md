# Guia do Desenvolvedor Etnopapers

**Versão**: 2.0
**Linguagem**: Português (Brasil)
**Atualizado**: 2024-01-15

---

## Índice

1. [Setup de Desenvolvimento](#setup-de-desenvolvimento)
2. [Arquitetura do Projeto](#arquitetura-do-projeto)
3. [Desenvolvimento Frontend](#desenvolvimento-frontend)
4. [Desenvolvimento Backend](#desenvolvimento-backend)
5. [Design do Banco de Dados](#design-do-banco-de-dados)
6. [Testes](#testes)
7. [Build e Deployment](#build-e-deployment)
8. [Contribuindo](#contribuindo)

---

## Setup de Desenvolvimento

### Pré-requisitos

- **Node.js**: 18.x ou superior
- **Python**: 3.11 ou superior
- **Git**: Versão mais recente
- **MongoDB**: Instância local ou conta Atlas
- **Ollama**: https://ollama.com/download

### Setup Inicial

```bash
# Clonar repositório
git clone https://github.com/edalcin/etnopapers.git
cd etnopapers

# Setup frontend
cd frontend
npm install
npm run dev  # Inicia em http://localhost:3000

# Setup backend (em outro terminal)
cd backend
python -m venv venv

# Ativar ambiente virtual
# Windows:
venv\Scripts\activate
# macOS/Linux:
source venv/bin/activate

# Instalar dependências
pip install -r requirements.txt

# Configurar variáveis de ambiente
export MONGO_URI="mongodb://localhost:27017/etnopapers"
export OLLAMA_URL="http://localhost:11434"

# Iniciar servidor de desenvolvimento
uvicorn main:app --reload

# Documentação da API disponível em: http://localhost:8000/docs
```

---

## Arquitetura do Projeto

### Estrutura de Diretórios

```
etnopapers/
├── backend/
│   ├── src/
│   │   ├── models/          # Schemas Pydantic
│   │   │   ├── reference.py
│   │   │   └── species.py
│   │   ├── routers/         # Handlers de rotas FastAPI
│   │   │   ├── health.py
│   │   │   ├── articles.py
│   │   │   ├── extraction.py
│   │   │   ├── database.py
│   │   │   └── configuration.py
│   │   ├── services/        # Lógica de negócio
│   │   │   ├── database_service.py
│   │   │   ├── pdf_service.py
│   │   │   ├── extraction_service.py
│   │   │   ├── search_service.py
│   │   │   ├── backup_service.py
│   │   │   ├── config_service.py
│   │   │   ├── article_service.py
│   │   │   └── ollama_service.py
│   │   ├── database/        # Conexão com banco de dados
│   │   │   └── connection.py
│   │   ├── middleware/      # Middleware (tratamento de erros)
│   │   │   └── error_handler.py
│   │   └── config/          # Configuração
│   │       └── logging.py
│   ├── tests/               # Testes unitários e integração
│   │   └── test_services.py
│   ├── main.py              # Entrada da aplicação FastAPI
│   ├── launcher.py          # Launcher da aplicação desktop
│   ├── build.spec           # Configuração PyInstaller
│   └── requirements.txt      # Dependências Python
│
├── frontend/
│   ├── src/
│   │   ├── components/      # Componentes React
│   │   │   ├── PDFUpload.tsx
│   │   │   ├── MetadataDisplay.tsx
│   │   │   ├── ArticlesTable.tsx
│   │   │   ├── MainLayout.tsx
│   │   │   └── ...
│   │   ├── pages/           # Componentes de página
│   │   │   ├── Home.tsx
│   │   │   └── Settings.tsx
│   │   ├── services/        # Clientes API
│   │   │   ├── api.ts
│   │   │   ├── healthService.ts
│   │   │   └── configService.ts
│   │   ├── hooks/           # Custom React hooks
│   │   │   └── useOllamaHealth.ts
│   │   ├── store/           # Stores Zustand
│   │   │   ├── appStore.ts
│   │   │   ├── configStore.ts
│   │   │   ├── extractionStore.ts
│   │   │   └── articleStore.ts
│   │   ├── __tests__/       # Testes de componentes
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
│   ├── API_DOCUMENTACAO.md
│   ├── GUIA_USUARIO.md
│   ├── GUIA_DESENVOLVEDOR.md
│   └── ...
│
├── .github/workflows/       # GitHub Actions
│   └── build-release.yml
│
├── build-windows.bat
├── build-macos.sh
├── build-linux.sh
└── CLAUDE.md               # Instruções do projeto
```

### Stack de Tecnologia

**Frontend**:
- React 18 com TypeScript
- Vite para build/desenvolvimento
- Zustand para gerenciamento de estado
- TanStack React Table v8
- react-hook-form
- CSS3 com animações

**Backend**:
- FastAPI (framework web Python)
- PyMongo (driver MongoDB)
- pdfplumber (extração de texto de PDF)
- instructor (saídas LLM estruturadas)
- Ollama (inferência LLM local)
- Uvicorn (servidor ASGI)

**Banco de Dados**:
- MongoDB (armazenamento de documentos NoSQL)
- Single collection model (tudo em `referencias`)
- Campos indexados: DOI (único), status, ano, busca full-text

**Distribuição**:
- PyInstaller (empacotamento executável)
- GitHub Actions (CI/CD)
- Multi-plataforma: Windows, macOS, Linux

---

## Desenvolvimento Frontend

### Estrutura de Componentes

Cada componente deve seguir esta estrutura:

```typescript
import { useState } from 'react'
import './ComponentName.css'

interface ComponentNameProps {
  onAction?: () => void
}

export default function ComponentName({ onAction }: ComponentNameProps) {
  const [state, setState] = useState(false)

  const handleAction = () => {
    // Lógica da ação
    onAction?.()
  }

  return (
    <div className="component-name">
      {/* JSX */}
    </div>
  )
}
```

### Convenções de CSS

- Use nomes BEM: `.component-name__element--modifier`
- Design mobile-first responsivo
- Variáveis para cores (definidas em index.css)
- Animações definidas no nível do componente

**Exemplo de CSS**:
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

### Gerenciamento de Estado com Zustand

Criar um novo store:

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

Usar em componente:
```typescript
import { useExampleStore } from '../store/exampleStore'

export default function Component() {
  const { data, setData } = useExampleStore()

  return (
    <button onClick={() => setData(newValue)}>
      {data?.name || 'Sem dados'}
    </button>
  )
}
```

### Custom Hooks

Criar lógica reutilizável com custom hooks:

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

### Comunicação com API

Usar o serviço api.ts:

```typescript
// src/services/api.ts
export async function fetchArticles(page: number) {
  const response = await fetch(`/api/articles?skip=${page * 20}&limit=20`)
  if (!response.ok) throw new Error('Falha ao buscar')
  return response.json()
}
```

---

## Desenvolvimento Backend

### Criar um Novo Endpoint

1. **Definir Modelo Pydantic** (`src/models/`)

```python
from pydantic import BaseModel
from typing import Optional

class MyDataModel(BaseModel):
    name: str
    description: Optional[str] = None
```

2. **Criar Serviço** (`src/services/my_service.py`)

```python
from typing import Dict, Any

class MyService:
    @staticmethod
    def process_data(data: str) -> Dict[str, Any]:
        # Lógica de negócio
        return {"result": processed_data}
```

3. **Criar Router** (`src/routers/my_router.py`)

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

4. **Registrar Router** em `main.py`

```python
from src.routers import my_router

app.include_router(my_router.router)
```

### Operações de Banco de Dados

Usar PyMongo com MongoDB:

```python
from src.database.connection import DatabaseConnection

# Obter instância de banco de dados
db = DatabaseConnection.get_db()

# Inserir documento
result = db['referencias'].insert_one({
    'titulo': 'Exemplo',
    'ano': 2020
})

# Buscar documentos
articles = db['referencias'].find({'ano': 2020})

# Atualizar documento
db['referencias'].update_one(
    {'_id': ObjectId('...')},
    {'$set': {'status': 'finalizado'}}
)

# Deletar documento
db['referencias'].delete_one({'_id': ObjectId('...')})
```

### Tratamento de Erros

Usar exceções FastAPI com status codes apropriados:

```python
from fastapi import HTTPException

# 400 Requisição Inválida
raise HTTPException(
    status_code=400,
    detail="Descrição do erro em português"
)

# 404 Não Encontrado
raise HTTPException(
    status_code=404,
    detail="Recurso não encontrado"
)

# 503 Serviço Indisponível
raise HTTPException(
    status_code=503,
    detail="Serviço indisponível"
)
```

### Logging

Usar o sistema de logging configurado:

```python
import logging

logger = logging.getLogger(__name__)

logger.info("Operação bem-sucedida")
logger.warning("Algo incomum aconteceu")
logger.error("Um erro ocorreu", exc_info=True)
```

---

## Design do Banco de Dados

### Modelo Single Collection

Todos os dados armazenados em coleção `referencias`:

```javascript
{
  "_id": ObjectId,

  // Metadados do artigo
  "titulo": String,
  "autores": [String],
  "ano": Number,
  "publicacao": String,
  "doi": String (índice único),
  "resumo": String,

  // Informações de espécies (array)
  "especies": [
    {
      "vernacular": String,
      "nomeCientifico": String,
      "familia": String
    }
  ],

  // Dados etnobotânicos
  "tipo_de_uso": String,
  "metodologia": String,

  // Dados geográficos
  "pais": String,
  "estado": String,
  "municipio": String,
  "local": String,

  // Dados ecológicos
  "bioma": String,
  "comunidade_indigena": String,

  // Status
  "status": String, // 'rascunho' ou 'finalizado'

  // Timestamps
  "created_at": ISODate,
  "updated_at": ISODate
}
```

### Índices

Criar índices para performance:

```python
# Na inicialização do banco de dados
db['referencias'].create_index('doi', unique=True)
db['referencias'].create_index('ano')
db['referencias'].create_index('status')
db['referencias'].create_index('pais')
db['referencias'].create_index([('titulo', 'text'), ('autores', 'text')])
```

### Adicionar Novos Campos

Para adicionar um novo campo, simplesmente:
1. Atualizar modelo Pydantic em `src/models/reference.py`
2. Adicionar o campo ao criar/atualizar documentos
3. Adicionar índice se filtrar por esse campo
4. Nenhuma migração necessária (MongoDB é schema-less)

---

## Testes

### Testes Backend

```bash
# Executar todos os testes
pytest

# Arquivo de teste específico
pytest tests/test_services.py

# Com cobertura
pytest --cov=src tests/

# Saída verbose
pytest -v
```

### Testes Frontend

```bash
# Executar testes
npm run test

# Modo watch
npm run test:watch

# Cobertura
npm run test:coverage
```

### Testes de Integração

Testar endpoints da API:

```bash
# Teste manual com curl
curl -X POST http://localhost:8000/api/extract/metadata \
  -F "file=@test.pdf"

# Usando httpie
http POST localhost:8000/api/articles \
  titulo="Teste" \
  autores:='["Autor"]' \
  ano:=2020
```

### Banco de Dados de Teste

Use banco de dados MongoDB separado para testes:

```bash
# Em testes, use banco de dados de teste
export MONGO_URI="mongodb://localhost:27017/etnopapers_test"
pytest
```

---

## Build e Deployment

### Build de Desenvolvimento

```bash
# Frontend
cd frontend
npm run dev    # Servidor de desenvolvimento com hot reload

# Backend
cd backend
uvicorn main:app --reload
```

### Build de Produção

```bash
# Build frontend de produção
cd frontend
npm run build
# Output: frontend/dist/

# Dependências backend
cd backend
pip install -r requirements.txt
```

### Criar Executáveis Standalone

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

### Deployment Automatizado com GitHub Actions

Ao fazer push de uma tag de versão, GitHub Actions automaticamente:
1. Compila frontend (npm run build)
2. Constrói executáveis para todas as plataformas
3. Cria release no GitHub
4. Faz upload dos executáveis como artefatos

```bash
# Criar release
git tag -a v2.0.0 -m "Release version 2.0.0"
git push origin v2.0.0
# Builds disparam automaticamente
```

---

## Otimização de Performance

### Frontend

- Use React.memo() para componentes caros
- Implemente virtualização para listas longas
- Lazy load rotas com React.lazy()
- Otimize imagens e assets

### Backend

- Adicione cache para consultas GBIF (TTL de 30 dias)
- Indexe campos frequentemente buscados
- Use paginação (limite 20-100 por requisição)
- Async/await para operações I/O

### Banco de Dados

- Índices em: doi, status, ano, pais
- Índice full-text em: titulo, autores
- Monitore queries lentas nos logs MongoDB

---

## Considerações de Segurança

### Implementação Atual

✅ **Seguro por Design**:
- Processamento local (sem API keys enviadas ao servidor)
- MongoDB Atlas suporta conexões criptografadas
- CORS limitado a localhost
- Sem autenticação (projetado para redes internas)

### Melhorias Futuras

- [ ] Autenticação JWT com HttpOnly cookies
- [ ] Imposição de HTTPS
- [ ] Rate limiting no endpoint de download
- [ ] Logging de auditoria
- [ ] Sanitização de entrada

---

## Solução de Problemas

### Problemas do Frontend

**Porta 3000 já em uso**:
```bash
# Encontre e mate o processo na porta 3000
# Windows: netstat -ano | findstr :3000
# macOS/Linux: lsof -i :3000
```

**Erros de módulo não encontrado**:
```bash
npm install
npm run dev
```

### Problemas do Backend

**Erros de importação**:
```bash
# Garanta que venv está ativado
source venv/bin/activate  # ou venv\Scripts\activate no Windows
pip install -r requirements.txt
```

**FastAPI não inicia**:
```bash
# Verifique versão Python
python --version  # Deve ser 3.11+

# Verifique disponibilidade da porta 8000
# Mate outros processos na porta 8000
```

### Problemas MongoDB

**Conexão recusada**:
```bash
# Verifique se MongoDB está rodando
mongosh

# Se MongoDB local, reinicie o serviço
# Windows: net start MongoDB
# macOS: brew services restart mongodb-community
# Linux: sudo systemctl restart mongodb
```

---

## Estilo de Código e Convenções

### Python

- Siga PEP 8
- Type hints em todas as funções
- Docstrings para funções públicas
- Use pytest para testes
- Formatar com black: `black src/`

### TypeScript/React

- Use TypeScript para todos os arquivos
- Nomes de Interface (PascalCase)
- Nomes de função (camelCase)
- Nomes de constante (UPPER_SNAKE_CASE)
- ESLint para verificação de estilo

---

## Contribuindo

1. Fork o repositório
2. Crie branch de feature: `git checkout -b feature/minha-feature`
3. Faça commits: `git commit -m "feat: Adicionar minha feature"`
4. Push para branch: `git push origin feature/minha-feature`
5. Crie Pull Request

### Formato de Mensagem de Commit

```
type: subject (máx 50 caracteres)

body (quebra em 72 caracteres)

footer (referências de issue)

Exemplos:
feat: Adicionar indicador de status do Ollama
fix: Corrigir tratamento de string de conexão MongoDB
docs: Atualizar documentação da API
test: Adicionar testes unitários para serviço de extração
chore: Atualizar dependências
```

---

## Recursos

- **FastAPI Docs**: https://fastapi.tiangolo.com/
- **React Docs**: https://react.dev/
- **MongoDB Docs**: https://docs.mongodb.com/
- **TypeScript Docs**: https://www.typescriptlang.org/docs/
- **Vite Docs**: https://vitejs.dev/

---

**Versão**: 2.0
**Última Atualização**: 2024-01-15
**Linguagem**: Português (Brasil)
