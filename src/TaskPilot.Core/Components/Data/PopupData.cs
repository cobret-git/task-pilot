namespace TaskPilot.Core.Components.Data
{
    /// <summary>
    /// Represents a popup notification with type, title, and message.
    /// Used for displaying inline validation messages or notifications in the UI.
    /// </summary>
    public class PopupData
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PopupData"/> class.
        /// </summary>
        /// <param name="type">The type of popup determining styling.</param>
        /// <param name="title">The popup title.</param>
        /// <param name="message">The popup message content.</param>
        public PopupData(PopupType type, string title, string message)
        {
            PopupType = type;
            Title = title;
            Message = message;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the popup title.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Gets the popup message content.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gets the popup type for styling purposes.
        /// </summary>
        public PopupType PopupType { get; }

        #endregion
    }

    /// <summary>
    /// Specifies the type of popup notification for styling purposes.
    /// </summary>
    public enum PopupType
    {
        /// <summary>
        /// Informational popup.
        /// </summary>
        Informational,

        /// <summary>
        /// Success notification.
        /// </summary>
        Success,

        /// <summary>
        /// Warning notification.
        /// </summary>
        Warning,

        /// <summary>
        /// Error notification.
        /// </summary>
        Error
    }
}
