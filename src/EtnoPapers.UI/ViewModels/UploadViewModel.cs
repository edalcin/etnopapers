using System;
using System.Collections.ObjectModel;
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
        private readonly OLLAMAService _ollamaService;
        private readonly ValidationService _validationService;
        private readonly LoggerService _loggerService;
        private readonly ConfigurationService _configService;
        private RelayCommand _startExtractionCommand;

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
            _pdfService = new PDFProcessingService();
            _configService = new ConfigurationService();
            _validationService = new ValidationService();
            _loggerService = new LoggerService();

            // Load OLLAMA configuration
            var config = _configService.LoadConfiguration();
            var ollamaUrl = config?.OllamaUrl ?? "http://localhost:11434";
            var ollamaModel = config?.OllamaModel ?? "llama2";

            _ollamaService = new OLLAMAService(ollamaUrl, ollamaModel);
            _extractionService = new ExtractionPipelineService(
                _pdfService,
                _ollamaService,
                _validationService,
                _storageService);

            SelectFileCommand = new RelayCommand(_ => SelectFile());
            _startExtractionCommand = new RelayCommand(_ => StartExtraction(), _ => CanStartExtraction());
            StartExtractionCommand = _startExtractionCommand;
            CancelExtractionCommand = new RelayCommand(_ => CancelExtraction(), _ => IsExtracting);
            SaveResultsCommand = new RelayCommand(_ => SaveResults(), _ => AllowSave && !IsExtracting);
            ClearCommand = new RelayCommand(_ => Clear());

            _loggerService.Info($"UploadViewModel initialized with OLLAMA: {ollamaUrl}, Model: {ollamaModel}");
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
                    ExtractedData = await _extractionService.ExtractFromPdfAsync(SelectedFilePath);

                    ExtractionProgress = 100;
                    CurrentStep = "Extração concluída";

                    // Check if we have a partial record that needs manual editing
                    if (ExtractedData != null && !_validationService.IsValidForSaving(ExtractedData))
                    {
                        CurrentStep = "Edição de Campos Faltantes";

                        if (progressViewModel != null)
                        {
                            progressViewModel.IsExtracting = false;
                        }

                        // Close progress window safely on UI thread
                        await Task.Delay(500);
                        System.Windows.Application.Current?.Dispatcher?.Invoke(() =>
                        {
                            try
                            {
                                progressWindow?.Close();
                            }
                            catch (Exception ex)
                            {
                                _loggerService.Error($"Error closing progress window: {ex.Message}", ex);
                            }
                        });

                        // Open edit dialog for user to fill missing fields
                        var editDialog = new Views.EditRecordDialog(ExtractedData)
                        {
                            Owner = System.Windows.Application.Current.MainWindow
                        };

                        if (editDialog.ShowDialog() == true)
                        {
                            // User saved the edited record
                            AllowSave = true;
                            CurrentStep = "Pronto para salvar";
                            _loggerService.Info($"Record edited and ready to save: {ExtractedData.Id}");
                        }
                    }
                    else
                    {
                        AllowSave = true;
                        _loggerService.Info($"Extraction completed successfully for: {System.IO.Path.GetFileName(SelectedFilePath)}");

                        // Close progress window safely on UI thread after a short delay
                        await Task.Delay(500);
                        System.Windows.Application.Current?.Dispatcher?.Invoke(() =>
                        {
                            try
                            {
                                progressWindow?.Close();
                            }
                            catch (Exception ex)
                            {
                                _loggerService.Error($"Error closing progress window: {ex.Message}", ex);
                            }
                        });
                    }
                }
                catch (Exception ex)
                {
                    if (progressViewModel != null)
                    {
                        progressViewModel.IsExtracting = false;
                    }

                    HasExtractionError = true;
                    ErrorMessage = $"Erro na extração: {ex.Message}";
                    CurrentStep = "Erro";
                    _loggerService.Error($"Extraction failed: {ex.Message}", ex);

                    // Keep progress window open to show error log
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

        #endregion
    }
}
