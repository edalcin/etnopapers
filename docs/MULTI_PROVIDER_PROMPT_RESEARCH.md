# Multi-Provider AI Prompt Engineering Research

**Date**: 2025-12-09
**Context**: EtnoPapers migration from local OLLAMA to cloud AI providers (OpenAI, Anthropic, Gemini)
**Objective**: Ensure consistent ethnobotanical metadata extraction across all three providers

---

## Executive Summary

Based on analysis of existing OLLAMA prompts, benchmark test results (Claude/Gemini), and AI provider API specifications, this document provides:

1. **Prompt Structure Adaptations** - How to structure prompts for each provider
2. **JSON Output Strategies** - Provider-specific approaches to structured output
3. **Parameter Mappings** - Temperature, top_p, top_k equivalents
4. **Translation Handling** - Brazilian Portuguese requirement strategies
5. **Anti-Hallucination Techniques** - Maintaining extraction accuracy

**Key Finding**: The existing OLLAMA prompt structure is highly compatible with all three cloud providers with minimal adaptation. The anti-hallucination prompt (`docs/pdfTest/prompt2.md`) shows excellent results and should be the foundation for cloud provider implementation.

---

## 1. Prompt Structure Differences

### Current OLLAMA Approach
```
Single prompt string with:
- Task description
- Extraction rules
- Example JSON structure
- Markdown content to analyze
```

### OpenAI API (GPT-4, GPT-4-Turbo, GPT-3.5)

**Message Structure**:
```csharp
var messages = new[]
{
    new { role = "system", content = SYSTEM_INSTRUCTION },
    new { role = "user", content = PDF_MARKDOWN_CONTENT }
};
```

**System Message** (extracted from OLLAMA prompt lines 1-311):
```
Analyze the following scientific paper about traditional plant use by indigenous and traditional communities.

IMPORTANT: The content below is in structured Markdown format...

EXTRACTION RULES:
1. Title: Usually the FIRST # heading...
2. Authors: Listed after the title...
[...full extraction rules...]

EXAMPLE OUTPUT:
{
  "titulo": "...",
  "autores": [...],
  ...
}
```

**User Message** (the actual markdown):
```
# Title from PDF
## Authors
[...actual PDF content as markdown...]
```

**Advantages**:
- Clean separation of instructions vs content
- System message establishes extraction rules once
- User message contains only data to process
- Supports conversation history (if needed for multi-turn extraction)

---

### Anthropic API (Claude 3.5 Sonnet, Claude Opus 4.5)

**Message Structure**:
```csharp
var requestBody = new
{
    model = "claude-3-5-sonnet-20241022",
    system = SYSTEM_INSTRUCTION,
    messages = new[]
    {
        new { role = "user", content = PDF_MARKDOWN_CONTENT }
    },
    max_tokens = 8000,
    temperature = 0.1
};
```

**System Parameter** (separate from messages array):
```
Analyze the following scientific paper about traditional plant use...
[...full extraction rules...]
CRITICAL: Return ONLY valid JSON. No markdown code blocks, no explanations.
```

**User Message**:
```
[PDF markdown content]
```

**Advantages**:
- Explicit `system` parameter at root level (not in messages)
- Supports very long system prompts (up to 32K tokens on Claude Opus)
- Excellent JSON formatting compliance when instructed
- Strong instruction-following with low temperature

**Anthropic-Specific Best Practices**:
1. Use `system` parameter for extraction rules (not a system role message)
2. Set `max_tokens` high enough (8000-16000 for full response)
3. Add explicit "Return ONLY valid JSON" instruction
4. Use XML tags for structure if needed: `<document>`, `<extraction_rules>`

---

### Google Gemini API (Gemini 1.5 Pro, Gemini 2.0 Flash)

**Message Structure** (REST API):
```csharp
var requestBody = new
{
    contents = new[]
    {
        new
        {
            role = "user",
            parts = new[]
            {
                new { text = FULL_PROMPT_WITH_CONTENT }
            }
        }
    },
    generationConfig = new
    {
        temperature = 0.1,
        topP = 0.3,
        topK = 10,
        maxOutputTokens = 8192,
        responseMimeType = "application/json", // KEY FEATURE!
        responseSchema = JSON_SCHEMA // Optional structured output
    }
};
```

**Full Prompt** (combined instruction + content):
```
Analyze the following scientific paper about traditional plant use...

EXTRACTION RULES:
[...rules...]

RETURN VALID JSON ONLY:
{
  "titulo": "string",
  "autores": ["string"],
  ...
}

DOCUMENT TO ANALYZE:
# Title from PDF
[...markdown content...]
```

**Advantages**:
- **Native JSON mode**: `responseMimeType = "application/json"`
- **JSON Schema validation**: Can provide schema for guaranteed structure
- Supports very long context (1M tokens on Gemini 1.5 Pro)
- No separate system/user split required (simpler for single-turn tasks)

**Gemini-Specific Best Practices**:
1. Use `responseMimeType: "application/json"` to enforce JSON output
2. Optionally provide `responseSchema` with exact JSON structure
3. Combine instructions and content in single message (no system/user split needed)
4. Set `maxOutputTokens: 8192` to prevent truncation

---

## 2. JSON Output Formatting Strategies

### OpenAI: JSON Mode + Function Calling

**Approach 1: JSON Mode (Recommended)**
```csharp
var requestBody = new
{
    model = "gpt-4-turbo-preview",
    messages = messages,
    response_format = new { type = "json_object" },
    temperature = 0.1,
    max_tokens = 8000
};
```

**Requirements**:
- Must include "JSON" keyword in system/user message
- Returns valid JSON object (guaranteed)
- No markdown code blocks (```json```)
- Schema enforcement via prompt engineering only

**Approach 2: Function Calling (Advanced)**
```csharp
var functions = new[]
{
    new
    {
        name = "extract_ethnobotanical_metadata",
        description = "Extracts metadata from ethnobotanical papers",
        parameters = new
        {
            type = "object",
            properties = new
            {
                titulo = new { type = "string" },
                autores = new { type = "array", items = new { type = "string" } },
                ano = new { type = "integer" },
                // ... full schema from docs/estrutura.json
            },
            required = new[] { "titulo", "autores", "ano" }
        }
    }
};
```

**Recommendation**: Use JSON Mode (Approach 1) for simplicity. Function calling is overkill unless you need strict schema validation.

---

### Anthropic: Prompt Engineering

**Strategy**: Explicit instructions + low temperature

```
CRITICAL INSTRUCTIONS:
1. Return ONLY valid JSON
2. No markdown code blocks (no ```json)
3. No explanations or commentary
4. Use null for missing fields (not "", not "unknown")

OUTPUT FORMAT:
{
  "titulo": "string or null",
  "autores": ["string"],
  ...
}
```

**Anti-Pattern Detection** (post-processing):
```csharp
// Remove markdown code blocks if present
var cleanedResponse = response.Trim();
if (cleanedResponse.StartsWith("```json"))
{
    cleanedResponse = cleanedResponse
        .Replace("```json", "")
        .Replace("```", "")
        .Trim();
}
```

**Recommendation**: Claude models (especially 3.5 Sonnet and Opus 4.5) have excellent instruction-following. With clear instructions and temperature=0.1, JSON formatting is highly reliable without native JSON mode.

---

### Gemini: Native JSON Schema (Best Approach)

**Strategy**: Use `responseMimeType` + optional `responseSchema`

```csharp
var generationConfig = new
{
    temperature = 0.1,
    topP = 0.3,
    topK = 10,
    maxOutputTokens = 8192,
    responseMimeType = "application/json",
    responseSchema = new
    {
        type = "object",
        properties = new
        {
            titulo = new { type = "string", nullable = true },
            autores = new
            {
                type = "array",
                items = new { type = "string" }
            },
            ano = new { type = "integer", nullable = true },
            resumo = new { type = "string", nullable = true },
            especies = new
            {
                type = "array",
                items = new
                {
                    type = "object",
                    properties = new
                    {
                        nome_vernacular = new { type = "string", nullable = true },
                        nome_cientifico = new { type = "string", nullable = true },
                        tipo_uso = new { type = "string", nullable = true },
                        parte_usada = new { type = "string", nullable = true },
                        preparacao = new { type = "string", nullable = true }
                    }
                }
            },
            comunidade = new
            {
                type = "object",
                properties = new
                {
                    nome = new { type = "string", nullable = true },
                    localizacao = new { type = "string", nullable = true }
                }
            },
            pais = new { type = "string", nullable = true },
            estado = new { type = "string", nullable = true },
            municipio = new { type = "string", nullable = true },
            local = new { type = "string", nullable = true },
            bioma = new { type = "string", nullable = true },
            metodologia = new { type = "string", nullable = true },
            ano_coleta = new { type = "integer", nullable = true }
        }
    }
};
```

**Advantages**:
- Guaranteed valid JSON structure
- Schema validation built-in
- No post-processing needed
- Handles null values correctly

**Recommendation**: This is the most robust approach. Use `responseMimeType` alone for simpler implementation, add `responseSchema` for guaranteed structure.

---

## 3. Parameter Mappings

### OLLAMA Parameters (Current)
```csharp
options = new
{
    temperature = 0.1,      // Very low = strict instruction following
    top_p = 0.3,            // Conservative nucleus sampling
    top_k = 10,             // Very conservative
    num_predict = 8000      // Allow full response
}
```

### OpenAI Equivalent
```csharp
{
    "temperature": 0.1,     // Range: 0.0 - 2.0 (same meaning)
    "top_p": 0.3,           // Range: 0.0 - 1.0 (same meaning)
    // NO top_k parameter (not supported)
    "max_tokens": 8000      // Equivalent to num_predict
}
```

**Notes**:
- OpenAI does NOT support `top_k`
- Use `temperature` + `top_p` together for deterministic output
- `max_tokens` includes both prompt and response (budget accordingly)

---

### Anthropic Equivalent
```csharp
{
    "temperature": 0.1,     // Range: 0.0 - 1.0 (same meaning)
    "top_p": 0.3,           // Range: 0.0 - 1.0 (same meaning)
    "top_k": 10,            // Range: 0 - 500 (same meaning) ✅
    "max_tokens": 8000      // Equivalent to num_predict
}
```

**Notes**:
- Anthropic SUPPORTS `top_k` (matches OLLAMA exactly)
- Claude models are very instruction-following at low temperature
- `max_tokens` is OUTPUT ONLY (prompt doesn't count against limit)

---

### Gemini Equivalent
```csharp
{
    "temperature": 0.1,         // Range: 0.0 - 2.0 (same meaning)
    "topP": 0.3,                // Range: 0.0 - 1.0 (camelCase!)
    "topK": 10,                 // Range: 1 - 40 (camelCase!) ✅
    "maxOutputTokens": 8192     // camelCase, higher limit possible
}
```

**Notes**:
- Gemini uses camelCase: `topP`, `topK`, `maxOutputTokens`
- `topK` range is smaller (1-40), but 10 is valid
- Can go higher on tokens (up to 32K on Gemini 1.5 Pro)

---

### Unified Parameter Helper Class

```csharp
public class ExtractionParameters
{
    public double Temperature { get; set; } = 0.1;
    public double TopP { get; set; } = 0.3;
    public int TopK { get; set; } = 10;
    public int MaxTokens { get; set; } = 8000;

    public object ToOpenAI() => new
    {
        temperature = Temperature,
        top_p = TopP,
        // top_k not supported
        max_tokens = MaxTokens
    };

    public object ToAnthropic() => new
    {
        temperature = Temperature,
        top_p = TopP,
        top_k = TopK,
        max_tokens = MaxTokens
    };

    public object ToGemini() => new
    {
        temperature = Temperature,
        topP = TopP,
        topK = TopK,
        maxOutputTokens = MaxTokens
    };
}
```

---

## 4. Translation Requirement: Resumo in Brazilian Portuguese

### Challenge
- PDF abstracts may be in English, Spanish, Portuguese, or other languages
- Requirement: `resumo` field MUST be in Brazilian Portuguese
- Question: Single extraction call or separate translation call?

### Approach 1: In-Prompt Translation (Recommended)

**Prompt Instruction**:
```
4. Abstract/Resumo:
   - Find the section titled "Abstract", "Resumo", or similar
   - If the abstract is in English or any other language, translate it to Brazilian Portuguese
   - Use natural, academic Brazilian Portuguese (avoid literal translations)
   - If no abstract exists, return null
   - ALWAYS return resumo in Brazilian Portuguese
```

**Advantages**:
- Single API call (faster, cheaper)
- All providers support multilingual instruction following
- Maintains context of full document during translation
- Benchmark tests show this works well (see `claude.json` and `gemini.json`)

**Disadvantages**:
- Quality depends on model's Portuguese proficiency
- May introduce translation errors

---

### Approach 2: Separate Translation Call (If Needed)

**When to use**:
- If provider consistently fails to translate correctly
- If you need higher translation quality than extraction quality

**Implementation**:
```csharp
// Step 1: Extract with original language
var extractedData = await ExtractMetadataAsync(pdfMarkdown);

// Step 2: Check if resumo is in Portuguese (heuristic)
if (!string.IsNullOrEmpty(extractedData.resumo) && !IsBrazilianPortuguese(extractedData.resumo))
{
    // Step 3: Translate resumo separately
    extractedData.resumo = await TranslateToPortugueseAsync(extractedData.resumo);
}

private async Task<string> TranslateToPortugueseAsync(string text)
{
    var prompt = $@"Translate the following scientific abstract to Brazilian Portuguese.
Use natural, academic language. Return only the translation, no explanations.

TEXT TO TRANSLATE:
{text}";

    return await CallProviderAPI(prompt);
}
```

**Advantages**:
- Higher translation quality (dedicated translation prompt)
- Can use cheaper/faster models for translation step
- Easier to debug translation issues

**Disadvantages**:
- Two API calls (slower, more expensive)
- Loses document context during translation

---

### Provider-Specific Translation Quality

Based on benchmark tests (`claude.json` vs `gemini.json`):

**Claude (Anthropic)**:
```json
"resumo": "Este estudo foi desenvolvido como parte do projeto 'Estudos para
Sustentabilidade Ambiental e Cultural do Sistema Médico Fulni-ô: Oficina sobre
manejo de plantas medicinais'. O povo Fulni-ô está localizado no Estado de
Pernambuco, Nordeste do Brasil..."
```
- Excellent Brazilian Portuguese quality
- Natural phrasing, correct grammar
- Academic tone maintained

**Gemini (Google)**:
```json
"resumo": "This study was developed as part of the project \"Studies for
Environmental and Cultural Sustainability of the Fulni-ô Medical System: Office
on handling medicinal plants\". The Fulni-ô people are located in Pernambuco
State, Northeastern Brazil..."
```
- **Did NOT translate** (kept English!)
- Indicates prompt instruction was missed or model failed to follow

**OpenAI (Expected)**:
- GPT-4 has excellent multilingual capabilities
- Should handle Portuguese translation well (on par with Claude)
- Requires testing to confirm

---

### Recommendation

**Use Approach 1 (in-prompt translation) with explicit instruction**:

```
4. Abstract/Resumo - CRITICAL REQUIREMENT:
   - Locate the abstract section (may be titled "Abstract", "Resumo", "Resumen", etc.)
   - If the abstract is in ANY language other than Portuguese, translate it to Brazilian Portuguese
   - Translation must be natural, academic, and use Brazilian Portuguese conventions
   - If no abstract exists in the document, return null
   - NEVER return the resumo in English, Spanish, or any language other than Portuguese
   - This is a MANDATORY field requirement
```

**Add post-extraction validation**:
```csharp
// After extraction
if (!string.IsNullOrEmpty(extracted.resumo) && !IsBrazilianPortuguese(extracted.resumo))
{
    // Fallback: Separate translation call
    extracted.resumo = await TranslateToPortugueseAsync(extracted.resumo);
}

private bool IsBrazilianPortuguese(string text)
{
    // Simple heuristic: check for Portuguese-specific patterns
    var portugueseIndicators = new[] { "ção", "ões", "ê", "á", "ã", "çã" };
    var englishIndicators = new[] { "tion", "the", "and", "this", "that" };

    int portugueseScore = portugueseIndicators.Count(p => text.Contains(p));
    int englishScore = englishIndicators.Count(p => text.Contains(p));

    return portugueseScore > englishScore;
}
```

---

## 5. Anti-Hallucination Techniques

### Current Approach (prompt2.md) - Excellent Results

```
TAREFA: Extrair APENAS dados encontrados no documento. NÃO invente, infira ou gere dados.

Se uma informação NÃO está no documento, retorne null. NÃO use "N/A", "desconhecido"
ou valores padrão.

REGRAS ABSOLUTAS:
1. COPIE EXATAMENTE do documento. NÃO reescreva, NÃO resuma, NÃO interprete.
2. SE NÃO ESTÁ NO DOCUMENTO → use null. NUNCA invente valores plausíveis.
3. titulo: copie palavra por palavra do documento
4. autores: copie os nomes EXATAMENTE como aparecem
5. NUNCA complete campos faltantes com inferências
```

**This approach achieved 78% accuracy in benchmarks.**

---

### Provider-Specific Anti-Hallucination Strategies

#### OpenAI (GPT-4)
- Naturally verbose, may add explanations
- Mitigation: "Return ONLY JSON. No explanations."
- Use `presence_penalty: 0.0` and `frequency_penalty: 0.0` (default)
- Set `temperature: 0.0` for maximum determinism

#### Anthropic (Claude)
- Excellent instruction-following (best of three providers)
- Naturally cautious about hallucination
- Mitigation: Standard prompt + temperature=0.1 is sufficient
- Use constitutional AI principles in prompt: "You must not fabricate information"

#### Gemini (Google)
- Can be over-confident in extracting non-existent data
- Mitigation: **Emphasize "null for missing fields" multiple times**
- Use `responseMimeType: "application/json"` to reduce freeform text
- Add explicit examples showing null usage

---

### Recommended Anti-Hallucination Prompt Structure

```
CRITICAL EXTRACTION RULES - READ CAREFULLY:

1. ONLY EXTRACT: Copy information that explicitly appears in the document
2. NEVER INFER: Do not guess, infer, or generate plausible values
3. MISSING = NULL: If a field is not found in the document, return null
4. NO DEFAULTS: Never use "N/A", "unknown", "not specified", or empty strings
5. EXACT COPY: Copy text exactly as it appears (no paraphrasing)

SPECIFIC FIELD RULES:
- titulo: Copy exact title from document (usually first # heading)
- autores: Copy author names exactly as written (do not reformat)
- ano: Extract 4-digit year ONLY if explicitly stated
- resumo: Translate abstract to Brazilian Portuguese if needed, or null if absent
- especies: ONLY list plants explicitly named in the document
- pais/estado/municipio: Copy exact location names, null if not found
- metodologia: Copy methods description verbatim, null if not found

QUALITY CHECK:
Before returning JSON, verify:
✓ Every field contains ONLY information from the document
✓ No invented or inferred data
✓ Missing fields are null (not empty strings)
✓ Names, titles, locations copied exactly
```

---

### Post-Processing Validation

```csharp
public class ExtractionValidator
{
    public ValidationResult Validate(ExtractionResult result, string originalMarkdown)
    {
        var issues = new List<string>();

        // Check for empty strings (should be null)
        if (result.resumo == "") issues.Add("resumo is empty string, should be null");
        if (result.bioma == "") issues.Add("bioma is empty string, should be null");

        // Check for suspicious patterns (hallucination indicators)
        if (result.pais == "N/A" || result.pais == "unknown")
            issues.Add("pais contains placeholder value");

        // Verify titulo exists in markdown
        if (!originalMarkdown.Contains(result.titulo))
            issues.Add($"titulo '{result.titulo}' not found in document");

        // Check for valid year range
        if (result.ano.HasValue && (result.ano < 1500 || result.ano > 2026))
            issues.Add($"ano {result.ano} is outside valid range");

        // Verify resumo is in Portuguese (if not null)
        if (!string.IsNullOrEmpty(result.resumo) && !IsBrazilianPortuguese(result.resumo))
            issues.Add("resumo is not in Brazilian Portuguese");

        return new ValidationResult
        {
            IsValid = issues.Count == 0,
            Issues = issues
        };
    }
}
```

---

## 6. Recommended Implementation Strategy

### Phase 1: Unified Abstraction Layer

Create a provider-agnostic interface:

```csharp
public interface ICloudAIProvider
{
    string ProviderName { get; }
    Task<string> ExtractMetadataAsync(string markdownContent, string customPrompt = null);
    Task<bool> CheckHealthAsync();
}

public class OpenAIProvider : ICloudAIProvider
{
    public string ProviderName => "OpenAI";

    public async Task<string> ExtractMetadataAsync(string markdownContent, string customPrompt = null)
    {
        var systemMessage = customPrompt ?? GenerateDefaultPrompt();
        var messages = new[]
        {
            new { role = "system", content = systemMessage },
            new { role = "user", content = markdownContent }
        };

        var requestBody = new
        {
            model = "gpt-4-turbo-preview",
            messages,
            response_format = new { type = "json_object" },
            temperature = 0.1,
            top_p = 0.3,
            max_tokens = 8000
        };

        // HTTP request to OpenAI API...
    }
}

public class AnthropicProvider : ICloudAIProvider { /* similar */ }
public class GeminiProvider : ICloudAIProvider { /* similar */ }
```

---

### Phase 2: Provider-Specific Prompt Formatting

```csharp
private string FormatPromptForProvider(string basePrompt, string markdown)
{
    return ProviderName switch
    {
        "OpenAI" => basePrompt, // System message, user gets markdown
        "Anthropic" => basePrompt, // System param, user gets markdown
        "Gemini" => $"{basePrompt}\n\nDOCUMENT TO ANALYZE:\n{markdown}", // Combined
        _ => throw new NotSupportedException()
    };
}
```

---

### Phase 3: Unified Response Processing

```csharp
private ExtractionResult ParseResponse(string rawResponse)
{
    // Remove markdown code blocks if present (some providers may add them)
    var cleaned = rawResponse.Trim();
    if (cleaned.StartsWith("```json"))
    {
        cleaned = cleaned
            .Replace("```json", "")
            .Replace("```", "")
            .Trim();
    }

    // Parse JSON
    var extracted = JsonConvert.DeserializeObject<ExtractionResult>(cleaned);

    // Post-processing: Convert empty strings to null
    if (string.IsNullOrWhiteSpace(extracted.resumo)) extracted.resumo = null;
    if (string.IsNullOrWhiteSpace(extracted.bioma)) extracted.bioma = null;

    foreach (var especie in extracted.especies ?? new List<Especie>())
    {
        if (string.IsNullOrWhiteSpace(especie.preparacao)) especie.preparacao = null;
    }

    // Validate and fallback translation if needed
    if (!string.IsNullOrEmpty(extracted.resumo) && !IsBrazilianPortuguese(extracted.resumo))
    {
        extracted.resumo = await TranslateToPortugueseAsync(extracted.resumo);
    }

    return extracted;
}
```

---

## 7. Expected Provider Performance

Based on benchmark results and API capabilities:

| Feature | OpenAI (GPT-4) | Anthropic (Claude) | Gemini (1.5 Pro) |
|---------|----------------|-------------------|------------------|
| JSON Reliability | ⭐⭐⭐⭐⭐ (Native mode) | ⭐⭐⭐⭐ (Prompt) | ⭐⭐⭐⭐⭐ (Native mode + schema) |
| Instruction Following | ⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ (Best) | ⭐⭐⭐⭐ |
| Anti-Hallucination | ⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ (Best) | ⭐⭐⭐ (Needs strong prompt) |
| Portuguese Translation | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ (Proven) | ⚠️ ⭐⭐⭐ (Benchmark failed) |
| Context Length | 128K tokens | 200K tokens | 1M tokens (Best) |
| Speed | Fast | Medium | Very Fast |
| Cost (per 1K tokens) | $0.01/$0.03 | $0.003/$0.015 | $0.00125/$0.005 |

**Recommendations**:
1. **Claude (Anthropic)**: Best overall accuracy, excellent Portuguese, proven results
2. **Gemini**: Best value, fastest, but needs careful prompt tuning for Portuguese
3. **OpenAI**: Solid middle ground, good for users with existing OpenAI accounts

---

## 8. Code Examples

### OpenAI Implementation

```csharp
public async Task<string> ExtractWithOpenAI(string markdownContent, string apiKey)
{
    using var client = new HttpClient();
    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

    var systemPrompt = GenerateExtractionPrompt(); // From OLLAMAService.cs
    var requestBody = new
    {
        model = "gpt-4-turbo-preview",
        messages = new[]
        {
            new { role = "system", content = systemPrompt },
            new { role = "user", content = markdownContent }
        },
        response_format = new { type = "json_object" },
        temperature = 0.1,
        top_p = 0.3,
        max_tokens = 8000
    };

    var content = new StringContent(
        JsonConvert.SerializeObject(requestBody),
        Encoding.UTF8,
        "application/json"
    );

    var response = await client.PostAsync(
        "https://api.openai.com/v1/chat/completions",
        content
    );

    var responseContent = await response.Content.ReadAsStringAsync();
    dynamic result = JsonConvert.DeserializeObject(responseContent);

    return result.choices[0].message.content.ToString();
}
```

---

### Anthropic Implementation

```csharp
public async Task<string> ExtractWithAnthropic(string markdownContent, string apiKey)
{
    using var client = new HttpClient();
    client.DefaultRequestHeaders.Add("x-api-key", apiKey);
    client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

    var systemPrompt = GenerateExtractionPrompt();
    var requestBody = new
    {
        model = "claude-3-5-sonnet-20241022",
        system = systemPrompt,
        messages = new[]
        {
            new { role = "user", content = markdownContent }
        },
        max_tokens = 8000,
        temperature = 0.1,
        top_p = 0.3,
        top_k = 10
    };

    var content = new StringContent(
        JsonConvert.SerializeObject(requestBody),
        Encoding.UTF8,
        "application/json"
    );

    var response = await client.PostAsync(
        "https://api.anthropic.com/v1/messages",
        content
    );

    var responseContent = await response.Content.ReadAsStringAsync();
    dynamic result = JsonConvert.DeserializeObject(responseContent);

    return result.content[0].text.ToString();
}
```

---

### Gemini Implementation

```csharp
public async Task<string> ExtractWithGemini(string markdownContent, string apiKey)
{
    using var client = new HttpClient();

    var fullPrompt = $@"{GenerateExtractionPrompt()}

DOCUMENT TO ANALYZE:
{markdownContent}";

    var requestBody = new
    {
        contents = new[]
        {
            new
            {
                role = "user",
                parts = new[] { new { text = fullPrompt } }
            }
        },
        generationConfig = new
        {
            temperature = 0.1,
            topP = 0.3,
            topK = 10,
            maxOutputTokens = 8192,
            responseMimeType = "application/json"
            // Optional: Add responseSchema here for strict validation
        }
    };

    var content = new StringContent(
        JsonConvert.SerializeObject(requestBody),
        Encoding.UTF8,
        "application/json"
    );

    var response = await client.PostAsync(
        $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-pro:generateContent?key={apiKey}",
        content
    );

    var responseContent = await response.Content.ReadAsStringAsync();
    dynamic result = JsonConvert.DeserializeObject(responseContent);

    return result.candidates[0].content.parts[0].text.ToString();
}
```

---

## 9. Testing Strategy

### Benchmark Test Suite

Use existing benchmark PDF (`docs/pdfTest/Rapid_ethnobotanical_diagnosis_of_the_Fu.pdf`) with all three providers:

1. **Extract with each provider** using identical prompts
2. **Compare against reference** (`docs/pdfTest/ollama.json`)
3. **Score accuracy**:
   - Header fields (titulo, autores, ano, resumo): 4 fields
   - Geographic fields (pais, estado, municipio, local, bioma): 5 fields
   - Research metadata (metodologia, ano_coleta): 2 fields
   - Community data (comunidade.nome, comunidade.localizacao): 2 fields
   - Species count and details: Variable

### Success Criteria

- **Minimum**: 75% field completion (matches current OLLAMA performance)
- **Target**: 85%+ field completion
- **Mandatory**: Resumo MUST be in Brazilian Portuguese for all providers
- **Critical**: No hallucinated species or locations

### Automated Test

```csharp
[Test]
public async Task TestAllProviders()
{
    var markdown = LoadBenchmarkPDF(); // From docs/pdfTest
    var reference = LoadReference(); // docs/pdfTest/ollama.json

    var providers = new ICloudAIProvider[]
    {
        new OpenAIProvider(apiKeys["openai"]),
        new AnthropicProvider(apiKeys["anthropic"]),
        new GeminiProvider(apiKeys["gemini"])
    };

    foreach (var provider in providers)
    {
        var result = await provider.ExtractMetadataAsync(markdown);
        var score = CompareToReference(result, reference);

        Console.WriteLine($"{provider.ProviderName}: {score.Percentage}% accurate");

        Assert.IsTrue(score.Percentage >= 75,
            $"{provider.ProviderName} failed: {score.Percentage}%");
        Assert.IsTrue(IsBrazilianPortuguese(result.resumo),
            $"{provider.ProviderName}: resumo not in Portuguese");
    }
}
```

---

## 10. Migration Checklist

- [ ] Create `ICloudAIProvider` interface
- [ ] Implement `OpenAIProvider` class
- [ ] Implement `AnthropicProvider` class
- [ ] Implement `GeminiProvider` class
- [ ] Update configuration UI to select provider + enter API key
- [ ] Store API keys in local config (exclude from version control)
- [ ] Add parameter mapping helper (`ExtractionParameters` class)
- [ ] Implement unified prompt formatting
- [ ] Add post-processing for empty string → null conversion
- [ ] Add Portuguese validation + fallback translation
- [ ] Create benchmark test suite for all providers
- [ ] Test with existing benchmark PDF
- [ ] Validate 75%+ accuracy for each provider
- [ ] Update documentation (README.md in Brazilian Portuguese)
- [ ] Remove OLLAMA-specific code
- [ ] Deploy and monitor first production extractions

---

## 11. Conclusion

**Key Findings**:

1. **Prompt Compatibility**: The existing OLLAMA prompt structure is highly compatible with all three cloud providers with minimal adaptation.

2. **JSON Output**:
   - OpenAI: Use `response_format: {type: "json_object"}`
   - Anthropic: Use prompt engineering (highly reliable)
   - Gemini: Use `responseMimeType: "application/json"` (best approach)

3. **Parameters**: All three support temperature and top_p. Only Anthropic and Gemini support top_k (OpenAI ignores it).

4. **Translation**: Use in-prompt translation for efficiency. Add fallback validation for providers that miss the instruction (especially Gemini based on benchmark).

5. **Anti-Hallucination**: The existing `prompt2.md` approach is excellent and works across providers with minimal changes.

6. **Recommended Provider Ranking**:
   1. **Anthropic (Claude)**: Best accuracy and instruction-following (proven in benchmarks)
   2. **OpenAI (GPT-4)**: Solid middle ground, widely adopted
   3. **Gemini**: Best value and speed, but needs careful Portuguese prompt tuning

**Implementation Recommendation**: Start with Anthropic (Claude 3.5 Sonnet) as the default recommended provider due to proven benchmark results, then add OpenAI and Gemini as alternatives. All three should be supported to give users choice based on their existing subscriptions and preferences.

---

**Document Version**: 1.0
**Last Updated**: 2025-12-09
**Status**: Ready for Implementation
