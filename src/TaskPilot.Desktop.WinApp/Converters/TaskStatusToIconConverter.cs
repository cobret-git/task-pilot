using Microsoft.UI.Xaml.Data;
using System;
using TaskPilot.Core.Components.Data;

namespace TaskPilot.Desktop.WinApp.Converters
{
    /// <summary>
    /// Converts TaskStatus enum to Material Symbols icon glyph.
    /// InProgress: &#xe1c4;, Cancelled: &#xe14b;, Completed: &#xe86c;, NotStarted: &#xf037;
    /// </summary>
    public class TaskStatusToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is TaskStatus status)
            {
                return status switch
                {
                    TaskStatus.InProgress => "\ue1c4",
                    TaskStatus.Cancelled => "\ue14b",
                    TaskStatus.Completed => "\ue86c",
                    TaskStatus.NotStarted => "\uf037",
                    _ => "\uf037"
                };
            }
            return "\uf037";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
