using System;
using System.Threading;
using System.Threading.Tasks;
using EtnoPapers.Core.Models;

namespace EtnoPapers.Core.Services
{
    /// <summary>
    /// Orchestrates the complete extraction pipeline: PDF → text → AI → validation → storage
    /// </summary>
    public class ExtractionPipelineService
    {
        private readonly PDFProcessingService _pdfService;
        private readonly OLLAMAService _ollamaService;
        private readonly ValidationService _validationService;
        private readonly DataStorageService _storageService;
        private CancellationTokenSource _cancellationTokenSource;

        public string CurrentStep { get; private set; }
        public int Progress { get; private set; }
        public bool IsExtracting { get; private set; }

        public ExtractionPipelineService(
            PDFProcessingService pdfService,
            OLLAMAService ollamaService,
            ValidationService validationService,
            DataStorageService storageService)
        {
            _pdfService = pdfService;
            _ollamaService = ollamaService;
            _validationService = validationService;
            _storageService = storageService;
        }

        /// <summary>
        /// Extracts metadata from PDF file.
        /// </summary>
        public async Task<ArticleRecord> ExtractFromPdfAsync(string filePath)
        {
            IsExtracting = true;
            try
            {
                CurrentStep = "Validating PDF";
                Progress = 10;

                if (!_pdfService.ValidatePDF(filePath))
                    throw new InvalidOperationException("Invalid PDF file");

                CurrentStep = "Extracting text";
                Progress = 25;
                var text = _pdfService.ExtractText(filePath);

                CurrentStep = "Processing with AI";
                Progress = 50;
                var metadata = await _ollamaService.ExtractMetadataAsync(text);

                CurrentStep = "Validating extracted data";
                Progress = 75;
                var record = Newtonsoft.Json.JsonConvert.DeserializeObject<ArticleRecord>(metadata);

                if (!_validationService.ValidateRecord(record))
                    throw new InvalidOperationException("Extracted data validation failed");

                CurrentStep = "Complete";
                Progress = 100;
                return record;
            }
            finally
            {
                IsExtracting = false;
            }
        }

        /// <summary>
        /// Cancels the extraction process.
        /// </summary>
        public void CancelExtraction()
        {
            _cancellationTokenSource?.Cancel();
        }

        /// <summary>
        /// Gets the extraction status.
        /// </summary>
        public (string step, int progress, bool isExtracting) GetExtractionStatus()
        {
            return (CurrentStep, Progress, IsExtracting);
        }
    }
}
