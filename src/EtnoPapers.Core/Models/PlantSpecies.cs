using Newtonsoft.Json;

namespace EtnoPapers.Core.Models
{
    /// <summary>
    /// Represents a plant species mentioned in an article with ethnobotanical use information.
    /// </summary>
    public class PlantSpecies
    {
        [JsonProperty("nome_vernacular")]
        public string NomeVernacular { get; set; }

        [JsonProperty("nome_cientifico")]
        public string NomeCientifico { get; set; }

        [JsonProperty("tipo_uso")]
        public string TipoUso { get; set; }

        [JsonProperty("parte_usada")]
        public string ParteUsada { get; set; }

        [JsonProperty("preparacao")]
        public string Preparacao { get; set; }
    }
}
