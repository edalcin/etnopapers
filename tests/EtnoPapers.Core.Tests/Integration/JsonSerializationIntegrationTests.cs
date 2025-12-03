using System;
using System.Collections.Generic;
using Xunit;
using Newtonsoft.Json;
using EtnoPapers.Core.Models;
using EtnoPapers.Core.Utils;

namespace EtnoPapers.Core.Tests.Integration
{
    /// <summary>
    /// Integration tests for JSON serialization/deserialization of ArticleRecord.
    /// Validates that records can be serialized to JSON and deserialized back without data loss.
    /// </summary>
    public class JsonSerializationIntegrationTests
    {
        private JsonSerializerSettings GetSerializerSettings()
        {
            return new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
                DateFormatString = "yyyy-MM-ddTHH:mm:ss.fffZ"
            };
        }

        [Fact]
        public void SerializeDeserialize_CompleteRecord_PreservesAllData()
        {
            // Arrange
            var originalRecord = new ArticleRecord
            {
                Id = Guid.NewGuid().ToString(),
                Titulo = "Medicinal Plants of the Amazon",
                Autores = new List<string> { "Silva, J.", "Santos, M." },
                Ano = 2023,
                Resumo = "This study investigates the medicinal properties of Amazon plants",
                Pais = "Brazil",
                Estado = "Amazonas",
                Municipio = "Manaus",
                Bioma = "Amazon",
                Especies = new List<PlantSpecies>
                {
                    new PlantSpecies
                    {
                        NomeVernacular = "Pau D'Arco",
                        NomeCientifico = "Tabebuia serratifolia",
                        TipoUso = "medicinal",
                        ParteUsada = "bark",
                        Preparacao = "decoction"
                    }
                },
                DataCriacao = new DateTime(2023, 1, 1, 12, 0, 0, DateTimeKind.Utc),
                DataUltimaAtualizacao = new DateTime(2023, 6, 15, 14, 30, 0, DateTimeKind.Utc),
                StatusSincronizacao = "pendente"
            };

            var settings = GetSerializerSettings();

            // Act
            var json = JsonConvert.SerializeObject(originalRecord, settings);
            var deserializedRecord = JsonConvert.DeserializeObject<ArticleRecord>(json, settings);

            // Assert
            Assert.NotNull(deserializedRecord);
            Assert.Equal(originalRecord.Id, deserializedRecord!.Id);
            Assert.Equal(originalRecord.Titulo, deserializedRecord.Titulo);
            Assert.Equal(originalRecord.Ano, deserializedRecord.Ano);
            Assert.Equal(originalRecord.Resumo, deserializedRecord.Resumo);
            Assert.Equal(originalRecord.Pais, deserializedRecord.Pais);
            Assert.Equal(originalRecord.Estado, deserializedRecord.Estado);
            Assert.Equal(originalRecord.Municipio, deserializedRecord.Municipio);
            Assert.Equal(originalRecord.Bioma, deserializedRecord.Bioma);
            Assert.Equal(2, deserializedRecord.Autores?.Count);
            Assert.Equal(1, deserializedRecord.Especies?.Count);
            Assert.Equal(originalRecord.StatusSincronizacao, deserializedRecord.StatusSincronizacao);
        }

        [Fact]
        public void SerializeDeserialize_WithNullOptionalFields_HandlesGracefully()
        {
            // Arrange
            var originalRecord = new ArticleRecord
            {
                Id = Guid.NewGuid().ToString(),
                Titulo = "Test Article",
                Autores = new List<string> { "Author One" },
                Ano = 2023,
                Resumo = "Test abstract",
                // Leave optional fields null
                Estado = null,
                Municipio = null,
                Especies = null,
                Comunidade = null,
                DataCriacao = DateTime.UtcNow,
                DataUltimaAtualizacao = DateTime.UtcNow,
                StatusSincronizacao = "pendente"
            };

            var settings = GetSerializerSettings();

            // Act
            var json = JsonConvert.SerializeObject(originalRecord, settings);
            var deserializedRecord = JsonConvert.DeserializeObject<ArticleRecord>(json, settings);

            // Assert
            Assert.NotNull(deserializedRecord);
            Assert.Equal(originalRecord.Titulo, deserializedRecord!.Titulo);
            Assert.Null(deserializedRecord.Estado);
            Assert.Null(deserializedRecord.Municipio);
        }

        [Fact]
        public void SerializeDeserialize_MultipleRecords_PreservesAllRecords()
        {
            // Arrange
            var records = new List<ArticleRecord>
            {
                new ArticleRecord
                {
                    Id = "1",
                    Titulo = "Article One",
                    Autores = new List<string> { "Author One" },
                    Ano = 2023,
                    Resumo = "Abstract one",
                    DataCriacao = DateTime.UtcNow,
                    DataUltimaAtualizacao = DateTime.UtcNow,
                    StatusSincronizacao = "pendente"
                },
                new ArticleRecord
                {
                    Id = "2",
                    Titulo = "Article Two",
                    Autores = new List<string> { "Author Two" },
                    Ano = 2022,
                    Resumo = "Abstract two",
                    DataCriacao = DateTime.UtcNow,
                    DataUltimaAtualizacao = DateTime.UtcNow,
                    StatusSincronizacao = "sincronizado"
                }
            };

            var settings = GetSerializerSettings();

            // Act
            var json = JsonConvert.SerializeObject(records, settings);
            var deserializedRecords = JsonConvert.DeserializeObject<List<ArticleRecord>>(json, settings);

            // Assert
            Assert.NotNull(deserializedRecords);
            Assert.Equal(2, deserializedRecords!.Count);
            Assert.Equal("Article One", deserializedRecords[0].Titulo);
            Assert.Equal("Article Two", deserializedRecords[1].Titulo);
            Assert.Equal(2023, deserializedRecords[0].Ano);
            Assert.Equal(2022, deserializedRecords[1].Ano);
        }

        [Fact]
        public void SerializeDeserialize_JsonString_ProducesValidFormat()
        {
            // Arrange
            var originalRecord = new ArticleRecord
            {
                Id = "test-id",
                Titulo = "Test Title",
                Autores = new List<string> { "Test Author" },
                Ano = 2023,
                Resumo = "Test abstract",
                DataCriacao = new DateTime(2023, 1, 1, 12, 0, 0, DateTimeKind.Utc),
                DataUltimaAtualizacao = new DateTime(2023, 1, 1, 12, 0, 0, DateTimeKind.Utc),
                StatusSincronizacao = "pendente"
            };

            var settings = GetSerializerSettings();

            // Act
            var json = JsonConvert.SerializeObject(originalRecord, settings);

            // Assert
            Assert.NotEmpty(json);
            Assert.Contains("\"titulo\"", json.ToLower());
            Assert.Contains("\"autores\"", json.ToLower());
            Assert.Contains("Test Title", json);
            Assert.Contains("Test Author", json);
        }

        [Fact]
        public void SerializeDeserialize_WithSpecies_PreservesSpeciesData()
        {
            // Arrange
            var originalRecord = new ArticleRecord
            {
                Id = "test-id",
                Titulo = "Plants Study",
                Autores = new List<string> { "Botanist" },
                Ano = 2023,
                Resumo = "Plant study",
                Especies = new List<PlantSpecies>
                {
                    new PlantSpecies
                    {
                        NomeVernacular = "Plant A",
                        NomeCientifico = "Genus A species",
                        TipoUso = "medicinal",
                        ParteUsada = "leaves",
                        Preparacao = "tea"
                    },
                    new PlantSpecies
                    {
                        NomeVernacular = "Plant B",
                        NomeCientifico = "Genus B species",
                        TipoUso = "food",
                        ParteUsada = "fruit",
                        Preparacao = "raw"
                    }
                },
                DataCriacao = DateTime.UtcNow,
                DataUltimaAtualizacao = DateTime.UtcNow,
                StatusSincronizacao = "pendente"
            };

            var settings = GetSerializerSettings();

            // Act
            var json = JsonConvert.SerializeObject(originalRecord, settings);
            var deserializedRecord = JsonConvert.DeserializeObject<ArticleRecord>(json, settings);

            // Assert
            Assert.NotNull(deserializedRecord?.Especies);
            Assert.Equal(2, deserializedRecord!.Especies.Count);
            Assert.Equal("Plant A", deserializedRecord.Especies[0].NomeVernacular);
            Assert.Equal("Genus A species", deserializedRecord.Especies[0].NomeCientifico);
            Assert.Equal("medicinal", deserializedRecord.Especies[0].TipoUso);
            Assert.Equal("Plant B", deserializedRecord.Especies[1].NomeVernacular);
        }

        [Fact]
        public void RoundTrip_MultipleSerializationCycles_MaintainsDataIntegrity()
        {
            // Arrange
            var originalRecord = new ArticleRecord
            {
                Id = Guid.NewGuid().ToString(),
                Titulo = "Complex Article",
                Autores = new List<string> { "A1", "A2", "A3" },
                Ano = 2023,
                Resumo = "Complex abstract with special characters: áéíóú",
                Pais = "Brazil",
                DataCriacao = DateTime.UtcNow,
                DataUltimaAtualizacao = DateTime.UtcNow,
                StatusSincronizacao = "pendente"
            };

            var settings = GetSerializerSettings();

            // Act - perform multiple serialization/deserialization cycles
            var record = originalRecord;
            for (int i = 0; i < 3; i++)
            {
                var json = JsonConvert.SerializeObject(record, settings);
                record = JsonConvert.DeserializeObject<ArticleRecord>(json, settings)!;
            }

            // Assert - data should be identical after multiple cycles
            Assert.Equal(originalRecord.Id, record.Id);
            Assert.Equal(originalRecord.Titulo, record.Titulo);
            Assert.Equal(3, record.Autores?.Count);
            Assert.Equal(originalRecord.Resumo, record.Resumo);
        }
    }
}
