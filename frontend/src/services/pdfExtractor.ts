export interface PDFExtractionResult {
  text: string
  pages: number
  filename: string
  size: number
}

export const extractTextFromPDF = async (file: File): Promise<PDFExtractionResult> => {
  const arrayBuffer = await file.arrayBuffer()
  return {
    text: new TextDecoder().decode(arrayBuffer),
    pages: 1,
    filename: file.name,
    size: file.size,
  }
}

export const validatePDF = (file: File): boolean => {
  return file.type === 'application/pdf' && file.size > 0
}

export const pdfExtractor = {
  extractText: extractTextFromPDF,
  validatePDF,
}
