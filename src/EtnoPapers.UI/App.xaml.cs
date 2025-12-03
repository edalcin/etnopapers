using System.Windows;
using EtnoPapers.Core.Services;
using EtnoPapers.UI.Views;

namespace EtnoPapers.UI
{
    /// <summary>
    /// Application entry point with dependency injection setup.
    /// Optimized for startup performance using lazy initialization of non-critical services.
    /// </summary>
    public partial class App : Application
    {
        private Lazy<ConfigurationService> _configService;
        private Lazy<DataStorageService> _storageService;
        private LoggerService _loggerService;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Initialize logger immediately (needed for all other logging)
            _loggerService = new LoggerService();

            _loggerService.Info("EtnoPapers application started");

            // Initialize services lazily (T073: Startup optimization)
            // ConfigurationService: lazy-loaded on first access
            _configService = new Lazy<ConfigurationService>(() => new ConfigurationService());

            // DataStorageService: lazy-loaded with deferred initialization
            _storageService = new Lazy<DataStorageService>(() =>
            {
                try
                {
                    var storage = new DataStorageService();
                    storage.Initialize();
                    _loggerService.Info($"Data storage initialized. Records count: {storage.Count()}");
                    return storage;
                }
                catch (Exception ex)
                {
                    _loggerService.Error("Failed to initialize data storage", ex);
                    throw;
                }
            });

            // Register services in application resources for dependency injection
            // Services are accessed through Lazy<T> - actual initialization occurs on first use
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

