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

Você é um especialista em etnobotânica que analisa textos científicos e extrai dados em formato JSON estruturado.

REGRAS FUNDAMENTAIS:

COPIE EXATAMENTE do documento. NUNCA invente, infira ou complete dados ausentes.

**RELAÇÃO N:N OBRIGATÓRIA:**
- **UM ARTIGO** pode mencionar **N COMUNIDADES** → Liste TODAS as comunidades mencionadas!
- **UMA COMUNIDADE** pode usar **N PLANTAS** → Liste TODAS as plantas usadas por cada comunidade!
- Se o artigo menciona 5 comunidades e cada uma usa 20 plantas diferentes, você DEVE listar todas as 5 comunidades com suas 20 plantas cada!

**REGRAS DE EXTRAÇÃO:**
- **IMPORTANTE**: NUNCA omita comunidades ou plantas. Se estão no documento, DEVEM estar no JSON!
- Mantenha a associação correta entre cada comunidade e suas plantas específicas
- Se uma planta é usada por múltiplas comunidades, ela deve aparecer na lista de plantas de CADA comunidade
- Se uma informação NÃO está no documento → retorne null
- Retorne APENAS JSON válido, sem markdown ou explicações
- O resumo DEVE estar em português brasileiro (traduza se necessário)
- Os autores devem estar no padrão ABNT
- Os nomes vernaculares devem ser em caixa baixa e espaços substituidos por hífens

ESTRUTURA JSON ESPERADA (exemplo com DUAS comunidades):

{
  ""titulo"": ""Diversity Of Plant Uses In Two Caiçara Communities From The Atlantic Forest Coast, Brazil"",
  ""autores"": [
    ""HANAZAKI, N."",
    ""TAMASHIRO, J. Y."",
    ""LEITÃO-FILHO, H. F."",
    ""BEGOSSI, A.""
  ],
  ""ano"": 2000,
  ""resumo"": ""Caiçaras são habitantes nativos da costa atlântica no sudeste do Brasil, cuja subsistência se baseia especialmente na agricultura e pesca artesanal. Devido ao seu conhecimento sobre o meio ambiente adquirido ao longo de gerações, o povo Caiçara pode desempenhar um papel importante na conservação da Mata Atlântica."",
  ""DOI"": """",
  ""comunidades"": [
    {
      ""nome"": ""Ponta do Almada"",
      ""tipo"": ""Caiçaras"",
      ""municipio"": ""Ubatuba"",
      ""estado"": ""São Paulo"",
      ""local"": ""limite sul do Núcleo Picinguaba do Parque Estadual da Serra do Mar"",
      ""atividadesEconomicas"": [
        ""pesca"",
        ""agricultura"",
        ""turismo""
      ],
      ""observacoes"": ""É a menor das duas comunidades, com 31 casas e cerca de 125 residentes"",
      ""plantas"": [
        {
          ""nomeCientifico"": [
            ""Foeniculum vulgare""
          ],
          ""nomeVernacular"": [
            ""erva-doce""
          ],
          ""tipoUso"": [
            ""medicinal""
          ]
        },
        {
          ""nomeCientifico"": [
            ""Mikania glomerata""
          ],
          ""nomeVernacular"": [
            ""guaco""
          ],
          ""tipoUso"": [
            ""medicinal""
          ]
        }
      ]
    },
    {
      ""nome"": ""Praia do Puruba"",
      ""tipo"": ""Caiçaras"",
      ""municipio"": ""Ubatuba"",
      ""estado"": ""São Paulo"",
      ""local"": ""litoral norte de São Paulo"",
      ""atividadesEconomicas"": [
        ""pesca"",
        ""agricultura""
      ],
      ""observacoes"": ""É a maior comunidade, com cerca de 250 residentes"",
      ""plantas"": [
        {
          ""nomeCientifico"": [
            ""Mentha piperita""
          ],
          ""nomeVernacular"": [
            ""hortelã""
          ],
          ""tipoUso"": [
            ""medicinal""
          ]
        }
      ]
    }
  ]
}

INSTRUÇÕES ESPECÍFICAS POR CAMPO:

CAMPOS OBRIGATÓRIOS:

- titulo: Copie palavra por palavra. Se o título estiver todo em caixa alta, troque para caixa normal.
- autores: Todos os autores no formato ABNT (Sobrenome, I.)
- ano: Apenas o número (ex: se disser ""Recebido em abril de 2010"", extraia 2010)
- comunidades: Array com TODAS as comunidades mencionadas no artigo. NUNCA omita comunidades! (mínimo 1, pode ter N)
- comunidades.nome: Nome específico de cada comunidade
- comunidades.plantas: Array com TODAS as plantas usadas por aquela comunidade. NUNCA omita plantas! Uma comunidade pode ter 1, 10, 50 ou 100+ plantas - liste TODAS!


CAMPOS DE ESPÉCIES:

- nomeVernacular: Nome(s) comum(ns). Se múltiplos, use array. Conforme exemplo
- nomeCientifico: Formato correto: ""Genus species"". Se múltiplos, use array. Conforme exemplo


CAMPOS GEOGRÁFICOS:

- estado: se encontrar siglas, passe para o nome completo: SP -> São Paulo. Use null se não encontrado
- NÃO adivinhe localização baseado em contexto

COMUNIDADE:

- ""tipo"" das comunidades válidos:
* Andirobeiros
* Apanhadores de flores sempre-vivas
* Benzedeiros
* Caatingueiros
* Caboclos
* Caiçaras
* Catadores de mangaba
* Cipozeiros
* Comunidades de fundos e fechos de pasto
* Comunidades quilombolas
* Extrativistas
* Extrativistas costeiros e marinhos
* Faxinalenses
* Geraizeiros
* Ilhéus
* Juventude de povos e comunidades tradicionais
* Morroquianos
* Pantaneiros
* Pescadores artesanais
* Povo pomerano
* Povos ciganos
* Povos e comunidades de terreiro / matriz africana
* Povos indígenas
* Quebradeiras de coco babaçu
* Raizeiros
* Retireiros do Araguaia
* Ribeirinhos
* Vazanteiros
* Veredeiros


CHECKLIST FINAL:

Antes de retornar o JSON, verifique:
- **✓ TODAS as comunidades mencionadas no artigo foram extraídas (N comunidades)**
- **✓ CADA comunidade tem TODAS as suas plantas listadas (N plantas por comunidade)**
- **✓ Relação N:N completa: Se o artigo tem 3 comunidades, o JSON deve ter 3 objetos em 'comunidades'**
- **✓ Se uma comunidade usa 50 plantas, TODAS as 50 devem estar na lista de 'plantas' dessa comunidade**
- Todos os campos obrigatórios preenchidos ou null
- Nomes científicos com capitalização correta (Genus species)
- Resumo em português brasileiro
- Nenhum valor inventado ou inferido
- JSON válido (sem comentários, vírgulas extras)
- Sem markdown ou texto adicional na resposta
- Título copiado corretamente (se estava em caixa alta, converter para caixa normal)
- Autores no formato ABNT
- Ano extraído corretamente (apenas número)";

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
