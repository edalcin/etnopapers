using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace EtnoPapers.UI.Converters
{
    /// <summary>
    /// Converts boolean values to Brush colors.
    /// Use ConverterParameter to specify colors: "Connected" for success/green, "Disconnected" for error/red.
    /// </summary>
    public class BoolToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not bool boolValue)
            {
                return new SolidColorBrush(Colors.Gray);
            }

            string parameterStr = parameter?.ToString()?.ToLower() ?? string.Empty;

            if (boolValue)
            {
                // Connected/True state
                return parameterStr == "disconnected"
                    ? new SolidColorBrush(Color.FromArgb(255, 209, 52, 56))  // Red (#D13438)
                    : new SolidColorBrush(Color.FromArgb(255, 16, 124, 16)); // Green (#107C10)
            }
            else
            {
                // Disconnected/False state
                return parameterStr == "disconnected"
                    ? new SolidColorBrush(Color.FromArgb(255, 16, 124, 16))  // Green (#107C10)
                    : new SolidColorBrush(Color.FromArgb(255, 209, 52, 56)); // Red (#D13438)
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("BoolToBrushConverter only supports one-way binding.");
        }
    }
}
