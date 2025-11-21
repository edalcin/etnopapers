import type { ExtractedMetadata } from '@types'

export const extractWithClaude = async (
  pdfText: string,
  apiKey: string
): Promise<ExtractedMetadata> => {
  // This is a stub implementation
  // In production, this would call the Claude API
  // For now, return a basic structure to allow the app to work
  return {
    titulo: 'Artigo Extraído',
    autores: [{ nome: 'Extraído', sobrenome: 'Por Claude' }],
    ano_publicacao: new Date().getFullYear(),
    resumo: 'Resumo extraído do PDF',
    doi: undefined,
  }
}
