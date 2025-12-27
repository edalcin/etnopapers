using System;
using System.Threading;
using System.Threading.Tasks;
using EtnoPapers.Core.Models;
using EtnoPapers.Core.Utils;

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
        private readonly ValidationService _validationService;
        private readonly DataStorageService _storageService;
        private readonly ConfigurationService _configurationService;
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
            ValidationService validationService,
            DataStorageService storageService,
            ConfigurationService configurationService)
        {
            _pdfService = pdfService;
            _validationService = validationService;
            _storageService = storageService;
            _configurationService = configurationService;
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
            var debugLogFile = System.IO.Path.Combine(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData),
                "EtnoPapers", "logs", "extraction-markdown-debug.log");

            try
            {
                // Log extraction start
                LogToFile(debugLogFile, $"\n{'='*60}");
                LogToFile(debugLogFile, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] STARTING EXTRACTION");
                LogToFile(debugLogFile, $"File: {filePath}");
                LogToFile(debugLogFile, $"{'='*60}");

                UpdateProgress(10, "Validating PDF", "Validando arquivo PDF...");

                if (!_pdfService.ValidatePDF(filePath))
                    throw new InvalidOperationException("Invalid PDF file");

                LogToFile(debugLogFile, $"[{DateTime.Now:HH:mm:ss}] PDF validated successfully");

                UpdateProgress(25, "Converting to Markdown", "Convertendo PDF para Markdown estruturado...");
                var markdown = _pdfService.ProcessPDF(filePath);

                // Log markdown result
                LogToFile(debugLogFile, $"\n>>> MARKDOWN CONVERSION RESULT");
                LogToFile(debugLogFile, $"Total length: {markdown.Length} characters");
                LogToFile(debugLogFile, $"Contains ## headings: {markdown.Contains("##")}");
                LogToFile(debugLogFile, $"Contains --- separators: {markdown.Contains("---")}");
                var markdownPreview = markdown.Length > 800 ? markdown.Substring(0, 800) + "\n[... truncated ...]" : markdown;
                LogToFile(debugLogFile, $"\nFirst 800 characters of Markdown:\n{markdownPreview}\n");

                // Load configuration and create AI provider
                var config = _configurationService.LoadConfiguration();

                if (!config.IsCloudAIConfigured)
                {
                    throw new InvalidOperationException(
                        "Provedor de IA não configurado. Configure um provedor de IA (Gemini, OpenAI ou Anthropic) nas Configurações.");
                }

                var providerName = config.AIProvider.Value.ToString();
                UpdateProgress(50, "Processing with AI", $"Processando com IA em nuvem ({providerName})...");

                // Check internet connectivity before calling cloud API
                LogToFile(debugLogFile, $"\n>>> CHECKING INTERNET CONNECTIVITY");
                if (!await NetworkHelper.IsInternetAvailableAsync())
                {
                    throw new InvalidOperationException(
                        "Sem conexão com a internet. Verifique sua conexão de rede e tente novamente.");
                }
                LogToFile(debugLogFile, $"Internet connection: OK");

                LogToFile(debugLogFile, $"\n>>> SENDING TO CLOUD AI");
                LogToFile(debugLogFile, $"Provider: {providerName}");
                LogToFile(debugLogFile, $"Markdown length: {markdown.Length} characters");

                // Create provider and extract metadata
                var provider = AIProviderFactory.CreateProvider(config.AIProvider.Value);
                provider.SetApiKey(config.ApiKey);

                var metadata = await provider.ExtractMetadataAsync(markdown, config.CustomExtractionPrompt);

                // Log AI response for debugging
                LogToFile(debugLogFile, $"\n>>> CLOUD AI RESPONSE RECEIVED");
                LogToFile(debugLogFile, $"Response length: {metadata.Length} characters");
                LogToFile(debugLogFile, $"\n--- FULL AI RESPONSE ---");
                LogToFile(debugLogFile, metadata);
                LogToFile(debugLogFile, $"--- END AI RESPONSE ---\n");

                UpdateProgress(75, "Validating extracted data", "Validando dados extraídos...");

                // Parse JSON response (already cleaned by the AI service)
                ArticleRecord record = null;
                try
                {
                    record = Newtonsoft.Json.JsonConvert.DeserializeObject<ArticleRecord>(metadata);
                    System.Diagnostics.Debug.WriteLine($"Parsed record: Titulo={record?.Titulo}, Autores={record?.Autores?.Count ?? 0}, Ano={record?.Ano}");
                }
                catch (Exception ex)
                {
                    var detailedError = $"Erro ao fazer parsing dos dados JSON extraídos:\n{ex.Message}\n\nJSON recebido:\n{metadata}";
                    System.Diagnostics.Debug.WriteLine($"JSON Parsing Error: {detailedError}");
                    UpdateProgress(75, "Error", $"Erro no parsing: {ex.Message}");
                    throw new InvalidOperationException(detailedError, ex);
                }

                if (!_validationService.ValidateRecord(record))
                {
                    var errors = _validationService.GetValidationErrors(record);
                    var errorDetails = "Erros de validação encontrados:\n" + string.Join("\n", errors);
                    System.Diagnostics.Debug.WriteLine($"Validation errors: {errorDetails}");

                    UpdateProgress(75, "Validation Error", $"Validação falhou: {errors.Count} erro(s)");

                    // Return partial record so user can edit it manually

                    // Generate ID and timestamps for partial record
                    if (string.IsNullOrEmpty(record.Id))
                        record.Id = Guid.NewGuid().ToString();

                    record.CreatedAt = DateTime.UtcNow;
                    record.UpdatedAt = DateTime.UtcNow;

                    System.Diagnostics.Debug.WriteLine($"Returning partial record for manual edit: {record.Id}");
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

        /// <summary>
        /// Logs a message to a file (for debug tracing when not in Visual Studio)
        /// </summary>
        private void LogToFile(string filePath, string message)
        {
            try
            {
                var directory = System.IO.Path.GetDirectoryName(filePath);
                if (!System.IO.Directory.Exists(directory))
                    System.IO.Directory.CreateDirectory(directory);

                System.IO.File.AppendAllText(filePath, message + Environment.NewLine);
            }
            catch
            {
                // Silently fail if can't write log (don't interrupt extraction)
            }
        }
    }
}

