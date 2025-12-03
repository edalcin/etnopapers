using System.Windows;
using EtnoPapers.Core.Services;
using EtnoPapers.UI.Views;

namespace EtnoPapers.UI
{
    /// <summary>
    /// Application entry point with dependency injection setup.
    /// </summary>
    public partial class App : Application
    {
        private ConfigurationService _configService;
        private DataStorageService _storageService;
        private LoggerService _loggerService;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Initialize core services
            _loggerService = new LoggerService();
            _configService = new ConfigurationService();
            _storageService = new DataStorageService();

            _loggerService.Info("EtnoPapers application started");

            try
            {
                // Initialize storage
                _storageService.Initialize();
                _loggerService.Info($"Data storage initialized. Records count: {_storageService.Count()}");
            }
            catch (Exception ex)
            {
                _loggerService.Error("Failed to initialize data storage", ex);
            }

            // Register services in application resources for dependency injection
            Resources["ConfigurationService"] = _configService;
            Resources["StorageService"] = _storageService;
            Resources["LoggerService"] = _loggerService;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _loggerService?.Info("EtnoPapers application closing");
            base.OnExit(e);
        }
    }
}

