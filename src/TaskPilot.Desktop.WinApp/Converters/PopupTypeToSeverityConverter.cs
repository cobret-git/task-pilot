using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.Windows.AI.ContentSafety;
using System;
using TaskPilot.Core.Components.Data;

namespace TaskPilot.Desktop.WinApp.Converters
{
    public class PopupTypeToSeverityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is PopupType popupType)
            {
                return popupType switch
                {
                    PopupType.Error => InfoBarSeverity.Error,
                    PopupType.Warning => InfoBarSeverity.Warning,
                    PopupType.Informational => InfoBarSeverity.Informational,
                    PopupType.Success => InfoBarSeverity.Success,
                    _ => throw new NotImplementedException()
                };
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
