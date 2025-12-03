using System.Windows;
using System.Windows.Controls;
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

        public SyncPage()
        {
            InitializeComponent();
            _viewModel = new SyncViewModel();
            DataContext = _viewModel;
            _viewModel.LoadAvailableRecords();
        }

        private void DismissSyncReminder_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.DismissSyncReminderCommand.Execute(null);
            }
        }

        private void ClearSelection_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.SelectedRecords.Clear();
            }
        }
    }
}
