using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EtnoPapers.Core.Services
{
    /// <summary>
    /// Integrates with local OLLAMA AI service for metadata extraction.
    /// </summary>
    public class OLLAMAService
    {
        private readonly string _baseUrl;
        private string _model;
        private readonly HttpClient _httpClient;

        // Preferred models for structured extraction (in order of preference)
        private static readonly string[] PreferredModels =
        {
            "mistral",
            "neural-chat",
            "dolphin-mixtral",
            "llama2",
            "orca2"
        };

        public OLLAMAService(string baseUrl = "http://localhost:11434", string model = null)
        {
            _baseUrl = baseUrl?.TrimEnd('/') ?? "http://localhost:11434";
            _model = model;
            _httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };
        }

        /// <summary>
        /// Gets the current model being used
        /// </summary>
        public string CurrentModel => _model;

        /// <summary>
        /// Sets the model to use for extraction
        /// </summary>
        public void SetModel(string model)
        {
            _model = model;
        }

        /// <summary>
        /// Checks if OLLAMA service is running and accessible.
        /// </summary>
        public async Task<bool> CheckHealthAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/tags");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Automatically detects and selects the best available model for extraction.
        /// Returns the selected model name or null if no suitable model found.
        /// </summary>
        public async Task<string> AutoDetectBestModelAsync()
        {
            var availableModels = await GetAvailableModelsAsync();
            if (availableModels.Count == 0)
                return null;

            // Try to find a preferred model
            foreach (var preferred in PreferredModels)
            {
                if (availableModels.Contains(preferred))
                {
                    _model = preferred;
                    System.Diagnostics.Debug.WriteLine($"Auto-detected model: {preferred}");
                    return preferred;
                }
            }

            // Fallback to first available model
            _model = availableModels[0];
            System.Diagnostics.Debug.WriteLine($"No preferred model found. Using: {_model}");
            return _model;
        }

        /// <summary>
        /// Extracts metadata from PDF text using OLLAMA with retry logic.
        /// Returns JSON response from AI model.
        /// </summary>
        public async Task<string> ExtractMetadataAsync(string pdfText, string customPrompt = null)
        {
            if (string.IsNullOrWhiteSpace(pdfText))
                throw new ArgumentException("PDF text cannot be empty");

            try
            {
                // First verify OLLAMA is running
                var models = await GetAvailableModelsAsync();
                if (models.Count == 0)
                    throw new InvalidOperationException($"OLLAMA is not responding at {_baseUrl}. Please ensure OLLAMA is running.");

                // Auto-detect best model if not specified
                if (string.IsNullOrWhiteSpace(_model))
                {
                    var detectedModel = await AutoDetectBestModelAsync();
                    if (detectedModel == null)
                        throw new InvalidOperationException("No suitable model found in OLLAMA");
                }

                // Verify selected model exists
                if (!models.Contains(_model))
                {
                    System.Diagnostics.Debug.WriteLine($"Model '{_model}' not found. Available: {string.Join(", ", models)}");
                    var detectedModel = await AutoDetectBestModelAsync();
                    if (detectedModel == null)
                        throw new InvalidOperationException($"Model '{_model}' not found and no alternative found. Available: {string.Join(", ", models)}");
                }

                var prompt = customPrompt != null
                    ? $"{customPrompt}\n\n{pdfText}"  // Append markdown content to custom prompt
                    : GenerateDefaultPrompt(pdfText);  // Use default prompt with markdown already included
                return await ExtractWithRetryAsync(_model, prompt);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to extract metadata from OLLAMA: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Extracts with retry logic, increasing timeout on failure.
        /// </summary>
        private async Task<string> ExtractWithRetryAsync(string model, string prompt, int retryCount = 0)
        {
            const int maxRetries = 2;
            const int baseTimeoutMinutes = 5;

            try
            {
                // Increase timeout with each retry (5min, 10min, 10min max)
                var timeoutMinutes = baseTimeoutMinutes * (retryCount + 1);
                if (timeoutMinutes > 10) timeoutMinutes = 10;

                var requestBody = new { model, prompt, stream = false };
                var content = new StringContent(
                    JsonConvert.SerializeObject(requestBody),
                    Encoding.UTF8,
                    "application/json"
                );

                System.Diagnostics.Debug.WriteLine($"Extraction attempt {retryCount + 1}/{maxRetries + 1} with timeout {timeoutMinutes}min");

                // Use CancellationToken for thread-safe timeout (instead of modifying HttpClient.Timeout)
                using (var cts = new CancellationTokenSource(TimeSpan.FromMinutes(timeoutMinutes)))
                {
                    var response = await _httpClient.PostAsync($"{_baseUrl}/api/generate", content, cts.Token);
                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        throw new HttpRequestException($"OLLAMA request failed: {response.StatusCode}");
                    }

                    var responseContent = await response.Content.ReadAsStringAsync();

                    System.Diagnostics.Debug.WriteLine($"OLLAMA Raw Response (full): {responseContent}");

                    // OLLAMA returns response in format: { "response": "...", "model": "...", ... }
                    // We need to extract just the "response" field which contains our JSON
                    try
                    {
                        dynamic ollamaResponse = JsonConvert.DeserializeObject(responseContent);
                        string extractedJsonResponse = ollamaResponse?.response?.ToString();

                        System.Diagnostics.Debug.WriteLine($"Parsed OLLAMA response object. Response field: {(extractedJsonResponse != null ? extractedJsonResponse.Substring(0, Math.Min(200, extractedJsonResponse.Length)) : "NULL")}");

                        if (string.IsNullOrEmpty(extractedJsonResponse))
                        {
                            System.Diagnostics.Debug.WriteLine($"Empty response field from OLLAMA. Full OLLAMA response: {responseContent}");
                            throw new InvalidOperationException("OLLAMA returned empty response field");
                        }

                        System.Diagnostics.Debug.WriteLine($"Extracted response (first 500 chars): {extractedJsonResponse.Substring(0, Math.Min(500, extractedJsonResponse.Length))}");
                        return extractedJsonResponse;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error parsing OLLAMA response: {ex.GetType().Name}: {ex.Message}");
                        System.Diagnostics.Debug.WriteLine($"Raw OLLAMA response: {responseContent}");
                        throw;
                    }
                }
            }
            catch (Exception ex) when (retryCount < maxRetries)
            {
                System.Diagnostics.Debug.WriteLine($"Extraction failed (attempt {retryCount + 1}): {ex.Message}. Retrying...");
                await Task.Delay(2000); // Wait 2 seconds before retry
                return await ExtractWithRetryAsync(model, prompt, retryCount + 1);
            }
        }

        /// <summary>
        /// Translates text to Portuguese using OLLAMA.
        /// </summary>
        public async Task<string> TranslateToPortugueseAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            var prompt = $"Translate the following text to Brazilian Portuguese. Return only the translation:\n\n{text}";
            var response = await ExtractMetadataAsync(text, prompt);
            return response;
        }

        /// <summary>
        /// Gets list of available models on OLLAMA.
        /// </summary>
        public async Task<List<string>> GetAvailableModelsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/tags");
                var content = await response.Content.ReadAsStringAsync();
                dynamic data = JsonConvert.DeserializeObject(content);
                var models = new List<string>();
                foreach (var model in data.models)
                {
                    models.Add((string)model.name);
                }
                return models;
            }
            catch
            {
                return new List<string>();
            }
        }

        private static string GenerateDefaultPrompt(string markdownContent)
        {
            return $@"Analyze the following scientific paper about traditional plant use by indigenous and traditional communities.

IMPORTANT: The content below is in structured Markdown format with:
- # Headings for main titles and sections
- ## Subheadings for subsections
- Tables formatted as | markdown tables |
- Clear paragraph separation

Extract metadata ACCURATELY from this structured content. DO NOT invent or hallucinate information.

EXTRACTION RULES:
1. Title: Usually the FIRST # heading in the document
2. Authors: Listed after the title, often with affiliations or email addresses
3. Year: Found in publication date, header, or references (4-digit year only, 1500-2026)
4. Abstract: Section titled ""Abstract"", ""Resumo"", or similar
5. Return valid JSON only - no markdown code blocks, no explanations
6. For missing fields, use null (not empty strings, not 'unknown', not guesses)
7. Resumo: ALWAYS in Brazilian Portuguese (translate if the abstract is in another language)
8. DO NOT extract information from References or Bibliography sections for main metadata

EXAMPLE OUTPUT:
{{
  ""titulo"": ""Medicinal Plants Used by Guarani Communities in Southern Brazil"",
  ""autores"": [""Silva, J.P."", ""Santos, M.A."", ""Oliveira, R.T.""],
  ""ano"": 2019,
  ""resumo"": ""Este estudo investigou o uso tradicional de plantas medicinais entre comunidades Guarani no sul do Brasil. Foram documentadas 47 espécies utilizadas para diversos fins medicinais, incluindo tratamento de doenças respiratórias, digestivas e infecções."",
  ""pais"": ""Brazil"",
  ""estado"": ""Mato Grosso do Sul"",
  ""municipio"": ""Dourados"",
  ""local"": ""Terra Indígena Guarani de Dourados"",
  ""bioma"": ""Atlantic Forest"",
  ""comunidade"": {{
    ""nome"": ""Guarani Kaiowá"",
    ""localizacao"": ""Dourados, Mato Grosso do Sul, Brazil""
  }},
  ""especies"": [
    {{
      ""nome_vernacular"": [""Jaborandi"", ""Jaborandi-verdadeiro""],
      ""nome_cientifico"": [""Pilocarpus microphyllus""],
      ""tipo_uso"": ""Medicinal - tratamento de febres e dores de cabeça""
    }},
    {{
      ""nome_vernacular"": [""Guaco""],
      ""nome_cientifico"": [""Mikania glomerata"", ""Mikania laevigata""],
      ""tipo_uso"": ""Medicinal - problemas respiratórios""
    }}
  ],
  ""metodologia"": ""Foram realizadas entrevistas semiestruturadas com 25 informantes-chave da comunidade, seguidas de caminhadas guiadas para identificação das espécies vegetais utilizadas."",
  ""ano_coleta"": 2018
}}

Now extract from this structured Markdown paper:

{markdownContent}";
        }
    }
}
