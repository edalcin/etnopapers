# Developer Quickstart: EtnoPapers

**Date**: 2025-12-01
**Target Audience**: Developers joining the EtnoPapers project

## Project Overview

EtnoPapers is an Electron-based Windows desktop application that extracts ethnobotanical metadata from scientific papers using AI (OLLAMA) and syncs data to MongoDB.

**Key Technologies**:
- **Frontend**: Electron + React + TypeScript + Tailwind CSS
- **State**: Zustand
- **Local Storage**: lowdb (JSON file)
- **Cloud Storage**: MongoDB (official driver)
- **PDF Processing**: pdf.js
- **AI Integration**: OLLAMA REST API
- **Testing**: Vitest + React Testing Library + Playwright

---

## Prerequisites

Before starting, ensure you have:

1. **Node.js 20 LTS**: [https://nodejs.org](https://nodejs.org)
2. **pnpm**: `npm install -g pnpm` (or use npm)
3. **Git**: For version control
4. **Windows 10+**: Required for testing Windows-specific features
5. **OLLAMA**: [https://ollama.ai](https://ollama.ai) - Install and start service
6. **VS Code** (recommended): With ESLint, Prettier, TypeScript extensions

---

## Initial Setup

### 1. Clone and Install

```bash
# Clone the repository
git clone https://github.com/your-org/etnopapers.git
cd etnopapers

# Install dependencies
pnpm install

# Verify installation
pnpm --version
node --version
```

### 2. Project Structure

```
etnopapers/
├── src/
│   ├── main/              # Electron main process
│   │   ├── index.ts       # Entry point
│   │   ├── ipc/           # IPC handlers
│   │   └── services/      # Business logic services
│   ├── renderer/          # React UI (renderer process)
│   │   ├── App.tsx        # Root component
│   │   ├── components/    # Reusable UI components
│   │   ├── pages/         # Page components
│   │   ├── stores/        # Zustand stores
│   │   └── services/      # Frontend service clients
│   ├── shared/            # Shared types and utilities
│   │   ├── types/         # TypeScript interfaces
│   │   └── utils/         # Helper functions
│   └── preload/           # Electron preload scripts
├── tests/
│   ├── unit/              # Unit tests
│   ├── integration/       # Integration tests
│   └── e2e/               # Playwright E2E tests
├── docs/                  # Project documentation
├── specs/                 # Feature specifications
└── resources/             # App icons, installers
```

### 3. Environment Configuration

Create `.env.local` file in root:

```env
# OLLAMA Configuration
VITE_OLLAMA_URL=http://localhost:11434
VITE_DEFAULT_MODEL=llama2

# Development Settings
VITE_LOG_LEVEL=debug
VITE_MAX_LOCAL_RECORDS=1000

# MongoDB (optional for development)
VITE_MONGODB_URI=mongodb://localhost:27017
```

### 4. Start Development

```bash
# Start Electron in development mode with hot reload
pnpm dev

# In separate terminal: Run tests in watch mode
pnpm test:watch
```

The app will open automatically with:
- Main window showing the UI
- DevTools open (F12)
- Hot reload enabled for both main and renderer

---

## Development Workflow

### Running Common Tasks

```bash
# Development
pnpm dev                    # Start Electron with hot reload
pnpm dev:renderer           # Start only renderer (for UI work)

# Testing
pnpm test                   # Run all tests once
pnpm test:watch             # Run tests in watch mode
pnpm test:unit              # Run only unit tests
pnpm test:e2e               # Run Playwright E2E tests
pnpm test:coverage          # Generate coverage report

# Linting & Formatting
pnpm lint                   # Check for lint errors
pnpm lint:fix               # Fix auto-fixable issues
pnpm format                 # Format code with Prettier

# Building
pnpm build                  # Build for production
pnpm dist                   # Create installer packages
pnpm dist:win               # Create Windows installer only

# Type Checking
pnpm type-check             # Run TypeScript compiler check
```

### Development Tips

1. **Main Process Changes**: Require full restart (Ctrl+C, then `pnpm dev`)
2. **Renderer Changes**: Hot reload automatically
3. **Shared Types**: Restart may be needed if types change
4. **Testing**: Write tests alongside features (TDD encouraged)

---

## Key Development Scenarios

### Scenario 1: Adding a New UI Component

**Goal**: Add a new reusable button component

```bash
# 1. Create component file
touch src/renderer/components/CustomButton.tsx

# 2. Create test file
touch tests/unit/components/CustomButton.test.tsx

# 3. Implement component with TypeScript
# src/renderer/components/CustomButton.tsx
```

```typescript
import React from 'react'

interface CustomButtonProps {
  label: string
  onClick: () => void
  variant?: 'primary' | 'secondary'
  disabled?: boolean
}

export const CustomButton: React.FC<CustomButtonProps> = ({
  label,
  onClick,
  variant = 'primary',
  disabled = false,
}) => {
  const baseClasses = 'px-4 py-2 rounded font-medium'
  const variantClasses = {
    primary: 'bg-blue-600 text-white hover:bg-blue-700',
    secondary: 'bg-gray-200 text-gray-800 hover:bg-gray-300',
  }

  return (
    <button
      onClick={onClick}
      disabled={disabled}
      className={`${baseClasses} ${variantClasses[variant]} ${
        disabled ? 'opacity-50 cursor-not-allowed' : ''
      }`}
    >
      {label}
    </button>
  )
}
```

```typescript
// tests/unit/components/CustomButton.test.tsx
import { render, screen, fireEvent } from '@testing-library/react'
import { describe, it, expect, vi } from 'vitest'
import { CustomButton } from '@/renderer/components/CustomButton'

describe('CustomButton', () => {
  it('renders with label', () => {
    render(<CustomButton label="Click Me" onClick={() => {}} />)
    expect(screen.getByText('Click Me')).toBeInTheDocument()
  })

  it('calls onClick when clicked', () => {
    const handleClick = vi.fn()
    render(<CustomButton label="Click Me" onClick={handleClick} />)

    fireEvent.click(screen.getByText('Click Me'))
    expect(handleClick).toHaveBeenCalledTimes(1)
  })

  it('disables button when disabled prop is true', () => {
    render(<CustomButton label="Click Me" onClick={() => {}} disabled />)
    expect(screen.getByText('Click Me')).toBeDisabled()
  })
})
```

---

### Scenario 2: Adding a Main Process Service

**Goal**: Create a new service for PDF processing

```bash
# 1. Define interface in shared types
touch src/shared/types/pdf-service.ts

# 2. Implement service in main process
touch src/main/services/PDFProcessingService.ts

# 3. Create tests
touch tests/unit/services/PDFProcessingService.test.ts
```

```typescript
// src/shared/types/pdf-service.ts
export interface IPDFProcessingService {
  extractText(filePath: string): Promise<string>
  getMetadata(filePath: string): Promise<PDFMetadata>
}

export interface PDFMetadata {
  pageCount: number
  title?: string
  author?: string
}
```

```typescript
// src/main/services/PDFProcessingService.ts
import * as pdfjsLib from 'pdfjs-dist'
import type { IPDFProcessingService, PDFMetadata } from '@/shared/types'

export class PDFProcessingService implements IPDFProcessingService {
  async extractText(filePath: string): Promise<string> {
    const loadingTask = pdfjsLib.getDocument(filePath)
    const pdf = await loadingTask.promise

    let fullText = ''
    for (let i = 1; i <= pdf.numPages; i++) {
      const page = await pdf.getPage(i)
      const content = await page.getTextContent()
      const pageText = content.items.map((item: any) => item.str).join(' ')
      fullText += pageText + '\n'
    }

    return fullText
  }

  async getMetadata(filePath: string): Promise<PDFMetadata> {
    const loadingTask = pdfjsLib.getDocument(filePath)
    const pdf = await loadingTask.promise
    const metadata = await pdf.getMetadata()

    return {
      pageCount: pdf.numPages,
      title: metadata.info?.Title,
      author: metadata.info?.Author,
    }
  }
}
```

```typescript
// tests/unit/services/PDFProcessingService.test.ts
import { describe, it, expect, beforeEach } from 'vitest'
import { PDFProcessingService } from '@/main/services/PDFProcessingService'
import path from 'path'

describe('PDFProcessingService', () => {
  let service: PDFProcessingService

  beforeEach(() => {
    service = new PDFProcessingService()
  })

  it('extracts text from valid PDF', async () => {
    const testPDF = path.join(__dirname, '../../fixtures/test-paper.pdf')
    const text = await service.extractText(testPDF)

    expect(text).toBeTruthy()
    expect(text.length).toBeGreaterThan(0)
  })

  it('gets metadata from PDF', async () => {
    const testPDF = path.join(__dirname, '../../fixtures/test-paper.pdf')
    const metadata = await service.getMetadata(testPDF)

    expect(metadata.pageCount).toBeGreaterThan(0)
  })
})
```

---

### Scenario 3: Adding IPC Communication

**Goal**: Expose a service to renderer via IPC

```typescript
// src/main/ipc/pdfHandlers.ts
import { ipcMain } from 'electron'
import { PDFProcessingService } from '../services/PDFProcessingService'

const pdfService = new PDFProcessingService()

export function registerPDFHandlers() {
  ipcMain.handle('pdf:extractText', async (event, filePath: string) => {
    try {
      return await pdfService.extractText(filePath)
    } catch (error) {
      console.error('PDF extraction failed:', error)
      throw error
    }
  })

  ipcMain.handle('pdf:getMetadata', async (event, filePath: string) => {
    return await pdfService.getMetadata(filePath)
  })
}
```

```typescript
// src/preload/index.ts
import { contextBridge, ipcRenderer } from 'electron'

contextBridge.exposeInMainWorld('pdfAPI', {
  extractText: (filePath: string) =>
    ipcRenderer.invoke('pdf:extractText', filePath),

  getMetadata: (filePath: string) =>
    ipcRenderer.invoke('pdf:getMetadata', filePath),
})
```

```typescript
// src/renderer/services/PDFServiceClient.ts
export class PDFServiceClient {
  async extractText(filePath: string): Promise<string> {
    return window.pdfAPI.extractText(filePath)
  }

  async getMetadata(filePath: string): Promise<PDFMetadata> {
    return window.pdfAPI.getMetadata(filePath)
  }
}
```

---

### Scenario 4: Adding State Management

**Goal**: Create a Zustand store for records

```typescript
// src/renderer/stores/useRecordsStore.ts
import create from 'zustand'
import type { ArticleRecord } from '@/shared/types'

interface RecordsState {
  records: ArticleRecord[]
  selectedIds: string[]
  isLoading: boolean
  error: string | null

  // Actions
  setRecords: (records: ArticleRecord[]) => void
  addRecord: (record: ArticleRecord) => void
  updateRecord: (id: string, updates: Partial<ArticleRecord>) => void
  deleteRecord: (id: string) => void
  toggleSelection: (id: string) => void
  clearSelection: () => void
  setLoading: (loading: boolean) => void
  setError: (error: string | null) => void
}

export const useRecordsStore = create<RecordsState>((set) => ({
  records: [],
  selectedIds: [],
  isLoading: false,
  error: null,

  setRecords: (records) => set({ records }),

  addRecord: (record) =>
    set((state) => ({ records: [...state.records, record] })),

  updateRecord: (id, updates) =>
    set((state) => ({
      records: state.records.map((r) =>
        r._id === id ? { ...r, ...updates, updatedAt: new Date() } : r
      ),
    })),

  deleteRecord: (id) =>
    set((state) => ({
      records: state.records.filter((r) => r._id !== id),
      selectedIds: state.selectedIds.filter((sid) => sid !== id),
    })),

  toggleSelection: (id) =>
    set((state) => ({
      selectedIds: state.selectedIds.includes(id)
        ? state.selectedIds.filter((sid) => sid !== id)
        : [...state.selectedIds, id],
    })),

  clearSelection: () => set({ selectedIds: [] }),

  setLoading: (isLoading) => set({ isLoading }),

  setError: (error) => set({ error }),
}))
```

```typescript
// Usage in component
import { useRecordsStore } from '@/renderer/stores/useRecordsStore'

function RecordsList() {
  const { records, isLoading, setRecords } = useRecordsStore()

  useEffect(() => {
    loadRecords()
  }, [])

  async function loadRecords() {
    const data = await window.storageAPI.getAll()
    setRecords(data)
  }

  if (isLoading) return <div>Loading...</div>

  return (
    <div>
      {records.map((record) => (
        <RecordCard key={record._id} record={record} />
      ))}
    </div>
  )
}
```

---

## Debugging

### Renderer Process (React UI)

1. Open DevTools: `Ctrl+Shift+I` or `F12`
2. Use React DevTools extension
3. Console logs appear in DevTools

### Main Process (Node.js)

1. Logs appear in terminal where you ran `pnpm dev`
2. Use VS Code debugger:
   - Set breakpoints in main process code
   - Run "Debug Main Process" launch configuration
3. Use `console.log()` for quick debugging

### IPC Communication

Add logging to IPC handlers:

```typescript
ipcMain.handle('my-channel', async (event, ...args) => {
  console.log('[IPC] my-channel called with:', args)
  const result = await myFunction(...args)
  console.log('[IPC] my-channel result:', result)
  return result
})
```

---

## Testing Strategy

### Unit Tests

Test individual functions and components in isolation:

```bash
pnpm test src/shared/utils/normalizeTitle.test.ts
```

### Integration Tests

Test service interactions:

```bash
pnpm test tests/integration/extraction-pipeline.test.ts
```

### E2E Tests

Test complete user workflows with Playwright:

```bash
pnpm test:e2e tests/e2e/upload-and-extract.spec.ts
```

---

## Common Issues & Solutions

### Issue: Electron app won't start

**Solution**:
```bash
# Clear node_modules and reinstall
rm -rf node_modules
pnpm install

# Rebuild native dependencies
pnpm rebuild
```

### Issue: Hot reload not working

**Solution**:
- Check if `vite.config.ts` has correct configuration
- Restart dev server
- Clear browser cache in DevTools

### Issue: TypeScript errors

**Solution**:
```bash
# Run type checker
pnpm type-check

# Update type definitions
pnpm install @types/node @types/react --save-dev
```

### Issue: OLLAMA connection fails

**Solution**:
1. Verify OLLAMA is running: `ollama list`
2. Check URL in config: `http://localhost:11434`
3. Test connection manually: `curl http://localhost:11434/api/tags`

---

## Next Steps

After setup, explore these areas:

1. **Read the specs**: `specs/main/` contains detailed specifications
2. **Review data model**: `specs/main/data-model.md`
3. **Check contracts**: `specs/main/contracts/service-interfaces.ts`
4. **Run tests**: Ensure all tests pass before making changes
5. **Pick a task**: Check `specs/main/tasks.md` (when available)

---

## Additional Resources

- [Electron Documentation](https://www.electronjs.org/docs/latest)
- [React Documentation](https://react.dev)
- [TypeScript Handbook](https://www.typescriptlang.org/docs/handbook/intro.html)
- [Zustand Documentation](https://github.com/pmndrs/zustand)
- [Tailwind CSS](https://tailwindcss.com/docs)
- [Vitest Documentation](https://vitest.dev)
- [Playwright Documentation](https://playwright.dev)

---

## Getting Help

- **Issues**: Create a GitHub issue for bugs or features
- **Questions**: Ask in team chat or discussions
- **Code Review**: Request review from team members

---

**Happy Coding!** 🚀
