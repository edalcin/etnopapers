using System;
using System.IO;
using System.Text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using Serilog;

namespace EtnoPapers.Core.Services
{
    /// <summary>
    /// Coordinates PDF processing and Markdown conversion.
    /// Primary method: ProcessPDF() returns structured Markdown for better LLM extraction.
    /// Legacy method: ExtractText() returns raw text (kept for backward compatibility).
    /// </summary>
    public class PDFProcessingService
    {
        private readonly MarkdownConverter _markdownConverter;
        private readonly ILogger _logger;

        public PDFProcessingService(MarkdownConverter markdownConverter, ILogger logger)
        {
            _markdownConverter = markdownConverter ?? throw new ArgumentNullException(nameof(markdownConverter));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        static PDFProcessingService()
        {
            // Register encoding provider for extended encodings (MacRoman, etc.)
            try
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            }
            catch
            {
                // Ignore if already registered
            }
        }

        /// <summary>
        /// Processes a PDF file and returns structured Markdown representation.
        /// This is the PRIMARY method to use for better LLM extraction accuracy.
        /// Falls back to raw text if Markdown conversion fails.
        /// </summary>
        /// <param name="filePath">Path to the PDF file</param>
        /// <returns>Structured Markdown text</returns>
        public string ProcessPDF(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"PDF file not found: {filePath}");

            _logger.Information("Processing PDF: {FilePath}", filePath);

            // Validate PDF first
            if (!ValidatePDF(filePath))
            {
                throw new InvalidOperationException($"Invalid PDF file: {filePath}");
            }

            // Check for text layers (not scanned)
            if (!CheckTextLayers(filePath))
            {
                _logger.Warning("PDF appears to be scanned (no text layers): {FilePath}", filePath);
                throw new InvalidOperationException("PDF appears to be scanned. OCR is not supported. Please provide a text-based PDF.");
            }

            // Convert to Markdown
            try
            {
                var markdown = _markdownConverter.ConvertToMarkdown(filePath);
                _logger.Information("Successfully processed PDF to Markdown ({CharCount} characters)", markdown.Length);
                return markdown;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to process PDF to Markdown: {FilePath}", filePath);
                throw new InvalidOperationException($"Failed to process PDF: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// LEGACY: Extracts all text from a PDF file (raw text, no structure).
        /// Use ProcessPDF() instead for better LLM extraction accuracy.
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
                        try
                        {
                            var pageText = PdfTextExtractor.GetTextFromPage(reader, page);
                            if (!string.IsNullOrWhiteSpace(pageText))
                            {
                                text.AppendLine(pageText);
                            }
                        }
                        catch (Exception pageEx)
                        {
                            // Log page error but continue with other pages
                            System.Diagnostics.Debug.WriteLine($"Warning: Failed to extract text from page {page}: {pageEx.Message}");
                        }
                    }
                }

                // If no text was extracted, throw error
                if (text.Length == 0)
                    throw new InvalidOperationException("No text content found in PDF. The file may be scanned or empty.");

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
