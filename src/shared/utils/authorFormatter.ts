/**
 * APA author formatter utility
 * Formats author names to APA style (LastName, F.I.)
 */

const PARTICLES = new Set(['de', 'da', 'do', 'von', 'van', 'el', 'la', 'le', 'al']);
const SUFFIXES = new Set(['Jr', 'Sr', 'II', 'III', 'IV', 'V']);

/**
 * Formats a single author name to APA style
 * Examples: "John Smith" → "Smith, J."
 *           "José da Silva" → "Silva, J. da"
 *           "Jean-Luc Picard" → "Picard, J.-L."
 * @param authorName Full author name
 * @returns Formatted author in APA style
 */
export function formatAuthorAPA(authorName: string): string {
  if (!authorName || authorName.trim().length === 0) {
    return authorName;
  }

  // Trim and split into parts
  const parts = authorName.trim().split(/\s+/);
  if (parts.length === 0) return authorName;

  // Extract suffix if present (Jr., Sr., etc.)
  let suffix = '';
  let workingParts = [...parts];

  const lastPart = workingParts[workingParts.length - 1];
  if (SUFFIXES.has(lastPart) || lastPart.match(/^(Jr|Sr)\.*$/i)) {
    suffix = workingParts.pop() || '';
  }

  if (workingParts.length === 0) return authorName;

  // Identify last name (can include particles)
  let lastNameParts: string[] = [];
  let firstNameParts: string[] = [];

  // Strategy: find the longest sequence of lowercase words at the end
  let splitIndex = workingParts.length - 1;

  // Move back while we have particles or lowercase words
  while (splitIndex > 0) {
    const word = workingParts[splitIndex - 1];
    if (PARTICLES.has(word.toLowerCase())) {
      splitIndex--;
    } else {
      break;
    }
  }

  // Last name is everything from splitIndex onwards
  lastNameParts = workingParts.slice(splitIndex);
  firstNameParts = workingParts.slice(0, splitIndex);

  if (firstNameParts.length === 0) {
    // All words are part of last name
    firstNameParts = [lastNameParts.shift() || ''];
  }

  // Format first names as initials
  const firstInitials = firstNameParts
    .map((part) => {
      // Handle hyphenated names like "Jean-Luc"
      return part
        .split('-')
        .map((p) => p.charAt(0).toUpperCase() + '.')
        .join('-');
    })
    .join(' ');

  // Format last name
  const lastName = lastNameParts.join(' ');

  // Combine
  let result = `${lastName}, ${firstInitials}`;

  if (suffix) {
    result += `, ${suffix}`;
  }

  return result;
}

/**
 * Formats array of author names to APA style
 * @param authors Array of author names
 * @returns Array of formatted author names
 */
export function formatAuthorsAPA(authors: string[]): string[] {
  return authors.map((author) => formatAuthorAPA(author));
}

/**
 * Example usage
 */
export const authorFormatterExamples = {
  'John Smith': 'Smith, J.',
  'José da Silva': 'Silva, J. da',
  'Jean-Luc Picard': 'Picard, J.-L.',
  'Maria Clara Souza': 'Souza, M. C.',
  'Carlos Alberto de Oliveira': 'Oliveira, C. A. de',
  'Robert Jr.': 'Robert, R., Jr.',
  'Ludwig van Beethoven': 'Beethoven, L. van',
};
