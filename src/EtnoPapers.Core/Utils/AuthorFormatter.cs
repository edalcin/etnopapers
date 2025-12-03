using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace EtnoPapers.Core.Utils
{
    /// <summary>
    /// Formats author names to APA style (LastName, F.I.).
    /// </summary>
    public static class AuthorFormatter
    {
        private static readonly HashSet<string> Particles = new(StringComparer.OrdinalIgnoreCase)
        {
            "de", "da", "do", "du", "von", "van", "der", "den", "di", "del"
        };

        /// <summary>
        /// Formats author names to APA style.
        /// Examples:
        /// "John Smith" -> "Smith, J."
        /// "John Q. Smith" -> "Smith, J. Q."
        /// "Jean-Pierre Dupont" -> "Dupont, J.-P."
        /// </summary>
        public static string FormatToAPA(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return fullName;

            var parts = fullName.Trim().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 0)
                return fullName;

            if (parts.Length == 1)
                return parts[0];

            // Try to identify last name considering particles
            var lastNameParts = new List<string>();
            var firstNames = new List<string>();
            var foundLastName = false;

            for (int i = parts.Length - 1; i >= 0; i--)
            {
                var part = parts[i];

                // Check if this part starts with particle or follows particle
                if (!foundLastName && !Particles.Contains(part))
                {
                    lastNameParts.Insert(0, part);
                    foundLastName = true;
                }
                else if (foundLastName && Particles.Contains(part))
                {
                    lastNameParts.Insert(0, part);
                }
                else if (!foundLastName)
                {
                    // This is a particle at the end, likely part of the name
                    lastNameParts.Insert(0, part);
                }
                else
                {
                    // These are first names
                    firstNames.Insert(0, part);
                }
            }

            if (lastNameParts.Count == 0)
            {
                lastNameParts.Add(parts[^1]);
                firstNames = parts.Take(parts.Length - 1).ToList();
            }

            var lastName = string.Join(" ", lastNameParts);
            var initials = ExtractInitials(string.Join(" ", firstNames));

            if (string.IsNullOrEmpty(initials))
                return lastName;

            return $"{lastName}, {initials}";
        }

        private static string ExtractInitials(string names)
        {
            if (string.IsNullOrWhiteSpace(names))
                return "";

            var parts = names.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            var initials = new List<string>();

            foreach (var part in parts)
            {
                if (string.IsNullOrEmpty(part))
                    continue;

                // Handle hyphenated first names like "Jean-Pierre"
                if (part.Contains("-"))
                {
                    var hyphenParts = part.Split('-');
                    foreach (var hp in hyphenParts)
                    {
                        if (!string.IsNullOrEmpty(hp))
                        {
                            initials.Add(hp[0].ToString().ToUpperInvariant() + ".");
                        }
                    }
                }
                else
                {
                    // Handle abbreviations that already have a period
                    if (part.EndsWith("."))
                    {
                        initials.Add(part);
                    }
                    else
                    {
                        initials.Add(part[0].ToString().ToUpperInvariant() + ".");
                    }
                }
            }

            return initials.Count > 0 ? string.Join(" ", initials) : "";
        }
    }
}
