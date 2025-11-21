/**
 * PDF Text Extraction Service using pdf.js
 * Extracts text from PDF files in the browser
 */

// @ts-ignore - pdf.js doesn't have perfect TypeScript support
import * as pdfjsLib from 'pdfjs-dist'

// Set worker
pdfjsLib.GlobalWorkerOptions.workerSrc = `//cdnjs.cloudflare.com/ajax/libs/pdf.js/${pdfjsLib.version}/pdf.worker.min.js`

export interface ExtractionProgress {
  currentPage: number
  totalPages: number
  percentage: number
}

export interface ExtractionResult {
  text: string
  totalPages: number
  isScanned: boolean
  confidence: number
}

/**
 * Extract text from a PDF file
 * @param file PDF file to extract from
 * @param onProgress Callback for progress updates
 * @returns Extracted text and metadata
 */
export async function extractTextFromPDF(
  file: File,
  onProgress?: (progress: ExtractionProgress) => void
): Promise<ExtractionResult> {
  try {
    const arrayBuffer = await file.arrayBuffer()
    const pdf = await pdfjsLib.getDocument({ data: arrayBuffer }).promise

    let fullText = ''
    let textCharacterCount = 0
    const totalPages = pdf.numPages

    // Extract text from each page
    for (let pageNum = 1; pageNum <= totalPages; pageNum++) {
      const page = await pdf.getPage(pageNum)
      const textContent = await page.getTextContent()

      // Extract text from items
      const pageText = textContent.items
        .map((item: any) => item.str || '')
        .join(' ')

      fullText += `\n--- Page ${pageNum} ---\n${pageText}`
      textCharacterCount += pageText.length

      // Report progress
      if (onProgress) {
        onProgress({
          currentPage: pageNum,
          totalPages,
          percentage: Math.round((pageNum / totalPages) * 100),
        })
      }
    }

    // Detect if PDF is scanned (low text-to-page ratio)
    const avgCharsPerPage = textCharacterCount / totalPages
    const isScanned = avgCharsPerPage < 500 // Threshold for scanned detection

    return {
      text: fullText.trim(),
      totalPages,
      isScanned,
      confidence: isScanned ? 0.3 : 0.9, // Lower confidence for scanned PDFs
    }
  } catch (error) {
    console.error('Error extracting PDF:', error)
    throw new Error(
      `Falha ao extrair texto do PDF: ${error instanceof Error ? error.message : 'Erro desconhecido'}`
    )
  }
}

/**
 * Get PDF metadata
 * @param file PDF file
 * @returns PDF metadata
 */
export async function getPDFMetadata(file: File) {
  try {
    const arrayBuffer = await file.arrayBuffer()
    const pdf = await pdfjsLib.getDocument({ data: arrayBuffer }).promise
    const metadata = await pdf.getMetadata()

    return {
      numPages: pdf.numPages,
      fingerprint: pdf.fingerprint,
      isEncrypted: pdf.isEncrypted,
      metadata: metadata?.metadata || {},
    }
  } catch (error) {
    console.error('Error getting PDF metadata:', error)
    return null
  }
}

/**
 * Validate if file is a valid PDF
 * @param file File to validate
 * @returns true if valid PDF
 */
export async function isValidPDF(file: File): Promise<boolean> {
  if (!file.type.includes('pdf') && !file.name.endsWith('.pdf')) {
    return false
  }

  try {
    const arrayBuffer = await file.arrayBuffer()
    const pdfSignature = new Uint8Array(arrayBuffer.slice(0, 4))
    const isValidSignature =
      pdfSignature[0] === 0x25 && // %
      pdfSignature[1] === 0x50 && // P
      pdfSignature[2] === 0x44 && // D
      pdfSignature[3] === 0x46 // F

    if (!isValidSignature) {
      return false
    }

    // Try to open it with pdf.js
    await pdfjsLib.getDocument({ data: arrayBuffer }).promise

    return true
  } catch (error) {
    console.error('Invalid PDF file:', error)
    return false
  }
}
