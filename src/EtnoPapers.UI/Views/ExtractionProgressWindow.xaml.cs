using System;
using System.Collections.ObjectModel;
using System.Windows;
using EtnoPapers.Core.Services;

namespace EtnoPapers.UI.Views
{
    /// <summary>
    /// ExtractionProgressWindow - Shows real-time extraction progress and log
    /// </summary>
    public partial class ExtractionProgressWindow : Window
    {
        public ExtractionProgressWindow()
        {
            try
            {
                InitializeComponent();
                DataContext = new ExtractionProgressViewModel();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing ExtractionProgressWindow: {ex.Message}");
                throw;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DataContext is ExtractionProgressViewModel vm)
                {
                    vm.CancelExtraction();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error canceling extraction: {ex.Message}");
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Close();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error closing window: {ex.Message}");
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            try
            {
                // Minimal cleanup - just nullify DataContext to detach bindings
                DataContext = null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during window cleanup: {ex.Message}");
            }
            finally
            {
                base.OnClosed(e);
            }
        }
    }

    /// <summary>
    /// ViewModel for extraction progress window
    /// </summary>
    public class ExtractionProgressViewModel : ViewModels.ViewModelBase
    {
        private int _progress = 0;
        private string _currentStep = "";
        private string _currentMessage = "";
        private bool _isExtracting = false;
        private ObservableCollection<string> _logItems = new();
        private ExtractionPipelineService _extractionService;

        public ExtractionProgressViewModel()
        {
            _extractionService = null;
        }

        #region Properties

        public int Progress
        {
            get => _progress;
            set => SetProperty(ref _progress, value);
        }

        public string CurrentStep
        {
            get => _currentStep;
            set => SetProperty(ref _currentStep, value);
        }

        public string CurrentMessage
        {
            get => _currentMessage;
            set => SetProperty(ref _currentMessage, value);
        }

        public bool IsExtracting
        {
            get => _isExtracting;
            set => SetProperty(ref _isExtracting, value);
        }

        public ObservableCollection<string> LogItems
        {
            get => _logItems;
            set => SetProperty(ref _logItems, value);
        }

        public string ProgressText => $"{Progress}%";

        #endregion

        #region Methods

        public void SetExtractionService(ExtractionPipelineService service)
        {
            _extractionService = service;
            if (_extractionService != null)
            {
                _extractionService.ProgressUpdated += OnProgressUpdated;
            }
        }

        private void OnProgressUpdated(object sender, ProgressUpdateEventArgs e)
        {
            try
            {
                Progress = e.Progress;
                CurrentStep = e.Step;
                CurrentMessage = e.Message;

                var timestamp = DateTime.Now.ToString("HH:mm:ss");
                var logMessage = $"[{timestamp}] {e.Message}";

                if (Application.Current?.Dispatcher != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        LogItems.Add(logMessage);
                    });
                }
                else
                {
                    LogItems.Add(logMessage);
                }

                OnPropertyChanged(nameof(ProgressText));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in OnProgressUpdated: {ex.Message}");
            }
        }

        public void CancelExtraction()
        {
            _extractionService?.CancelExtraction();
            IsExtracting = false;
        }

        #endregion
    }
}
