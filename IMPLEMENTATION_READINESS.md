# EtnoPapers WPF Migration - Implementation Readiness Guide

**Status**: ✅ Ready for Phase 0 (Project Setup & Infrastructure)
**Date**: 2025-12-06
**Total Tasks**: 100 (organized in 9 phases)
**Quality Score**: 95/100

---

## Current Status Summary

### ✅ Completed Steps

1. **Specification (✅ Complete)**
   - Feature specification in `specs/main/spec.md`
   - 4 User Stories (P1-P2 priority)
   - 40 Functional Requirements (FR-001 to FR-032)
   - 10 Success Criteria (SC-001 to SC-010)
   - Edge cases documented and handled

2. **Planning (✅ Complete)**
   - Technical implementation plan in `specs/main/plan.md`
   - Architecture decisions documented
   - Data model design in `specs/main/data-model.md`
   - Service contracts in `specs/main/contracts/`
   - Technology stack finalized (C# 12, .NET 8, WPF)

3. **Task Breakdown (✅ Complete)**
   - 100 tasks organized in 9 phases
   - All tasks have acceptance criteria
   - Dependencies mapped and validated
   - Parallel execution markers assigned

4. **Analysis & Remediation (✅ Complete)**
   - Specification analysis completed (82/100 → 95/100)
   - 4 HIGH-severity issues resolved:
     - Constitution populated with actual principles
     - PDF→Markdown requirement clarified (not optional)
     - Startup metrics precisely defined
     - Edge cases (T092-T094) added
   - All artifacts consistent and validated

5. **Constitution (✅ Established)**
   - 6 core principles defined (non-negotiable)
   - Phase gates documented
   - Single-branch workflow formalized
   - Performance targets confirmed

### ⏳ Next Phase: Phase 0 - Project Setup & Infrastructure

---

## Prerequisites for Starting Implementation

### System Requirements

- **Visual Studio 2022** (Community, Professional, or Enterprise)
  - With C# workload installed
  - .NET 8.0 SDK installed (check: `dotnet --version`)
  - NuGet package manager available

- **.NET 8.0 LTS** (must be installed)
  ```bash
  dotnet --version
  # Should show: 8.0.x or higher
  ```

- **Git** (already in use)
  - Current branch: `main`
  - Remote: `origin`

- **PowerShell 5.0+** (for build scripts)

### Verify Prerequisites

```bash
# Check all prerequisites
.specify/scripts/powershell/check-prerequisites.ps1 -Json -RequireTasks -IncludeTasks

# Verify .NET 8.0
dotnet --version

# Verify git setup
git status
```

---

## How to Start Phase 0 (First Day of Implementation)

### Step 1: Review the Specification & Plan

Read the complete context before starting:

```bash
# Open documentation
code specs/main/spec.md          # User stories, requirements
code specs/main/plan.md          # Technical architecture
code .specify/memory/constitution.md  # Project principles
```

**Key points to understand**:
- ✅ 100% feature parity with Electron version required
- ✅ Native Windows design (WPF controls, not web emulation)
- ✅ Performance targets: startup < 2s, memory < 150MB
- ✅ PDF → Markdown conversion (PdfPig) is critical for reducing AI hallucinations
- ✅ Single-branch workflow (all commits to `main`)

### Step 2: Execute Phase 0 Tasks (Tasks T001-T011)

**Duration**: ~1-2 hours
**Tasks**: 11 tasks (setup and infrastructure)

#### T001: Create Visual Studio Solution

```bash
dotnet new sln -n EtnoPapers
```

**Acceptance**: Solution file created, can open in Visual Studio 2022

#### T002-T004: Create Project Structure

```bash
# Core library
dotnet new classlib -n EtnoPapers.Core -f net8.0 -o src/EtnoPapers.Core

# WPF UI
dotnet new wpf -n EtnoPapers.UI -f net8.0 -o src/EtnoPapers.UI

# Test project
dotnet new xunit -n EtnoPapers.Core.Tests -f net8.0 -o tests/EtnoPapers.Core.Tests
```

#### T005-T008: Install NuGet Dependencies

```bash
# Core services
dotnet add src/EtnoPapers.Core package MongoDB.Driver
dotnet add src/EtnoPapers.Core package Newtonsoft.Json
dotnet add src/EtnoPapers.Core package Serilog
dotnet add src/EtnoPapers.Core package Serilog.Sinks.File
dotnet add src/EtnoPapers.Core package PdfPig  # Critical: PDF extraction

# UI
dotnet add src/EtnoPapers.UI package Newtonsoft.Json

# Testing
dotnet add tests/EtnoPapers.Core.Tests package Moq
dotnet add tests/EtnoPapers.Core.Tests package FluentAssertions
```

#### T009-T011: Directory Structure & Build Configuration

```bash
# Create directories
mkdir -p src/EtnoPapers.Core/{Services,Models,Utils,Validation}
mkdir -p src/EtnoPapers.UI/{Views,ViewModels,Controls,Converters,Localization}
mkdir -p tests/EtnoPapers.Core.Tests/{Services,Utils}

# Add to solution
dotnet sln EtnoPapers.sln add src/EtnoPapers.Core/EtnoPapers.Core.csproj
dotnet sln EtnoPapers.sln add src/EtnoPapers.UI/EtnoPapers.UI.csproj
dotnet sln EtnoPapers.sln add tests/EtnoPapers.Core.Tests/EtnoPapers.Core.Tests.csproj

# Test the build
dotnet build EtnoPapers.sln
```

### Step 3: Verify Phase 0 Completion

```bash
# Should complete without errors
dotnet build EtnoPapers.sln

# Run tests (should show 0 tests, but no errors)
dotnet test tests/EtnoPapers.Core.Tests/EtnoPapers.Core.Tests.csproj
```

### Step 4: Commit Phase 0 Results

```bash
git add .
git commit -m "feat: Phase 0 - Project setup and infrastructure

- Created Visual Studio solution with 3 projects
- EtnoPapers.Core (business logic)
- EtnoPapers.UI (WPF application)
- EtnoPapers.Core.Tests (unit tests)
- Installed all required NuGet dependencies
- Created directory structure per implementation plan
- Solution builds and tests run successfully

Phase 0 Gate: PASSED
Ready for Phase 1 (Core Services & Data Layer)"
```

---

## Phase 1: Core Services & Data Layer (Day 2+)

### Overview

**Duration**: ~3-5 days
**Tasks**: 21 tasks (T012-T034, including T092-T094)
**Focus**: Implement all services without UI

### Key Components to Implement

1. **Data Models** (T012-T015)
   - ArticleRecord
   - PlantSpecies
   - Community
   - Configuration

2. **Markdown Conversion Pipeline** (T023a-d, T024-T025)
   - MarkdownConverter (PdfPig integration) ← **Critical**
   - PDFProcessingService
   - OLLAMAService with Markdown-optimized prompts

3. **Data Storage** (T016-T023)
   - DataStorageService
   - JSON serialization
   - Compatibility with Electron format

4. **Edge Case Handling** (T092-T094) ← **NEW - High Priority**
   - ConfigurationMigrationService (schema migration)
   - ApplicationLockService (simultaneous version detection)
   - MigrationCompletionService (incomplete migration detection)

### Execution Order for Phase 1

```
Models (T012-T015) ────┐
                       ├─→ Serialization (T016-T017) ────┐
Utilities (T018-T021) ──→ Storage (T023) ──────┐         │
                                                ├─→ Extraction Pipeline (T028)
PdfPig Integration ───→ MarkdownConverter (T023a-d) ──┤
(T006 dependency)       PDFProcessingService (T024) ────┤
                                                ├─→ (Use in Phase 3)
                        OLLAMAService (T025) ──┘

Edge Cases (T092-T094) → Can start after DataStorageService (T023)
```

### Start Phase 1

Once Phase 0 is complete:

```bash
# Open the specification for Phase 1 details
code specs/main/tasks.md +104

# Start with T012-T015 (data models)
# Then T023a-d (Markdown pipeline - critical)
# Then T092-T094 (edge case handling)
# Then remaining services
```

---

## Documentation Reference

### Key Files

| File | Purpose |
|------|---------|
| `specs/main/spec.md` | Feature specification with requirements |
| `specs/main/plan.md` | Technical architecture and design |
| `specs/main/tasks.md` | 100 implementation tasks (this is the source of truth) |
| `specs/main/data-model.md` | JSON schema and entity definitions |
| `specs/main/research.md` | Technology research and justifications |
| `.specify/memory/constitution.md` | Project principles and gates |
| `CLAUDE.md` | Project-specific development guidance |

### Important Decisions Documented

1. **PDF → Markdown Layer** (specs/main/plan.md L290-421)
   - Uses PdfPig library (MIT license)
   - Reduces OLLAMA hallucinations from 40%+ to <10%
   - Critical for WPF version success

2. **Performance Targets** (specs/main/plan.md L47-50)
   - Startup < 2 seconds (measured with System.Diagnostics.Stopwatch)
   - Idle memory < 150MB (measured after 30 seconds)
   - UI responsiveness < 200ms (record operations)

3. **Edge Cases** (specs/main/spec.md L80-85 + tasks T092-T094)
   - Schema migration with unknown field preservation
   - Simultaneous version detection (Electron + WPF running together)
   - Incomplete migration detection with rollback

---

## Daily Workflow During Implementation

### Morning: Plan the Day

```bash
# 1. Check specification for today's tasks
code specs/main/tasks.md

# 2. Review task acceptance criteria
# 3. Identify which tasks can run in parallel
# 4. Plan for dependencies
```

### During Development

```bash
# Build frequently to catch errors early
dotnet build EtnoPapers.sln

# Run tests after implementing services
dotnet test tests/EtnoPapers.Core.Tests/EtnoPapers.Core.Tests.csproj

# View git status
git status

# Create feature commits (all to main)
git add .
git commit -m "feat: Implement [TaskID] [TaskName]"
```

### End of Day: Update Progress

```bash
# Mark completed tasks in specs/main/tasks.md
# Example: Change "- [ ] T012" to "- [x] T012"

# Commit work
git add specs/main/tasks.md
git commit -m "chore: Mark T012-T015 as completed"

# Push to remote (if desired)
git push
```

---

## Running All 100 Tasks to Completion

### Phase Breakdown

| Phase | Tasks | Duration | Focus |
|-------|-------|----------|-------|
| **0** | T001-T011 | 1-2 hours | Setup & infrastructure |
| **1** | T012-T094 | 3-5 days | Core services + edge cases |
| **2** | T030-T040 | 2-3 days | WPF UI foundation |
| **3** | T041-T049 | 2-3 days | PDF upload workflow |
| **4** | T050-T057 | 2-3 days | Record management UI |
| **5** | T058-T062 | 1-2 days | MongoDB sync |
| **6** | T063-T074 | 2-3 days | Settings & configuration |
| **7** | T075-T083 | 3-4 days | Testing & performance |
| **8** | T084-T091 | 2-3 days | Installer & release |
| **9** | T095-T100 | 1-2 days | Launch & documentation |

**Total Estimated Duration**: 8-16 weeks (1-2 developers)

---

## Troubleshooting

### If Build Fails

```bash
# Clean and rebuild
dotnet clean EtnoPapers.sln
dotnet build EtnoPapers.sln --verbose

# Check .NET version
dotnet --version  # Must be 8.0+

# Restore packages
dotnet restore EtnoPapers.sln
```

### If Tests Fail

```bash
# Run specific test with detailed output
dotnet test tests/EtnoPapers.Core.Tests/EtnoPapers.Core.Tests.csproj -v d

# Check for missing dependencies
dotnet add tests/EtnoPapers.Core.Tests package [PackageName]
```

### If Git Commits Fail

```bash
# Verify you're on main branch
git status

# Check git config
git config --global user.name
git config --global user.email

# Set config if needed
git config --global user.name "Your Name"
git config --global user.email "your.email@example.com"
```

---

## Success Criteria for Starting Tomorrow

Before starting Phase 0 tomorrow, verify:

- ✅ Visual Studio 2022 is installed
- ✅ .NET 8.0 SDK is installed (`dotnet --version`)
- ✅ Git is configured with user name and email
- ✅ You can read `specs/main/spec.md` and understand the 4 user stories
- ✅ You understand the PDF → Markdown → OLLAMA pipeline is critical
- ✅ You have this file (`IMPLEMENTATION_READINESS.md`) as a reference

---

## Commands to Copy Tomorrow

```bash
# Phase 0 Setup (Day 1)
cd H:\git\etnopapers

# Create solution
dotnet new sln -n EtnoPapers

# Create projects
dotnet new classlib -n EtnoPapers.Core -f net8.0 -o src/EtnoPapers.Core
dotnet new wpf -n EtnoPapers.UI -f net8.0 -o src/EtnoPapers.UI
dotnet new xunit -n EtnoPapers.Core.Tests -f net8.0 -o tests/EtnoPapers.Core.Tests

# Install dependencies
dotnet add src/EtnoPapers.Core package MongoDB.Driver
dotnet add src/EtnoPapers.Core package Newtonsoft.Json
dotnet add src/EtnoPapers.Core package Serilog
dotnet add src/EtnoPapers.Core package Serilog.Sinks.File
dotnet add src/EtnoPapers.Core package PdfPig
dotnet add src/EtnoPapers.UI package Newtonsoft.Json
dotnet add tests/EtnoPapers.Core.Tests package Moq
dotnet add tests/EtnoPapers.Core.Tests package FluentAssertions

# Create directories
mkdir -p src/EtnoPapers.Core/{Services,Models,Utils,Validation}
mkdir -p src/EtnoPapers.UI/{Views,ViewModels,Controls,Converters,Localization}
mkdir -p tests/EtnoPapers.Core.Tests/{Services,Utils}

# Add to solution
dotnet sln EtnoPapers.sln add src/EtnoPapers.Core/EtnoPapers.Core.csproj
dotnet sln EtnoPapers.sln add src/EtnoPapers.UI/EtnoPapers.UI.csproj
dotnet sln EtnoPapers.sln add tests/EtnoPapers.Core.Tests/EtnoPapers.Core.Tests.csproj

# Verify setup
dotnet build EtnoPapers.sln
dotnet test tests/EtnoPapers.Core.Tests/EtnoPapers.Core.Tests.csproj

# Commit
git add .
git commit -m "feat: Phase 0 - Project setup and infrastructure"
```

---

## Contact & Support

For clarification on requirements:
- Specification details: See `specs/main/spec.md`
- Technical decisions: See `specs/main/plan.md`
- Specific task requirements: See `specs/main/tasks.md` (search for task ID)
- Project principles: See `.specify/memory/constitution.md`

---

**Document Date**: 2025-12-06
**Last Updated**: 2025-12-06
**Ready Status**: ✅ YES - Ready to begin Phase 0 tomorrow
