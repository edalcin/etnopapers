# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**EtnoPapers** is a Windows desktop application for automated extraction and cataloging of ethnobotanical metadata from scientific papers about traditional plant use by indigenous and traditional communities.

### Application Purpose

The application enables ethnobotany researchers to:
- Upload scientific papers in PDF format
- Extract article metadata and ethnobotanical data using AI (via local OLLAMA installation)
- Store extracted data in local JSON files
- Edit, review, and manage extracted records (CRUD interface)
- Synchronize selected records to external MongoDB (Atlas or local server)

### Key Technical Requirements

- **Platform**: Windows desktop native application
- **AI Integration**: Local OLLAMA for metadata extraction
- **Data Storage**:
  - Local: JSON files (structure defined in `docs/estrutura.json`)
  - Remote: MongoDB (Atlas or local, URI configured by user)
- **Core Features**:
  - PDF upload and processing interface
  - Configuration area (OLLAMA prompt, MongoDB URI)
  - CRUD interface for managing extracted records
  - Selective sync to MongoDB (deletes local copy on successful upload)
  - Professional Windows installer

### Data Structure

Mandatory extracted fields (see `docs/estrutura.json` for complete schema):
- `titulo` (normalized case)
- `autores` (APA format array)
- `ano` (year)
- `resumo` (always in Brazilian Portuguese)
- `especies` (array with vernacular names, scientific names, use types)
- Geographic data: `pais`, `estado`, `municipio`, `local`, `bioma`
- `comunidade` (community name and location)
- `metodologia`

### Development Framework

This project uses **SpecKit** - a specification-driven development workflow system integrated with Claude Code via slash commands. SpecKit guides feature development through structured phases: specification → planning → task breakdown → implementation.

The workflow is designed around PowerShell automation scripts and markdown-based artifacts, enabling incremental, constitution-driven feature development.

## Core Architecture

### Workflow Phases

SpecKit follows a strict multi-phase workflow:

1. **Specification Phase** (`/speckit.specify`): Creates user-centric feature specifications in `specs/###-feature-name/spec.md`
2. **Planning Phase** (`/speckit.plan`): Generates technical implementation plans in `plan.md` with research, data models, and contracts
3. **Task Breakdown** (`/speckit.tasks`): Produces dependency-ordered, user-story-based task lists in `tasks.md`
4. **Analysis** (`/speckit.analyze`): Validates cross-artifact consistency before implementation
5. **Implementation** (`/speckit.implement`): Executes tasks with TDD approach and checklist validation

### Directory Structure

```
etnopapers/
├── .specify/                        # SpecKit framework files
│   ├── scripts/powershell/          # Automation scripts for workflow
│   │   ├── create-new-feature.ps1   # Branch + spec directory creation
│   │   ├── setup-plan.ps1           # Plan phase initialization
│   │   ├── check-prerequisites.ps1  # Validates workflow prerequisites
│   │   └── update-agent-context.ps1 # Updates AI agent context files
│   ├── memory/
│   │   └── constitution.md          # Project constitution (principles/gates)
│   └── templates/                   # Markdown templates for artifacts
│       ├── spec-template.md
│       ├── plan-template.md
│       ├── tasks-template.md
│       └── checklist-template.md
│
└── .claude/
    └── commands/                    # Claude Code slash commands
        ├── speckit.specify.md
        ├── speckit.plan.md
        ├── speckit.tasks.md
        ├── speckit.implement.md
        └── speckit.analyze.md

specs/                               # Feature artifacts (created per feature)
└── ###-feature-name/
    ├── spec.md                      # User-centric specification
    ├── plan.md                      # Technical implementation plan
    ├── tasks.md                     # Ordered task checklist
    ├── research.md                  # Phase 0 research output
    ├── data-model.md                # Entity definitions
    ├── quickstart.md                # Integration scenarios
    ├── contracts/                   # API specifications
    └── checklists/                  # Quality validation checklists
```

## Key Commands

### SpecKit Workflow Commands

**Start a new feature:**
```bash
/speckit.specify "Add user authentication with OAuth2"
```
- Creates feature branch `###-feature-name`
- Generates `specs/###-feature-name/spec.md`
- Validates spec quality with checklist
- Resolves clarifications interactively (max 3)

**Create technical plan:**
```bash
/speckit.plan
```
- Must run from feature branch
- Generates `plan.md`, `research.md`, `data-model.md`, `contracts/`, `quickstart.md`
- Validates against constitution (`.specify/memory/constitution.md`)
- Updates agent context files

**Generate task breakdown:**
```bash
/speckit.tasks
```
- Organizes tasks by user story priority (P1, P2, P3)
- Each task uses checklist format: `- [ ] T### [P?] [US#?] Description with file path`
- Enables independent user story implementation

**Validate consistency:**
```bash
/speckit.analyze
```
- READ-ONLY analysis across spec/plan/tasks
- Detects: duplication, ambiguity, coverage gaps, constitution violations
- Severity levels: CRITICAL, HIGH, MEDIUM, LOW

**Execute implementation:**
```bash
/speckit.implement
```
- Validates checklists before starting
- Follows phase-by-phase execution (Setup → Tests → Core → Integration → Polish)
- Marks tasks as completed in `tasks.md`
- Creates ignore files based on detected tech stack

**Additional workflow commands:**

```bash
/speckit.clarify
```
- Identifies underspecified areas in the spec
- Asks up to 5 targeted clarification questions
- Encodes answers back into the spec
- Should run before `/speckit.plan` to reduce rework risk

```bash
/speckit.checklist
```
- Generates custom quality checklists for requirements
- Creates "unit tests for English" - validates requirement quality, not implementation
- Checks completeness, clarity, consistency, measurability, coverage
- Outputs to `FEATURE_DIR/checklists/[domain].md`

```bash
/speckit.constitution
```
- Creates or updates project constitution (`.specify/memory/constitution.md`)
- Defines non-negotiable principles that gate workflow phases
- Propagates changes to dependent templates
- Uses semantic versioning for constitution amendments

```bash
/speckit.taskstoissues
```
- Converts tasks from `tasks.md` into GitHub issues
- Requires GitHub remote URL
- Creates dependency-ordered issues in the repository

### PowerShell Scripts

All scripts support `-Json` flag for machine-readable output:

```powershell
# Create new feature (auto-detects next number)
.specify/scripts/powershell/create-new-feature.ps1 -Json "Feature description" -Number 5 -ShortName "oauth-integration"

# Setup planning phase
.specify/scripts/powershell/setup-plan.ps1 -Json

# Check workflow prerequisites
.specify/scripts/powershell/check-prerequisites.ps1 -Json -RequireTasks -IncludeTasks

# Update agent context (detects Claude/Cursor/Copilot)
.specify/scripts/powershell/update-agent-context.ps1 -AgentType claude
```

## Constitution-Driven Development

The project constitution (`.specify/memory/constitution.md`) defines non-negotiable principles that gate each workflow phase. Common gates include:

- **Test-First**: TDD mandatory (tests → approval → implementation)
- **Library-First**: Features as standalone libraries
- **CLI Interface**: Text in/out protocol for all functionality
- **Simplicity**: YAGNI principles, complexity requires justification

Constitution violations are CRITICAL severity in `/speckit.analyze` and must be justified in `plan.md` complexity tracking table.

## Task Format Requirements

Every task in `tasks.md` MUST follow this format:

```markdown
- [ ] T### [P?] [US#?] Description with file path
```

Components:
- `- [ ]`: Markdown checkbox (required)
- `T###`: Sequential task ID (T001, T002, ...)
- `[P]`: Parallel execution marker (optional)
- `[US#]`: User story label ([US1], [US2], ...) - required for story phases
- Description: Clear action with exact file path

Example:
```markdown
- [ ] T012 [P] [US1] Implement User model in src/models/user.py
```

## Workflow Best Practices

### Branch Naming

Format: `###-feature-name` (e.g., `5-oauth-integration`)

The number is auto-incremented by checking:
1. Remote branches (`git ls-remote --heads origin`)
2. Local branches (`git branch`)
3. Specs directories (`specs/###-feature-name/`)

### User Story Organization

Specs prioritize user stories (P1, P2, P3). Each story must be:
- Independently testable
- Deliverable as standalone MVP
- Mapped to specific tasks in `tasks.md`

### Specification Quality

Specs must be:
- Technology-agnostic (no frameworks, languages, APIs)
- Written for business stakeholders
- Success criteria must be measurable and verifiable
- Maximum 3 `[NEEDS CLARIFICATION]` markers (prioritize: scope > security > UX > technical)

### Implementation Strategy

1. Validate all checklists complete before implementation
2. Execute tasks phase-by-phase (no skipping)
3. Mark tasks as `[X]` immediately after completion
4. Respect [P] parallel markers (different files only)
5. Follow TDD when tests are specified

## Common Patterns

### Handling Clarifications

When spec generation encounters ambiguity:
1. Make informed guesses for reasonable defaults
2. Document assumptions in spec
3. Use `[NEEDS CLARIFICATION: specific question]` only for critical decisions
4. Present 3-4 options with implications in table format
5. Update spec after user selects option

### Cross-Artifact References

- Tasks reference requirements by keyword/phrase matching
- Plan references spec sections by heading
- Analyze validates bidirectional coverage

### Agent Context Updates

After planning phase, run:
```powershell
.specify/scripts/powershell/update-agent-context.ps1 -AgentType claude
```

This updates `.claude/claude.md` (or equivalent for Cursor/Copilot) with:
- New tech stack from current plan
- Preserves manual additions between markers
- Avoids duplication across features

## Project-Specific Constraints

### Git Workflow
- **All commits must be made to the `main` branch** - Never create feature branches
- This differs from the standard SpecKit workflow which typically uses numbered feature branches

### Documentation Language
- **README.md must be in Brazilian Portuguese** - This is user-facing documentation
- **Technical documentation (specs, plans, code) may be in English**
- **Data fields**: `resumo` must always be extracted/translated to Brazilian Portuguese

### Performance Considerations
- Local JSON file has a maximum record limit for performance
- System should warn users to sync to MongoDB before reaching limit
- Block new PDF uploads when local storage limit is reached

## Important Notes

- All PowerShell scripts must be run from repository root
- Single quotes in arguments require escape syntax: `'I'\''m Groot'` (or use double quotes)
- Scripts output JSON when `-Json` flag is used (always parse for paths)
- Constitution is non-negotiable during analysis (modifications require separate process)
- Slash commands detect current context (branch, feature dir) automatically
- Due to single-branch workflow, the SpecKit branch naming conventions do not apply to this project
