using System;
using System.Collections.Generic;
using Xunit;
using EtnoPapers.Core.Models;
using EtnoPapers.Core.Validation;

namespace EtnoPapers.Core.Tests.Validation
{
    /// <summary>
    /// Unit tests for ArticleRecordValidator.
    /// Validates record validation logic, mandatory fields, constraints, and data types.
    /// </summary>
    public class ArticleRecordValidatorTests
    {
        private ArticleRecordValidator _validator = new();

        private ArticleRecord CreateValidRecord()
        {
            return new ArticleRecord
            {
                Titulo = "Test Article",
                Autores = new List<string> { "Author One", "Author Two" },
                Ano = 2023,
                Resumo = "This is a test abstract",
                Pais = "Brazil",
                DataCriacao = DateTime.UtcNow,
                DataUltimaAtualizacao = DateTime.UtcNow,
                StatusSincronizacao = "pendente"
            };
        }

        [Fact]
        public void Validate_ValidRecord_ReturnsTrue()
        {
            // Arrange
            var record = CreateValidRecord();

            // Act
            var result = _validator.Validate(record);

            // Assert
            Assert.True(result);
            Assert.Empty(_validator.ValidationErrors);
        }

        [Fact]
        public void Validate_NullRecord_ReturnsFalse()
        {
            // Arrange
            ArticleRecord? record = null;

            // Act
            var result = _validator.Validate(record!);

            // Assert
            Assert.False(result);
            Assert.Contains("Record cannot be null", _validator.ValidationErrors);
        }

        [Fact]
        public void Validate_MissingTitulo_ReturnsFalse()
        {
            // Arrange
            var record = CreateValidRecord();
            record.Titulo = null;

            // Act
            var result = _validator.Validate(record);

            // Assert
            Assert.False(result);
            Assert.Contains("Titulo", _validator.ValidationErrors[0]);
        }

        [Fact]
        public void Validate_MissingAutores_ReturnsFalse()
        {
            // Arrange
            var record = CreateValidRecord();
            record.Autores = new List<string>();

            // Act
            var result = _validator.Validate(record);

            // Assert
            Assert.False(result);
            Assert.Contains("Autores", _validator.ValidationErrors[0]);
        }

        [Fact]
        public void Validate_MissingAno_ReturnsFalse()
        {
            // Arrange
            var record = CreateValidRecord();
            record.Ano = null;

            // Act
            var result = _validator.Validate(record);

            // Assert
            Assert.False(result);
            Assert.Contains("Ano", _validator.ValidationErrors[0]);
        }

        [Fact]
        public void Validate_MissingResumo_ReturnsFalse()
        {
            // Arrange
            var record = CreateValidRecord();
            record.Resumo = "";

            // Act
            var result = _validator.Validate(record);

            // Assert
            Assert.False(result);
            Assert.Contains("Resumo", _validator.ValidationErrors[0]);
        }

        [Fact]
        public void Validate_YearTooOld_ReturnsFalse()
        {
            // Arrange
            var record = CreateValidRecord();
            record.Ano = 1400; // Before 1500

            // Act
            var result = _validator.Validate(record);

            // Assert
            Assert.False(result);
            Assert.Contains("Ano", _validator.ValidationErrors[0]);
        }

        [Fact]
        public void Validate_YearInFuture_ReturnsFalse()
        {
            // Arrange
            var record = CreateValidRecord();
            record.Ano = DateTime.Now.Year + 5; // Too far in future

            // Act
            var result = _validator.Validate(record);

            // Assert
            Assert.False(result);
            Assert.Contains("Ano", _validator.ValidationErrors[0]);
        }

        [Fact]
        public void Validate_TitleTooLong_ReturnsFalse()
        {
            // Arrange
            var record = CreateValidRecord();
            record.Titulo = new string('a', 501); // 501 characters

            // Act
            var result = _validator.Validate(record);

            // Assert
            Assert.False(result);
            Assert.Contains("Title is too long", _validator.ValidationErrors[0]);
        }

        [Fact]
        public void Validate_AbstractTooLong_ReturnsFalse()
        {
            // Arrange
            var record = CreateValidRecord();
            record.Resumo = new string('a', 10001); // 10001 characters

            // Act
            var result = _validator.Validate(record);

            // Assert
            Assert.False(result);
            Assert.Contains("Abstract is too long", _validator.ValidationErrors[0]);
        }

        [Fact]
        public void Validate_TooManyAuthors_ReturnsFalse()
        {
            // Arrange
            var record = CreateValidRecord();
            record.Autores = new List<string>();
            for (int i = 0; i < 101; i++)
            {
                record.Autores.Add($"Author {i}");
            }

            // Act
            var result = _validator.Validate(record);

            // Assert
            Assert.False(result);
            Assert.Contains("Too many authors", _validator.ValidationErrors[0]);
        }

        [Fact]
        public void Validate_EmptyAuthorName_ReturnsFalse()
        {
            // Arrange
            var record = CreateValidRecord();
            record.Autores = new List<string> { "Valid Author", "" };

            // Act
            var result = _validator.Validate(record);

            // Assert
            Assert.False(result);
            Assert.Contains("Author names cannot be empty", _validator.ValidationErrors[0]);
        }

        [Fact]
        public void Validate_SpeciesMissingVernacularName_ReturnsFalse()
        {
            // Arrange
            var record = CreateValidRecord();
            record.Especies = new List<PlantSpecies>
            {
                new PlantSpecies { NomeVernacular = "", NomeCientifico = "Genus species", TipoUso = "medicinal" }
            };

            // Act
            var result = _validator.Validate(record);

            // Assert
            Assert.False(result);
            Assert.Contains("vernacular name", _validator.ValidationErrors[0]);
        }

        [Fact]
        public void Validate_InvalidCollectionYear_ReturnsFalse()
        {
            // Arrange
            var record = CreateValidRecord();
            record.AnoColeta = 1400; // Before 1500

            // Act
            var result = _validator.Validate(record);

            // Assert
            Assert.False(result);
            Assert.Contains("collection year", _validator.ValidationErrors[0]);
        }

        [Fact]
        public void ValidateMandatoryFields_ValidRecord_ReturnsTrue()
        {
            // Arrange
            var record = CreateValidRecord();

            // Act
            var result = _validator.ValidateMandatoryFields(record);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsValidForSaving_ValidRecord_ReturnsTrue()
        {
            // Arrange
            var record = CreateValidRecord();

            // Act
            var result = _validator.IsValidForSaving(record);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsValidForSaving_NullRecord_ReturnsFalse()
        {
            // Arrange
            ArticleRecord? record = null;

            // Act
            var result = _validator.IsValidForSaving(record!);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsValidForSaving_MissingTitulo_ReturnsFalse()
        {
            // Arrange
            var record = CreateValidRecord();
            record.Titulo = "";

            // Act
            var result = _validator.IsValidForSaving(record);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsValidForSaving_NoAuthors_ReturnsFalse()
        {
            // Arrange
            var record = CreateValidRecord();
            record.Autores = new List<string>();

            // Act
            var result = _validator.IsValidForSaving(record);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsValidForSaving_InvalidYear_ReturnsFalse()
        {
            // Arrange
            var record = CreateValidRecord();
            record.Ano = 1400;

            // Act
            var result = _validator.IsValidForSaving(record);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsValidForSaving_NoResumo_ReturnsTrue()
        {
            // Arrange
            var record = CreateValidRecord();
            record.Resumo = ""; // Resumo is not required for saving

            // Act
            var result = _validator.IsValidForSaving(record);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void GetValidationErrorsAsString_MultipleErrors_ReturnsFormattedString()
        {
            // Arrange
            var record = CreateValidRecord();
            record.Titulo = "";
            record.Autores = new List<string>();
            _validator.Validate(record);

            // Act
            var result = _validator.GetValidationErrorsAsString();

            // Assert
            Assert.NotEmpty(result);
            Assert.Contains("\n", result);
            Assert.Contains("Titulo", result);
            Assert.Contains("Autores", result);
        }

        [Fact]
        public void Validate_MaximumLengthTitle_ReturnsTrue()
        {
            // Arrange
            var record = CreateValidRecord();
            record.Titulo = new string('a', 500); // Exactly 500 characters

            // Act
            var result = _validator.Validate(record);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Validate_MaximumAuthors_ReturnsTrue()
        {
            // Arrange
            var record = CreateValidRecord();
            record.Autores = new List<string>();
            for (int i = 0; i < 100; i++)
            {
                record.Autores.Add($"Author {i}");
            }

            // Act
            var result = _validator.Validate(record);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Validate_SpeciesWithValidData_ReturnsTrue()
        {
            // Arrange
            var record = CreateValidRecord();
            record.Especies = new List<PlantSpecies>
            {
                new PlantSpecies
                {
                    NomeVernacular = "Pau D'Arco",
                    NomeCientifico = "Tabebuia serratifolia",
                    TipoUso = "medicinal",
                    ParteUsada = "bark",
                    Preparacao = "decoction"
                }
            };

            // Act
            var result = _validator.Validate(record);

            // Assert
            Assert.True(result);
        }
    }
}
