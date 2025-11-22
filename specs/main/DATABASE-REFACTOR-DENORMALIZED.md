# Refatoração do Banco de Dados: Modelo Denormalizado

**Data**: 22/11/2025
**Versão**: 2.0 (Denormalizado)
**Status**: ✅ Implementado

---

## 📌 Resumo Executivo

A arquitetura do banco de dados foi **refatorada de um modelo normalizado com múltiplas coleções para um modelo denormalizado com uma única coleção**. Esta mudança melhora significativamente a **simplicidade, flexibilidade e velocidade** do sistema.

### Antes vs Depois

| Aspecto | Antes (Normalizado) | Depois (Denormalizado) |
|---------|-------------------|----------------------|
| **Coleções** | 4 (referencias, especies_plantas, comunidades_indígenas, localizacoes) | 1 (referencias) |
| **Query Típica** | JOINs entre tabelas | find() único |
| **Adicionar campo** | Migração de schema | Sem alterações |
| **Tamanho DB** | Menor (deduplicado) | Ligeiramente maior (mas com menos overhead) |
| **Complexidade** | Alta | Baixa |
| **Extração IA** | Normalização manual | Mapeamento direto |

---

## 🎯 Por que essa mudança?

### Problema com Normalização Extrema

O modelo anterior (4 coleções) trazia:

1. **Complexidade**: Relacionamentos entre `referencias` → `especies` → `comunidades` → `localizacoes`
2. **JOINs em memória**: Mongita não suporta $lookup bem, forçava procesamento em Python
3. **Migrações**: Adicionar novo campo exigia atualização de múltiplas tabelas
4. **Mapeamento**: Saída da IA precisava de transformação complexa antes de salvar
5. **Overhead**: Múltiplas queries para obter informações de uma referência

### Solução: Denormalização

Todos os dados de uma referência em um **único documento**:

```json
{
  "_id": "ObjectId",
  "ano": 2010,
  "titulo": "...",
  "autores": ["Giraldi, M.", "Hanazaki, N."],
  "especies": [
    {"vernacular": "maçanilha", "nomeCientifico": "Chamomilla recutita"},
    ...
  ],
  "pais": "Brasil",
  "estado": "SC",
  "municipio": "Florianópolis",
  "local": "Sertão do Ribeirão",
  "bioma": "Mata Atlântica",
  "status": "rascunho"
}
```

---

## 🔄 Mudanças Implementadas

### 1. Backend Models (`backend/models/article.py`)

**Antes**:
```python
class AuthorInfo(BaseModel):
    nome: str
    sobrenome: Optional[str]
    email: Optional[str]

class ArticleRequest(BaseModel):
    titulo: str
    doi: Optional[str]
    ano_publicacao: int
    autores: List[AuthorInfo]
    # ... metadata_estudo, especies separadas, etc.
```

**Depois**:
```python
class SpeciesData(BaseModel):
    vernacular: str
    nomeCientifico: str

class ReferenceData(BaseModel):
    ano: int
    titulo: str
    autores: List[str]  # Simples lista de strings
    especies: List[SpeciesData]  # Embarcado
    pais: Optional[str]
    estado: Optional[str]
    municipio: Optional[str]
    local: Optional[str]
    bioma: Optional[str]
    # ... todos os campos aqui
```

### 2. Database Layer (`backend/database/connection.py`)

**Índices Simplificados**:

```python
# Antes (3 coleções com múltiplos índices)
db["referencias"].create_index("doi", unique=True)
db["referencias"].create_index("status")
db["referencias"].create_index("ano_publicacao")
db["referencias"].create_index([("especies.especie_id", 1)])  # Foreign key

db["especies_plantas"].create_index("nome_cientifico", unique=True)
db["especies_plantas"].create_index("familia_botanica")

# Depois (1 coleção, índices simples)
db["referencias"].create_index("doi", unique=True)
db["referencias"].create_index("ano")
db["referencias"].create_index("status")
db["referencias"].create_index("titulo")
```

### 3. Service Layer (`backend/services/article_service.py`)

**Método de Criação - Antes**:
```python
def create_article(
    titulo, ano_publicacao, autores,  # autores como [AuthorInfo]
    metadata_estudo, especies, comunidades, localizacoes,  # separados
) -> Dict:
    # Normalizar espécies
    especies_ids = []
    for sp in especies:
        existing = db["especies_plantas"].find_one({"nome_cientifico": sp.nome_cientifico})
        if not existing:
            result = db["especies_plantas"].insert_one(sp)
            sp_id = result.inserted_id
        else:
            sp_id = existing["_id"]
        especies_ids.append(sp_id)

    # Salvar referência com IDs
    doc = {..., "especies": especies_ids}
```

**Método de Criação - Depois**:
```python
def create_article(
    ano, titulo, autores,  # autores como [str]
    especies,  # [{"vernacular": "...", "nomeCientifico": "..."}]
    pais, estado, municipio, local, bioma,  # todos como fields simples
) -> Dict:
    doc = {
        "ano": ano,
        "titulo": titulo,
        "autores": autores,
        "especies": especies,  # Embarcado direto
        "pais": pais,
        "estado": estado,
        # ... tudo junto
    }
    result = db["referencias"].insert_one(doc)
```

### 4. API Routers (`backend/routers/articles.py`)

**Endpoint - Antes**:
```python
@router.post("/api/articles")
async def create_article(article: ArticleRequest):
    # Processar autores complexos
    authors = [f"{a.nome} {a.sobrenome}" for a in article.autores]

    # Normalizar dados estudos
    study_data = normalize_study_data(article.metadata_estudo)

    # Normalizar espécies
    species = normalize_species(article.especies)

    result = ArticleService.create_article(
        titulo=article.titulo,
        ano_publicacao=article.ano_publicacao,
        autores=authors,
        metadata_estudo=study_data,
        especies=species,
        # ... etc
    )
```

**Endpoint - Depois**:
```python
@router.post("/api/referencias")
async def create_reference(reference: ReferenceData):
    result = ArticleService.create_article(
        ano=reference.ano,
        titulo=reference.titulo,
        autores=reference.autores,  # Direto
        especies=reference.especies,  # Direto
        pais=reference.pais,
        estado=reference.estado,
        municipio=reference.municipio,
        local=reference.local,
        bioma=reference.bioma,
        # ... tudo é simples!
    )
```

---

## 📊 Comparação de Performance

### Query Simples: "Obter referência por ID"

**Antes** (com joins):
```python
# Query em 3 coleções
ref = db["referencias"].find_one({"_id": ref_id})  # 1 query
species = [
    db["especies_plantas"].find_one({"_id": sp_id})  # 5+ queries
    for sp_id in ref["especies"]
]
communities = [
    db["comunidades_indígenas"].find_one({"_id": com_id})  # 2+ queries
    for com_id in ref["comunidades"]
]
# Total: 8+ queries de BD + processamento em Python
```

**Depois** (sem joins):
```python
# Uma única query
ref = db["referencias"].find_one({"_id": ref_id})  # Pronto!
# Total: 1 query, zero processamento
```

### Query Complexa: "Encontrar referências com uma espécie"

**Antes**:
```python
species_id = db["especies_plantas"].find_one({"nome_cientifico": "Chamomilla recutita"})["_id"]
refs = list(db["referencias"].find({"especies": species_id}))
```

**Depois**:
```python
refs = list(db["referencias"].find({
    "especies.nomeCientifico": "Chamomilla recutita"
}))
```

---

## 🔀 Migração de Dados

Para usuários **com banco de dados existente** (não se aplica ao MVP):

```bash
# 1. Backup dos dados antigos
mongodump --db etnopapers --out backup_old_schema

# 2. Transformation script em Python
python migration/denormalize.py

# 3. Verify
python migration/verify.py
```

Para **novo deployment**: Automaticamente cria com novo schema!

---

## ✨ Benefícios Realizados

### 1. Simplicidade Máxima
- ❌ Sem JOINs
- ❌ Sem foreign keys
- ✅ Um documento = uma referência completa
- ✅ Fácil de entender e debugar

### 2. Extração Direta da IA
A IA extrai exatamente o JSON que precisa:

```python
# IA retorna
{
    "ano": 2010,
    "titulo": "Uso e conhecimento...",
    "autores": ["Giraldi, M.", "Hanazaki, N."],
    "especies": [
        {"vernacular": "maçanilha", "nomeCientifico": "Chamomilla recutita"}
    ],
    "pais": "Brasil",
    "estado": "SC"
    # ...
}

# Salva diretamente no BD!
db["referencias"].insert_one(ia_output)
```

### 3. Schema-less Evolution
Adicionar novo campo **sem migração**:

```python
# Código novo (tomorrow)
ref["plantas_ameacadas"] = ["Araucaria angustifolia"]
db["referencias"].update_one({"_id": ref_id}, {"$set": {"plantas_ameacadas": [...]}})

# Documentos antigos não precisam de alteração!
# Sistema funciona mesmo com documentos parciais
```

### 4. Performance
- ✅ Sem overhead de normalizações
- ✅ Índices simples e eficientes
- ✅ MongoDB-compatível (permite upgrade futuro para cloud)

### 5. Flexibilidade
Novos tipos de análise:

```python
# Estudos por bioma
db["referencias"].aggregate([
    {"$group": {"_id": "$bioma", "count": {"$sum": 1}}},
    {"$sort": {"count": -1}}
])

# Sem código extra - dados estão lá!
```

---

## 📝 Manutenção Futura

### Se precisar de Normalização:

**Fase 1**: Criar coleção de espécies
```python
# Extrair espécies únicas de todos os documentos
all_species = set()
for ref in db["referencias"].find({}, {"especies": 1}):
    for sp in ref["especies"]:
        all_species.add(sp["nomeCientifico"])

# Inserir em nova coleção
for species_name in all_species:
    db["especies_plantas"].insert_one({
        "nomeCientifico": species_name,
        "vernaculares": [],  # Preencher depois
        "validado": False
    })
```

**Fase 2**: Atualizar referências (opcional, se tiver limitações de espaço)
```python
# Trocar nomes por IDs se necessário
for ref in db["referencias"].find():
    new_species = []
    for sp in ref["especies"]:
        sp_doc = db["especies_plantas"].find_one({"nomeCientifico": sp["nomeCientifico"]})
        new_species.append({
            "vernacular": sp["vernacular"],
            "especie_id": sp_doc["_id"]
        })
    db["referencias"].update_one({"_id": ref._id}, {"$set": {"especies": new_species}})
```

---

## 🎓 Conclusão

A refatoração para denormalizado é a **abordagem certa para etnobotânica** porque:

1. **MVP é prioridade**: Lançar rapidamente com complexidade mínima
2. **Dados são embarcados naturalmente**: PDF → AI → BD (uma transformação linear)
3. **Escala é razoável**: 100K documentos = 100-200MB (sem problema)
4. **MongoDB-compatível**: Upgrade para cloud é trivial
5. **Schema-less**: Evoluir sem downtime

Para um próximo app com requisitos completamente diferentes (mil relacionamentos, complexas queries), poderia considerar normalização. Mas para etnobotânica, **denormalizado é perfeito**! 🌿
