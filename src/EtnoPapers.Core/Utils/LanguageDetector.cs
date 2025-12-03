using System;
using System.Collections.Generic;
using System.Linq;

namespace EtnoPapers.Core.Utils
{
    /// <summary>
    /// Detects language of text (Portuguese, English, Spanish).
    /// Uses simple keyword detection as heuristic.
    /// </summary>
    public static class LanguageDetector
    {
        private static readonly HashSet<string> PortugueseWords = new(StringComparer.OrdinalIgnoreCase)
        {
            "o", "a", "de", "para", "por", "com", "em", "é", "que", "e",
            "não", "se", "da", "do", "ou", "como", "mas", "onde", "quando",
            "qual", "quem", "sobre", "durante", "entre", "antes", "depois",
            "será", "está", "são", "foram", "sido", "foi", "ser", "há",
            "tem", "tinha", "tinha", "temos", "tendo", "tenho", "tive"
        };

        private static readonly HashSet<string> EnglishWords = new(StringComparer.OrdinalIgnoreCase)
        {
            "the", "a", "of", "and", "to", "in", "is", "that", "it",
            "for", "was", "are", "been", "be", "have", "has", "had",
            "do", "does", "did", "on", "at", "by", "this", "but",
            "or", "with", "as", "from", "can", "could", "would", "will"
        };

        private static readonly HashSet<string> SpanishWords = new(StringComparer.OrdinalIgnoreCase)
        {
            "el", "la", "de", "que", "y", "a", "en", "un", "ser", "se",
            "no", "haber", "por", "con", "su", "para", "es", "una", "o",
            "como", "más", "hacer", "o", "estar", "tener", "le", "lo",
            "todo", "pero", "más", "haciendo", "porque", "está", "sí"
        };

        /// <summary>
        /// Detects the language of the given text.
        /// Returns "pt", "en", or "es" as language code.
        /// Defaults to "pt" (Portuguese) if ambiguous.
        /// </summary>
        public static string DetectLanguage(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "pt"; // Default to Portuguese

            var words = text.ToLowerInvariant()
                .Split(new[] { ' ', '\t', '\n', '\r', '.', ',', ';', ':', '!', '?' },
                    StringSplitOptions.RemoveEmptyEntries)
                .Where(w => w.Length > 2) // Only consider words with 3+ characters
                .ToList();

            if (words.Count == 0)
                return "pt";

            var ptScore = words.Count(w => PortugueseWords.Contains(w));
            var enScore = words.Count(w => EnglishWords.Contains(w));
            var esScore = words.Count(w => SpanishWords.Contains(w));

            if (ptScore >= enScore && ptScore >= esScore && ptScore > 0)
                return "pt";

            if (enScore >= esScore && enScore > 0)
                return "en";

            if (esScore > 0)
                return "es";

            // Default to Portuguese if no clear match
            return "pt";
        }

        /// <summary>
        /// Gets the language name from language code.
        /// </summary>
        public static string GetLanguageName(string languageCode)
        {
            return languageCode?.ToLowerInvariant() switch
            {
                "pt" => "Portuguese",
                "en" => "English",
                "es" => "Spanish",
                _ => "Unknown"
            };
        }
    }
}
