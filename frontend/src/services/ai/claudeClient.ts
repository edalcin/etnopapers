import type { ExtractedMetadata } from '@types'

export const extractWithClaude = async (
  pdfText: string,
  apiKey: string,
  instructions?: string
): Promise<ExtractedMetadata> => {
  // This is a stub implementation
  // In production, this would call the Claude API with the provided instructions
  //
  // The instructions parameter allows users to customize the extraction prompt.
  // In a real implementation, you would:
  // 1. Use the instructions as a system prompt
  // 2. Send the pdfText as user content
  // 3. Request JSON-structured response matching ExtractedMetadata interface
  //
  // Example API call structure:
  // const response = await fetch('https://api.anthropic.com/v1/messages', {
  //   method: 'POST',
  //   headers: {
  //     'x-api-key': apiKey,
  //     'anthropic-version': '2023-06-01',
  //     'Content-Type': 'application/json'
  //   },
  //   body: JSON.stringify({
  //     model: 'claude-3-haiku-20240307',
  //     max_tokens: 1024,
  //     system: instructions,
  //     messages: [{
  //       role: 'user',
  //       content: `Extract metadata from this article:\n\n${pdfText}\n\nReturn JSON with: titulo, autores, ano_publicacao, resumo, doi`
  //     }]
  //   })
  // })

  console.log('Extracting with Claude API')
  console.log('Using custom instructions:', !!instructions)

  return {
    titulo: 'Artigo Extraído',
    autores: [{ nome: 'Extraído', sobrenome: 'Por Claude' }],
    ano_publicacao: new Date().getFullYear(),
    resumo: 'Resumo extraído do PDF',
    doi: undefined,
  }
}
