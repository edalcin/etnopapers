using System.Collections.Generic;
using Newtonsoft.Json;
using EtnoPapers.Core.Converters;

namespace EtnoPapers.Core.Models
{
    /// <summary>
    /// Represents a plant species mentioned in an article with ethnobotanical use information.
    /// Supports multiple vernacular and scientific names.
    /// </summary>
    [JsonConverter(typeof(PlantSpeciesConverter))]
    public class PlantSpecies
    {
        [JsonProperty("nome_vernacular")]
        public List<string> NomeVernacular { get; set; } = new List<string>();

        [JsonProperty("nome_cientifico")]
        public List<string> NomeCientifico { get; set; } = new List<string>();

        [JsonProperty("tipo_uso")]
        public string TipoUso { get; set; }

        [JsonProperty("parte_usada")]
        public string ParteUsada { get; set; }

        [JsonProperty("preparacao")]
        public string Preparacao { get; set; }
    }
}
