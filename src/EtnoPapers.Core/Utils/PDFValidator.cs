using System;
using System.IO;
using System.Text;

namespace EtnoPapers.Core.Utils
{
    /// <summary>
    /// Validates PDF files before processing.
    /// </summary>
    public static class PDFValidator
    {
        private const long MaxFileSizeBytes = 50 * 1024 * 1024; // 50 MB
        private const string PDFMagicNumber = "%PDF";

        /// <summary>
        /// Validates a PDF file for processing.
        /// </summary>
        public static bool ValidatePDFFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return false;

            if (!File.Exists(filePath))
                return false;

            // Check magic number
            if (!IsValidPDFMagic(filePath))
                return false;

            // Check file size
            if (!IsValidFileSize(filePath))
                return false;

            return true;
        }

        /// <summary>
        /// Gets the file size in bytes.
        /// </summary>
        public static long GetFileSize(string filePath)
        {
            try
            {
                var fileInfo = new FileInfo(filePath);
                return fileInfo.Length;
            }
            catch
            {
                return 0;
            }
        }

        private static bool IsValidPDFMagic(string filePath)
        {
            try
            {
                var header = new byte[4];
                using (var fs = File.OpenRead(filePath))
                {
                    if (fs.Read(header, 0, 4) < 4)
                        return false;
                }

                var headerString = Encoding.ASCII.GetString(header);
                return headerString.StartsWith(PDFMagicNumber);
            }
            catch
            {
                return false;
            }
        }

        private static bool IsValidFileSize(string filePath)
        {
            try
            {
                var fileSize = GetFileSize(filePath);
                return fileSize > 0 && fileSize <= MaxFileSizeBytes;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets human-readable error message for validation failure.
        /// </summary>
        public static string GetValidationError(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return "O caminho do arquivo está vazio.";

            if (!File.Exists(filePath))
                return $"O arquivo não foi encontrado: {filePath}";

            if (!IsValidPDFMagic(filePath))
                return "O arquivo não é um PDF válido.";

            if (!IsValidFileSize(filePath))
            {
                var fileSize = GetFileSize(filePath);
                var sizeMB = fileSize / (1024 * 1024);
                return $"O arquivo é muito grande ({sizeMB} MB). Máximo: 50 MB.";
            }

            return "Erro desconhecido ao validar o arquivo PDF.";
        }
    }
}
