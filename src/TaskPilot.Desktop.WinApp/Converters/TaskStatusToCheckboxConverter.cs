using Microsoft.UI.Xaml.Data;
using System;
using TaskPilot.Core.Components.Data;

namespace TaskPilot.Desktop.WinApp.Converters
{
    /// <summary>
    /// Converts TaskStatus to checkbox checked state.
    /// Returns true if status is Completed or Cancelled.
    /// </summary>
    public class TaskStatusToCheckboxConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is TaskStatus status)
            {
                return status == TaskStatus.Completed || status == TaskStatus.Cancelled;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
