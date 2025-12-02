# Implementation Plan: Migrate to Native Windows Desktop Application

**Branch**: `002-native-windows-migration` | **Date**: 2025-12-02 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `specs/002-native-windows-migration/spec.md`

## Summary

Migrate EtnoPapers from Electron to a native Windows desktop application to resolve stability and responsiveness issues while preserving core functionality (PDF extraction via OLLAMA, record management, MongoDB sync). The implementation will use a modern native Windows framework (C# with WPF/WinUI3, or C++ with Win32/Qt) selected based on stability requirements, development timeline, and maintainability. Priority focus: eliminate crashes and freezes from the current Electron implementation.

## Technical Context

**Language/Version**: [NEEDS CLARIFICATION: C# 11+ with .NET 7/8, or C++ 17+]
**Primary Dependencies**: [NEEDS CLARIFICATION: C#: WPF or WinUI3 (UI), System.Net.Http (networking), System.Text.Json (serialization) | C++: Win32 API or Qt 6 (UI), cURL/libcurl (networking)]
**Storage**: Local JSON files (restructured from docs/estrutura.json schema) + MongoDB Atlas/local (optional)
**Testing**: xUnit/NUnit (C#) or Google Test/Catch2 (C++) for unit tests; Selenium/WinAppDriver for UI automation
**Target Platform**: Windows 10/11 (x86-64), .NET 7/8 runtime or standalone native executable
**Project Type**: Desktop application (single installer, no web/mobile components)
**Performance Goals**: UI responsiveness <100ms, PDF extraction <5 minutes (typical 10-page document), app startup <3 seconds, handle 1000+ records locally
**Constraints**: Offline-capable, self-contained (no Node.js/Electron runtime), integrate with local OLLAMA on configurable host/port, graceful handling of missing/unavailable dependencies (OLLAMA, MongoDB)
**Scale/Scope**: Single-window application, ~5 main views (Dashboard, Record Browser, Record Editor, Settings, Sync Status), ~1500-2500 lines of UI code plus libraries

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

**Constitution Status**: Template constitution in place; no specific project-level constraints preventing desktop application development. Core principles (test-first, simplicity, stability) align with migration goals.

**Key Gates**:
1. ✅ **Stability First**: Native Windows implementation MUST resolve Electron crashes/freezes (core requirement)
2. ✅ **Maintainability**: Technology choice must balance rapid development with long-term maintainability
3. ✅ **Feature Parity**: Redesign UX while preserving all core functionality from spec
4. ✅ **Offline Capability**: Application must work without MongoDB; sync is optional enhancement
5. ⚠️ **Technology Decision**: C# vs C++ decision deferred to Phase 0 research; must be resolved before Phase 1 begins

## Project Structure

### Documentation (this feature)

```text
specs/[###-feature]/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

**Option: Native Windows Desktop Application (selected)**

```text
etnopapers/                          # Root project directory
├── src/
│   ├── EtnoPapers.Core/             # Core business logic (language-agnostic)
│   │   ├── Models/                  # Data classes (ExtractedRecord, Configuration)
│   │   ├── Services/                # Business services (PDFProcessor, OLLAMAService, MongoDBService)
│   │   ├── Interfaces/              # Service interfaces for testing
│   │   └── Schema/                  # estrutura.json schema validation
│   │
│   └── EtnoPapers.Desktop/          # Windows desktop UI application
│       ├── Views/                   # XAML/UI definition files
│       │   ├── MainWindow
│       │   ├── RecordBrowserView
│       │   ├── RecordEditorView
│       │   ├── SettingsView
│       │   └── SyncProgressView
│       ├── ViewModels/              # MVVM ViewModels (UI logic, binding)
│       ├── Services/                # Desktop-specific services (file dialogs, system notifications)
│       └── Resources/               # Icons, styles, configuration templates
│
├── tests/
│   ├── EtnoPapers.Core.Tests/       # Unit tests for business logic
│   │   ├── Services/
│   │   ├── Models/
│   │   └── Schema/
│   │
│   └── EtnoPapers.Desktop.Tests/    # UI automation tests (WinAppDriver/Selenium)
│       ├── UIWorkflows/
│       └── Integration/
│
├── docs/
│   ├── estrutura.json               # Data schema definition
│   └── [existing documentation]
│
├── installation/                    # Windows installer configuration (WiX/NSIS)
│   └── etnopapers-installer.wxs
│
└── [solution files: EtnoPapers.sln, etc.]
```

**Structure Decision**: Separated Core (business logic) from Desktop (UI) to enable testing, potential future platforms, and clean architecture. Desktop uses MVVM pattern for C# or native patterns for C++. Phase 0 research will determine final technology choice and refine structure accordingly.

## Complexity Tracking

| Design Decision | Justification | Simpler Alternative & Why Rejected |
|-----------------|---------------|-----------------------------------|
| Separate Core/Desktop projects | Enables testing without UI framework, supports future platforms | Monolithic approach: harder to test business logic, ties UI changes to service logic |
| MVVM pattern (C#) or MVC (C++) | Industry standard for responsive desktop apps, decouples UI from logic | Direct code-behind: UI and business logic entangled, harder to test |
| Local JSON storage + MongoDB sync | Supports offline work while enabling cloud backup | Cloud-only: blocks offline research; local-only: prevents multi-machine sync |
