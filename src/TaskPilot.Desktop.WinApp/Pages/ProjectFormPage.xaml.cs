using Microsoft.UI.Xaml.Controls;
using TaskPilot.Core.ViewModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TaskPilot.Desktop.WinApp.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ProjectFormPage : Page
    {
        public ProjectFormPage()
        {
            InitializeComponent();
        }


        #region Properties
        public ProjectFormViewModel ViewModel => (ProjectFormViewModel)DataContext;
        #endregion
    }
}
