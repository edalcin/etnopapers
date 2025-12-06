using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using EtnoPapers.Core.Models;

namespace EtnoPapers.Core.Converters
{
    /// <summary>
    /// Custom JSON converter for PlantSpecies that handles both string and array formats
    /// and automatically splits comma-separated values into arrays.
    /// </summary>
    public class PlantSpeciesConverter : JsonConverter<PlantSpecies>
    {
        public override PlantSpecies ReadJson(JsonReader reader, Type objectType, PlantSpecies existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var jo = JObject.Load(reader);
            var species = new PlantSpecies();

            // Parse nome_vernacular - can be string or array
            if (jo["nome_vernacular"] != null)
            {
                species.NomeVernacular = ParseNameArray(jo["nome_vernacular"]);
            }

            // Parse nome_cientifico - can be string or array
            if (jo["nome_cientifico"] != null)
            {
                species.NomeCientifico = ParseNameArray(jo["nome_cientifico"]);
            }

            // Parse other fields
            species.TipoUso = jo["tipo_uso"]?.ToString();
            species.ParteUsada = jo["parte_usada"]?.ToString();
            species.Preparacao = jo["preparacao"]?.ToString();

            return species;
        }

        public override void WriteJson(JsonWriter writer, PlantSpecies value, JsonSerializer serializer)
        {
            var jo = new JObject();

            // Write nome_vernacular as array
            if (value.NomeVernacular != null && value.NomeVernacular.Count > 0)
            {
                jo["nome_vernacular"] = JArray.FromObject(value.NomeVernacular);
            }
            else
            {
                jo["nome_vernacular"] = null;
            }

            // Write nome_cientifico as array
            if (value.NomeCientifico != null && value.NomeCientifico.Count > 0)
            {
                jo["nome_cientifico"] = JArray.FromObject(value.NomeCientifico);
            }
            else
            {
                jo["nome_cientifico"] = null;
            }

            jo["tipo_uso"] = value.TipoUso;
            jo["parte_usada"] = value.ParteUsada;
            jo["preparacao"] = value.Preparacao;

            jo.WriteTo(writer);
        }

        /// <summary>
        /// Parses a JToken that can be either a string (comma-separated) or an array
        /// and returns a List of trimmed, non-empty strings.
        /// </summary>
        private List<string> ParseNameArray(JToken token)
        {
            var result = new List<string>();

            if (token.Type == JTokenType.Array)
            {
                // If it's already an array, just extract the strings
                foreach (var item in token.Children())
                {
                    string value = item.ToString()?.Trim();
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        result.Add(value);
                    }
                }
            }
            else if (token.Type == JTokenType.String)
            {
                // If it's a string, split by comma and trim each part
                string value = token.ToString();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    var parts = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var part in parts)
                    {
                        string trimmed = part.Trim();
                        if (!string.IsNullOrWhiteSpace(trimmed))
                        {
                            result.Add(trimmed);
                        }
                    }
                }
            }

            return result;
        }
    }
}
