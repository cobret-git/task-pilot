namespace TaskPilot.Core.Components.Data
{
    /// <summary>
    /// Represents the outcome of a navigation operation.
    /// Contains success status, target ViewModel ID, and error details if failed.
    /// </summary>
    public class NavigationResult
    {
        #region Properties

        /// <summary>
        /// Gets a value indicating whether the navigation completed successfully.
        /// </summary>
        public bool Success { get; init; }

        /// <summary>
        /// Gets the unique identifier of the navigated ViewModel (if successful).
        /// </summary>
        public Guid? ViewModelId { get; init; }

        /// <summary>
        /// Gets the error message if navigation failed.
        /// </summary>
        public string ErrorMessage { get; init; } = string.Empty;

        /// <summary>
        /// Gets the exception that caused the failure (if any).
        /// </summary>
        public Exception? Exception { get; init; }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a successful navigation result.
        /// </summary>
        /// <param name="viewModelId">The ID of the navigated ViewModel.</param>
        public static NavigationResult Successful(Guid viewModelId) => new()
        {
            Success = true,
            ViewModelId = viewModelId
        };

        /// <summary>
        /// Creates a failed navigation result.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="ex">The exception that caused the failure.</param>
        public static NavigationResult Failed(string message, Exception? ex = null) => new()
        {
            Success = false,
            ErrorMessage = message,
            Exception = ex
        };

        /// <summary>
        /// Creates a cancelled navigation result.
        /// </summary>
        public static NavigationResult Cancelled() => new()
        {
            Success = false,
            ErrorMessage = "Navigation was cancelled"
        };

        #endregion
    }
}
