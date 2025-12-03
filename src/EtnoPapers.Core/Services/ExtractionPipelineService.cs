using System;
using System.Threading;
using System.Threading.Tasks;
using EtnoPapers.Core.Models;

namespace EtnoPapers.Core.Services
{
    /// <summary>
    /// Progress update event arguments
    /// </summary>
    public class ProgressUpdateEventArgs : EventArgs
    {
        public int Progress { get; set; }
        public string Step { get; set; }
        public string Message { get; set; }
    }

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

        /// <summary>
        /// Event raised when progress is updated
        /// </summary>
        public event EventHandler<ProgressUpdateEventArgs>? ProgressUpdated;

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
        /// Updates progress and raises ProgressUpdated event
        /// </summary>
        private void UpdateProgress(int progress, string step, string message)
        {
            Progress = progress;
            CurrentStep = step;
            ProgressUpdated?.Invoke(this, new ProgressUpdateEventArgs
            {
                Progress = progress,
                Step = step,
                Message = message
            });
        }

        /// <summary>
        /// Extracts metadata from PDF file.
        /// </summary>
        public async Task<ArticleRecord> ExtractFromPdfAsync(string filePath)
        {
            IsExtracting = true;
            try
            {
                UpdateProgress(10, "Validating PDF", "Validando arquivo PDF...");

                if (!_pdfService.ValidatePDF(filePath))
                    throw new InvalidOperationException("Invalid PDF file");

                UpdateProgress(25, "Extracting text", "Extraindo texto do PDF...");
                var text = _pdfService.ExtractText(filePath);

                UpdateProgress(50, "Processing with AI", "Processando com IA (OLLAMA)...");
                var metadata = await _ollamaService.ExtractMetadataAsync(text);

                UpdateProgress(75, "Validating extracted data", "Validando dados extraídos...");

                ArticleRecord record = null;
                try
                {
                    record = Newtonsoft.Json.JsonConvert.DeserializeObject<ArticleRecord>(metadata);
                }
                catch (Exception ex)
                {
                    var detailedError = $"Erro ao fazer parsing dos dados JSON extraídos:\n{ex.Message}\n\nJSON recebido:\n{metadata}";
                    UpdateProgress(75, "Error", $"Erro no parsing: {ex.Message}");
                    throw new InvalidOperationException(detailedError, ex);
                }

                if (!_validationService.ValidateRecord(record))
                {
                    var errors = _validationService.GetValidationErrors(record);
                    var errorDetails = "Erros de validação encontrados:\n" + string.Join("\n", errors);

                    UpdateProgress(75, "Validation Error", $"Validação falhou: {errors.Count} erro(s)");

                    // Return partial record so user can edit it manually

                    // Generate ID and timestamps for partial record
                    if (string.IsNullOrEmpty(record.Id))
                        record.Id = Guid.NewGuid().ToString();

                    record.DataCriacao = DateTime.UtcNow;
                    record.DataUltimaAtualizacao = DateTime.UtcNow;
                    record.StatusSincronizacao = "local";

                    UpdateProgress(100, "Complete - Manual Edit Needed",
                        "Extração parcial concluída. Campos faltantes precisam ser preenchidos manualmente.");

                    return record;
                }

                UpdateProgress(100, "Complete", "Extração concluída com sucesso!");
                return record;
            }
            finally
            {
                IsExtracting = false;
            }
        }

        /// <summary>
        /// Suggests field fixes based on validation errors
        /// </summary>
        private string SuggestFieldFixes(ArticleRecord record, List<string> errors)
        {
            var suggestions = "Sugestões:\n";

            foreach (var error in errors)
            {
                if (error.Contains("Titulo"))
                    suggestions += "• Título vazio: O PDF pode estar em idioma estrangeiro ou corrompido\n";
                else if (error.Contains("Autores"))
                    suggestions += "• Autores ausentes: Verifique se o PDF tem metadata ou página de capa\n";
                else if (error.Contains("Ano"))
                    suggestions += "• Ano inválido: Verifique o ano de publicação no PDF\n";
                else if (error.Contains("Resumo"))
                    suggestions += "• Resumo vazio: O PDF pode não ter um resumo estruturado\n";
            }

            suggestions += "\nA janela de edição permitirá que você corrija esses campos manualmente.";
            return suggestions;
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
