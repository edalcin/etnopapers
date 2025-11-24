# Etnopapers v1.0 - Archived Documentation

**Date Archived**: 2025-11-24
**Reason**: Migration to v2.0 architecture (Local AI + MongoDB)

## What's Here

This directory contains the original v1.0 specification and planning documents before the migration to v2.0.

### Files

- **plan-v1.0.md**: Original implementation plan for v1.0 (external APIs, SQLite)
- **tasks-v1.0.md**: Original task breakdown for v1.0 implementation

## v1.0 Architecture Summary

**Technology Stack:**
- Frontend: React 18 + TypeScript (direct API calls to Gemini/ChatGPT/Claude)
- Backend: FastAPI + Python 3.11
- Database: SQLite 3.35+ with WAL
- Deployment: Single Docker container (no GPU required)

**Key Differences from v2.0:**
- API keys stored in browser localStorage (privacy)
- External AI APIs (Gemini, ChatGPT, Claude) - user provides keys
- SQLite for data storage (single file, portable)
- No GPU requirement (runs on CPU)
- No MongoDB dependency
- Frontend handled API key selection and validation

**Functional Features:**
- PDF upload with drag-and-drop
- Metadata extraction via external AI APIs
- Manual metadata editing
- Article history and search
- Database download (SQLite file)
- Taxonomy validation (GBIF/Tropicos)
- Draft auto-save

## Why v2.0 Was Chosen

### Advantages of v2.0 (Local AI + MongoDB)

| Aspect | v1.0 | v2.0 |
|--------|------|------|
| **Privacy** | Data sent to external APIs | 100% local processing |
| **Cost** | $0.01-0.05 per article | $0 per article |
| **Quota** | Limited by API plan | Unlimited |
| **Latency** | 2-10 seconds | 1-3 seconds (with GPU) |
| **Setup** | Complex (API keys) | Simple (environment vars) |
| **Hardware** | Works on CPU | Requires GPU (6-8 GB VRAM) |
| **Database** | SQLite (single file) | MongoDB (scalable) |

### Migration Decision

v2.0 was chosen because:
1. ✅ Better privacy (data never leaves server)
2. ✅ Lower total cost of ownership (no per-API charges)
3. ✅ Better performance with GPU (1-3s vs 2-10s)
4. ✅ Simplified deployment (no API key management)
5. ✅ Offline capability (after model download)
6. ✅ Document-centric data model (MongoDB) aligns with ethnobotany metadata

## References

- **Constitution v2.0**: `.specify/memory/constitution.md`
- **v2.0 Specification**: `specs/main/spec.md`
- **v2.0 Plan**: `specs/main/plan.md` (regenerated)
- **v2.0 Tasks**: `specs/main/tasks.md` (regenerated)
- **Local AI Integration Guide**: `specs/main/local-ai-integration.md`

## Important Notes

- v1.0 implementation was **never completed** (design only)
- v2.0 is now the active development branch
- If reverting to v1.0 is needed, files are here for reference
- New feature development should target v2.0 only

---

**Last Updated**: 2025-11-24
**Archive Maintainer**: Etnopapers Team
