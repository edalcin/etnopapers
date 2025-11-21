import type { ExtractedMetadata } from '@types'

export const extractWithOpenAI = async (
  pdfText: string,
  apiKey: string
): Promise<ExtractedMetadata> => {
  // This is a stub implementation
  // In production, this would call the OpenAI API
  // For now, return a basic structure to allow the app to work
  return {
    titulo: 'Artigo Extraído',
    autores: [{ nome: 'Extraído', sobrenome: 'Por OpenAI' }],
    ano_publicacao: new Date().getFullYear(),
    resumo: 'Resumo extraído do PDF',
    doi: undefined,
  }
}
