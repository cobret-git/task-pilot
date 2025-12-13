using TaskPilot.Core.Components.Entities;
using TaskPilot.Core.ViewModel;

namespace TaskPilot.Core.Components.Data
{
    /// <summary>
    /// Navigation request to display a project's details page.
    /// </summary>
    public class ProjectPageRequest : PageRequestBase
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectPageRequest"/> class.
        /// </summary>
        /// <param name="project">The project to display.</param>
        public ProjectPageRequest(Project project)
        {
            Data = project;
            ViewModelType = typeof(ProjectPageViewModel);
        }

        #endregion
    }
}
