# Investigation Plan: Quality Difference Between EtnoPapers and OLLAMA Desktop

## Problem Statement
EtnoPapers extraction is now returning **valid data** (not hallucinations), but when compared to OLLAMA Desktop using the **same model (Qwen2.5:7b), same PDF, same prompt**, the results are **significantly inferior**:

### Quality Comparison

| Field | EtnoPapers | OLLAMA Desktop | Status |
|-------|-----------|---|--------|
| titulo | ✅ Correct | ✅ Correct | PASS |
| autores | ✅ Correct (6) | ✅ Correct (6) | PASS |
| ano | ✅ 2010 | ✅ 2010 | PASS |
| resumo | ❌ "" (empty) | ✅ null | FAIL |
| especies | 7 plants | 6 plants | DIFFERENT |
| especies.tipo_uso | ❌ ALL empty | ✅ Some filled | FAIL |
| especies.parte_usada | ❌ ALL empty | ✅ Some filled | FAIL |
| especies.preparacao | ❌ ALL empty | ✅ Some filled | FAIL |
| pais | ❌ "" (empty) | ✅ "Brasil" | FAIL |
| estado | ❌ "" (empty) | ✅ "Pernambuco" | FAIL |
| municipio | ✅ Correct | ✅ Correct | PASS |
| local | ❌ null | ✅ "Ouricuri Forest..." | FAIL |
| bioma | ❌ "" (empty) | ✅ null | FAIL |
| metodologia | ❌ null | ✅ Description | FAIL |
| ano_coleta | ❌ null | ✅ 2007 | FAIL |

**Score**: EtnoPapers 6/14 fields correct (43%) vs OLLAMA Desktop 11/14 fields correct (79%)

---

## Root Cause Hypotheses

### H1: Different OLLAMA Generation Parameters
**Hypothesis**: OLLAMA Desktop has different temperature/top_p/top_k settings than EtnoPapers
- OLLAMA Desktop default temperature ≠ EtnoPapers HTTP request parameters
- Higher temperature in EtnoPapers = more repetition of "empty" values
- **Impact**: Would explain systematic failures in detail extraction

### H2: Prompt Format/Encoding Issue
**Hypothesis**: When custom prompt is appended to markdown with `$"{customPrompt}\n\n{pdfText}"`, the formatting breaks OLLAMA's instruction parsing
- EtnoPapers appends: `prompt\n\npdfText`
- OLLAMA Desktop might send differently (e.g., using system vs user message)
- Line breaks or encoding might cause OLLAMA to ignore the anti-hallucination rules
- **Impact**: Would explain why OLLAMA reverts to generic/empty values

### H3: Markdown Content Difference
**Hypothesis**: Different markdown conversion between tools or truncation
- EtnoPapers sends 50,905 characters
- OLLAMA Desktop might receive different content (better structured, more complete)
- Markdown structure (headings, separators) might differ
- **Impact**: Less information = less data to extract

### H4: Token Context Length / Prompt Truncation
**Hypothesis**: The combined prompt + markdown exceeds OLLAMA's context or gets truncated
- Custom prompt is ~2000+ characters
- Markdown is ~50,905 characters
- Total: ~53,000 characters
- If Qwen2.5:7b has context limits, might truncate at markdown or instruction end
- **Impact**: Would explain missing detailed fields (metodologia, ano_coleta, etc.)

### H5: OLLAMA API vs Desktop Differences
**Hypothesis**: OLLAMA HTTP API processes requests differently than OLLAMA Desktop UI
- Desktop might use different API endpoints
- Desktop might not support the `/api/generate` endpoint in the same way
- Different streaming/response handling
- **Impact**: Would explain consistent quality difference

---

## Investigation Steps

### Phase 1: Diagnostic Logging (READY TO TEST)
**Status**: ✅ Code is ready
**Commands**:
```
1. Run: H:\git\etnopapers\src\EtnoPapers.UI\bin\Release\publish\EtnoPapers.UI.exe
2. Extract the same PDF
3. Check log: C:\Users\EDalcin\AppData\Roaming\EtnoPapers\logs\extraction-markdown-debug.log
4. Compare:
   - Custom prompt length and first 500 chars
   - Full markdown content
   - Full OLLAMA response
```

**What to look for**:
- Is the custom prompt being sent in full?
- Is the markdown being truncated?
- Does the OLLAMA response show the anti-hallucination instructions being followed?

### Phase 2: OLLAMA API Parameter Testing
**Goal**: Test if different generation parameters improve quality
**Options**:
```
A. Add temperature parameter (default OLLAMA: 0.7)
   - Try: 0.1 (deterministic, follow instructions strictly)
   - Try: 0.5 (balanced)

B. Add top_p parameter (nucleus sampling)
   - Try: 0.3 (conservative, follow prompt more strictly)
   - Try: 0.5 (balanced)

C. Add top_k parameter
   - Try: 40 (conservative)
   - Try: 10 (very conservative)
```

### Phase 3: Prompt Format Optimization
**Current Format**:
```
{customPrompt}
{blank line}
{markdown}
```

**Alternative Formats to Test**:

**Option A**: System Message Wrapper
```
You are an ethnobotanical data extraction expert. Extract data EXACTLY as it appears in the document.

{custom prompt}

DOCUMENT TO EXTRACT FROM:
{markdown}
```

**Option B**: Explicit Instruction Emphasis
```
{custom prompt}

---

NOW EXTRACT FROM THIS DOCUMENT (do NOT invent or assume):

{markdown}

---

CRITICAL REMINDERS:
- Return ONLY valid JSON
- Copy exactly as appears, do NOT reformat
- Use null for any field not found - NEVER use empty strings
- NEVER generate, infer, or assume information
```

**Option C**: Structured Prompt with Markers
```
EXTRACTION TASK BEGIN
Role: {description}
Rules: {rules}
Format: {format}

INPUT DOCUMENT START
{markdown}
INPUT DOCUMENT END

OUTPUT JSON START
{template}
OUTPUT JSON END
```

### Phase 4: OLLAMA Desktop Capture
**Goal**: Capture exact request that OLLAMA Desktop sends
**Method**:
1. Enable network capture (Wireshark) or browser dev tools
2. Use OLLAMA Desktop to extract from same PDF
3. Capture the HTTP POST body
4. Analyze: prompt format, parameters, encoding

---

## Implementation Roadmap

### Short-term (Immediate)
1. ✅ Run Phase 1 diagnostic logging
2. Add OLLAMA generation parameters (temperature, top_p) to config.json
3. Test with conservative parameters (temperature=0.1)

### Medium-term (Next)
1. Test alternative prompt formats (Phase 3, Options A-C)
2. Optimize markdown format (cleaner structure)
3. Compare results with OLLAMA Desktop

### Long-term (Future)
1. Add prompt template library for different extraction styles
2. Implement A/B testing framework for prompt comparison
3. Model-specific parameter profiles (Qwen vs Mistral vs Llama)
4. Implement extraction quality scoring

---

## Key Files to Monitor

**Logging**:
- `C:\Users\EDalcin\AppData\Roaming\EtnoPapers\logs\extraction-markdown-debug.log`

**Configuration**:
- `C:\Users\EDalcin\Documents\EtnoPapers\config.json`

**Code**:
- `src/EtnoPapers.Core/Services/OLLAMAService.cs` - HTTP request parameters
- `src/EtnoPapers.Core/Services/ExtractionPipelineService.cs` - Prompt + markdown combination
- `src/EtnoPapers.Core/Services/MarkdownConverter.cs` - Markdown generation

---

## Next Steps

1. **Run the updated EtnoPapers** with enhanced logging
2. **Extract the same PDF** used for OLLAMA Desktop test
3. **Share the log file** so we can see:
   - Exact custom prompt being used
   - Exact markdown content being sent
   - Exact OLLAMA response
4. **Compare with OLLAMA Desktop**
5. **Implement Phase 2**: Add temperature parameter to config

This will pinpoint whether the issue is in:
- Input (prompt/markdown format)
- Parameters (OLLAMA generation settings)
- Output (response parsing/processing)
