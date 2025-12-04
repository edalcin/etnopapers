using System.Windows.Controls;
using EtnoPapers.UI.ViewModels;

namespace EtnoPapers.UI.Views
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// Provides configuration UI for OLLAMA, MongoDB, and application settings.
    /// </summary>
    public partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            try
            {
                InitializeComponent();
                DataContext = new SettingsViewModel();
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
    }
}
