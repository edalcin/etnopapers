using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using EtnoPapers.Core.Utils;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EtnoPapers.Core.Services;

/// <summary>
/// OpenAI GPT provider implementation for ethnobotanical metadata extraction.
/// Uses GPT-4o-mini model via REST API.
/// </summary>
public class OpenAIService : AIProviderService
{
    private const string ApiEndpoint = "https://api.openai.com/v1/chat/completions";
    private const string DefaultModel = "gpt-4o-mini";

    protected override string ProviderName => "OpenAI";

    public OpenAIService(HttpClient? httpClient = null) : base(httpClient)
    {
        // Set timeout to 5 minutes for AI processing of large PDFs
        HttpClient.Timeout = TimeSpan.FromSeconds(300);
    }

    /// <summary>
    /// Extracts ethnobotanical metadata from PDF text using OpenAI API.
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
                async ct => await CallOpenAIApiAsync(pdfText, ct),
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
    /// Calls OpenAI API and returns extracted JSON metadata.
    /// </summary>
    private async Task<string> CallOpenAIApiAsync(string pdfText, CancellationToken cancellationToken)
    {
        var systemPrompt = GetExtractionPrompt();

        // Build request payload according to OpenAI Chat Completions API format
        var requestBody = new
        {
            model = DefaultModel,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = pdfText }
            },
            temperature = Temperature,
            top_p = TopP,
            max_tokens = MaxTokens,
            response_format = new { type = "json_object" }
        };

        var json = JsonConvert.SerializeObject(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Set Authorization header with Bearer token
        using var request = new HttpRequestMessage(HttpMethod.Post, ApiEndpoint);
        request.Content = content;
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);

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

        // Parse OpenAI response format
        return ExtractJsonFromOpenAIResponse(responseContent);
    }

    /// <summary>
    /// Extracts JSON metadata from OpenAI API response.
    /// Response format: { "choices": [{ "message": { "content": "..." } }] }
    /// </summary>
    private string ExtractJsonFromOpenAIResponse(string responseContent)
    {
        try
        {
            var jResponse = JObject.Parse(responseContent);
            var choices = jResponse["choices"] as JArray;

            if (choices == null || choices.Count == 0)
                throw new InvalidOperationException("No choices in OpenAI response");

            var firstChoice = choices[0] as JObject;
            var message = firstChoice?["message"] as JObject;
            var content = message?["content"]?.ToString();

            if (string.IsNullOrEmpty(content))
                throw new InvalidOperationException("Empty content in OpenAI response");

            Logger.Debug("[{Provider}] Extracted content from response (length: {Length})",
                ProviderName, content.Length);

            // Validate the JSON response
            var validatedJson = ParseJsonResponse(content);

            return validatedJson.ToString(Formatting.None);
        }
        catch (JsonException ex)
        {
            Logger.Error(ex, "[{Provider}] Failed to parse OpenAI response", ProviderName);
            throw new InvalidOperationException("Invalid response format from OpenAI", ex);
        }
    }
}
