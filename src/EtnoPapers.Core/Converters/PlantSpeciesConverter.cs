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
            // Support both "nome_vernacular", "nomeVernacular" and "vernacular" field names (from AI providers)
            if (jo["nomeVernacular"] != null)
            {
                species.NomeVernacular = ParseNameArray(jo["nomeVernacular"]);
            }
            else if (jo["nome_vernacular"] != null)
            {
                species.NomeVernacular = ParseNameArray(jo["nome_vernacular"]);
            }
            else if (jo["vernacular"] != null)
            {
                species.NomeVernacular = ParseNameArray(jo["vernacular"]);
            }

            // Parse nome_cientifico - can be string or array
            // Support both "nomeCientifico", "nome_cientifico" field names (from AI providers)
            if (jo["nomeCientifico"] != null)
            {
                species.NomeCientifico = ParseNameArray(jo["nomeCientifico"]);
            }
            else if (jo["nome_cientifico"] != null)
            {
                species.NomeCientifico = ParseNameArray(jo["nome_cientifico"]);
            }

            // Parse tipoUso - can be string or array
            // Support both "tipo_uso" and "tipoUso" field names
            if (jo["tipoUso"] != null)
            {
                species.TipoUso = ParseNameArray(jo["tipoUso"]);
            }
            else if (jo["tipo_uso"] != null)
            {
                species.TipoUso = ParseNameArray(jo["tipo_uso"]);
            }

            return species;
        }

        public override void WriteJson(JsonWriter writer, PlantSpecies value, JsonSerializer serializer)
        {
            var jo = new JObject();

            // Write nomeVernacular as array
            if (value.NomeVernacular != null && value.NomeVernacular.Count > 0)
            {
                jo["nomeVernacular"] = JArray.FromObject(value.NomeVernacular);
            }
            else
            {
                jo["nomeVernacular"] = null;
            }

            // Write nomeCientifico as array
            if (value.NomeCientifico != null && value.NomeCientifico.Count > 0)
            {
                jo["nomeCientifico"] = JArray.FromObject(value.NomeCientifico);
            }
            else
            {
                jo["nomeCientifico"] = null;
            }

            // Write tipoUso as array
            if (value.TipoUso != null && value.TipoUso.Count > 0)
            {
                jo["tipoUso"] = JArray.FromObject(value.TipoUso);
            }
            else
            {
                jo["tipoUso"] = null;
            }

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
