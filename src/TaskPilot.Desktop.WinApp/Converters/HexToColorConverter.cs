using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using System;
using Windows.UI;

namespace TaskPilot.Desktop.WinApp.Converters
{
    public class HexToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string hex && !string.IsNullOrWhiteSpace(hex))
            {
                hex = hex.TrimStart('#');
                if (hex.Length == 6)
                {
                    return Color.FromArgb(
                        255,
                        System.Convert.ToByte(hex.Substring(0, 2), 16),
                        System.Convert.ToByte(hex.Substring(2, 2), 16),
                        System.Convert.ToByte(hex.Substring(4, 2), 16));
                }
            }
            return Colors.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

}
