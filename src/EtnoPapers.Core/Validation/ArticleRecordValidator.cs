using System;
using System.Collections.Generic;
using EtnoPapers.Core.Models;

namespace EtnoPapers.Core.Validation
{
    /// <summary>
    /// Validates ArticleRecord against required schema and constraints.
    /// </summary>
    public class ArticleRecordValidator
    {
        public List<string> ValidationErrors { get; private set; } = new();

        /// <summary>
        /// Validates a complete ArticleRecord.
        /// </summary>
        public bool Validate(ArticleRecord record)
        {
            ValidationErrors.Clear();

            if (record == null)
            {
                ValidationErrors.Add("Record cannot be null");
                return false;
            }

            ValidateMandatoryFields(record);
            ValidateDataTypes(record);
            ValidateConstraints(record);

            return ValidationErrors.Count == 0;
        }

        /// <summary>
        /// Validates that all mandatory fields are present and non-empty.
        /// Mandatory fields: titulo, autores, ano, resumo
        /// Allows partial records with warnings for manual editing.
        /// </summary>
        public bool ValidateMandatoryFields(ArticleRecord record)
        {
            bool hasMissingFields = false;

            if (string.IsNullOrWhiteSpace(record.Titulo))
            {
                ValidationErrors.Add("⚠ Titulo (title) is empty - requires manual entry");
                hasMissingFields = true;
            }

            if (record.Autores == null || record.Autores.Count == 0)
            {
                ValidationErrors.Add("⚠ Autores (authors) is empty - requires at least one author");
                hasMissingFields = true;
            }

            if (!record.Ano.HasValue || record.Ano < 1500 || record.Ano > DateTime.Now.Year + 1)
            {
                ValidationErrors.Add($"⚠ Ano (year) invalid - must be between 1500 and {DateTime.Now.Year + 1}");
                hasMissingFields = true;
            }

            if (string.IsNullOrWhiteSpace(record.Resumo))
            {
                ValidationErrors.Add("⚠ Resumo (abstract) is empty - requires manual entry");
                hasMissingFields = true;
            }

            // Mark as partial record if missing fields but some data was extracted
            if (hasMissingFields && !string.IsNullOrWhiteSpace(record.Titulo))
                ValidationErrors.Insert(0, "ℹ This is a partial record - please complete the missing required fields");

            return !hasMissingFields; // Return false only if missing mandatory fields
        }

        /// <summary>
        /// Validates data types and formats.
        /// </summary>
        private void ValidateDataTypes(ArticleRecord record)
        {
            // Verify authors are strings
            if (record.Autores != null)
            {
                foreach (var author in record.Autores)
                {
                    if (string.IsNullOrWhiteSpace(author))
                        ValidationErrors.Add("Author names cannot be empty strings");
                }
            }

            // Verify communities have required fields if present
            if (record.Comunidades != null)
            {
                foreach (var comunidade in record.Comunidades)
                {
                    if (comunidade == null)
                    {
                        ValidationErrors.Add("Community entry cannot be null");
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(comunidade.Nome))
                        ValidationErrors.Add("Each community must have a name (nome)");

                    // Verify plants have required fields if present
                    if (comunidade.Plantas != null)
                    {
                        foreach (var planta in comunidade.Plantas)
                        {
                            if (planta == null)
                            {
                                ValidationErrors.Add("Plant entry cannot be null");
                                continue;
                            }

                            if (planta.NomeVernacular == null || planta.NomeVernacular.Count == 0 ||
                                planta.NomeVernacular.TrueForAll(n => string.IsNullOrWhiteSpace(n)))
                                ValidationErrors.Add("Each plant must have at least one vernacular name (nomeVernacular)");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Validates value constraints and business rules.
        /// </summary>
        private void ValidateConstraints(ArticleRecord record)
        {
            // Title length
            if (!string.IsNullOrEmpty(record.Titulo) && record.Titulo.Length > 500)
                ValidationErrors.Add("Title is too long (max 500 characters)");

            // Authors count
            if (record.Autores?.Count > 100)
                ValidationErrors.Add("Too many authors (max 100)");

            // Resumo (abstract) should typically be present and reasonably sized
            if (!string.IsNullOrEmpty(record.Resumo) && record.Resumo.Length > 10000)
                ValidationErrors.Add("Abstract is too long (max 10000 characters)");

            // Year should be reasonable
            if (record.AnoColeta.HasValue &&
                (record.AnoColeta < 1500 || record.AnoColeta > DateTime.Now.Year + 1))
                ValidationErrors.Add("Ano coleta (collection year) must be valid");
        }

        /// <summary>
        /// Gets all validation errors as a formatted string.
        /// </summary>
        public string GetValidationErrorsAsString()
        {
            return string.Join("\n", ValidationErrors);
        }

        /// <summary>
        /// Checks if record is valid for saving.
        /// More lenient than full validation - only requires truly mandatory fields.
        /// </summary>
        public bool IsValidForSaving(ArticleRecord record)
        {
            if (record == null)
                return false;

            // Minimal validation for saving
            return !string.IsNullOrWhiteSpace(record.Titulo) &&
                   record.Autores != null && record.Autores.Count > 0 &&
                   record.Ano.HasValue && record.Ano >= 1500;
        }
    }
}
