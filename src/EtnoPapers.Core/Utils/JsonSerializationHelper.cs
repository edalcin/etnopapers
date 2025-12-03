using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using EtnoPapers.Core.Models;

namespace EtnoPapers.Core.Utils
{
    /// <summary>
    /// Handles JSON serialization and deserialization for ArticleRecord objects.
    /// Ensures compatibility with Electron-generated JSON files.
    /// </summary>
    public class JsonSerializationHelper
    {
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            DateFormatString = "yyyy-MM-ddTHH:mm:ss.fffZ",
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented
        };

        /// <summary>
        /// Serializes an ArticleRecord to JSON string.
        /// </summary>
        public static string SerializeToJson(ArticleRecord record)
        {
            return JsonConvert.SerializeObject(record, Settings);
        }

        /// <summary>
        /// Deserializes JSON string to ArticleRecord.
        /// </summary>
        public static ArticleRecord DeserializeFromJson(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<ArticleRecord>(json, Settings);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("Failed to deserialize JSON to ArticleRecord", ex);
            }
        }

        /// <summary>
        /// Serializes a list of ArticleRecords to JSON string.
        /// </summary>
        public static string SerializeList(List<ArticleRecord> records)
        {
            return JsonConvert.SerializeObject(records, Settings);
        }

        /// <summary>
        /// Deserializes JSON string to list of ArticleRecords.
        /// </summary>
        public static List<ArticleRecord> DeserializeList(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<List<ArticleRecord>>(json, Settings) ?? new();
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("Failed to deserialize JSON to ArticleRecord list", ex);
            }
        }

        /// <summary>
        /// Performs round-trip serialization test: object -> JSON -> object
        /// Used to verify Electron compatibility.
        /// </summary>
        public static bool RoundTripTest(ArticleRecord original)
        {
            try
            {
                var json = SerializeToJson(original);
                var deserialized = DeserializeFromJson(json);
                return original.Titulo == deserialized.Titulo &&
                       original.Ano == deserialized.Ano;
            }
            catch
            {
                return false;
            }
        }
    }
}
