# EtnoPapers Project Constitution

**Version**: 1.0.0 | **Ratified**: 2025-12-06 | **Last Amended**: 2025-12-06

## Core Principles

### I. Functional Parity (NON-NEGOTIABLE)
100% feature compatibility with the Electron version is mandatory. All user workflows, data formats, and AI integration must be identical between Electron and WPF versions. Zero data loss is required during migration. Users must be able to seamlessly transition between versions without friction or capability loss.

**GATE**: Every feature must have acceptance criteria mapping to Electron equivalence. Any deviation requires explicit justification in plan.md complexity tracking.

### II. Native Windows Design (NON-NEGOTIABLE)
The WPF application MUST use native Windows controls throughout—no web-like emulation or Electron-derived UI patterns. Windows 11 design language compliance is mandatory. Standard Windows keyboard shortcuts (Ctrl+S, Ctrl+Q, Alt+F4, Tab navigation) MUST work intuitively. Windows file dialogs, drag-and-drop, and notifications MUST integrate seamlessly.

**GATE**: UI review must verify native control usage; non-native patterns require architecture review.

### III. Performance First (HARD REQUIREMENT)
Performance targets are non-negotiable and define success for this migration:
- **Startup** < 2 seconds (vs. Electron ~5-10 seconds)
- **Idle Memory** < 150 MB (vs. Electron ~300-500 MB)
- **UI Responsiveness** < 200ms for record operations
These metrics MUST be measured and validated before release. Deviations require root cause analysis and explicit acceptance.

**GATE**: Benchmarks (T070, T071, T072) must pass before Phase 7 completion.

### IV. Data Integrity (NON-NEGOTIABLE)
JSON file format MUST remain identical to Electron version. MongoDB documents MUST be unchanged. Configuration compatibility MUST be maintained across versions. Migration path MUST be documented and tested. Unknown fields in JSON MUST be preserved (forward/backward compatibility).

**GATE**: Data serialization tests (T016) MUST validate round-trip compatibility.

### V. Simplicity & Maintainability
Standard WPF patterns and best practices MUST be followed. MVVM architectural pattern is required (standard WPF). Clear separation of concerns (Data Layer, Service Layer, UI Layer) is mandatory. Complexity introduced during migration MUST be justified in plan.md and marked as accepted technical debt.

**GATE**: Architecture review before Phase 1; complexity justification required in plan.md.

### VI. Quality Assurance (NON-NEGOTIABLE)
Unit tests for business logic are mandatory. Integration tests for data layer and MongoDB are required. Manual acceptance testing for UI and workflows is required. Performance benchmarking MUST occur before release.

**GATE**: No task completion without tests (where applicable per tech stack). T070/T071/T072 benchmarks must validate SC-002, SC-003.

## Single-Branch Workflow (Project-Specific Deviation)

**DEVIATION FROM SPECKIT**: This project uses a single `main` branch instead of the SpecKit-standard numbered feature branches (`###-feature-name`).

**Rationale**:
- Windows desktop application maintained by single owner
- Single-branch simplifies deployment and release process
- Eliminates branch merge complexity for small team
- All specifications stored in `specs/main/` directory

**Implementation**:
- ALL commits go to `main` branch (never create feature branches)
- Specs directory: `specs/main/` (not `specs/###-feature-name/`)
- Feature tracking via task IDs in tasks.md, not branch names
- This deviation is INTENTIONAL and NON-NEGOTIABLE

## Phase Gates

### Phase 0 Gate ✅ PASSED
- Technology choices justified (C# WPF vs alternatives)
- Performance targets defined and measurable (SC-002, SC-003)
- Data compatibility strategy documented (identical JSON/MongoDB)
- Architecture approach documented in plan.md

### Phase 1 Gate ✅ PASSED
- Data model matches Electron version (JSON structure validated in research.md)
- Service interfaces designed (contracts/ directory)
- No unnecessary complexity added (YAGNI principle)
- All technical decisions traceable to requirements (spec.md)

### Phase 2-7 Gates
- T016 (JSON compatibility tests) must pass
- T070/T071/T072 (performance benchmarks) must validate SC-002, SC-003
- T083 (UI acceptance checklist) must confirm feature parity
- All edge case tests (T048, new schema migration tasks) must pass

## Governance

**Authority**: This constitution is non-negotiable. Violations in `/speckit.analyze` are automatically CRITICAL severity.

**Amendment Process**:
1. Constitution changes require explicit documentation and user approval
2. Changes propagated to dependent templates (spec/plan/tasks)
3. Semantic versioning: MAJOR.MINOR.PATCH (e.g., 1.0.0 → 1.1.0 if adding principle, 2.0.0 if removing)

**Enforcement**:
- All PRs/reviews MUST verify compliance with principles
- Complexity justification required in plan.md for deviations
- Performance targets are hard requirements, not aspirational
- Data integrity gates MUST pass before release

**Reference**: Development guidance in `CLAUDE.md` (project-level instructions)
