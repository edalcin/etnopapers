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
           ├────────────────────────────────┐
           │                                │
           │                                │
     ┌─────▼──────┐              ┌──────▼──────────┐
     │DadosEstudo │              │ArtigoLocalizacao│
     │   (1:1)    │              │  (Associação)   │
     └────────────┘              └─────┬───────────┘
                                       │ N:1
                              ┌────────┴────────┐
                              │                 │
                     ┌────────▼──────┐   ┌──────▼─────────┐
                     │   Municipios  │   │  Territorios   │
                     │ (Hierárquico) │   │  (Comunitário) │
                     └───────┬───────┘   └────────┬───────┘
                             │ N:1                │ N:1
                     ┌───────▼────────┐    ┌──────▼────────┐
                     │    Estados     │    │  Comunidades  │
                     └───────┬────────┘    └───────────────┘
                             │ N:1
                     ┌───────▼────────┐
                     │     Paises     │
                     └────────────────┘

┌──────────────────┐                      ┌──────────────────┐
│ArtigosCientificos│                      │ NomesVernaculares│
└────────┬─────────┘                      └────────┬─────────┘
         │ N:M                                     │ N:M
    ┌────▼────────────┐                 ┌──────────▼──────────┐
    │ArtigoEspecie    │                 │EspecieNomeVernacular│
    │  (Associação)   │                 │    (Associação)     │
    └────┬────────────┘                 └──────────┬──────────┘
         │ N:1                                     │ N:1
    ┌────▼──────────┐◄────────────────────────────┘
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

-- Índice composto para detecção de duplicatas (quando DOI não disponível)
CREATE INDEX idx_artigos_duplicatas ON ArtigosCientificos(titulo, ano_publicacao);
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
- **Detecção de duplicatas**: Sistema previne duplicação usando estratégia em duas camadas (detalhes abaixo)

**Detecção de Duplicatas**:

O sistema implementa verificação de duplicatas após extração de metadados para evitar processamento redundante:

**Estratégia de Detecção**:
1. **Critério Primário - DOI**: Se DOI estiver disponível, verifica unicidade via constraint UNIQUE
   ```sql
   SELECT id, titulo, data_processamento, status
   FROM ArtigosCientificos
   WHERE doi = ? AND doi IS NOT NULL;
   ```

2. **Critério Secundário - Título + Ano + Autor**: Se DOI não disponível, usa combinação:
   ```sql
   SELECT id, titulo, autores, data_processamento, status
   FROM ArtigosCientificos
   WHERE titulo = ? AND ano_publicacao = ?;
   -- Backend verifica primeiro autor do JSON array
   ```

**Fluxo de Detecção**:
1. Após extração de metadados, antes de salvar
2. Se duplicata encontrada: exibir mensagem com detalhes do registro existente
3. Usuário escolhe:
   - **Descartar**: Abandona novos metadados, mantém registro original
   - **Sobrescrever**: Substitui registro existente, atualiza `data_ultima_modificacao`

**Considerações**:
- DOI garante 100% de precisão na detecção
- Título+Ano+Autor pode ter falsos positivos em casos de artigos muito similares (estimativa: <5%)
- Verificação inclui tanto artigos finalizados quanto rascunhos
- Sobrescrever preserva `id` original mas atualiza todos os outros campos

---

### 2. Paises

**Descrição**: Representa países onde estudos etnobotânicos foram conduzidos. Nível mais alto da hierarquia geográfica.

**Schema SQL**:
```sql
CREATE TABLE Paises (
    -- Identificação
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    nome TEXT NOT NULL UNIQUE,  -- Nome do país (ex: "Brasil", "Peru")
    codigo_iso TEXT UNIQUE,  -- Código ISO 3166-1 alpha-2 (ex: "BR", "PE")

    -- Auditoria
    data_criacao DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- Índices
CREATE UNIQUE INDEX idx_paises_nome ON Paises(nome);
CREATE INDEX idx_paises_codigo ON Paises(codigo_iso);
```

**Campos**:
- `id`: Chave primária
- `nome`: Nome do país (obrigatório, único)
- `codigo_iso`: Código ISO de 2 letras (opcional, único)
- `data_criacao`: Timestamp de criação

**Regras de Negócio**:
- Países são reutilizados entre artigos (normalização)
- Nome deve ser único para evitar duplicatas
- Código ISO facilita integração com APIs externas

---

### 3. Estados

**Descrição**: Representa estados, províncias ou divisões administrativas de primeiro nível dentro de um país.

**Schema SQL**:
```sql
CREATE TABLE Estados (
    -- Identificação
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    nome TEXT NOT NULL,  -- Nome do estado (ex: "Amazonas", "Acre")
    sigla TEXT,  -- Sigla do estado (ex: "AM", "AC")

    -- Hierarquia
    pais_id INTEGER NOT NULL,

    -- Auditoria
    data_criacao DATETIME DEFAULT CURRENT_TIMESTAMP,

    FOREIGN KEY (pais_id) REFERENCES Paises(id) ON DELETE RESTRICT,
    UNIQUE(nome, pais_id)  -- Nome único dentro do país
);

-- Índices
CREATE INDEX idx_estados_pais ON Estados(pais_id);
CREATE INDEX idx_estados_nome ON Estados(nome);
CREATE INDEX idx_estados_sigla ON Estados(sigla);
```

**Campos**:
- `id`: Chave primária
- `nome`: Nome do estado/província (obrigatório)
- `sigla`: Sigla ou código do estado (opcional)
- `pais_id`: Referência ao país (obrigatório)
- `data_criacao`: Timestamp de criação

**Regras de Negócio**:
- Cada estado pertence a um único país
- Combinação nome+país deve ser única
- Estados são reutilizados entre artigos

---

### 4. Municipios

**Descrição**: Representa municípios, cidades ou divisões administrativas de segundo nível dentro de um estado.

**Schema SQL**:
```sql
CREATE TABLE Municipios (
    -- Identificação
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    nome TEXT NOT NULL,  -- Nome do município (ex: "Manaus", "São Gabriel da Cachoeira")

    -- Hierarquia
    estado_id INTEGER NOT NULL,

    -- Coordenadas (opcional)
    latitude REAL,
    longitude REAL,

    -- Auditoria
    data_criacao DATETIME DEFAULT CURRENT_TIMESTAMP,

    -- Validações
    CHECK (latitude IS NULL OR (latitude >= -90 AND latitude <= 90)),
    CHECK (longitude IS NULL OR (longitude >= -180 AND longitude <= 180)),
    FOREIGN KEY (estado_id) REFERENCES Estados(id) ON DELETE RESTRICT,
    UNIQUE(nome, estado_id)  -- Nome único dentro do estado
);

-- Índices
CREATE INDEX idx_municipios_estado ON Municipios(estado_id);
CREATE INDEX idx_municipios_nome ON Municipios(nome);
CREATE INDEX idx_municipios_coords ON Municipios(latitude, longitude);
```

**Campos**:
- `id`: Chave primária
- `nome`: Nome do município/cidade (obrigatório)
- `estado_id`: Referência ao estado (obrigatório)
- `latitude`, `longitude`: Coordenadas geográficas do município (opcional)
- `data_criacao`: Timestamp de criação

**Regras de Negócio**:
- Cada município pertence a um único estado
- Combinação nome+estado deve ser única
- Coordenadas podem ser obtidas de APIs de geocodificação
- Municípios são reutilizados entre artigos

---

### 5. Territorios

**Descrição**: Representa territórios comunitários sem definição espacial precisa ou relação com hierarquia geográfica tradicional. Espaços utilizados por comunidades tradicionais que não correspondem a divisões administrativas formais.

**Schema SQL**:
```sql
CREATE TABLE Territorios (
    -- Identificação
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    nome TEXT NOT NULL,  -- Nome do território (ex: "Terra Indígena Yanomami", "Território Quilombola Ivaporunduva")
    descricao TEXT,  -- Descrição detalhada do território e seus limites

    -- Relação com Comunidade
    comunidade_id INTEGER,  -- Opcional: comunidade associada ao território

    -- Localização Aproximada (opcional)
    descricao_localizacao TEXT,  -- Descrição textual (ex: "Região do Alto Rio Negro")
    latitude REAL,  -- Coordenadas aproximadas do centro ou ponto de referência
    longitude REAL,

    -- Auditoria
    data_criacao DATETIME DEFAULT CURRENT_TIMESTAMP,

    -- Validações
    CHECK (latitude IS NULL OR (latitude >= -90 AND latitude <= 90)),
    CHECK (longitude IS NULL OR (longitude >= -180 AND longitude <= 180)),
    FOREIGN KEY (comunidade_id) REFERENCES Comunidades(id) ON DELETE SET NULL
);

-- Índices
CREATE INDEX idx_territorios_nome ON Territorios(nome);
CREATE INDEX idx_territorios_comunidade ON Territorios(comunidade_id);
CREATE INDEX idx_territorios_coords ON Territorios(latitude, longitude);
```

**Campos**:
- `id`: Chave primária
- `nome`: Nome do território (obrigatório)
- `descricao`: Descrição detalhada incluindo limites, características
- `comunidade_id`: Comunidade associada ao território (opcional)
- `descricao_localizacao`: Descrição textual da localização
- `latitude`, `longitude`: Coordenadas aproximadas (opcional)
- `data_criacao`: Timestamp de criação

**Regras de Negócio**:
- Territórios NÃO possuem relação hierárquica com países/estados/municípios
- Representam espaços comunitários tradicionais que podem sobrepor ou transcender limites administrativos
- Coordenadas são aproximadas e opcionais (podem não ter definição espacial precisa)
- Territórios podem existir sem comunidade associada (ex: território ancestral desocupado)
- Um território pode ser referenciado por múltiplos artigos

**Exemplos de Territórios**:
- Terras Indígenas demarcadas ou em processo de demarcação
- Territórios quilombolas
- Áreas de uso tradicional de comunidades ribeirinhas
- Reservas extrativistas comunitárias
- Territórios sagrados ou de importância cultural

---

### 6. ArtigoLocalizacao

**Descrição**: Tabela de associação que permite vincular artigos a localizações. Um artigo pode estar associado a municípios (hierarquia geográfica) OU a territórios comunitários, mas não necessariamente ambos.

**Schema SQL**:
```sql
CREATE TABLE ArtigoLocalizacao (
    -- Identificação
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    artigo_id INTEGER NOT NULL,

    -- Localização (apenas um dos dois deve ser preenchido)
    municipio_id INTEGER,  -- Localização hierárquica (País > Estado > Município)
    territorio_id INTEGER,  -- Localização comunitária (sem hierarquia)

    -- Auditoria
    data_criacao DATETIME DEFAULT CURRENT_TIMESTAMP,

    -- Validações
    CHECK ((municipio_id IS NOT NULL AND territorio_id IS NULL) OR
           (municipio_id IS NULL AND territorio_id IS NOT NULL)),
    FOREIGN KEY (artigo_id) REFERENCES ArtigosCientificos(id) ON DELETE CASCADE,
    FOREIGN KEY (municipio_id) REFERENCES Municipios(id) ON DELETE RESTRICT,
    FOREIGN KEY (territorio_id) REFERENCES Territorios(id) ON DELETE RESTRICT
);

-- Índices
CREATE INDEX idx_artigo_loc_artigo ON ArtigoLocalizacao(artigo_id);
CREATE INDEX idx_artigo_loc_municipio ON ArtigoLocalizacao(municipio_id);
CREATE INDEX idx_artigo_loc_territorio ON ArtigoLocalizacao(territorio_id);
```

**Campos**:
- `id`: Chave primária
- `artigo_id`: Referência ao artigo (obrigatório)
- `municipio_id`: Referência a município (exclusivo com território)
- `territorio_id`: Referência a território (exclusivo com município)
- `data_criacao`: Timestamp de criação

**Regras de Negócio**:
- Um registro deve ter OU `municipio_id` OU `territorio_id`, nunca ambos
- Um artigo pode ter múltiplas localizações (mix de municípios e territórios)
- `ON DELETE CASCADE`: Se artigo for excluído, associações são removidas
- `ON DELETE RESTRICT`: Municípios/territórios não podem ser excluídos se associados a artigos

**Exemplos de Uso**:
1. Artigo sobre estudo em município específico:
   ```sql
   INSERT INTO ArtigoLocalizacao (artigo_id, municipio_id)
   VALUES (123, 45);  -- São Gabriel da Cachoeira, AM
   ```

2. Artigo sobre estudo em território tradicional:
   ```sql
   INSERT INTO ArtigoLocalizacao (artigo_id, territorio_id)
   VALUES (123, 67);  -- Terra Indígena Alto Rio Negro
   ```

3. Artigo com múltiplas localizações (mix):
   ```sql
   INSERT INTO ArtigoLocalizacao (artigo_id, municipio_id) VALUES (123, 45);
   INSERT INTO ArtigoLocalizacao (artigo_id, territorio_id) VALUES (123, 67);
   ```

---

### 7. Comunidades

**Descrição**: Representa comunidades tradicionais estudadas nos artigos (indígenas, quilombolas, ribeirinhas, etc.).

**Schema SQL**:
```sql
CREATE TABLE Comunidades (
    -- Identificação
    id INTEGER PRIMARY KEY AUTOINCREMENT,
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
- Comunidades são associadas a Territórios (não diretamente a municípios)

---

### 8. EspeciesPlantas

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
- `usos_reportados`: JSON array com usos mencionados nos artigos
- `data_criacao`: Timestamp de criação
- `data_ultima_validacao`: Timestamp da última validação taxonômica

**Regras de Negócio**:
- **Unicidade por nome científico**: Garante que não há duplicatas de espécies
- Se API de taxonomia retornar que o nome extraído é sinônimo, criar registro apontando para espécie aceita
- Campo `usos_reportados` é agregado de todos os artigos que mencionam a espécie
- Validação taxonômica pode ser re-executada periodicamente (ex: anualmente) para atualizar nomenclatura
- **Nomes vernaculares são armazenados em tabela separada com relação N:N** (ver NomesVernaculares)

---

### 9. NomesVernaculares

**Descrição**: Catálogo de nomes populares/vernaculares de plantas. Uma espécie pode ter múltiplos nomes vernaculares, e um nome vernacular pode referir-se a múltiplas espécies (relação N:N).

**Schema SQL**:
```sql
CREATE TABLE NomesVernaculares (
    -- Identificação
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    nome TEXT NOT NULL,  -- Nome vernacular (ex: "ipê-roxo", "pau d'arco", "unha-de-gato")

    -- Contexto Linguístico/Cultural
    idioma TEXT,  -- Idioma ou língua (ex: "português", "Yanomami", "Guarani")
    regiao_uso TEXT,  -- Descrição da região onde o nome é usado

    -- Auditoria
    data_criacao DATETIME DEFAULT CURRENT_TIMESTAMP,

    -- Índice composto para evitar duplicatas de nome+idioma
    UNIQUE(nome, idioma)
);

-- Índices
CREATE INDEX idx_nomes_vern_nome ON NomesVernaculares(nome);
CREATE INDEX idx_nomes_vern_idioma ON NomesVernaculares(idioma);
```

**Campos**:
- `id`: Chave primária
- `nome`: Nome vernacular (obrigatório)
- `idioma`: Idioma ou língua indígena do nome (opcional)
- `regiao_uso`: Descrição da região onde este nome é utilizado (opcional)
- `data_criacao`: Timestamp de criação

**Regras de Negócio**:
- Combinação nome+idioma deve ser única (mesmo nome em idiomas diferentes são registros distintos)
- Nomes vernaculares são reutilizados entre espécies (permite homonímia)
- Campo `idioma` é essencial para distinguir nomes vernaculares em línguas indígenas diferentes
- Um nome vernacular pode estar associado a múltiplas espécies (ex: "capim" pode referir-se a diversas gramíneas)

**Exemplos**:
- ("unha-de-gato", "português", "Amazônia") → pode referir-se a Uncaria tomentosa ou Uncaria guianensis
- ("ipê-roxo", "português", "Brasil") → pode referir-se a Handroanthus impetiginosus
- ("yãpinã", "Yanomami", "Alto Rio Negro") → nome indígena para espécie específica

---

### 10. EspecieNomeVernacular

**Descrição**: Tabela de associação N:M entre Espécies de Plantas e Nomes Vernaculares. Permite que uma espécie tenha múltiplos nomes populares e que um nome popular refira-se a múltiplas espécies.

**Schema SQL**:
```sql
CREATE TABLE EspecieNomeVernacular (
    especie_id INTEGER NOT NULL,
    nome_vernacular_id INTEGER NOT NULL,

    -- Contexto da Associação
    fonte_informacao TEXT,  -- De onde veio esta associação (ex: "artigo #123", "comunidade Baniwa")
    confianca TEXT DEFAULT 'média',  -- 'alta', 'média', 'baixa' - nível de confiança na associação

    -- Auditoria
    data_criacao DATETIME DEFAULT CURRENT_TIMESTAMP,

    PRIMARY KEY (especie_id, nome_vernacular_id),
    FOREIGN KEY (especie_id) REFERENCES EspeciesPlantas(id) ON DELETE CASCADE,
    FOREIGN KEY (nome_vernacular_id) REFERENCES NomesVernaculares(id) ON DELETE RESTRICT,
    CHECK (confianca IN ('alta', 'média', 'baixa'))
);

-- Índices
CREATE INDEX idx_esp_nome_vern_especie ON EspecieNomeVernacular(especie_id);
CREATE INDEX idx_esp_nome_vern_nome ON EspecieNomeVernacular(nome_vernacular_id);
```

**Campos**:
- `especie_id`: Referência à espécie (obrigatório)
- `nome_vernacular_id`: Referência ao nome vernacular (obrigatório)
- `fonte_informacao`: Origem desta associação (opcional)
- `confianca`: Nível de confiança na associação (padrão: 'média')
- `data_criacao`: Timestamp de criação

**Regras de Negócio**:
- Permite modelar homonímia (um nome popular para múltiplas espécies)
- Permite modelar sinonímia vernacular (múltiplos nomes para uma espécie)
- Campo `confianca` indica qualidade da associação (útil para casos ambíguos)
- `ON DELETE CASCADE`: Se espécie for excluída, associações são removidas
- `ON DELETE RESTRICT`: Nomes vernaculares não podem ser excluídos se associados a espécies

**Exemplos de Uso**:
1. Uma espécie com múltiplos nomes vernaculares:
   ```sql
   -- Uncaria tomentosa tem vários nomes populares
   INSERT INTO EspecieNomeVernacular (especie_id, nome_vernacular_id, confianca)
   VALUES
     (1, 5, 'alta'),  -- "unha-de-gato" em português
     (1, 6, 'alta'),  -- "uña de gato" em espanhol
     (1, 7, 'média'); -- "cat's claw" em inglês
   ```

2. Um nome vernacular referindo-se a múltiplas espécies (homonímia):
   ```sql
   -- "capim-limão" pode referir-se a diferentes espécies
   INSERT INTO EspecieNomeVernacular (especie_id, nome_vernacular_id, confianca)
   VALUES
     (10, 20, 'alta'),  -- Cymbopogon citratus (mais comum)
     (11, 20, 'média'); -- Melissa officinalis (menos comum, mas também chamada assim)
   ```

---

### 11. ArtigoEspecie

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

### 12. DadosEstudo

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

    -- Agregações de Localização
    GROUP_CONCAT(DISTINCT
        CASE
            WHEN al.municipio_id IS NOT NULL THEN
                m.nome || ', ' || e.sigla || ', ' || p.nome
            WHEN al.territorio_id IS NOT NULL THEN
                t.nome || ' (território)'
        END
    ) AS localizacoes,

    GROUP_CONCAT(DISTINCT c.nome) AS comunidades,
    COUNT(DISTINCT ae.especie_id) AS total_especies

FROM ArtigosCientificos a
LEFT JOIN DadosEstudo de ON a.id = de.artigo_id
LEFT JOIN ArtigoLocalizacao al ON a.id = al.artigo_id
LEFT JOIN Municipios m ON al.municipio_id = m.id
LEFT JOIN Estados e ON m.estado_id = e.id
LEFT JOIN Paises p ON e.pais_id = p.id
LEFT JOIN Territorios t ON al.territorio_id = t.id
LEFT JOIN Comunidades c ON t.comunidade_id = c.id
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

-- 3A. OPÇÃO 1: Inserir localização hierárquica (município)
-- Inserir ou reutilizar país
INSERT OR IGNORE INTO Paises (nome, codigo_iso) VALUES ('Brasil', 'BR');

-- Inserir ou reutilizar estado
INSERT OR IGNORE INTO Estados (nome, sigla, pais_id)
VALUES ('Amazonas', 'AM', (SELECT id FROM Paises WHERE codigo_iso = 'BR'));

-- Inserir ou reutilizar município
INSERT OR IGNORE INTO Municipios (nome, estado_id)
VALUES ('São Gabriel da Cachoeira', (SELECT id FROM Estados WHERE sigla = 'AM'));

-- Associar artigo ao município
INSERT INTO ArtigoLocalizacao (artigo_id, municipio_id)
VALUES (
    last_insert_rowid(),  -- ID do artigo
    (SELECT id FROM Municipios WHERE nome = 'São Gabriel da Cachoeira')
);

-- 3B. OPÇÃO 2: Inserir localização por território comunitário
-- Inserir ou reutilizar comunidade
INSERT OR IGNORE INTO Comunidades (nome, tipo_comunidade)
VALUES ('Comunidade Baniwa do Rio Içana', 'indígena');

-- Inserir território
INSERT INTO Territorios (nome, descricao, comunidade_id, descricao_localizacao)
VALUES (
    'Terra Indígena Alto Rio Negro',
    'Território tradicional do povo Baniwa no Alto Rio Negro',
    (SELECT id FROM Comunidades WHERE nome = 'Comunidade Baniwa do Rio Içana'),
    'Região do Alto Rio Negro, fronteira Brasil-Colômbia'
);

-- Associar artigo ao território
INSERT INTO ArtigoLocalizacao (artigo_id, territorio_id)
VALUES (
    last_insert_rowid(),  -- ID do artigo
    (SELECT id FROM Territorios WHERE nome = 'Terra Indígena Alto Rio Negro')
);

-- 4. Inserir ou reutilizar espécie
INSERT OR IGNORE INTO EspeciesPlantas (nome_cientifico, familia_botanica, status_validacao)
VALUES ('Uncaria tomentosa', 'Rubiaceae', 'não validado');

-- 5. Inserir nomes vernaculares para a espécie
INSERT OR IGNORE INTO NomesVernaculares (nome, idioma) VALUES ('unha-de-gato', 'português');
INSERT OR IGNORE INTO NomesVernaculares (nome, idioma) VALUES ('uña de gato', 'espanhol');

-- Associar nomes vernaculares à espécie
INSERT OR IGNORE INTO EspecieNomeVernacular (especie_id, nome_vernacular_id, confianca)
VALUES
    (
        (SELECT id FROM EspeciesPlantas WHERE nome_cientifico = 'Uncaria tomentosa'),
        (SELECT id FROM NomesVernaculares WHERE nome = 'unha-de-gato' AND idioma = 'português'),
        'alta'
    ),
    (
        (SELECT id FROM EspeciesPlantas WHERE nome_cientifico = 'Uncaria tomentosa'),
        (SELECT id FROM NomesVernaculares WHERE nome = 'uña de gato' AND idioma = 'espanhol'),
        'alta'
    );

-- 6. Associar espécie ao artigo
INSERT INTO ArtigoEspecie (artigo_id, especie_id, contexto_uso, parte_planta_utilizada)
VALUES (
    last_insert_rowid(),  -- ID do artigo
    (SELECT id FROM EspeciesPlantas WHERE nome_cientifico = 'Uncaria tomentosa'),
    'Medicinal para inflamações',
    '["casca", "raiz"]'
);
```

### Verificar duplicatas antes de inserir

```sql
-- Verificação primária: por DOI (se disponível)
SELECT
    id,
    titulo,
    ano_publicacao,
    autores,
    data_processamento,
    status,
    editado_manualmente
FROM ArtigosCientificos
WHERE doi = '10.1234/exemplo.2023'
  AND doi IS NOT NULL;

-- Se retornar resultado: duplicata detectada
-- Se retornar vazio: prosseguir para verificação secundária

-- Verificação secundária: por título + ano (quando DOI não disponível)
SELECT
    id,
    titulo,
    ano_publicacao,
    autores,
    data_processamento,
    status
FROM ArtigosCientificos
WHERE titulo = 'Etnobotânica da comunidade ribeirinha do Rio Negro'
  AND ano_publicacao = 2023;

-- Backend: extrair primeiro autor do JSON array e comparar
-- Se título+ano+autor coincidirem: duplicata detectada

-- Verificar duplicatas incluindo rascunhos
SELECT
    id,
    titulo,
    ano_publicacao,
    status,
    data_processamento,
    CASE
        WHEN status = 'rascunho' THEN 'Rascunho pendente desde ' || data_processamento
        WHEN status = 'finalizado' THEN 'Finalizado em ' || data_processamento
    END as status_descricao
FROM ArtigosCientificos
WHERE doi = ?
   OR (titulo = ? AND ano_publicacao = ?)
ORDER BY data_processamento DESC;
```

### Sobrescrever artigo duplicado

```sql
-- Quando usuário escolhe "Sobrescrever" em duplicata detectada
UPDATE ArtigosCientificos
SET
    titulo = 'Novo título (se diferente)',
    ano_publicacao = 2024,
    autores = '["Silva, A.B.", "Santos, C.D.", "Oliveira, E.F."]',
    resumo = 'Novo resumo...',
    local_publicacao = 'Nova revista',
    status = 'finalizado',
    editado_manualmente = 1,
    data_ultima_modificacao = CURRENT_TIMESTAMP
WHERE id = ?;  -- ID do registro duplicado encontrado

-- Trigger atualiza automaticamente data_ultima_modificacao
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

### Buscar espécie por nome vernacular

```sql
-- Buscar todas as espécies conhecidas como "unha-de-gato"
SELECT
    e.id,
    e.nome_cientifico,
    e.familia_botanica,
    nv.nome AS nome_vernacular,
    nv.idioma,
    env.confianca
FROM EspeciesPlantas e
JOIN EspecieNomeVernacular env ON e.id = env.especie_id
JOIN NomesVernaculares nv ON env.nome_vernacular_id = nv.id
WHERE nv.nome = 'unha-de-gato'
ORDER BY env.confianca DESC, e.nome_cientifico;
```

### Listar todos os nomes vernaculares de uma espécie

```sql
-- Listar todos os nomes populares de Uncaria tomentosa
SELECT
    nv.nome,
    nv.idioma,
    nv.regiao_uso,
    env.confianca,
    env.fonte_informacao
FROM EspeciesPlantas e
JOIN EspecieNomeVernacular env ON e.id = env.especie_id
JOIN NomesVernaculares nv ON env.nome_vernacular_id = nv.id
WHERE e.nome_cientifico = 'Uncaria tomentosa'
ORDER BY nv.idioma, nv.nome;
```

### Buscar artigos por localização hierárquica

```sql
-- Buscar artigos realizados no estado do Amazonas
SELECT DISTINCT
    a.id,
    a.titulo,
    a.ano_publicacao,
    m.nome AS municipio,
    e.nome AS estado,
    p.nome AS pais
FROM ArtigosCientificos a
JOIN ArtigoLocalizacao al ON a.id = al.artigo_id
JOIN Municipios m ON al.municipio_id = m.id
JOIN Estados e ON m.estado_id = e.id
JOIN Paises p ON e.pais_id = p.id
WHERE e.sigla = 'AM'
ORDER BY a.ano_publicacao DESC;

-- Buscar artigos por país
SELECT DISTINCT
    a.id,
    a.titulo,
    a.ano_publicacao,
    p.nome AS pais,
    COUNT(DISTINCT m.id) AS total_municipios
FROM ArtigosCientificos a
JOIN ArtigoLocalizacao al ON a.id = al.artigo_id
JOIN Municipios m ON al.municipio_id = m.id
JOIN Estados e ON m.estado_id = e.id
JOIN Paises p ON e.pais_id = p.id
WHERE p.codigo_iso = 'BR'
GROUP BY a.id, a.titulo, a.ano_publicacao, p.nome
ORDER BY a.ano_publicacao DESC;
```

### Buscar artigos por território

```sql
-- Buscar artigos realizados em territórios indígenas
SELECT DISTINCT
    a.id,
    a.titulo,
    a.ano_publicacao,
    t.nome AS territorio,
    c.nome AS comunidade,
    c.tipo_comunidade
FROM ArtigosCientificos a
JOIN ArtigoLocalizacao al ON a.id = al.artigo_id
JOIN Territorios t ON al.territorio_id = t.id
LEFT JOIN Comunidades c ON t.comunidade_id = c.id
WHERE c.tipo_comunidade = 'indígena'
ORDER BY a.ano_publicacao DESC;
```

### Listar espécies com múltiplos nomes vernaculares (sinonímia vernacular)

```sql
-- Espécies com mais de 3 nomes vernaculares diferentes
SELECT
    e.nome_cientifico,
    e.familia_botanica,
    GROUP_CONCAT(nv.nome || ' (' || nv.idioma || ')', '; ') AS nomes_vernaculares,
    COUNT(DISTINCT nv.id) AS total_nomes
FROM EspeciesPlantas e
JOIN EspecieNomeVernacular env ON e.id = env.especie_id
JOIN NomesVernaculares nv ON env.nome_vernacular_id = nv.id
GROUP BY e.id
HAVING COUNT(DISTINCT nv.id) > 3
ORDER BY total_nomes DESC;
```

### Detectar homonímia (nome vernacular referindo-se a múltiplas espécies)

```sql
-- Nomes vernaculares que referem-se a múltiplas espécies
SELECT
    nv.nome,
    nv.idioma,
    COUNT(DISTINCT e.id) AS total_especies,
    GROUP_CONCAT(e.nome_cientifico, ', ') AS especies
FROM NomesVernaculares nv
JOIN EspecieNomeVernacular env ON nv.id = env.nome_vernacular_id
JOIN EspeciesPlantas e ON env.especie_id = e.id
GROUP BY nv.id
HAVING COUNT(DISTINCT e.id) > 1
ORDER BY total_especies DESC;
```

### Estatísticas de localização

```sql
-- Distribuição de artigos por tipo de localização
SELECT
    CASE
        WHEN al.municipio_id IS NOT NULL THEN 'Município (hierárquico)'
        WHEN al.territorio_id IS NOT NULL THEN 'Território (comunitário)'
    END AS tipo_localizacao,
    COUNT(DISTINCT a.id) AS total_artigos
FROM ArtigosCientificos a
JOIN ArtigoLocalizacao al ON a.id = al.artigo_id
GROUP BY tipo_localizacao;

-- Top 10 municípios com mais artigos
SELECT
    m.nome AS municipio,
    e.sigla AS estado,
    COUNT(DISTINCT al.artigo_id) AS total_artigos
FROM Municipios m
JOIN Estados e ON m.estado_id = e.id
JOIN ArtigoLocalizacao al ON m.id = al.municipio_id
GROUP BY m.id
ORDER BY total_artigos DESC
LIMIT 10;

-- Top 10 territórios com mais artigos
SELECT
    t.nome AS territorio,
    c.nome AS comunidade,
    c.tipo_comunidade,
    COUNT(DISTINCT al.artigo_id) AS total_artigos
FROM Territorios t
LEFT JOIN Comunidades c ON t.comunidade_id = c.id
JOIN ArtigoLocalizacao al ON t.id = al.territorio_id
GROUP BY t.id
ORDER BY total_artigos DESC
LIMIT 10;
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

**Total de índices**: 29 índices criados para otimizar queries comuns:
- Busca por status de artigos
- Filtros por ano de publicação
- Lookup de espécies por nome científico
- Queries de associação N:M
- Hierarquia geográfica (país → estado → município)
- Busca por nomes vernaculares e idioma
- Localização por município ou território

### Estimativas de Tamanho

Para **1.000 artigos** com média de:
- 5 espécies por artigo
- 3 nomes vernaculares por espécie
- 2 localizações por artigo (mix de municípios e territórios)
- 1 comunidade por território

**Tamanho estimado**:
- ArtigosCientificos: ~500 KB
- EspeciesPlantas: ~100 KB (assumindo 200 espécies únicas)
- NomesVernaculares: ~60 KB (assumindo 600 nomes únicos)
- Paises: ~2 KB (assumindo 10 países)
- Estados: ~10 KB (assumindo 50 estados)
- Municipios: ~40 KB (assumindo 200 municípios únicos)
- Territorios: ~50 KB (assumindo 100 territórios únicos)
- Comunidades: ~20 KB (assumindo 50 comunidades únicas)
- Tabelas de associação: ~250 KB
- **Total**: ~1.0 MB para 1.000 artigos

Para **10.000 artigos**: ~10 MB
Para **100.000 artigos**: ~100 MB

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
