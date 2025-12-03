using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace EtnoPapers.UI.Converters
{
    /// <summary>
    /// Converts string values to Visibility (Visible if non-empty, Collapsed if empty/null).
    /// </summary>
    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string strValue)
            {
                return string.IsNullOrWhiteSpace(strValue) ? Visibility.Collapsed : Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
