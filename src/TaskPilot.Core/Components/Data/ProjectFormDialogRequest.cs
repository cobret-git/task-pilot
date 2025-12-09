using TaskPilot.Core.Components.Entities;
using TaskPilot.Core.ViewModel;

namespace TaskPilot.Core.Components.Data
{
    /// <summary>
    /// Dialog request configuration for creating or editing a project.
    /// Configures the dialog with <see cref="ProjectFormViewModel"/> and appropriate buttons
    /// based on the <see cref="FormDialogAction"/>.
    /// </summary>
    public class ProjectFormDialogRequest : DialogRequestBase
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectFormDialogRequest"/> class.
        /// </summary>
        /// <param name="project">The project to create or edit.</param>
        /// <param name="action">The form action (Create or Edit).</param>
        public ProjectFormDialogRequest(Project project, FormDialogAction action)
        {
            Data = new ProjectFormData(project, action);
            ViewModelType = typeof(ProjectFormViewModel);

            var primaryAction = action switch
            {
                FormDialogAction.Create => "Create",
                FormDialogAction.Edit => "Save",
                _ => throw new NotImplementedException()
            };
            Buttons = [primaryAction, "Cancel"];
        }

        #endregion
    }

    /// <summary>
    /// Data payload for the project form dialog.
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

        #endregion
    }
}
