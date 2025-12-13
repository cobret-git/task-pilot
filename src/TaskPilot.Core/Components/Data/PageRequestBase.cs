namespace TaskPilot.Core.Components.Data
{
    /// <summary>
    /// Base class for navigation requests.
    /// Encapsulates the target ViewModel type and optional data payload.
    /// </summary>
    public abstract class PageRequestBase
    {
        #region Properties

        /// <summary>
        /// Gets the type of ViewModel to navigate to.
        /// </summary>
        public Type ViewModelType { get; protected init; } = null!;

        /// <summary>
        /// Gets the data payload to pass to the target ViewModel.
        /// </summary>
        public object? Data { get; protected init; }

        #endregion
    }
}
