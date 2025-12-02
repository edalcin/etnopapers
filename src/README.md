# EtnoPapers - Estrutura de Código Fonte

Esta pasta contém a implementação em C# WPF do EtnoPapers.

## Estrutura de Diretórios

```
src/
├── EtnoPapers.Core/          # Lógica de negócio e serviços core
│   ├── Services/             # Serviços da aplicação
│   │   ├── ConfigurationService.cs
│   │   ├── DataStorageService.cs
│   │   ├── PDFProcessingService.cs
│   │   ├── OLLAMAService.cs
│   │   ├── MongoDBSyncService.cs
│   │   └── ExtractionPipelineService.cs
│   ├── Models/               # Modelos de dados
│   │   ├── Article.cs
│   │   ├── Configuration.cs
│   │   └── SyncStatus.cs
│   ├── Utils/                # Utilitários e helpers
│   │   ├── TitleNormalizer.cs
│   │   ├── AuthorFormatter.cs
│   │   └── LanguageDetector.cs
│   └── Validation/           # Validação de dados
│       └── SchemaValidator.cs
│
├── EtnoPapers.UI/            # Interface WPF
│   ├── Views/                # Janelas e controles WPF
│   │   ├── MainWindow.xaml
│   │   ├── MainWindow.xaml.cs
│   │   ├── ExtractorView.xaml
│   │   ├── RecordsView.xaml
│   │   ├── SettingsView.xaml
│   │   └── SyncView.xaml
│   ├── ViewModels/           # ViewModels (MVVM pattern)
│   │   ├── MainViewModel.cs
│   │   ├── ExtractorViewModel.cs
│   │   ├── RecordsViewModel.cs
│   │   ├── SettingsViewModel.cs
│   │   └── SyncViewModel.cs
│   ├── Resources/            # Recursos (estilos, imagens, etc)
│   │   ├── Styles.xaml
│   │   └── Colors.xaml
│   └── App.xaml
│       └── App.xaml.cs
│
└── EtnoPapers.sln            # Solução Visual Studio

tests/
├── EtnoPapers.Core.Tests/    # Testes unitários e de integração
│   └── Services/
│       ├── PDFProcessingServiceTests.cs
│       ├── ExtractionPipelineTests.cs
│       └── MongoDBSyncServiceTests.cs

build/                        # Scripts e configurações de build
└── installer/               # Configuração do instalador Windows
```

## Tecnologias Utilizadas

- **.NET 8.0**: Framework principal
- **WPF**: Interface desktop Windows
- **MVVM**: Padrão de arquitetura (CommunityToolkit.Mvvm)
- **MongoDB .NET Driver**: Integração com MongoDB
- **iTextSharp ou Spire.Pdf**: Processamento de PDFs
- **Newtonsoft.Json**: Serialização JSON
- **xUnit/NUnit**: Testes unitários

## Como Contribuir

Veja `specs/migrate-to-wpf/spec.md` para especificação completa e `specs/migrate-to-wpf/plan.md` (em desenvolvimento) para o plano de implementação.

## Status da Migração

Este código-fonte está sendo desenvolvido como parte da migração de Electron para C# WPF. Acompanhe o progresso em `specs/migrate-to-wpf/tasks.md`.
