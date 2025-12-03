using Newtonsoft.Json;

namespace EtnoPapers.Core.Models
{
    /// <summary>
    /// Represents a traditional or indigenous community mentioned in the article.
    /// </summary>
    public class Community
    {
        [JsonProperty("nome")]
        public string Nome { get; set; }

        [JsonProperty("localizacao")]
        public string Localizacao { get; set; }

        [JsonProperty("povo")]
        public string Povo { get; set; }
    }
}
