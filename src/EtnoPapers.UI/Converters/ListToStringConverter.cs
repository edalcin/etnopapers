using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace EtnoPapers.UI.Converters
{
    /// <summary>
    /// Converts a List<string> to/from a newline-separated string for display in TextBox.
    /// </summary>
    public class ListToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is List<string> list)
            {
                return string.Join(Environment.NewLine, list);
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string text && !string.IsNullOrWhiteSpace(text))
            {
                var lines = text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                return new List<string>(lines);
            }
            return new List<string>();
        }
    }
}
