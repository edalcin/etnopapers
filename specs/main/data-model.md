# Modelo de Dados: Etnopapers

**Funcionalidade**: Sistema de Extração de Metadados de Artigos Etnobotânicos
**Branch**: main
**Criado**: 2025-11-20
**Status**: Em Desenvolvimento

## Visão Geral

Este documento define o schema completo do banco de dados SQLite para armazenamento de metadados de artigos científicos sobre etnobotânica. O modelo foi projetado para normalização adequada, evitando duplicação de dados (especialmente espécies de plantas) e permitindo consultas eficientes.

## Diagrama de Relacionamento de Entidades (ERD)

```
┌─────────────────────┐
│   ArtigosCientificos│
│  (Tabela Principal) │
└──────────┬──────────┘
           │
           │ 1:N
           ├──────────────────────┐
           │                      │
           │                      │
     ┌─────▼──────┐        ┌──────▼──────────┐
     │DadosEstudo │        │ArtigoRegiao     │
     │   (1:1)    │        │ (Associação)    │
     └────────────┘        └─────┬───────────┘
                                 │ N:1
                           ┌─────▼──────┐
                           │  Regioes   │
                           └─────┬──────┘
                                 │ 1:N
                      ┌──────────▼────────────┐
                      │RegiaoComunidade       │
                      │   (Associação)        │
                      └──────────┬────────────┘
                                 │ N:1
                           ┌─────▼──────────┐
                           │  Comunidades   │
                           └────────────────┘

┌──────────────────┐
│ArtigosCientificos│
└────────┬─────────┘
         │ N:M
    ┌────▼────────────┐
    │ArtigoEspecie    │
    │  (Associação)   │
    └────┬────────────┘
         │ N:1
    ┌────▼──────────┐
    │EspeciesPlantas│
    └───────────────┘
```

## Entidades e Tabelas

### 1. ArtigosCientificos

**Descrição**: Tabela principal que armazena informações bibliográficas e metadados dos artigos científicos processados.

**Schema SQL**:
```sql
CREATE TABLE ArtigosCientificos (
    -- Identificação
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    titulo TEXT NOT NULL,
    doi TEXT UNIQUE,  -- Pode ser NULL se artigo não tiver DOI

    -- Publicação
    ano_publicacao INTEGER NOT NULL,
    local_publicacao TEXT,  -- Nome do jornal/revista científica

    -- Autoria
    autores TEXT NOT NULL,  -- JSON array: ["Nome1", "Nome2", "Nome3"]

    -- Conteúdo
    resumo TEXT,  -- Abstract/resumo do artigo

    -- Status e Controle
    status TEXT NOT NULL DEFAULT 'rascunho',  -- 'rascunho' | 'finalizado'
    editado_manualmente BOOLEAN DEFAULT 0,  -- 1 se usuário editou metadados

    -- Auditoria
    data_processamento DATETIME DEFAULT CURRENT_TIMESTAMP,
    data_ultima_modificacao DATETIME DEFAULT CURRENT_TIMESTAMP,

    -- Validações
    CHECK (status IN ('rascunho', 'finalizado')),
    CHECK (ano_publicacao >= 1900 AND ano_publicacao <= 2100),
    CHECK (editado_manualmente IN (0, 1))
);

-- Índices para performance
CREATE INDEX idx_artigos_status ON ArtigosCientificos(status);
CREATE INDEX idx_artigos_ano ON ArtigosCientificos(ano_publicacao);
CREATE INDEX idx_artigos_doi ON ArtigosCientificos(doi);
CREATE INDEX idx_artigos_data_proc ON ArtigosCientificos(data_processamento);
```

**Campos**:
- `id`: Chave primária auto-incrementada
- `titulo`: Título completo do artigo (obrigatório)
- `doi`: Digital Object Identifier (único, opcional)
- `ano_publicacao`: Ano de publicação (1900-2100)
- `local_publicacao`: Nome da revista/jornal científico
- `autores`: JSON array com nomes dos autores (exemplo: `["Silva, J.", "Santos, M."]`)
- `resumo`: Abstract ou resumo do artigo
- `status`: Estado do registro (`rascunho` ou `finalizado`)
- `editado_manualmente`: Flag indicando se usuário modificou os metadados extraídos
- `data_processamento`: Timestamp de quando o PDF foi processado
- `data_ultima_modificacao`: Timestamp da última edição (atualizado via trigger)

**Regras de Negócio**:
- Artigos com status `rascunho` podem ser recuperados da seção "Rascunhos Pendentes"
- Rascunhos com mais de 7 dias podem ser excluídos automaticamente (limpeza agendada)
- DOI deve ser validado em formato (10.xxxx/xxxx) se fornecido
- Autores armazenados como JSON para permitir lista de tamanho variável

---

### 2. Regioes

**Descrição**: Representa regiões geográficas onde estudos etnobotânicos foram conduzidos.

**Schema SQL**:
```sql
CREATE TABLE Regioes (
    -- Identificação
    id INTEGER PRIMARY KEY AUTOINCREMENT,

    -- Localização
    descricao TEXT NOT NULL,  -- Descrição textual (ex: "Floresta Amazônica, Rio Negro")
    pais TEXT,
    estado_provincia TEXT,

    -- Coordenadas (opcional)
    latitude REAL,
    longitude REAL,

    -- Auditoria
    data_criacao DATETIME DEFAULT CURRENT_TIMESTAMP,

    -- Validações
    CHECK (latitude IS NULL OR (latitude >= -90 AND latitude <= 90)),
    CHECK (longitude IS NULL OR (longitude >= -180 AND longitude <= 180))
);

-- Índices
CREATE INDEX idx_regioes_pais ON Regioes(pais);
CREATE INDEX idx_regioes_estado ON Regioes(estado_provincia);
CREATE INDEX idx_regioes_coords ON Regioes(latitude, longitude);
```

**Campos**:
- `id`: Chave primária
- `descricao`: Descrição textual da região (obrigatório)
- `pais`: Nome do país
- `estado_provincia`: Estado, província ou divisão administrativa
- `latitude`, `longitude`: Coordenadas geográficas (opcional, extraído se disponível no artigo)
- `data_criacao`: Timestamp de criação do registro

**Regras de Negócio**:
- Regiões podem ser reutilizadas entre artigos (normalização)
- Coordenadas são opcionais e podem ser adicionadas manualmente
- Descrição deve ser única para evitar duplicatas sutis (implementado em nível de aplicação)

---

### 3. ArtigoRegiao

**Descrição**: Tabela de associação N:M entre Artigos e Regiões (um artigo pode ter múltiplas regiões de estudo).

**Schema SQL**:
```sql
CREATE TABLE ArtigoRegiao (
    artigo_id INTEGER NOT NULL,
    regiao_id INTEGER NOT NULL,

    PRIMARY KEY (artigo_id, regiao_id),
    FOREIGN KEY (artigo_id) REFERENCES ArtigosCientificos(id) ON DELETE CASCADE,
    FOREIGN KEY (regiao_id) REFERENCES Regioes(id) ON DELETE RESTRICT
);

-- Índices
CREATE INDEX idx_artigo_regiao_artigo ON ArtigoRegiao(artigo_id);
CREATE INDEX idx_artigo_regiao_regiao ON ArtigoRegiao(regiao_id);
```

**Regras de Negócio**:
- `ON DELETE CASCADE`: Se artigo for excluído, associações são removidas
- `ON DELETE RESTRICT`: Regiões não podem ser excluídas se estiverem associadas a artigos

---

### 4. Comunidades

**Descrição**: Representa comunidades tradicionais estudadas nos artigos (indígenas, quilombolas, ribeirinhas, etc.).

**Schema SQL**:
```sql
CREATE TABLE Comunidades (
    -- Identificação
    id INTEGER PRIMARY KEY AUTOINCREMENT,

    -- Identificação
    nome TEXT NOT NULL,  -- Nome ou descrição da comunidade
    tipo_comunidade TEXT,  -- 'indígena', 'quilombola', 'ribeirinha', 'caiçara', etc.

    -- Auditoria
    data_criacao DATETIME DEFAULT CURRENT_TIMESTAMP,

    -- Validações
    CHECK (tipo_comunidade IS NULL OR tipo_comunidade IN (
        'indígena', 'quilombola', 'ribeirinha', 'caiçara',
        'seringueira', 'pantaneira', 'outro'
    ))
);

-- Índices
CREATE INDEX idx_comunidades_tipo ON Comunidades(tipo_comunidade);
CREATE INDEX idx_comunidades_nome ON Comunidades(nome);
```

**Campos**:
- `id`: Chave primária
- `nome`: Nome ou descrição da comunidade (ex: "Comunidade Yanomami", "Quilombo Ivaporunduva")
- `tipo_comunidade`: Classificação do tipo de comunidade tradicional
- `data_criacao`: Timestamp de criação

**Regras de Negócio**:
- Comunidades são reutilizadas entre artigos
- Tipo de comunidade é controlado por enum (pode ser expandido conforme necessário)
- Nome deve ser único (implementado em nível de aplicação)

---

### 5. RegiaoComunidade

**Descrição**: Associação N:M entre Regiões e Comunidades (uma região pode ter múltiplas comunidades).

**Schema SQL**:
```sql
CREATE TABLE RegiaoComunidade (
    regiao_id INTEGER NOT NULL,
    comunidade_id INTEGER NOT NULL,

    PRIMARY KEY (regiao_id, comunidade_id),
    FOREIGN KEY (regiao_id) REFERENCES Regioes(id) ON DELETE CASCADE,
    FOREIGN KEY (comunidade_id) REFERENCES Comunidades(id) ON DELETE RESTRICT
);

-- Índices
CREATE INDEX idx_regiao_comunidade_regiao ON RegiaoComunidade(regiao_id);
CREATE INDEX idx_regiao_comunidade_comunidade ON RegiaoComunidade(comunidade_id);
```

**Regras de Negócio**:
- Permite modelar que uma comunidade está localizada em uma região específica
- Comunidades podem estar em múltiplas regiões (ex: comunidades nômades)

---

### 6. EspeciesPlantas

**Descrição**: Catálogo normalizado de espécies de plantas mencionadas nos artigos. Garante unicidade por nome científico.

**Schema SQL**:
```sql
CREATE TABLE EspeciesPlantas (
    -- Identificação
    id INTEGER PRIMARY KEY AUTOINCREMENT,

    -- Nomenclatura Científica
    nome_cientifico TEXT NOT NULL UNIQUE,  -- Binomial científico (Genus species)
    autores_nome_cientifico TEXT,  -- Autores do nome científico (ex: "L." para Linnaeus)
    familia_botanica TEXT,

    -- Validação Taxonômica
    nome_aceito_atual TEXT,  -- Nome científico aceito atual (pode diferir do extraído)
    sinonimo_de_id INTEGER,  -- Se for sinônimo, referencia espécie aceita
    status_validacao TEXT DEFAULT 'não validado',  -- 'validado' | 'não validado' | 'erro'
    fonte_validacao TEXT,  -- 'GBIF' | 'Tropicos' | 'manual'

    -- Nomes Populares
    nomes_vernaculares TEXT,  -- JSON array: ["nome1", "nome2", "nome3"]

    -- Usos Reportados
    usos_reportados TEXT,  -- JSON array: ["medicinal", "alimentício", "ritual"]

    -- Auditoria
    data_criacao DATETIME DEFAULT CURRENT_TIMESTAMP,
    data_ultima_validacao DATETIME,

    -- Validações
    CHECK (status_validacao IN ('validado', 'não validado', 'erro')),
    CHECK (fonte_validacao IS NULL OR fonte_validacao IN ('GBIF', 'Tropicos', 'manual')),
    FOREIGN KEY (sinonimo_de_id) REFERENCES EspeciesPlantas(id) ON DELETE SET NULL
);

-- Índices
CREATE UNIQUE INDEX idx_especies_nome_cientifico ON EspeciesPlantas(nome_cientifico);
CREATE INDEX idx_especies_familia ON EspeciesPlantas(familia_botanica);
CREATE INDEX idx_especies_status ON EspeciesPlantas(status_validacao);
CREATE INDEX idx_especies_sinonimo ON EspeciesPlantas(sinonimo_de_id);
```

**Campos**:
- `id`: Chave primária
- `nome_cientifico`: Nome binomial (Genus species) - **ÚNICO**
- `autores_nome_cientifico`: Autoria do nome (ex: "L." para Linnaeus)
- `familia_botanica`: Família botânica (ex: "Fabaceae")
- `nome_aceito_atual`: Nome científico válido atual conforme API de taxonomia
- `sinonimo_de_id`: Se este nome é sinônimo, aponta para espécie aceita
- `status_validacao`: Estado da validação taxonômica
- `fonte_validacao`: API usada para validação (GBIF, Tropicos, ou manual)
- `nomes_vernaculares`: JSON array com nomes populares (ex: `["ipê-roxo", "pau d'arco"]`)
- `usos_reportados`: JSON array com usos mencionados nos artigos
- `data_criacao`: Timestamp de criação
- `data_ultima_validacao`: Timestamp da última validação taxonômica

**Regras de Negócio**:
- **Unicidade por nome científico**: Garante que não há duplicatas de espécies
- Se API de taxonomia retornar que o nome extraído é sinônimo, criar registro apontando para espécie aceita
- Campo `usos_reportados` é agregado de todos os artigos que mencionam a espécie
- Validação taxonômica pode ser re-executada periodicamente (ex: anualmente) para atualizar nomenclatura

---

### 7. ArtigoEspecie

**Descrição**: Associação N:M entre Artigos e Espécies de Plantas. Um artigo menciona múltiplas espécies, e uma espécie aparece em múltiplos artigos.

**Schema SQL**:
```sql
CREATE TABLE ArtigoEspecie (
    artigo_id INTEGER NOT NULL,
    especie_id INTEGER NOT NULL,

    -- Contexto da Menção
    contexto_uso TEXT,  -- Como a espécie foi usada neste artigo específico
    parte_planta_utilizada TEXT,  -- JSON array: ["folha", "raiz", "casca"]

    PRIMARY KEY (artigo_id, especie_id),
    FOREIGN KEY (artigo_id) REFERENCES ArtigosCientificos(id) ON DELETE CASCADE,
    FOREIGN KEY (especie_id) REFERENCES EspeciesPlantas(id) ON DELETE RESTRICT
);

-- Índices
CREATE INDEX idx_artigo_especie_artigo ON ArtigoEspecie(artigo_id);
CREATE INDEX idx_artigo_especie_especie ON ArtigoEspecie(especie_id);
```

**Campos**:
- `artigo_id`, `especie_id`: Chaves estrangeiras (chave primária composta)
- `contexto_uso`: Descrição de como a espécie foi usada (ex: "medicinal para febre")
- `parte_planta_utilizada`: JSON array com partes da planta usadas (ex: `["folha", "casca"]`)

**Regras de Negócio**:
- Permite que uma espécie seja mencionada em múltiplos artigos
- Contexto de uso é específico do artigo (uma mesma planta pode ter usos diferentes em comunidades distintas)

---

### 8. DadosEstudo

**Descrição**: Informações metodológicas sobre o estudo conduzido no artigo. Relação 1:1 com ArtigosCientificos.

**Schema SQL**:
```sql
CREATE TABLE DadosEstudo (
    -- Relacionamento
    artigo_id INTEGER PRIMARY KEY,  -- 1:1 com ArtigosCientificos

    -- Período do Estudo
    periodo_inicio DATE,  -- Data de início da coleta de dados
    periodo_fim DATE,  -- Data de término da coleta de dados
    duracao_meses INTEGER,  -- Duração em meses (calculado ou extraído)

    -- Metodologia
    metodos_coleta_dados TEXT,  -- Descrição dos métodos (ex: "Entrevistas semi-estruturadas")
    tipo_amostragem TEXT,  -- 'probabilística', 'não-probabilística', 'snowball', etc.
    tamanho_amostra INTEGER,  -- Número de participantes/informantes

    -- Instrumentos
    instrumentos_coleta TEXT,  -- JSON array: ["questionário", "entrevista", "observação participante"]

    -- Auditoria
    data_criacao DATETIME DEFAULT CURRENT_TIMESTAMP,

    -- Validações
    CHECK (periodo_fim IS NULL OR periodo_inicio IS NULL OR periodo_fim >= periodo_inicio),
    CHECK (duracao_meses IS NULL OR duracao_meses > 0),
    CHECK (tamanho_amostra IS NULL OR tamanho_amostra > 0),
    FOREIGN KEY (artigo_id) REFERENCES ArtigosCientificos(id) ON DELETE CASCADE
);

-- Índices
CREATE INDEX idx_dados_estudo_periodo ON DadosEstudo(periodo_inicio, periodo_fim);
CREATE INDEX idx_dados_estudo_amostragem ON DadosEstudo(tipo_amostragem);
```

**Campos**:
- `artigo_id`: Chave primária e estrangeira (1:1 com ArtigosCientificos)
- `periodo_inicio`, `periodo_fim`: Datas de início e fim da coleta
- `duracao_meses`: Duração do estudo em meses
- `metodos_coleta_dados`: Descrição textual dos métodos
- `tipo_amostragem`: Tipo de amostragem utilizada
- `tamanho_amostra`: Número de participantes/informantes
- `instrumentos_coleta`: JSON array com instrumentos utilizados
- `data_criacao`: Timestamp de criação

**Regras de Negócio**:
- Relação 1:1 com ArtigosCientificos (um artigo tem um conjunto de dados de estudo)
- Campos de data são opcionais (nem todos artigos reportam datas exatas)
- Se `periodo_inicio` e `periodo_fim` estiverem disponíveis, calcular `duracao_meses` automaticamente

---

## Triggers e Automações

### Trigger: Atualizar data_ultima_modificacao

```sql
CREATE TRIGGER atualizar_modificacao_artigo
AFTER UPDATE ON ArtigosCientificos
FOR EACH ROW
BEGIN
    UPDATE ArtigosCientificos
    SET data_ultima_modificacao = CURRENT_TIMESTAMP
    WHERE id = NEW.id;
END;
```

### Trigger: Marcar editado_manualmente

```sql
CREATE TRIGGER marcar_edicao_manual
AFTER UPDATE ON ArtigosCientificos
FOR EACH ROW
WHEN OLD.titulo != NEW.titulo
   OR OLD.autores != NEW.autores
   OR OLD.resumo != NEW.resumo
   OR OLD.ano_publicacao != NEW.ano_publicacao
BEGIN
    UPDATE ArtigosCientificos
    SET editado_manualmente = 1
    WHERE id = NEW.id;
END;
```

### View: Artigos Completos (com relacionamentos)

```sql
CREATE VIEW vw_artigos_completos AS
SELECT
    a.id,
    a.titulo,
    a.doi,
    a.ano_publicacao,
    a.autores,
    a.local_publicacao,
    a.resumo,
    a.status,
    a.editado_manualmente,
    a.data_processamento,

    -- Dados do Estudo
    de.metodos_coleta_dados,
    de.tipo_amostragem,
    de.tamanho_amostra,

    -- Agregações
    GROUP_CONCAT(DISTINCT r.descricao) AS regioes,
    GROUP_CONCAT(DISTINCT c.nome) AS comunidades,
    COUNT(DISTINCT ae.especie_id) AS total_especies

FROM ArtigosCientificos a
LEFT JOIN DadosEstudo de ON a.id = de.artigo_id
LEFT JOIN ArtigoRegiao ar ON a.id = ar.artigo_id
LEFT JOIN Regioes r ON ar.regiao_id = r.id
LEFT JOIN RegiaoComunidade rc ON r.id = rc.regiao_id
LEFT JOIN Comunidades c ON rc.comunidade_id = c.id
LEFT JOIN ArtigoEspecie ae ON a.id = ae.artigo_id
GROUP BY a.id;
```

---

## Queries de Exemplo

### Inserir novo artigo completo

```sql
-- 1. Inserir artigo
INSERT INTO ArtigosCientificos (titulo, doi, ano_publicacao, autores, resumo, local_publicacao, status)
VALUES (
    'Etnobotânica da comunidade ribeirinha do Rio Negro',
    '10.1234/exemplo.2023',
    2023,
    '["Silva, A.B.", "Santos, C.D."]',
    'Estudo sobre uso de plantas medicinais...',
    'Brazilian Journal of Ethnobiology',
    'finalizado'
);

-- 2. Inserir dados do estudo
INSERT INTO DadosEstudo (artigo_id, periodo_inicio, periodo_fim, metodos_coleta_dados, tipo_amostragem, tamanho_amostra)
VALUES (
    last_insert_rowid(),  -- ID do artigo recém-inserido
    '2022-01-15',
    '2022-06-30',
    'Entrevistas semi-estruturadas e observação participante',
    'não-probabilística',
    45
);

-- 3. Inserir ou reutilizar região
INSERT OR IGNORE INTO Regioes (descricao, pais, estado_provincia)
VALUES ('Rio Negro, Amazonas', 'Brasil', 'Amazonas');

INSERT INTO ArtigoRegiao (artigo_id, regiao_id)
VALUES (
    last_insert_rowid(),  -- ID do artigo
    (SELECT id FROM Regioes WHERE descricao = 'Rio Negro, Amazonas')
);

-- 4. Inserir ou reutilizar espécie
INSERT OR IGNORE INTO EspeciesPlantas (nome_cientifico, familia_botanica, status_validacao)
VALUES ('Uncaria tomentosa', 'Rubiaceae', 'não validado');

INSERT INTO ArtigoEspecie (artigo_id, especie_id, contexto_uso, parte_planta_utilizada)
VALUES (
    last_insert_rowid(),  -- ID do artigo
    (SELECT id FROM EspeciesPlantas WHERE nome_cientifico = 'Uncaria tomentosa'),
    'Medicinal para inflamações',
    '["casca", "raiz"]'
);
```

### Consultar artigos por espécie

```sql
SELECT
    a.titulo,
    a.ano_publicacao,
    ae.contexto_uso,
    ae.parte_planta_utilizada
FROM ArtigosCientificos a
JOIN ArtigoEspecie ae ON a.id = ae.artigo_id
JOIN EspeciesPlantas e ON ae.especie_id = e.id
WHERE e.nome_cientifico = 'Uncaria tomentosa'
ORDER BY a.ano_publicacao DESC;
```

### Listar espécies mais estudadas

```sql
SELECT
    e.nome_cientifico,
    e.familia_botanica,
    e.nomes_vernaculares,
    COUNT(ae.artigo_id) AS total_artigos
FROM EspeciesPlantas e
JOIN ArtigoEspecie ae ON e.id = ae.especie_id
GROUP BY e.id
ORDER BY total_artigos DESC
LIMIT 10;
```

### Buscar rascunhos antigos (limpeza automática)

```sql
SELECT id, titulo, data_processamento
FROM ArtigosCientificos
WHERE status = 'rascunho'
  AND data_processamento < datetime('now', '-7 days')
ORDER BY data_processamento ASC;
```

### Validar integridade referencial

```sql
-- Verificar artigos sem dados de estudo
SELECT a.id, a.titulo
FROM ArtigosCientificos a
LEFT JOIN DadosEstudo de ON a.id = de.artigo_id
WHERE de.artigo_id IS NULL;

-- Verificar espécies não validadas
SELECT nome_cientifico, data_criacao
FROM EspeciesPlantas
WHERE status_validacao = 'não validado'
ORDER BY data_criacao DESC;
```

---

## Migração e Versionamento

### Estratégia de Migrações

Utilizar **Alembic** (biblioteca Python) para versionamento de schema:

```python
# alembic/versions/001_initial_schema.py
def upgrade():
    op.execute("""
        CREATE TABLE ArtigosCientificos (
            -- schema completo aqui
        );
    """)
    # ... outras tabelas

def downgrade():
    op.execute("DROP TABLE IF EXISTS ArtigosCientificos;")
    # ... outras tabelas
```

### Versão Inicial

**Versão**: 1.0.0
**Data**: 2025-11-20
**Descrição**: Schema inicial com 8 tabelas + triggers + views

---

## Considerações de Performance

### Índices Criados

**Total de índices**: 18 índices criados para otimizar queries comuns:
- Busca por status de artigos
- Filtros por ano de publicação
- Lookup de espécies por nome científico
- Queries de associação N:M

### Estimativas de Tamanho

Para **1.000 artigos** com média de:
- 5 espécies por artigo
- 2 regiões por artigo
- 3 comunidades por região

**Tamanho estimado**:
- ArtigosCientificos: ~500 KB
- EspeciesPlantas: ~100 KB (assumindo 200 espécies únicas)
- Regioes: ~30 KB (assumindo 50 regiões únicas)
- Comunidades: ~20 KB (assumindo 30 comunidades únicas)
- Tabelas de associação: ~150 KB
- **Total**: ~800 KB para 1.000 artigos

Para **10.000 artigos**: ~8 MB
Para **100.000 artigos**: ~80 MB

SQLite performa bem até centenas de milhares de registros com os índices adequados.

---

## Backup e Exportação

### Backup do Banco de Dados

```bash
# Backup completo
sqlite3 /data/etnopapers.db ".backup /backup/etnopapers_$(date +%Y%m%d).db"

# Exportar para SQL
sqlite3 /data/etnopapers.db .dump > etnopapers_backup.sql
```

### Exportar para CSV

```sql
.headers on
.mode csv
.output artigos_export.csv
SELECT * FROM vw_artigos_completos;
.output stdout
```

---

## Próximos Passos

1. Implementar scripts de migração Alembic
2. Criar seeds de dados de teste
3. Implementar queries no backend FastAPI usando SQLAlchemy ORM
4. Adicionar testes de integridade referencial
5. Documentar procedures de backup no quickstart.md

## Referências

- [SQLite Documentation](https://www.sqlite.org/docs.html)
- [SQLite Foreign Key Support](https://www.sqlite.org/foreignkeys.html)
- [SQLite Triggers](https://www.sqlite.org/lang_createtrigger.html)
- [Alembic Documentation](https://alembic.sqlalchemy.org/)
