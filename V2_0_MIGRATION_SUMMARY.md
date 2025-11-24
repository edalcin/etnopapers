# Etnopapers v2.0 Migration Summary

**Date**: 2025-11-24
**Status**: In Progress (Plan & Tasks regeneration pending)

## Overview

This document tracks the migration from Etnopapers v1.0 (external APIs + SQLite) to v2.0 (local AI + MongoDB).

## Changes Completed ✅

### 1. Constitution Amendment (✅ DONE)
**File**: `.specify/memory/constitution.md`

**Changes Made:**
- Updated Principle II: Authorized MongoDB with portable backup capability
- Updated Principle III: Clarified Ollama as local service with graceful degradation
- Updated Principle V: Authorized GPU requirement for production
- Bumped version to 2.0.0 with amendment date

**Impact**: Constitution now fully authorizes v2.0 architecture

---

### 2. v1.0 Documentation Archived (✅ DONE)
**Location**: `specs/archive/v1.0/`

**Files Archived:**
- `plan-v1.0.md`: Original SQLite + external API plan
- `tasks-v1.0.md`: Original task breakdown
- `README.md`: Archive documentation explaining v1.0 vs v2.0 differences

**Purpose**: Preserve v1.0 for reference; prevent confusion with v2.0 plans

---

### 3. AI Integration Contract Verified (✅ DONE)
**File**: `specs/main/contracts/ai-integration.md`

**Status**: Already updated for v2.0!
**Content Includes:**
- Complete Ollama integration architecture
- Qwen2.5-7B-Instruct model specification
- Instructor + Pydantic structured output validation
- GPU configuration and performance benchmarks
- Troubleshooting guide
- POST /api/extract/metadata endpoint specification

**No changes needed** - contract is current and comprehensive

---

## Changes In Progress ⏳

### 4. Plan Regeneration (⏳ IN PROGRESS)
**Command**: `/speckit.plan`

**Expected Output**: `specs/main/plan.md`

**What Will Change:**
- Replace v1.0 architecture (SQLite, external APIs) with v2.0 (MongoDB, Ollama)
- Update tech stack section (MongoDB connection, Ollama service, pdfplumber)
- Update phases to include:
  - Phase 0: GPU infrastructure setup
  - Phase 1: MongoDB schema + Ollama integration
  - Phase 2: Backend extraction endpoint
  - Phase 3: Frontend integration
- Update risk assessment and performance targets
- Add deployment guide for UNRAID with GPU

---

### 5. Tasks Regeneration (⏳ IN PROGRESS)
**Command**: `/speckit.tasks`

**Expected Output**: `specs/main/tasks.md`

**What Will Change:**
- Replace 33 v1.0 tasks with v2.0 tasks
- Remove API key management tasks (TASK-010, TASK-028)
- Remove external AI integration tasks (TASK-013-015)
- Add infrastructure tasks:
  - GPU passthrough setup (docker-compose)
  - Ollama service configuration
  - MongoDB collection schema
  - Model download and health checks
- Add extraction service tasks:
  - POST /api/extract/metadata implementation
  - Instructor + Pydantic validation
  - PDF text extraction with pdfplumber
  - Error handling and logging
- All 68 RF requirements (RF-001 to RF-068) mapped to specific tasks
- Dependency graph showing task order
- Parallel execution opportunities identified

---

## Artifact Alignment Status

### Core Documents

| Document | Version | Status | Notes |
|----------|---------|--------|-------|
| spec.md | v2.0 | ✅ Current | Requires RF-069 (offline requirement) |
| plan.md | v1.0 | ⏳ Regenerating | Awaiting /speckit.plan output |
| tasks.md | v1.0 | ⏳ Regenerating | Awaiting /speckit.tasks output |
| constitution.md | v2.0 | ✅ Updated | Fully authorizes v2.0 principles |
| contracts/ai-integration.md | v2.0 | ✅ Current | Complete Ollama specification |

### Specification Documents

| Document | Status | Notes |
|----------|--------|-------|
| local-ai-integration.md | ✅ Reference | Comprehensive technical guide (keep for reference) |
| plan-local-ai.md | ✅ Reference | Implementation roadmap (keep for reference) |
| tasks-v2-local-ai.md | ✅ Reference | Alternative task breakdown (keep or merge) |
| CLAUDE.md | ✅ Current | Updated for v2.0 architecture |
| README.md | ✅ Current | Updated for v2.0 setup |

---

## Pending Actions

After slash commands complete:

1. **Verify plan.md regeneration**
   - Check architecture section matches v2.0
   - Verify phases and timeline
   - Ensure MongoDB + Ollama mentioned

2. **Verify tasks.md regeneration**
   - Check all 68 RF items have task mappings
   - Verify no v1.0 tasks remain
   - Confirm GPU infrastructure tasks included
   - Validate dependency ordering

3. **Consolidate documentation** (optional)
   - Consider merging local-ai-integration.md into spec.md
   - Decide whether to keep or archive plan-local-ai.md and tasks-v2-local-ai.md

4. **Final git commit**
   - Constitution amendments
   - Archived v1.0 docs
   - Updated plan.md and tasks.md
   - This summary document
   - Commit message explaining v2.0 migration

---

## Commit Details (Pending)

**Message Title**: `Docs: Complete v2.0 migration (Local AI + MongoDB architecture)`

**Description**:
```
Major architecture update from v1.0 (external APIs + SQLite) to v2.0 (local AI + MongoDB):

**Constitution Changes**:
- Principle II: Authorize MongoDB with portable backup
- Principle III: Clarify Ollama graceful degradation
- Principle V: Authorize GPU requirement (6-8 GB VRAM)
- Version bumped to 2.0.0

**Plan Regeneration**:
- Updated architecture (MongoDB, Ollama, GPU infrastructure)
- New phases: GPU setup, Ollama integration, extraction service
- Performance targets: 1-3s inference, offline capability

**Tasks Regeneration**:
- All 68 RF requirements mapped to v2.0 tasks
- Removed API key management (v1.0 only)
- Removed external API integration (v1.0 only)
- Added infrastructure setup (GPU, Ollama, MongoDB)
- Added extraction service implementation
- Dependency graph and parallel opportunities identified

**Documentation**:
- Archived v1.0 plan and tasks to specs/archive/v1.0/
- Verified contracts/ai-integration.md (already v2.0)
- Confirmed constitution compliance

**Status**: Ready for implementation Phase 0 (Infrastructure)
```

---

## Next Steps After This Commit

1. **Implement Phase 0 (Infrastructure)** - ~1-2 days
   - Update docker-compose.yml with Ollama service
   - Configure GPU passthrough (nvidia-docker)
   - Set up MongoDB collection schema
   - Test Ollama model download and health checks

2. **Implement Phase 1 (Backend)** - ~2-3 days
   - Create extraction_service.py with Ollama client
   - Implement POST /api/extract/metadata endpoint
   - Add pdfplumber integration
   - Add Instructor + Pydantic validation

3. **Implement Phase 2 (Frontend)** - ~1-2 days
   - Remove APIConfiguration component
   - Update PDFUpload to call backend endpoint
   - Remove API key management from store
   - Update UI messages for simplified flow

4. **Phase 3 (Testing & Refinement)** - ~2-3 days
   - Create test dataset with 20 PDFs
   - Measure extraction accuracy across fields
   - Performance benchmarking
   - Quality metrics and prompt optimization

5. **Phase 4 (Documentation & Deploy)** - ~1 day
   - Update README with GPU setup instructions
   - Create UNRAID-specific deployment guide
   - Troubleshooting documentation
   - Release notes for v2.0

---

**Document Created**: 2025-11-24
**Status**: Awaiting slash command completion to finalize
**Commitment**: All v2.0 architecture decisions documented and approved
