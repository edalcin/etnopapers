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
        private readonly string _model;
        private readonly HttpClient _httpClient;

        public OLLAMAService(string baseUrl = "http://localhost:11434", string model = "llama2")
        {
            _baseUrl = baseUrl?.TrimEnd('/') ?? "http://localhost:11434";
            _model = model ?? "llama2";
            _httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };
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
        /// Extracts metadata from PDF text using OLLAMA.
        /// Returns JSON response from AI model.
        /// </summary>
        public async Task<string> ExtractMetadataAsync(string pdfText, string customPrompt = null)
        {
            if (string.IsNullOrWhiteSpace(pdfText))
                throw new ArgumentException("PDF text cannot be empty");

            try
            {
                var prompt = customPrompt ?? GenerateDefaultPrompt(pdfText);
                var requestBody = new { model = _model, prompt, stream = false };
                var content = new StringContent(
                    JsonConvert.SerializeObject(requestBody),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync($"{_baseUrl}/api/generate", content);
                if (!response.IsSuccessStatusCode)
                    throw new HttpRequestException($"OLLAMA request failed: {response.StatusCode}");

                var responseContent = await response.Content.ReadAsStringAsync();
                return responseContent;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to extract metadata from OLLAMA: {ex.Message}", ex);
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
            return $@"Analyze the following academic paper about traditional plant use and extract the following information in JSON format:
{{
  'titulo': title in Portuguese or English as found,
  'autores': array of author names,
  'ano': publication year,
  'resumo': brief summary in Brazilian Portuguese,
  'pais': country of study,
  'especies': array of plant species with {{nome_vernacular, nome_cientifico, tipo_uso}},
  'comunidade': {{nome, localizacao}},
  'metodologia': research methodology
}}

Paper text:
{pdfText.Substring(0, Math.Min(2000, pdfText.Length))}";
        }
    }
}
