using System;
using System.Threading.Tasks;
using System.Windows.Input;
using EtnoPapers.Core.Services;
using EtnoPapers.UI.Commands;

namespace EtnoPapers.UI.ViewModels
{
    /// <summary>
    /// ViewModel for the main application window.
    /// Manages navigation, connection status, and overall application state.
    /// </summary>
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly ConfigurationService _configService;
        private readonly LoggerService _loggerService;
        private readonly MongoDBSyncService _mongodbService;
        private string _currentPage = "Home";
        private bool _cloudAIConfigured = false;
        private bool _mongodbConnected = false;
        private string _applicationTitle = "EtnoPapers - Gerenciador de Pesquisa Etnobotânica";

        public MainWindowViewModel()
        {
            // Placeholder - services will be injected from App.xaml resources
            _configService = new ConfigurationService();
            _loggerService = new LoggerService();
            _mongodbService = new MongoDBSyncService();

            _loggerService.Info("MainWindowViewModel initialized");

            // Initialize commands
            NavigateCommand = new RelayCommand(Navigate);
        }

        #region Properties

        public string CurrentPage
        {
            get => _currentPage;
            set => SetProperty(ref _currentPage, value);
        }

        public bool CloudAIConfigured
        {
            get => _cloudAIConfigured;
            set => SetProperty(ref _cloudAIConfigured, value);
        }

        public bool MongodbConnected
        {
            get => _mongodbConnected;
            set => SetProperty(ref _mongodbConnected, value);
        }

        public string ApplicationTitle
        {
            get => _applicationTitle;
            set => SetProperty(ref _applicationTitle, value);
        }

        public string Version => "2.1.0";

        #endregion

        #region Commands

        public ICommand NavigateCommand { get; }

        #endregion

        #region Methods

        private void Navigate(object parameter)
        {
            if (parameter is string pageName)
            {
                CurrentPage = pageName;
                _loggerService.Info($"Navigated to page: {pageName}");
            }
        }

        public async void CheckConnections()
        {
            try
            {
                // Small delay to ensure file system writes are complete
                await Task.Delay(100);

                // Check Cloud AI configuration
                var config = _configService.LoadConfiguration();
                CloudAIConfigured = config?.IsCloudAIConfigured ?? false;
                _loggerService.Info($"Cloud AI configured: {CloudAIConfigured}, Provider: {config?.AIProvider}");

                // Check MongoDB connection if URI is configured
                if (!string.IsNullOrWhiteSpace(config?.MongodbUri))
                {
                    var mongodbConnected = await _mongodbService.TestConnectionAsync(config.MongodbUri);
                    MongodbConnected = mongodbConnected;
                    _loggerService.Info($"MongoDB connection test: {mongodbConnected}, URI: {config.MongodbUri}");
                }
                else
                {
                    MongodbConnected = false;
                    _loggerService.Info("MongoDB URI not configured");
                }

                _loggerService.Info($"Connection check completed - Cloud AI: {CloudAIConfigured}, MongoDB: {MongodbConnected}");
            }
            catch (Exception ex)
            {
                _loggerService.Error($"Error checking connections: {ex.Message}", ex);
            }
        }

        #endregion
    }
}
