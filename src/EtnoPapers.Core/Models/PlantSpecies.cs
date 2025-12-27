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
        [JsonProperty("nomeVernacular")]
        public List<string> NomeVernacular { get; set; } = new List<string>();

        [JsonProperty("nomeCientifico")]
        public List<string> NomeCientifico { get; set; } = new List<string>();

        [JsonProperty("tipoUso")]
        public List<string> TipoUso { get; set; } = new List<string>();
    }
}
