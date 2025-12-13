using Newtonsoft.Json;
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
    /// Extracts ethnobotanical metadata from PDF text.
    /// </summary>
    public abstract Task<string> ExtractMetadataAsync(string pdfText, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the system prompt for ethnobotanical extraction.
    /// This is reused across all providers for consistency.
    /// </summary>
    protected string GetExtractionPrompt() => @"Você é um especialista em etnobotânica. Analise o seguinte texto científico e extraia os metadados etnobotânicos em formato JSON estritamente válido.

IMPORTANTE:
1. Responda SOMENTE com JSON válido, sem explicações adicionais
2. O resumo DEVE estar em português brasileiro
3. Todos os campos obrigatórios devem ser preenchidos
4. Use nomes científicos com capitalização correta (Genus species)
5. Datas devem estar no formato YYYY

Estrutura esperada:
{
  ""titulo"": ""string normalizando em Proper Case"",
  ""autores"": [""Sobrenome, I.""],
  ""ano"": número,
  ""resumo"": ""sempre em português brasileiro"",
  ""especies"": [
    {
      ""vernacular"": ""nome comum"",
      ""nomeCientifico"": ""Genus species"",
      ""tipoUso"": ""medicinal|alimentação|construção|ritual|artesanato|combustível|forrageiro|outro""
    }
  ],
  ""pais"": ""string"",
  ""estado"": ""string"",
  ""municipio"": ""string"",
  ""local"": ""string"",
  ""bioma"": ""string"",
  ""comunidade"": {
    ""nome"": ""string"",
    ""localizacao"": ""string""
  },
  ""metodologia"": ""string""
}";

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

// Import for JSON handling
using Newtonsoft.Json.Linq;
