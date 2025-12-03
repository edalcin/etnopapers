using System;
using System.Collections.Generic;
using System.Linq;
using EtnoPapers.Core.Models;

namespace EtnoPapers.Core.Services
{
    /// <summary>
    /// Detects potential duplicate records based on title, authors, and year.
    /// </summary>
    public class DuplicateDetectionService
    {
        private readonly DataStorageService _storageService;

        public DuplicateDetectionService(DataStorageService storageService)
        {
            _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
        }

        /// <summary>
        /// Finds potential duplicate records by comparing title, authors, and year.
        /// </summary>
        public List<ArticleRecord> FindPotentialDuplicates(ArticleRecord newRecord)
        {
            if (newRecord == null || string.IsNullOrEmpty(newRecord.Titulo))
                return new List<ArticleRecord>();

            var allRecords = _storageService.LoadAll();
            var duplicates = new List<ArticleRecord>();

            var newTitle = NormalizeString(newRecord.Titulo);

            foreach (var existing in allRecords)
            {
                var score = CalculateSimilarityScore(newRecord, existing);

                // Consider it a duplicate if similarity score is >= 0.7 (70%)
                if (score >= 0.7)
                {
                    duplicates.Add(existing);
                }
            }

            return duplicates;
        }

        /// <summary>
        /// Calculates a similarity score between two records (0.0 to 1.0).
        /// </summary>
        private double CalculateSimilarityScore(ArticleRecord record1, ArticleRecord record2)
        {
            double score = 0;
            int factors = 0;

            // Title similarity (highest weight)
            if (!string.IsNullOrEmpty(record1.Titulo) && !string.IsNullOrEmpty(record2.Titulo))
            {
                var titleSimilarity = CalculateStringSimilarity(record1.Titulo, record2.Titulo);
                score += titleSimilarity * 0.6; // 60% weight
                factors++;
            }

            // Year match (if both have years)
            if (record1.Ano.HasValue && record2.Ano.HasValue)
            {
                var yearMatch = record1.Ano == record2.Ano ? 1.0 : 0.0;
                score += yearMatch * 0.2; // 20% weight
                factors++;
            }

            // Author match (at least one author in common)
            if (record1.Autores?.Count > 0 && record2.Autores?.Count > 0)
            {
                var authorMatch = HasCommonAuthors(record1.Autores, record2.Autores) ? 1.0 : 0.0;
                score += authorMatch * 0.2; // 20% weight
                factors++;
            }

            return factors > 0 ? score / factors : 0;
        }

        /// <summary>
        /// Calculates string similarity using Levenshtein distance.
        /// Returns a value between 0 (completely different) and 1 (identical).
        /// </summary>
        private double CalculateStringSimilarity(string s1, string s2)
        {
            var normalized1 = NormalizeString(s1);
            var normalized2 = NormalizeString(s2);

            int distance = LevenshteinDistance(normalized1, normalized2);
            int maxLength = Math.Max(normalized1.Length, normalized2.Length);

            if (maxLength == 0)
                return 1.0; // Both empty strings are identical

            return 1.0 - (distance / (double)maxLength);
        }

        /// <summary>
        /// Calculates Levenshtein distance between two strings.
        /// </summary>
        private int LevenshteinDistance(string s1, string s2)
        {
            int len1 = s1.Length;
            int len2 = s2.Length;
            var d = new int[len1 + 1, len2 + 1];

            for (int i = 0; i <= len1; i++)
                d[i, 0] = i;

            for (int j = 0; j <= len2; j++)
                d[0, j] = j;

            for (int i = 1; i <= len1; i++)
            {
                for (int j = 1; j <= len2; j++)
                {
                    int cost = s1[i - 1] == s2[j - 1] ? 0 : 1;

                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }

            return d[len1, len2];
        }

        /// <summary>
        /// Normalizes a string for comparison (lowercase, trim, remove extra spaces).
        /// </summary>
        private string NormalizeString(string s)
        {
            if (string.IsNullOrEmpty(s))
                return "";

            return System.Text.RegularExpressions.Regex.Replace(
                s.ToLowerInvariant().Trim(),
                @"\s+",
                " ");
        }

        /// <summary>
        /// Checks if two author lists have common authors (fuzzy match).
        /// </summary>
        private bool HasCommonAuthors(List<string> authors1, List<string> authors2)
        {
            foreach (var author1 in authors1)
            {
                foreach (var author2 in authors2)
                {
                    var similarity = CalculateStringSimilarity(author1, author2);
                    if (similarity >= 0.8) // 80% similarity for author names
                        return true;
                }
            }

            return false;
        }
    }
}
