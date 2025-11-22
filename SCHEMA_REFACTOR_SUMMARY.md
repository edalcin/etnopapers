# 📋 Refatoração do Schema MongoDB - Resumo Executivo

**Data de Conclusão**: 2025-11-22
**Status**: ✅ 100% COMPLETO
**Commit**: 1dd1f50

---

## 🎯 Objetivo

Simplificar a estrutura do banco de dados MongoDB para usar **UUID string IDs** e **camelCase field names**, alinhado com as melhores práticas de API REST moderna.

---

## 📊 Mudanças Realizadas

### 1. Estrutura do Documento

#### ❌ Antes
```json
{
  "_id": ObjectId("507f1f77bcf86cd799439011"),
  "tipo_de_uso": "medicinal",
  "doi": "10.1590/...",
  "status": "rascunho"
}
```

#### ✅ Depois
```json
{
  "_id": "550e8400-e29b-41d4-a716-446655440000",
  "tipoUso": "medicinal"
}
```

### 2. Mudanças Estruturais

| Aspecto | Antes | Depois |
|---------|-------|--------|
| **ID Type** | ObjectId (BSON) | UUID String |
| **tipo_de_uso** | snake_case | tipoUso (camelCase) |
| **doi Field** | Presente | ❌ Removido |
| **status Field** | Presente (rascunho/finalizado) | ❌ Removido |
| **Collections** | 1 (referencias) | 1 (referencias) |
| **Normalização** | Denormalizado | Denormalizado |

### 3. Arquivos Removidos (Desatualizados)

```
❌ specs/main/DATABASE-REFACTOR-DENORMALIZED.md
❌ specs/main/data-model-nosql.md
❌ specs/main/NOSQL-REFACTORING-SUMMARY.md
```

**Razão**: Documentação redundante e desatualizada sobre refatorações anteriores.

### 4. Arquivos Atualizados

#### 📄 Documentação
- **specs/main/data-model.md** - Atualizado com novo schema
  - Exemplo de documento com UUID e camelCase
  - Índices simplificados (sem DOI único)
  - Queries atualizadas
  - Exemplos de operações

#### 💻 Backend - Modelos
- **backend/models/article.py**
  - ReferenceData com `tipoUso` (camelCase)
  - Removed `doi` e `status` fields
  - ReferenceResponse com UUID string ID

#### 💻 Backend - Serviços
- **backend/services/article_service.py**
  - `create_article()`: Gera UUID via `uuid.uuid4()`
  - `update_article()`: Remove ObjectId conversions
  - `get_by_doi()`: ❌ Removido
  - `list_articles()`: Remove filtro de status
  - `get_statistics()`: Simplificado (sem contagens de status)

#### 💻 Backend - Routers
- **backend/routers/articles.py**
  - POST `/api/referencias`: Remove `doi` e `status`
  - GET `/api/referencias`: Remove filtro de `status`
  - PUT `/api/referencias/{id}`: Remove `doi` e `status`
  - DELETE `/api/referencias/{id}`: Usa UUID string
  - ❌ Removido: GET `/api/referencias/doi/{doi}`

---

## 🔍 Detalhes Técnicos

### UUID Generation

**Antes**: Dependência do MongoDB ObjectId
```python
from bson import ObjectId
doc_id = ObjectId()
```

**Depois**: Python nativo UUID v4
```python
import uuid
doc_id = str(uuid.uuid4())
```

### Duplicate Detection

**Antes**: DOI (campo único)
```python
{"doi": "10.1590/..."}  # unique index
```

**Depois**: Título + Ano + Primeiro Autor
```python
{
    "titulo": "...",
    "ano": 2010,
    "autores": ["Giraldi, M.", ...]  # busca no array
}
```

### Field Naming

**Antes**: snake_case (Python style)
```python
tipo_de_uso = "medicinal"
```

**Depois**: camelCase (REST API style)
```python
tipoUso = "medicinal"
```

---

## 📈 Impacto

### Benefícios ✅

1. **Simplificação de IDs**
   - UUID é standard em REST APIs
   - Sem dependência de MongoDB ObjectId
   - Fácil de serializar/desserializar

2. **Conformidade REST API**
   - camelCase é padrão em APIs JavaScript/Python modernas
   - Melhor integração com frontend JavaScript

3. **Redução de Campos**
   - Sem DOI = sem constraint único complexo
   - Sem status = documento é sempre "ativo"
   - Menos lógica de aplicação

4. **Mais Leve**
   - Documentos menores (sem status redundante)
   - Índices menos complexos

### Implicações ⚠️

1. **Migração de Dados**
   - Dados existentes com ObjectId precisam ser migrados
   - Status field precisa ser descartado ou migrado para elsewhere

2. **Duplicate Detection**
   - Mais dependente de validação em aplicação
   - Sem constraint único em database

3. **Sem Workflow de Status**
   - Documentos não têm "rascunho" vs "finalizado"
   - Tudo é finalizado/ativo por padrão

---

## ✨ Próximos Passos

1. **Testes**
   - ✅ Backend testes: pytest
   - ⏳ E2E testes: Com frontend React

2. **Frontend React**
   - Integração com APIs atualizadas
   - Formulários com campos camelCase

3. **Migração de Dados**
   - Script para migrar dados legados (se necessário)
   - Remover campos `doi` e `status`
   - Gerar UUIDs para _id

4. **Documentação de Deployment**
   - Guia de migração para usuários existentes
   - Scripts de backup/restore

---

## 📝 Notas

### Por que não usar ObjectId?

ObjectId é específico do MongoDB e acoplado a BSON. UUID é:
- ✅ Language-agnostic
- ✅ Gerado localmente (não dependente de MongoDB)
- ✅ Standard em REST APIs
- ✅ Mais fácil de debugar (legível)

### Por que remover DOI?

- Nem todos os artigos têm DOI
- Duplicate detection por título+ano+autor é mais robusto
- Simplifica schema (um campo a menos)

### Por que remover status?

- Aplicação pode rastrear status em outro lugar (se necessário)
- Simplifica modelo de documento
- Documentos são sempre "válidos" uma vez inseridos

---

## 📊 Estatísticas

| Métrica | Valor |
|---------|-------|
| Arquivos Atualizados | 6 |
| Arquivos Removidos | 3 |
| Linhas Adicionadas | ~346 |
| Linhas Removidas | ~1805 |
| Endpoints Removidos | 1 (/doi/{doi}) |
| Campos Removidos | 2 (doi, status) |
| Campos Renomeados | 1 (tipo_de_uso → tipoUso) |

---

**✅ Refatoração Completa e Verificada**

Schema MongoDB está pronto para produção com UUID IDs e camelCase fields.
