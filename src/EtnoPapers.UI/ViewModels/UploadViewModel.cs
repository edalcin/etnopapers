using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Media;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Input;
using EtnoPapers.Core.Models;
using EtnoPapers.Core.Services;
using EtnoPapers.UI.Commands;

namespace EtnoPapers.UI.ViewModels
{
    /// <summary>
    /// ViewModel for the PDF upload and extraction workflow.
    /// Manages file selection, extraction progress, and results editing.
    /// </summary>
    public class UploadViewModel : ViewModelBase
    {
        private readonly DataStorageService _storageService;
        private readonly ExtractionPipelineService _extractionService;
        private readonly PDFProcessingService _pdfService;
        private readonly ValidationService _validationService;
        private readonly LoggerService _loggerService;
        private readonly ConfigurationService _configService;
        private RelayCommand _startExtractionCommand;
        private Stopwatch? _extractionStopwatch;

        private string _selectedFilePath = "";
        private bool _isExtracting = false;
        private string _currentStep = "";
        private int _extractionProgress = 0;
        private ArticleRecord? _extractedData;
        private bool _allowSave = false;
        private string _errorMessage = "";
        private bool _hasExtractionError = false;

                public UploadViewModel()
        {
            _storageService = new DataStorageService();
            _configService = new ConfigurationService();
            _validationService = new ValidationService();
            _loggerService = new LoggerService();

            // Initialize PDF processing service with Markdown converter
            var markdownConverter = new MarkdownConverter(_loggerService.Logger);
            _pdfService = new PDFProcessingService(markdownConverter, _loggerService.Logger);

            // ExtractionPipelineService now uses cloud AI providers (Gemini, OpenAI, Anthropic)
            // Configuration is loaded from ConfigurationService which manages API keys securely
            _extractionService = new ExtractionPipelineService(
                _pdfService,
                _validationService,
                _storageService,
                _configService);

            SelectFileCommand = new RelayCommand(_ => SelectFile());
            _startExtractionCommand = new RelayCommand(_ => StartExtraction(), _ => CanStartExtraction());
            StartExtractionCommand = _startExtractionCommand;
            CancelExtractionCommand = new RelayCommand(_ => CancelExtraction(), _ => IsExtracting);
            SaveResultsCommand = new RelayCommand(_ => SaveResults(), _ => AllowSave && !IsExtracting);
            ClearCommand = new RelayCommand(_ => Clear());

            _loggerService.Info("UploadViewModel initialized with cloud AI provider support (Gemini, OpenAI, Anthropic)");
        }

        #region Properties

        public string SelectedFilePath
        {
            get => _selectedFilePath;
            set => SetProperty(ref _selectedFilePath, value);
        }

        public bool IsExtracting
        {
            get => _isExtracting;
            set => SetProperty(ref _isExtracting, value);
        }

        public string CurrentStep
        {
            get => _currentStep;
            set => SetProperty(ref _currentStep, value);
        }

        public int ExtractionProgress
        {
            get => _extractionProgress;
            set => SetProperty(ref _extractionProgress, value);
        }

        public ArticleRecord? ExtractedData
        {
            get => _extractedData;
            set => SetProperty(ref _extractedData, value);
        }

        public bool AllowSave
        {
            get => _allowSave;
            set => SetProperty(ref _allowSave, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool HasExtractionError
        {
            get => _hasExtractionError;
            set => SetProperty(ref _hasExtractionError, value);
        }

        #endregion

        #region Commands

        public ICommand SelectFileCommand { get; }
        public ICommand StartExtractionCommand { get; }
        public ICommand CancelExtractionCommand { get; }
        public ICommand SaveResultsCommand { get; }
        public ICommand ClearCommand { get; }

        #endregion

        #region Methods

        private void SelectFile()
        {
            var openFileDialog = new System.Windows.Forms.OpenFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf|All Files (*.*)|*.*",
                Title = "Select PDF File",
                CheckFileExists = true,
                CheckPathExists = true
            };

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SelectedFilePath = openFileDialog.FileName;
                _loggerService.Info($"PDF selected: {SelectedFilePath}");

                // Notify StartExtractionCommand that CanExecute may have changed
                _startExtractionCommand?.RaiseCanExecuteChanged();
            }
        }

        private bool CanStartExtraction()
        {
            return !string.IsNullOrEmpty(SelectedFilePath) && !IsExtracting &&
                   System.IO.File.Exists(SelectedFilePath);
        }

        private async void StartExtraction()
        {
            if (!CanStartExtraction())
                return;

            IsExtracting = true;
            HasExtractionError = false;
            ErrorMessage = "";
            ExtractionProgress = 0;

            // Start extraction timer
            _extractionStopwatch = Stopwatch.StartNew();
            _loggerService.Info("Extraction timer started");

            Views.ExtractionProgressWindow progressWindow = null;
            Views.ExtractionProgressViewModel progressViewModel = null;
            try
            {
                _loggerService.Info($"Starting extraction for: {SelectedFilePath}");

                try
                {
                    // Create progress window (show modeless, not blocking)
                    progressWindow = new Views.ExtractionProgressWindow
                    {
                        Owner = System.Windows.Application.Current.MainWindow
                    };

                    progressViewModel = progressWindow.DataContext as Views.ExtractionProgressViewModel;
                    if (progressViewModel != null)
                    {
                        progressViewModel.SetExtractionService(_extractionService);
                        progressViewModel.IsExtracting = true;
                    }

                    // Show as modeless window (non-blocking)
                    progressWindow.Show();
                }
                catch (Exception ex)
                {
                    _loggerService.Error($"Error creating progress window: {ex.Message}", ex);
                    HasExtractionError = true;
                    ErrorMessage = $"Erro ao mostrar janela de progresso: {ex.Message}";
                    throw;
                }

                try
                {
                    _loggerService.Info("Calling ExtractFromPdfAsync...");
                    ExtractedData = await _extractionService.ExtractFromPdfAsync(SelectedFilePath);

                    // Stop extraction timer and save elapsed time and agent
                    if (_extractionStopwatch != null)
                    {
                        _extractionStopwatch.Stop();
                        double elapsedSeconds = _extractionStopwatch.Elapsed.TotalSeconds;
                        if (ExtractedData != null)
                        {
                            ExtractedData.TempoExtracao = elapsedSeconds;

                            // Record the AI agent/provider used
                            var config = _configService.LoadConfiguration();
                            if (config?.AIProvider.HasValue == true)
                            {
                                ExtractedData.AgenteIA = config.AIProvider.Value.ToString();
                            }

                            _loggerService.Info($"Extraction completed in {elapsedSeconds:F2} seconds using {ExtractedData.AgenteIA}");
                        }
                    }

                    ExtractionProgress = 100;
                    CurrentStep = "Extração concluída";

                    _loggerService.Info($"ExtractFromPdfAsync returned. ExtractedData is null: {ExtractedData == null}");

                    if (ExtractedData == null)
                    {
                        _loggerService.Error("CRITICAL: ExtractFromPdfAsync returned NULL!");
                        HasExtractionError = true;
                        ErrorMessage = "ERRO CRÍTICO: Pipeline de extração retornou dados NULL!";
                        CurrentStep = "Erro Crítico";

                        System.Windows.MessageBox.Show(
                            "ERRO CRÍTICO:\n\nO pipeline de extração retornou dados NULL.\n\nVerifique os logs para mais detalhes.",
                            "Erro Crítico",
                            System.Windows.MessageBoxButton.OK,
                            System.Windows.MessageBoxImage.Error);

                        return; // Exit extraction
                    }

                    _loggerService.Info($"  Titulo: {ExtractedData.Titulo}");
                    _loggerService.Info($"  Autores count: {ExtractedData.Autores?.Count ?? 0}");
                    _loggerService.Info($"  Ano: {ExtractedData.Ano}");
                    var isValid = _validationService.IsValidForSaving(ExtractedData);
                    _loggerService.Info($"  IsValidForSaving: {isValid}");

                    // Always open EditRecordDialog to allow user review/validation/edit
                    _loggerService.Info("Opening EditRecordDialog for user review...");
                    CurrentStep = isValid ? "Revisão de Dados" : "Edição de Campos Faltantes";

                    // Close progress window safely on UI thread
                    await Task.Delay(500);
                    try
                    {
                        progressWindow?.Close();
                        _loggerService.Info("Progress window closed successfully");
                    }
                    catch (Exception ex)
                    {
                        _loggerService.Error($"Error closing progress window: {ex.Message}", ex);
                    }

                    // Play completion sound
                    try
                    {
                        PlayExtractionCompleteSound();
                    }
                    catch (Exception ex)
                    {
                        _loggerService.Warn($"Could not play completion sound: {ex.Message}");
                    }

                    // Open edit dialog for user to review/edit/validate
                    try
                    {
                        var editDialog = new Views.EditRecordDialog(ExtractedData)
                        {
                            Owner = System.Windows.Application.Current.MainWindow
                        };
                        _loggerService.Info("EditRecordDialog created. Showing dialog...");

                        if (editDialog.ShowDialog() == true)
                        {
                            // User saved the edited record - save automatically
                            _loggerService.Info($"Record edited by user. Saving: {ExtractedData.Id}");
                            SaveResults();
                        }
                        else
                        {
                            _loggerService.Info("EditRecordDialog cancelled by user");
                        }
                    }
                    catch (Exception ex)
                    {
                        _loggerService.Error($"Error creating/showing EditRecordDialog: {ex.Message}\n{ex.StackTrace}", ex);
                        HasExtractionError = true;
                        ErrorMessage = $"Erro ao abrir tela de edição: {ex.Message}";
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    if (progressViewModel != null)
                    {
                        progressViewModel.IsExtracting = false;
                    }

                    // Stop timer if still running
                    if (_extractionStopwatch?.IsRunning == true)
                    {
                        _extractionStopwatch.Stop();
                    }

                    HasExtractionError = true;
                    CurrentStep = "Erro";
                    _loggerService.Error($"Extraction failed: {ex.Message}\nStack trace: {ex.StackTrace}", ex);

                    // Close progress window if still open
                    try
                    {
                        progressWindow?.Close();
                    }
                    catch { }

                    // Handle different error types with user-friendly messages
                    string userMessage = GetUserFriendlyErrorMessage(ex);
                    ErrorMessage = userMessage;

                    // Show error message box to user
                    System.Windows.MessageBox.Show(
                        userMessage,
                        "Erro na Extração",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Error);

                    // Keep error visible to user
                }
            }
            finally
            {
                IsExtracting = false;
                _startExtractionCommand?.RaiseCanExecuteChanged();
            }
        }

        private void CancelExtraction()
        {
            _extractionService.CancelExtraction();
            IsExtracting = false;
            _startExtractionCommand?.RaiseCanExecuteChanged();
            CurrentStep = "Cancelado";
            _loggerService.Info("Extraction cancelled by user");
        }

        private void SaveResults()
        {
            if (ExtractedData == null)
                return;

            try
            {
                if (!_validationService.IsValidForSaving(ExtractedData))
                {
                    ErrorMessage = "Os dados extraídos não passam na validação. Verifique os campos obrigatórios.";
                    _loggerService.Warn("Extracted data failed validation");
                    return;
                }

                if (_storageService.Create(ExtractedData))
                {
                    ErrorMessage = "";
                    _loggerService.Info($"Record saved successfully: {ExtractedData.Id}");

                    // Clear for next upload
                    Clear();

                    // Show success message
                    System.Windows.MessageBox.Show(
                        $"Registro salvo com sucesso!\n\nID: {ExtractedData.Id}",
                        "Sucesso",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Information);
                }
                else
                {
                    ErrorMessage = "Erro ao salvar o registro. O armazenamento local pode estar cheio.";
                    _loggerService.Error("Failed to create record - storage full");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Erro ao salvar: {ex.Message}";
                _loggerService.Error($"Error saving record: {ex.Message}", ex);
            }
        }

        private void Clear()
        {
            SelectedFilePath = "";
            ExtractedData = null;
            CurrentStep = "";
            ExtractionProgress = 0;
            AllowSave = false;
            HasExtractionError = false;
            ErrorMessage = "";
            _loggerService.Info("Upload view cleared");
            _startExtractionCommand?.RaiseCanExecuteChanged();
        }

        private string GetUserFriendlyErrorMessage(Exception ex)
        {
            // Check for timeout-related exceptions
            if (ex is OperationCanceledException)
            {
                return "A extração excedeu o tempo limite de 10 minutos.\n\n" +
                       "Possíveis causas:\n" +
                       "• OLLAMA está processando muito lentamente\n" +
                       "• O modelo de IA é muito grande para o arquivo PDF\n" +
                       "• Problema temporário de conexão\n\n" +
                       "Soluções:\n" +
                       "1. Tentar novamente com um PDF menor\n" +
                       "2. Verificar se OLLAMA está respondendo (http://localhost:11434)\n" +
                       "3. Considerar usar um modelo mais rápido nas configurações";
            }

            if (ex is HttpRequestException && ex.Message.Contains("timeout", System.StringComparison.OrdinalIgnoreCase))
            {
                return "Timeout de conexão com OLLAMA.\n\n" +
                       "O serviço OLLAMA não respondeu dentro do tempo esperado.\n\n" +
                       "Verifique:\n" +
                       "• Se OLLAMA está rodando\n" +
                       "• A URL de conexão está correta\n" +
                       "• Sua conexão de rede está estável";
            }

            if (ex is InvalidOperationException && ex.Message.Contains("OLLAMA", System.StringComparison.OrdinalIgnoreCase))
            {
                return "Erro de conexão com OLLAMA:\n\n" + ex.Message + "\n\n" +
                       "Verifique se OLLAMA está instalado e rodando em http://localhost:11434";
            }

            if (ex is InvalidOperationException && ex.Message.Contains("modelo", System.StringComparison.OrdinalIgnoreCase))
            {
                return "Erro de modelo de IA:\n\n" + ex.Message + "\n\n" +
                       "Verifique nas configurações qual modelo está sendo usado";
            }

            // Generic error message
            return $"Erro durante a extração:\n\n{ex.Message}\n\n" +
                   "Verifique os logs para mais detalhes.\n\n" +
                   "Tente novamente ou entre em contato com o suporte.";
        }

        private void PlayExtractionCompleteSound()
        {
            // Play a pleasant completion sound using system beeps
            try
            {
                // Create a simple pleasant melody: two ascending beeps
                SystemSounds.Beep.Play();
                Task.Delay(200).Wait();
                SystemSounds.Beep.Play();
            }
            catch
            {
                // If system sounds fail, try alternative approach
                try
                {
                    // Use console beep as fallback
                    System.Console.Beep(800, 150);
                    Task.Delay(100).Wait();
                    System.Console.Beep(1000, 150);
                }
                catch
                {
                    // Silent fallback - no sound
                }
            }
        }

        #endregion
    }
}

