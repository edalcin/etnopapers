using Newtonsoft.Json;

namespace EtnoPapers.Core.Models
{
    /// <summary>
    /// Holds application configuration for OLLAMA and MongoDB settings.
    /// </summary>
    public class Configuration
    {
        [JsonProperty("ollamaUrl")]
        public string OllamaUrl { get; set; } = "http://localhost:11434";

        [JsonProperty("ollamaModel")]
        public string OllamaModel { get; set; } = "llama2";

        [JsonProperty("ollamaPrompt")]
        public string OllamaPrompt { get; set; }

        [JsonProperty("mongodbUri")]
        public string MongodbUri { get; set; }

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
    }
}
