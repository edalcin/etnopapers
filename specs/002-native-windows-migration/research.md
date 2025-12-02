# Phase 0 Research: Native Windows Desktop Technology Selection

**Date**: 2025-12-02
**Feature**: Migrate to Native Windows Desktop Application
**Status**: Research Complete

---

## Research Overview

This document resolves key technology decisions identified in `plan.md` Technical Context section through comprehensive research and comparison of Windows desktop application frameworks.

**Questions Answered**:
1. C# with WPF vs WinUI3 vs C++ with Qt vs C++ with Win32
2. Best PDF text extraction library for each technology
3. Recommended technology stack for rapid development with long-term stability

---

## Decision 1: Primary Technology Framework

### Options Evaluated

| Framework | Language | Maturity | Primary Use | Stability | Dev Speed |
|-----------|----------|----------|------------|-----------|-----------|
| **WPF** | C# | Mature (19 years) | Enterprise desktop apps | Excellent | Fast |
| **WinUI3** | C# | Beta (2021-present) | Modern Windows UX | Poor | Moderate |
| **Qt** | C++ | Mature (30 years) | Cross-platform desktop | Excellent | Moderate |
| **Win32** | C++ | Legacy (40+ years) | System utilities | Good | Very Slow |

### Technology Comparison

#### C# with WPF (Windows Presentation Foundation)

**Decision**: ✅ **RECOMMENDED as Primary Technology**

**Rationale**:
- 19 years of production use across thousands of enterprise applications
- MVVM architecture built-in, enabling clean separation of concerns and testability
- Fastest time-to-market: 2-4 weeks to stable MVP with mature tooling
- Visual Studio designer with drag-and-drop UI development
- Rich NuGet ecosystem (MongoDB.Driver, OllamaSharp, PdfPig)
- Largest .NET developer pool for hiring and consultation
- Microsoft continues Windows 11 theming support and updates
- Proven crash prevention patterns (async/await, virtualization, global exception handling)

**Ecosystem Strengths**:
- MongoDB: Official MongoDB.Driver NuGet package with async/await and LINQ support
- OLLAMA: OllamaSharp NuGet package with idiomatic C# async methods
- PDF: Multiple options (PdfPig open-source, IronPDF commercial)
- JSON: Native System.Text.Json (high performance, modern API)
- HTTP: Native HttpClient with async support
- Testing: Gu.Wpf.UiAutomation, Coded UI, WinAppDriver for UI automation

**Why Chosen Over Alternatives**:
- **vs WinUI3**: WPF is production-stable; WinUI3 has "random crashes" and "bugs holding back adoption"
- **vs Qt**: WPF offers 2-3x faster development with equivalent stability
- **vs Win32**: WPF provides 3-5x faster development with modern architecture

**Implementation Pattern** (Crash Prevention):
```csharp
// Global exception handling
DispatcherUnhandledException += App_DispatcherUnhandledException;
AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

// Async operations for all I/O (prevents UI thread blocking)
private async Task ProcessPdfAsync(string filePath)
{
    await Task.Run(async () =>
    {
        var text = await ExtractTextAsync(filePath);
        await Dispatcher.InvokeAsync(() => DisplayText(text));
    });
}
```

---

#### C# with WinUI3 (Windows UI Library 3)

**Decision**: ❌ **NOT RECOMMENDED (Production Risk)**

**Issues Identified**:
- **Production Instability**: Developer reports indicate "random crashes," "bugs holding back adoption," "need for full regression tests on each upgrade"
- **Immature Tooling**: No Visual Studio UI designer (scheduled for Windows App SDK 1.7, no release date specified)
- **Feature Gaps**: "Significant XAML feature gap" compared to WPF; missing features discovered by UWP developers
- **Breaking Changes**: Frequent SDK updates break functionality, requiring extensive regression testing
- **Community Concerns**: GitHub discussions titled "WinUI3 is really dead!" reflect developer concerns about Microsoft's commitment
- **Risk Assessment**: Beta-quality stability unsuitable for stability-critical research application

**Why Not Chosen**:
- Directly contradicts core requirement: "eliminate crashes and freezes from Electron implementation"
- Production stability not yet proven at scale
- Tooling gaps would increase development time

**Consider If**: Modern Windows 11 UI design is critical AND stability concerns are resolved (revisit 2026-2027)

---

#### C++ with Qt

**Decision**: ⚠️ **SECONDARY RECOMMENDATION (If C++ Team)**

**Rationale**:
- 30 years of production use, excellent stability track record
- Cross-platform capability (Linux/macOS) if future expansion needed
- Qt Designer provides WYSIWYG UI development
- Mature ecosystem (networking, JSON parsing, PDF libraries available)
- Excellent documentation and large community

**Why Secondary Choice**:
- **Slower Development**: 2-3x longer than WPF for equivalent features ("tedious to write C++")
- **Compile Times**: Significantly slower than C# build/test cycle
- **Integration Complexity**:
  - MongoDB: mongo-cxx-driver requires manual C++17 setup (vs C# one-line NuGet)
  - OLLAMA: No dedicated SDK; requires custom HTTP + JSON integration
- **PDF Libraries**: Fewer mature options; primarily Poppler (GPL), MuPDF (AGPL), Aspose (commercial)
- **Testing**: No native UI automation comparable to C# options (no equivalent to Gu.Wpf.UiAutomation)

**Estimated Timeline**: 4-8 weeks to MVP (vs 2-4 weeks for WPF)

**Choose Qt If**: Team has strong C++ expertise AND maximum native performance is critical AND willing to invest 2-3x development time

---

#### C++ with Win32 API

**Decision**: ❌ **NOT RECOMMENDED (Productivity Loss)**

**Issues**:
- Extremely low-level: "Not easy to program/learn," massive boilerplate
- 12-20 weeks estimated development time (5-10x longer than WPF)
- Poor maintainability; modern frameworks exist for good reasons
- No UI designer; manual window creation and message handling

**Only Use For**: Low-level system utilities, not application-level development

---

## Decision 2: PDF Text Extraction Library

### Options Evaluated

#### C# Options

| Library | License | Type | Best For | Installation | Rating |
|---------|---------|------|----------|--------------|--------|
| **PdfPig** | Apache 2.0 | Open-source | Budget projects, standard PDFs | NuGet | ⭐⭐⭐⭐⭐ |
| **IronPDF** | Commercial | Proprietary | Commercial projects with support | NuGet | ⭐⭐⭐⭐⭐ |
| **Syncfusion** | Commercial/Free | Proprietary | Enterprise (free if <$1M revenue) | NuGet | ⭐⭐⭐⭐ |
| **iTextSharp** | AGPL/Commercial | Proprietary | Advanced PDF manipulation | NuGet | ⭐⭐⭐ (AGPL risk) |

**Decision**: ✅ **PdfPig for Budget Projects, IronPDF for Commercial**

**Rationale**:
- **PdfPig**:
  - Open-source Apache 2.0 license (no viral requirements)
  - Port of PDFBox (proven algorithm)
  - Good community support
  - Excellent for standard searchable PDFs
  - Free to use

- **IronPDF** (if budget allows):
  - Modern API with excellent documentation
  - Faster integration (less learning curve)
  - Professional support available
  - Handles edge cases (scanned PDFs, complex layouts)

**For This Project**: Recommend **PdfPig** as initial choice (open-source alignment, no licensing costs), with option to upgrade to **IronPDF** if edge cases (scanned PDFs, complex extraction) prove problematic.

#### C++ Options

| Library | License | Best For | Stability |
|---------|---------|----------|-----------|
| **MuPDF** | AGPL/Commercial | Performance | Excellent |
| **Poppler** | GPL | Cross-platform | Mature |
| **PDF-Writer (Hummus)** | MIT | Open-source | Good |
| **Aspose.PDF** | Commercial | Enterprise | Excellent |

**For C++ (if chosen)**: Use **PDF-Writer (Hummus)** for open-source MIT license or **MuPDF** for commercial use.

---

## Decision 3: Supporting Technology Stack

### JSON Handling

**Decision**: ✅ **System.Text.Json (C#) or simdjson (C++)**

**C# Choice**: `System.Text.Json`
- Native .NET library (no external dependency)
- High performance (4x faster than Newtonsoft.Json in benchmarks)
- Modern API aligned with .NET patterns
- Structured serialization for Data Types (estrutura.json schema)

**Alternative**: Newtonsoft.Json (Json.NET) if System.Text.Json missing required features

**C++ Choice**: `simdjson` (if Qt/C++ chosen)
- Fastest JSON parsing library (4x faster than RapidJSON)
- Used by Facebook/Meta at scale
- SIMD vectorization provides maximum performance

---

### MongoDB Integration

**Decision**: ✅ **Official MongoDB.Driver (C#) with async/await**

**C# Implementation**:
```csharp
// Installation: Install-Package MongoDB.Driver
var client = new MongoClient("mongodb+srv://username:password@cluster.mongodb.net/");
var database = client.GetDatabase("etnopapers");
var collection = database.GetCollection<ExtractedRecord>("records");

// Insert with async/await
await collection.InsertOneAsync(record);

// Query with LINQ
var records = await collection.AsQueryable()
    .Where(r => r.Year >= 2020)
    .ToListAsync();
```

**Features**:
- Strongly-typed API
- Async/await support prevents UI blocking
- Automatic connection pooling
- BSON serialization for estrutura.json compatibility
- Excellent documentation

---

### OLLAMA Integration

**Decision**: ✅ **OllamaSharp NuGet Package (C#)**

**Installation**: `Install-Package OllamaSharp`

**Usage Pattern**:
```csharp
var ollama = new OllamaApiClient(new Uri("http://localhost:11434"));
var response = await ollama.GetEmbeddingsAsync(
    new GetEmbeddingsRequest
    {
        Model = "mistral",
        Prompt = pdfText
    }
);
```

**Features**:
- C# idiomatic async API
- Streaming support for long responses
- Connection timeout handling
- Graceful fallback when OLLAMA unavailable

---

### Windows Installer Technology

**Decision**: ✅ **WiX Toolset 4.0**

**Rationale**:
- Creates professional MSI installers (enterprise-friendly)
- Supports complex installation scenarios (registry, shortcuts, uninstall)
- Windows Installer platform enables:
  - Network deployment
  - Automatic rollback on failure
  - Repair/modify installation options
  - Update/upgrade scenarios

**Alternatives Considered**:
- **NSIS**: Simpler but creates EXE (less enterprise-friendly)
- **Advanced Installer**: Commercial WiX alternative (adds cost)

**WiX 4.0 Improvements** (released 2024):
- Improved tooling and documentation
- Cleaner XML syntax
- Better Visual Studio integration

---

## Technology Stack Summary

### Recommended Stack

```
Platform:           Windows 10/11 (x86-64)
Language:           C# 12 (.NET 8 LTS)
UI Framework:       WPF (Windows Presentation Foundation)
Architecture:       MVVM (Model-View-ViewModel)
PDF Library:        PdfPig (open-source)
JSON Library:       System.Text.Json (native .NET)
MongoDB Driver:     MongoDB.Driver (official NuGet)
OLLAMA Client:      OllamaSharp (NuGet)
HTTP Client:        HttpClient (native .NET)
Testing:            xUnit + Gu.Wpf.UiAutomation
Installer:          WiX Toolset 4.0
IDE:                Visual Studio 2022 Community/Professional
```

### Alternative Stack (C++ with Qt)

```
Platform:           Windows 10/11 (x86-64)
Language:           C++ 17 or later
UI Framework:       Qt 6.7+ with Qt Designer
Architecture:       Model-View-Controller (MVC)
PDF Library:        PDF-Writer (Hummus) or MuPDF
JSON Library:       simdjson (performance) or nlohmann/json (ease-of-use)
MongoDB Driver:     mongo-cxx-driver
OLLAMA Client:      libcurl + custom JSON parsing
HTTP Client:        Qt Network or libcurl
Testing:            Google Test + Qt Test + WinAppDriver
Installer:          WiX Toolset or NSIS
IDE:                Qt Creator + Visual Studio 2022
```

---

## Implementation Timeline (C# WPF Recommended)

### Phase 1: Proof of Concept (1 week)
- Basic WPF window setup
- PDF upload and PdfPig integration demo
- Local JSON save/load
- OLLAMA connection test
- **Validates technical feasibility**

### Phase 2: Core Functionality (2-3 weeks)
- MVVM architecture implementation
- PDF processing pipeline with async/await
- OLLAMA integration (OllamaSharp)
- Local JSON CRUD interface
- MongoDB sync functionality
- Global exception handling for stability

### Phase 3: Polish & Installer (1 week)
- UI refinement and error messaging
- WiX installer creation
- Performance profiling and optimization
- Comprehensive testing

**Total Estimated Timeline**: 4-5 weeks to stable MVP

---

## Risk Assessment & Mitigation

### Technical Risks

| Risk | Impact | Mitigation | Owner |
|------|--------|-----------|-------|
| WPF performance with large record sets (1000+) | High | Use VirtualizingStackPanel for list views, implement pagination/filtering | Architecture review |
| OLLAMA unavailability during extraction | High | Graceful error handling, queue failed PDFs, show user-friendly error messages | Service layer design |
| MongoDB connection failures during sync | Medium | Implement retry logic with exponential backoff, preserve local state, allow manual retry | Sync service design |
| PDF extraction edge cases (scanned images, complex layouts) | Medium | Test with diverse PDFs, plan upgrade path to IronPDF if needed, document limitations | QA testing |

### Schedule Risks

| Risk | Impact | Mitigation |
|------|--------|-----------|
| Underestimated UI complexity | Medium | Start with Proof of Concept to validate assumptions |
| Unexpected WPF performance issues | Medium | Profile early and often; follow best practices from day one |
| Installer complexity | Low | Use WiX templates and examples; allocate full week for installer development |

---

## Conclusion

**Primary Technology Selection**: C# with WPF

This research strongly recommends C# with WPF as the primary technology for the EtnoPapers native Windows desktop migration because:

1. ✅ **Directly addresses stability requirement**: 19-year production track record with proven crash prevention patterns
2. ✅ **Fastest time-to-market**: 2-4 weeks to stable MVP vs 4-8 weeks (Qt) or 12-20 weeks (Win32)
3. ✅ **Excellent ecosystem fit**: Native NuGet packages for all dependencies (MongoDB, OLLAMA, PDF, JSON)
4. ✅ **Long-term maintainability**: Large developer community, established MVVM patterns, mature third-party ecosystem
5. ✅ **Professional deployment**: WiX Toolset creates enterprise-grade MSI installers
6. ✅ **Best testing infrastructure**: Comprehensive UI automation frameworks

**Avoid WinUI3** despite modern appeal due to production instability, breaking changes, and missing features.

**Secondary Option: Qt** only if team has strong C++ expertise and can justify 2-3x development time for maximum native performance.

---

## References

### Technology Comparisons
- Claudio Bernasconi: What User Interface Framework Should You Choose for .NET Desktop Applications?
- ComponentOne: WinUI vs WPF, WinForms, UWP, and MFC
- Visual Studio Magazine: Choosing the Right UI Framework for Native Windows Applications
- Stack Overflow: C++ Qt vs C# .NET for Windows Development

### Library Documentation
- MongoDB C# Driver: https://www.mongodb.com/docs/drivers/csharp/
- OllamaSharp: https://github.com/awaescher/OllamaSharp
- PdfPig: https://uglytoad.github.io/PdfPig/
- WiX Toolset: https://wixtoolset.org/

### Best Practices
- PostSharp: 10 WPF Best Practices
- Microsoft Learn: Handle Exceptions in WPF Applications
- Microsoft Learn: WPF Application Architecture

