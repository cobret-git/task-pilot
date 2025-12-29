using Microsoft.UI.Xaml.Controls;
using TaskPilot.Core.ViewModel;

namespace TaskPilot.Desktop.WinApp.Pages
{
    /// <summary>
    /// Task form page for creating and editing tasks.
    /// </summary>
    public sealed partial class TaskFormPage : Page
    {
        #region Constructors
        public TaskFormPage()
        {
            InitializeComponent();
        }
        #endregion

        #region Properties
        public TaskFormViewModel? ViewModel => DataContext as TaskFormViewModel;
        #endregion
    }
}
