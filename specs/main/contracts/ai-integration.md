# Integração com APIs de IA: Etnopapers

**Funcionalidade**: Sistema de Extração de Metadados de Artigos Etnobotânicos
**Branch**: main
**Criado**: 2025-11-20
**Status**: Em Desenvolvimento

## Visão Geral

Este documento especifica como o frontend do Etnopapers se integra diretamente com APIs externas de IA (Google Gemini, OpenAI ChatGPT, Anthropic Claude) para extração de metadados de PDFs, sem passar pelo backend.

## Arquitetura de Integração

```
┌─────────────┐
│  Navegador  │
│  (Frontend) │
└──────┬──────┘
       │
       │ 1. localStorage.getItem('apiKey')
       ▼
┌─────────────────┐
│  PDF Upload     │
│  + Text Extract │ ◄─── pdf.js extrai texto do PDF
└──────┬──────────┘
       │
       │ 2. HTTPS direto (CORS habilitado)
       │
       ▼
┌──────────────────────────────┐
│  API de IA Externa           │
│  ├─ Gemini (Google)          │
│  ├─ ChatGPT (OpenAI)         │
│  └─ Claude (Anthropic)       │
└──────┬───────────────────────┘
       │
       │ 3. JSON com metadados extraídos
       ▼
┌─────────────────┐
│  Frontend       │
│  Exibe/Edita    │
└──────┬──────────┘
       │
       │ 4. POST /api/articles (somente metadados)
       ▼
┌─────────────────┐
│  Backend        │
│  SQLite         │
└─────────────────┘
```

**Fluxo Detalhado**:
1. Usuário faz upload de PDF no frontend
2. Frontend usa `pdf.js` para extrair texto do PDF
3. Frontend monta prompt estruturado + texto extraído
4. Frontend faz request HTTPS direto para API de IA selecionada usando chave do localStorage
5. API de IA retorna JSON com metadados
6. Frontend exibe metadados extraídos
7. Usuário clica "Salvar" → frontend POST para backend com metadados
8. Backend valida taxonomia e salva no SQLite

## Google Gemini API

### Endpoint

```
POST https://generativelanguage.googleapis.com/v1/models/gemini-pro:generateContent
```

### Headers

```http
Content-Type: application/json
x-goog-api-key: {user_api_key}
```

### Request Body

```json
{
  "contents": [
    {
      "parts": [
        {
          "text": "{prompt_completo}"
        }
      ]
    }
  ],
  "generationConfig": {
    "temperature": 0.1,
    "topK": 1,
    "topP": 1,
    "maxOutputTokens": 2048
  }
}
```

### Response Format

```json
{
  "candidates": [
    {
      "content": {
        "parts": [
          {
            "text": "{json_com_metadados}"
          }
        ]
      },
      "finishReason": "STOP"
    }
  ],
  "usageMetadata": {
    "promptTokenCount": 1234,
    "candidatesTokenCount": 567,
    "totalTokenCount": 1801
  }
}
```

### Código Frontend (TypeScript)

```typescript
interface GeminiRequest {
  contents: { parts: { text: string }[] }[];
  generationConfig: {
    temperature: number;
    topK: number;
    topP: number;
    maxOutputTokens: number;
  };
}

async function extractWithGemini(
  pdfText: string,
  apiKey: string
): Promise<Metadata> {
  const prompt = buildPrompt(pdfText);

  const requestBody: GeminiRequest = {
    contents: [
      {
        parts: [{ text: prompt }],
      },
    ],
    generationConfig: {
      temperature: 0.1,
      topK: 1,
      topP: 1,
      maxOutputTokens: 2048,
    },
  };

  const response = await fetch(
    "https://generativelanguage.googleapis.com/v1/models/gemini-pro:generateContent",
    {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        "x-goog-api-key": apiKey,
      },
      body: JSON.stringify(requestBody),
    }
  );

  if (!response.ok) {
    throw new Error(`Gemini API error: ${response.statusText}`);
  }

  const data = await response.json();
  const extractedText = data.candidates[0].content.parts[0].text;

  // Parse JSON extraído
  return parseMetadataJSON(extractedText);
}
```

### Validação de API Key

```typescript
async function validateGeminiKey(apiKey: string): Promise<boolean> {
  try {
    const response = await fetch(
      "https://generativelanguage.googleapis.com/v1/models/gemini-pro:generateContent",
      {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          "x-goog-api-key": apiKey,
        },
        body: JSON.stringify({
          contents: [{ parts: [{ text: "test" }] }],
          generationConfig: { maxOutputTokens: 5 },
        }),
      }
    );

    return response.ok;
  } catch {
    return false;
  }
}
```

---

## OpenAI ChatGPT API

### Endpoint

```
POST https://api.openai.com/v1/chat/completions
```

### Headers

```http
Content-Type: application/json
Authorization: Bearer {user_api_key}
```

### Request Body

```json
{
  "model": "gpt-3.5-turbo",
  "messages": [
    {
      "role": "system",
      "content": "Você é um assistente especializado em extrair metadados de artigos científicos sobre etnobotânica. Responda APENAS com JSON válido."
    },
    {
      "role": "user",
      "content": "{prompt_com_texto_pdf}"
    }
  ],
  "temperature": 0.1,
  "max_tokens": 2048
}
```

### Response Format

```json
{
  "id": "chatcmpl-xyz",
  "object": "chat.completion",
  "created": 1234567890,
  "model": "gpt-3.5-turbo",
  "choices": [
    {
      "index": 0,
      "message": {
        "role": "assistant",
        "content": "{json_com_metadados}"
      },
      "finish_reason": "stop"
    }
  ],
  "usage": {
    "prompt_tokens": 1234,
    "completion_tokens": 567,
    "total_tokens": 1801
  }
}
```

### Código Frontend (TypeScript)

```typescript
interface OpenAIRequest {
  model: string;
  messages: { role: string; content: string }[];
  temperature: number;
  max_tokens: number;
}

async function extractWithChatGPT(
  pdfText: string,
  apiKey: string
): Promise<Metadata> {
  const prompt = buildPrompt(pdfText);

  const requestBody: OpenAIRequest = {
    model: "gpt-3.5-turbo",
    messages: [
      {
        role: "system",
        content:
          "Você é um assistente especializado em extrair metadados de artigos científicos sobre etnobotânica. Responda APENAS com JSON válido no formato especificado.",
      },
      {
        role: "user",
        content: prompt,
      },
    ],
    temperature: 0.1,
    max_tokens: 2048,
  };

  const response = await fetch(
    "https://api.openai.com/v1/chat/completions",
    {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${apiKey}`,
      },
      body: JSON.stringify(requestBody),
    }
  );

  if (!response.ok) {
    throw new Error(`OpenAI API error: ${response.statusText}`);
  }

  const data = await response.json();
  const extractedText = data.choices[0].message.content;

  return parseMetadataJSON(extractedText);
}
```

### Validação de API Key

```typescript
async function validateOpenAIKey(apiKey: string): Promise<boolean> {
  try {
    const response = await fetch(
      "https://api.openai.com/v1/chat/completions",
      {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${apiKey}`,
        },
        body: JSON.stringify({
          model: "gpt-3.5-turbo",
          messages: [{ role: "user", content: "test" }],
          max_tokens: 5,
        }),
      }
    );

    return response.ok;
  } catch {
    return false;
  }
}
```

---

## Anthropic Claude API

### Endpoint

```
POST https://api.anthropic.com/v1/messages
```

### Headers

```http
Content-Type: application/json
x-api-key: {user_api_key}
anthropic-version: 2023-06-01
```

### Request Body

```json
{
  "model": "claude-3-haiku-20240307",
  "max_tokens": 2048,
  "temperature": 0.1,
  "messages": [
    {
      "role": "user",
      "content": "{prompt_com_texto_pdf}"
    }
  ],
  "system": "Você é um assistente especializado em extrair metadados de artigos científicos sobre etnobotânica. Responda APENAS com JSON válido."
}
```

### Response Format

```json
{
  "id": "msg_xyz",
  "type": "message",
  "role": "assistant",
  "content": [
    {
      "type": "text",
      "text": "{json_com_metadados}"
    }
  ],
  "model": "claude-3-haiku-20240307",
  "stop_reason": "end_turn",
  "usage": {
    "input_tokens": 1234,
    "output_tokens": 567
  }
}
```

### Código Frontend (TypeScript)

```typescript
interface ClaudeRequest {
  model: string;
  max_tokens: number;
  temperature: number;
  messages: { role: string; content: string }[];
  system: string;
}

async function extractWithClaude(
  pdfText: string,
  apiKey: string
): Promise<Metadata> {
  const prompt = buildPrompt(pdfText);

  const requestBody: ClaudeRequest = {
    model: "claude-3-haiku-20240307",
    max_tokens: 2048,
    temperature: 0.1,
    messages: [
      {
        role: "user",
        content: prompt,
      },
    ],
    system:
      "Você é um assistente especializado em extrair metadados de artigos científicos sobre etnobotânica. Responda APENAS com JSON válido no formato especificado.",
  };

  const response = await fetch("https://api.anthropic.com/v1/messages", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      "x-api-key": apiKey,
      "anthropic-version": "2023-06-01",
    },
    body: JSON.stringify(requestBody),
  });

  if (!response.ok) {
    throw new Error(`Claude API error: ${response.statusText}`);
  }

  const data = await response.json();
  const extractedText = data.content[0].text;

  return parseMetadataJSON(extractedText);
}
```

### Validação de API Key

```typescript
async function validateClaudeKey(apiKey: string): Promise<boolean> {
  try {
    const response = await fetch("https://api.anthropic.com/v1/messages", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        "x-api-key": apiKey,
        "anthropic-version": "2023-06-01",
      },
      body: JSON.stringify({
        model: "claude-3-haiku-20240307",
        max_tokens: 5,
        messages: [{ role: "user", content: "test" }],
      }),
    });

    return response.ok;
  } catch {
    return false;
  }
}
```

---

## Prompt Template para Extração

```typescript
function buildPrompt(pdfText: string): string {
  return `
Analise o seguinte artigo científico sobre etnobotânica e extraia os metadados especificados.

IMPORTANTE: Responda APENAS com JSON válido no formato especificado abaixo. Não adicione texto extra antes ou depois do JSON.

FORMATO DE RESPOSTA:
{
  "titulo": "string",
  "ano_publicacao": number,
  "autores": ["string", "string"],
  "resumo": "string",
  "doi": "string ou null",
  "local_publicacao": "string ou null",
  "regioes": [
    {
      "descricao": "string",
      "pais": "string ou null",
      "estado_provincia": "string ou null",
      "latitude": number ou null,
      "longitude": number ou null
    }
  ],
  "comunidades": [
    {
      "nome": "string",
      "tipo_comunidade": "indígena|quilombola|ribeirinha|caiçara|seringueira|pantaneira|outro ou null"
    }
  ],
  "especies": [
    {
      "nome_cientifico": "string (formato binomial: Genus species)",
      "nomes_vernaculares": ["string", "string"],
      "contexto_uso": "string",
      "parte_planta_utilizada": ["folha", "raiz", "casca"]
    }
  ],
  "dados_estudo": {
    "periodo_inicio": "YYYY-MM-DD ou null",
    "periodo_fim": "YYYY-MM-DD ou null",
    "metodos_coleta_dados": "string ou null",
    "tipo_amostragem": "string ou null",
    "tamanho_amostra": number ou null,
    "instrumentos_coleta": ["string", "string"] ou null
  }
}

INSTRUÇÕES:
- Se um campo não puder ser identificado no artigo, use null ou array vazio [] conforme o tipo
- Nomes científicos DEVEM estar no formato binomial (Genus species), sem autores
- Extraia TODOS os autores listados
- Anos devem ser números inteiros (ex: 2023)
- DOI deve estar no formato 10.xxxx/xxxx
- Tipo de comunidade deve ser um dos valores especificados ou null
- Nomes vernaculares são nomes populares/comuns das plantas
- Datas devem estar no formato YYYY-MM-DD

TEXTO DO ARTIGO:
${pdfText}

JSON:
`.trim();
}
```

---

## Parsing de Resposta JSON

```typescript
interface Metadata {
  titulo: string;
  ano_publicacao: number;
  autores: string[];
  resumo: string | null;
  doi: string | null;
  local_publicacao: string | null;
  regioes: Region[];
  comunidades: Community[];
  especies: Species[];
  dados_estudo: StudyData | null;
}

function parseMetadataJSON(text: string): Metadata {
  // Remove possíveis markdown code blocks (```json ... ```)
  let cleanedText = text.trim();

  const jsonBlockMatch = cleanedText.match(/```json?\s*([\s\S]*?)\s*```/);
  if (jsonBlockMatch) {
    cleanedText = jsonBlockMatch[1];
  }

  // Tenta fazer parse do JSON
  try {
    const parsed = JSON.parse(cleanedText);
    validateMetadata(parsed);
    return parsed as Metadata;
  } catch (error) {
    throw new Error(
      `Erro ao fazer parse do JSON retornado pela IA: ${error.message}`
    );
  }
}

function validateMetadata(data: any): void {
  if (!data.titulo || typeof data.titulo !== "string") {
    throw new Error("Campo 'titulo' é obrigatório e deve ser string");
  }

  if (
    !data.ano_publicacao ||
    typeof data.ano_publicacao !== "number" ||
    data.ano_publicacao < 1900 ||
    data.ano_publicacao > 2100
  ) {
    throw new Error(
      "Campo 'ano_publicacao' é obrigatório e deve ser número entre 1900-2100"
    );
  }

  if (!Array.isArray(data.autores) || data.autores.length === 0) {
    throw new Error(
      "Campo 'autores' é obrigatório e deve ser array não-vazio"
    );
  }

  // Validações adicionais...
}
```

---

## Tratamento de Erros

### Erros Comuns e Soluções

**Erro 401 Unauthorized**:
- Causa: API key inválida ou expirada
- Solução: Exibir mensagem pedindo ao usuário para atualizar chave

**Erro 429 Too Many Requests**:
- Causa: Limite de requisições excedido
- Solução: Exibir mensagem pedindo para aguardar alguns minutos

**Erro 500 Internal Server Error**:
- Causa: Problema temporário na API de IA
- Solução: Permitir retry após 30 segundos

**JSON Malformado**:
- Causa: IA retornou texto ao invés de JSON válido
- Solução: Exibir mensagem e permitir edição manual

### Código de Tratamento

```typescript
async function extractMetadataWithRetry(
  pdfText: string,
  provider: "gemini" | "openai" | "claude",
  apiKey: string,
  maxRetries: number = 3
): Promise<Metadata> {
  let lastError: Error | null = null;

  for (let attempt = 1; attempt <= maxRetries; attempt++) {
    try {
      switch (provider) {
        case "gemini":
          return await extractWithGemini(pdfText, apiKey);
        case "openai":
          return await extractWithChatGPT(pdfText, apiKey);
        case "claude":
          return await extractWithClaude(pdfText, apiKey);
      }
    } catch (error) {
      lastError = error;

      // Se erro 401, não tentar retry
      if (error.message.includes("401")) {
        throw new Error("Chave de API inválida. Por favor, atualize sua chave.");
      }

      // Se erro 429, aguardar antes de retry
      if (error.message.includes("429")) {
        if (attempt < maxRetries) {
          await sleep(30000); // 30 segundos
          continue;
        }
      }

      // Se último retry, lançar erro
      if (attempt === maxRetries) {
        break;
      }

      // Aguardar antes de retry
      await sleep(2000 * attempt);
    }
  }

  throw new Error(
    `Falha ao extrair metadados após ${maxRetries} tentativas: ${lastError?.message}`
  );
}

function sleep(ms: number): Promise<void> {
  return new Promise((resolve) => setTimeout(resolve, ms));
}
```

---

## Extração de Texto do PDF com pdf.js

```typescript
import * as pdfjsLib from "pdfjs-dist";

// Configurar worker
pdfjsLib.GlobalWorkerOptions.workerSrc = "/pdf.worker.js";

async function extractTextFromPDF(file: File): Promise<string> {
  const arrayBuffer = await file.arrayBuffer();
  const pdf = await pdfjsLib.getDocument({ data: arrayBuffer }).promise;

  let fullText = "";

  for (let pageNum = 1; pageNum <= pdf.numPages; pageNum++) {
    const page = await pdf.getPage(pageNum);
    const textContent = await page.getTextContent();
    const pageText = textContent.items.map((item: any) => item.str).join(" ");
    fullText += pageText + "\n\n";
  }

  return fullText;
}
```

---

## Estimativa de Custos por Artigo

### Gemini (Recomendado)

- **Quota Gratuita**: 60 requests/minuto
- **Custo após quota**: ~$0.00025 por 1K caracteres
- **Artigo de 30 páginas (50K chars)**: ~$0.0125 (1 centavo)

### ChatGPT

- **Modelo gpt-3.5-turbo**: $0.0015 por 1K tokens
- **Artigo de 30 páginas (~12.5K tokens)**: ~$0.01875 (2 centavos)

### Claude

- **Modelo claude-3-haiku**: $0.00025 por 1K tokens
- **Artigo de 30 páginas (~12.5K tokens)**: ~$0.003125 (menos de 1 centavo)

---

## Segurança e Privacidade

### Armazenamento de API Keys

```typescript
// Salvar chave
function saveApiKey(provider: string, key: string): void {
  localStorage.setItem(`etnopapers_api_key_${provider}`, key);
}

// Recuperar chave
function getApiKey(provider: string): string | null {
  return localStorage.getItem(`etnopapers_api_key_${provider}`);
}

// Remover chave
function clearApiKey(provider: string): void {
  localStorage.removeItem(`etnopapers_api_key_${provider}`);
}
```

### Importante

- **API keys NUNCA são enviadas ao backend**
- **API keys permanecem apenas no navegador do usuário**
- **Se usuário limpar cache do navegador, precisará reconfigurar**
- **Frontend faz chamadas HTTPS diretas para APIs de IA**

---

## Próximos Passos

1. Implementar componente React para upload de PDF
2. Implementar extração de texto com pdf.js
3. Implementar integração com as 3 APIs de IA
4. Criar interface de configuração de API key
5. Implementar tratamento de erros robusto
6. Adicionar testes unitários para parsing de JSON

## Referências

- [Google Gemini API Docs](https://ai.google.dev/docs)
- [OpenAI API Reference](https://platform.openai.com/docs/api-reference)
- [Anthropic Claude API](https://docs.anthropic.com/claude/reference)
- [pdf.js Documentation](https://mozilla.github.io/pdf.js/)
