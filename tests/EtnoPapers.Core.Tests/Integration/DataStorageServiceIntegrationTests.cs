using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using EtnoPapers.Core.Models;
using EtnoPapers.Core.Services;

namespace EtnoPapers.Core.Tests.Integration
{
    /// <summary>
    /// Integration tests for DataStorageService.
    /// Tests CRUD operations and persistence with actual storage.
    /// Note: Tests use actual file storage in Documents/EtnoPapers/ directory.
    /// </summary>
    public class DataStorageServiceIntegrationTests
    {
        private readonly DataStorageService _storageService;

        public DataStorageServiceIntegrationTests()
        {
            _storageService = new DataStorageService();
            _storageService.Initialize();
        }

        private ArticleRecord CreateTestRecord(string id = null, string title = "Test Article")
        {
            return new ArticleRecord
            {
                Id = id ?? Guid.NewGuid().ToString(),
                Titulo = title,
                Autores = new List<string> { "Test Author" },
                Ano = 2023,
                Resumo = "Test abstract",
                Pais = "Brazil",
                DataCriacao = DateTime.UtcNow,
                DataUltimaAtualizacao = DateTime.UtcNow,
                StatusSincronizacao = "pendente"
            };
        }

        [Fact]
        public void Create_ValidRecord_PersistsAndCanBeRetrieved()
        {
            // Arrange
            var record = CreateTestRecord("test-create-" + Guid.NewGuid(), "Create Test");

            // Act
            var created = _storageService.Create(record);
            var loadedRecord = _storageService.GetById(record.Id);

            // Assert
            Assert.True(created);
            Assert.NotNull(loadedRecord);
            Assert.Equal(record.Id, loadedRecord!.Id);
            Assert.Equal(record.Titulo, loadedRecord.Titulo);
            Assert.Equal(record.Ano, loadedRecord.Ano);
        }

        [Fact]
        public void GetById_NonexistentId_ReturnsNull()
        {
            // Act
            var record = _storageService.GetById("nonexistent-id-" + Guid.NewGuid());

            // Assert
            Assert.Null(record);
        }

        [Fact]
        public void Update_ExistingRecord_ModifiesPersistedData()
        {
            // Arrange
            var id = "test-update-" + Guid.NewGuid();
            var originalRecord = CreateTestRecord(id, "Original Title");
            _storageService.Create(originalRecord);

            // Act
            originalRecord.Titulo = "Updated Title";
            originalRecord.Resumo = "Updated abstract";
            var updated = _storageService.Update(originalRecord);
            var loadedRecord = _storageService.GetById(id);

            // Assert
            Assert.True(updated);
            Assert.NotNull(loadedRecord);
            Assert.Equal("Updated Title", loadedRecord!.Titulo);
            Assert.Equal("Updated abstract", loadedRecord.Resumo);
        }

        [Fact]
        public void Delete_ExistingRecord_RemovesFromStorage()
        {
            // Arrange
            var id = "test-delete-" + Guid.NewGuid();
            var record = CreateTestRecord(id, "Record to Delete");
            _storageService.Create(record);

            // Act
            var deleted = _storageService.Delete(id);
            var loadedRecord = _storageService.GetById(id);

            // Assert
            Assert.True(deleted);
            Assert.Null(loadedRecord);
        }

        [Fact]
        public void LoadAll_ReturnsListOfRecords()
        {
            // Act
            var records = _storageService.LoadAll();

            // Assert
            Assert.IsType<List<ArticleRecord>>(records);
            Assert.NotNull(records);
        }

        [Fact]
        public void Create_WithSpecies_PersistsSpeciesData()
        {
            // Arrange
            var id = "test-species-" + Guid.NewGuid();
            var record = CreateTestRecord(id, "Plants Study");
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
            var created = _storageService.Create(record);
            var loadedRecord = _storageService.GetById(id);

            // Assert
            Assert.True(created);
            Assert.NotNull(loadedRecord);
            Assert.NotNull(loadedRecord!.Especies);
            Assert.Single(loadedRecord.Especies);
            Assert.Equal("Pau D'Arco", loadedRecord.Especies[0].NomeVernacular);
            Assert.Equal("medicinal", loadedRecord.Especies[0].TipoUso);
        }

        [Fact]
        public void Create_MultipleRecords_PersistsAll()
        {
            // Arrange
            var ids = new[] { Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString() };
            var records = ids.Select((id, i) => CreateTestRecord(id, $"Article {i}")).ToList();

            // Act
            var allCreated = records.All(r => _storageService.Create(r));
            var allRecords = _storageService.LoadAll();

            // Assert
            Assert.True(allCreated);
            Assert.True(ids.All(id => allRecords.Any(r => r.Id == id)));
        }

        [Fact]
        public void Create_RecordWithComplexData_PreservesAllFields()
        {
            // Arrange
            var id = "test-complex-" + Guid.NewGuid();
            var record = new ArticleRecord
            {
                Id = id,
                Titulo = "Complex Data Record",
                Autores = new List<string> { "Author A", "Author B" },
                Ano = 2023,
                Resumo = "Abstract with special chars: áéíóú",
                Pais = "Brazil",
                Estado = "São Paulo",
                Municipio = "São Paulo",
                Bioma = "Atlantic Forest",
                DataCriacao = DateTime.UtcNow,
                DataUltimaAtualizacao = DateTime.UtcNow,
                StatusSincronizacao = "pendente"
            };

            // Act
            var created = _storageService.Create(record);
            var loadedRecord = _storageService.GetById(id);

            // Assert
            Assert.True(created);
            Assert.NotNull(loadedRecord);
            Assert.Equal(record.Titulo, loadedRecord!.Titulo);
            Assert.Equal(2, loadedRecord.Autores?.Count);
            Assert.Equal("Atlantic Forest", loadedRecord.Bioma);
            Assert.Equal("São Paulo", loadedRecord.Estado);
        }

        [Fact]
        public void Update_NonexistentRecord_ReturnsFalse()
        {
            // Arrange
            var record = CreateTestRecord("nonexistent-" + Guid.NewGuid());

            // Act
            var updated = _storageService.Update(record);

            // Assert
            Assert.False(updated);
        }

        [Fact]
        public void Delete_NonexistentRecord_ReturnsFalse()
        {
            // Act
            var deleted = _storageService.Delete("nonexistent-" + Guid.NewGuid());

            // Assert
            Assert.False(deleted);
        }
    }
}
