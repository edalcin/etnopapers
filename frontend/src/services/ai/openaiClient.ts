import { ExtractedMetadata } from '@types'

export const openaiClient = {
  extractMetadata: async (
    pdfText: string,
    apiKey: string
  ): Promise<ExtractedMetadata> => {
    throw new Error('OpenAI client not yet implemented')
  },
}
