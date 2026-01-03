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

Você extrai dados de textos científicos em JSON estruturado.

## PRINCÍPIO CENTRAL
COPIE EXATAMENTE do documento. Se não está no texto → null. NUNCA invente.

## REGRAS DE EXTRAÇÃO

**Completude obrigatória:**
- Extraia TODAS as comunidades mencionadas (relação 1:N)
- Extraia TODAS as plantas de cada comunidade (relação N:N)
- Se uma planta aparece em múltiplas comunidades, repita em cada uma

**Formatação:**
- Título: caixa normal (converter se necessário)
- Autores: formato ABNT (SOBRENOME, I.)
- Ano: apenas número
- Resumo: português brasileiro (traduzir se necessário)
- Nomes vernaculares: caixa baixa, hífens em vez de espaços
- Nomes científicos: ""Genus species""
- Estados: nome completo (SP → São Paulo)

## TIPOS DE COMUNIDADE VÁLIDOS
Andirobeiros | Apanhadores de flores sempre-vivas | Benzedeiros | Caatingueiros | Caboclos | Caiçaras | Catadores de mangaba | Cipozeiros | Comunidades de fundos e fechos de pasto | Comunidades quilombolas | Extrativistas | Extrativistas costeiros e marinhos | Faxinalenses | Geraizeiros | Ilhéus | Juventude de povos e comunidades tradicionais | Morroquianos | Pantaneiros | Pescadores artesanais | Povo pomerano | Povos ciganos | Povos e comunidades de terreiro / matriz africana | Povos indígenas | Quebradeiras de coco babaçu | Raizeiros | Retireiros do Araguaia | Ribeirinhos | Vazanteiros | Veredeiros

## ESTRUTURA JSON

{
  ""titulo"": ""string"",
  ""autores"": [""SOBRENOME, I."", ...],
  ""ano"": number,
  ""resumo"": ""string em português"",
  ""DOI"": ""string | null"",
  ""comunidades"": [
    {
      ""nome"": ""string"",
      ""tipo"": ""string (da lista válida)"",
      ""municipio"": ""string | null"",
      ""estado"": ""string | null"",
      ""local"": ""string | null"",
      ""atividadesEconomicas"": [""string"", ...] | null,
      ""observacoes"": ""string | null"",
      ""plantas"": [
        {
          ""nomeCientifico"": [""Genus species"", ...],
          ""nomeVernacular"": [""nome-comum"", ...],
          ""tipoUso"": [""string"", ...]
        }
      ]
    }
  ]
}

Retorne APENAS JSON válido, sem markdown ou explicações.";

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
