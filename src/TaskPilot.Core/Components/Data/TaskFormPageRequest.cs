using TaskPilot.Core.Components.Entities;
using TaskPilot.Core.ViewModel;

namespace TaskPilot.Core.Components.Data
{
    /// <summary>
    /// Represents a navigation request to the task form page.
    /// </summary>
    public class TaskFormPageRequest : PageRequestBase
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskFormPageRequest"/> class.
        /// </summary>
        /// <param name="task">The task to create or edit.</param>
        /// <param name="action">The form action (Create or Edit).</param>
        /// <param name="project">The project to auto-select (nullable).</param>
        /// <param name="milestone">The milestone to auto-select (nullable).</param>
        public TaskFormPageRequest(TaskItem task, FormDialogAction action, Project? project = null, Milestone? milestone = null)
        {
            Data = new TaskFormData(task, action, project, milestone);
            ViewModelType = typeof(TaskFormViewModel);
        }

        #endregion
    }

    /// <summary>
    /// Data payload for the task form page.
    /// Contains the task entity, action being performed, and optional context for auto-selection.
    /// </summary>
    public record TaskFormData
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskFormData"/> record.
        /// </summary>
        /// <param name="task">The task to create or edit.</param>
        /// <param name="action">The form action (Create or Edit).</param>
        /// <param name="project">The project to auto-select (nullable).</param>
        /// <param name="milestone">The milestone to auto-select (nullable).</param>
        public TaskFormData(TaskItem task, FormDialogAction action, Project? project = null, Milestone? milestone = null)
        {
            Task = task;
            Action = action;
            ContextProject = project;
            ContextMilestone = milestone;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the task being created or edited.
        /// </summary>
        public TaskItem Task { get; }

        /// <summary>
        /// Gets the action being performed on the task.
        /// </summary>
        public FormDialogAction Action { get; }

        /// <summary>
        /// Gets the project to auto-select if task creation was requested from Project page.
        /// </summary>
        public Project? ContextProject { get; }

        /// <summary>
        /// Gets the milestone to auto-select if task creation was requested from Milestone page.
        /// </summary>
        public Milestone? ContextMilestone { get; }

        /// <summary>
        /// Gets or sets a value indicating whether changes were saved successfully.
        /// </summary>
        public bool IsChangesSaved { get; set; }

        #endregion
    }
}
