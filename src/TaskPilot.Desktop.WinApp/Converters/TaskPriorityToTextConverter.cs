using Microsoft.UI.Xaml.Data;
using System;
using TaskPilot.Core.Components.Data;

namespace TaskPilot.Desktop.WinApp.Converters
{
    /// <summary>
    /// Converts TaskPriority enum to human-readable text.
    /// </summary>
    public class TaskPriorityToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is TaskPriority priority)
            {
                return priority switch
                {
                    TaskPriority.Low => "Low",
                    TaskPriority.Normal => "Normal",
                    TaskPriority.High => "High",
                    TaskPriority.Critical => "Critical",
                    _ => "Unknown"
                };
            }
            return "Unknown";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
