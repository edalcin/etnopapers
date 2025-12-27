using System.Collections.Generic;
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

        [JsonProperty("tipo")]
        public string Tipo { get; set; }

        [JsonProperty("municipio")]
        public string Municipio { get; set; }

        [JsonProperty("estado")]
        public string Estado { get; set; }

        [JsonProperty("local")]
        public string Local { get; set; }

        [JsonProperty("atividadesEconomicas")]
        public List<string> AtividadesEconomicas { get; set; } = new List<string>();

        [JsonProperty("observacoes")]
        public string Observacoes { get; set; }

        [JsonProperty("plantas")]
        public List<PlantSpecies> Plantas { get; set; } = new List<PlantSpecies>();
    }
}
