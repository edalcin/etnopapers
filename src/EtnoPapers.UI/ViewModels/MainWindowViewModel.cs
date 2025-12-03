using System;
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
        private string _currentPage = "Home";
        private bool _ollamaConnected = false;
        private bool _mongodbConnected = false;
        private string _applicationTitle = "EtnoPapers - Gerenciador de Pesquisa Etnobotânica";

        public MainWindowViewModel()
        {
            // Placeholder - services will be injected from App.xaml resources
            _configService = new ConfigurationService();
            _loggerService = new LoggerService();

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

        public bool OllamaConnected
        {
            get => _ollamaConnected;
            set => SetProperty(ref _ollamaConnected, value);
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

        public string Version => "1.0.0";

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

        public void CheckConnections()
        {
            // Will be implemented to check OLLAMA and MongoDB connections
            // Called periodically or on demand
        }

        #endregion
    }
}
