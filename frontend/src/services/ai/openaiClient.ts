import type { ExtractedMetadata } from '@types'

export const extractWithOpenAI = async (
  pdfText: string,
  apiKey: string,
  instructions?: string
): Promise<ExtractedMetadata> => {
  // This is a stub implementation
  // In production, this would call the OpenAI API with the provided instructions
  //
  // The instructions parameter allows users to customize the extraction prompt.
  // In a real implementation, you would:
  // 1. Use the instructions as a system message
  // 2. Send the pdfText with extraction request
  // 3. Request JSON-structured response matching ExtractedMetadata interface
  //
  // Example API call structure:
  // const response = await fetch('https://api.openai.com/v1/chat/completions', {
  //   method: 'POST',
  //   headers: {
  //     'Authorization': `Bearer ${apiKey}`,
  //     'Content-Type': 'application/json'
  //   },
  //   body: JSON.stringify({
  //     model: 'gpt-3.5-turbo',
  //     messages: [
  //       { role: 'system', content: instructions },
  //       { role: 'user', content: `Extract metadata from this article:\n\n${pdfText}\n\nReturn JSON with: titulo, autores, ano_publicacao, resumo, doi` }
  //     ],
  //     response_format: { type: 'json_object' }
  //   })
  // })

  console.log('Extracting with OpenAI ChatGPT')
  console.log('Using custom instructions:', !!instructions)

  return {
    titulo: 'Artigo Extraído',
    autores: [{ nome: 'Extraído', sobrenome: 'Por OpenAI' }],
    ano_publicacao: new Date().getFullYear(),
    resumo: 'Resumo extraído do PDF',
    doi: undefined,
  }
}
