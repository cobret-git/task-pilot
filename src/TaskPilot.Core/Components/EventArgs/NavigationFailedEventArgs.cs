using TaskPilot.Core.Components.Data;

namespace TaskPilot.Core.Components.EventArgs
{
    /// <summary>
    /// Provides data for the <see cref="Services.INavigationService.NavigationFailed"/> event.
    /// Contains the exception and error details for the failed navigation.
    /// </summary>
    public class NavigationFailedEventArgs : System.EventArgs
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationFailedEventArgs"/> class.
        /// </summary>
        /// <param name="context">The navigation context.</param>
        /// <param name="exception">The exception that caused the failure.</param>
        /// <param name="errorMessage">The error message.</param>
        public NavigationFailedEventArgs(NavigationContext context, Exception exception, string errorMessage)
        {
            Context = context;
            Exception = exception;
            ErrorMessage = errorMessage;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the navigation context with source/target information.
        /// </summary>
        public NavigationContext Context { get; init; }

        /// <summary>
        /// Gets the exception that caused the navigation failure.
        /// </summary>
        public Exception Exception { get; init; }

        /// <summary>
        /// Gets the error message describing the failure.
        /// </summary>
        public string ErrorMessage { get; init; }

        #endregion
    }
}
