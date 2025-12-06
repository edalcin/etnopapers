using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace EtnoPapers.UI.Converters
{
    /// <summary>
    /// Converts zero count to Visible, non-zero to Collapsed.
    /// Used to show "empty state" messages when collections are empty.
    /// </summary>
    public class EmptyStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count)
            {
                return count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("EmptyStateConverter only supports one-way binding.");
        }
    }
}
