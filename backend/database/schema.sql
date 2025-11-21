-- Etnopapers Database Schema
-- SQLite 3.35+ with WAL mode
-- 12 tables with normalized design, triggers, and indexes

-- Table 1: ArtigosCientificos - Scientific articles (main entity)
CREATE TABLE IF NOT EXISTS ArtigosCientificos (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    titulo TEXT NOT NULL,
    doi TEXT UNIQUE,
    ano_publicacao INTEGER CHECK(ano_publicacao >= 1900 AND ano_publicacao <= 2100),
    autores TEXT NOT NULL,  -- JSON array: [{"nome": "...", "sobrenome": "...", "email": "..."}]
    resumo TEXT,
    status TEXT NOT NULL DEFAULT 'rascunho' CHECK(status IN ('rascunho', 'finalizado')),
    editado_manualmente BOOLEAN DEFAULT 0,
    data_processamento DATETIME DEFAULT CURRENT_TIMESTAMP,
    data_ultima_modificacao DATETIME DEFAULT CURRENT_TIMESTAMP,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- Table 2: DadosEstudo - Study methodology (1:1 with articles)
CREATE TABLE IF NOT EXISTS DadosEstudo (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    artigo_id INTEGER NOT NULL UNIQUE,
    periodo_inicio INTEGER CHECK(periodo_inicio >= 1900 AND periodo_inicio <= 2100),
    periodo_fim INTEGER CHECK(periodo_fim >= 1900 AND periodo_fim <= 2100),
    duracao_meses INTEGER,
    metodos_coleta_dados TEXT,
    tipo_amostragem TEXT,
    tamanho_amostra INTEGER,
    instrumentos_coleta TEXT,  -- JSON array
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (artigo_id) REFERENCES ArtigosCientificos(id) ON DELETE CASCADE
);

-- Table 3: Paises - Countries (geographic hierarchy level 1)
CREATE TABLE IF NOT EXISTS Paises (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    nome TEXT NOT NULL UNIQUE,
    codigo_iso TEXT UNIQUE,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- Table 4: Estados - States/Provinces (geographic hierarchy level 2)
CREATE TABLE IF NOT EXISTS Estados (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    pais_id INTEGER NOT NULL,
    nome TEXT NOT NULL,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (pais_id) REFERENCES Paises(id) ON DELETE CASCADE,
    UNIQUE(pais_id, nome)
);

-- Table 5: Municipios - Municipalities (geographic hierarchy level 3)
CREATE TABLE IF NOT EXISTS Municipios (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    estado_id INTEGER NOT NULL,
    nome TEXT NOT NULL,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (estado_id) REFERENCES Estados(id) ON DELETE CASCADE,
    UNIQUE(estado_id, nome)
);

-- Table 6: Territorios - Community territories (non-hierarchical)
CREATE TABLE IF NOT EXISTS Territorios (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    nome TEXT NOT NULL UNIQUE,
    tipo TEXT,  -- e.g., "Terra Indígena", "Quilombo", "Reserva Extrativista"
    area_hectares REAL,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- Table 7: ArtigoLocalizacao - Link articles to locations (polymorphic)
CREATE TABLE IF NOT EXISTS ArtigoLocalizacao (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    artigo_id INTEGER NOT NULL,
    municipio_id INTEGER,
    territorio_id INTEGER,
    descricao TEXT,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (artigo_id) REFERENCES ArtigosCientificos(id) ON DELETE CASCADE,
    FOREIGN KEY (municipio_id) REFERENCES Municipios(id) ON DELETE SET NULL,
    FOREIGN KEY (territorio_id) REFERENCES Territorios(id) ON DELETE SET NULL,
    CHECK((municipio_id IS NOT NULL AND territorio_id IS NULL) OR (municipio_id IS NULL AND territorio_id IS NOT NULL))
);

-- Table 8: EspeciesPlantas - Plant species (scientific names are unique keys)
CREATE TABLE IF NOT EXISTS EspeciesPlantas (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    nome_cientifico TEXT NOT NULL UNIQUE COLLATE NOCASE,
    autores_nome_cientifico TEXT,
    familia_botanica TEXT,
    nome_aceito_atual TEXT,
    sinonimo_de_id INTEGER,
    status_validacao TEXT DEFAULT 'nao_validado' CHECK(status_validacao IN ('validado', 'nao_validado', 'ambiguo')),
    fonte_validacao TEXT,  -- e.g., "GBIF", "Tropicos"
    usos_reportados TEXT,  -- JSON array: [{"uso": "...", "parte_utilizada": "...", "comunidade": "..."}]
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (sinonimo_de_id) REFERENCES EspeciesPlantas(id) ON DELETE SET NULL
);

-- Table 9: NomesVernaculares - Common/folk names for plants (supports homonomy)
CREATE TABLE IF NOT EXISTS NomesVernaculares (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    nome TEXT NOT NULL COLLATE NOCASE,
    idioma TEXT DEFAULT 'pt',
    regiao TEXT,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- Table 10: EspecieNomeVernacular - N:M relationship (especie ↔ vernacular names)
CREATE TABLE IF NOT EXISTS EspecieNomeVernacular (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    especie_id INTEGER NOT NULL,
    nome_vernacular_id INTEGER NOT NULL,
    confianca REAL DEFAULT 0.5 CHECK(confianca >= 0.0 AND confianca <= 1.0),
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (especie_id) REFERENCES EspeciesPlantas(id) ON DELETE CASCADE,
    FOREIGN KEY (nome_vernacular_id) REFERENCES NomesVernaculares(id) ON DELETE CASCADE,
    UNIQUE(especie_id, nome_vernacular_id)
);

-- Table 11: ArtigoEspecie - N:M relationship (articles ↔ species)
CREATE TABLE IF NOT EXISTS ArtigoEspecie (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    artigo_id INTEGER NOT NULL,
    especie_id INTEGER NOT NULL,
    parte_planta_utilizada TEXT,  -- e.g., "folha", "raiz", "casca"
    uso TEXT,  -- e.g., "medicinal", "alimentar", "ritual"
    comunidade TEXT,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (artigo_id) REFERENCES ArtigosCientificos(id) ON DELETE CASCADE,
    FOREIGN KEY (especie_id) REFERENCES EspeciesPlantas(id) ON DELETE CASCADE,
    UNIQUE(artigo_id, especie_id)
);

-- Table 12: Comunidades - Traditional communities
CREATE TABLE IF NOT EXISTS Comunidades (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    nome TEXT NOT NULL UNIQUE,
    tipo TEXT NOT NULL CHECK(tipo IN ('indigena', 'quilombola', 'ribeirinha', 'caicara', 'seringueira', 'pantaneira', 'outro')),
    localizacao TEXT,
    populacao INTEGER,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- ============ VIEWS ============

-- View: vw_artigos_completos - Aggregated article data
CREATE VIEW IF NOT EXISTS vw_artigos_completos AS
SELECT
    ac.id,
    ac.titulo,
    ac.doi,
    ac.ano_publicacao,
    ac.autores,
    ac.resumo,
    ac.status,
    ac.editado_manualmente,
    ac.data_processamento,
    ac.data_ultima_modificacao,
    de.periodo_inicio,
    de.periodo_fim,
    de.duracao_meses,
    COUNT(DISTINCT ae.especie_id) as total_especies,
    GROUP_CONCAT(DISTINCT ep.nome_cientifico, '; ') as especies,
    GROUP_CONCAT(DISTINCT m.nome, '; ') as municipios,
    GROUP_CONCAT(DISTINCT t.nome, '; ') as territorios,
    GROUP_CONCAT(DISTINCT c.nome, '; ') as comunidades
FROM ArtigosCientificos ac
LEFT JOIN DadosEstudo de ON ac.id = de.artigo_id
LEFT JOIN ArtigoEspecie ae ON ac.id = ae.artigo_id
LEFT JOIN EspeciesPlantas ep ON ae.especie_id = ep.id
LEFT JOIN ArtigoLocalizacao al ON ac.id = al.artigo_id
LEFT JOIN Municipios m ON al.municipio_id = m.id
LEFT JOIN Territorios t ON al.territorio_id = t.id
LEFT JOIN Comunidades c ON c.id = (
    SELECT id FROM Comunidades
    WHERE nome = COALESCE(
        (SELECT localizacao FROM (SELECT t.nome as localizacao FROM Territorios t WHERE t.id = al.territorio_id LIMIT 1)),
        (SELECT m.nome FROM Municipios m WHERE m.id = al.municipio_id LIMIT 1)
    )
    LIMIT 1
)
GROUP BY ac.id;

-- ============ TRIGGERS ============

-- Trigger: Update data_ultima_modificacao on ArtigosCientificos update
CREATE TRIGGER IF NOT EXISTS atualizar_modificacao_artigo
AFTER UPDATE ON ArtigosCientificos
FOR EACH ROW
BEGIN
    UPDATE ArtigosCientificos
    SET data_ultima_modificacao = CURRENT_TIMESTAMP
    WHERE id = NEW.id;
END;

-- Trigger: Mark as manually edited
CREATE TRIGGER IF NOT EXISTS marcar_edicao_manual
AFTER UPDATE ON ArtigosCientificos
FOR EACH ROW
WHEN OLD.titulo != NEW.titulo
   OR OLD.autores != NEW.autores
   OR OLD.resumo != NEW.resumo
BEGIN
    UPDATE ArtigosCientificos
    SET editado_manualmente = 1
    WHERE id = NEW.id;
END;

-- Trigger: Update EspeciesPlantas modification time
CREATE TRIGGER IF NOT EXISTS atualizar_especies_timestamp
AFTER UPDATE ON EspeciesPlantas
FOR EACH ROW
BEGIN
    UPDATE EspeciesPlantas
    SET updated_at = CURRENT_TIMESTAMP
    WHERE id = NEW.id;
END;

-- ============ INDEXES ============
-- Total: 18 indexes for query optimization

-- Articles indexes
CREATE INDEX IF NOT EXISTS idx_artigos_status ON ArtigosCientificos(status);
CREATE INDEX IF NOT EXISTS idx_artigos_ano ON ArtigosCientificos(ano_publicacao);
CREATE INDEX IF NOT EXISTS idx_artigos_data ON ArtigosCientificos(data_processamento);
CREATE INDEX IF NOT EXISTS idx_artigos_doi ON ArtigosCientificos(doi);
CREATE INDEX IF NOT EXISTS idx_artigos_duplicatas ON ArtigosCientificos(titulo, ano_publicacao);

-- Study data indexes
CREATE INDEX IF NOT EXISTS idx_estudo_artigo ON DadosEstudo(artigo_id);
CREATE INDEX IF NOT EXISTS idx_estudo_periodo ON DadosEstudo(periodo_inicio, periodo_fim);

-- Location hierarchy indexes
CREATE INDEX IF NOT EXISTS idx_estados_pais ON Estados(pais_id);
CREATE INDEX IF NOT EXISTS idx_municipios_estado ON Municipios(estado_id);

-- Article location indexes
CREATE INDEX IF NOT EXISTS idx_artigo_loc_artigo ON ArtigoLocalizacao(artigo_id);
CREATE INDEX IF NOT EXISTS idx_artigo_loc_municipio ON ArtigoLocalizacao(municipio_id);
CREATE INDEX IF NOT EXISTS idx_artigo_loc_territorio ON ArtigoLocalizacao(territorio_id);

-- Species indexes
CREATE INDEX IF NOT EXISTS idx_especies_nome ON EspeciesPlantas(nome_cientifico);
CREATE INDEX IF NOT EXISTS idx_especies_familia ON EspeciesPlantas(familia_botanica);
CREATE INDEX IF NOT EXISTS idx_especies_validacao ON EspeciesPlantas(status_validacao);
CREATE INDEX IF NOT EXISTS idx_especies_sinonimo ON EspeciesPlantas(sinonimo_de_id);

-- Article-species link indexes
CREATE INDEX IF NOT EXISTS idx_artigo_especie_artigo ON ArtigoEspecie(artigo_id);
CREATE INDEX IF NOT EXISTS idx_artigo_especie_especie ON ArtigoEspecie(especie_id);

-- Vernacular names indexes
CREATE INDEX IF NOT EXISTS idx_nomes_vernaculares_nome ON NomesVernaculares(nome);
CREATE INDEX IF NOT EXISTS idx_especie_nome_vern ON EspecieNomeVernacular(especie_id);

-- ============ PRAGMAS ============

-- Enable foreign keys
PRAGMA foreign_keys = ON;

-- WAL mode for better concurrency
PRAGMA journal_mode = WAL;

-- Sync after every transaction for safety
PRAGMA synchronous = NORMAL;

-- Cache size (negative = KB)
PRAGMA cache_size = -64000;
