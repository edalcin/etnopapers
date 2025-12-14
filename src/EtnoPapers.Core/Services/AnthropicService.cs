using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using EtnoPapers.Core.Utils;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EtnoPapers.Core.Services;

/// <summary>
/// Anthropic Claude AI provider implementation for ethnobotanical metadata extraction.
/// Uses Claude 3.5 Sonnet model via REST API.
/// </summary>
public class AnthropicService : AIProviderService
{
    private const string ApiEndpoint = "https://api.anthropic.com/v1/messages";
    private const string DefaultModel = "claude-3-5-sonnet-20241022";
    private const string ApiVersion = "2023-06-01";

    protected override string ProviderName => "Anthropic";

    public AnthropicService(HttpClient? httpClient = null) : base(httpClient)
    {
        HttpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    /// <summary>
    /// Tests connectivity to Anthropic API without validating metadata structure.
    /// Simple test that just validates the API key and connection.
    /// </summary>
    public async Task<bool> TestApiConnectionAsync(CancellationToken cancellationToken = default)
    {
        ValidateApiKey();

        try
        {
            Logger.Debug("[{Provider}] Starting API connection test", ProviderName);

            // Ultra-simple test request
            var requestBody = new
            {
                model = DefaultModel,
                max_tokens = 50,
                messages = new[]
                {
                    new { role = "user", content = "Responda com 'OK'" }
                }
            };

            var json = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var request = new HttpRequestMessage(HttpMethod.Post, ApiEndpoint);
            request.Content = content;
            request.Headers.Add("x-api-key", ApiKey);
            request.Headers.Add("anthropic-version", ApiVersion);

            Logger.Debug("[{Provider}] Sending test request", ProviderName);

            var response = await HttpClient.SendAsync(request, cancellationToken);

            Logger.Debug("[{Provider}] Test response status: {StatusCode}", ProviderName, response.StatusCode);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                Logger.Error("[{Provider}] Test request failed with status {StatusCode}: {Error}",
                    ProviderName, response.StatusCode, errorContent);

                // Check if it's an authentication error (invalid API key)
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                    errorContent.Contains("authentication") || errorContent.Contains("invalid"))
                {
                    Logger.Error("[{Provider}] Authentication failed - invalid or expired API key", ProviderName);
                }

                return false;
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            Logger.Debug("[{Provider}] Received response (length: {Length})", ProviderName, responseContent.Length);

            // Just check if we got a valid response
            try
            {
                var jResponse = JObject.Parse(responseContent);
                var hasContent = jResponse["content"] != null;

                Logger.Information("[{Provider}] Connection test successful. Response has content: {HasContent}",
                    ProviderName, hasContent);

                return hasContent;
            }
            catch (JsonException jsonEx)
            {
                Logger.Error(jsonEx, "[{Provider}] Failed to parse response JSON", ProviderName);
                // If we got a 200 OK but the JSON is malformed, the connection works but response format is wrong
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
    /// Extracts ethnobotanical metadata from PDF text using Anthropic API.
    /// </summary>
    public override async Task<string> ExtractMetadataAsync(string pdfText, CancellationToken cancellationToken = default)
    {
        ValidateApiKey();

        if (string.IsNullOrWhiteSpace(pdfText))
            throw new ArgumentException("PDF text cannot be empty", nameof(pdfText));

        var startTime = DateTime.UtcNow;

        try
        {
            Logger.Information("Starting metadata extraction with {Provider}", ProviderName);

            var response = await RetryHelper.ExecuteWithRetryAsync(
                async ct => await CallAnthropicApiAsync(pdfText, ct),
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
    /// Calls Anthropic API and returns extracted JSON metadata.
    /// </summary>
    private async Task<string> CallAnthropicApiAsync(string pdfText, CancellationToken cancellationToken)
    {
        var systemPrompt = GetExtractionPrompt();

        // Build request payload according to Anthropic Messages API format
        var requestBody = new
        {
            model = DefaultModel,
            max_tokens = MaxTokens,
            temperature = Temperature,
            system = systemPrompt,
            messages = new[]
            {
                new { role = "user", content = pdfText }
            }
        };

        var json = JsonConvert.SerializeObject(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Set required Anthropic headers
        using var request = new HttpRequestMessage(HttpMethod.Post, ApiEndpoint);
        request.Content = content;
        request.Headers.Add("x-api-key", ApiKey);
        request.Headers.Add("anthropic-version", ApiVersion);

        Logger.Debug("[{Provider}] Sending extraction request (PDF text length: {Length})",
            ProviderName, pdfText.Length);

        var response = await HttpClient.SendAsync(request, cancellationToken);

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

        // Parse Anthropic response format
        return ExtractJsonFromAnthropicResponse(responseContent);
    }

    /// <summary>
    /// Extracts JSON metadata from Anthropic API response.
    /// Response format: { "content": [{ "text": "..." }] }
    /// </summary>
    private string ExtractJsonFromAnthropicResponse(string responseContent)
    {
        try
        {
            var jResponse = JObject.Parse(responseContent);
            var content = jResponse["content"] as JArray;

            if (content == null || content.Count == 0)
                throw new InvalidOperationException("No content in Anthropic response");

            var firstContent = content[0] as JObject;
            var text = firstContent?["text"]?.ToString();

            if (string.IsNullOrEmpty(text))
                throw new InvalidOperationException("Empty text in Anthropic response");

            Logger.Debug("[{Provider}] Extracted text from response (length: {Length})",
                ProviderName, text.Length);

            // Clean and validate the extracted JSON
            var cleanedJson = CleanJsonResponse(text);
            var validatedJson = ParseJsonResponse(cleanedJson);

            return validatedJson.ToString(Formatting.None);
        }
        catch (JsonException ex)
        {
            Logger.Error(ex, "[{Provider}] Failed to parse Anthropic response", ProviderName);
            throw new InvalidOperationException("Invalid response format from Anthropic", ex);
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
