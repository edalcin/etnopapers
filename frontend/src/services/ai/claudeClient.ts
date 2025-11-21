import { ExtractedMetadata } from '@types'

export const claudeClient = {
  extractMetadata: async (
    pdfText: string,
    apiKey: string
  ): Promise<ExtractedMetadata> => {
    throw new Error('Claude client not yet implemented')
  },
}
