using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace EtnoPapers.Core.Utils
{
    /// <summary>
    /// Normalizes article titles to proper case, preserving acronyms and particles.
    /// </summary>
    public static class TitleNormalizer
    {
        private static readonly HashSet<string> Particles = new(StringComparer.OrdinalIgnoreCase)
        {
            "de", "da", "do", "du", "des", "di", "del", "della", "von", "van", "der", "den",
            "the", "a", "an", "and", "or", "for", "in", "on", "at", "by", "to", "with"
        };

        private static readonly HashSet<string> CommonAcronyms = new(StringComparer.OrdinalIgnoreCase)
        {
            "DNA", "RNA", "ATP", "USA", "UK", "EU", "WHO", "UNESCO", "IUCN", "GPS",
            "PDF", "HTML", "XML", "JSON", "API", "HTTP", "HTTPS", "SQL"
        };

        /// <summary>
        /// Normalizes a title to proper case.
        /// </summary>
        public static string Normalize(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return title;

            var words = title.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            var normalized = new List<string>();

            for (int i = 0; i < words.Length; i++)
            {
                var word = words[i];

                // Skip empty words
                if (string.IsNullOrEmpty(word))
                    continue;

                // First word always capitalized
                if (i == 0)
                {
                    normalized.Add(CapitalizeWord(word));
                    continue;
                }

                // Known acronyms
                if (CommonAcronyms.Contains(word))
                {
                    normalized.Add(word.ToUpperInvariant());
                    continue;
                }

                // Particles (like "de", "von") - lowercase
                if (Particles.Contains(word))
                {
                    normalized.Add(word.ToLowerInvariant());
                    continue;
                }

                // Regular words - capitalize
                normalized.Add(CapitalizeWord(word));
            }

            return string.Join(" ", normalized);
        }

        private static string CapitalizeWord(string word)
        {
            if (string.IsNullOrEmpty(word))
                return word;

            // Handle hyphenated words
            if (word.Contains("-"))
            {
                var parts = word.Split('-');
                for (int i = 0; i < parts.Length; i++)
                {
                    if (!string.IsNullOrEmpty(parts[i]))
                        parts[i] = CapitalizeSingleWord(parts[i]);
                }
                return string.Join("-", parts);
            }

            return CapitalizeSingleWord(word);
        }

        private static string CapitalizeSingleWord(string word)
        {
            if (string.IsNullOrEmpty(word))
                return word;

            var textInfo = CultureInfo.CurrentCulture.TextInfo;
            return textInfo.ToTitleCase(word.ToLowerInvariant());
        }
    }
}
