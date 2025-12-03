using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using EtnoPapers.Core.Services;
using EtnoPapers.UI.Commands;

namespace EtnoPapers.UI.ViewModels
{
    /// <summary>
    /// ViewModel for managing application settings and configuration.
    /// Handles OLLAMA, MongoDB, and application preference settings.
    /// </summary>
    public class SettingsViewModel : ViewModelBase
    {
        private readonly ConfigurationService _configService;
        private readonly OLLAMAService _ollamaService;
        private readonly MongoDBSyncService _mongodbService;
        private readonly LoggerService _loggerService;

        // OLLAMA Settings
        private string _ollamaUrl = "http://localhost:11434";
        private string _ollamaModel = "llama2";
        private string _ollamaPrompt = "";
        private bool _isOllamaConnected = false;
        private string _ollamaTestStatus = "";

        // MongoDB Settings
        private string _mongodbUri = "";
        private bool _isMongodbConnected = false;
        private string _mongodbTestStatus = "";

        // Application Settings
        private string _language = "pt-BR";
        private int _windowWidth = 1200;
        private int _windowHeight = 800;
        private bool _windowMaximized = false;

        // General State
        private bool _isSaving = false;
        private bool _hasError = false;
        private string _errorMessage = "";
        private string _successMessage = "";

        public SettingsViewModel()
        {
            _configService = new ConfigurationService();
            _ollamaService = new OLLAMAService();
            _mongodbService = new MongoDBSyncService();
            _loggerService = new LoggerService();

            LoadSettingsCommand = new RelayCommand(_ => LoadSettings());
            SaveSettingsCommand = new RelayCommand(_ => SaveSettings(), _ => !IsSaving);
            ResetToDefaultsCommand = new RelayCommand(_ => ResetToDefaults());
            TestOllamaCommand = new RelayCommand(_ => TestOllamaConnection());
            TestMongodbCommand = new RelayCommand(_ => TestMongodbConnection());
            ClearErrorCommand = new RelayCommand(_ => ClearError());

            _loggerService.Info("SettingsViewModel initialized");
            LoadSettings();
        }

        #region OLLAMA Properties

        public string OllamaUrl
        {
            get => _ollamaUrl;
            set => SetProperty(ref _ollamaUrl, value);
        }

        public string OllamaModel
        {
            get => _ollamaModel;
            set => SetProperty(ref _ollamaModel, value);
        }

        public string OllamaPrompt
        {
            get => _ollamaPrompt;
            set => SetProperty(ref _ollamaPrompt, value);
        }

        public bool IsOllamaConnected
        {
            get => _isOllamaConnected;
            set => SetProperty(ref _isOllamaConnected, value);
        }

        public string OllamaTestStatus
        {
            get => _ollamaTestStatus;
            set => SetProperty(ref _ollamaTestStatus, value);
        }

        #endregion

        #region MongoDB Properties

        public string MongodbUri
        {
            get => _mongodbUri;
            set => SetProperty(ref _mongodbUri, value);
        }

        public bool IsMongodbConnected
        {
            get => _isMongodbConnected;
            set => SetProperty(ref _isMongodbConnected, value);
        }

        public string MongodbTestStatus
        {
            get => _mongodbTestStatus;
            set => SetProperty(ref _mongodbTestStatus, value);
        }

        #endregion

        #region Application Properties

        public ObservableCollection<string> AvailableLanguages { get; } = new()
        {
            "pt-BR",
            "en-US"
        };

        public string Language
        {
            get => _language;
            set => SetProperty(ref _language, value);
        }

        public int WindowWidth
        {
            get => _windowWidth;
            set => SetProperty(ref _windowWidth, value);
        }

        public int WindowHeight
        {
            get => _windowHeight;
            set => SetProperty(ref _windowHeight, value);
        }

        public bool WindowMaximized
        {
            get => _windowMaximized;
            set => SetProperty(ref _windowMaximized, value);
        }

        #endregion

        #region General State

        public bool IsSaving
        {
            get => _isSaving;
            set => SetProperty(ref _isSaving, value);
        }

        public bool HasError
        {
            get => _hasError;
            set => SetProperty(ref _hasError, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public string SuccessMessage
        {
            get => _successMessage;
            set => SetProperty(ref _successMessage, value);
        }

        #endregion

        #region Commands

        public ICommand LoadSettingsCommand { get; }
        public ICommand SaveSettingsCommand { get; }
        public ICommand ResetToDefaultsCommand { get; }
        public ICommand TestOllamaCommand { get; }
        public ICommand TestMongodbCommand { get; }
        public ICommand ClearErrorCommand { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Loads current settings from configuration file.
        /// </summary>
        public void LoadSettings()
        {
            try
            {
                var config = _configService.LoadConfiguration();

                OllamaUrl = config?.OllamaUrl ?? "http://localhost:11434";
                OllamaModel = config?.OllamaModel ?? "llama2";
                OllamaPrompt = config?.OllamaPrompt ?? "";
                MongodbUri = config?.MongodbUri ?? "";
                Language = config?.Language ?? "pt-BR";
                WindowWidth = config?.WindowWidth ?? 1200;
                WindowHeight = config?.WindowHeight ?? 800;
                WindowMaximized = config?.WindowMaximized ?? false;

                ClearError();
                _loggerService.Info("Settings loaded successfully");
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = $"Erro ao carregar configurações: {ex.Message}";
                _loggerService.Error($"Error loading settings: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Saves current settings to configuration file.
        /// </summary>
        public void SaveSettings()
        {
            if (IsSaving)
                return;

            IsSaving = true;
            ClearError();

            try
            {
                // Validate settings
                if (string.IsNullOrWhiteSpace(OllamaUrl))
                {
                    throw new Exception("A URL do OLLAMA é obrigatória.");
                }

                if (string.IsNullOrWhiteSpace(OllamaModel))
                {
                    throw new Exception("O modelo do OLLAMA é obrigatório.");
                }

                var config = _configService.LoadConfiguration();
                config.OllamaUrl = OllamaUrl.Trim();
                config.OllamaModel = OllamaModel.Trim();
                config.OllamaPrompt = OllamaPrompt?.Trim() ?? "";
                config.MongodbUri = MongodbUri?.Trim() ?? "";
                config.Language = Language;
                config.WindowWidth = WindowWidth;
                config.WindowHeight = WindowHeight;
                config.WindowMaximized = WindowMaximized;

                _configService.SaveConfiguration(config);

                SuccessMessage = "✓ Configurações salvas com sucesso!";
                _loggerService.Info("Settings saved successfully");
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = $"Erro ao salvar configurações: {ex.Message}";
                _loggerService.Error($"Error saving settings: {ex.Message}", ex);
            }
            finally
            {
                IsSaving = false;
            }
        }

        /// <summary>
        /// Resets all settings to default values.
        /// </summary>
        public void ResetToDefaults()
        {
            OllamaUrl = "http://localhost:11434";
            OllamaModel = "llama2";
            OllamaPrompt = "";
            MongodbUri = "";
            Language = "pt-BR";
            WindowWidth = 1200;
            WindowHeight = 800;
            WindowMaximized = false;

            IsOllamaConnected = false;
            IsMongodbConnected = false;
            OllamaTestStatus = "";
            MongodbTestStatus = "";

            ClearError();
            _loggerService.Info("Settings reset to defaults");
        }

        /// <summary>
        /// Tests connection to OLLAMA service.
        /// </summary>
        public async void TestOllamaConnection()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(OllamaUrl))
                {
                    OllamaTestStatus = "URL do OLLAMA não configurada";
                    IsOllamaConnected = false;
                    return;
                }

                OllamaTestStatus = "Testando...";

                // Create a temporary OLLAMAService with the configured URL
                var tempService = new OLLAMAService(OllamaUrl, OllamaModel);
                var isConnected = await tempService.CheckHealthAsync();

                if (isConnected)
                {
                    IsOllamaConnected = true;
                    OllamaTestStatus = "✓ Conectado ao OLLAMA";
                    _loggerService.Info("OLLAMA connection test successful");
                }
                else
                {
                    IsOllamaConnected = false;
                    OllamaTestStatus = "✗ Erro ao conectar ao OLLAMA";
                    _loggerService.Warn("OLLAMA connection test failed");
                }
            }
            catch (Exception ex)
            {
                IsOllamaConnected = false;
                OllamaTestStatus = $"✗ Erro: {ex.Message}";
                _loggerService.Error($"OLLAMA connection test error: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Tests connection to MongoDB service.
        /// </summary>
        public async void TestMongodbConnection()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(MongodbUri))
                {
                    MongodbTestStatus = "URI do MongoDB não configurada";
                    IsMongodbConnected = false;
                    return;
                }

                MongodbTestStatus = "Testando...";

                var isConnected = await _mongodbService.TestConnectionAsync(MongodbUri);

                if (isConnected)
                {
                    IsMongodbConnected = true;
                    MongodbTestStatus = "✓ Conectado ao MongoDB";
                    _loggerService.Info("MongoDB connection test successful");
                }
                else
                {
                    IsMongodbConnected = false;
                    MongodbTestStatus = "✗ Erro ao conectar ao MongoDB";
                    _loggerService.Warn("MongoDB connection test failed");
                }
            }
            catch (Exception ex)
            {
                IsMongodbConnected = false;
                MongodbTestStatus = $"✗ Erro: {ex.Message}";
                _loggerService.Error($"MongoDB connection test error: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Clears error and success messages.
        /// </summary>
        private void ClearError()
        {
            HasError = false;
            ErrorMessage = "";
            SuccessMessage = "";
        }

        #endregion
    }
}
