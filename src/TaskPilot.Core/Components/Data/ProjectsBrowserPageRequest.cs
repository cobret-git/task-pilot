using TaskPilot.Core.ViewModel;

namespace TaskPilot.Core.Components.Data
{
    /// <summary>
    /// Navigation request to display the projects browser page.
    /// </summary>
    public class ProjectsBrowserPageRequest : PageRequestBase
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectsBrowserPageRequest"/> class.
        /// </summary>
        public ProjectsBrowserPageRequest()
        {
            ViewModelType = typeof(ProjectsBrowserViewModel);
        }

        #endregion
    }
}
