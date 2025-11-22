import type { ExtractedMetadata } from '@types'

export const extractWithGemini = async (
  pdfText: string,
  apiKey: string,
  instructions?: string
): Promise<ExtractedMetadata> => {
  // This is a stub implementation
  // In production, this would call the Gemini API with the provided instructions
  //
  // The instructions parameter allows users to customize the extraction prompt.
  // In a real implementation, you would:
  // 1. Use the instructions as a system prompt
  // 2. Send the pdfText as user content
  // 3. Request JSON-structured response matching ExtractedMetadata interface
  //
  // Example API call structure:
  // const response = await fetch('https://generativelanguage.googleapis.com/v1/models/gemini-pro:generateContent', {
  //   method: 'POST',
  //   headers: { 'Content-Type': 'application/json' },
  //   body: JSON.stringify({
  //     systemInstruction: { parts: [{ text: instructions }] },
  //     contents: [{
  //       parts: [{
  //         text: `Extract metadata from this article text:\n\n${pdfText}\n\nReturn a JSON object with: titulo, autores (array of {nome, sobrenome}), ano_publicacao, resumo, doi`
  //       }]
  //     }]
  //   })
  // })

  console.log('Extracting with Gemini API')
  console.log('Using custom instructions:', !!instructions)

  return {
    titulo: 'Artigo Extraído',
    autores: [{ nome: 'Extraído', sobrenome: 'Por Gemini' }],
    ano_publicacao: new Date().getFullYear(),
    resumo: 'Resumo extraído do PDF',
    doi: undefined,
  }
}
