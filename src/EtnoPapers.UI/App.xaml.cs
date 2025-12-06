using System.Windows;
using System.IO;
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

        public App()
        {
            // Add global exception handlers BEFORE startup
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                Exception ex = (Exception)e.ExceptionObject;
                LogException("AppDomain", ex);

                try
                {
                    MessageBox.Show(
                        $"[AppDomain] {ex?.GetType().Name}:\n\n{ex?.Message}\n\nStack Trace:\n{ex?.StackTrace}",
                        "CRITICAL ERROR",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }
                catch { }
            };

            DispatcherUnhandledException += (sender, e) =>
            {
                LogException("Dispatcher", e.Exception);

                try
                {
                    MessageBox.Show(
                        $"[Dispatcher] {e.Exception?.GetType().Name}:\n\n{e.Exception?.Message}\n\nStack Trace:\n{e.Exception?.StackTrace}",
                        "ERROR",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }
                catch { }

                e.Handled = true;  // Mark as handled to prevent crash
            };

            Dispatcher.UnhandledException += (sender, e) =>
            {
                LogException("DispatcherEvent", e.Exception);

                try
                {
                    MessageBox.Show(
                        $"[DispatcherEvent] {e.Exception?.GetType().Name}:\n\n{e.Exception?.Message}\n\nStack Trace:\n{e.Exception?.StackTrace}",
                        "ERROR",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }
                catch { }

                e.Handled = true;  // Mark as handled to prevent crash
            };
        }

        private void LogException(string source, Exception ex)
        {
            try
            {
                string logPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "EtnoPapers",
                    "exception.log"
                );
                Directory.CreateDirectory(Path.GetDirectoryName(logPath));
                string message = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {source}: {ex?.GetType().Name}\n{ex?.Message}\n{ex?.StackTrace}\n\n";
                File.AppendAllText(logPath, message);
            }
            catch { }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                // Write error to temporary file since logging might have failed
                try
                {
                    string tempLogPath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "EtnoPapers",
                        "startup-error.log"
                    );
                    Directory.CreateDirectory(Path.GetDirectoryName(tempLogPath));
                    File.WriteAllText(tempLogPath, $"Startup Error: {ex.Message}\n{ex.StackTrace}");
                }
                catch { }

                // Show error to user
                MessageBox.Show($"Failed to start application:\n\n{ex.Message}", "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(1);
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _loggerService?.Info("EtnoPapers application closing");
            base.OnExit(e);
        }
    }
}

