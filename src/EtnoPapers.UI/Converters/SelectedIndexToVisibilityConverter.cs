using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace EtnoPapers.UI.Converters
{
    /// <summary>
    /// Converts a selected index to Visibility by comparing against a target index.
    /// Shows element only if selectedIndex equals the targetIndex parameter.
    /// </summary>
    public class SelectedIndexToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int selectedIndex && parameter is string parameterString && int.TryParse(parameterString, out int targetIndex))
            {
                return selectedIndex == targetIndex ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
