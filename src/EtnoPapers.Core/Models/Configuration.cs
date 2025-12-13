using Newtonsoft.Json;

namespace EtnoPapers.Core.Models
{
    /// <summary>
    /// Holds application configuration for cloud AI providers and MongoDB settings.
    /// Supports Gemini, OpenAI, and Anthropic for metadata extraction.
    ///
    /// Note: Legacy OLLAMA fields (OllamaUrl, OllamaModel, OllamaPrompt) are preserved for backward
    /// compatibility with existing configurations but are no longer used by the application.
    /// Migration banner is shown when legacy config is detected.
    /// </summary>
    public class Configuration
    {
        // Cloud AI Provider Configuration (NEW in v1.1.0)
        [JsonProperty("aiProvider")]
        public AIProviderType? AIProvider { get; set; }

        [JsonProperty("apiKey")]
        public string ApiKey { get; set; }

        // Legacy OLLAMA Fields (DEPRECATED - kept for backward compatibility)
        [JsonProperty("ollamaUrl")]
        public string OllamaUrl { get; set; } = "http://localhost:11434";

        [JsonProperty("ollamaModel")]
        public string OllamaModel { get; set; } = "llama2";

        [JsonProperty("ollamaPrompt")]
        public string OllamaPrompt { get; set; }

        // MongoDB Configuration
        [JsonProperty("mongodbUri")]
        public string MongodbUri { get; set; }

        // UI Settings
        [JsonProperty("language")]
        public string Language { get; set; } = "pt-BR";

        [JsonProperty("windowWidth")]
        public int WindowWidth { get; set; } = 1200;

        [JsonProperty("windowHeight")]
        public int WindowHeight { get; set; } = 800;

        [JsonProperty("windowX")]
        public int WindowX { get; set; } = 100;

        [JsonProperty("windowY")]
        public int WindowY { get; set; } = 100;

        [JsonProperty("windowMaximized")]
        public bool WindowMaximized { get; set; } = false;

        /// <summary>
        /// Determines if this is a legacy OLLAMA-only configuration
        /// (used to show migration banner in UI).
        /// </summary>
        [JsonIgnore]
        public bool IsLegacyOllamaConfig =>
            !string.IsNullOrEmpty(OllamaUrl) &&
            AIProvider == null &&
            string.IsNullOrEmpty(ApiKey);

        /// <summary>
        /// Determines if cloud AI provider is properly configured.
        /// </summary>
        [JsonIgnore]
        public bool IsCloudAIConfigured =>
            AIProvider.HasValue &&
            !string.IsNullOrEmpty(ApiKey);
    }
}
