# Arquitetura do Sistema - EtnoPapers

> 📚 **Voltar ao README**: Veja [README.md](README.md) para informações gerais do projeto, instalação e como usar.

O EtnoPapers segue uma arquitetura em camadas que integra componentes locais e externos para processamento de documentos científicos.

### Visão Geral (C4 Model - Nível 1: Contexto do Sistema)

```mermaid
graph TB
    User[👤 Pesquisador<br/>Etnobotânico]

    subgraph Sistema["EtnoPapers"]
        App[EtnoPapers]
    end

    OLLAMA[🤖 OLLAMA<br/>Serviço Local de IA]
    MongoDB[☁️ MongoDB<br/>Atlas ou Local]

    User -->|Upload PDFs<br/>Gerencia Registros| App
    App -->|Texto do PDF| OLLAMA
    OLLAMA -->|Metadados Extraídos| App
    App -->|Sincroniza Dados| MongoDB
    MongoDB -->|Confirma Upload| App

    style Sistema fill:#e1f5ff,stroke:#0066cc,stroke-width:3px,color:black
    style OLLAMA fill:#fff4e6,stroke:#ff9800,stroke-width:2px,color:black
    style MongoDB fill:#e8f5e9,stroke:#4caf50,stroke-width:2px,color:black
    style User fill:#f3e5f5,stroke:#9c27b0,stroke-width:2px,color:black
```

### Containers (C4 Model - Nível 2: Containers)

```mermaid
graph TB
    User[👤 Usuário]

    subgraph EtnoPapers["EtnoPapers Application"]
        UI[WPF Desktop UI<br/>C# .NET 8<br/>---<br/>Páginas: Upload, Registros,<br/>Configurações]

        Services[Camada de Serviços<br/>---<br/>ExtractionService<br/>DataStorageService<br/>MongoSyncService<br/>LoggerService]

        LocalDB[(JSON Local<br/>---<br/>Documents/<br/>EtnoPapers/<br/>data.json)]
    end

    OLLAMA[OLLAMA API<br/>HTTP REST]
    MongoDB[(MongoDB<br/>---<br/>Coleção:<br/>papers)]

    User -->|Interage| UI
    UI -->|Chama| Services
    Services -->|POST /api/generate| OLLAMA
    Services -->|Lê/Escreve| LocalDB
    Services -->|Insert Documents| MongoDB

    style UI fill:#bbdefb,stroke:#1976d2,stroke-width:2px,color:black
    style Services fill:#c8e6c9,stroke:#388e3c,stroke-width:2px,color:black
    style LocalDB fill:#fff9c4,stroke:#f57c00,stroke-width:2px,color:black
    style OLLAMA fill:#ffccbc,stroke:#d84315,stroke-width:2px,color:black
    style MongoDB fill:#b2dfdb,stroke:#00796b,stroke-width:2px,color:black
```

### Componentes (C4 Model - Nível 3: Componentes Principais)

```mermaid
graph LR
    subgraph UI["Interface WPF"]
        UploadPage[UploadPage<br/>Upload de PDFs]
        RecordsPage[RecordsPage<br/>Lista de Registros]
        ConfigPage[ConfigPage<br/>Configurações]
        EditDialog[EditRecordDialog<br/>Edição de Dados]
    end

    subgraph ViewModels["ViewModels - MVVM"]
        UVM[UploadViewModel]
        RVM[RecordsViewModel]
        CVM[ConfigViewModel]
    end

    subgraph Services["Serviços"]
        ES[ExtractionService<br/>Integração OLLAMA]
        DSS[DataStorageService<br/>Persistência JSON]
        MSS[MongoSyncService<br/>Upload MongoDB]
        LS[LoggerService<br/>Logs e Rastreamento]
    end

    UploadPage --> UVM
    RecordsPage --> RVM
    ConfigPage --> CVM
    UVM --> EditDialog

    UVM --> ES
    UVM --> DSS
    RVM --> DSS
    RVM --> MSS
    CVM --> MSS

    ES --> LS
    DSS --> LS
    MSS --> LS

    style UI fill:#e3f2fd,stroke:#1565c0,stroke-width:2px,color:black
    style ViewModels fill:#f3e5f5,stroke:#7b1fa2,stroke-width:2px,color:black
    style Services fill:#e8f5e9,stroke:#2e7d32,stroke-width:2px,color:black
```

---

## Fluxo de Trabalho

O EtnoPapers processa documentos científicos através de um fluxo automatizado com validação humana.

```mermaid
flowchart TD
    Start([👤 Usuário inicia<br/>upload de PDF])

    Upload[📄 Upload do PDF<br/>Arquivo selecionado]
    ValidPDF{PDF válido?}

    CheckOLLAMA{OLLAMA<br/>disponível?}
    ErrorOLLAMA[❌ Erro: OLLAMA<br/>não conectado]

    ExtractText[📖 Extração de texto<br/>do PDF]
    SendOLLAMA[🤖 Envio para OLLAMA<br/>com prompt configurado]

    ProcessAI[⚙️ Processamento IA<br/>Análise do conteúdo]

    ExtractMetadata[📋 Extração de metadados:<br/>- Título<br/>- Autores<br/>- Ano<br/>- Resumo<br/>- Espécies<br/>- Localização<br/>- Bioma]

    Validate{Dados<br/>completos?}

    ShowDialog[✏️ Janela de Edição<br/>EditRecordDialog]
    ReviewData[👁️ Usuário revisa<br/>e edita dados]

    UserAction{Ação do<br/>usuário?}

    SaveLocal[💾 Salvar no JSON local<br/>Documents/EtnoPapers/data.json]
    ShowSuccess[✅ Mensagem de sucesso<br/>Limpar tela]

    NavigateRecords[📚 Usuário navega<br/>para Registros]
    LoadRecords[🔄 Carregar registros<br/>do JSON]
    DisplayGrid[📊 Exibir em DataGrid<br/>Título, Ano, Autores, País, Bioma]

    SelectSync{Usuário seleciona<br/>registros para sync?}
    CheckMongo{MongoDB<br/>configurado?}
    SyncMongo[☁️ Sincronizar com MongoDB<br/>Upload de documentos]
    DeleteLocal[🗑️ Deletar registros locais<br/>após confirmação]

    End([✅ Processo concluído])
    Cancel([❌ Cancelado])

    Start --> Upload
    Upload --> ValidPDF
    ValidPDF -->|Não| ErrorOLLAMA
    ValidPDF -->|Sim| CheckOLLAMA
    CheckOLLAMA -->|Não| ErrorOLLAMA
    CheckOLLAMA -->|Sim| ExtractText
    ExtractText --> SendOLLAMA
    SendOLLAMA --> ProcessAI
    ProcessAI --> ExtractMetadata
    ExtractMetadata --> Validate

    Validate -->|Completos ou<br/>Incompletos| ShowDialog
    ShowDialog --> ReviewData
    ReviewData --> UserAction

    UserAction -->|Salvar| SaveLocal
    UserAction -->|Cancelar| Cancel

    SaveLocal --> ShowSuccess
    ShowSuccess --> NavigateRecords
    NavigateRecords --> LoadRecords
    LoadRecords --> DisplayGrid

    DisplayGrid --> SelectSync
    SelectSync -->|Sim| CheckMongo
    SelectSync -->|Não| End

    CheckMongo -->|Configurado| SyncMongo
    CheckMongo -->|Não| End

    SyncMongo --> DeleteLocal
    DeleteLocal --> End

    ErrorOLLAMA --> Cancel

    style Start fill:#c8e6c9,stroke:#388e3c,stroke-width:2px,color:black
    style End fill:#c8e6c9,stroke:#388e3c,stroke-width:2px,color:black
    style Cancel fill:#ffcdd2,stroke:#c62828,stroke-width:2px,color:black
    style ProcessAI fill:#fff9c4,stroke:#f57c00,stroke-width:2px,color:black
    style ShowDialog fill:#e1bee7,stroke:#7b1fa2,stroke-width:2px,color:black
    style SaveLocal fill:#bbdefb,stroke:#1976d2,stroke-width:2px,color:black
    style SyncMongo fill:#b2dfdb,stroke:#00796b,stroke-width:2px,color:black
```

---

## 🔧 Considerações Técnicas: OLLAMA e Modelos de IA

### Integração com OLLAMA

O EtnoPapers utiliza OLLAMA como serviço de IA local para extração de metadados. A integração é feita via **API REST HTTP** na porta padrão `11434`.

**Fluxo Técnico:**

```
PDF → Texto Extraído → OLLAMAService → Prompt Estruturado → API /api/generate → JSON Response → Validação → ArticleRecord
```

### Modelo Recomendado: Qwen 2.5 7B

**Para máxima compatibilidade e desempenho, use: `ollama pull qwen2.5:7b`**

**Por que Qwen 2.5 7B é a melhor escolha para EtnoPapers:**

1. **Suporte Robusto a Português**
   - Treinamento específico em português brasileiro
   - Compreensão de termos científicos e etnobotânicos
   - Melhor handling de nomes vernaculares/científicos

2. **Excelência em Extração Estruturada (JSON)**
   - Modelo especializado em retornar JSON válido
   - Menos erros de sintaxe nas respostas
   - Melhor parsing das estruturas de dados esperadas

3. **Desempenho Otimizado**
   - Tempo médio: 15-30 segundos por PDF
   - Timeout configurado para 10 minutos (adequado)
   - Uso de RAM: 8-10 GB (compatível com máquinas comuns)

4. **Qualidade de Extração**
   - Taxa mais alta de campos extraídos corretamente
   - Menos alucinações e dados fictícios
   - Melhor compreensão de contexto etnobotânico

### Alternativas e Fallbacks

Se Qwen 2.5 não for adequado para sua máquina:

- **Qwen 2.5 14B** (16+ GB RAM): Versão maior, mais precisa
- **Mistral 7B**: Rápido, suporte razoável a português
- **Neural Chat 7B**: Compacto, menos preciso para JSON
- **Llama 2 7B**: Legacy, requer mais validação manual

### Configuração no OLLAMAService

O serviço é configurado em `src/EtnoPapers.Core/Services/OLLAMAService.cs`:

- **URL padrão**: `http://localhost:11434`
- **Timeout base**: 5 minutos, com retry até 10 minutos
- **Retry logic**: 2 tentativas com timeout crescente
- **Auto-detection**: Sistema detecta melhor modelo disponível

### Melhorias Futuras

- Suporte a modelos vision para análise de imagens em PDFs
- Cache de embeddings para PDFs similares
- Integração com modelos mais novos (Qwen 3.0, etc)
- Quantização para rodar em máquinas com menos RAM

---

> 👉 Para detalhes de instalação e guia de uso, volte ao [README.md](README.md)
