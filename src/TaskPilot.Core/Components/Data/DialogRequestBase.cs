namespace TaskPilot.Core.Components.Data
{
    /// <summary>
    /// Abstract base class for dialog configuration requests.
    /// Encapsulates all information needed to present a dialog including ViewModel type,
    /// data payload, button configuration, and result tracking.
    /// </summary>
    /// <remarks>
    /// Concrete implementations should:
    /// 1. Set <see cref="ViewModelType"/> to the dialog's ViewModel type
    /// 2. Initialize <see cref="Data"/> with the dialog payload
    /// 3. Configure <see cref="Buttons"/> array with button labels (e.g., ["Save", "Cancel"])
    /// The <see cref="Result"/> property is set by <see cref="IDialogService"/> after dialog closure.
    /// </remarks>
    public abstract class DialogRequestBase
    {
        #region Properties

        /// <summary>
        /// Gets the type of ViewModel associated with this dialog.
        /// The dialog service uses this type to resolve the ViewModel instance from the service provider.
        /// Must implement or be compatible with the expected dialog ViewModel contract.
        /// </summary>
        public Type ViewModelType { get; protected init; } = null!;

        /// <summary>
        /// Gets the data payload to be passed to the dialog's ViewModel.
        /// If the ViewModel implements <see cref="IFormViewModel{TData}"/>, this data is injected
        /// via the Data property before the dialog is displayed.
        /// </summary>
        public object? Data { get; protected init; }

        /// <summary>
        /// Gets the array of button labels to display in the dialog.
        /// Buttons are displayed in order: typically [Primary Action, Secondary Action, ...].
        /// </summary>
        /// <example>
        /// ["Create", "Cancel"] or ["Save", "Delete", "Cancel"]
        /// </example>
        public string[] Buttons { get; protected init; } = [];

        /// <summary>
        /// Gets or sets the index of the button clicked by the user (0-based).
        /// Null if the dialog was dismissed without clicking any button.
        /// Set by the dialog service implementation after the dialog closes.
        /// </summary>
        /// <example>
        /// For Buttons = ["Save", "Cancel"]:
        /// - Result = 0 means "Save" was clicked
        /// - Result = 1 means "Cancel" was clicked
        /// - Result = null means dialog was dismissed (ESC, X button, etc.)
        /// </example>
        public int? Result { get; set; }

        #endregion
    }
}
