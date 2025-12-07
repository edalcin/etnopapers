using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;
using Serilog;

namespace EtnoPapers.Core.Services
{
    /// <summary>
    /// Converts PDF documents to structured Markdown format to improve LLM extraction accuracy.
    /// Uses PdfPig for superior text extraction and document structure analysis.
    /// </summary>
    public class MarkdownConverter
    {
        private readonly ILogger _logger;

        public MarkdownConverter(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Converts a PDF document to structured Markdown.
        /// Falls back to raw text extraction if structure detection fails.
        /// </summary>
        /// <param name="pdfPath">Path to the PDF file</param>
        /// <returns>Markdown-formatted text</returns>
        public string ConvertToMarkdown(string pdfPath)
        {
            try
            {
                _logger.Information("Starting PDF to Markdown conversion: {PdfPath}", pdfPath);
                System.Diagnostics.Debug.WriteLine($"\n>>> MarkdownConverter.ConvertToMarkdown() called with: {pdfPath}");

                using (var document = PdfDocument.Open(pdfPath))
                {
                    var markdown = new StringBuilder();
                    var pageCount = document.NumberOfPages;

                    _logger.Debug("PDF has {PageCount} pages", pageCount);
                    System.Diagnostics.Debug.WriteLine($">>> PDF opened successfully: {pageCount} pages");

                    for (int pageNumber = 1; pageNumber <= pageCount; pageNumber++)
                    {
                        var page = document.GetPage(pageNumber);
                        var pageMarkdown = ConvertPageToMarkdown(page, pageNumber);
                        markdown.Append(pageMarkdown);

                        // Add page separator (except for last page)
                        if (pageNumber < pageCount)
                        {
                            markdown.AppendLine();
                            markdown.AppendLine("---");
                            markdown.AppendLine();
                        }
                    }

                    var result = markdown.ToString();
                    _logger.Information("Successfully converted PDF to Markdown ({CharCount} characters)", result.Length);
                    System.Diagnostics.Debug.WriteLine($">>> ConvertToMarkdown() returning {result.Length} characters");
                    var preview = result.Length > 300 ? result.Substring(0, 300) : result;
                    System.Diagnostics.Debug.WriteLine($">>> Result preview (first 300 chars):\n{preview}...");
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "Structured Markdown conversion failed, falling back to raw text extraction");
                System.Diagnostics.Debug.WriteLine($"\n>>> FALLBACK TO RAW TEXT: {ex.Message}");
                return ConvertToMarkdownWithFallback(pdfPath);
            }
        }

        /// <summary>
        /// Converts a single PDF page to Markdown format
        /// </summary>
        private string ConvertPageToMarkdown(Page page, int pageNumber)
        {
            var markdown = new StringBuilder();

            try
            {
                // Extract words with position and font information
                var words = page.GetWords().ToList();

                if (words.Count == 0)
                {
                    _logger.Warning("Page {PageNumber} has no extractable words", pageNumber);
                    return string.Empty;
                }

                // Detect document structure
                var headings = DetectHeadings(words);
                var tables = DetectTables(words);
                var paragraphs = DetectParagraphs(words, headings);

                _logger.Debug("Page {PageNumber}: {HeadingCount} headings, {TableCount} tables, {ParagraphCount} paragraphs",
                    pageNumber, headings.Count, tables.Count, paragraphs.Count);

                // Build Markdown structure
                // For now, use simple text extraction with intelligent line breaking
                // TODO: Implement full structure detection in future iterations

                var pageText = ContentOrderTextExtractor.GetText(page);
                var lines = pageText.Split('\n', StringSplitOptions.RemoveEmptyEntries);

                foreach (var line in lines)
                {
                    var trimmedLine = line.Trim();
                    if (string.IsNullOrWhiteSpace(trimmedLine))
                        continue;

                    // Simple heuristic: if line is short and in UPPER CASE or Title Case, treat as heading
                    if (trimmedLine.Length < 100 && IsLikelyHeading(trimmedLine))
                    {
                        markdown.AppendLine($"## {trimmedLine}");
                        markdown.AppendLine();
                    }
                    else
                    {
                        markdown.AppendLine(trimmedLine);
                        markdown.AppendLine();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error converting page {PageNumber} to Markdown", pageNumber);
                // Return empty string for failed pages
                return string.Empty;
            }

            return markdown.ToString();
        }

        /// <summary>
        /// Detects headings based on font size, boldness, and position
        /// </summary>
        private List<HeadingInfo> DetectHeadings(List<Word> words)
        {
            var headings = new List<HeadingInfo>();

            if (words.Count == 0)
                return headings;

            // Calculate average font size
            var fontSizes = words.Select(w => w.Letters.FirstOrDefault()?.FontSize ?? 0).Where(size => size > 0).ToList();
            if (fontSizes.Count == 0)
                return headings;

            var avgFontSize = fontSizes.Average();
            var maxFontSize = fontSizes.Max();

            // Detect headings: words with font size significantly larger than average
            var currentHeading = new List<Word>();

            foreach (var word in words)
            {
                var fontSize = word.Letters.FirstOrDefault()?.FontSize ?? 0;

                // Consider it a heading if font size is >20% larger than average
                if (fontSize > avgFontSize * 1.2)
                {
                    currentHeading.Add(word);
                }
                else if (currentHeading.Any())
                {
                    // End of heading
                    var headingText = string.Join(" ", currentHeading.Select(w => w.Text));
                    var level = fontSize > avgFontSize * 1.5 ? 1 : 2; // H1 vs H2

                    headings.Add(new HeadingInfo
                    {
                        Text = headingText,
                        Level = level,
                        Position = currentHeading.First().BoundingBox.Bottom
                    });

                    currentHeading.Clear();
                }
            }

            return headings;
        }

        /// <summary>
        /// Detects tables based on alignment patterns
        /// </summary>
        private List<TableInfo> DetectTables(List<Word> words)
        {
            var tables = new List<TableInfo>();

            // TODO: Implement sophisticated table detection
            // For now, return empty list (tables will be rendered as plain text)

            return tables;
        }

        /// <summary>
        /// Detects paragraphs based on spacing and line breaks
        /// </summary>
        private List<ParagraphInfo> DetectParagraphs(List<Word> words, List<HeadingInfo> headings)
        {
            var paragraphs = new List<ParagraphInfo>();

            // TODO: Implement paragraph detection
            // For now, return empty list (text will be processed line by line)

            return paragraphs;
        }

        /// <summary>
        /// Simple heuristic to detect if a line is likely a heading
        /// </summary>
        private bool IsLikelyHeading(string line)
        {
            // Check if line is mostly uppercase (section headings)
            var upperCaseRatio = line.Count(char.IsUpper) / (double)line.Length;
            if (upperCaseRatio > 0.7)
                return true;

            // Check if line starts with a number (e.g., "1. Introduction")
            if (char.IsDigit(line[0]) && line.Contains('.'))
                return true;

            // Check common heading keywords
            var headingKeywords = new[] { "abstract", "resumo", "introduction", "introdução", "methodology", "metodologia",
                                          "results", "resultados", "discussion", "discussão", "conclusion", "conclusão",
                                          "references", "referências", "acknowledgments", "agradecimentos" };

            if (headingKeywords.Any(keyword => line.ToLowerInvariant().StartsWith(keyword)))
                return true;

            return false;
        }

        /// <summary>
        /// Fallback method: extracts raw text without structure detection
        /// </summary>
        private string ConvertToMarkdownWithFallback(string pdfPath)
        {
            try
            {
                _logger.Information("Using fallback raw text extraction for: {PdfPath}", pdfPath);

                using (var document = PdfDocument.Open(pdfPath))
                {
                    var text = new StringBuilder();

                    for (int i = 1; i <= document.NumberOfPages; i++)
                    {
                        var page = document.GetPage(i);
                        var pageText = ContentOrderTextExtractor.GetText(page);
                        text.AppendLine(pageText);
                        text.AppendLine();
                    }

                    return text.ToString();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to extract text from PDF even with fallback method");
                throw new InvalidOperationException($"Unable to extract text from PDF: {pdfPath}", ex);
            }
        }

        // Helper classes for structure detection

        private class HeadingInfo
        {
            public string Text { get; set; }
            public int Level { get; set; }
            public double Position { get; set; }
        }

        private class TableInfo
        {
            public List<List<string>> Rows { get; set; } = new List<List<string>>();
        }

        private class ParagraphInfo
        {
            public string Text { get; set; }
            public double PositionY { get; set; }
        }
    }
}
