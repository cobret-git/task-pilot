using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using TaskPilot.Core.ViewModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TaskPilot.Desktop.WinApp.Pages
{
    /// <summary>
    /// Project form page for creating and editing projects.
    /// Uses the MarkdownEditorControl for description input.
    /// </summary>
    public sealed partial class ProjectFormPage : Page
    {
        #region Constructors

        public ProjectFormPage()
        {
            InitializeComponent();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the ViewModel from DataContext.
        /// </summary>
        public ProjectFormViewModel? ViewModel => DataContext as ProjectFormViewModel;

        #endregion

        #region Handlers

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

        #endregion
    }
}
