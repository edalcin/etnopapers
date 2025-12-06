using System;
using System.Windows;
using System.Windows.Controls;
using EtnoPapers.Core.Models;
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

                // Setup ListBox selection synchronization manually (avoid binding issues during initialization)
                try
                {
                    _logger.Info("Setting up ListBox selection synchronization...");
                    RecordsListBox.SelectionChanged += (s, e) =>
                    {
                        try
                        {
                            _logger.Debug($"ListBox SelectionChanged fired. Added: {e.AddedItems.Count}, Removed: {e.RemovedItems.Count}");

                            // Sync ListBox selection to ViewModel
                            _viewModel.SelectedRecords.Clear();
                            int selectedCount = 0;

                            foreach (var item in RecordsListBox.SelectedItems)
                            {
                                if (item is ArticleRecord record)
                                {
                                    _viewModel.SelectedRecords.Add(record);
                                    selectedCount++;
                                    _logger.Debug($"Added record to SelectedRecords: {record.Titulo}");
                                }
                            }

                            _logger.Info($"Selection sync completed. Total selected: {selectedCount}");

                            // Atualizar estado do checkbox "Selecionar Todos"
                            if (selectedCount == _viewModel.AvailableRecords.Count && selectedCount > 0)
                            {
                                SelectAllCheckBox.IsChecked = true;
                            }
                            else if (selectedCount == 0)
                            {
                                SelectAllCheckBox.IsChecked = false;
                            }
                        }
                        catch (Exception selEx)
                        {
                            _logger.Error($"Error syncing ListBox selection: {selEx.Message}", selEx);
                        }
                    };
                    _logger.Info("ListBox selection synchronization setup completed");
                }
                catch (Exception syncEx)
                {
                    _logger.Error($"Error setting up ListBox sync: {syncEx.Message}", syncEx);
                    throw;
                }

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
                    RecordsListBox.SelectedItems.Clear();
                    SelectAllCheckBox.IsChecked = false;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error clearing selection: {ex.Message}", ex);
                MessageBox.Show($"Erro ao limpar seleção: {ex.Message}");
            }
        }

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SelectAllCheckBox.IsChecked == true)
                {
                    // Selecionar todos os itens
                    RecordsListBox.SelectAll();
                    _logger.Info("All records selected");
                }
                else
                {
                    // Desselecionar todos os itens
                    RecordsListBox.SelectedItems.Clear();
                    _logger.Info("All records deselected");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error in SelectAll_Click: {ex.Message}", ex);
                MessageBox.Show($"Erro ao selecionar todos: {ex.Message}");
            }
        }
    }
}
