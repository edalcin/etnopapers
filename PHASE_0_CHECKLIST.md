# Phase 0 - Project Setup & Infrastructure Checklist

**Estimated Time**: 1-2 hours
**Status**: Ready to execute
**Date**: 2025-12-06

---

## Pre-Implementation Checklist

Before starting, verify your environment:

### Development Environment

- [ ] Visual Studio 2022 installed (Community or higher)
- [ ] C# workload installed in Visual Studio
- [ ] .NET 8.0 SDK installed
  ```bash
  dotnet --version
  # Output should be: 8.0.x or higher
  ```
- [ ] Git configured
  ```bash
  git config --global user.name "Your Name"
  git config --global user.email "your.email@example.com"
  ```
- [ ] PowerShell 5.0+ available
- [ ] Current working directory: `H:\git\etnopapers`

### Documentation Review

- [ ] Read `specs/main/spec.md` (user stories)
- [ ] Read `specs/main/plan.md` (technical architecture)
- [ ] Read `.specify/memory/constitution.md` (project principles)
- [ ] Understand the 4 user stories (US1-US4)
- [ ] Understand performance targets (startup < 2s, memory < 150MB)

### Git Status

- [ ] On `main` branch
  ```bash
  git branch
  # Should show: * main
  ```
- [ ] Working directory clean
  ```bash
  git status
  # Should show: nothing to commit, working tree clean
  ```

---

## Phase 0 Task Execution Checklist

### Task T001: Create Visual Studio Solution

**File**: `EtnoPapers.sln`

**Commands**:
```bash
dotnet new sln -n EtnoPapers
```

**Acceptance Criteria**:
- [ ] Solution file created (`EtnoPapers.sln`)
- [ ] Can open in Visual Studio 2022
- [ ] Solution builds without errors

**Verification**:
```bash
ls -la EtnoPapers.sln
```

---

### Tasks T002-T004: Create Projects

#### T002: Create EtnoPapers.Core class library

**File**: `src/EtnoPapers.Core/EtnoPapers.Core.csproj`

**Commands**:
```bash
dotnet new classlib -n EtnoPapers.Core -f net8.0 -o src/EtnoPapers.Core
```

**Acceptance Criteria**:
- [ ] Project created in `src/EtnoPapers.Core/`
- [ ] Targets .NET 8.0
- [ ] .csproj file is valid

**Verification**:
```bash
ls -la src/EtnoPapers.Core/EtnoPapers.Core.csproj
```

#### T003: Create EtnoPapers.UI WPF project

**File**: `src/EtnoPapers.UI/EtnoPapers.UI.csproj`

**Commands**:
```bash
dotnet new wpf -n EtnoPapers.UI -f net8.0 -o src/EtnoPapers.UI
```

**Acceptance Criteria**:
- [ ] WPF project created in `src/EtnoPapers.UI/`
- [ ] Targets .NET 8.0
- [ ] App.xaml and MainWindow.xaml generated

**Verification**:
```bash
ls src/EtnoPapers.UI/App.xaml
ls src/EtnoPapers.UI/MainWindow.xaml
```

#### T004: Create EtnoPapers.Core.Tests test project

**File**: `tests/EtnoPapers.Core.Tests/EtnoPapers.Core.Tests.csproj`

**Commands**:
```bash
dotnet new xunit -n EtnoPapers.Core.Tests -f net8.0 -o tests/EtnoPapers.Core.Tests
```

**Acceptance Criteria**:
- [ ] xUnit test project created in `tests/EtnoPapers.Core.Tests/`
- [ ] Targets .NET 8.0
- [ ] UnitTest1.cs template exists

**Verification**:
```bash
ls tests/EtnoPapers.Core.Tests/EtnoPapers.Core.Tests.csproj
```

---

### Tasks T005-T008: Install NuGet Dependencies

#### T005: Install core NuGet packages

**File**: `src/EtnoPapers.Core/EtnoPapers.Core.csproj`

**Commands**:
```bash
dotnet add src/EtnoPapers.Core package MongoDB.Driver
dotnet add src/EtnoPapers.Core package Newtonsoft.Json
dotnet add src/EtnoPapers.Core package Serilog
dotnet add src/EtnoPapers.Core package Serilog.Sinks.File
```

**Acceptance Criteria**:
- [ ] MongoDB.Driver installed
- [ ] Newtonsoft.Json installed
- [ ] Serilog installed
- [ ] Serilog.Sinks.File installed
- [ ] .csproj contains `<PackageReference>` entries

**Verification**:
```bash
grep -i "PackageReference" src/EtnoPapers.Core/EtnoPapers.Core.csproj
```

#### T006: Install PdfPig library

**File**: `src/EtnoPapers.Core/EtnoPapers.Core.csproj`

**Commands**:
```bash
dotnet add src/EtnoPapers.Core package PdfPig
```

**Acceptance Criteria**:
- [ ] PdfPig library installed (latest stable version)
- [ ] Package reference appears in .csproj
- [ ] Will be used for PDF → Markdown conversion (T023a-d)

**Verification**:
```bash
grep "PdfPig" src/EtnoPapers.Core/EtnoPapers.Core.csproj
```

#### T007: Install WPF UI dependencies

**File**: `src/EtnoPapers.UI/EtnoPapers.UI.csproj`

**Commands**:
```bash
dotnet add src/EtnoPapers.UI package Newtonsoft.Json
```

**Acceptance Criteria**:
- [ ] Newtonsoft.Json installed for UI project
- [ ] UI project can serialize/deserialize JSON

**Verification**:
```bash
grep "Newtonsoft.Json" src/EtnoPapers.UI/EtnoPapers.UI.csproj
```

#### T008: Install testing dependencies

**File**: `tests/EtnoPapers.Core.Tests/EtnoPapers.Core.Tests.csproj`

**Commands**:
```bash
dotnet add tests/EtnoPapers.Core.Tests package Moq
dotnet add tests/EtnoPapers.Core.Tests package FluentAssertions
```

**Acceptance Criteria**:
- [ ] Moq installed (for mocking)
- [ ] FluentAssertions installed (for fluent test assertions)
- [ ] Test project can use both libraries

**Verification**:
```bash
grep -E "Moq|FluentAssertions" tests/EtnoPapers.Core.Tests/EtnoPapers.Core.Tests.csproj
```

---

### Task T009: Create Directory Structure

**Files**: Various directories under `src/` and `tests/`

**Commands**:
```bash
mkdir -p src/EtnoPapers.Core/{Services,Models,Utils,Validation}
mkdir -p src/EtnoPapers.UI/{Views,ViewModels,Controls,Converters,Localization}
mkdir -p tests/EtnoPapers.Core.Tests/{Services,Utils}
```

**Acceptance Criteria**:
- [ ] `src/EtnoPapers.Core/Services/` directory created
- [ ] `src/EtnoPapers.Core/Models/` directory created
- [ ] `src/EtnoPapers.Core/Utils/` directory created
- [ ] `src/EtnoPapers.Core/Validation/` directory created
- [ ] `src/EtnoPapers.UI/Views/` directory created
- [ ] `src/EtnoPapers.UI/ViewModels/` directory created
- [ ] `src/EtnoPapers.UI/Controls/` directory created
- [ ] `src/EtnoPapers.UI/Converters/` directory created
- [ ] `src/EtnoPapers.UI/Localization/` directory created
- [ ] `tests/EtnoPapers.Core.Tests/Services/` directory created
- [ ] `tests/EtnoPapers.Core.Tests/Utils/` directory created

**Verification**:
```bash
tree src/
tree tests/
```

---

### Task T010: Create .gitignore

**File**: `.gitignore`

**Content** (if not already present):
```
# Build results
bin/
obj/
*.exe
*.dll
*.pdb

# Visual Studio
.vs/
*.sln.docstates
*.user
*.userosscache
*.suo
*.VC.db

# NuGet
packages/
.nuget/

# IDE
.vscode/
.idea/

# OS
.DS_Store
Thumbs.db

# Application specific
logs/
data.json
config.json
```

**Acceptance Criteria**:
- [ ] `.gitignore` created or updated
- [ ] Ignores bin/, obj/, .vs/, *.exe, *.dll, packages/

**Verification**:
```bash
cat .gitignore | head -10
```

---

### Task T011: Create Build Script

**File**: `build.ps1` or `Makefile`

**PowerShell Script** (`build.ps1`):
```powershell
#!/usr/bin/env pwsh

Write-Host "Building EtnoPapers solution..." -ForegroundColor Cyan

# Build
Write-Host "Running dotnet build..." -ForegroundColor Yellow
dotnet build EtnoPapers.sln
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

# Test
Write-Host "Running tests..." -ForegroundColor Yellow
dotnet test tests/EtnoPapers.Core.Tests/EtnoPapers.Core.Tests.csproj --verbosity minimal
if ($LASTEXITCODE -ne 0) {
    Write-Host "Tests failed!" -ForegroundColor Red
    exit 1
}

Write-Host "Build successful!" -ForegroundColor Green
```

**Acceptance Criteria**:
- [ ] Build script created
- [ ] Script builds solution
- [ ] Script runs tests
- [ ] Script reports success/failure

**Verification**:
```bash
./build.ps1
# Should output: "Build successful!"
```

---

## Solution Assembly Checklist

### Add Projects to Solution

**Commands**:
```bash
dotnet sln EtnoPapers.sln add src/EtnoPapers.Core/EtnoPapers.Core.csproj
dotnet sln EtnoPapers.sln add src/EtnoPapers.UI/EtnoPapers.UI.csproj
dotnet sln EtnoPapers.sln add tests/EtnoPapers.Core.Tests/EtnoPapers.Core.Tests.csproj
```

**Verification**:
```bash
dotnet sln EtnoPapers.sln list
# Should show 3 projects
```

---

## Final Verification - Build & Test

### Full Build

- [ ] Solution builds without errors
  ```bash
  dotnet build EtnoPapers.sln
  # Should end with: Build succeeded.
  ```

### Run Tests

- [ ] Tests execute (may show 0 tests at this point)
  ```bash
  dotnet test tests/EtnoPapers.Core.Tests/EtnoPapers.Core.Tests.csproj
  # Should show: 0 passed, no failures
  ```

### Visual Studio Verification

- [ ] Open solution in Visual Studio 2022
  ```bash
  # Or use: Start EtnoPapers.sln
  start EtnoPapers.sln
  ```
- [ ] All 3 projects visible in Solution Explorer
- [ ] No red squiggles or errors

---

## Git Commit Checklist

### Stage Changes

```bash
git status
# Should show modified/new files
```

**Files to stage**:
- [ ] `EtnoPapers.sln`
- [ ] `src/EtnoPapers.Core/`
- [ ] `src/EtnoPapers.UI/`
- [ ] `tests/EtnoPapers.Core.Tests/`
- [ ] `build.ps1` (or build script)
- [ ] `.gitignore` (if modified)

**Commands**:
```bash
git add EtnoPapers.sln src/ tests/ build.ps1 .gitignore
git status
# Should show "Changes to be committed"
```

### Create Commit

```bash
git commit -m "feat: Phase 0 - Project setup and infrastructure

- Created Visual Studio solution with 3 projects
- EtnoPapers.Core (business logic, C# class library)
- EtnoPapers.UI (WPF application)
- EtnoPapers.Core.Tests (xUnit test project)
- Installed all required NuGet dependencies:
  * MongoDB.Driver (MongoDB integration)
  * Newtonsoft.Json (JSON serialization)
  * Serilog (logging)
  * PdfPig (PDF extraction for Markdown conversion)
  * Moq, FluentAssertions (testing)
- Created directory structure per implementation plan
- Created build script for automated builds
- All projects target .NET 8.0 LTS

Phase 0 Gate: PASSED
- Solution builds without errors
- Tests run without failures (0 tests at this stage)
- Ready for Phase 1 (Core Services & Data Layer)

Next: T012-T015 (Data models), T023a-d (Markdown pipeline)"
```

### Verify Commit

```bash
git log --oneline -1
# Should show Phase 0 commit

git status
# Should show: On branch main, nothing to commit
```

---

## Phase 0 Completion

When all checkboxes are ✅:

- [ ] All 11 tasks (T001-T011) completed
- [ ] Solution builds successfully
- [ ] Tests run without errors
- [ ] All files committed to main branch
- [ ] No uncommitted changes

**Status**: ✅ Phase 0 COMPLETE - Ready for Phase 1

---

## Next Steps After Phase 0

1. Review Phase 1 tasks in `specs/main/tasks.md` (lines 104-251)
2. Start with T012-T015 (data models)
3. Then implement T023a-d (Markdown conversion pipeline - critical)
4. Then T092-T094 (edge case handling)

See `IMPLEMENTATION_READINESS.md` for complete Phase 1 instructions.

---

**Checklist Created**: 2025-12-06
**Phase 0 Duration**: ~1-2 hours
**Status**: Ready to execute
