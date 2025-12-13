# Quick Diagnosis: EtnoPapers vs OLLAMA Desktop

## Benchmark Results (OLLAMA Desktop with prompt2.md)
- pais: "Brasil" ✅
- estado: "Pernambuco" ✅
- local: "Ouricuri Forest, Fulni-o Indigenous Land" ✅
- metodologia: Present with full text ✅
- ano_coleta: 2007 ✅

## Current EtnoPapers Results
- pais: "" (empty string) ❌
- estado: "" (empty string) ❌
- local: null ❌
- metodologia: null ❌
- ano_coleta: null ❌

## Critical Observations

1. **Empty Strings vs Null**
   - OLLAMA Desktop returns null for missing fields
   - EtnoPapers returns "" (empty strings)
   - This suggests: JSON serialization is converting nulls to empty strings

2. **Missing Complex Fields**
   - metodologia: NOT being extracted at all
   - ano_coleta: NOT being extracted at all
   - These are in the PDF abstract but not being captured

3. **Both Have Same Fields But Different Values**
   - Title: Both correct
   - Authors: Both correct
   - Year: Both correct
   - This proves markdown IS reaching OLLAMA
   - Problem is SELECTIVE FIELD EXTRACTION

## Possible Root Causes

A. **JSON Serialization Setting**
   - NullValueHandling = NullValueHandling.Ignore in JsonSerializationHelper
   - This might be converting nulls to empty strings on deserialization
   
B. **Truncated OLLAMA Response**
   - OLLAMA might be cutting off after certain fields
   - metodologia and ano_coleta are the LAST fields
   - If response is truncated, these would be missing

C. **Generation Parameters**
   - OLLAMA Desktop might have max_tokens=unlimited or very high
   - EtnoPapers might have a default max_tokens that cuts response short
   - OLLAMA might not finish generating the full JSON

D. **Response Format Mismatch**
   - OLLAMA Desktop might be returning complete JSON
   - EtnoPapers might be receiving truncated/incomplete JSON
   - CleanOLLAMAResponse might be removing valid data

## Investigation Priority

HIGH PRIORITY:
1. Check if OLLAMA response is being truncated
2. Check JSON serialization settings
3. Add max_tokens parameter to OLLAMA request (unlimited)
4. Log full OLLAMA response to disk, not just first 800 chars

MEDIUM PRIORITY:
5. Test with explicit field ordering in prompt
6. Add response length validation
7. Compare encoding/format between EtnoPapers and Desktop

