using Microsoft.UI.Xaml.Data;
using System;

namespace TaskPilot.Desktop.WinApp.Converters
{
    public class BoolToSortIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value is bool b && b ? "\uE74A" : "\uE74B"; // Up/Down arrows
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
