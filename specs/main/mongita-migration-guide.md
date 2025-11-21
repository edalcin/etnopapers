# Guia de Migração: SQLite → Mongita

**Objetivo**: Refatorar banco de dados de SQLite (12 tabelas normalizadas) para Mongita (documentos centrados em referências)
**Banco de Dados Alvo**: Mongita (Embedded MongoDB-compatible)
**Formato**: BSON (Binary JSON)
**Status**: Especificação

## Visão Geral da Migração

```
SQLite (Relacional)              →    Mongita (Document)
─────────────────────────────────────────────────────────
ArtigosCientificos              →    referencias
+ DadosEstudo                   │    (com dados_estudo embutido)
+ ArtigoLocalizacao             │    (com localizacoes[] embutido)
+ Paises/Estados/Municipios     │    (com estrutura geográfica)
+ Territorios                   │    (com territorio)
+ ArtigoEspecie                 │    (com especies[] embutido)
+ EspeciesPlantas              →    especies_plantas (sep. collection)
+ NomesVernaculares            │    (embedded em species)
+ Comunidades                  →    comunidades_indígenas (sep.)
```

---

## Passo 1: Instalar Dependências

```bash
# requirements.txt
mongita==3.0.0
pymongo==4.5.0        # Cliente PyMongo (API)
aiofiles==23.2.1      # Para operações async de arquivo
```

```bash
pip install -r requirements.txt
```

---

## Passo 2: Inicializar Cliente Mongita

**`backend/database.py`** (novo arquivo):

```python
from mongita import MongitaClientDisk, MongitaClientMemory
from contextlib import contextmanager
import os
from typing import Optional

# Configuração do banco de dados
DATABASE_PATH = os.getenv('DATABASE_PATH', './data/etnopapers')
DATABASE_BACKEND = os.getenv('DATABASE_BACKEND', 'disk')  # disk | memory

# Inicializar cliente baseado no backend
if DATABASE_BACKEND == 'memory':
    # Útil para testes
    client = MongitaClientMemory()
else:
    # Produção: persistência em disco
    os.makedirs(DATABASE_PATH, exist_ok=True)
    client = MongitaClientDisk(database_dir=DATABASE_PATH)

# Obter database
db = client['etnopapers']

# Inicializar coleções e índices
def init_database():
    """Inicializar coleções e criar índices"""

    # Coleção: referencias
    if 'referencias' not in db.list_collection_names():
        db.create_collection('referencias')

    # Índices para referencias
    db['referencias'].create_index('doi', unique=True)
    db['referencias'].create_index('status')
    db['referencias'].create_index('ano_publicacao')
    db['referencias'].create_index('data_processamento')
    db['referencias'].create_index([('especies.especie_id', 1)])
    db['referencias'].create_index([('comunidades.comunidade_id', 1)])
    db['referencias'].create_index([('localizacoes.pais', 1)])

    # Coleção: especies_plantas
    if 'especies_plantas' not in db.list_collection_names():
        db.create_collection('especies_plantas')

    db['especies_plantas'].create_index('nome_cientifico', unique=True)
    db['especies_plantas'].create_index('familia_botanica')
    db['especies_plantas'].create_index('status_validacao')

    # Coleção: comunidades_indígenas
    if 'comunidades_indígenas' not in db.list_collection_names():
        db.create_collection('comunidades_indígenas')

    db['comunidades_indígenas'].create_index('nome')
    db['comunidades_indígenas'].create_index('tipo')

    print("✓ Database initialized successfully")

@contextmanager
def get_db():
    """Context manager para acesso ao banco (compatível com async)"""
    try:
        yield db
    finally:
        pass  # Mongita não requer cleanup de conexão
```

---

## Passo 3: Criar Schemas Pydantic

**`backend/models/reference.py`** (novo):

```python
from pydantic import BaseModel, Field, validator
from typing import Optional, List, Dict, Any
from datetime import datetime
from bson import ObjectId

# ========== Nested Models ==========

class MetadadosEstudo(BaseModel):
    """Dados de metodologia do estudo"""
    periodo_inicio: Optional[str] = None
    periodo_fim: Optional[str] = None
    duracao_meses: Optional[int] = None
    metodos_coleta_dados: List[str] = []
    tipo_amostragem: Optional[str] = None
    tamanho_amostra: Optional[int] = None
    instrumentos_coleta: List[str] = []

    class Config:
        extra = 'allow'  # Permitir campos adicionais

class Localizacao(BaseModel):
    """Informação geográfica"""
    pais: str
    estado: Optional[str] = None
    municipio: Optional[str] = None
    territorio: Optional[str] = None
    coordenadas: Optional[Dict[str, float]] = None

class NomeVernacular(BaseModel):
    """Nome comum/folk de uma planta"""
    nome: str
    idioma: Optional[str] = None
    confianca: float = Field(default=0.5, ge=0.0, le=1.0)
    comunidade: Optional[str] = None

class Especie(BaseModel):
    """Referência a uma espécie de planta em uma referência"""
    especie_id: str  # ObjectId como string
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
    """Referência a uma comunidade indígena"""
    comunidade_id: str  # ObjectId como string
    nome: str
    tipo: str
    populacao_estimada: Optional[int] = None
    linguas: List[str] = []
    localizacao_territorio: Optional[str] = None

class UsoReportado(BaseModel):
    """Agregação de usos reportados por categoria"""
    categoria: str  # medicinal|alimentar|ritual|etc
    quantidade_especies: Optional[int] = None
    indicacoes_principais: List[str] = []
    alimentos: List[str] = []
    descricao: Optional[str] = None

# ========== Main Model ==========

class Referencia(BaseModel):
    """Documento principal de uma referência bibliográfica"""
    _id: Optional[str] = None  # ObjectId como string, auto-gerado

    # Identificação bibliográfica
    titulo: str
    doi: Optional[str] = None
    ano_publicacao: int
    local_publicacao: Optional[str] = None
    autores: List[str] = []
    resumo: Optional[str] = None

    # Conteúdo agregado
    metadata_estudo: MetadadosEstudo = Field(default_factory=MetadadosEstudo)
    localizacoes: List[Localizacao] = []
    especies: List[Especie] = []
    comunidades: List[Comunidade] = []
    usos_reportados: List[UsoReportado] = []

    # Status
    status: str = Field(default='rascunho', regex='^(rascunho|finalizado)$')
    editado_manualmente: bool = False
    notas_editor: Optional[str] = None

    # Auditoria
    data_processamento: Optional[datetime] = None
    data_ultima_modificacao: Optional[datetime] = None
    usuario_processamento: Optional[str] = None
    usuario_edicao: Optional[str] = None

    # Extensíveis (permitem adição de novos campos)
    origem_do_artigo: Optional[str] = None
    keywords: List[str] = []
    indice_qualidade_extracao: Optional[float] = None
    marcadores_pesquisa: List[str] = []

    class Config:
        extra = 'allow'  # Permite campos adicionais não definidos
        populate_by_name = True
        arbitrary_types_allowed = True

    @validator('ano_publicacao')
    def validate_ano(cls, v):
        if not (1900 <= v <= 2100):
            raise ValueError('Ano deve estar entre 1900 e 2100')
        return v

class ReferenceResponse(Referencia):
    """Resposta de API (com _id serializado)"""
    class Config(Referencia.Config):
        json_encoders = {
            ObjectId: str,
            datetime: lambda v: v.isoformat() if v else None
        }
```

**`backend/models/species.py`**:

```python
from pydantic import BaseModel
from typing import Optional, List, Dict, Any
from datetime import datetime

class NomeVernacularEspecie(BaseModel):
    nome: str
    idioma: Optional[str] = None
    pais: Optional[str] = None
    comunidades: List[str] = []
    confianca: float = 0.5
    fonte: Optional[str] = None

class CompotoAtivo(BaseModel):
    nome: str
    nome_cientifico: Optional[str] = None
    concentracao: Optional[str] = None
    parte_planta: Optional[str] = None
    efeito: Optional[str] = None

class UsoEspecie(BaseModel):
    categoria: str  # medicinal|alimentar|ritual_chamânico|etc
    indicacoes: List[str] = []
    modo_preparacao: Optional[str] = None
    quantidade_referencias: int = 0
    comunidades_mais_relatam: List[str] = []
    descricao: Optional[str] = None

class EspeciePlanta(BaseModel):
    _id: Optional[str] = None

    # Identificação científica
    nome_cientifico: str  # Unique index
    familia_botanica: Optional[str] = None
    autores_nome: Optional[str] = None

    # Nomenclatura validada
    nome_aceito_atual: Optional[str] = None
    sinonimos: List[str] = []

    # Validação taxonômica
    status_validacao: str = 'nao_validado'  # validado|nao_validado|sinonimo
    fonte_validacao: Optional[str] = None  # GBIF|Tropicos|manual
    data_validacao: Optional[datetime] = None
    gbif_key: Optional[int] = None
    tropicos_id: Optional[int] = None

    # Nomes populares
    nomes_vernaculares: List[NomeVernacularEspecie] = []

    # Características botânicas
    distribuicao_geografica: List[str] = []
    habitat: List[str] = []
    altura_media: Optional[str] = None
    tipo_folha: Optional[str] = None
    tipo_fruto: Optional[str] = None

    # Química
    compostos_ativos: List[CompotoAtivo] = []

    # Usos agregados
    usos_reportados: List[UsoEspecie] = []

    # Referências bibliográficas
    referencias_bibliograficas: List[Dict[str, Any]] = []

    # Controle
    data_criacao: Optional[datetime] = None
    data_ultima_atualizacao: Optional[datetime] = None
    quantidade_referencias_usando: int = 0

    class Config:
        extra = 'allow'
```

**`backend/models/community.py`**:

```python
from pydantic import BaseModel
from typing import Optional, List, Dict, Any
from datetime import datetime

class ComunidadeIndígena(BaseModel):
    _id: Optional[str] = None

    # Identificação
    nome: str
    tipo: str  # indígena|quilombola|ribeirinha|caiçara|seringueira|pantaneira|outro
    nomes_alternativos: List[str] = []

    # Geografia
    localizacao: Dict[str, Any] = {
        'pais': None,
        'estados': [],
        'municipios': [],
        'territorio_nome': None,
        'territorio_demarcado': False,
        'area_km2': None,
        'coordenadas_centro': None
    }

    # Demografia
    populacao_estimada: Optional[int] = None
    total_aldeias: Optional[int] = None
    lingua_principal: Optional[str] = None
    linguas_faladas: List[str] = []

    # Contexto cultural
    periodo_contato: Optional[str] = None
    organizacao_social: Optional[str] = None
    conhecimento_botanico_relevancia: str = 'medio'  # alto|medio|baixo
    descricao_breve: Optional[str] = None

    # Pesquisa
    referencias_etnobotanicas: Dict[str, Any] = {
        'quantidade_referencias': 0,
        'principais_pesquisadores': [],
        'periodo_pesquisa': None
    }

    # Conectar a referências
    referencias_ids: List[str] = []

    # Dados sensíveis
    dados_sensíveis_acordado: bool = False
    instituicao_parceira: Optional[str] = None
    contato_comunidade: Optional[str] = None

    class Config:
        extra = 'allow'
```

---

## Passo 4: Implementar Service Layer

**`backend/services/reference_service.py`** (novo):

```python
from datetime import datetime
from typing import Optional, List, Dict, Any
from bson import ObjectId
from ..database import db
from ..models.reference import Referencia, ReferenceResponse

class ReferenceService:
    """Serviço de operações de referências"""

    @staticmethod
    def create_reference(ref_data: Dict[str, Any]) -> str:
        """
        Criar nova referência

        Returns: ObjectId como string
        """
        # Preparar documento
        doc = {
            **ref_data,
            'data_processamento': datetime.now(),
            'data_ultima_modificacao': datetime.now()
        }

        # Inserir
        result = db['referencias'].insert_one(doc)
        return str(result.inserted_id)

    @staticmethod
    def get_reference(reference_id: str) -> Optional[Dict[str, Any]]:
        """Obter uma referência por ID"""
        try:
            doc = db['referencias'].find_one({'_id': ObjectId(reference_id)})
            if doc:
                doc['_id'] = str(doc['_id'])
            return doc
        except Exception as e:
            print(f"Erro ao buscar referência: {e}")
            return None

    @staticmethod
    def get_by_doi(doi: str) -> Optional[Dict[str, Any]]:
        """Buscar referência por DOI"""
        doc = db['referencias'].find_one({'doi': doi})
        if doc:
            doc['_id'] = str(doc['_id'])
        return doc

    @staticmethod
    def check_duplicate(titulo: str, ano: int, primeiro_autor: str) -> Optional[str]:
        """
        Verificar duplicata por título + ano + primeiro autor
        Returns: ID da referência existente ou None
        """
        doc = db['referencias'].find_one({
            'titulo': titulo,
            'ano_publicacao': ano,
            'autores': {'$elemMatch': {'$eq': primeiro_autor}}
        })
        return str(doc['_id']) if doc else None

    @staticmethod
    def update_reference(reference_id: str, update_data: Dict[str, Any]) -> bool:
        """Atualizar referência"""
        update_data['data_ultima_modificacao'] = datetime.now()

        result = db['referencias'].update_one(
            {'_id': ObjectId(reference_id)},
            {'$set': update_data}
        )
        return result.modified_count > 0

    @staticmethod
    def list_references(
        status: Optional[str] = None,
        ano: Optional[int] = None,
        limite: int = 100,
        offset: int = 0
    ) -> List[Dict[str, Any]]:
        """Listar referências com filtros opcionais"""
        query = {}
        if status:
            query['status'] = status
        if ano:
            query['ano_publicacao'] = ano

        docs = list(db['referencias']
                   .find(query)
                   .sort('data_processamento', -1)
                   .skip(offset)
                   .limit(limite))

        # Serializar ObjectIds
        for doc in docs:
            doc['_id'] = str(doc['_id'])

        return docs

    @staticmethod
    def count_references(status: Optional[str] = None) -> int:
        """Contar total de referências"""
        query = {'status': status} if status else {}
        return db['referencias'].count_documents(query)

    @staticmethod
    def delete_reference(reference_id: str) -> bool:
        """Deletar referência"""
        result = db['referencias'].delete_one({'_id': ObjectId(reference_id)})
        return result.deleted_count > 0

# Instância singleton
reference_service = ReferenceService()
```

**`backend/services/species_service.py`**:

```python
from typing import Optional, Dict, Any, List
from bson import ObjectId
from datetime import datetime
from ..database import db
from ..models.species import EspeciePlanta

class SpeciesService:
    """Serviço de operações de espécies de plantas"""

    @staticmethod
    def create_species(species_data: Dict[str, Any]) -> str:
        """Criar nova espécie"""
        doc = {
            **species_data,
            'data_criacao': datetime.now(),
            'data_ultima_atualizacao': datetime.now()
        }
        result = db['especies_plantas'].insert_one(doc)
        return str(result.inserted_id)

    @staticmethod
    def get_by_scientific_name(nome_cientifico: str) -> Optional[Dict[str, Any]]:
        """Buscar espécie por nome científico"""
        doc = db['especies_plantas'].find_one({'nome_cientifico': nome_cientifico})
        if doc:
            doc['_id'] = str(doc['_id'])
        return doc

    @staticmethod
    def get_species(species_id: str) -> Optional[Dict[str, Any]]:
        """Obter espécie por ID"""
        try:
            doc = db['especies_plantas'].find_one({'_id': ObjectId(species_id)})
            if doc:
                doc['_id'] = str(doc['_id'])
            return doc
        except:
            return None

    @staticmethod
    def update_species(species_id: str, update_data: Dict[str, Any]) -> bool:
        """Atualizar espécie"""
        update_data['data_ultima_atualizacao'] = datetime.now()
        result = db['especies_plantas'].update_one(
            {'_id': ObjectId(species_id)},
            {'$set': update_data}
        )
        return result.modified_count > 0

    @staticmethod
    def list_by_family(familia: str) -> List[Dict[str, Any]]:
        """Listar espécies por família botânica"""
        docs = list(db['especies_plantas'].find({'familia_botanica': familia}))
        for doc in docs:
            doc['_id'] = str(doc['_id'])
        return docs

    @staticmethod
    def get_validation_status(species_id: str) -> Optional[str]:
        """Obter status de validação de uma espécie"""
        doc = db['especies_plantas'].find_one(
            {'_id': ObjectId(species_id)},
            {'status_validacao': 1}
        )
        return doc['status_validacao'] if doc else None

species_service = SpeciesService()
```

---

## Passo 5: Implementar Routers FastAPI

**`backend/routers/references.py`** (novo):

```python
from fastapi import APIRouter, HTTPException, Query
from typing import Optional, List
from datetime import datetime
from ..services.reference_service import reference_service
from ..models.reference import Referencia, ReferenceResponse

router = APIRouter(prefix='/api/referencias', tags=['Referências'])

@router.post('/', response_model=dict)
async def create_reference(referencia: Referencia):
    """Criar nova referência"""
    # Verificar duplicata por DOI
    if referencia.doi:
        doc_existente = reference_service.get_by_doi(referencia.doi)
        if doc_existente:
            raise HTTPException(
                status_code=409,
                detail={'mensagem': 'DOI já existe', 'referencia_id': doc_existente['_id']}
            )

    # Verificar duplicata por título+ano+primeiro autor
    if referencia.autores:
        duplicado_id = reference_service.check_duplicate(
            referencia.titulo,
            referencia.ano_publicacao,
            referencia.autores[0]
        )
        if duplicado_id:
            raise HTTPException(
                status_code=409,
                detail={'mensagem': 'Referência duplicada detectada', 'referencia_id': duplicado_id}
            )

    ref_dict = referencia.dict(exclude_none=True)
    ref_dict['data_processamento'] = datetime.now()
    ref_dict['data_ultima_modificacao'] = datetime.now()

    referencia_id = reference_service.create_reference(ref_dict)
    return {'_id': referencia_id, 'mensagem': 'Referência criada com sucesso'}

@router.get('/{referencia_id}')
async def get_reference(referencia_id: str):
    """Obter referência por ID"""
    doc = reference_service.get_reference(referencia_id)
    if not doc:
        raise HTTPException(status_code=404, detail='Referência não encontrada')
    return doc

@router.get('/')
async def list_references(
    status: Optional[str] = Query(None),
    ano: Optional[int] = Query(None),
    limite: int = Query(100, ge=1, le=1000),
    offset: int = Query(0, ge=0)
):
    """Listar referências com filtros opcionais"""
    docs = reference_service.list_references(status, ano, limite, offset)
    total = reference_service.count_references(status)
    return {
        'total': total,
        'limite': limite,
        'offset': offset,
        'referências': docs
    }

@router.put('/{referencia_id}')
async def update_reference(referencia_id: str, updates: dict):
    """Atualizar referência"""
    sucesso = reference_service.update_reference(referencia_id, updates)
    if not sucesso:
        raise HTTPException(status_code=404, detail='Referência não encontrada')
    return {'mensagem': 'Referência atualizada com sucesso'}

@router.delete('/{referencia_id}')
async def delete_reference(referencia_id: str):
    """Deletar referência"""
    sucesso = reference_service.delete_reference(referencia_id)
    if not sucesso:
        raise HTTPException(status_code=404, detail='Referência não encontrada')
    return {'mensagem': 'Referência deletada com sucesso'}
```

---

## Passo 6: Migrar Dados Existentes (SQLite → Mongita)

**`backend/migration/migrate_sqlite_to_mongita.py`** (novo):

```python
import sqlite3
import json
from datetime import datetime
from bson import ObjectId
from mongita import MongitaClientDisk
from typing import Dict, List, Any

def migrate_sqlite_to_mongita(sqlite_path: str, mongita_path: str):
    """
    Migrar dados de SQLite para Mongita

    Mapear tabelas SQL para coleções Mongita:
    - ArtigosCientificos + DadosEstudo + relacionamentos → referencias
    - EspeciesPlantas + relacionamentos → especies_plantas
    - Comunidades → comunidades_indígenas
    """

    # Conectar ao SQLite
    sqlite_conn = sqlite3.connect(sqlite_path)
    sqlite_conn.row_factory = sqlite3.Row
    cursor = sqlite_conn.cursor()

    # Conectar ao Mongita
    mongita_client = MongitaClientDisk(database_dir=mongita_path)
    mongita_db = mongita_client['etnopapers']

    print("🔄 Iniciando migração SQLite → Mongita...")

    # ===== MIGRAR ESPÉCIES =====
    print("\n📚 Migrando espécies de plantas...")
    cursor.execute('SELECT * FROM EspeciesPlantas')
    especies_map = {}  # Map de SQLite ID → Mongita ObjectId

    for row in cursor.fetchall():
        spec_id = row['id']
        doc = {
            'nome_cientifico': row['nome_cientifico'],
            'familia_botanica': row['familia_botanica'],
            'nome_aceito_atual': row['nome_aceito_atual'],
            'autores_nome': row['autores_nome_cientifico'],
            'status_validacao': row['status_validacao'],
            'fonte_validacao': row['fonte_validacao'],
            'data_criacao': datetime.now(),
            'data_ultima_atualizacao': datetime.now()
        }

        # Obter nomes vernaculares
        cursor.execute('''
            SELECT nv.nome, envn.confianca
            FROM NomesVernaculares nv
            JOIN EspecieNomeVernacular envn ON nv.id = envn.nome_vernacular_id
            WHERE envn.especie_id = ?
        ''', (spec_id,))

        nomes_vern = [
            {'nome': r['nome'], 'confianca': r['confianca']}
            for r in cursor.fetchall()
        ]
        doc['nomes_vernaculares'] = nomes_vern

        # Inserir em Mongita
        result = mongita_db['especies_plantas'].insert_one(doc)
        especies_map[spec_id] = str(result.inserted_id)

    print(f"✓ {len(especies_map)} espécies migradas")

    # ===== MIGRAR COMUNIDADES =====
    print("\n👥 Migrando comunidades...")
    cursor.execute('SELECT * FROM Comunidades')
    comunidades_map = {}

    for row in cursor.fetchall():
        com_id = row['id']
        doc = {
            'nome': row['nome'],
            'tipo': row['tipo'],
            'populacao_estimada': row.get('populacao_estimada'),
            'data_criacao': datetime.now()
        }
        result = mongita_db['comunidades_indígenas'].insert_one(doc)
        comunidades_map[com_id] = str(result.inserted_id)

    print(f"✓ {len(comunidades_map)} comunidades migradas")

    # ===== MIGRAR REFERÊNCIAS =====
    print("\n📄 Migrando referências...")
    cursor.execute('SELECT * FROM ArtigosCientificos')

    for row in cursor.fetchall():
        artigo_id = row['id']

        # Parsing de autores (JSON)
        autores = json.loads(row['autores']) if isinstance(row['autores'], str) else []

        # Obter dados do estudo
        cursor.execute('SELECT * FROM DadosEstudo WHERE artigo_id = ?', (artigo_id,))
        estudo_row = cursor.fetchone()

        metadata_estudo = {}
        if estudo_row:
            metadata_estudo = {
                'periodo_inicio': estudo_row['periodo_inicio'],
                'periodo_fim': estudo_row['periodo_fim'],
                'duracao_meses': estudo_row['duracao_meses'],
                'metodos_coleta_dados': json.loads(estudo_row['metodos_coleta_dados']) if estudo_row['metodos_coleta_dados'] else [],
                'tipo_amostragem': estudo_row['tipo_amostragem'],
                'tamanho_amostra': estudo_row['tamanho_amostra'],
                'instrumentos_coleta': json.loads(estudo_row['instrumentos_coleta']) if estudo_row['instrumentos_coleta'] else []
            }

        # Obter localizações
        cursor.execute('''
            SELECT DISTINCT pais, estado, municipio, territorio
            FROM ArtigoLocalizacao al
            LEFT JOIN Municipios m ON al.municipio_id = m.id
            LEFT JOIN Estados e ON m.estado_id = e.id
            LEFT JOIN Paises p ON e.pais_id = p.id
            LEFT JOIN Territorios t ON al.territorio_id = t.id
            WHERE al.artigo_id = ?
        ''', (artigo_id,))

        localizacoes = [
            {
                'pais': r['pais'],
                'estado': r['estado'],
                'municipio': r['municipio'],
                'territorio': r['territorio']
            }
            for r in cursor.fetchall()
        ]

        # Obter espécies
        cursor.execute('''
            SELECT ep.id, ep.nome_cientifico, ae.contexto_uso
            FROM ArtigoEspecie ae
            JOIN EspeciesPlantas ep ON ae.especie_id = ep.id
            WHERE ae.artigo_id = ?
        ''', (artigo_id,))

        especies = []
        for r in cursor.fetchall():
            esp_id_sql = r['id']
            if esp_id_sql in especies_map:
                especies.append({
                    'especie_id': especies_map[esp_id_sql],
                    'nome_cientifico': r['nome_cientifico'],
                    'contexto_uso': r['contexto_uso']
                })

        # Obter comunidades
        cursor.execute('''
            SELECT DISTINCT c.id, c.nome, c.tipo
            FROM ArtigoLocalizacao al
            LEFT JOIN Territorios t ON al.territorio_id = t.id
            LEFT JOIN Comunidades c ON t.comunidade_id = c.id
            WHERE al.artigo_id = ? AND c.id IS NOT NULL
        ''', (artigo_id,))

        comunidades = []
        for r in cursor.fetchall():
            com_id_sql = r['id']
            if com_id_sql in comunidades_map:
                comunidades.append({
                    'comunidade_id': comunidades_map[com_id_sql],
                    'nome': r['nome'],
                    'tipo': r['tipo']
                })

        # Construir documento da referência
        referencia_doc = {
            'titulo': row['titulo'],
            'doi': row['doi'],
            'ano_publicacao': row['ano_publicacao'],
            'local_publicacao': row.get('local_publicacao'),
            'autores': autores,
            'resumo': row.get('resumo'),
            'metadata_estudo': metadata_estudo,
            'localizacoes': localizacoes,
            'especies': especies,
            'comunidades': comunidades,
            'status': row['status'],
            'editado_manualmente': row['editado_manualmente'],
            'data_processamento': row['data_processamento'],
            'data_ultima_modificacao': row['data_ultima_modificacao']
        }

        mongita_db['referencias'].insert_one(referencia_doc)

    print(f"✓ Referências migradas")

    # ===== CRIAR ÍNDICES =====
    print("\n🔍 Criando índices...")
    mongita_db['referencias'].create_index('doi', unique=True)
    mongita_db['referencias'].create_index('status')
    mongita_db['referencias'].create_index('ano_publicacao')
    mongita_db['especies_plantas'].create_index('nome_cientifico', unique=True)
    mongita_db['comunidades_indígenas'].create_index('nome')

    print("✓ Índices criados")

    sqlite_conn.close()
    print("\n✅ Migração concluída com sucesso!")

# Executar migração
if __name__ == '__main__':
    migrate_sqlite_to_mongita(
        sqlite_path='./data/etnopapers.db',
        mongita_path='./data/etnopapers'
    )
```

---

## Passo 7: Atualizar requirements.txt

```
# Backend Dependencies
fastapi==0.104.1
uvicorn[standard]==0.24.0
pydantic==2.5.0
python-multipart==0.0.6

# Database
mongita==3.0.0
pymongo==4.5.0
aiofiles==23.2.1

# Utilities
python-dotenv==1.0.0
```

---

## Passo 8: Atualizar Configuração Docker

**`Dockerfile`** (atualizado):

```dockerfile
# Build stage
FROM python:3.11-slim as builder

WORKDIR /app
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

# Runtime stage
FROM python:3.11-slim

WORKDIR /app

# Copiar dependências do builder
COPY --from=builder /usr/local/lib/python3.11/site-packages /usr/local/lib/python3.11/site-packages
COPY --from=builder /usr/local/bin /usr/local/bin

# Copiar código
COPY backend/ ./backend/
COPY frontend/dist/ ./frontend/dist/

# Criar diretório de dados
RUN mkdir -p /data/etnopapers

# Expor porta
EXPOSE 8000

# Variáveis de ambiente
ENV DATABASE_PATH=/data/etnopapers
ENV DATABASE_BACKEND=disk
ENV PORT=8000
ENV LOG_LEVEL=info

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD python -c "import requests; requests.get('http://localhost:8000/health')" || exit 1

# Iniciar aplicação
CMD ["uvicorn", "backend.main:app", "--host", "0.0.0.0", "--port", "8000"]
```

**`docker-compose.yml`**:

```yaml
version: '3.8'

services:
  etnopapers:
    build: .
    container_name: etnopapers
    ports:
      - "8000:8000"
    volumes:
      - ./data:/data
    environment:
      - DATABASE_PATH=/data/etnopapers
      - DATABASE_BACKEND=disk
      - PORT=8000
      - LOG_LEVEL=info
      - TAXONOMY_API_TIMEOUT=5
      - CACHE_TTL_DAYS=30
    restart: unless-stopped
```

---

## Checklist de Migração

- [ ] Instalar dependências (mongita, pymongo)
- [ ] Criar `backend/database.py` com cliente Mongita
- [ ] Criar modelos Pydantic em `backend/models/`
- [ ] Implementar service layer em `backend/services/`
- [ ] Implementar routers FastAPI em `backend/routers/`
- [ ] Escrever script de migração SQLite → Mongita
- [ ] Testar migração com dados de exemplo
- [ ] Validar integridade de dados após migração
- [ ] Atualizar requirements.txt
- [ ] Atualizar Dockerfile
- [ ] Testar build Docker
- [ ] Documentar novos endpoints API
- [ ] Atualizar testes unitários/integração

---

## Referências

- [Mongita Documentation](https://github.com/scottrogowski/mongita)
- [PyMongo API Reference](https://pymongo.readthedocs.io/)
- [FastAPI Database Tutorial](https://fastapi.tiangolo.com/)
- [Pydantic Validation](https://docs.pydantic.dev/)
