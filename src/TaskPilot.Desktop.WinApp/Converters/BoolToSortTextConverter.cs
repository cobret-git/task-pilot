using Microsoft.UI.Xaml.Data;
using System;

namespace TaskPilot.Desktop.WinApp.Converters
{
    public class BoolToSortTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value is bool b && b ? "Ascending" : "Descending";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
