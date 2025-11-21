export interface PDFExtractionResult {
  text: string
  pages: number
  filename: string
  size: number
}

export const pdfExtractor = {
  extractText: async (file: File): Promise<PDFExtractionResult> => {
    const arrayBuffer = await file.arrayBuffer()
    return {
      text: new TextDecoder().decode(arrayBuffer),
      pages: 1,
      filename: file.name,
      size: file.size,
    }
  },

  validatePDF: (file: File): boolean => {
    return file.type === 'application/pdf' && file.size > 0
  },
}
