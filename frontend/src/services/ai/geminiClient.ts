import { ExtractedMetadata } from '@types'

export const geminiClient = {
  extractMetadata: async (
    pdfText: string,
    apiKey: string
  ): Promise<ExtractedMetadata> => {
    throw new Error('Gemini client not yet implemented')
  },
}
