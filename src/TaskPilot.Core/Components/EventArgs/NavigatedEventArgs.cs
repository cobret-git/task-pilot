using TaskPilot.Core.Components.Data;

namespace TaskPilot.Core.Components.EventArgs
{
    /// <summary>
    /// Provides data for the <see cref="Services.INavigationService.Navigated"/> event.
    /// Contains navigation context and success status.
    /// </summary>
    public class NavigatedEventArgs : System.EventArgs
    {
        #region Properties

        /// <summary>
        /// Gets the navigation context with source/target information.
        /// </summary>
        public NavigationContext Context { get; init; } = null!;

        /// <summary>
        /// Gets a value indicating whether the navigation was successful.
        /// </summary>
        public bool Success { get; init; }

        /// <summary>
        /// Gets the source ViewModel identifier (convenience property).
        /// </summary>
        public Guid? OldViewModelId => Context.SourceViewModelId;

        /// <summary>
        /// Gets the source ViewModel type (convenience property).
        /// </summary>
        public Type? OldViewModelType => Context.SourceViewModelType;

        /// <summary>
        /// Gets the target ViewModel identifier (convenience property).
        /// </summary>
        public Guid? NewViewModelId => Context.TargetViewModelId;

        /// <summary>
        /// Gets the target ViewModel type (convenience property).
        /// </summary>
        public Type NewViewModelType => Context.TargetViewModelType;

        #endregion

        #region Methods

        /// <summary>
        /// Checks if the specified ViewModel is involved in this navigation (as source or target).
        /// </summary>
        /// <param name="viewModelId">The ViewModel identifier to check.</param>
        public bool InvolvedViewModel(Guid viewModelId) =>
            OldViewModelId == viewModelId || NewViewModelId == viewModelId;

        /// <summary>
        /// Checks if navigating away from the specified ViewModel.
        /// </summary>
        /// <param name="viewModelId">The ViewModel identifier to check.</param>
        public bool IsNavigatingFrom(Guid viewModelId) => OldViewModelId == viewModelId;

        /// <summary>
        /// Checks if navigating to the specified ViewModel.
        /// </summary>
        /// <param name="viewModelId">The ViewModel identifier to check.</param>
        public bool IsNavigatingTo(Guid viewModelId) => NewViewModelId == viewModelId;

        #endregion
    }
}
