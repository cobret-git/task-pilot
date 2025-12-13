namespace TaskPilot.Core.Components.Data
{
    /// <summary>
    /// Contains contextual information about a navigation operation.
    /// Tracks source and target ViewModels, navigation mode, and data parameter.
    /// </summary>
    public class NavigationContext
    {
        #region Properties

        /// <summary>
        /// Gets the data parameter passed to the target ViewModel.
        /// </summary>
        public object? Parameter { get; init; }

        /// <summary>
        /// Gets the navigation mode indicating how the navigation was initiated.
        /// </summary>
        public NavigationMode Mode { get; init; }

        /// <summary>
        /// Gets the unique identifier of the source ViewModel (null for first navigation).
        /// </summary>
        public Guid? SourceViewModelId { get; init; }

        /// <summary>
        /// Gets the type of the source ViewModel (null for first navigation).
        /// </summary>
        public Type? SourceViewModelType { get; init; }

        /// <summary>
        /// Gets the unique identifier of the target ViewModel.
        /// </summary>
        public Guid TargetViewModelId { get; init; }

        /// <summary>
        /// Gets the type of the target ViewModel.
        /// </summary>
        public Type TargetViewModelType { get; init; } = null!;

        #endregion

        #region Methods

        /// <summary>
        /// Checks if this navigation originates from the specified ViewModel.
        /// </summary>
        /// <param name="viewModelId">The ViewModel identifier to check.</param>
        /// <returns>True if navigating from the specified ViewModel; otherwise, false.</returns>
        public bool IsNavigatingFrom(Guid viewModelId) => SourceViewModelId == viewModelId;

        /// <summary>
        /// Checks if this navigation targets the specified ViewModel.
        /// </summary>
        /// <param name="viewModelId">The ViewModel identifier to check.</param>
        /// <returns>True if navigating to the specified ViewModel; otherwise, false.</returns>
        public bool IsNavigatingTo(Guid viewModelId) => TargetViewModelId == viewModelId;

        #endregion
    }

    /// <summary>
    /// Specifies how a navigation operation was initiated.
    /// </summary>
    public enum NavigationMode
    {
        /// <summary>
        /// Navigation to a new page.
        /// </summary>
        New,

        /// <summary>
        /// Navigation backward in the stack.
        /// </summary>
        Back,

        /// <summary>
        /// Navigation forward in the stack.
        /// </summary>
        Forward,

        /// <summary>
        /// Refresh of the current page.
        /// </summary>
        Refresh
    }
}
