using System;
using System.Windows.Controls;
using EtnoPapers.UI.Views;

namespace EtnoPapers.UI.Services
{
    /// <summary>
    /// Manages navigation between pages in the application.
    /// </summary>
    public class NavigationService
    {
        private readonly Frame _mainFrame;

        public NavigationService(Frame mainFrame)
        {
            _mainFrame = mainFrame ?? throw new ArgumentNullException(nameof(mainFrame));
        }

        /// <summary>
        /// Navigates to a page by name.
        /// </summary>
        public void NavigateTo(string pageName)
        {
            Page page = pageName?.ToLowerInvariant() switch
            {
                "home" => new HomePage(),
                "upload" => new UploadPage(),
                "records" => new RecordsPage(),
                "sync" => new SyncPage(),
                "settings" => new SettingsPage(),
                _ => new HomePage()
            };

            _mainFrame.Navigate(page);
        }

        /// <summary>
        /// Gets the current page displayed in the frame.
        /// </summary>
        public Page GetCurrentPage()
        {
            return _mainFrame.Content as Page;
        }

        /// <summary>
        /// Checks if navigation can go back.
        /// </summary>
        public bool CanGoBack()
        {
            return _mainFrame.CanGoBack;
        }

        /// <summary>
        /// Goes back to the previous page.
        /// </summary>
        public void GoBack()
        {
            if (_mainFrame.CanGoBack)
                _mainFrame.GoBack();
        }
    }
}
