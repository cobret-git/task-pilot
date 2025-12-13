namespace TaskPilot.Core.Components.Data
{
    /// <summary>
    /// Specifies the type of message dialog, which determines the icon and styling.
    /// </summary>
    public enum MessageDialogType
    {
        /// <summary>
        /// Informational message.
        /// </summary>
        Info,

        /// <summary>
        /// Success/completion message.
        /// </summary>
        Success,

        /// <summary>
        /// Warning message.
        /// </summary>
        Warning,

        /// <summary>
        /// Error message.
        /// </summary>
        Error,

        /// <summary>
        /// Confirmation prompt.
        /// </summary>
        Confirm
    }
}
