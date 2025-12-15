using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace EtnoPapers.Core.Services;

/// <summary>
/// Abstract base class for cloud AI provider implementations.
/// Provides common functionality like retry logic, error handling, and prompt management.
/// </summary>
public abstract class AIProviderService : IAIProvider
{
    protected readonly ILogger Logger;
    protected readonly HttpClient HttpClient;
    protected string? ApiKey;

    /// <summary>
    /// Gets the name of the AI provider (for logging and error messages).
    /// </summary>
    protected abstract string ProviderName { get; }

    /// <summary>
    /// Temperature for AI response generation (0.1 for deterministic extraction).
    /// </summary>
    protected const float Temperature = 0.1f;

    /// <summary>
    /// Maximum tokens for AI response.
    /// </summary>
    protected const int MaxTokens = 8000;

    /// <summary>
    /// Top-p (nucleus sampling) parameter for response generation.
    /// </summary>
    protected const float TopP = 0.3f;

    protected AIProviderService(HttpClient? httpClient = null)
    {
        Logger = Log.ForContext(GetType());
        HttpClient = httpClient ?? new HttpClient();
    }

    /// <summary>
    /// Sets the API key for authentication.
    /// </summary>
    public virtual void SetApiKey(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new ArgumentException("API key cannot be null or empty.", nameof(apiKey));

        ApiKey = apiKey;
    }

    /// <summary>
    /// Tests connectivity to the AI provider without requiring valid metadata structure.
    /// Used for validating API keys and connectivity.
    /// </summary>
    public virtual async Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        ValidateApiKey();

        // Simple test that doesn't require full metadata extraction
        var simplePrompt = "Responda com um simples 'ok' em JSON: {\"status\": \"ok\"}";

        try
        {
            var result = await ExtractMetadataAsync(simplePrompt, null, cancellationToken);
            return !string.IsNullOrEmpty(result);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Extracts ethnobotanical metadata from PDF text.
    /// </summary>
    public abstract Task<string> ExtractMetadataAsync(string pdfText, string customPrompt = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Default system prompt for ethnobotanical extraction.
    /// </summary>
    public static readonly string DefaultExtractionPrompt = @"SISTEMA DE EXTRAÇÃO DE METADADOS ETNOBOTÂNICOS

Você é um especialista em etnobotânica que analisa textos científicos e extrai metadados em formato JSON estruturado.

REGRAS FUNDAMENTAIS:

COPIE EXATAMENTE do documento. NUNCA invente, infira ou complete dados ausentes.

- Se uma informação NÃO está no documento -> retorne null
- Se está no documento -> copie EXATAMENTE como aparece
- Retorne APENAS JSON válido, sem markdown ou explicações
- O resumo DEVE estar em português brasileiro (traduza se necessário)

ESTRUTURA JSON ESPERADA:

{
  ""titulo"": ""string em Proper Case (copiado exatamente do documento)"",
  ""autores"": [""array com autores no padrão ABNT""],
  ""ano"": 2010,
  ""resumo"": ""sempre em português brasileiro"",
  ""especies"": [
    {
      ""vernacular"": ""nome comum ou array se houver vários"",
      ""nomeCientifico"": ""Genus species ou array se houver vários"",
      ""tipoUso"": ""medicinal|alimentação|construção|ritual|artesanato|combustível|forrageiro|outro""
    }
  ],
  ""pais"": ""string ou null"",
  ""estado"": ""string ou null"",
  ""municipio"": ""string ou null"",
  ""local"": ""string ou null"",
  ""bioma"": ""string ou null"",
  ""comunidade"": {
    ""nome"": ""string"",
    ""localizacao"": ""string""
  },
  ""metodologia"": ""string ou null""
}

INSTRUÇÕES ESPECÍFICAS POR CAMPO:

CAMPOS OBRIGATÓRIOS:

- titulo: Copie palavra por palavra, normalize para Proper Case
- autores: Todos os autores no formato ABNT (Sobrenome, I.)
- ano: Apenas o número (ex: se disser ""Recebido em abril de 2010"", extraia 2010)
- resumo: Se houver abstract/resumo, copie. Se em outra língua, traduza para português brasileiro. Se não houver, use null

CAMPOS DE ESPÉCIES:

- vernacular: Nome(s) comum(ns). Se múltiplos, use array
- nomeCientifico: Formato correto: ""Genus species"". Se múltiplos, use array
- tipoUso: Apenas valores da lista permitida

CAMPOS GEOGRÁFICOS:

- Copie EXATAMENTE como aparecem no texto
- Use null se não encontrado
- NÃO adivinhe localização baseado em contexto

COMUNIDADE:

- Se mencionada, extraia nome e localização
- Use null para o objeto inteiro se não mencionada

VALIDAÇÕES AUTOMÁTICAS:

- Nomes científicos: devem seguir padrão ""Genus species"" (primeira letra maiúscula)
- Ano: deve estar entre 1500 e ano atual + 1
- Datas: extraia apenas o ano (YYYY)
- Tipos de uso: devem estar na lista permitida

VALORES PERMITIDOS PARA tipoUso:

- medicinal: Uso medicinal/terapêutico
- alimentação: Uso alimentar
- construção: Uso em construções
- ritual: Uso ritual/religioso/espiritual
- artesanato: Uso em artesanato
- combustível: Uso como combustível/energia
- forrageiro: Uso como forragem para animais
- outro: Outros usos não categorizados

CHECKLIST FINAL:

Antes de retornar o JSON, verifique:
- Todos os campos obrigatórios preenchidos ou null
- Nomes científicos com capitalização correta (Genus species)
- Resumo em português brasileiro
- Nenhum valor inventado ou inferido
- JSON válido (sem comentários, vírgulas extras)
- Sem markdown ou texto adicional na resposta
- Título em Proper Case
- Autores no formato ABNT
- Ano extraído corretamente (apenas número)
- Tipos de uso dentro da lista permitida";

    /// <summary>
    /// Gets the extraction prompt, using custom prompt if provided, otherwise default.
    /// </summary>
    protected string GetExtractionPrompt(string customPrompt = null)
    {
        if (!string.IsNullOrWhiteSpace(customPrompt))
            return customPrompt;

        return DefaultExtractionPrompt;
    }

    /// <summary>
    /// Validates that API key is configured before making API calls.
    /// </summary>
    protected void ValidateApiKey()
    {
        if (string.IsNullOrEmpty(ApiKey))
            throw new InvalidOperationException($"API key not configured for {ProviderName}");
    }

    /// <summary>
    /// Parses JSON response and validates it contains required fields.
    /// </summary>
    protected static JObject ParseJsonResponse(string response)
    {
        try
        {
            var jObject = JsonConvert.DeserializeObject<JObject>(response)
                ?? throw new InvalidOperationException("Response deserialized to null");

            ValidateRequiredFields(jObject);
            return jObject;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Invalid JSON response from AI provider", ex);
        }
    }

    /// <summary>
    /// Validates that all mandatory fields are present in the extracted metadata.
    /// </summary>
    private static void ValidateRequiredFields(JObject metadata)
    {
        var requiredFields = new[] { "titulo", "autores", "ano", "resumo" };
        foreach (var field in requiredFields)
        {
            if (metadata[field] == null)
                throw new InvalidOperationException($"Missing required field: {field}");
        }
    }
}
