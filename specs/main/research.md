# Research: Cloud-Based AI Provider Migration

**Date**: 2025-12-09

## Executive Summary

Research completed for migrating from local OLLAMA to cloud AI (Gemini, OpenAI, Anthropic).

**Key Decisions**:
- SDKs: OpenAI package + HTTP for Gemini/Anthropic
- Security: Windows DPAPI + AppData/Local
- Errors: Exponential backoff + Portuguese messages
- Prompts: Reuse OLLAMA prompt with provider formatting
- Migration: Settings banner + backward-compatible config

## R001: Cloud AI SDKs

### Gemini
- HTTP API: `https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent`
- Auth: `x-goog-api-key` header
- Model: `gemini-pro`

### OpenAI  
- NuGet: `OpenAI` package (official)
- Model: `gpt-4o` or `gpt-3.5-turbo`
- Feature: `response_format: {type: "json_object"}`

### Anthropic
- HTTP API: `https://api.anthropic.com/v1/messages`
- Auth: `x-api-key` header
- Model: `claude-3-5-sonnet-20241022`

## R002: API Key Security

- DPAPI: `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- Storage: `AppData/Local/EtnoPapers/config.json`
- UI: PasswordBox + masked display (`••••abcd`)
- Git: Exclude `config.json`

## R003: Error Handling

- 401: "Chave inválida"
- 429: "Limite excedido"  
- 500: "Serviço indisponível"
- Retry: Exponential backoff (2s, 4s, 8s), max 3 attempts

## R004: Prompts

- Reuse existing OLLAMA prompt (line 257-311 in OLLAMAService.cs)
- Format per provider: OpenAI uses system+user messages, Anthropic uses system+messages, Gemini uses contents
- Parameters: temperature=0.1, top_p/topP=0.3, max_tokens=8000

## R005: Migration

- Detect legacy: `config.OllamaUrl != null && config.ApiKey == null`
- UX: Settings banner (non-intrusive)
- Config: Backward-compatible (preserve old fields)
- Docs: API key acquisition links in README

## Phase 0 Complete ✅
