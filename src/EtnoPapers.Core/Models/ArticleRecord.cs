using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace EtnoPapers.Core.Models
{
    /// <summary>
    /// Represents an ethnobotanical research article record with extracted metadata.
    /// This model matches the JSON schema from the Electron version for data compatibility.
    /// </summary>
    public class ArticleRecord
    {
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty("titulo")]
        public string Titulo { get; set; }

        [JsonProperty("autores")]
        public List<string> Autores { get; set; } = new();

        [JsonProperty("ano")]
        public int? Ano { get; set; }

        [JsonProperty("resumo")]
        public string Resumo { get; set; }

        [JsonProperty("especies")]
        public List<PlantSpecies> Especies { get; set; } = new();

        [JsonProperty("comunidade")]
        public Community Comunidade { get; set; }

        [JsonProperty("pais")]
        public string Pais { get; set; }

        [JsonProperty("estado")]
        public string Estado { get; set; }

        [JsonProperty("municipio")]
        public string Municipio { get; set; }

        [JsonProperty("local")]
        public string Local { get; set; }

        [JsonProperty("bioma")]
        public string Bioma { get; set; }

        [JsonProperty("metodologia")]
        public string Metodologia { get; set; }

        [JsonProperty("ano_coleta")]
        public int? AnoColeta { get; set; }

        [JsonProperty("data_criacao")]
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        [JsonProperty("data_ultima_atualizacao")]
        public DateTime DataUltimaAtualizacao { get; set; } = DateTime.UtcNow;

        [JsonProperty("status_sincronizacao")]
        public string StatusSincronizacao { get; set; } = "local";

        [JsonProperty("atributos_customizados")]
        public Dictionary<string, object> AtributosCustomizados { get; set; } = new();

        [JsonProperty("tempo_extracao_segundos")]
        public double? TempoExtracao { get; set; }
    }
}
