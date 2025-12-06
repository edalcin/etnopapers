using System;
using System.Windows;
using System.Windows.Controls;
using EtnoPapers.Core.Services;
using EtnoPapers.UI.ViewModels;

namespace EtnoPapers.UI.Views
{
    /// <summary>
    /// Interaction logic for SyncPage.xaml
    /// Manages MongoDB synchronization of local records.
    /// </summary>
    public partial class SyncPage : Page
    {
        private SyncViewModel? _viewModel;
        private readonly LoggerService _logger = new LoggerService();

        public SyncPage()
        {
            try
            {
                _logger.Info("SyncPage constructor started");
                InitializeComponent();
                _logger.Info("InitializeComponent completed");

                _viewModel = new SyncViewModel();
                _logger.Info("SyncViewModel created successfully");

                DataContext = _viewModel;
                _logger.Info("DataContext set to SyncViewModel");

                _viewModel.LoadAvailableRecords();
                _logger.Info("LoadAvailableRecords called successfully");
            }
            catch (Exception ex)
            {
                _logger.Error($"CRITICAL ERROR in SyncPage constructor: {ex.GetType().Name}: {ex.Message}", ex);
                _logger.Error($"Stack trace: {ex.StackTrace}");

                // Try to show error to user
                try
                {
                    MessageBox.Show(
                        $"Erro ao inicializar página de sincronização:\n\n{ex.Message}\n\nVerifique os logs em %AppData%\\EtnoPapers\\logs\\ para mais detalhes.",
                        "Erro na Sincronização",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }
                catch (Exception msgEx)
                {
                    _logger.Error($"Failed to show error message: {msgEx.Message}");
                }

                throw;
            }
        }

        private void DismissSyncReminder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_viewModel != null)
                {
                    _viewModel.DismissSyncReminderCommand.Execute(null);
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error dismissing sync reminder: {ex.Message}", ex);
                MessageBox.Show($"Erro ao descartar lembrete: {ex.Message}");
            }
        }

        private void ClearSelection_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_viewModel != null)
                {
                    _viewModel.SelectedRecords.Clear();
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error clearing selection: {ex.Message}", ex);
                MessageBox.Show($"Erro ao limpar seleção: {ex.Message}");
            }
        }
    }
}
