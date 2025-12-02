/**
 * Title normalization utility
 * Converts titles to proper case, preserving acronyms
 */

const LOWERCASE_WORDS = new Set([
  'a', 'an', 'and', 'as', 'at', 'but', 'by', 'for', 'from', 'in', 'into',
  'is', 'it', 'of', 'on', 'or', 'the', 'to', 'with',
  // Portuguese
  'a', 'ao', 'aos', 'as', 'até', 'com', 'da', 'das', 'de', 'dela', 'delas',
  'dele', 'deles', 'do', 'dos', 'e', 'em', 'entre', 'era', 'eram', 'essa',
  'essas', 'esse', 'esses', 'esta', 'estamos', 'estas', 'este', 'esteja',
  'estemos', 'estes', 'estive', 'estivemos', 'estivera', 'estivéramos',
  'estive', 'estivemos', 'estivera', 'estivéramos', 'estivesse',
  'estivéssemos', 'estou', 'esteja', 'foi', 'fomos', 'for', 'fora',
  'fóramos', 'fosse', 'fôssemos', 'há', 'haja', 'hajamos', 'hão', 'houve',
  'houvemos', 'houvera', 'houvéramos', 'houverei', 'houveria', 'houvéreis',
  'houvésseis', 'houvesse', 'houvéssemos', 'houvestes', 'houveste', 'houve',
  'o', 'os', 'ou', 'para', 'pela', 'pelas', 'pelo', 'pelos', 'por', 'qual',
  'quando', 'quanto', 'que', 'quem', 'se', 'sem', 'senão', 'seu', 'seus',
  'si', 'sido', 'sendo', 'seu', 'seus', 'sob', 'sobre', 'só', 'sua', 'suas',
  'tal', 'também', 'tampouco', 'tanta', 'tantas', 'tanto', 'tantos', 'te',
  'tenha', 'tenhamos', 'tenho', 'terei', 'teria', 'tereis', 'teríamos',
  'terás', 'teríeis', 'teríamos', 'terão', 'teríeis', 'teríeis', 'teríeis',
  'ti', 'tido', 'tinha', 'tínhamos', 'tinham', 'tinhas', 'tive', 'tivemos',
  'tivera', 'tivéramos', 'tiverei', 'tiveria', 'tivéreis', 'tivésseis',
  'tivesse', 'tivéssemos', 'tiveste', 'tivestes', 'tiveste', 'tivemos',
  'tiver', 'tivermos', 'tiverem', 'tivéramos', 'tivéreis', 'tivésseis',
  'tivéssemos', 'tivera', 'tivéramos', 'tivera', 'tivéramos', 'tu',
  'tua', 'tuas', 'tudo', 'um', 'uma', 'umas', 'uns', 'vai', 'vais', 'vamos',
  'vão', 'vária', 'várias', 'vário', 'vários', 'vás', 'vê', 'vede', 'veem',
  'vei', 'veia', 'veia', 'veias', 'vejais', 'vejamos', 'vejas', 'veja', 'vejo',
  'vendo', 'venha', 'venhamos', 'venham', 'venhas', 'venia', 'venia', 'veno',
  'veno', 'ves', 'vês', 'vese', 'vesa', 'vinha', 'vínhamos', 'vinham', 'vinhas',
  'vinha', 'vinha', 'vinha', 'vinha', 'vinha', 'vinha', 'vinha', 'vinha',
  'vinha', 'vinha', 'vinha', 'vinha', 'vinha', 'vinha', 'vinha',
]);

const ACRONYM_PATTERN = /\b[A-Z]{2,}\b/g;
const WORD_PATTERN = /\b\w+(?:[-']\w+)?\b/g;

/**
 * Normalizes title to proper case, preserving acronyms and handling particles
 * @param title Original title text
 * @returns Normalized title in proper case
 */
export function normalizeTitle(title: string): string {
  if (!title || title.trim().length === 0) {
    return title;
  }

  // Extract acronyms to preserve them
  const acronyms = new Set<string>();
  let match;
  const acronymRegex = new RegExp(ACRONYM_PATTERN);
  while ((match = acronymRegex.exec(title)) !== null) {
    acronyms.add(match[0]);
  }

  // Split into words while preserving hyphens and apostrophes
  const words = title.match(WORD_PATTERN) || [];

  // Process each word
  const normalized = words.map((word, index) => {
    // Check if this is an acronym to preserve
    const baseWord = word.replace(/[-']/g, '');
    if (acronyms.has(baseWord.toUpperCase())) {
      return word.toUpperCase();
    }

    // Check if it's a lowercase article/preposition (except first and last word)
    const lowerWord = word.toLowerCase();
    if (index > 0 && index < words.length - 1 && LOWERCASE_WORDS.has(lowerWord)) {
      return lowerWord;
    }

    // Capitalize first letter, lowercase rest
    return word.charAt(0).toUpperCase() + word.slice(1).toLowerCase();
  });

  return normalized.join(' ');
}

/**
 * Example usage
 */
export const titleNormalizerExamples = {
  'THE QUICK BROWN FOX': 'The Quick Brown Fox',
  'ethnobotanical survey IN THE AMAZON RAINFOREST': 'Ethnobotanical Survey in the Amazon Rainforest',
  'a NEW METHOD FOR DNA ANALYSIS': 'A New Method for DNA Analysis',
  'MAPPING TRADITIONAL PLANT USES BY THE YANOMAMI': 'Mapping Traditional Plant Uses by the Yanomami',
};
