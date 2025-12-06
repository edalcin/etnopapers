using System.Windows;
using System.Windows.Controls;
using EtnoPapers.UI.ViewModels;

namespace EtnoPapers.UI.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// Main application window with navigation and status bar.
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();

            // Set DataContext to ViewModel
            _viewModel = new MainWindowViewModel();
            DataContext = _viewModel;

            System.Diagnostics.Debug.WriteLine("MainWindow DataContext set: " + (DataContext != null ? "OK" : "NULL"));
            System.Diagnostics.Debug.WriteLine("NavigateCommand: " + (_viewModel.NavigateCommand != null ? "OK" : "NULL"));

            // Restore window state if previously saved
            RestoreWindowState();

            // Carregar página inicial
            NavigateToPage("Home");

            // Subscribe to Loaded event to check connections
            Loaded += (s, e) => _viewModel.CheckConnections();
        }

        /// <summary>
        /// Public method to allow other views/viewmodels to request connection check
        /// </summary>
        public void RefreshConnectionStatus()
        {
            _viewModel?.CheckConnections();
        }

        private void OnMenuExit(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void OnMenuSettings(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as MainWindowViewModel;
            viewModel?.NavigateCommand.Execute("Settings");
        }

        private void OnMenuAbout(object sender, RoutedEventArgs e)
        {
            var aboutWindow = new AboutWindow
            {
                Owner = this
            };
            aboutWindow.ShowDialog();
        }

        private void OnNavigationButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                System.Diagnostics.Debug.WriteLine("Button clicked: " + btn.Name);

                // Navegar para página apropriada
                string pageName = btn.Name switch
                {
                    "BtnHome" => "Home",
                    "BtnUpload" => "Upload",
                    "BtnRecords" => "Records",
                    "BtnSync" => "Sync",
                    "BtnSettings" => "Settings",
                    _ => "Home"
                };

                NavigateToPage(pageName);
            }
        }

        private void NavigateToPage(string pageName)
        {
            System.Diagnostics.Debug.WriteLine($"[NavigateToPage] Starting navigation to {pageName}");
            try
            {
                // Mapear nome da página para arquivo XAML
                string pageUri = pageName switch
                {
                    "Home" => "/Views/HomePage.xaml",
                    "Upload" => "/Views/UploadPage.xaml",
                    "Records" => "/Views/RecordsPage.xaml",
                    "Sync" => "/Views/SyncPage.xaml",
                    "Settings" => "/Views/SettingsPage.xaml",
                    _ => "/Views/HomePage.xaml"
                };

                System.Diagnostics.Debug.WriteLine($"[NavigateToPage] Page URI: {pageUri}");

                // Clear any previous navigation state
                MainFrame.Source = null;
                System.Diagnostics.Debug.WriteLine($"[NavigateToPage] Cleared frame");

                // Create and navigate to page
                MainFrame.Source = new System.Uri(pageUri, System.UriKind.Relative);
                System.Diagnostics.Debug.WriteLine($"[NavigateToPage] Navigation succeeded for: {pageName}");
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[NavigateToPage] CRITICAL ERROR: {ex.GetType().Name}: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[NavigateToPage] Stack trace: {ex.StackTrace}");

                try
                {
                    MessageBox.Show(
                        $"Erro ao navegar para {pageName}:\n\n{ex.GetType().Name}: {ex.Message}\n\nStack trace:\n{ex.StackTrace}",
                        "Erro de Navegação",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }
                catch
                {
                    // Fallback if MessageBox also fails
                }
            }
        }

        private void RestoreWindowState()
        {
            // Placeholder for window state restoration (size, position, etc.)
            // Will be implemented in full version
        }

        protected override void OnClosed(System.EventArgs e)
        {
            // Save window state before closing
            // Will be implemented in full version
            base.OnClosed(e);
        }
    }
}