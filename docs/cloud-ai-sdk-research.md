# Cloud AI Provider SDK Research for C#/.NET Integration

**Date**: 2025-12-09
**Context**: EtnoPapers Windows desktop application - Migration from local OLLAMA to cloud AI providers
**Target Framework**: .NET 6+ / WPF Desktop Application

---

## 1. Google Gemini API

### Official SDK

**NuGet Package**: `Google.Cloud.AIPlatform.V1`
- **Latest Version**: 3.x+ (as of 2025)
- **Alternative**: Direct HTTP calls using `System.Net.Http.HttpClient`

**Simpler Alternative**: `GenerativeAI` (community package)
- More straightforward for basic text generation
- Less enterprise features than official Google Cloud SDK

### Authentication

- **Method**: API Key in URL parameter or header
- **Header**: `x-goog-api-key: YOUR_API_KEY`
- **URL Parameter**: `?key=YOUR_API_KEY`

### API Endpoint

```
POST https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent
```

**Recommended Models**:
- `gemini-1.5-pro` - Best for complex extraction tasks
- `gemini-1.5-flash` - Faster, cost-effective for simpler tasks

### Request Structure (JSON)

```json
{
  "contents": [
    {
      "parts": [
        {
          "text": "Your prompt text here"
        }
      ]
    }
  ],
  "generationConfig": {
    "temperature": 0.1,
    "maxOutputTokens": 8192,
    "topP": 0.8
  }
}
```

### Response Structure (JSON)

```json
{
  "candidates": [
    {
      "content": {
        "parts": [
          {
            "text": "Generated response text"
          }
        ]
      },
      "finishReason": "STOP",
      "safetyRatings": [...]
    }
  ],
  "usageMetadata": {
    "promptTokenCount": 100,
    "candidatesTokenCount": 500,
    "totalTokenCount": 600
  }
}
```

### C# Example (HttpClient Approach)

```csharp
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class GeminiApiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private const string BaseUrl = "https://generativelanguage.googleapis.com/v1beta/models";

    public GeminiApiClient(string apiKey)
    {
        _apiKey = apiKey;
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
    }

    public async Task<string> GenerateContentAsync(string prompt, string model = "gemini-1.5-pro")
    {
        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            },
            generationConfig = new
            {
                temperature = 0.1,
                maxOutputTokens = 8192
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var url = $"{BaseUrl}/{model}:generateContent?key={_apiKey}";
        var response = await _httpClient.PostAsync(url, content);

        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(responseJson);

        return jsonDoc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();
    }
}
```

### Usage in WPF Application

```csharp
// In your ViewModel or service class
private async Task ExtractMetadataWithGeminiAsync(string pdfText)
{
    var client = new GeminiApiClient(ConfigurationManager.AppSettings["GeminiApiKey"]);

    try
    {
        var prompt = $@"Extract ethnobotanical metadata from this paper:
{pdfText}

Return as JSON with fields: titulo, autores, ano, resumo (in Portuguese), especies...";

        var result = await client.GenerateContentAsync(prompt);

        // Parse result and populate your data model
        var metadata = JsonSerializer.Deserialize<ArticleMetadata>(result);

        // Update UI on dispatcher thread
        Application.Current.Dispatcher.Invoke(() =>
        {
            // Update your UI here
        });
    }
    catch (HttpRequestException ex)
    {
        // Handle API errors
        MessageBox.Show($"API Error: {ex.Message}");
    }
}
```

---

## 2. OpenAI API

### Official SDK

**NuGet Package**: `Betalgo.OpenAI` (most popular community package)
- **Latest Version**: 8.x+ (as of 2025)
- **Package ID**: `Betalgo.OpenAI`
- **Highly recommended**: Well-maintained, extensive features

**Alternative**: Official `OpenAI` package by OpenAI
- Released in late 2024
- Less mature than Betalgo but official

### Authentication

- **Method**: Bearer token in Authorization header
- **Header**: `Authorization: Bearer YOUR_API_KEY`

### API Endpoint

```
POST https://api.openai.com/v1/chat/completions
```

**Recommended Models**:
- `gpt-4-turbo` or `gpt-4` - Best accuracy for structured extraction
- `gpt-3.5-turbo` - Cost-effective, good for simpler tasks
- `gpt-4o` - Multimodal, can process images directly (useful for PDFs)

### Request Structure (JSON)

```json
{
  "model": "gpt-4-turbo",
  "messages": [
    {
      "role": "system",
      "content": "You are an ethnobotany metadata extraction assistant."
    },
    {
      "role": "user",
      "content": "Extract metadata from this paper..."
    }
  ],
  "temperature": 0.1,
  "max_tokens": 4096,
  "response_format": { "type": "json_object" }
}
```

**Note**: `response_format: json_object` forces structured JSON output (requires GPT-4 Turbo or newer)

### Response Structure (JSON)

```json
{
  "id": "chatcmpl-abc123",
  "object": "chat.completion",
  "created": 1677858242,
  "model": "gpt-4-turbo",
  "choices": [
    {
      "index": 0,
      "message": {
        "role": "assistant",
        "content": "{\"titulo\": \"...\", \"autores\": [...]}"
      },
      "finish_reason": "stop"
    }
  ],
  "usage": {
    "prompt_tokens": 100,
    "completion_tokens": 500,
    "total_tokens": 600
  }
}
```

### C# Example (Using Betalgo.OpenAI SDK)

```csharp
using OpenAI.GPT3;
using OpenAI.GPT3.Managers;
using OpenAI.GPT3.ObjectModels;
using OpenAI.GPT3.ObjectModels.RequestModels;
using System.Threading.Tasks;

public class OpenAiApiClient
{
    private readonly OpenAIService _openAiService;

    public OpenAiApiClient(string apiKey)
    {
        _openAiService = new OpenAIService(new OpenAiOptions()
        {
            ApiKey = apiKey
        });
    }

    public async Task<string> ExtractMetadataAsync(string prompt)
    {
        var completionResult = await _openAiService.ChatCompletion.CreateCompletion(
            new ChatCompletionCreateRequest
            {
                Messages = new List<ChatMessage>
                {
                    ChatMessage.FromSystem("You are an ethnobotany metadata extraction assistant. Always respond with valid JSON."),
                    ChatMessage.FromUser(prompt)
                },
                Model = Models.Gpt_4_turbo,
                Temperature = 0.1f,
                MaxTokens = 4096,
                ResponseFormat = new ResponseFormat { Type = StaticValues.CompletionStatics.ResponseFormat.Json }
            });

        if (completionResult.Successful)
        {
            return completionResult.Choices[0].Message.Content;
        }
        else
        {
            throw new Exception($"OpenAI API Error: {completionResult.Error?.Message}");
        }
    }
}
```

### C# Example (Direct HttpClient Approach)

```csharp
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class OpenAiApiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private const string ApiUrl = "https://api.openai.com/v1/chat/completions";

    public OpenAiApiClient(string apiKey)
    {
        _apiKey = apiKey;
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _apiKey);
    }

    public async Task<string> ExtractMetadataAsync(string prompt)
    {
        var requestBody = new
        {
            model = "gpt-4-turbo",
            messages = new[]
            {
                new { role = "system", content = "You are an ethnobotany metadata extraction assistant." },
                new { role = "user", content = prompt }
            },
            temperature = 0.1,
            max_tokens = 4096,
            response_format = new { type = "json_object" }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(ApiUrl, content);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(responseJson);

        return jsonDoc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();
    }
}
```

### Usage in WPF Application

```csharp
// Using Betalgo.OpenAI SDK (recommended)
private async Task ExtractMetadataWithOpenAiAsync(string pdfText)
{
    var client = new OpenAiApiClient(ConfigurationManager.AppSettings["OpenAiApiKey"]);

    try
    {
        var prompt = $@"Extract ethnobotanical metadata from this paper and return as JSON:
{pdfText}

Required fields: titulo, autores (array), ano, resumo (in Portuguese), especies (array with vernacular_names, scientific_name, use_type)...";

        var result = await client.ExtractMetadataAsync(prompt);

        var metadata = JsonSerializer.Deserialize<ArticleMetadata>(result);

        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            // Update UI binding or ObservableCollection
            ExtractedArticles.Add(metadata);
        });
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Extraction Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
```

---

## 3. Anthropic Claude API

### Available SDKs

**NuGet Package**: `Anthropic.SDK` (unofficial community package)
- **Latest Version**: 0.x (relatively new as of 2025)
- **Status**: Functional but less mature than OpenAI packages

**Recommended Approach**: Direct HTTP calls using `HttpClient`
- Official .NET SDK not yet released
- REST API is straightforward and well-documented
- More control and reliability than unofficial packages

### Authentication

- **Method**: API Key in custom header
- **Header**: `x-api-key: YOUR_API_KEY`
- **Additional Required Header**: `anthropic-version: 2023-06-01`

### API Endpoint

```
POST https://api.anthropic.com/v1/messages
```

**Available Models** (as of 2025):
- `claude-3-opus-20240229` - Most capable, best for complex reasoning
- `claude-3-sonnet-20240229` - Balanced performance/cost
- `claude-3-haiku-20240307` - Fastest, most cost-effective
- `claude-3-5-sonnet-20241022` - Latest, enhanced capabilities

### Request Structure (JSON)

```json
{
  "model": "claude-3-5-sonnet-20241022",
  "max_tokens": 4096,
  "temperature": 0.1,
  "messages": [
    {
      "role": "user",
      "content": "Extract ethnobotanical metadata from this paper..."
    }
  ],
  "system": "You are an ethnobotany metadata extraction assistant."
}
```

**Key Differences from OpenAI**:
- `system` is a separate top-level parameter (not in messages array)
- `max_tokens` is required (no default)
- First message must be from "user" role

### Response Structure (JSON)

```json
{
  "id": "msg_abc123",
  "type": "message",
  "role": "assistant",
  "content": [
    {
      "type": "text",
      "text": "The extracted metadata is..."
    }
  ],
  "model": "claude-3-5-sonnet-20241022",
  "stop_reason": "end_turn",
  "usage": {
    "input_tokens": 100,
    "output_tokens": 500
  }
}
```

### C# Example (HttpClient Approach - Recommended)

```csharp
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class ClaudeApiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private const string ApiUrl = "https://api.anthropic.com/v1/messages";
    private const string AnthropicVersion = "2023-06-01";

    public ClaudeApiClient(string apiKey)
    {
        _apiKey = apiKey;
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(60) // Claude can be slower for complex tasks
        };
        _httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);
        _httpClient.DefaultRequestHeaders.Add("anthropic-version", AnthropicVersion);
    }

    public async Task<string> ExtractMetadataAsync(string prompt, string systemPrompt = null)
    {
        var requestBody = new
        {
            model = "claude-3-5-sonnet-20241022",
            max_tokens = 4096,
            temperature = 0.1,
            system = systemPrompt ?? "You are an ethnobotany metadata extraction assistant. Always respond with valid JSON.",
            messages = new[]
            {
                new
                {
                    role = "user",
                    content = prompt
                }
            }
        };

        var json = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(ApiUrl, content);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Claude API Error: {response.StatusCode} - {errorContent}");
        }

        var responseJson = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(responseJson);

        // Extract text from content array
        return jsonDoc.RootElement
            .GetProperty("content")[0]
            .GetProperty("text")
            .GetString();
    }
}
```

### C# Example with JSON Parsing Helper

```csharp
public class ClaudeApiClient
{
    // ... (previous code)

    public async Task<T> ExtractMetadataAsync<T>(string prompt, string systemPrompt = null)
    {
        var responseText = await ExtractMetadataAsync(prompt, systemPrompt);

        // Claude may wrap JSON in markdown code blocks - clean it
        var cleanedJson = CleanJsonResponse(responseText);

        return JsonSerializer.Deserialize<T>(cleanedJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    private string CleanJsonResponse(string response)
    {
        // Remove markdown code block markers if present
        response = response.Trim();

        if (response.StartsWith("```json"))
        {
            response = response.Substring(7);
        }
        else if (response.StartsWith("```"))
        {
            response = response.Substring(3);
        }

        if (response.EndsWith("```"))
        {
            response = response.Substring(0, response.Length - 3);
        }

        return response.Trim();
    }
}
```

### Usage in WPF Application

```csharp
private async Task ExtractMetadataWithClaudeAsync(string pdfText)
{
    var client = new ClaudeApiClient(ConfigurationManager.AppSettings["ClaudeApiKey"]);

    try
    {
        var systemPrompt = @"You are an expert ethnobotany metadata extraction assistant.
Extract data according to the specified JSON schema.
Always ensure 'resumo' is translated to Brazilian Portuguese.
Return only valid JSON, no markdown formatting.";

        var userPrompt = $@"Extract ethnobotanical metadata from this paper:

{pdfText}

Return as JSON with this structure:
{{
  ""titulo"": ""string"",
  ""autores"": [""Author 1"", ""Author 2""],
  ""ano"": 2024,
  ""resumo"": ""Portuguese abstract"",
  ""especies"": [
    {{
      ""vernacular_names"": [""common name""],
      ""scientific_name"": ""Species name"",
      ""use_type"": ""medicinal""
    }}
  ],
  ""pais"": ""Brazil"",
  ""estado"": ""state"",
  ""municipio"": ""city"",
  ""bioma"": ""biome"",
  ""comunidade"": {{
    ""nome"": ""community name"",
    ""localizacao"": ""location""
  }},
  ""metodologia"": ""methodology description""
}}";

        var metadata = await client.ExtractMetadataAsync<ArticleMetadata>(
            userPrompt,
            systemPrompt
        );

        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            ExtractedArticles.Add(metadata);
            StatusMessage = $"Successfully extracted: {metadata.Titulo}";
        });
    }
    catch (HttpRequestException ex)
    {
        MessageBox.Show($"Claude API Error: {ex.Message}", "Error",
            MessageBoxButton.OK, MessageBoxImage.Error);
    }
    catch (JsonException ex)
    {
        MessageBox.Show($"JSON Parsing Error: {ex.Message}", "Error",
            MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
```

---

## 4. Common Patterns and Best Practices

### HttpClient Configuration

**Singleton Pattern for HttpClient** (Critical for performance)

```csharp
// BAD: Creates new HttpClient for each request (socket exhaustion)
public async Task<string> CallApiAsync()
{
    using var client = new HttpClient(); // DON'T DO THIS
    // ...
}

// GOOD: Reuse HttpClient instance (implement in service class)
public class ApiClientService
{
    private static readonly HttpClient _httpClient = new HttpClient
    {
        Timeout = TimeSpan.FromSeconds(30)
    };

    // Use _httpClient for all requests
}

// BEST: Use IHttpClientFactory in .NET Core/.NET 5+ applications
public class ApiClientService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ApiClientService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<string> CallApiAsync()
    {
        var client = _httpClientFactory.CreateClient("ApiClient");
        // ...
    }
}
```

### Timeout Recommendations

```csharp
// Different timeouts based on expected operation duration
public class TimeoutConfiguration
{
    // For fast models (Gemini Flash, GPT-3.5, Claude Haiku)
    public static TimeSpan FastModelTimeout = TimeSpan.FromSeconds(15);

    // For standard models (GPT-4, Claude Sonnet, Gemini Pro)
    public static TimeSpan StandardModelTimeout = TimeSpan.FromSeconds(30);

    // For complex tasks (long documents, Claude Opus)
    public static TimeSpan ComplexTaskTimeout = TimeSpan.FromSeconds(60);

    // For very large documents or batch processing
    public static TimeSpan ExtendedTimeout = TimeSpan.FromSeconds(120);
}

// Usage
_httpClient.Timeout = TimeoutConfiguration.StandardModelTimeout;
```

### Retry Strategy with Exponential Backoff

```csharp
using Polly;
using Polly.Retry;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

public class ResilientApiClient
{
    private readonly HttpClient _httpClient;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;

    public ResilientApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;

        // Define retry policy with exponential backoff
        _retryPolicy = Policy
            .HandleResult<HttpResponseMessage>(r =>
                r.StatusCode == HttpStatusCode.TooManyRequests || // 429
                r.StatusCode == HttpStatusCode.ServiceUnavailable || // 503
                r.StatusCode == HttpStatusCode.InternalServerError) // 500
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // 2, 4, 8 seconds
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"Retry {retryCount} after {timespan.TotalSeconds}s due to {outcome.Result.StatusCode}");
                }
            );
    }

    public async Task<HttpResponseMessage> PostWithRetryAsync(string url, HttpContent content)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            var response = await _httpClient.PostAsync(url, content);
            return response;
        });
    }
}

// Install Polly via NuGet: Install-Package Polly
```

### Simpler Retry Without Polly (Manual Implementation)

```csharp
public class ManualRetryApiClient
{
    private readonly HttpClient _httpClient;
    private const int MaxRetries = 3;

    public ManualRetryApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<HttpResponseMessage> PostWithRetryAsync(string url, HttpContent content)
    {
        int retryCount = 0;

        while (true)
        {
            try
            {
                var response = await _httpClient.PostAsync(url, content);

                // Check if we should retry
                if (response.StatusCode == HttpStatusCode.TooManyRequests ||
                    response.StatusCode == HttpStatusCode.ServiceUnavailable ||
                    response.StatusCode == HttpStatusCode.InternalServerError)
                {
                    if (retryCount >= MaxRetries)
                    {
                        return response; // Give up after max retries
                    }

                    retryCount++;
                    var delay = TimeSpan.FromSeconds(Math.Pow(2, retryCount));
                    await Task.Delay(delay);
                    continue;
                }

                return response;
            }
            catch (HttpRequestException ex) when (retryCount < MaxRetries)
            {
                retryCount++;
                var delay = TimeSpan.FromSeconds(Math.Pow(2, retryCount));
                await Task.Delay(delay);
            }
        }
    }
}
```

### Async/Await Patterns for WPF UI Responsiveness

```csharp
// ViewModel pattern with async command handling
public class ExtractionViewModel : INotifyPropertyChanged
{
    private bool _isProcessing;
    private string _statusMessage;

    public bool IsProcessing
    {
        get => _isProcessing;
        set
        {
            _isProcessing = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanExtract));
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            _statusMessage = value;
            OnPropertyChanged();
        }
    }

    public bool CanExtract => !IsProcessing;

    // Async command pattern
    public ICommand ExtractMetadataCommand { get; }

    public ExtractionViewModel()
    {
        ExtractMetadataCommand = new AsyncRelayCommand(
            ExecuteExtractMetadataAsync,
            () => CanExtract
        );
    }

    private async Task ExecuteExtractMetadataAsync()
    {
        IsProcessing = true;
        StatusMessage = "Extracting metadata...";

        try
        {
            // Long-running API call
            var result = await _apiClient.ExtractMetadataAsync(_pdfText);

            // Update UI on dispatcher thread (if needed)
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Articles.Add(result);
                StatusMessage = "Extraction completed successfully";
            });
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            MessageBox.Show(ex.Message, "Extraction Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsProcessing = false;
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
```

### Progress Reporting Pattern

```csharp
public class ApiClientWithProgress
{
    public async Task<ArticleMetadata> ExtractMetadataAsync(
        string pdfText,
        IProgress<string> progress = null,
        CancellationToken cancellationToken = default)
    {
        progress?.Report("Preparing API request...");

        var request = BuildRequest(pdfText);

        progress?.Report("Sending request to API...");

        var response = await _httpClient.PostAsync(ApiUrl, request, cancellationToken);

        progress?.Report("Receiving response...");

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        progress?.Report("Parsing JSON response...");

        var metadata = JsonSerializer.Deserialize<ArticleMetadata>(content);

        progress?.Report("Extraction completed");

        return metadata;
    }
}

// Usage in ViewModel
private async Task ExtractWithProgressAsync()
{
    var progress = new Progress<string>(status =>
    {
        StatusMessage = status;
    });

    var cts = new CancellationTokenSource();
    CancelCommand = new RelayCommand(() => cts.Cancel());

    try
    {
        var result = await _apiClient.ExtractMetadataAsync(
            _pdfText,
            progress,
            cts.Token
        );
    }
    catch (OperationCanceledException)
    {
        StatusMessage = "Operation cancelled by user";
    }
}
```

### Error Handling Pattern

```csharp
public class ApiErrorHandler
{
    public async Task<ApiResult<T>> TryExecuteAsync<T>(Func<Task<T>> apiCall)
    {
        try
        {
            var result = await apiCall();
            return ApiResult<T>.Success(result);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
        {
            return ApiResult<T>.Failure("Invalid API key. Please check your configuration.");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
        {
            return ApiResult<T>.Failure("Rate limit exceeded. Please try again later.");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
        {
            return ApiResult<T>.Failure($"Invalid request: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            return ApiResult<T>.Failure("Request timed out. The API may be overloaded.");
        }
        catch (JsonException ex)
        {
            return ApiResult<T>.Failure($"Failed to parse response: {ex.Message}");
        }
        catch (Exception ex)
        {
            return ApiResult<T>.Failure($"Unexpected error: {ex.Message}");
        }
    }
}

public class ApiResult<T>
{
    public bool IsSuccess { get; }
    public T Data { get; }
    public string ErrorMessage { get; }

    private ApiResult(bool isSuccess, T data, string errorMessage)
    {
        IsSuccess = isSuccess;
        Data = data;
        ErrorMessage = errorMessage;
    }

    public static ApiResult<T> Success(T data) =>
        new ApiResult<T>(true, data, null);

    public static ApiResult<T> Failure(string errorMessage) =>
        new ApiResult<T>(false, default, errorMessage);
}

// Usage
var result = await _errorHandler.TryExecuteAsync(async () =>
{
    return await _apiClient.ExtractMetadataAsync(pdfText);
});

if (result.IsSuccess)
{
    Articles.Add(result.Data);
}
else
{
    MessageBox.Show(result.ErrorMessage, "Error");
}
```

### Configuration Management Pattern

```csharp
// App.config or appsettings.json
public class AiProviderConfiguration
{
    public string Provider { get; set; } // "gemini", "openai", "claude"
    public string GeminiApiKey { get; set; }
    public string GeminiModel { get; set; } = "gemini-1.5-pro";
    public string OpenAiApiKey { get; set; }
    public string OpenAiModel { get; set; } = "gpt-4-turbo";
    public string ClaudeApiKey { get; set; }
    public string ClaudeModel { get; set; } = "claude-3-5-sonnet-20241022";
    public int TimeoutSeconds { get; set; } = 30;
    public int MaxRetries { get; set; } = 3;
}

// Factory pattern for client selection
public class AiClientFactory
{
    private readonly AiProviderConfiguration _config;

    public AiClientFactory(AiProviderConfiguration config)
    {
        _config = config;
    }

    public IAiClient CreateClient()
    {
        return _config.Provider.ToLower() switch
        {
            "gemini" => new GeminiApiClient(_config.GeminiApiKey),
            "openai" => new OpenAiApiClient(_config.OpenAiApiKey),
            "claude" => new ClaudeApiClient(_config.ClaudeApiKey),
            _ => throw new InvalidOperationException($"Unknown provider: {_config.Provider}")
        };
    }
}

// Common interface for all providers
public interface IAiClient
{
    Task<string> ExtractMetadataAsync(string prompt);
}
```

---

## 5. Recommendations for EtnoPapers Implementation

### SDK vs Direct HTTP: Decision Matrix

| Provider | Recommendation | Reason |
|----------|---------------|---------|
| **OpenAI** | Use `Betalgo.OpenAI` SDK | Mature, well-maintained, extensive features, active community |
| **Google Gemini** | Direct `HttpClient` | Simpler API, official SDK is overkill for basic needs |
| **Anthropic Claude** | Direct `HttpClient` | No official .NET SDK yet, unofficial packages immature |

### Model Selection for Ethnobotanical Extraction

| Provider | Recommended Model | Reasoning |
|----------|------------------|-----------|
| **OpenAI** | `gpt-4-turbo` or `gpt-4o` | Best structured data extraction, JSON mode support, good Portuguese |
| **Google Gemini** | `gemini-1.5-pro` | Large context window (good for long papers), cost-effective |
| **Anthropic Claude** | `claude-3-5-sonnet-20241022` | Best reasoning, excellent multilingual (Portuguese), strong JSON |

### Performance Characteristics

| Provider | Typical Latency | Cost (per 1K tokens) | Max Context | Best For |
|----------|----------------|---------------------|-------------|----------|
| **OpenAI GPT-4 Turbo** | 3-8 seconds | $0.01 input / $0.03 output | 128K tokens | Structured JSON extraction |
| **Gemini 1.5 Pro** | 2-5 seconds | $0.00125 input / $0.005 output | 2M tokens | Long documents, cost-sensitive |
| **Claude 3.5 Sonnet** | 4-10 seconds | $0.003 input / $0.015 output | 200K tokens | Complex reasoning, Portuguese |

### Architecture Recommendation for EtnoPapers

```
┌─────────────────────────────────────────────────┐
│           WPF Application Layer                 │
│  (ViewModels, Commands, UI Bindings)            │
└────────────────┬────────────────────────────────┘
                 │
┌────────────────┴────────────────────────────────┐
│        Extraction Service Layer                 │
│  - Provider selection logic                     │
│  - Prompt management                            │
│  - Response parsing/validation                  │
└────────────────┬────────────────────────────────┘
                 │
┌────────────────┴────────────────────────────────┐
│          AI Client Abstraction                  │
│  Interface: IAiClient                           │
│  Implementations:                               │
│    - GeminiApiClient (HttpClient)               │
│    - OpenAiApiClient (Betalgo.OpenAI SDK)       │
│    - ClaudeApiClient (HttpClient)               │
└────────────────┬────────────────────────────────┘
                 │
┌────────────────┴────────────────────────────────┐
│       Infrastructure Layer                      │
│  - HttpClient management                        │
│  - Retry policies (Polly or manual)             │
│  - Error handling                               │
│  - Configuration loading                        │
└─────────────────────────────────────────────────┘
```

### Implementation Checklist

**Phase 1: Foundation**
- [ ] Create `IAiClient` interface
- [ ] Implement configuration management (App.config or appsettings.json)
- [ ] Setup HttpClient with singleton pattern
- [ ] Implement basic error handling and logging

**Phase 2: Provider Implementations**
- [ ] Implement `OpenAiApiClient` using Betalgo.OpenAI SDK
- [ ] Implement `GeminiApiClient` using HttpClient
- [ ] Implement `ClaudeApiClient` using HttpClient
- [ ] Add retry logic with exponential backoff

**Phase 3: Service Layer**
- [ ] Create `ExtractionService` with provider selection
- [ ] Implement prompt management (load from config/database)
- [ ] Add response validation (check required fields)
- [ ] Implement progress reporting for UI

**Phase 4: UI Integration**
- [ ] Add provider selection dropdown in settings
- [ ] Implement async commands with progress indicators
- [ ] Add cancellation support (CancellationToken)
- [ ] Display extraction status and errors to user

**Phase 5: Testing & Polish**
- [ ] Test with sample PDFs from each biome/region
- [ ] Validate Portuguese translation quality in `resumo`
- [ ] Performance testing (latency, throughput)
- [ ] Error handling edge cases (rate limits, timeouts)

### NuGet Packages Required

```xml
<!-- For OpenAI (recommended approach) -->
<PackageReference Include="Betalgo.OpenAI" Version="8.0.0" />

<!-- For retry policies (optional but recommended) -->
<PackageReference Include="Polly" Version="8.0.0" />

<!-- For JSON serialization (built-in .NET 6+) -->
<PackageReference Include="System.Text.Json" Version="8.0.0" />

<!-- For async commands in WPF -->
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.0" />
```

### Sample Configuration File

```xml
<!-- App.config -->
<configuration>
  <appSettings>
    <!-- AI Provider Configuration -->
    <add key="AiProvider" value="openai" /> <!-- gemini, openai, claude -->

    <!-- OpenAI -->
    <add key="OpenAiApiKey" value="sk-..." />
    <add key="OpenAiModel" value="gpt-4-turbo" />

    <!-- Google Gemini -->
    <add key="GeminiApiKey" value="AIza..." />
    <add key="GeminiModel" value="gemini-1.5-pro" />

    <!-- Anthropic Claude -->
    <add key="ClaudeApiKey" value="sk-ant-..." />
    <add key="ClaudeModel" value="claude-3-5-sonnet-20241022" />

    <!-- API Settings -->
    <add key="ApiTimeoutSeconds" value="30" />
    <add key="MaxRetries" value="3" />
  </appSettings>
</configuration>
```

---

## Summary

### Quick Start Guide

1. **For immediate implementation**: Start with OpenAI using `Betalgo.OpenAI` SDK
   - Most mature ecosystem
   - Best documentation and community support
   - Excellent JSON structured output mode

2. **For cost optimization**: Add Google Gemini with direct HttpClient
   - Significantly cheaper than OpenAI
   - Good for high-volume processing
   - Simple REST API

3. **For quality/reasoning**: Add Anthropic Claude with direct HttpClient
   - Best for complex extraction logic
   - Excellent multilingual (Portuguese) support
   - Strong at following structured instructions

### Key Takeaways

- **Use HttpClient singleton pattern** to avoid socket exhaustion
- **Implement retry logic** for 429/500/503 errors with exponential backoff
- **Always use async/await** to keep WPF UI responsive
- **Handle progress and cancellation** for better user experience
- **Validate API responses** before deserializing (check for errors, required fields)
- **Abstract provider differences** behind common interface for easier switching
- **Store API keys securely** (encrypted config, Windows Credential Manager, Azure Key Vault)

### Next Steps

1. Review existing OLLAMA integration code in EtnoPapers
2. Design `IAiClient` interface matching current extraction flow
3. Implement OpenAI client first (easiest, best SDK support)
4. Add Gemini and Claude incrementally
5. Update UI to allow provider selection
6. Test with sample PDFs to validate extraction quality
