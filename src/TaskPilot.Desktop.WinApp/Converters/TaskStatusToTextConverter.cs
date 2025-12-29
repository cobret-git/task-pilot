using Microsoft.UI.Xaml.Data;
using System;
using TaskPilot.Core.Components.Data;

namespace TaskPilot.Desktop.WinApp.Converters
{
    /// <summary>
    /// Converts TaskStatus enum to human-readable text for tooltips.
    /// </summary>
    public class TaskStatusToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is TaskStatus status)
            {
                return status switch
                {
                    TaskStatus.InProgress => "In Progress",
                    TaskStatus.Cancelled => "Cancelled",
                    TaskStatus.Completed => "Completed",
                    TaskStatus.NotStarted => "Not Started",
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
