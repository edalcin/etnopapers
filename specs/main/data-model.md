# Modelo de Dados: Etnopapers (Denormalizado)

**Funcionalidade**: Sistema de Extração de Metadados de Artigos Etnobotânicos
**Branch**: main
**Criado**: 2025-11-20
**Status**: Em Desenvolvimento
**Estrutura**: Mongita NoSQL (Denormalizado - Uma Coleção)

---

## 📋 Visão Geral

Este documento define o schema do banco de dados **Mongita** para armazenamento de metadados de artigos científicos sobre etnobotânica. O modelo utiliza uma abordagem **denormalizada** com uma única coleção (`referencias`), onde todos os dados de uma referência estão embarcados em um único documento.

### Por que denormalizado?

- ✅ **Queries simples**: Uma única `find()` retorna tudo (sem JOINs)
- ✅ **Schema flexível**: Novos campos podem ser adicionados sem migrações
- ✅ **Mapeamento direto**: A saída da IA mapeia diretamente para a estrutura do documento
- ✅ **Performance**: Sem overhead de relacionamentos
- ✅ **Sem redundância**: Um documento por referência = sem duplicação de dados

---

## 📄 Estrutura do Documento (Exemplo)

```json
{
  "_id": "507f1f77bcf86cd799439011",
  "ano": 2010,
  "titulo": "Uso e conhecimento tradicional de plantas medicinais no Sertão do Ribeirão, Florianópolis, SC, Brasil",
  "publicacao": "Acta bot. bras. 24(2): 395-406",
  "autores": [
    "Giraldi, M.",
    "Hanazaki, N."
  ],
  "resumo": "O objetivo desta pesquisa foi realizar um estudo etnobotânico sobre o uso e o conhecimento tradicional de plantas medicinais no Sertão do Ribeirão, uma comunidade de origem açoriana, inserida no domínio da Mata Atlântica...",
  "doi": "10.1590/S0102-33062011000200011",
  "especies": [
    {
      "vernacular": "maçanilha",
      "nomeCientifico": "Chamomilla recutita"
    },
    {
      "vernacular": "hortelã-branca",
      "nomeCientifico": "Mentha sp1."
    },
    {
      "vernacular": "gengibre",
      "nomeCientifico": "Zingiber officinale"
    }
  ],
  "tipo_de_uso": "medicinal",
  "metodologia": "entrevistas",
  "pais": "Brasil",
  "estado": "SC",
  "municipio": "Florianópolis",
  "local": "Sertão do Ribeirão",
  "bioma": "Mata Atlântica",
  "status": "rascunho"
}
```

---

## 🗄️ Coleção: `referencias`

**Descrição**: Coleção única que armazena referências científicas com todos os metadados embarcados.

### Campos

| Campo | Tipo | Obrigatório | Descrição |
|-------|------|-------------|-----------|
| `_id` | ObjectId | ✅ | ID único gerado automaticamente pelo Mongita |
| `ano` | Integer | ✅ | Ano de publicação (1900-2100) |
| `titulo` | String | ✅ | Título do artigo (máx 500 caracteres) |
| `publicacao` | String | ❌ | Venue de publicação (journal, conference) |
| `autores` | Array[String] | ✅ | Lista de nomes dos autores |
| `resumo` | String | ❌ | Abstract ou resumo do artigo |
| `doi` | String | ❌ | Digital Object Identifier (índice único) |
| `especies` | Array[Object] | ✅ | Espécies de plantas mencionadas |
| `especies[].vernacular` | String | ✅ | Nome vernacular/comum |
| `especies[].nomeCientifico` | String | ✅ | Nome científico da planta |
| `tipo_de_uso` | String | ❌ | Tipo de uso (medicinal, alimentar, ritual, etc.) |
| `metodologia` | String | ❌ | Metodologia de pesquisa (entrevistas, observação, etc.) |
| `pais` | String | ❌ | País onde estudo foi realizado |
| `estado` | String | ❌ | Estado ou província |
| `municipio` | String | ❌ | Município/cidade |
| `local` | String | ❌ | Localidade específica (comunidade, território) |
| `bioma` | String | ❌ | Bioma onde estudo foi realizado (Mata Atlântica, Cerrado, etc.) |
| `status` | String | ✅ | Status do documento ('rascunho' ou 'finalizado') |

---

## 📊 Índices

Para performance de queries comuns, os seguintes índices são criados automaticamente:

```python
# Índice único para prevenir duplicatas
db["referencias"].create_index("doi", unique=True)

# Índices para filtragem
db["referencias"].create_index("ano")        # Filtrar por ano
db["referencias"].create_index("status")     # Filtrar por status
db["referencias"].create_index("titulo")     # Busca por título
```

---

## 🔍 Exemplos de Queries

### 1. Obter uma referência por ID

```python
doc = db["referencias"].find_one({"_id": ObjectId("507f1f77bcf86cd799439011")})
```

**Resposta**:
```json
{
  "_id": "507f1f77bcf86cd799439011",
  "ano": 2010,
  "titulo": "Uso e conhecimento tradicional...",
  ...
}
```

### 2. Listar todas as referências de um ano específico

```python
refs_2010 = list(db["referencias"].find({"ano": 2010}).sort("titulo", 1))
```

### 3. Buscar referências com status "finalizado"

```python
finalizados = list(db["referencias"].find({"status": "finalizado"}))
print(f"Total finalizado: {len(finalizados)}")
```

### 4. Encontrar referências com uma espécie específica

```python
refs_with_chamomilla = list(
    db["referencias"].find({
        "especies.nomeCientifico": "Chamomilla recutita"
    })
)
```

### 5. Contar referências por país

```python
por_pais = list(
    db["referencias"].aggregate([
        {
            "$group": {
                "_id": "$pais",
                "count": {"$sum": 1}
            }
        },
        {"$sort": {"count": -1}}
    ])
)
# Resultado: [{"_id": "Brasil", "count": 45}, {"_id": "Colômbia", "count": 12}, ...]
```

### 6. Filtrar por múltiplos critérios

```python
# Referências brasileiras sobre plantas medicinais finalizadas em 2020
refs = list(
    db["referencias"].find({
        "pais": "Brasil",
        "tipo_de_uso": "medicinal",
        "ano": 2020,
        "status": "finalizado"
    })
)
```

### 7. Atualizar uma referência

```python
from datetime import datetime

result = db["referencias"].update_one(
    {"_id": ObjectId("507f1f77bcf86cd799439011")},
    {
        "$set": {
            "status": "finalizado",
            "bioma": "Cerrado"
        }
    }
)
print(f"Documentos atualizados: {result.modified_count}")
```

### 8. Deletar uma referência

```python
result = db["referencias"].delete_one({"_id": ObjectId("507f1f77bcf86cd799439011")})
print(f"Documentos deletados: {result.deleted_count}")
```

---

## 📈 Estimativas de Tamanho

**Formato**: Mongita usa BSON (binary encoding), mais compacto que JSON

| Quantidade | Tamanho Estimado | Descrição |
|------------|------------------|-----------|
| 100 referencias | 100-200 KB | Prototipagem |
| 1.000 referencias | 1-2 MB | Coleta pequena |
| 10.000 referencias | 10-20 MB | Banco médio |
| 100.000 referencias | 100-200 MB | Banco grande |

*Nota: Tamanho varia com comprimento de títulos, resumos e número de espécies por referência*

---

## 🔄 Detecção de Duplicatas

### Estratégia 1: Por DOI (Primária)

Se duas referências têm o mesmo DOI, são duplicatas:

```python
# DOI é índice único - tentativa de inserir duplicata falha
try:
    db["referencias"].insert_one({
        "doi": "10.1590/S0102-33062011000200011",  # Já existe
        ...
    })
except:
    print("Erro: DOI já existe")
```

### Estratégia 2: Por Título + Ano + Primeiro Autor

Se DOI não está disponível, detectar por combinação:

```python
existing = db["referencias"].find_one({
    "titulo": "Uso e conhecimento tradicional...",
    "ano": 2010,
    "autores": "Giraldi, M."  # Busca no array
})

if existing:
    print(f"Duplicata encontrada: {existing['_id']}")
```

---

## 🛠️ Operações Comuns na Aplicação

### Criar Nova Referência (do PDF extraído)

```python
new_ref = {
    "ano": 2010,
    "titulo": "...",
    "publicacao": "...",
    "autores": ["...", "..."],
    "resumo": "...",
    "doi": "...",
    "especies": [
        {"vernacular": "maçanilha", "nomeCientifico": "Chamomilla recutita"},
        ...
    ],
    "tipo_de_uso": "medicinal",
    "metodologia": "entrevistas",
    "pais": "Brasil",
    "estado": "SC",
    "municipio": "Florianópolis",
    "local": "Sertão do Ribeirão",
    "bioma": "Mata Atlântica",
    "status": "rascunho"
}

result = db["referencias"].insert_one(new_ref)
print(f"Criado com ID: {result.inserted_id}")
```

### Editar Referência Existente

```python
# Usuário corrigiu o status
db["referencias"].update_one(
    {"_id": ObjectId("507f1f77bcf86cd799439011")},
    {"$set": {"status": "finalizado"}}
)
```

### Listar com Paginação

```python
page = 1
page_size = 50
skip = (page - 1) * page_size

refs = list(
    db["referencias"]
        .find({})
        .sort("ano", -1)  # Mais recentes primeiro
        .skip(skip)
        .limit(page_size)
)

total = db["referencias"].count_documents({})
print(f"Página {page}/{(total + page_size - 1) // page_size}: {len(refs)} itens")
```

---

## 📝 Notas Importantes

1. **Sem Migrações**: Mongita é schema-less. Novos campos podem ser adicionados sem migração de dados
2. **Sem Normalização**: Dados sobre espécies (nome científico, família botânica) não são armazenados separadamente. Se 100 artigos mencionam a mesma espécie, o nome aparece 100 vezes
3. **Simplicidade**: Trade-off entre normalização e simplicidade - escolhemos simplicidade e flexibilidade
4. **MongoDB-Compatível**: Este schema é 100% compatível com MongoDB, permitindo migração futura para cloud

---

## 🚀 Próximas Evoluções

Se precisar de mais normalizador no futuro:

1. **Criar coleção `especies_plantas`**: Para taxonomia padronizada
2. **Criar coleção `comunidades`**: Para dados sobre comunidades indígenas
3. **Criar índices de texto**: Para busca full-text mais robusta
4. **Adicionar validações**: Constraints de integridade nos valores

Mas para o MVP, uma coleção é suficiente! ✨
