using System;
using System.Windows.Controls;
using EtnoPapers.UI.ViewModels;

namespace EtnoPapers.UI.Views
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// Provides configuration UI for Cloud AI providers, MongoDB, and application settings.
    /// </summary>
    public partial class SettingsPage : Page
    {
        private SettingsViewModel ViewModel => DataContext as SettingsViewModel;

        public SettingsPage()
        {
            try
            {
                InitializeComponent();
                DataContext = new SettingsViewModel();

                // Initialize PasswordBox with masked API key after ViewModel loads
                if (ViewModel != null)
                {
                    Loaded += (s, e) =>
                    {
                        // If there's an existing API key, show placeholder dots (actual key is in ViewModel)
                        if (!string.IsNullOrEmpty(ViewModel.ApiKey))
                        {
                            // Set placeholder password to indicate there's a saved key
                            ApiKeyPasswordBox.Password = "••••••••";
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing SettingsPage: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                // Create a simple error page instead of crashing
                var errorPanel = new StackPanel { Margin = new System.Windows.Thickness(20) };
                errorPanel.Children.Add(new TextBlock
                {
                    Text = "Erro ao carregar Configurações",
                    FontSize = 16,
                    FontWeight = System.Windows.FontWeights.Bold,
                    Foreground = System.Windows.Media.Brushes.Red,
                    Margin = new System.Windows.Thickness(0, 0, 0, 10)
                });
                errorPanel.Children.Add(new TextBlock
                {
                    Text = ex.Message,
                    TextWrapping = System.Windows.TextWrapping.Wrap,
                    Foreground = System.Windows.Media.Brushes.DarkRed
                });
                Content = errorPanel;
            }
        }

        /// <summary>
        /// Handles PasswordBox text changes and updates ViewModel.
        /// </summary>
        private void ApiKeyPasswordBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ViewModel != null && sender is PasswordBox passwordBox)
            {
                // Don't update if it's just the placeholder dots
                if (passwordBox.Password != "••••••••")
                {
                    ViewModel.ApiKey = passwordBox.Password;
                }
            }
        }
    }
}
