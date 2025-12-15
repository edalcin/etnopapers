using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using EtnoPapers.Core.Models;
using EtnoPapers.Core.Utils;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EtnoPapers.Core.Services;

/// <summary>
/// Google Gemini AI provider implementation for ethnobotanical metadata extraction.
/// Uses Gemini 2.5 Flash and 2.5 Pro models via REST API.
/// </summary>
public class GeminiService : AIProviderService
{
    // Endpoint supports both gemini-2.5-flash and gemini-2.5-pro models (2025 stable versions)
    private const string ApiEndpointFlash = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";
    private const string ApiEndpointPro = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-pro:generateContent";
    private string _apiEndpoint = ApiEndpointFlash;
    private GeminiModelType _selectedModel = GeminiModelType.Flash;

    protected override string ProviderName => "Gemini";

    public GeminiService(HttpClient? httpClient = null) : base(httpClient)
    {
        // Set timeout to 5 minutes to allow Gemini API sufficient time to process large PDFs
        // Gemini can take longer than other providers due to more thorough analysis
        HttpClient.Timeout = TimeSpan.FromSeconds(300);
    }

    /// <summary>
    /// Sets the Gemini model to use for API calls.
    /// </summary>
    public void SetModel(GeminiModelType model)
    {
        _selectedModel = model;
        _apiEndpoint = model == GeminiModelType.Flash ? ApiEndpointFlash : ApiEndpointPro;
        Logger.Information("Gemini model set to: {Model}", model);
    }

    /// <summary>
    /// Tests connectivity to Gemini API without validating metadata structure.
    /// Simple test that just validates the API key and connection.
    /// </summary>
    public async Task<bool> TestApiConnectionAsync(CancellationToken cancellationToken = default)
    {
        ValidateApiKey();

        try
        {
            Logger.Debug("[{Provider}] Starting API connection test with endpoint: {Endpoint}", ProviderName, _apiEndpoint);

            // Ultra-simple test: just ask for a confirmation
            var testPrompt = "Responda com 'OK'";
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[] { new { text = testPrompt } }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.1f,
                    topP = 0.3f,
                    maxOutputTokens = 50
                }
            };

            var json = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var urlWithKey = $"{_apiEndpoint}?key={ApiKey}";

            Logger.Debug("[{Provider}] Sending test request to: {Endpoint}", ProviderName, _apiEndpoint);

            var response = await HttpClient.PostAsync(urlWithKey, content, cancellationToken);

            Logger.Debug("[{Provider}] Test response status: {StatusCode}", ProviderName, response.StatusCode);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                Logger.Error("[{Provider}] Test request failed with status {StatusCode}: {Error}",
                    ProviderName, response.StatusCode, errorContent);

                // Try fallback to gemini-2.5-pro if gemini-2.5-flash fails with 404
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound && _apiEndpoint == ApiEndpointFlash)
                {
                    Logger.Information("[{Provider}] Model gemini-2.5-flash not available, trying gemini-2.5-pro fallback",
                        ProviderName);
                    _apiEndpoint = ApiEndpointPro;
                    return await TestApiConnectionAsync(cancellationToken);
                }

                // Check if it's an authentication error (invalid API key)
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                    response.StatusCode == System.Net.HttpStatusCode.BadRequest ||
                    errorContent.Contains("API key") || errorContent.Contains("authentication"))
                {
                    Logger.Error("[{Provider}] Authentication failed - invalid or expired API key", ProviderName);
                }

                return false;
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            Logger.Debug("[{Provider}] Received response (length: {Length})", ProviderName, responseContent.Length);

            // Just check if we got a valid response with candidates
            try
            {
                var jResponse = JObject.Parse(responseContent);
                var candidates = jResponse["candidates"] as JArray;
                var hasContent = candidates != null && candidates.Count > 0;

                Logger.Information("[{Provider}] Connection test successful. Response contains candidates: {HasContent}",
                    ProviderName, hasContent);

                return hasContent;
            }
            catch (JsonException jsonEx)
            {
                Logger.Error(jsonEx, "[{Provider}] Failed to parse response JSON", ProviderName);
                // If we got a 200 OK but the JSON is malformed, the connection works but response format is wrong
                // This might still indicate a working API key
                return responseContent.Length > 0;
            }
        }
        catch (HttpRequestException ex)
        {
            Logger.Error(ex, "[{Provider}] HTTP request error during connection test", ProviderName);
            return false;
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "[{Provider}] Unexpected error during connection test", ProviderName);
            return false;
        }
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
        var startTime = DateTime.UtcNow;

        try
        {
            Logger.Information("Starting metadata extraction with {Provider}", ProviderName);

            var response = await RetryHelper.ExecuteWithRetryAsync(
                async ct => await CallGeminiApiAsync(prompt, ct),
                $"Extract metadata from {ProviderName}",
                cancellationToken: cancellationToken);

            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            Logger.Information("Metadata extraction from {Provider} completed in {Duration}ms with status {Status}",
                ProviderName, duration, "Success");

            return response;
        }
        catch (HttpRequestException ex)
        {
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            var statusCode = CloudErrorMapper.TryExtractStatusCode(ex);
            var message = CloudErrorMapper.GetErrorMessage(ex, statusCode);
            Logger.Error(ex, "Metadata extraction from {Provider} failed after {Duration}ms with status {StatusCode}",
                ProviderName, duration, statusCode);
            throw new InvalidOperationException(message, ex);
        }
        catch (Exception ex)
        {
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            Logger.Error(ex, "Metadata extraction from {Provider} failed after {Duration}ms with error: {Error}",
                ProviderName, duration, ex.Message);
            throw;
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
        var urlWithKey = $"{_apiEndpoint}?key={ApiKey}";

        Logger.Debug("[{Provider}] Sending extraction request (prompt length: {Length})",
            ProviderName, prompt.Length);

        var response = await HttpClient.PostAsync(urlWithKey, content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Logger.Error("[{Provider}] API returned {StatusCode}: {Error}",
                ProviderName, response.StatusCode, errorContent);

            // Try fallback to gemini-2.5-pro if gemini-2.5-flash fails with 404
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound && _apiEndpoint == ApiEndpointFlash)
            {
                Logger.Information("[{Provider}] Model gemini-2.5-flash not available, trying gemini-2.5-pro fallback",
                    ProviderName);
                _apiEndpoint = ApiEndpointPro;
                return await CallGeminiApiAsync(prompt, cancellationToken);
            }

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
