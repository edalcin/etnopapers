using System.Windows.Controls;
using EtnoPapers.UI.ViewModels;

namespace EtnoPapers.UI.Views
{
    /// <summary>
    /// Interaction logic for UploadPage.xaml
    /// Handles PDF upload and AI-based metadata extraction.
    /// </summary>
    public partial class UploadPage : Page
    {
        public UploadPage()
        {
            InitializeComponent();
            DataContext = new UploadViewModel();
        }
    }
}
