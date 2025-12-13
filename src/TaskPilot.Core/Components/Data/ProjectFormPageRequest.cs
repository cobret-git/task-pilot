using TaskPilot.Core.Components.Entities;
using TaskPilot.Core.ViewModel;

namespace TaskPilot.Core.Components.Data
{
    /// <summary>
    /// Navigation request to display the project create/edit form page.
    /// </summary>
    public class ProjectFormPageRequest : PageRequestBase
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectFormPageRequest"/> class.
        /// </summary>
        /// <param name="project">The project to create or edit.</param>
        /// <param name="action">The form action (Create or Edit).</param>
        public ProjectFormPageRequest(Project project, FormDialogAction action)
        {
            Data = new ProjectFormData(project, action);
            ViewModelType = typeof(ProjectFormViewModel);
        }

        #endregion
    }

    /// <summary>
    /// Data payload for the project form page.
    /// Contains the project entity and the action being performed.
    /// </summary>
    public record ProjectFormData
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectFormData"/> record.
        /// </summary>
        /// <param name="project">The project to create or edit.</param>
        /// <param name="action">The form action (Create or Edit).</param>
        public ProjectFormData(Project project, FormDialogAction action)
        {
            Project = project;
            Action = action;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the project being created or edited.
        /// </summary>
        public Project Project { get; }

        /// <summary>
        /// Gets the action being performed on the project.
        /// </summary>
        public FormDialogAction Action { get; }

        /// <summary>
        /// Gets or sets a value indicating whether changes were saved successfully.
        /// </summary>
        public bool IsChangesSaved { get; set; }

        #endregion
    }
}
