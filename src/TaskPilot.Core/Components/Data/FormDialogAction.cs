namespace TaskPilot.Core.Components.Data
{
    /// <summary>
    /// Defines the action mode for form-based dialogs.
    /// Determines dialog behavior, button labels, and validation rules.
    /// </summary>
    public enum FormDialogAction
    {
        /// <summary>
        /// Creating a new entity. Primary button typically labeled "Create".
        /// Form fields are empty or use default values.
        /// </summary>
        Create,

        /// <summary>
        /// Editing an existing entity. Primary button typically labeled "Save".
        /// Form fields are pre-populated with existing entity data.
        /// </summary>
        Edit
    }
}
