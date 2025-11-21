# Modelo de Dados NoSQL: Etnopapers (Mongita/BSON)

**Funcionalidade**: Sistema de Extração de Metadados de Artigos Etnobotânicos (Versão NoSQL)
**Database**: Mongita (Embedded MongoDB-compatible)
**Formato**: BSON (Binary JSON) - mais compacto que JSON puro
**Criado**: 2025-11-21
**Status**: Especificação

## Visão Geral

O modelo foi refatorado de um design relacional normalizado (SQLite com 12 tabelas) para um design **document-centric** centrado em **referências** (artigos científicos). Esta mudança oferece:

- **Flexibilidade de Schema**: Novos campos de etnobotânica podem ser adicionados sem migrations
- **Agregação Natural**: Todos os dados relacionados a uma referência em um único documento
- **Query Eficiência**: Acesso a todos os metadados em uma única leitura
- **Escalabilidade Futura**: Compatível com MongoDB para migração para cloud

## Diagrama de Coleções (Document Relationships)

```
┌─────────────────────────────────────┐
│  referencias (Main Collection)      │
│  ├─ _id (ObjectId)                  │
│  ├─ titulo                          │
│  ├─ doi (unique index)              │
│  ├─ ano_publicacao                  │
│  ├─ autores[]                       │
│  ├─ resumo                          │
│  ├─ metadata_estudo {}              │  (embedded study data)
│  ├─ localizacoes[] (geog refs)      │
│  ├─ especies[] (plant refs)         │
│  ├─ comunidades[] (community refs)  │
│  ├─ usos_reportados[]               │  (aggregated uses)
│  ├─ status                          │
│  ├─ editado_manualmente             │
│  ├─ data_processamento              │
│  └─ data_ultima_modificacao         │
└───────────────┬──────────────────────┘
                │
                ├─────References to───────────────┐
                │                                  │
        ┌───────▼─────────────┐          ┌────────▼──────────────┐
        │ especies_plantas    │          │comunidades_indígenas │
        │ (separate coll.)    │          │  (separate coll.)    │
        ├─ _id (ObjectId)     │          ├─ _id (ObjectId)      │
        ├─ nome_cientifico(U) │          ├─ nome               │
        ├─ familia_botanica   │          ├─ tipo               │
        ├─ nome_aceito_atual  │          ├─ localizacao_ref{}  │
        ├─ autores_nome       │          ├─ descricao          │
        ├─ nomes_vernaculares │          └──────────────────────┘
        ├─ status_validacao   │
        ├─ fonte_validacao    │
        └─ data_validacao     │
                │              │
                └──────────────┘

Obs: Referencias document has arrays of nested references to:
- especies_plantas._id
- comunidades_indígenas._id
```

---

## Coleções Detalhadas

### 1. Coleção: `referencias`

**Descrição**: Documento raiz contendo todos os metadados de uma referência bibliográfica (artigo científico) e seus metadados etnobotânicos agregados.

**Estrutura BSON/JSON**:

```json
{
  "_id": ObjectId("507f1f77bcf86cd799439011"),

  // Identificação Bibliográfica
  "titulo": "Ethnobotanical Survey of Medicinal Plants used by Yanomami Community, Brazil",
  "doi": "10.1234/ethnobotany.2023.001",
  "ano_publicacao": 2023,
  "local_publicacao": "Journal of Ethnopharmacology",
  "autores": [
    "Silva, João",
    "Santos, Maria",
    "Oliveira, Pedro"
  ],
  "resumo": "This study documents traditional knowledge of medicinal plants...",

  // Metadados do Estudo (agregado do que era DadosEstudo)
  "metadata_estudo": {
    "periodo_inicio": "2021-06-15",
    "periodo_fim": "2022-12-31",
    "duracao_meses": 18,
    "metodos_coleta_dados": [
      "entrevistas_semi-estruturadas",
      "observacao_participante",
      "analise_documentos"
    ],
    "tipo_amostragem": "purposive",
    "tamanho_amostra": 45,
    "instrumentos_coleta": [
      "gravador_audio",
      "camera_digital",
      "caderno_campo"
    ]
  },

  // Localizações (referências ou embedded, dependendo da escala)
  "localizacoes": [
    {
      "pais": "Brasil",
      "estado": "Amazonas",
      "municipio": "Alto Alegre",
      "territorio": "Terra Indígena Yanomami",
      "coordenadas": {
        "latitude": -1.9234,
        "longitude": -62.5432
      }
    }
  ],

  // Espécies de Plantas (referências por ObjectId)
  "especies": [
    {
      "especie_id": ObjectId("507f191e810c19729de860ea"),  // ref para especies_plantas
      "nome_cientifico": "Psychotria viridis",
      "familia_botanica": "Rubiaceae",
      "nomes_vernaculares": [
        {
          "nome": "Chacruna",
          "idioma": "Quechua",
          "confianca": 0.95,
          "comunidade": "Yanomami"
        }
      ],
      "partes_utilizadas": [
        "folhas",
        "raizes"
      ],
      "modo_preparacao": [
        "decocção",
        "infusão"
      ],
      "indicacoes_uso": [
        "dores_reumaticas",
        "febre",
        "infeccoes_respiratorias"
      ],
      "preparacao_detalhada": "Fervem as folhas em agua por 30 minutos, bebi quente",
      "frequencia_uso": "ocasional",
      "eficacia_percebida": "alta",
      "efeitos_adversos": "nenhum relatado",
      "data_identificacao": "2021-07-10",
      "identificador": "Silva, João"
    },
    {
      "especie_id": ObjectId("507f191e810c19729de860eb"),
      "nome_cientifico": "Areca catechu",
      "familia_botanica": "Arecaceae",
      // ... demais campos
    }
  ],

  // Comunidades (referências por ObjectId)
  "comunidades": [
    {
      "comunidade_id": ObjectId("507f191e810c19729de860ec"),
      "nome": "Yanomami",
      "tipo": "indígena",
      "populacao_estimada": 25000,
      "linguas": [
        "Yanomami",
        "Português"
      ],
      "localizacao_territorio": "Terra Indígena Yanomami (AM/RR)"
    }
  ],

  // Usos Agregados (resumo de todos os usos reportados no artigo)
  "usos_reportados": [
    {
      "categoria": "medicinal",
      "quantidade_especies": 12,
      "indicacoes_principais": [
        "infeccoes",
        "febre",
        "dores"
      ]
    },
    {
      "categoria": "alimentar",
      "quantidade_especies": 5,
      "alimentos": [
        "açaí",
        "castanha_do_brasil"
      ]
    },
    {
      "categoria": "ritual_cultural",
      "quantidade_especies": 3,
      "descricao": "Plantas usadas em cerimônias e rituais"
    }
  ],

  // Status e Controle
  "status": "finalizado",  // "rascunho" | "finalizado"
  "editado_manualmente": false,
  "notas_editor": "Verificado e validado. Imagens de espécies incluídas.",

  // Auditoria
  "data_processamento": ISODate("2023-01-15T10:30:00Z"),
  "data_ultima_modificacao": ISODate("2023-01-20T14:45:00Z"),
  "usuario_processamento": "ai_gemini_3.5",  // qual IA processou
  "usuario_edicao": "maria_silva",  // quem editou manualmente (opcional)

  // Campos extensíveis (podem ser adicionados sem migrations!)
  "origem_do_artigo": "Web of Science",
  "keywords": ["Ethnobotany", "Indigenous knowledge", "Medicinal plants"],
  "indice_qualidade_extracao": 0.92,
  "marcadores_pesquisa": ["verificado", "alta_qualidade"],
  "fonte_pdf_url": null  // Se armazenar referência ao PDF original
}
```

**Índices para Performance**:
```
db['referencias'].create_index('doi', unique=True)
db['referencias'].create_index('status')
db['referencias'].create_index('ano_publicacao')
db['referencias'].create_index('data_processamento')
db['referencias'].create_index('especies.especie_id')
db['referencias'].create_index('comunidades.comunidade_id')
db['referencias'].create_index('localizacoes.pais')
db['referencias'].create_index('localizacoes.territorio')
```

**Validação de Schema** (Pydantic):
```python
from datetime import datetime
from typing import List, Optional, Dict, Any
from pydantic import BaseModel, Field, validator

class MetadadosEstudo(BaseModel):
    periodo_inicio: Optional[str] = None
    periodo_fim: Optional[str] = None
    duracao_meses: Optional[int] = None
    metodos_coleta_dados: List[str] = []
    tipo_amostragem: Optional[str] = None
    tamanho_amostra: Optional[int] = None
    instrumentos_coleta: List[str] = []

class Localizacao(BaseModel):
    pais: str
    estado: Optional[str] = None
    municipio: Optional[str] = None
    territorio: Optional[str] = None
    coordenadas: Optional[Dict[str, float]] = None

class NomeVernacular(BaseModel):
    nome: str
    idioma: Optional[str] = None
    confianca: float = 0.5
    comunidade: Optional[str] = None

class Especie(BaseModel):
    especie_id: str  # ObjectId as string
    nome_cientifico: str
    familia_botanica: Optional[str] = None
    nomes_vernaculares: List[NomeVernacular] = []
    partes_utilizadas: List[str] = []
    modo_preparacao: List[str] = []
    indicacoes_uso: List[str] = []
    preparacao_detalhada: Optional[str] = None
    frequencia_uso: Optional[str] = None
    eficacia_percebida: Optional[str] = None
    efeitos_adversos: Optional[str] = None
    data_identificacao: Optional[str] = None
    identificador: Optional[str] = None

class Comunidade(BaseModel):
    comunidade_id: str  # ObjectId as string
    nome: str
    tipo: str  # indígena|quilombola|ribeirinha|etc
    populacao_estimada: Optional[int] = None
    linguas: List[str] = []
    localizacao_territorio: Optional[str] = None

class Referencia(BaseModel):
    _id: Optional[str] = None  # ObjectId as string, auto-generated
    titulo: str
    doi: Optional[str] = None
    ano_publicacao: int
    local_publicacao: Optional[str] = None
    autores: List[str] = []
    resumo: Optional[str] = None
    metadata_estudo: MetadadosEstudo = MetadadosEstudo()
    localizacoes: List[Localizacao] = []
    especies: List[Especie] = []
    comunidades: List[Comunidade] = []
    usos_reportados: List[Dict[str, Any]] = []
    status: str = 'rascunho'  # rascunho|finalizado
    editado_manualmente: bool = False
    data_processamento: Optional[datetime] = None
    data_ultima_modificacao: Optional[datetime] = None
    usuario_processamento: Optional[str] = None
    usuario_edicao: Optional[str] = None
```

---

### 2. Coleção: `especies_plantas`

**Descrição**: Catálogo de espécies de plantas para deduplicação e reuso. Cada espécie documentada é armazenada uma única vez, referenciada por múltiplas referências.

**Estrutura BSON/JSON**:

```json
{
  "_id": ObjectId("507f191e810c19729de860ea"),

  // Identificação Científica
  "nome_cientifico": "Psychotria viridis",
  "familia_botanica": "Rubiaceae",
  "autores_nome": "Ruiz & Pavón",

  // Nomenclatura Aceita (resultado da validação GBIF)
  "nome_aceito_atual": "Psychotria viridis",
  "sinonimos": [
    "Psychotria alba",
    "Psychotria albida"
  ],

  // Validação Taxonômica
  "status_validacao": "validado",  // validado|nao_validado|sinonimo
  "fonte_validacao": "GBIF",  // GBIF|Tropicos|manual
  "data_validacao": ISODate("2023-01-10T08:00:00Z"),
  "gbif_key": 2684961,
  "tropicos_id": 14103471,

  // Nomes Vernaculares (suporta múltiplos idiomas e comunidades)
  "nomes_vernaculares": [
    {
      "nome": "Chacruna",
      "idioma": "Quechua",
      "pais": "Peru",
      "comunidades": ["Shipibo", "Quechua"],
      "confianca": 0.95,
      "fonte": "Schultes & Raffauf (1990)"
    },
    {
      "nome": "Rainha da Floresta",
      "idioma": "Português",
      "pais": "Brasil",
      "comunidades": ["Yanomami", "Tukano"],
      "confianca": 0.80,
      "fonte": "Luna & White (2000)"
    }
  ],

  // Propriedades Botânicas
  "distribuicao_geografica": [
    "Amazônia",
    "América Central",
    "Caribe"
  ],
  "habitat": [
    "floresta_tropical",
    "sub_bosque",
    "margens_rios"
  ],
  "altura_media": "2-5 metros",
  "tipo_folha": "oposta",
  "tipo_fruto": "baga",

  // Propriedades Químicas (compostos conhecidos)
  "compostos_ativos": [
    {
      "nome": "DMT",
      "nome_cientifico": "N,N-Dimethyltryptamine",
      "concentracao": "0.2-0.5%",
      "parte_planta": "folhas",
      "efeito": "psicoativo"
    },
    {
      "nome": "Alcaloides",
      "tipo": "Indólicos",
      "parte_planta": "folhas e raizes"
    }
  ],

  // Informações de Uso (agregado de todas as referências)
  "usos_reportados": [
    {
      "categoria": "medicinal",
      "indicacoes": [
        "febre",
        "infeccoes",
        "dores_reumaticas"
      ],
      "modo_preparacao": "decocção",
      "quantidade_referencias": 15,
      "comunidades_mais_relatam": ["Yanomami", "Tukano", "Shipibo"]
    },
    {
      "categoria": "ritual_chamânico",
      "descricao": "Componente de bebida psicoativa ritual",
      "comunidades": ["Shipibo", "Quechua", "Tukano"],
      "quantidade_referencias": 12
    }
  ],

  // Literatura e Referências
  "referencias_bibliograficas": [
    {
      "autor": "Schultes, R. E.",
      "titulo": "Ethnobotany of South American Natives",
      "ano": 1990,
      "doi": "10.1234/ethnobotany.1990"
    }
  ],

  // Controle e Auditoria
  "data_criacao": ISODate("2023-01-10T08:00:00Z"),
  "data_ultima_atualizacao": ISODate("2023-06-15T14:30:00Z"),
  "quantidade_referencias_usando": 28  // quantas referencias mencionam esta especie
}
```

**Índices**:
```
db['especies_plantas'].create_index('nome_cientifico', unique=True)
db['especies_plantas'].create_index('familia_botanica')
db['especies_plantas'].create_index('status_validacao')
db['especies_plantas'].create_index('nomes_vernaculares.nome')
```

---

### 3. Coleção: `comunidades_indígenas`

**Descrição**: Registro de comunidades tradicionais, povos indígenas e grupos étnico-culturais.

**Estrutura BSON/JSON**:

```json
{
  "_id": ObjectId("507f191e810c19729de860ec"),

  // Identificação
  "nome": "Yanomami",
  "tipo": "indígena",  // indígena|quilombola|ribeirinha|caiçara|seringueira|pantaneira|outro
  "nomes_alternativos": [
    "Yanomâmi",
    "Yanomamö"
  ],

  // Geografia
  "localizacao": {
    "pais": "Brasil",
    "estados": [
      "Amazonas",
      "Roraima"
    ],
    "municipios": [
      "Alto Alegre",
      "Iracema",
      "Boa Vista"
    ],
    "territorio_nome": "Terra Indígena Yanomami",
    "territorio_demarcado": true,
    "area_km2": 96650,
    "coordenadas_centro": {
      "latitude": -2.0123,
      "longitude": -63.5456
    }
  },

  // Demografia
  "populacao_estimada": 25000,
  "total_aldeias": 200,
  "lingua_principal": "Yanomami",
  "linguas_faladas": [
    "Yanomami",
    "Português",
    "Espanhol"
  ],

  // Contexto Cultural
  "periodo_contato": "primeiro_contato_1960",
  "organizacao_social": "descentralizado, autonomia aldeias",
  "conhecimento_botanico_relevancia": "alto",  // alto|medio|baixo
  "descricao_breve": "Povo indígena da Amazônia com vasto conhecimento de plantas medicinais",

  // Informações de Pesquisa
  "referencias_etnobotanicas": {
    "quantidade_referencias": 45,
    "principais_pesquisadores": [
      "Silva, João",
      "Santos, Maria"
    ],
    "periodo_pesquisa": "1980-presente"
  },

  // Conectar a Referências
  "referencias_ids": [
    ObjectId("507f1f77bcf86cd799439011"),
    ObjectId("507f1f77bcf86cd799439012")
  ],

  // Dados Culturais Sensíveis (com permissão)
  "dados_sensíveis_acordado": true,
  "instituicao_parceira": "FUNAI",
  "contato_comunidade": "Associação Yanomami do Brasil"
}
```

**Índices**:
```
db['comunidades_indígenas'].create_index('nome')
db['comunidades_indígenas'].create_index('tipo')
db['comunidades_indígenas'].create_index('localizacao.pais')
```

---

### 4. Coleção: `localizacoes` (opcional - para datasets grandes)

Se você tiver muitas referências com localizações, pode ser mais eficiente manter uma coleção separada e referenciar por ID em vez de embutir em cada referência.

**Estrutura BSON/JSON**:

```json
{
  "_id": ObjectId("507f191e810c19729de860ed"),
  "pais": "Brasil",
  "estado": "Amazonas",
  "municipio": "Alto Alegre",
  "territorio": "Terra Indígena Yanomami",
  "coordenadas": {
    "latitude": -1.9234,
    "longitude": -62.5432
  },
  "area_km2": 96650
}
```

---

## Comparação: SQLite (Anterior) vs. Mongita (Novo)

| Aspecto | SQLite (Anterior) | Mongita (Novo) |
|---------|---|---|
| **Tabelas/Coleções** | 12 tabelas normalizadas | 3-4 coleções agregadas |
| **Duplicação** | Mínima (normalização) | Ligeira (dados de referência agregados) |
| **Flexibilidade** | Rígida (schema fixo) | Flexible (schema evoluível) |
| **Query Simples** | Múltiplas JOINs | Uma query |
| **Tamanho em Disco** | JSON: ~1-2 MB/1000 ref | BSON: ~0.8-1 MB/1000 ref |
| **Velocidade Leitura** | Moderada | Rápida (sem JOINs) |
| **Adição de Campo** | Requer ALTER TABLE | Sem mudança necessária |
| **Compatibilidade Futura** | Nenhuma (SQLite específico) | MongoDB (cloud) |

---

## Exemplos de Queries Mongita (PyMongo API)

```python
from mongita import MongitaClientDisk

db = MongitaClientDisk(database_dir='./data/etnopapers').etnopapers

# 1. Encontrar todas as referências sobre um ano
refs = list(db['referencias'].find({'ano_publicacao': 2023}))

# 2. Encontrar referências com uma espécie específica
refs_com_especie = list(db['referencias'].find({
    'especies.nome_cientifico': 'Psychotria viridis'
}))

# 3. Encontrar por comunidade
refs_yanomami = list(db['referencias'].find({
    'comunidades.nome': 'Yanomami'
}))

# 4. Agregação: contar espécies por ano
pipeline = [
    {'$group': {
        '_id': '$ano_publicacao',
        'total_especies': {'$sum': {'$size': '$especies'}}
    }},
    {'$sort': {'_id': -1}}
]
resultado = list(db['referencias'].aggregate(pipeline))

# 5. Buscar por DOI
ref = db['referencias'].find_one({'doi': '10.1234/ethnobotany.2023.001'})

# 6. Atualizar um documento (rascunho para finalizado)
db['referencias'].update_one(
    {'_id': ObjectId('507f1f77bcf86cd799439011')},
    {'$set': {
        'status': 'finalizado',
        'data_ultima_modificacao': datetime.now()
    }}
)

# 7. Busca por texto (requer índice de texto)
# Para textos: implementar busca simples em titulos/resumos
refs = list(db['referencias'].find({
    'titulo': {'$regex': 'medicinal', '$options': 'i'}
}))
```

---

## Considerações de Performance

### Índices Recomendados

```python
# Índices para referências (críticos)
db['referencias'].create_index('doi', unique=True)
db['referencias'].create_index('status')
db['referencias'].create_index('ano_publicacao')
db['referencias'].create_index([('localizacoes.territorio', 1)])
db['referencias'].create_index([('comunidades.nome', 1)])

# Índices para especies
db['especies_plantas'].create_index('nome_cientifico', unique=True)
db['especies_plantas'].create_index('familia_botanica')

# Índices para comunidades
db['comunidades_indígenas'].create_index('nome')
db['comunidades_indígenas'].create_index('tipo')
```

### Tamanho de Disco Estimado

| Volume | SQLite | Mongita (BSON) |
|--------|--------|---|
| 1.000 referências | ~1-2 MB | ~0.8-1 MB |
| 10.000 referências | ~10-20 MB | ~8-10 MB |
| 100.000 referências | ~100-200 MB | ~80-100 MB |

BSON é ~15-20% mais compacto que JSON puro devido à codificação binária.

---

## Migração de Dados: SQLite → Mongita

Veja documento separado: `mongita-migration-guide.md`

---

## Extensibilidade: Adicionando Novos Campos

A maior vantagem do modelo NoSQL é que novos campos podem ser adicionados sem migrations:

**Exemplo 1**: Pesquisador quer rastrear "origem_do_artigo" (onde o PDF foi obtido)

```python
# Simples: apenas adicione o campo ao inserir/atualizar
db['referencias'].insert_one({
    'titulo': 'Nova Pesquisa',
    'origem_do_artigo': 'Web of Science',  # ← Novo campo
    # ... outros campos
})
```

**Exemplo 2**: Adicionar estrutura de "revisores_verificacao" com múltiplos revisores

```python
# Sem mudança de schema!
db['referencias'].update_one(
    {'_id': ObjectId('...')},
    {'$set': {
        'revisores_verificacao': [  # ← Nova estrutura
            {
                'nome': 'Maria Silva',
                'data': datetime.now(),
                'observacoes': 'Dados verificados e corretos',
                'status': 'aprovado'
            }
        ]
    }}
)
```

**Exemplo 3**: Pesquisador quer adicionar tags personalizadas

```python
# Simplesmente add o novo campo
db['referencias'].update_one(
    {'_id': ObjectId('...')},
    {'$set': {
        'tags_pesquisa': ['verificado', 'alta-qualidade', 'espécies-raras']
    }}
)
```

**Sem Mongita**: Isso exigiria:
1. `ALTER TABLE referencias ADD COLUMN origem_do_artigo TEXT`
2. Geração de migration com Alembic
3. Downtime potencial
4. Reversibilidade complexa

**Com Mongita**: Apenas escreva o código e deseche!

---

## Referências

- [Mongita GitHub](https://github.com/scottrogowski/mongita)
- [MongoDB Documentation](https://docs.mongodb.com/)
- [PyMongo Docs](https://pymongo.readthedocs.io/)
- [BSON Specification](https://bsonspec.org/)
