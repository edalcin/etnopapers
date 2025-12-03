using System;
using System.IO;
using System.Text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace EtnoPapers.Core.Services
{
    /// <summary>
    /// Extracts text and metadata from PDF files using iTextSharp.
    /// </summary>
    public class PDFProcessingService
    {
        /// <summary>
        /// Extracts all text from a PDF file.
        /// </summary>
        public string ExtractText(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"PDF file not found: {filePath}");

            var text = new StringBuilder();

            try
            {
                using (var reader = new PdfReader(filePath))
                {
                    for (int page = 1; page <= reader.NumberOfPages; page++)
                    {
                        var pageText = PdfTextExtractor.GetTextFromPage(reader, page);
                        text.AppendLine(pageText);
                    }
                }

                return text.ToString();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to extract text from PDF: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Checks if PDF contains text layers (not scanned).
        /// </summary>
        public bool CheckTextLayers(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"PDF file not found: {filePath}");

            try
            {
                using (var reader = new PdfReader(filePath))
                {
                    for (int page = 1; page <= reader.NumberOfPages; page++)
                    {
                        var pageText = PdfTextExtractor.GetTextFromPage(reader, page);
                        if (!string.IsNullOrWhiteSpace(pageText))
                            return true; // Has text layers
                    }
                }

                return false; // No text found, likely scanned
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validates that a PDF file is readable.
        /// </summary>
        public bool ValidatePDF(string filePath)
        {
            if (!File.Exists(filePath))
                return false;

            try
            {
                // Check magic number for PDF
                var header = new byte[5];
                using (var fs = File.OpenRead(filePath))
                {
                    fs.Read(header, 0, 5);
                }

                var headerString = Encoding.ASCII.GetString(header);
                if (headerString != "%PDF-")
                    return false;

                // Try to open and validate
                using (var reader = new PdfReader(filePath))
                {
                    return reader.NumberOfPages > 0;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
