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

                var prompt = customPrompt ?? GenerateDefaultPrompt(pdfText);
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
            const int baseTimeoutSeconds = 5;

            try
            {
                // Increase timeout with each retry
                var timeoutSeconds = baseTimeoutSeconds * (retryCount + 1);
                _httpClient.Timeout = TimeSpan.FromSeconds(Math.Min(timeoutSeconds * 60, 600)); // Max 10 minutes

                var requestBody = new { model, prompt, stream = false };
                var content = new StringContent(
                    JsonConvert.SerializeObject(requestBody),
                    Encoding.UTF8,
                    "application/json"
                );

                System.Diagnostics.Debug.WriteLine($"Extraction attempt {retryCount + 1}/{maxRetries + 1} with timeout {timeoutSeconds * 60}s");

                var response = await _httpClient.PostAsync($"{_baseUrl}/api/generate", content);
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"OLLAMA request failed: {response.StatusCode}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                return responseContent;
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

        private static string GenerateDefaultPrompt(string pdfText)
        {
            return $@"Analyze the following academic paper about traditional plant use by indigenous and traditional communities. Extract ONLY valid information found in the text. Return a JSON object with these fields:

IMPORTANT RULES:
1. Return valid JSON only - no markdown, no explanations
2. For missing fields, use null (not empty strings or 'unknown')
3. Autores: array of full names as found in the paper
4. Ano: 4-digit year only (1500-2026)
5. Resumo: always in Brazilian Portuguese (translate if needed)
6. Return all text content, not just the first 2000 characters

EXAMPLE OUTPUT:
{{
  ""titulo"": ""Medicinal Plants Used by Guarani Communities"",
  ""autores"": [""Silva, J."", ""Santos, M.""],
  ""ano"": 2019,
  ""resumo"": ""Este estudo investigou o uso de plantas medicinais entre comunidades Guarani. Foram documentadas 47 espécies utilizadas para fins medicinais."",
  ""pais"": ""Brazil"",
  ""estado"": ""Mato Grosso do Sul"",
  ""municipio"": null,
  ""local"": ""Indigenous Territory"",
  ""bioma"": ""Atlantic Forest"",
  ""comunidade"": {{
    ""nome"": ""Guarani de Iguape"",
    ""localizacao"": ""Mato Grosso do Sul, Brazil""
  }},
  ""especies"": [
    {{
      ""nome_vernacular"": ""Jaborandi"",
      ""nome_cientifico"": ""Pilocarpus microphyllus"",
      ""tipo_uso"": ""Medicinal""
    }}
  ],
  ""metodologia"": ""Estudo etnobotânico com 25 informantes locais, utilizando entrevistas semiestruturadas."",
  ""ano_coleta"": 2018
}}

Now extract from this paper text:

{pdfText}";
        }
    }
}
