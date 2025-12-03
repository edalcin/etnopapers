using System.Windows;
using System.Windows.Controls;
using EtnoPapers.Core.Models;
using EtnoPapers.UI.ViewModels;

namespace EtnoPapers.UI.Views
{
    /// <summary>
    /// Interaction logic for RecordsPage.xaml
    /// Displays, filters, and manages local records with CRUD operations.
    /// </summary>
    public partial class RecordsPage : Page
    {
        private RecordsViewModel? _viewModel;

        public RecordsPage()
        {
            InitializeComponent();
            _viewModel = new RecordsViewModel();
            DataContext = _viewModel;
            _viewModel.LoadRecords();
        }

        /// <summary>
        /// Handle click event for "New Record" button - opens NewRecordDialog
        /// </summary>
        private void NewRecordButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null)
                return;

            var dialog = new NewRecordDialog();
            if (dialog.ShowDialog() == true && dialog.CreatedRecord != null)
            {
                if (_viewModel.SaveNewRecord(dialog.CreatedRecord))
                {
                    MessageBox.Show(
                        $"Novo registro criado com sucesso!\\n\\nID: {dialog.CreatedRecord.Id}",
                        "Sucesso",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(
                        "Erro ao criar novo registro.",
                        "Erro",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Handle click event for "Edit" button - opens EditRecordDialog
        /// </summary>
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null || RecordsDataGrid.SelectedItem is not ArticleRecord selectedRecord)
            {
                MessageBox.Show(
                    "Por favor, selecione um registro para editar.",
                    "Nenhum Registro Selecionado",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            var dialog = new EditRecordDialog(selectedRecord);
            if (dialog.ShowDialog() == true && dialog.EditedRecord != null)
            {
                _viewModel.EditRecord(dialog.EditedRecord);
                MessageBox.Show(
                    "Registro atualizado com sucesso!",
                    "Sucesso",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Handle click event for "Delete" button - shows confirmation dialog
        /// </summary>
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null || _viewModel.SelectedRecords.Count == 0)
            {
                MessageBox.Show(
                    "Por favor, selecione um ou mais registros para deletar.",
                    "Nenhum Registro Selecionado",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            var confirmDialog = new DeleteConfirmDialog(_viewModel.SelectedRecords.Count);
            if (confirmDialog.ShowDialog() == true)
            {
                int deletedCount = 0;
                foreach (var record in _viewModel.SelectedRecords)
                {
                    if (_viewModel.DeleteRecord(record.Id))
                        deletedCount++;
                }

                _viewModel.SelectedRecords.Clear();
                MessageBox.Show(
                    $"{deletedCount} registro(s) deletado(s) com sucesso.",
                    "Sucesso",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }
    }
}
