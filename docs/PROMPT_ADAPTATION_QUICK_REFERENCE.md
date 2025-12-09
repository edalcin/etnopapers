# Prompt Adaptation Quick Reference

**Quick guide for adapting EtnoPapers extraction prompts to cloud AI providers**

---

## Base Prompt (Anti-Hallucination Version)

Use `docs/pdfTest/prompt2.md` as the foundation. This prompt achieved 78% accuracy with OLLAMA.

**Key elements to preserve**:
```
TAREFA: Extrair APENAS dados encontrados no documento. NÃO invente, infira ou gere dados.

REGRAS ABSOLUTAS:
1. COPIE EXATAMENTE do documento
2. SE NÃO ESTÁ NO DOCUMENTO → use null
3. NUNCA complete campos faltantes com inferências
```

---

## OpenAI Adaptation

### Message Structure
```json
{
  "model": "gpt-4-turbo-preview",
  "messages": [
    {
      "role": "system",
      "content": "[EXTRACTION RULES + EXAMPLE JSON]"
    },
    {
      "role": "user",
      "content": "[PDF MARKDOWN CONTENT]"
    }
  ],
  "response_format": { "type": "json_object" },
  "temperature": 0.1,
  "top_p": 0.3,
  "max_tokens": 8000
}
```

### System Message Template
```
You are an expert ethnobotanical metadata extractor.

CRITICAL RULES:
1. Extract ONLY information that explicitly appears in the document
2. NEVER invent, infer, or generate plausible data
3. Use null for missing fields (not "N/A", not empty strings)
4. Copy text exactly as it appears in the document

MANDATORY FIELD REQUIREMENT:
- resumo: MUST be in Brazilian Portuguese
- If abstract is in English/Spanish, translate to Brazilian Portuguese
- If no abstract exists, return null

JSON OUTPUT STRUCTURE:
{
  "titulo": "string or null",
  "autores": ["string"],
  "ano": integer or null,
  "resumo": "string in Brazilian Portuguese or null",
  "especies": [
    {
      "nome_vernacular": "string or null",
      "nome_cientifico": "string or null",
      "tipo_uso": "string or null",
      "parte_usada": "string or null",
      "preparacao": "string or null"
    }
  ],
  "comunidade": {
    "nome": "string or null",
    "localizacao": "string or null"
  },
  "pais": "string or null",
  "estado": "string or null",
  "municipio": "string or null",
  "local": "string or null",
  "bioma": "string or null",
  "metodologia": "string or null",
  "ano_coleta": integer or null
}

EXTRACTION GUIDELINES:
- titulo: First # heading in document
- autores: Names as they appear (do not reformat)
- ano: 4-digit year (1500-2026) from publication date
- especies: ONLY plants explicitly named in document
- metodologia: Research methods description (exact copy)
- ano_coleta: Data collection year if different from publication year

Return ONLY valid JSON. No explanations, no markdown code blocks.
```

### User Message
```
[Insert full PDF markdown content here]
```

---

## Anthropic (Claude) Adaptation

### Message Structure
```json
{
  "model": "claude-3-5-sonnet-20241022",
  "system": "[EXTRACTION RULES + EXAMPLE JSON]",
  "messages": [
    {
      "role": "user",
      "content": "[PDF MARKDOWN CONTENT]"
    }
  ],
  "max_tokens": 8000,
  "temperature": 0.1,
  "top_p": 0.3,
  "top_k": 10
}
```

### System Prompt Template
```
You are an expert ethnobotanical metadata extractor analyzing scientific papers about traditional plant use by indigenous and traditional communities.

EXTRACTION PRINCIPLES:
You must follow these principles strictly:
1. Extract only information that explicitly appears in the document
2. Never fabricate, infer, or generate plausible data
3. Use null for fields not found in the document
4. Copy text exactly as written (no paraphrasing)

MANDATORY REQUIREMENTS:
- resumo: ALWAYS in Brazilian Portuguese
  • If the abstract is in English, Spanish, or any other language, translate it to natural Brazilian Portuguese
  • Use academic tone appropriate for scientific papers
  • If no abstract exists, return null

JSON OUTPUT FORMAT:
{
  "titulo": "exact title from document",
  "autores": ["author names exactly as written"],
  "ano": 2010,
  "resumo": "abstract in Brazilian Portuguese",
  "especies": [
    {
      "nome_vernacular": "vernacular name from text",
      "nome_cientifico": "scientific name from text",
      "tipo_uso": "medicinal/alimentar/ritual/etc",
      "parte_usada": "folhas/raiz/casca/etc",
      "preparacao": "chá/decocção/etc"
    }
  ],
  "comunidade": {
    "nome": "community name",
    "localizacao": "full location description"
  },
  "pais": "country name",
  "estado": "state/province name",
  "municipio": "municipality name",
  "local": "specific location description",
  "bioma": "biome name",
  "metodologia": "research methodology description",
  "ano_coleta": 2010
}

FIELD-SPECIFIC INSTRUCTIONS:
- titulo: Usually the first # heading in the markdown
- autores: Copy names exactly, including initials and formatting
- ano: Extract 4-digit year only (validate: 1500-2026)
- especies: List ONLY plants explicitly mentioned with identifiable names
- geographic fields: Copy exact location names from document
- metodologia: Verbatim copy of methods section
- ano_coleta: Data collection year if stated separately from publication year

QUALITY CHECKS:
Before returning, verify:
✓ All data comes from the document (no invented content)
✓ Missing fields are null (not empty strings, not "N/A")
✓ resumo is in Brazilian Portuguese
✓ Year values are realistic (1500-2026)
✓ Scientific names follow binomial nomenclature

Return ONLY the JSON object. No markdown code blocks (no ```json), no explanations.
```

### User Message
```
[Insert full PDF markdown content here]
```

---

## Gemini Adaptation

### Message Structure
```json
{
  "contents": [
    {
      "role": "user",
      "parts": [
        {
          "text": "[FULL PROMPT + PDF MARKDOWN CONTENT]"
        }
      ]
    }
  ],
  "generationConfig": {
    "temperature": 0.1,
    "topP": 0.3,
    "topK": 10,
    "maxOutputTokens": 8192,
    "responseMimeType": "application/json"
  }
}
```

### Full Prompt Template (Combined)
```
TASK: Extract ethnobotanical metadata from the scientific paper below.

CRITICAL EXTRACTION RULES - READ CAREFULLY:

1. ONLY EXTRACT DATA FOUND IN THE DOCUMENT
   - Do NOT invent, infer, or generate plausible information
   - If a field is not in the document, return null
   - Copy information exactly as it appears

2. NULL FOR MISSING FIELDS
   - Use null, not "N/A", not "unknown", not empty strings
   - Better to return null than guess or infer

3. RESUMO MUST BE IN BRAZILIAN PORTUGUESE
   - If the abstract is in English: translate to Brazilian Portuguese
   - If the abstract is in Spanish: translate to Brazilian Portuguese
   - If no abstract exists: return null
   - Use natural, academic Brazilian Portuguese

4. EXACT COPYING
   - titulo: Copy word-for-word from document
   - autores: Copy names exactly as they appear
   - especies: List ONLY plants explicitly named in the text
   - locations: Copy geographic names exactly

JSON STRUCTURE TO RETURN:
{
  "titulo": "exact title or null",
  "autores": ["author names"],
  "ano": 2010,
  "resumo": "BRAZILIAN PORTUGUESE translation of abstract or null",
  "especies": [
    {
      "nome_vernacular": "vernacular name or null",
      "nome_cientifico": "scientific name or null",
      "tipo_uso": "medicinal/alimentar/etc or null",
      "parte_usada": "folhas/raiz/etc or null",
      "preparacao": "chá/decocção/etc or null"
    }
  ],
  "comunidade": {
    "nome": "community name or null",
    "localizacao": "location description or null"
  },
  "pais": "country or null",
  "estado": "state or null",
  "municipio": "municipality or null",
  "local": "specific location or null",
  "bioma": "biome or null",
  "metodologia": "methods description or null",
  "ano_coleta": 2010
}

FIELD EXTRACTION GUIDE:
- titulo: First # heading in markdown
- autores: Names in order as listed (no reformatting)
- ano: 4-digit publication year (must be 1500-2026)
- resumo: Abstract section → translate to Portuguese if needed
- especies: Plants with vernacular or scientific names
- pais/estado/municipio/local: Geographic data from text
- bioma: Ecosystem type if mentioned
- metodologia: Research methods section
- ano_coleta: Year data was collected (if different from publication)

IMPORTANT REMINDERS:
- Return ONLY valid JSON
- No markdown code blocks
- No explanations or commentary
- Use null for missing fields (NOT empty strings)
- resumo MUST be in Brazilian Portuguese

DOCUMENT TO ANALYZE:

[Insert full PDF markdown content here]
```

---

## Parameter Reference Table

| Parameter | OpenAI | Anthropic | Gemini | EtnoPapers Value |
|-----------|--------|-----------|--------|------------------|
| Temperature | `temperature` | `temperature` | `temperature` | `0.1` |
| Top-P | `top_p` | `top_p` | `topP` | `0.3` |
| Top-K | ❌ Not supported | `top_k` | `topK` | `10` |
| Max Tokens | `max_tokens` | `max_tokens` | `maxOutputTokens` | `8000` |
| JSON Mode | `response_format: {type: "json_object"}` | ❌ Prompt only | `responseMimeType: "application/json"` | Required |

---

## Response Parsing

### All Providers: Handle Markdown Code Blocks

Some models may wrap JSON in markdown despite instructions:

```csharp
private string CleanJsonResponse(string rawResponse)
{
    var cleaned = rawResponse.Trim();

    // Remove markdown code blocks if present
    if (cleaned.StartsWith("```json"))
    {
        cleaned = cleaned.Replace("```json", "").Replace("```", "").Trim();
    }
    else if (cleaned.StartsWith("```"))
    {
        cleaned = cleaned.Replace("```", "").Trim();
    }

    return cleaned;
}
```

---

## Post-Processing: Convert Empty Strings to Null

```csharp
private void NormalizeNullValues(ExtractionResult result)
{
    // Top-level fields
    if (string.IsNullOrWhiteSpace(result.resumo)) result.resumo = null;
    if (string.IsNullOrWhiteSpace(result.bioma)) result.bioma = null;
    if (string.IsNullOrWhiteSpace(result.local)) result.local = null;
    if (string.IsNullOrWhiteSpace(result.metodologia)) result.metodologia = null;

    // Community
    if (result.comunidade != null)
    {
        if (string.IsNullOrWhiteSpace(result.comunidade.nome))
            result.comunidade.nome = null;
        if (string.IsNullOrWhiteSpace(result.comunidade.localizacao))
            result.comunidade.localizacao = null;
    }

    // Species
    foreach (var especie in result.especies ?? new List<Especie>())
    {
        if (string.IsNullOrWhiteSpace(especie.nome_vernacular))
            especie.nome_vernacular = null;
        if (string.IsNullOrWhiteSpace(especie.nome_cientifico))
            especie.nome_cientifico = null;
        if (string.IsNullOrWhiteSpace(especie.tipo_uso))
            especie.tipo_uso = null;
        if (string.IsNullOrWhiteSpace(especie.parte_usada))
            especie.parte_usada = null;
        if (string.IsNullOrWhiteSpace(especie.preparacao))
            especie.preparacao = null;
    }
}
```

---

## Portuguese Translation Validation

```csharp
private bool IsBrazilianPortuguese(string text)
{
    if (string.IsNullOrWhiteSpace(text)) return true; // null is OK

    // Portuguese-specific character patterns
    var portuguesePatterns = new[]
    {
        "ção", "ões", "ção", "ã", "õ", "á", "é", "ê", "ú", "ô"
    };

    // English patterns that shouldn't appear in Portuguese
    var englishPatterns = new[]
    {
        " the ", " and ", " this ", " that ", " with ", " from "
    };

    int portugueseScore = portuguesePatterns.Count(p => text.Contains(p));
    int englishScore = englishPatterns.Count(p =>
        text.ToLower().Contains(p.ToLower()));

    // If significantly more English patterns than Portuguese, likely English
    return portugueseScore > 0 || englishScore == 0;
}

private async Task<string> TranslateToPortugueseAsync(string text)
{
    var translatePrompt = $@"Translate the following scientific abstract to Brazilian Portuguese.
Use natural, academic language appropriate for scientific papers.
Return ONLY the translation, no explanations.

TEXT TO TRANSLATE:
{text}";

    // Call same provider API with translation prompt
    return await CallProviderAPI(translatePrompt, temperature: 0.3);
}
```

---

## Testing Checklist

For each provider, verify:

- [ ] Extracts titulo correctly (exact match to document)
- [ ] Extracts all autores (count and names match)
- [ ] Extracts ano within valid range (1500-2026)
- [ ] resumo is in Brazilian Portuguese (not English/Spanish)
- [ ] Geographic fields populated from document (pais, estado, municipio)
- [ ] especies array contains only plants mentioned in document
- [ ] Missing fields are null (not empty strings, not "N/A")
- [ ] No hallucinated data (all fields verified against source)
- [ ] JSON is valid and parseable
- [ ] Response completes without truncation

**Target Accuracy**: 75% minimum, 85%+ ideal

---

## API Endpoint Reference

### OpenAI
```
POST https://api.openai.com/v1/chat/completions
Authorization: Bearer {API_KEY}
Content-Type: application/json
```

### Anthropic
```
POST https://api.anthropic.com/v1/messages
x-api-key: {API_KEY}
anthropic-version: 2023-06-01
Content-Type: application/json
```

### Gemini
```
POST https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-pro:generateContent?key={API_KEY}
Content-Type: application/json
```

---

## Recommended Models

| Provider | Recommended Model | Alternative |
|----------|------------------|-------------|
| OpenAI | `gpt-4-turbo-preview` | `gpt-4o` |
| Anthropic | `claude-3-5-sonnet-20241022` | `claude-opus-4-5-20251101` |
| Gemini | `gemini-1.5-pro` | `gemini-2.0-flash-exp` |

---

**Quick Reference Version**: 1.0
**Last Updated**: 2025-12-09
**For detailed research**: See `MULTI_PROVIDER_PROMPT_RESEARCH.md`
