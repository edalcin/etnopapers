using System.Windows;
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
            DataContext = new MainWindowViewModel();

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
                "EtnoPapers v1.0.0\n\nEthnobotanical Research Manager\n\nA Windows desktop application for automated extraction and cataloging of ethnobotanical metadata from scientific papers.",
                "About EtnoPapers",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
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