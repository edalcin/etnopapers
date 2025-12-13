using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using EtnoPapers.Core.Utils;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EtnoPapers.Core.Services;

/// <summary>
/// Google Gemini AI provider implementation for ethnobotanical metadata extraction.
/// Uses Gemini 1.5 Flash model via REST API.
/// </summary>
public class GeminiService : AIProviderService
{
    private const string ApiEndpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent";

    protected override string ProviderName => "Gemini";

    public GeminiService(HttpClient? httpClient = null) : base(httpClient)
    {
        HttpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    /// <summary>
    /// Extracts ethnobotanical metadata from PDF text using Gemini API.
    /// </summary>
    public override async Task<string> ExtractMetadataAsync(string pdfText, CancellationToken cancellationToken = default)
    {
        ValidateApiKey();

        if (string.IsNullOrWhiteSpace(pdfText))
            throw new ArgumentException("PDF text cannot be empty", nameof(pdfText));

        var prompt = GetExtractionPrompt() + "\n\n" + pdfText;

        try
        {
            var response = await RetryHelper.ExecuteWithRetryAsync(
                async ct => await CallGeminiApiAsync(prompt, ct),
                $"Extract metadata from {ProviderName}",
                cancellationToken: cancellationToken);

            return response;
        }
        catch (HttpRequestException ex)
        {
            var statusCode = CloudErrorMapper.TryExtractStatusCode(ex);
            var message = CloudErrorMapper.GetErrorMessage(ex, statusCode);
            Logger.Error(ex, "[{Provider}] API error: {StatusCode}", ProviderName, statusCode);
            throw new InvalidOperationException(message, ex);
        }
    }

    /// <summary>
    /// Calls Gemini API and returns extracted JSON metadata.
    /// </summary>
    private async Task<string> CallGeminiApiAsync(string prompt, CancellationToken cancellationToken)
    {
        // Build request payload according to Gemini API format
        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[] { new { text = prompt } }
                }
            },
            generationConfig = new
            {
                temperature = Temperature,
                topP = TopP,
                maxOutputTokens = MaxTokens
            }
        };

        var json = JsonConvert.SerializeObject(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Add API key to query parameter (Gemini uses this instead of headers)
        var urlWithKey = $"{ApiEndpoint}?key={ApiKey}";

        Logger.Debug("[{Provider}] Sending extraction request (prompt length: {Length})",
            ProviderName, prompt.Length);

        var response = await HttpClient.PostAsync(urlWithKey, content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Logger.Error("[{Provider}] API returned {StatusCode}: {Error}",
                ProviderName, response.StatusCode, errorContent);

            // Throw HttpRequestException - the StatusCode will be available in .NET 5+
            response.EnsureSuccessStatusCode();
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        Logger.Debug("[{Provider}] Received response (length: {Length})",
            ProviderName, responseContent.Length);

        // Parse Gemini response format
        return ExtractJsonFromGeminiResponse(responseContent);
    }

    /// <summary>
    /// Extracts JSON metadata from Gemini API response.
    /// Response format: { "candidates": [{ "content": { "parts": [{ "text": "..." }] } }] }
    /// </summary>
    private string ExtractJsonFromGeminiResponse(string responseContent)
    {
        try
        {
            var jResponse = JObject.Parse(responseContent);
            var candidates = jResponse["candidates"] as JArray;

            if (candidates == null || candidates.Count == 0)
                throw new InvalidOperationException("No candidates in Gemini response");

            var firstCandidate = candidates[0] as JObject;
            var content = firstCandidate?["content"] as JObject;
            var parts = content?["parts"] as JArray;

            if (parts == null || parts.Count == 0)
                throw new InvalidOperationException("No parts in Gemini response");

            var text = parts[0]["text"]?.ToString();

            if (string.IsNullOrEmpty(text))
                throw new InvalidOperationException("Empty text in Gemini response");

            Logger.Debug("[{Provider}] Extracted text from response (length: {Length})",
                ProviderName, text.Length);

            // Clean and validate the extracted JSON
            var cleanedJson = CleanJsonResponse(text);
            var validatedJson = ParseJsonResponse(cleanedJson);

            return validatedJson.ToString(Formatting.None);
        }
        catch (JsonException ex)
        {
            Logger.Error(ex, "[{Provider}] Failed to parse Gemini response", ProviderName);
            throw new InvalidOperationException("Invalid response format from Gemini", ex);
        }
    }

    /// <summary>
    /// Cleans JSON response by removing markdown code blocks and extra formatting.
    /// </summary>
    private static string CleanJsonResponse(string text)
    {
        var cleaned = text.Trim();

        // Remove markdown code blocks if present (```json ... ```)
        if (cleaned.StartsWith("```"))
        {
            var lines = cleaned.Split('\n');
            cleaned = string.Join('\n', lines.Skip(1).Take(lines.Length - 2));
        }

        // Remove "json" prefix if present
        if (cleaned.StartsWith("json", StringComparison.OrdinalIgnoreCase))
        {
            cleaned = cleaned.Substring(4).Trim();
        }

        // Find actual JSON boundaries
        int startIndex = cleaned.IndexOf('{');
        int endIndex = cleaned.LastIndexOf('}');

        if (startIndex >= 0 && endIndex > startIndex)
        {
            cleaned = cleaned.Substring(startIndex, endIndex - startIndex + 1);
        }

        return cleaned;
    }
}
