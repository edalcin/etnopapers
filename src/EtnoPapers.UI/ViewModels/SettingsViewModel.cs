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
    /// ViewModel for managing application settings and configuration.
    /// Handles Cloud AI provider, MongoDB, and application preference settings.
    /// </summary>
    public class SettingsViewModel : ViewModelBase
    {
        private readonly ConfigurationService _configService;
        private readonly MongoDBSyncService _mongodbService;
        private readonly LoggerService _loggerService;

        // Cloud AI Provider Settings
        private int _selectedProviderIndex = -1;
        private string _apiKey = "";
        private string _maskedApiKey = "";
        private string _providerTestStatus = "";
        private string _customExtractionPrompt = "";

        // Legacy OLLAMA Settings (kept for backward compatibility, not displayed in UI)
        private string _ollamaUrl = "http://localhost:11434";
        private string _ollamaModel = "llama2";
        private string _ollamaPrompt = "";

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

        // Migration Banner
        private bool _showMigrationBanner = false;

        public SettingsViewModel()
        {
            _configService = new ConfigurationService();
            _mongodbService = new MongoDBSyncService();
            _loggerService = new LoggerService();

            LoadSettingsCommand = new RelayCommand(_ => LoadSettings());
            SaveSettingsCommand = new RelayCommand(_ => SaveSettings(), _ => !IsSaving);
            ResetToDefaultsCommand = new RelayCommand(_ => ResetToDefaults());
            TestMongodbCommand = new AsyncRelayCommand(_ => TestMongodbConnectionAsync());
            ClearErrorCommand = new RelayCommand(_ => ClearError());
            DismissMigrationBannerCommand = new RelayCommand(_ => DismissMigrationBanner());

            _loggerService.Info("SettingsViewModel initialized");
            LoadSettings();
        }

        #region Cloud AI Provider Properties

        /// <summary>
        /// Selected AI provider index (0=Gemini, 1=OpenAI, 2=Anthropic).
        /// -1 means no provider selected.
        /// </summary>
        public int SelectedProviderIndex
        {
            get => _selectedProviderIndex;
            set
            {
                if (SetProperty(ref _selectedProviderIndex, value))
                {
                    OnProviderChanged();
                }
            }
        }

        /// <summary>
        /// API key for the selected provider (stored encrypted).
        /// </summary>
        public string ApiKey
        {
            get => _apiKey;
            set
            {
                if (SetProperty(ref _apiKey, value))
                {
                    UpdateMaskedApiKey();
                }
            }
        }

        /// <summary>
        /// Masked version of API key for display (e.g., "••••abcd").
        /// </summary>
        public string MaskedApiKey
        {
            get => _maskedApiKey;
            private set => SetProperty(ref _maskedApiKey, value);
        }

        /// <summary>
        /// Status message for provider configuration/testing.
        /// </summary>
        public string ProviderTestStatus
        {
            get => _providerTestStatus;
            set => SetProperty(ref _providerTestStatus, value);
        }

        /// <summary>
        /// Custom extraction prompt for LLM (overrides default prompt if provided).
        /// </summary>
        public string CustomExtractionPrompt
        {
            get => _customExtractionPrompt;
            set => SetProperty(ref _customExtractionPrompt, value);
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
            "pt-BR"
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

        /// <summary>
        /// Shows migration banner when legacy OLLAMA config is detected.
        /// </summary>
        public bool ShowMigrationBanner
        {
            get => _showMigrationBanner;
            set => SetProperty(ref _showMigrationBanner, value);
        }

        #endregion

        #region Commands

        public ICommand LoadSettingsCommand { get; }
        public ICommand SaveSettingsCommand { get; }
        public ICommand ResetToDefaultsCommand { get; }
        public ICommand TestMongodbCommand { get; }
        public ICommand ClearErrorCommand { get; }
        public ICommand DismissMigrationBannerCommand { get; }

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

                // Load cloud AI provider settings
                if (config?.AIProvider.HasValue == true)
                {
                    SelectedProviderIndex = (int)config.AIProvider.Value;
                }
                else
                {
                    SelectedProviderIndex = -1;
                }

                ApiKey = config?.ApiKey ?? "";
                CustomExtractionPrompt = config?.CustomExtractionPrompt ?? "";

                // Load legacy OLLAMA settings (for backward compatibility, not displayed)
                _ollamaUrl = config?.OllamaUrl ?? "http://localhost:11434";
                _ollamaModel = config?.OllamaModel ?? "llama2";
                _ollamaPrompt = config?.OllamaPrompt ?? "";

                // Load other settings
                MongodbUri = config?.MongodbUri ?? "";
                Language = config?.Language ?? "pt-BR";
                WindowWidth = config?.WindowWidth ?? 1200;
                WindowHeight = config?.WindowHeight ?? 800;
                WindowMaximized = config?.WindowMaximized ?? false;

                // Detect legacy OLLAMA configuration
                DetectLegacyOllamaConfig(config);

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
                // Validate cloud AI provider settings
                if (SelectedProviderIndex >= 0)
                {
                    if (string.IsNullOrWhiteSpace(ApiKey))
                    {
                        throw new Exception("A chave de API é obrigatória quando um provedor está selecionado.");
                    }
                }

                var config = _configService.LoadConfiguration();

                // Save cloud AI provider settings
                if (SelectedProviderIndex >= 0)
                {
                    config.AIProvider = (AIProviderType)SelectedProviderIndex;
                    config.ApiKey = ApiKey.Trim();
                }
                else
                {
                    config.AIProvider = null;
                    config.ApiKey = null;
                }

                config.CustomExtractionPrompt = CustomExtractionPrompt?.Trim() ?? "";

                // Save legacy OLLAMA settings (for backward compatibility)
                config.OllamaUrl = _ollamaUrl;
                config.OllamaModel = _ollamaModel;
                config.OllamaPrompt = _ollamaPrompt;

                // Save other settings
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
            SelectedProviderIndex = -1;
            ApiKey = "";
            MongodbUri = "";
            Language = "pt-BR";
            WindowWidth = 1200;
            WindowHeight = 800;
            WindowMaximized = false;

            IsMongodbConnected = false;
            MongodbTestStatus = "";
            ProviderTestStatus = "";

            ClearError();
            _loggerService.Info("Settings reset to defaults");
        }

        /// <summary>
        /// Called when provider selection changes. Clears API key field.
        /// </summary>
        private void OnProviderChanged()
        {
            // Clear API key when provider changes to avoid confusion
            ApiKey = "";
            ProviderTestStatus = "";
            ClearError();

            var providerName = SelectedProviderIndex switch
            {
                0 => "Google Gemini",
                1 => "OpenAI",
                2 => "Anthropic Claude",
                _ => "nenhum"
            };

            _loggerService.Info($"AI provider changed to: {providerName}");
        }

        /// <summary>
        /// Updates the masked API key for display.
        /// Shows last 4 characters as "••••abcd" or empty if key is empty.
        /// </summary>
        private void UpdateMaskedApiKey()
        {
            if (string.IsNullOrEmpty(ApiKey))
            {
                MaskedApiKey = "";
            }
            else if (ApiKey.Length <= 4)
            {
                MaskedApiKey = "Chave atual: " + new string('•', ApiKey.Length);
            }
            else
            {
                var lastFour = ApiKey.Substring(ApiKey.Length - 4);
                MaskedApiKey = "Chave atual: ••••" + lastFour;
            }
        }

        /// <summary>
        /// Tests connection to MongoDB service.
        /// </summary>
        public async Task TestMongodbConnectionAsync()
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

                // Save MongoDB URI before testing so MainWindow can find it
                try
                {
                    var config = _configService.LoadConfiguration();
                    config.MongodbUri = MongodbUri.Trim();
                    _configService.SaveConfiguration(config);
                }
                catch { /* Ignore save errors, test anyway */ }

                var isConnected = await _mongodbService.TestConnectionAsync(MongodbUri);

                if (isConnected)
                {
                    IsMongodbConnected = true;
                    MongodbTestStatus = "✓ Conectado ao MongoDB";
                    _loggerService.Info("MongoDB connection test successful");

                    // Update MainWindow connection status
                    var mainWindow = System.Windows.Application.Current.MainWindow as Views.MainWindow;
                    mainWindow?.RefreshConnectionStatus();
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

        /// <summary>
        /// Detects if the configuration contains legacy OLLAMA settings
        /// and shows migration banner if needed.
        /// </summary>
        private void DetectLegacyOllamaConfig(Configuration config)
        {
            if (config == null) return;

            if (config.IsLegacyOllamaConfig)
            {
                ShowMigrationBanner = true;
                _loggerService.Info("Legacy OLLAMA configuration detected - showing migration banner");
            }
            else
            {
                ShowMigrationBanner = false;
            }
        }

        /// <summary>
        /// Dismisses the migration banner for this session.
        /// </summary>
        private void DismissMigrationBanner()
        {
            ShowMigrationBanner = false;
            _loggerService.Info("Migration banner dismissed");
        }

        #endregion
    }
}
