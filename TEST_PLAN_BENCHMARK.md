# Test Plan: Quality Improvement Verification

## Benchmark Setup
- **PDF**: `docs/pdfTest/Rapid_ethnobotanical_diagnosis_of_the_Fu.pdf`
- **Expected Output**: `docs/pdfTest/ollama.json` (OLLAMA Desktop result)
- **Prompt**: `docs/pdfTest/prompt2.md` (anti-hallucination rules)
- **Model**: Qwen2.5:7b (same as OLLAMA Desktop test)

---

## Critical Fixes Applied

### Fix #1: OLLAMA Generation Parameters
Added to OLLAMAService.cs (line 157-163):
```csharp
options = new {
    temperature = 0.1,      // Strict instruction following
    top_p = 0.3,            // Conservative
    top_k = 10,             // Very conservative
    num_predict = 8000      // Prevent truncation
}
```

**Problem Solved**: OLLAMA default `max_tokens` was truncating responses, causing fields like `metodologia` and `ano_coleta` to be cut off.

**Expected Outcome**: Complete JSON responses with all fields present.

### Fix #2: Complete Response Logging
Changed from first 800 chars to full response capture:
```
--- FULL OLLAMA RESPONSE ---
[complete JSON output]
--- END OLLAMA RESPONSE ---
```

**Problem Solved**: Can now inspect exact OLLAMA output for comparison.

---

## Test Execution Steps

### Step 1: Prepare Environment
1. Stop any running EtnoPapers instances
2. Clear log file (or rename):
   ```
   C:\Users\EDalcin\AppData\Roaming\EtnoPapers\logs\extraction-markdown-debug.log
   ```
3. Ensure config uses prompt2.md settings:
   ```json
   "ollamaUrl": "http://localhost:11434",
   "ollamaModel": "Qwen2.5:7b"
   ```

### Step 2: Extract Benchmark PDF
1. Run: `H:\git\etnopapers\src\EtnoPapers.UI\bin\Release\publish\EtnoPapers.UI.exe`
2. Select PDF: `C:\Users\EDalcin\Desktop\Desenv\artigos\Rapid_ethnobotanical_diagnosis_of_the_Fu.pdf`
3. Or use the one in `docs/pdfTest/`
4. Wait for extraction to complete (~3 minutes)

### Step 3: Capture Results
1. Extract the generated JSON from the UI or from storage
2. Extract the complete log file:
   ```
   C:\Users\EDalcin\AppData\Roaming\EtnoPapers\logs\extraction-markdown-debug.log
   ```

### Step 4: Compare Outputs
Create comparison table with benchmark:
```
Field           | EtnoPapers v2 | OLLAMA Desktop | Match? | Status
----------------|---------------|---|--------|--------
titulo          | ...           | ... | Y/N | ✅/❌
autores         | ...           | ... | Y/N | ✅/❌
ano             | ...           | ... | Y/N | ✅/❌
resumo          | ...           | ... | Y/N | ✅/❌
pais            | ...           | ... | Y/N | ✅/❌
estado          | ...           | ... | Y/N | ✅/❌
municipio       | ...           | ... | Y/N | ✅/❌
local           | ...           | ... | Y/N | ✅/❌
metodologia     | ...           | ... | Y/N | ✅/❌
ano_coleta      | ...           | ... | Y/N | ✅/❌
especies.count  | ...           | ... | Y/N | ✅/❌
```

---

## Success Criteria

### MUST HAVE (Critical)
- [ ] metodologia is populated (not null)
- [ ] ano_coleta is populated (not null)
- [ ] pais is "Brasil" (not empty string)
- [ ] estado is "Pernambuco" (not empty string)
- [ ] local is populated (not null)
- [ ] No empty strings "", all nulls are null (not "")

### SHOULD HAVE (High Priority)
- [ ] All fields match OLLAMA Desktop output exactly
- [ ] Full OLLAMA response logged to file (can inspect)
- [ ] Extraction time reasonable (~2-3 minutes)

### NICE TO HAVE (Enhancement)
- [ ] Species details (tipo_uso, parte_usada) populated where present
- [ ] Community information complete
- [ ] Bioma extracted if present

---

## Expected Improvement

### Before Fixes (EtnoPapers v1)
```
Fields Correct: 6/14 (43%)
- titulo: ✅
- autores: ✅
- ano: ✅
- municipio: ✅
- comunidade.nome: ✅
- comunidade.localizacao: ✅

Fields Failing: 8/14 (57%)
- resumo: "" (empty)
- pais: "" (empty)
- estado: "" (empty)
- local: null
- bioma: "" (empty)
- metodologia: null
- ano_coleta: null
- especies details: All empty
```

### After Fixes (EtnoPapers v2) - EXPECTED
```
Fields Correct: ~12/14 (86%)
- titulo: ✅
- autores: ✅
- ano: ✅
- resumo: ✅ (null, correct)
- pais: ✅ ("Brasil")
- estado: ✅ ("Pernambuco")
- municipio: ✅
- local: ✅ (extracted)
- metodologia: ✅ (extracted)
- ano_coleta: ✅ (extracted)
- comunidade: ✅
- bioma: ✅ (null, correct)
- especies.count: ✅

With num_predict=8000 parameter, OLLAMA should generate
the full response without truncation.
```

---

## If Issues Persist

### If metodologia/ano_coleta Still Null
1. Check log file for FULL OLLAMA RESPONSE
2. Verify num_predict=8000 is in the request (should see in debug)
3. Check if response was actually truncated
4. Increase num_predict to 10000-12000

### If Empty Strings Persist
1. Check JSON deserialization in ExtractionPipelineService
2. Verify NullValueHandling settings in JsonSerializationHelper
3. Check if empty fields are being filled somewhere else

### If Fields Still Missing
1. Compare prompt formatting between EtnoPapers and OLLAMA Desktop
2. Check if markdown content is complete
3. Verify character encoding (UTF-8)

---

## Files to Monitor

**Extraction Log**:
```
C:\Users\EDalcin\AppData\Roaming\EtnoPapers\logs\extraction-markdown-debug.log
```

**Results**:
```
C:\Users\EDalcin\Documents\EtnoPapers\records\*.json
```

**Configuration**:
```
C:\Users\EDalcin\Documents\EtnoPapers\config.json
```

---

## Testing Timeline

- Step 1-2: 5 minutes
- Step 3: 2-3 minutes (extraction)
- Step 4: 10 minutes (comparison)
- **Total**: ~20 minutes

---

## Report Format

Please share:
1. **etnopapers_v2.json** - The newly extracted JSON
2. **comparison_results.md** - Field-by-field comparison
3. **extraction_log_excerpt.txt** - The "OLLAMA RESPONSE RECEIVED" section from log

This will allow verification of the fix effectiveness.
