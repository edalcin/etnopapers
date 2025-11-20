# Pesquisa e Decisões Técnicas: Etnopapers

**Funcionalidade**: Sistema de Extração de Metadados de Artigos Etnobotânicos
**Branch**: main
**Criado**: 2025-11-20
**Status**: Em Desenvolvimento

## Resumo Executivo

Este documento detalha as decisões tecnológicas para implementar o sistema de extração de metadados de artigos etnobotânicos. A arquitetura foi simplificada para utilizar APIs externas de IA (Gemini, ChatGPT, Claude) com chaves fornecidas pelo usuário, eliminando a necessidade de GPU e infraestrutura local de IA.

## Pilha Tecnológica Recomendada

### Frontend

**Decisão: React com TypeScript**

**Justificativa**:
- **localStorage Management**: React hooks facilitam gerenciamento do estado de API keys armazenadas no navegador
- **Componentes Reutilizáveis**: Interface de edição de metadados requer múltiplos componentes complexos (formulários, validação, auto-save)
- **Ecossistema Rico**: Bibliotecas disponíveis para upload de PDF, validação de formulários, e gestão de estado
- **TypeScript**: Tipagem forte previne erros no gerenciamento de chaves de API e estruturas de metadados
- **Comunidade Ativa**: Amplo suporte e documentação em português

**Alternativas Consideradas**:
- **Vue.js**: Excelente opção, sintaxe mais simples, mas ecossistema ligeiramente menor para upload de PDFs
- **Svelte**: Performance superior mas comunidade menor e menos bibliotecas para funcionalidades específicas
- **Vanilla JS**: Muito trabalhoso para interface complexa de edição

**Bibliotecas Frontend Recomendadas**:
```json
{
  "react": "^18.2.0",
  "typescript": "^5.0.0",
  "axios": "^1.6.0",
  "react-hook-form": "^7.48.0",
  "react-dropzone": "^14.2.3",
  "pdf-lib": "^1.17.1",
  "zustand": "^4.4.0",
  "@tanstack/react-table": "^8.10.0"
}
```

**Justificativa das Bibliotecas**:
- `axios`: Cliente HTTP para chamadas diretas às APIs de IA
- `react-hook-form`: Gestão eficiente de formulários com validação
- `react-dropzone`: Upload de arquivos PDF drag-and-drop
- `pdf-lib`: Manipulação e validação de PDFs no navegador
- `zustand`: Gestão de estado leve para API keys e metadados em edição
- `@tanstack/react-table`: Tabela headless com ordenação, filtros e paginação integrados (anteriormente react-table v8)

### Backend

**Decisão: Python FastAPI**

**Justificativa**:
- **Leveza**: Backend mínimo (não processa IA, apenas gerencia metadados)
- **SQLite Nativo**: Python tem excelente suporte a SQLite via `sqlite3` built-in
- **API REST Rápida**: FastAPI gera documentação automática (Swagger/OpenAPI)
- **Validação de Dados**: Pydantic models garantem integridade dos metadados
- **Async/Await**: Suporte nativo para operações assíncronas (chamadas a API de taxonomia)
- **Docker Simples**: Imagens Python oficiais, sem necessidade de nvidia-docker

**Alternativas Consideradas**:
- **Node.js/Express**: Boa opção, mas Python tem melhor ecossistema para manipulação de dados científicos
- **Go**: Performance excelente mas curva de aprendizado maior e menos bibliotecas para SQLite
- **Django**: Muito pesado para necessidades mínimas deste projeto

**Dependências Backend Recomendadas**:
```python
fastapi==0.104.0
uvicorn==0.24.0
pydantic==2.4.0
sqlite3  # built-in
httpx==0.25.0  # para chamadas à API de taxonomia
python-multipart==0.0.6  # para upload de arquivos
```

### Banco de Dados

**Decisão: SQLite 3**

**Justificativa**:
- **Requisito Explícito**: Especificado na descrição inicial do projeto
- **Simplicidade**: Arquivo único, sem servidor separado
- **Portabilidade**: Fácil backup e migração (apenas copiar arquivo .db)
- **Performance Adequada**: Suficiente para volume esperado de artigos
- **Docker-Friendly**: Fácil persistência via volume Docker
- **Zero Configuração**: Funciona out-of-the-box sem setup adicional

**Schema Version**: SQLite 3.35+ (suporte a JSON1 extension para arrays de autores e nomes vernaculares)

**Migrações**: Alembic (biblioteca Python) para versionamento do schema

### APIs Externas de IA

**Decisão: Suporte a Google Gemini, OpenAI ChatGPT e Anthropic Claude**

**Integração Frontend Direta**:
- **Chamadas do Browser**: Frontend faz requests HTTPS diretos usando fetch/axios
- **CORS**: APIs de IA suportam CORS para chamadas client-side
- **API Keys no localStorage**: Nunca passam pelo backend
- **Timeout**: 60 segundos por request de extração

**Endpoints Utilizados**:

**Google Gemini**:
```
POST https://generativelanguage.googleapis.com/v1/models/gemini-pro:generateContent
Headers: { "Content-Type": "application/json", "x-goog-api-key": "<user_key>" }
```

**OpenAI ChatGPT**:
```
POST https://api.openai.com/v1/chat/completions
Headers: { "Content-Type": "application/json", "Authorization": "Bearer <user_key>" }
```

**Anthropic Claude**:
```
POST https://api.anthropic.com/v1/messages
Headers: { "Content-Type": "application/json", "x-api-key": "<user_key>", "anthropic-version": "2023-06-01" }
```

**Prompts de Extração**:
- Sistema usará prompts estruturados em português solicitando extração em formato JSON
- Template de prompt será configurável via variável de ambiente
- Resposta esperada em JSON com campos padronizados

### API de Taxonomia Botânica

**Decisão: GBIF Species API como primária, com fallback para Tropicos**

**Justificativa**:
- **GBIF**: Cobertura global, API gratuita sem autenticação, bem documentada
- **Tropicos**: Especializado em flora neotropical (relevante para estudos brasileiros)
- **Fallback**: Se GBIF falhar, tentar Tropicos; se ambos falharem, salvar como "não validado"

**Endpoints**:

**GBIF Species Match**:
```
GET https://api.gbif.org/v1/species/match?name=<nome_cientifico>
Response: { "usageKey": int, "scientificName": string, "canonicalName": string, "family": string, "status": "ACCEPTED|SYNONYM", "acceptedUsageKey": int }
```

**Tropicos Name Search** (fallback):
```
GET http://services.tropicos.org/Name/Search?name=<nome_cientifico>&apikey=<public_key>&format=json
Response: [ { "NameId": int, "ScientificName": string, "Family": string, "Authors": string } ]
```

**Cache Local**:
- Backend implementará cache em memória (dicionário Python) para consultas repetidas
- TTL: 30 dias (dados taxonômicos são estáveis)
- Persistência opcional: salvar cache em arquivo JSON ao desligar container

### Docker e Deployment

**Decisão: Multi-stage Docker build com Alpine Linux**

**Justificativa**:
- **Tamanho Reduzido**: Alpine é mínima (5-10 MB base)
- **Segurança**: Menos superfície de ataque, atualizações frequentes
- **Multi-stage**: Build separado para frontend e backend, imagem final otimizada
- **UNRAID Compatibility**: Standard Docker runtime (não requer nvidia-docker)

**Estrutura do Dockerfile**:
```dockerfile
# Stage 1: Build frontend
FROM node:18-alpine AS frontend-builder
WORKDIR /app/frontend
COPY frontend/package*.json ./
RUN npm ci --only=production
COPY frontend/ ./
RUN npm run build

# Stage 2: Backend runtime
FROM python:3.11-alpine
WORKDIR /app
COPY backend/requirements.txt ./
RUN pip install --no-cache-dir -r requirements.txt
COPY backend/ ./
COPY --from=frontend-builder /app/frontend/dist ./static

ENV DATABASE_PATH=/data/etnopapers.db
ENV PORT=8000
EXPOSE 8000

CMD ["uvicorn", "main:app", "--host", "0.0.0.0", "--port", "8000"]
```

**Variáveis de Ambiente**:
```bash
DATABASE_PATH=/data/etnopapers.db  # Caminho para SQLite (mapeado via volume)
PORT=8000                           # Porta da aplicação web
LOG_LEVEL=info                      # Nível de logging (debug|info|warning|error)
TAXONOMY_API_TIMEOUT=5              # Timeout para API de taxonomia (segundos)
CACHE_TTL_DAYS=30                   # TTL do cache de taxonomia
```

**Docker Compose para UNRAID**:
```yaml
version: '3.8'
services:
  etnopapers:
    image: etnopapers:latest
    container_name: etnopapers
    ports:
      - "8000:8000"
    volumes:
      - ./data:/data
    environment:
      - DATABASE_PATH=/data/etnopapers.db
      - LOG_LEVEL=info
    restart: unless-stopped
```

## Decisões de Arquitetura

### Separação de Responsabilidades

**Frontend**:
- Gerenciamento de API keys (localStorage)
- Upload de PDF e conversão para base64
- Chamadas diretas às APIs de IA (Gemini/ChatGPT/Claude)
- Renderização de metadados extraídos
- Interface de edição manual
- Validação de formulários
- Auto-save de rascunhos
- **Tabela de artigos com ordenação, filtros e paginação**
- **Interface de download do banco de dados**

**Backend**:
- API REST para CRUD de metadados
- Persistência em SQLite
- Validação taxonômica via GBIF/Tropicos
- Cache de consultas taxonômicas
- Servir frontend estático
- Logging e monitoramento
- **Endpoint para listagem paginada de artigos**
- **Endpoint para download do arquivo SQLite completo**
- **Verificação de integridade do banco antes de download**

### Fluxo de Processamento

```
1. Usuário seleciona provedor IA → salva no localStorage
2. Usuário insere API key → validação via chamada de teste → salva no localStorage
3. Usuário faz upload de PDF → frontend lê arquivo como base64
4. Frontend envia PDF (base64) + prompt para API de IA selecionada
5. API de IA retorna JSON com metadados extraídos
6. Frontend exibe resultados + botões "Salvar", "Editar", "Descartar"
7. Se "Salvar": frontend POST /api/articles com metadados → backend valida taxonomia → salva no SQLite
8. Se "Editar": frontend abre formulário → usuário edita → frontend POST /api/articles → backend salva
9. Se "Descartar": frontend descarta dados (não envia ao backend)
10. Se usuário fechar janela: frontend POST /api/articles com status "rascunho"
```

### Segurança e Privacidade

**API Keys**:
- **Armazenamento**: `localStorage.setItem('apiKey', key)` no navegador
- **Transmissão**: HTTPS direto do browser para API de IA (sem passar pelo backend)
- **Persistência**: Apenas no dispositivo do usuário
- **Remoção**: `localStorage.removeItem('apiKey')` via botão "Limpar Chave"

**Validação de API Key**:
```javascript
async function validateApiKey(provider, key) {
  const testPrompts = {
    gemini: { model: "gemini-pro", contents: [{ parts: [{ text: "test" }] }] },
    openai: { model: "gpt-3.5-turbo", messages: [{ role: "user", content: "test" }], max_tokens: 5 },
    claude: { model: "claude-3-haiku-20240307", messages: [{ role: "user", content: "test" }], max_tokens: 5 }
  };

  try {
    const response = await fetch(API_ENDPOINTS[provider], {
      method: 'POST',
      headers: getHeaders(provider, key),
      body: JSON.stringify(testPrompts[provider])
    });
    return response.ok;
  } catch {
    return false;
  }
}
```

**CORS**:
- Backend não precisa configurar CORS para API keys (não passam pelo backend)
- Backend configura CORS permissivo para endpoints próprios (GET/POST /api/*)

### Gestão de Estado Frontend

**Zustand Store Structure**:
```typescript
interface AppState {
  // API Configuration
  selectedProvider: 'gemini' | 'openai' | 'claude' | null;
  apiKey: string | null;
  isKeyValid: boolean;

  // Upload & Processing
  uploadedPdf: File | null;
  isProcessing: boolean;
  extractedMetadata: Metadata | null;

  // Editing
  isEditing: boolean;
  editedMetadata: Metadata | null;

  // Drafts
  drafts: Metadata[];

  // Actions
  setProvider: (provider: string) => void;
  setApiKey: (key: string) => void;
  validateKey: () => Promise<boolean>;
  uploadPdf: (file: File) => Promise<void>;
  extractMetadata: () => Promise<Metadata>;
  saveMetadata: (status: 'draft' | 'final') => Promise<void>;
  discardMetadata: () => void;
}
```

### Otimizações de Performance

**Frontend**:
- **Code Splitting**: Lazy load componentes de edição (não necessários na tela inicial)
- **PDF Preview**: Não renderizar preview completo, apenas metadados
- **Debounce**: Auto-save com debounce de 2 segundos ao editar
- **Tabela Virtual**: Usar @tanstack/react-table com paginação client-side (50 itens por página)
- **Filtro com Debounce**: Busca/filtro com debounce de 300ms para evitar re-renders excessivos
- **Memoização**: Usar React.memo para linhas da tabela evitar re-renders desnecessários

**Backend**:
- **Connection Pooling**: SQLite em modo WAL (Write-Ahead Logging) para concorrência
- **Cache de Taxonomia**: Dicionário em memória (10.000 espécies = ~5 MB RAM)
- **Async I/O**: FastAPI com async/await para operações de BD e API externa
- **Paginação no BD**: Queries com LIMIT/OFFSET para listas grandes
- **Streaming de Download**: Usar FileResponse do FastAPI para downloads grandes (evita carregar arquivo completo em memória)

**Docker**:
- **Imagem Otimizada**: Multi-stage build reduz tamanho final para ~100 MB
- **Volumes**: Apenas /data montado, melhor I/O performance

### Implementação de Tabela de Artigos

**Decisão: TanStack Table (React Table v8)**

**Justificativa**:
- **Headless**: Controle total sobre renderização e estilização
- **TypeScript First**: Tipagem forte nativa
- **Performance**: Virtualização e paginação client-side eficientes
- **Funcionalidades Built-in**: Ordenação, filtros, paginação sem bibliotecas adicionais
- **Bundle Size**: ~15KB gzipped (muito leve)

**Alternativas Consideradas**:
- **AG-Grid**: Muito pesado (~500KB), excesso de features
- **Material-UI DataGrid**: Acoplado ao Material-UI, menos flexível
- **Custom Table**: Reimplementar ordenação/filtros do zero (não vale o esforço)

**Estrutura da Tabela**:
```typescript
interface ArticleRow {
  id: number;
  titulo: string;
  ano_publicacao: number;
  autores: string[];  // Mostrar apenas primeiro autor + "et al."
  status: 'finalizado' | 'rascunho';
  data_processamento: string;
  total_especies: number;
}

const columns: ColumnDef<ArticleRow>[] = [
  { accessorKey: 'titulo', header: 'Título', sortable: true },
  { accessorKey: 'ano_publicacao', header: 'Ano', sortable: true },
  { accessorKey: 'autores', header: 'Autores', sortable: false },  // Não ordenável (array)
  { accessorKey: 'status', header: 'Status', sortable: true },
  { accessorKey: 'data_processamento', header: 'Processado em', sortable: true },
  { accessorKey: 'total_especies', header: 'Espécies', sortable: true }
];
```

**Filtro Global**:
```typescript
const [globalFilter, setGlobalFilter] = useState('');

// Debounced filter
const debouncedFilter = useMemo(
  () => debounce((value: string) => setGlobalFilter(value), 300),
  []
);

// Busca em múltiplos campos
const filterFn = (row, columnId, filterValue) => {
  const searchableFields = [
    row.getValue('titulo'),
    row.getValue('ano_publicacao')?.toString(),
    row.getValue('autores')?.join(' '),
    row.getValue('status')
  ];

  return searchableFields.some(field =>
    field?.toLowerCase().includes(filterValue.toLowerCase())
  );
};
```

**Paginação**:
- **Client-side**: Todos os dados carregados de uma vez
- **50 itens por página**: Balanceamento entre scroll e performance
- **Navegação**: Primeira, Anterior, Próxima, Última página
- **Indicador**: "Mostrando 1-50 de 237 artigos"

### Download do Banco de Dados

**Decisão: Streaming com FastAPI FileResponse**

**Justificativa**:
- **Eficiência de Memória**: Não carrega arquivo completo em RAM
- **Suporte Nativo**: FastAPI FileResponse otimizada para arquivos grandes
- **Compatibilidade**: Funciona com qualquer tamanho de banco
- **Simplicidade**: Menos código que implementação custom

**Implementação Backend**:
```python
from fastapi import FastAPI
from fastapi.responses import FileResponse
import sqlite3
import shutil
from datetime import datetime

@app.get("/api/database/download")
async def download_database():
    """
    Endpoint para download do banco SQLite completo.
    Verifica integridade antes de enviar.
    """
    db_path = os.getenv('DATABASE_PATH', '/data/etnopapers.db')

    # Verificar integridade
    conn = sqlite3.connect(db_path)
    cursor = conn.cursor()
    cursor.execute("PRAGMA integrity_check;")
    integrity = cursor.fetchone()[0]
    conn.close()

    if integrity != "ok":
        raise HTTPException(status_code=500, detail="Database integrity check failed")

    # Gerar nome com data
    date_str = datetime.now().strftime('%Y%m%d')
    filename = f"etnopapers_{date_str}.db"

    return FileResponse(
        path=db_path,
        media_type='application/x-sqlite3',
        filename=filename,
        headers={
            'Content-Disposition': f'attachment; filename="{filename}"'
        }
    )
```

**Implementação Frontend**:
```typescript
async function downloadDatabase() {
  try {
    const response = await fetch('/api/database/download');

    if (!response.ok) {
      throw new Error('Download failed');
    }

    // Obter nome do arquivo do header
    const contentDisposition = response.headers.get('Content-Disposition');
    const filename = contentDisposition?.match(/filename="(.+)"/)?.[1] || 'etnopapers.db';

    // Criar blob e trigger download
    const blob = await response.blob();
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    window.URL.revokeObjectURL(url);
    document.body.removeChild(a);

    // Feedback
    toast.success(`Banco de dados baixado: ${filename}`);
  } catch (error) {
    toast.error('Erro ao baixar banco de dados');
  }
}
```

**Considerações de Segurança**:
- **Verificação de Integridade**: PRAGMA integrity_check antes de download
- **Rate Limiting**: Limitar downloads para evitar abuso (ex: 1 download por minuto por IP)
- **Tamanho Máximo**: Alertar se banco > 100MB (pode demorar)
- **Permissões**: Endpoint acessível apenas se usuário tem permissão (futuro: autenticação)

**Casos de Uso do Download**:
1. **Backup Manual**: Usuário baixa periodicamente para backup local
2. **Análise Externa**: Importar em R, Python, Excel para análises estatísticas
3. **Migração**: Mover dados para outro servidor
4. **Auditoria**: Revisão completa dos dados coletados
5. **Compartilhamento**: Enviar dados completos para colaboradores

## Pontos de Decisão Técnica Pendentes

### 1. Extração de PDF no Frontend vs Backend

**Opção A: Frontend (Recomendado)**
- Pros: Sem upload de PDF para servidor, privacidade total, reduz carga do servidor
- Cons: PDF grande = base64 grande em request para API de IA (limite de tokens)

**Opção B: Backend**
- Pros: Pode pré-processar PDF (extração de texto, OCR se necessário)
- Cons: Viola requisito de não armazenar PDF, upload pesado

**Decisão**: Opção A (frontend extrai texto do PDF usando pdf.js e envia apenas texto para API de IA)

### 2. Formato de Resposta da API de IA

**Opção A: JSON Estruturado**
- Frontend inclui no prompt: "Responda APENAS com JSON no formato: { 'titulo': '...', 'autores': [...], ... }"
- Parsing: `JSON.parse(response)`
- Pros: Estruturado, fácil validação
- Cons: APIs podem gerar JSON malformado

**Opção B: Texto Livre + Parsing**
- Frontend permite resposta em texto livre
- Backend faz parsing com regex/NLP
- Pros: Mais robusto a variações da IA
- Cons: Complexo, propenso a erros

**Decisão**: Opção A com fallback (tentar JSON.parse, se falhar exibir erro e permitir edição manual)

### 3. Estratégia de Cache de Taxonomia

**Opção A: Cache em Memória (Dict Python)**
- Pros: Rápido (O(1) lookup), simples
- Cons: Perde cache ao reiniciar container

**Opção B: Cache em SQLite**
- Pros: Persiste entre reinícios
- Cons: I/O adicional, mais complexo

**Decisão**: Opção A com persistência opcional (salvar dict como JSON ao shutdown, carregar no startup)

### 4. Limite de Tamanho de PDF

**Restrição**: 50 MB por arquivo (especificado)

**Implementação**:
- Frontend: Validação antes de upload via `file.size`
- Timeout API de IA: 60 segundos (artigos grandes podem demorar)

## Estimativas de Recursos

### Hardware (Servidor UNRAID)

**Requisitos Mínimos**:
- **CPU**: 2 cores (processamento mínimo, apenas servir frontend e SQLite)
- **RAM**: 512 MB (backend) + 256 MB (frontend build) = 768 MB total
- **Disco**: 100 MB (imagem Docker) + 10 GB (dados SQLite, estimativa para 10.000 artigos)
- **Rede**: Conexão estável para chamadas a APIs externas

**Escalabilidade**:
- Volume esperado: 1-10 usuários simultâneos (uso acadêmico individual/pequeno grupo)
- Throughput: 1-5 artigos processados por hora
- Crescimento: SQLite suporta até 140 TB (limite teórico), adequado para décadas de uso

### Custos de API de IA

**Google Gemini** (recomendado para começar):
- **Quota Gratuita**: 60 requests/minuto
- **Custo após quota**: ~$0.00025 por 1K caracteres (entrada)
- **Estimativa por artigo**: PDF de 30 páginas ≈ 50K caracteres ≈ $0.0125 (1 centavo)

**OpenAI ChatGPT**:
- **Modelo gpt-3.5-turbo**: $0.0015 por 1K tokens (entrada)
- **Estimativa por artigo**: 50K caracteres ≈ 12.5K tokens ≈ $0.01875 (2 centavos)

**Anthropic Claude**:
- **Modelo claude-3-haiku**: $0.00025 por 1K tokens (entrada)
- **Estimativa por artigo**: 50K caracteres ≈ 12.5K tokens ≈ $0.003125 (menos de 1 centavo)

**Recomendação**: Começar com Gemini (quota gratuita generosa), migrar para Claude se precisar escalar com baixo custo.

## Próximos Passos

1. **Definir schema detalhado do SQLite** (data-model.md)
2. **Especificar contratos da API REST** (contracts/)
3. **Criar guia de início rápido** (quickstart.md)
4. **Implementar protótipo mínimo**:
   - Frontend: Tela de configuração de API key + upload
   - Backend: Endpoint POST /api/articles
   - Validação com 1 artigo de teste

## Referências

- [FastAPI Documentation](https://fastapi.tiangolo.com/)
- [React TypeScript Handbook](https://react-typescript-cheatsheet.netlify.app/)
- [SQLite Documentation](https://www.sqlite.org/docs.html)
- [Google Gemini API](https://ai.google.dev/docs)
- [OpenAI API Reference](https://platform.openai.com/docs/api-reference)
- [Anthropic Claude API](https://docs.anthropic.com/claude/reference)
- [GBIF Species API](https://www.gbif.org/developer/species)
- [Tropicos API](http://services.tropicos.org/help)
