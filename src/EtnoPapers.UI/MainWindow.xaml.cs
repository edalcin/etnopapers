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
        public MainWindow()
        {
            InitializeComponent();

            // Set DataContext to ViewModel
            var viewModel = new MainWindowViewModel();
            DataContext = viewModel;

            System.Diagnostics.Debug.WriteLine("MainWindow DataContext set: " + (DataContext != null ? "OK" : "NULL"));
            System.Diagnostics.Debug.WriteLine("NavigateCommand: " + (viewModel.NavigateCommand != null ? "OK" : "NULL"));

            // Restore window state if previously saved
            RestoreWindowState();
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
            MessageBox.Show(
                "EtnoPapers v1.0.0\n\nGerenciador de Pesquisa Etnobotânica\n\nAplicativo Windows desktop para extração e catalogação automatizada de metadados etnobotânicos de artigos científicos.",
                "Sobre o EtnoPapers",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void OnNavigationButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                System.Diagnostics.Debug.WriteLine("Button clicked: " + btn.Name);
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