# Error Handling and Rate Limiting Strategies for Cloud AI APIs

**Research Date**: December 2025
**Target Application**: EtnoPapers Windows Desktop Application
**Programming Context**: C# .NET 8.0, WPF, async/await patterns

---

## Table of Contents

1. [Rate Limiting Fundamentals](#rate-limiting-fundamentals)
2. [HTTP Error Status Codes](#http-error-status-codes)
3. [Provider-Specific Rate Limits](#provider-specific-rate-limits)
4. [Exponential Backoff Algorithm](#exponential-backoff-algorithm)
5. [Retry Implementation Patterns](#retry-implementation-patterns)
6. [User-Friendly Error Messages](#user-friendly-error-messages)
7. [Pre-flight Checks](#pre-flight-checks)
8. [Polly Library Integration](#polly-library-integration)
9. [Complete Implementation Example](#complete-implementation-example)

---

## 1. Rate Limiting Fundamentals

### HTTP 429 Too Many Requests

When cloud APIs enforce rate limits, they return:
- **Status Code**: `429 Too Many Requests`
- **Retry-After Header**: Time to wait before retry (seconds or HTTP date)
- **Optional Headers**: Provider-specific rate limit information

### Retry-After Header Parsing

```csharp
public static TimeSpan? ParseRetryAfter(HttpResponseMessage response)
{
    if (!response.Headers.TryGetValues("Retry-After", out var values))
        return null;

    var retryAfter = values.FirstOrDefault();
    if (string.IsNullOrEmpty(retryAfter))
        return null;

    // Try parsing as seconds (integer)
    if (int.TryParse(retryAfter, out int seconds))
        return TimeSpan.FromSeconds(seconds);

    // Try parsing as HTTP date
    if (DateTimeOffset.TryParse(retryAfter, out var date))
        return date - DateTimeOffset.UtcNow;

    return null;
}
```

### Industry Standard Backoff Pattern

**Exponential Backoff with Jitter**:
- Base delay: 1 second
- Pattern: 1s → 2s → 4s → 8s → 16s
- Jitter: Add randomness (±25%) to prevent thundering herd
- Maximum retries: 3-5 attempts
- Maximum delay cap: 32-60 seconds

---

## 2. HTTP Error Status Codes

### Error Code Classification

| Status Code | Category | Retry? | User Message |
|------------|----------|--------|--------------|
| **400** | Bad Request | No | "Invalid request format. Please check your PDF." |
| **401** | Unauthorized | No | "Invalid API key. Please check Settings." |
| **403** | Forbidden | No | "Access denied. API key may lack permissions." |
| **404** | Not Found | No | "Endpoint not found. Service may be unavailable." |
| **408** | Request Timeout | Yes | "Request timed out. Retrying..." |
| **429** | Too Many Requests | Yes | "Rate limit exceeded. Waiting before retry..." |
| **500** | Internal Server Error | Yes | "Server error. Retrying..." |
| **502** | Bad Gateway | Yes | "Gateway error. Retrying..." |
| **503** | Service Unavailable | Yes | "Service temporarily unavailable. Retrying..." |
| **504** | Gateway Timeout | Yes | "Gateway timeout. Retrying..." |

### Network Exceptions

```csharp
public static (bool ShouldRetry, string Message) ClassifyException(Exception ex)
{
    return ex switch
    {
        HttpRequestException httpEx => (true, "Network error. Check your internet connection."),
        TaskCanceledException => (false, "Request canceled by user."),
        OperationCanceledException => (false, "Operation canceled."),
        TimeoutException => (true, "Request timed out. Retrying..."),
        _ => (false, $"Unexpected error: {ex.Message}")
    };
}
```

---

## 3. Provider-Specific Rate Limits

### Google Gemini API

**Rate Limits** (as of December 2025):
- **Free Tier**: 15 requests per minute (RPM), 1 million tokens per minute (TPM)
- **Pay-as-you-go**: 1,000 RPM, configurable TPM
- **Response Headers**: `X-RateLimit-Limit`, `X-RateLimit-Remaining`, `X-RateLimit-Reset`

**Error Response Example**:
```json
{
  "error": {
    "code": 429,
    "message": "Resource has been exhausted (e.g. check quota).",
    "status": "RESOURCE_EXHAUSTED"
  }
}
```

**Documentation**: https://ai.google.dev/gemini-api/docs/quota

### OpenAI API

**Rate Limits** (as of December 2025):
- **Free Tier**: 3 RPM, 40,000 TPM
- **Tier 1** ($5 spent): 500 RPM, 200,000 TPM
- **Tier 5** ($1,000+ spent): 10,000 RPM, 30M TPM
- **Response Headers**: `x-ratelimit-limit-requests`, `x-ratelimit-remaining-requests`, `x-ratelimit-reset-requests`

**Error Response Example**:
```json
{
  "error": {
    "message": "Rate limit reached for requests",
    "type": "rate_limit_error",
    "param": null,
    "code": "rate_limit_exceeded"
  }
}
```

**Documentation**: https://platform.openai.com/docs/guides/rate-limits

### Anthropic Claude API

**Rate Limits** (as of December 2025):
- **Free Tier**: Not available (requires payment method)
- **Build Tier**: 50 RPM, 40,000 TPM
- **Scale Tier**: 1,000 RPM, 400,000 TPM
- **Response Headers**: `anthropic-ratelimit-requests-limit`, `anthropic-ratelimit-requests-remaining`, `anthropic-ratelimit-requests-reset`

**Error Response Example**:
```json
{
  "type": "error",
  "error": {
    "type": "rate_limit_error",
    "message": "Number of request tokens has exceeded your per-minute rate limit"
  }
}
```

**Documentation**: https://docs.anthropic.com/claude/reference/rate-limits

---

## 4. Exponential Backoff Algorithm

### Basic Implementation

```csharp
public static class ExponentialBackoff
{
    private static readonly Random _random = new Random();

    /// <summary>
    /// Calculates delay for exponential backoff with jitter
    /// </summary>
    /// <param name="attemptNumber">Current attempt (0-based)</param>
    /// <param name="baseDelaySeconds">Base delay in seconds (default: 1)</param>
    /// <param name="maxDelaySeconds">Maximum delay cap (default: 32)</param>
    /// <param name="jitterPercent">Jitter percentage (default: 0.25 = 25%)</param>
    public static TimeSpan CalculateDelay(
        int attemptNumber,
        double baseDelaySeconds = 1.0,
        double maxDelaySeconds = 32.0,
        double jitterPercent = 0.25)
    {
        // Calculate exponential delay: base * 2^attempt
        double delay = baseDelaySeconds * Math.Pow(2, attemptNumber);

        // Cap at maximum
        delay = Math.Min(delay, maxDelaySeconds);

        // Add jitter: delay ± (delay * jitterPercent)
        double jitterRange = delay * jitterPercent;
        double jitter = (_random.NextDouble() * 2 - 1) * jitterRange;
        delay += jitter;

        return TimeSpan.FromSeconds(Math.Max(0, delay));
    }

    /// <summary>
    /// Example delays for different attempt numbers (with jitter variance)
    /// Attempt 0: ~1s (0.75-1.25s)
    /// Attempt 1: ~2s (1.5-2.5s)
    /// Attempt 2: ~4s (3-5s)
    /// Attempt 3: ~8s (6-10s)
    /// Attempt 4: ~16s (12-20s)
    /// Attempt 5+: ~32s (24-40s, capped)
    /// </summary>
}
```

### Usage Example

```csharp
for (int attempt = 0; attempt < maxRetries; attempt++)
{
    try
    {
        return await CallApiAsync(cancellationToken);
    }
    catch (HttpRequestException ex) when (IsTransient(ex))
    {
        if (attempt < maxRetries - 1)
        {
            var delay = ExponentialBackoff.CalculateDelay(attempt);
            await Task.Delay(delay, cancellationToken);
        }
    }
}
```

---

## 5. Retry Implementation Patterns

### Pattern 1: Manual Retry Loop (No Dependencies)

```csharp
public class ApiRetryHandler
{
    private const int MaxRetries = 5;
    private static readonly int[] RetryableStatusCodes = { 408, 429, 500, 502, 503, 504 };

    public async Task<T> ExecuteWithRetryAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        IProgress<string> progress = null,
        CancellationToken cancellationToken = default)
    {
        Exception lastException = null;

        for (int attempt = 0; attempt < MaxRetries; attempt++)
        {
            try
            {
                return await operation(cancellationToken);
            }
            catch (HttpRequestException ex) when (ex.StatusCode.HasValue)
            {
                int statusCode = (int)ex.StatusCode.Value;

                if (!RetryableStatusCodes.Contains(statusCode))
                {
                    // Non-retryable error
                    throw new InvalidOperationException(
                        GetUserFriendlyMessage(statusCode), ex);
                }

                lastException = ex;

                if (attempt < MaxRetries - 1)
                {
                    var delay = ExponentialBackoff.CalculateDelay(attempt);
                    progress?.Report(
                        $"Request failed (attempt {attempt + 1}/{MaxRetries}). " +
                        $"Retrying in {delay.TotalSeconds:F1}s...");

                    await Task.Delay(delay, cancellationToken);
                }
            }
            catch (TaskCanceledException)
            {
                throw; // User canceled, don't retry
            }
            catch (Exception ex) when (IsTransientNetworkError(ex))
            {
                lastException = ex;

                if (attempt < MaxRetries - 1)
                {
                    var delay = ExponentialBackoff.CalculateDelay(attempt);
                    progress?.Report($"Network error. Retrying in {delay.TotalSeconds:F1}s...");
                    await Task.Delay(delay, cancellationToken);
                }
            }
        }

        throw new InvalidOperationException(
            $"Operation failed after {MaxRetries} attempts. Check your connection and API key.",
            lastException);
    }

    private static bool IsTransientNetworkError(Exception ex)
    {
        return ex is HttpRequestException
            || ex is TimeoutException
            || ex is SocketException;
    }

    private static string GetUserFriendlyMessage(int statusCode)
    {
        return statusCode switch
        {
            400 => "Invalid request format. Please check your PDF file.",
            401 => "Invalid API key. Please verify your key in Settings.",
            403 => "Access denied. Your API key may lack required permissions.",
            404 => "Service endpoint not found. The service may be unavailable.",
            429 => "Rate limit exceeded. Please wait before trying again.",
            500 => "Server error occurred. Please try again later.",
            502 => "Gateway error. The service is temporarily unavailable.",
            503 => "Service unavailable. Please try again in a few moments.",
            504 => "Gateway timeout. The request took too long to process.",
            _ => $"HTTP error {statusCode} occurred."
        };
    }
}
```

### Pattern 2: Retry with Retry-After Header Support

```csharp
public async Task<HttpResponseMessage> SendWithRetryAsync(
    HttpClient client,
    HttpRequestMessage request,
    IProgress<string> progress = null,
    CancellationToken cancellationToken = default)
{
    for (int attempt = 0; attempt < MaxRetries; attempt++)
    {
        var response = await client.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
            return response;

        if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        {
            // Check Retry-After header
            var retryAfter = ParseRetryAfter(response);
            var delay = retryAfter ?? ExponentialBackoff.CalculateDelay(attempt);

            if (attempt < MaxRetries - 1)
            {
                progress?.Report(
                    $"Rate limit exceeded. Waiting {delay.TotalSeconds:F0}s before retry...");
                await Task.Delay(delay, cancellationToken);
                continue;
            }
        }

        if (!IsRetryableStatusCode((int)response.StatusCode))
        {
            throw new HttpRequestException(
                GetUserFriendlyMessage((int)response.StatusCode),
                null,
                response.StatusCode);
        }

        if (attempt < MaxRetries - 1)
        {
            var delay = ExponentialBackoff.CalculateDelay(attempt);
            progress?.Report($"Request failed. Retrying in {delay.TotalSeconds:F1}s...");
            await Task.Delay(delay, cancellationToken);
        }
    }

    throw new HttpRequestException("Maximum retry attempts exceeded.");
}
```

---

## 6. User-Friendly Error Messages

### Error Message Mapping Table

```csharp
public static class ErrorMessageMapper
{
    private static readonly Dictionary<int, string> StatusCodeMessages = new()
    {
        // Client Errors (4xx)
        { 400, "O formato da solicitação é inválido. Verifique o arquivo PDF." },
        { 401, "Chave de API inválida. Verifique suas configurações." },
        { 403, "Acesso negado. Sua chave de API pode não ter permissões necessárias." },
        { 404, "Serviço não encontrado. O endpoint da API pode estar incorreto." },
        { 408, "Tempo limite da solicitação excedido. Tentando novamente..." },
        { 429, "Limite de taxa excedido. Aguardando antes de tentar novamente..." },

        // Server Errors (5xx)
        { 500, "Erro interno do servidor. O serviço está temporariamente indisponível." },
        { 502, "Erro de gateway. O serviço está temporariamente inacessível." },
        { 503, "Serviço indisponível. Tente novamente em alguns instantes." },
        { 504, "Tempo limite do gateway excedido. A solicitação demorou muito para processar." }
    };

    private static readonly Dictionary<string, string> ExceptionMessages = new()
    {
        { nameof(HttpRequestException), "Erro de rede. Verifique sua conexão com a internet." },
        { nameof(TaskCanceledException), "Operação cancelada pelo usuário." },
        { nameof(TimeoutException), "Tempo limite excedido. Verificando conexão..." },
        { nameof(SocketException), "Falha na conexão de rede. Verifique sua internet." }
    };

    public static string GetPortugueseMessage(int statusCode)
    {
        return StatusCodeMessages.TryGetValue(statusCode, out var message)
            ? message
            : $"Erro HTTP {statusCode} ocorreu.";
    }

    public static string GetPortugueseMessage(Exception ex)
    {
        var exceptionType = ex.GetType().Name;

        if (ExceptionMessages.TryGetValue(exceptionType, out var message))
            return message;

        if (ex is HttpRequestException httpEx && httpEx.StatusCode.HasValue)
            return GetPortugueseMessage((int)httpEx.StatusCode);

        return $"Erro inesperado: {ex.Message}";
    }

    public static string GetEnglishMessage(int statusCode)
    {
        return statusCode switch
        {
            400 => "Invalid request format. Please check your PDF file.",
            401 => "Invalid API key. Please verify your key in Settings.",
            403 => "Access denied. Your API key may lack required permissions.",
            404 => "Service endpoint not found. The service may be unavailable.",
            408 => "Request timeout. Retrying...",
            429 => "Rate limit exceeded. Waiting before retry...",
            500 => "Internal server error. Service temporarily unavailable.",
            502 => "Gateway error. Service temporarily inaccessible.",
            503 => "Service unavailable. Please try again in a few moments.",
            504 => "Gateway timeout. Request took too long to process.",
            _ => $"HTTP error {statusCode} occurred."
        };
    }
}
```

### WPF Progress Reporting

```csharp
// In ViewModel
private async void ExtractMetadataCommand()
{
    var progress = new Progress<string>(message =>
    {
        StatusMessage = message; // Bound to UI TextBlock
    });

    try
    {
        progress.Report("Verificando conexão...");
        await PreflightCheckAsync();

        progress.Report("Enviando PDF para análise...");
        var result = await _retryHandler.ExecuteWithRetryAsync(
            async ct => await _apiClient.ExtractMetadataAsync(pdfPath, ct),
            progress,
            _cancellationTokenSource.Token
        );

        progress.Report("Metadados extraídos com sucesso!");
    }
    catch (Exception ex)
    {
        StatusMessage = ErrorMessageMapper.GetPortugueseMessage(ex);
        MessageBox.Show(StatusMessage, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
```

---

## 7. Pre-flight Checks

### API Key Validation

```csharp
public static class ApiKeyValidator
{
    /// <summary>
    /// Validates API key format before making requests
    /// </summary>
    public static (bool IsValid, string ErrorMessage) ValidateGeminiKey(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            return (false, "API key is empty. Please enter a valid key in Settings.");

        // Gemini keys typically start with "AI" and are 39 characters
        if (!apiKey.StartsWith("AI") || apiKey.Length != 39)
            return (false, "Invalid Gemini API key format. Keys should start with 'AI' and be 39 characters.");

        return (true, string.Empty);
    }

    public static (bool IsValid, string ErrorMessage) ValidateOpenAIKey(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            return (false, "API key is empty. Please enter a valid key in Settings.");

        // OpenAI keys start with "sk-" and are typically 48-51 characters
        if (!apiKey.StartsWith("sk-"))
            return (false, "Invalid OpenAI API key format. Keys should start with 'sk-'.");

        if (apiKey.Length < 40)
            return (false, "OpenAI API key appears too short. Please verify your key.");

        return (true, string.Empty);
    }

    public static (bool IsValid, string ErrorMessage) ValidateAnthropicKey(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            return (false, "API key is empty. Please enter a valid key in Settings.");

        // Anthropic keys start with "sk-ant-" and are longer
        if (!apiKey.StartsWith("sk-ant-"))
            return (false, "Invalid Anthropic API key format. Keys should start with 'sk-ant-'.");

        return (true, string.Empty);
    }
}
```

### Network Connectivity Check

```csharp
public static class NetworkChecker
{
    /// <summary>
    /// Checks internet connectivity by DNS lookup
    /// </summary>
    public static async Task<bool> IsInternetAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
            var response = await client.GetAsync("https://www.google.com",
                HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if specific API endpoint is reachable
    /// </summary>
    public static async Task<(bool IsReachable, string Message)> CheckApiEndpointAsync(
        string baseUrl,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            var response = await client.GetAsync(baseUrl,
                HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            // Even 401/403 means endpoint is reachable
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                return (true, "Endpoint reachable (authentication required).");

            return response.IsSuccessStatusCode
                ? (true, "Endpoint is reachable.")
                : (false, $"Endpoint returned {response.StatusCode}.");
        }
        catch (HttpRequestException ex)
        {
            return (false, $"Network error: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            return (false, "Connection timeout. Endpoint may be unreachable.");
        }
    }
}
```

### Complete Pre-flight Check

```csharp
public async Task<(bool Success, string Message)> PreflightCheckAsync(
    string apiProvider,
    string apiKey,
    CancellationToken cancellationToken = default)
{
    // Step 1: Validate API key format
    var (isValidKey, keyError) = apiProvider switch
    {
        "Gemini" => ApiKeyValidator.ValidateGeminiKey(apiKey),
        "OpenAI" => ApiKeyValidator.ValidateOpenAIKey(apiKey),
        "Anthropic" => ApiKeyValidator.ValidateAnthropicKey(apiKey),
        _ => (false, "Unknown API provider.")
    };

    if (!isValidKey)
        return (false, keyError);

    // Step 2: Check internet connectivity
    if (!await NetworkChecker.IsInternetAvailableAsync(cancellationToken))
        return (false, "No internet connection. Please check your network.");

    // Step 3: Check API endpoint reachability
    var baseUrl = apiProvider switch
    {
        "Gemini" => "https://generativelanguage.googleapis.com",
        "OpenAI" => "https://api.openai.com",
        "Anthropic" => "https://api.anthropic.com",
        _ => null
    };

    if (baseUrl != null)
    {
        var (isReachable, message) = await NetworkChecker.CheckApiEndpointAsync(
            baseUrl, cancellationToken);

        if (!isReachable)
            return (false, $"Cannot reach {apiProvider} API: {message}");
    }

    return (true, "All checks passed.");
}
```

---

## 8. Polly Library Integration

### Installation

```powershell
dotnet add package Polly
dotnet add package Polly.Extensions.Http
```

### Basic Polly Retry Policy

```csharp
using Polly;
using Polly.Extensions.Http;

public class PollyRetryHandler
{
    private static readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy =
        HttpPolicyExtensions
            .HandleTransientHttpError() // Handles 408, 5xx, HttpRequestException
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(
                retryCount: 5,
                sleepDurationProvider: retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) // 2, 4, 8, 16, 32 seconds
                    + TimeSpan.FromMilliseconds(Random.Shared.Next(0, 1000)), // Jitter
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    Console.WriteLine(
                        $"Retry {retryCount} after {timespan.TotalSeconds:F1}s " +
                        $"due to: {outcome.Result?.StatusCode}");
                });

    public async Task<HttpResponseMessage> ExecuteAsync(
        Func<Task<HttpResponseMessage>> action,
        CancellationToken cancellationToken = default)
    {
        return await _retryPolicy.ExecuteAsync(async () => await action());
    }
}
```

### Advanced Polly with Progress Reporting

```csharp
public class AdvancedPollyHandler
{
    public static IAsyncPolicy<HttpResponseMessage> CreateRetryPolicy(
        IProgress<string> progress = null)
    {
        return Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .Or<TimeoutException>()
            .OrResult(r =>
                r.StatusCode == System.Net.HttpStatusCode.TooManyRequests ||
                (int)r.StatusCode >= 500)
            .WaitAndRetryAsync(
                retryCount: 5,
                sleepDurationProvider: (retryAttempt, result, context) =>
                {
                    // Check for Retry-After header
                    if (result?.Result != null)
                    {
                        var retryAfter = ParseRetryAfter(result.Result);
                        if (retryAfter.HasValue)
                            return retryAfter.Value;
                    }

                    // Default exponential backoff
                    return TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                        + TimeSpan.FromMilliseconds(Random.Shared.Next(0, 1000));
                },
                onRetryAsync: async (outcome, timespan, retryCount, context) =>
                {
                    var statusCode = outcome.Result?.StatusCode.ToString() ?? "Network Error";
                    progress?.Report(
                        $"Tentativa {retryCount}/5 falhou ({statusCode}). " +
                        $"Aguardando {timespan.TotalSeconds:F0}s...");
                    await Task.CompletedTask;
                });
    }

    public static IAsyncPolicy<HttpResponseMessage> CreateTimeoutPolicy()
    {
        return Policy.TimeoutAsync<HttpResponseMessage>(
            TimeSpan.FromMinutes(2),
            onTimeoutAsync: async (context, timespan, abandonedTask) =>
            {
                Console.WriteLine($"Request timed out after {timespan.TotalSeconds}s");
                await Task.CompletedTask;
            });
    }

    public static IAsyncPolicy<HttpResponseMessage> CreateWrapPolicy(
        IProgress<string> progress = null)
    {
        var retry = CreateRetryPolicy(progress);
        var timeout = CreateTimeoutPolicy();

        // Timeout applies to each retry attempt
        return Policy.WrapAsync(retry, timeout);
    }
}
```

### HttpClient Configuration with Polly

```csharp
public class ApiClientFactory
{
    public static HttpClient CreateGeminiClient(string apiKey, IProgress<string> progress = null)
    {
        var handler = new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(2),
            ConnectTimeout = TimeSpan.FromSeconds(15)
        };

        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://generativelanguage.googleapis.com/v1beta/"),
            Timeout = TimeSpan.FromMinutes(3) // Overall timeout
        };

        client.DefaultRequestHeaders.Add("x-goog-api-key", apiKey);

        return client;
    }

    public static async Task<T> SendWithPolicyAsync<T>(
        HttpClient client,
        HttpRequestMessage request,
        IAsyncPolicy<HttpResponseMessage> policy,
        Func<HttpResponseMessage, Task<T>> deserializer,
        CancellationToken cancellationToken = default)
    {
        var response = await policy.ExecuteAsync(async () =>
        {
            var clonedRequest = await CloneHttpRequestAsync(request);
            return await client.SendAsync(clonedRequest, cancellationToken);
        });

        response.EnsureSuccessStatusCode();
        return await deserializer(response);
    }

    private static async Task<HttpRequestMessage> CloneHttpRequestAsync(
        HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri)
        {
            Version = request.Version
        };

        foreach (var header in request.Headers)
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

        if (request.Content != null)
        {
            var contentBytes = await request.Content.ReadAsByteArrayAsync();
            clone.Content = new ByteArrayContent(contentBytes);

            foreach (var header in request.Content.Headers)
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return clone;
    }
}
```

---

## 9. Complete Implementation Example

### Unified API Client with Error Handling

```csharp
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace EtnoPapers.Services
{
    public class ResilientApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ApiRetryHandler _retryHandler;
        private readonly string _apiProvider;
        private readonly string _apiKey;

        public ResilientApiClient(
            string apiProvider,
            string apiKey,
            HttpClient httpClient = null)
        {
            _apiProvider = apiProvider;
            _apiKey = apiKey;
            _httpClient = httpClient ?? CreateDefaultClient(apiProvider, apiKey);
            _retryHandler = new ApiRetryHandler();
        }

        /// <summary>
        /// Extracts metadata from PDF with full error handling and retry logic
        /// </summary>
        public async Task<MetadataExtractionResult> ExtractMetadataAsync(
            string pdfPath,
            IProgress<string> progress = null,
            CancellationToken cancellationToken = default)
        {
            // Pre-flight checks
            progress?.Report("Verificando configuração...");
            var (checkSuccess, checkMessage) = await PreflightCheckAsync(cancellationToken);
            if (!checkSuccess)
                throw new InvalidOperationException(checkMessage);

            // Execute with retry
            return await _retryHandler.ExecuteWithRetryAsync(
                async ct => await ExtractMetadataInternalAsync(pdfPath, progress, ct),
                progress,
                cancellationToken
            );
        }

        private async Task<MetadataExtractionResult> ExtractMetadataInternalAsync(
            string pdfPath,
            IProgress<string> progress,
            CancellationToken cancellationToken)
        {
            progress?.Report("Enviando PDF para análise...");

            var request = BuildMetadataRequest(pdfPath);
            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new HttpRequestException(
                    $"API returned {response.StatusCode}: {errorContent}",
                    null,
                    response.StatusCode
                );
            }

            progress?.Report("Processando resposta...");
            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            return ParseMetadataResponse(json);
        }

        private async Task<(bool Success, string Message)> PreflightCheckAsync(
            CancellationToken cancellationToken)
        {
            // Validate API key format
            var (isValidKey, keyError) = _apiProvider switch
            {
                "Gemini" => ApiKeyValidator.ValidateGeminiKey(_apiKey),
                "OpenAI" => ApiKeyValidator.ValidateOpenAIKey(_apiKey),
                "Anthropic" => ApiKeyValidator.ValidateAnthropicKey(_apiKey),
                _ => (false, "Provedor de API desconhecido.")
            };

            if (!isValidKey)
                return (false, keyError);

            // Check internet connectivity
            if (!await NetworkChecker.IsInternetAvailableAsync(cancellationToken))
                return (false, "Sem conexão com a internet. Verifique sua rede.");

            return (true, "Verificações concluídas.");
        }

        private HttpRequestMessage BuildMetadataRequest(string pdfPath)
        {
            // Implementation depends on provider
            // This is a placeholder for the actual request building logic
            throw new NotImplementedException("Implement provider-specific request building");
        }

        private MetadataExtractionResult ParseMetadataResponse(string json)
        {
            // Implementation depends on provider
            // This is a placeholder for the actual response parsing logic
            throw new NotImplementedException("Implement provider-specific response parsing");
        }

        private static HttpClient CreateDefaultClient(string apiProvider, string apiKey)
        {
            var handler = new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(2),
                ConnectTimeout = TimeSpan.FromSeconds(15)
            };

            var baseAddress = apiProvider switch
            {
                "Gemini" => "https://generativelanguage.googleapis.com/v1beta/",
                "OpenAI" => "https://api.openai.com/v1/",
                "Anthropic" => "https://api.anthropic.com/v1/",
                _ => throw new ArgumentException($"Unknown API provider: {apiProvider}")
            };

            var client = new HttpClient(handler)
            {
                BaseAddress = new Uri(baseAddress),
                Timeout = TimeSpan.FromMinutes(3)
            };

            // Provider-specific headers
            switch (apiProvider)
            {
                case "Gemini":
                    client.DefaultRequestHeaders.Add("x-goog-api-key", apiKey);
                    break;
                case "OpenAI":
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
                    break;
                case "Anthropic":
                    client.DefaultRequestHeaders.Add("x-api-key", apiKey);
                    client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
                    break;
            }

            return client;
        }
    }

    public class MetadataExtractionResult
    {
        public string Title { get; set; }
        public string[] Authors { get; set; }
        public int Year { get; set; }
        public string Abstract { get; set; }
        // ... other fields
    }
}
```

### WPF ViewModel Integration

```csharp
using System;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace EtnoPapers.ViewModels
{
    public class PdfProcessingViewModel : ViewModelBase
    {
        private readonly ResilientApiClient _apiClient;
        private CancellationTokenSource _cancellationTokenSource;

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        private bool _isProcessing;
        public bool IsProcessing
        {
            get => _isProcessing;
            set => SetProperty(ref _isProcessing, value);
        }

        public ICommand ProcessPdfCommand { get; }
        public ICommand CancelCommand { get; }

        public PdfProcessingViewModel(ResilientApiClient apiClient)
        {
            _apiClient = apiClient;
            ProcessPdfCommand = new AsyncRelayCommand(ProcessPdfAsync, () => !IsProcessing);
            CancelCommand = new RelayCommand(Cancel, () => IsProcessing);
        }

        private async Task ProcessPdfAsync()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            IsProcessing = true;

            var progress = new Progress<string>(message =>
            {
                StatusMessage = message;
            });

            try
            {
                var pdfPath = SelectPdfFile(); // Show file dialog
                if (string.IsNullOrEmpty(pdfPath))
                    return;

                StatusMessage = "Iniciando processamento...";

                var result = await _apiClient.ExtractMetadataAsync(
                    pdfPath,
                    progress,
                    _cancellationTokenSource.Token
                );

                StatusMessage = "Metadados extraídos com sucesso!";
                ShowResults(result);
            }
            catch (OperationCanceledException)
            {
                StatusMessage = "Operação cancelada pelo usuário.";
            }
            catch (InvalidOperationException ex)
            {
                // Pre-flight check failed
                StatusMessage = ex.Message;
                MessageBox.Show(
                    ex.Message,
                    "Erro de Configuração",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }
            catch (HttpRequestException ex) when (ex.StatusCode.HasValue)
            {
                var message = ErrorMessageMapper.GetPortugueseMessage((int)ex.StatusCode);
                StatusMessage = message;
                MessageBox.Show(
                    message,
                    "Erro de API",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
            catch (Exception ex)
            {
                var message = ErrorMessageMapper.GetPortugueseMessage(ex);
                StatusMessage = message;
                MessageBox.Show(
                    $"Erro inesperado: {ex.Message}",
                    "Erro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
            finally
            {
                IsProcessing = false;
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
        }

        private void Cancel()
        {
            _cancellationTokenSource?.Cancel();
            StatusMessage = "Cancelando operação...";
        }

        private string SelectPdfFile()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                Title = "Selecione o arquivo PDF"
            };

            return dialog.ShowDialog() == true ? dialog.FileName : null;
        }

        private void ShowResults(MetadataExtractionResult result)
        {
            // Navigate to results view or show in current view
        }
    }
}
```

---

## Summary and Recommendations

### Key Takeaways

1. **Always implement exponential backoff with jitter** to avoid thundering herd problems
2. **Parse Retry-After headers** when available (especially for 429 responses)
3. **Validate API keys before making requests** to provide immediate feedback
4. **Check network connectivity** before long operations
5. **Use Progress<T>** for real-time status updates in WPF
6. **Provide Portuguese error messages** for end users
7. **Limit retries to 3-5 attempts** with sensible timeout caps
8. **Don't retry non-transient errors** (400, 401, 403, 404)

### Recommended Approach for EtnoPapers

**Option 1: Manual Retry (No Dependencies)**
- Pros: No external dependencies, full control
- Cons: More code to maintain
- Best for: Simple scenarios, learning

**Option 2: Polly Library**
- Pros: Battle-tested, flexible, extensive features
- Cons: Additional dependency (~200KB)
- Best for: Production applications, complex policies

### Recommended Implementation

For EtnoPapers, I recommend **starting with manual retry** (Pattern 1) because:
1. No additional dependencies needed
2. Full transparency for learning/debugging
3. Sufficient for the application's needs
4. Easy to migrate to Polly later if needed

### Testing Strategies

```csharp
// Unit test for retry logic
[Fact]
public async Task ExtractMetadata_RetriesOn429()
{
    var mockHandler = new MockHttpMessageHandler();
    mockHandler.SetupSequence()
        .Returns429() // First attempt
        .Returns429() // Second attempt
        .Returns200WithData(); // Success

    var client = new ResilientApiClient("Gemini", "test-key",
        new HttpClient(mockHandler));

    var result = await client.ExtractMetadataAsync("test.pdf");

    Assert.NotNull(result);
    Assert.Equal(3, mockHandler.RequestCount);
}
```

### Production Checklist

- [ ] Implement exponential backoff with jitter
- [ ] Parse Retry-After headers
- [ ] Validate API keys before requests
- [ ] Check network connectivity
- [ ] Provide Portuguese error messages
- [ ] Add progress reporting for long operations
- [ ] Implement cancellation support
- [ ] Log retry attempts for monitoring
- [ ] Add metrics for success/failure rates
- [ ] Test with network simulation tools (Fiddler, Charles Proxy)
- [ ] Document rate limits in user manual
- [ ] Consider caching to reduce API calls

---

## References

- **HTTP Status Codes**: RFC 9110 - HTTP Semantics
- **Retry-After Header**: RFC 9110 Section 10.2.3
- **Exponential Backoff**: AWS Architecture Blog, Google Cloud Best Practices
- **Polly Documentation**: https://www.pollydocs.org/
- **Gemini API Docs**: https://ai.google.dev/gemini-api/docs
- **OpenAI API Docs**: https://platform.openai.com/docs
- **Anthropic Claude Docs**: https://docs.anthropic.com/claude/reference
