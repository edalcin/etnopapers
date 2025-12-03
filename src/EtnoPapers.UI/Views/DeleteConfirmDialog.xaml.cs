using System.Windows;

namespace EtnoPapers.UI.Views
{
    /// <summary>
    /// DeleteConfirmDialog - Confirmation dialog for destructive delete operations
    /// </summary>
    public partial class DeleteConfirmDialog : Window
    {
        public DeleteConfirmDialog(int recordCount)
        {
            InitializeComponent();

            if (recordCount == 1)
            {
                MessageTextBlock.Text = "Você está prestes a deletar 1 registro. Esta ação é irreversível.";
            }
            else
            {
                MessageTextBlock.Text = $"Você está prestes a deletar {recordCount} registros. Esta ação é irreversível.";
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
