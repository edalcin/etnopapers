/**
 * Language detection utility
 * Detects Portuguese, English, Spanish from text samples
 */

// Common Portuguese words
const PORTUGUESE_WORDS = new Set([
  'que', 'de', 'a', 'e', 'do', 'da', 'o', 'para', 'com', 'em', 'as', 'por',
  'os', 'um', 'uma', 'é', 'seu', 'esta', 'são', 'ele', 'tem', 'sido', 'isso',
  'foi', 'não', 'mais', 'também', 'assim', 'como', 'entre', 'quando', 'mesmo',
  'muito', 'tanto', 'enquanto', 'dentro', 'onde', 'qual', 'pode', 'deve',
  'fazer', 'conhecer', 'trabalho', 'comunidade', 'espécie', 'planta', 'uso',
  'tradicional', 'indígena', 'floresta', 'medicina', 'saúde', 'pesquisa',
  'estudar', 'etnobotânica', 'recurso', 'natural', 'local', 'região', 'povo',
  'língua', 'cultural', 'prático', 'importante', 'específico', 'realidade',
  'realizado', 'realizado', 'presente', 'existente', 'anterior', 'superior',
  'inferior', 'novo', 'velho', 'diferente', 'idêntico', 'semelhante', 'único',
  'duplo', 'múltiplo', 'completo', 'incompleto', 'inteiro', 'parcial',
]);

// Common English words
const ENGLISH_WORDS = new Set([
  'the', 'and', 'a', 'of', 'to', 'in', 'is', 'that', 'it', 'for', 'you',
  'all', 'not', 'but', 'can', 'her', 'was', 'one', 'our', 'out', 'day',
  'get', 'has', 'him', 'his', 'how', 'man', 'new', 'now', 'old', 'see',
  'two', 'way', 'who', 'boy', 'did', 'its', 'let', 'put', 'say', 'she',
  'too', 'use', 'been', 'call', 'come', 'find', 'give', 'good', 'hand',
  'have', 'here', 'home', 'just', 'know', 'like', 'make', 'made', 'many',
  'over', 'such', 'take', 'than', 'them', 'then', 'they', 'this', 'time',
  'very', 'want', 'well', 'were', 'what', 'when', 'with', 'word', 'work',
  'world', 'would', 'year', 'study', 'research', 'plant', 'species',
  'traditional', 'indigenous', 'community', 'forest', 'medicine', 'health',
  'ethnobotany', 'resource', 'natural', 'local', 'region', 'people',
  'language', 'cultural', 'practical', 'important', 'specific', 'reality',
  'present', 'existing', 'previous', 'upper', 'lower', 'different',
  'identical', 'similar', 'unique', 'multiple', 'complete', 'incomplete',
  'entire', 'partial',
]);

// Common Spanish words
const SPANISH_WORDS = new Set([
  'de', 'que', 'y', 'a', 'en', 'un', 'ser', 'se', 'no', 'haber', 'por',
  'con', 'su', 'para', 'es', 'una', 'o', 'este', 'sí', 'ya', 'o', 'este',
  'lo', 'como', 'más', 'o', 'fue', 'este', 'ha', 'sí', 'porque', 'esta',
  'son', 'entre', 'está', 'cuando', 'muy', 'sin', 'sobre', 'ser', 'tiene',
  'también', 'me', 'hasta', 'hay', 'donde', 'han', 'quien', 'están',
  'estado', 'desde', 'todo', 'nos', 'durante', 'estados', 'todos',
  'uno', 'les', 'ni', 'contra', 'otros', 'fueron', 'ese', 'eso', 'había',
  'ante', 'ellos', 'fue', 'esto', 'mí', 'antes', 'algunos', 'qué', 'unos',
  'yo', 'otro', 'otras', 'otra', 'él', 'tanto', 'esa', 'estos',
  'ecología', 'ecológico', 'plantas', 'especie', 'medicina', 'salud',
  'investigación', 'etnobotánica', 'recurso', 'natural', 'local', 'región',
  'pueblo', 'lengua', 'cultura', 'cultural', 'práctico', 'importante',
  'específico', 'realidad', 'realizado', 'presente', 'existente', 'anterior',
  'superior', 'inferior', 'nuevo', 'viejo', 'diferente', 'idéntico',
  'semejante', 'único', 'múltiple', 'completo', 'incompleto', 'entero',
  'parcial', 'tradicional', 'indígena', 'comunidad', 'bosque', 'selva',
]);

export interface LanguageDetectionResult {
  language: 'pt' | 'en' | 'es' | 'unknown';
  confidence: number; // 0-1
  scores: {
    portuguese: number;
    english: number;
    spanish: number;
  };
}

/**
 * Detects language from text sample
 * @param text Text sample to analyze
 * @returns Language detection result with confidence score
 */
export function detectLanguage(text: string): LanguageDetectionResult {
  if (!text || text.trim().length === 0) {
    return {
      language: 'unknown',
      confidence: 0,
      scores: { portuguese: 0, english: 0, spanish: 0 },
    };
  }

  // Normalize text to lowercase and split into words
  const words = text.toLowerCase().match(/\b\w+\b/g) || [];
  if (words.length === 0) {
    return {
      language: 'unknown',
      confidence: 0,
      scores: { portuguese: 0, english: 0, spanish: 0 },
    };
  }

  // Count matches for each language
  let ptCount = 0;
  let enCount = 0;
  let esCount = 0;

  for (const word of words) {
    if (PORTUGUESE_WORDS.has(word)) ptCount++;
    if (ENGLISH_WORDS.has(word)) enCount++;
    if (SPANISH_WORDS.has(word)) esCount++;
  }

  // Calculate confidence scores (percentage of words matched)
  const total = words.length;
  const scores = {
    portuguese: ptCount / total,
    english: enCount / total,
    spanish: esCount / total,
  };

  // Determine language based on highest score
  const max = Math.max(scores.portuguese, scores.english, scores.spanish);

  let language: 'pt' | 'en' | 'es' | 'unknown' = 'unknown';
  if (max > 0.01) {
    // At least 1% of words should match
    if (scores.portuguese === max) {
      language = 'pt';
    } else if (scores.english === max) {
      language = 'en';
    } else {
      language = 'es';
    }
  }

  return {
    language,
    confidence: max,
    scores,
  };
}

/**
 * Example usage
 */
export const languageDetectorExamples = {
  portuguese: 'Estudo etnobotânico das plantas utilizadas pela comunidade indígena do rio Negro',
  english: 'Ethnobotanical survey of medicinal plants used by traditional communities in the Amazon',
  spanish: 'Estudio etnobotánico de las plantas medicinales utilizadas por pueblos indígenas de la Amazonía',
};
