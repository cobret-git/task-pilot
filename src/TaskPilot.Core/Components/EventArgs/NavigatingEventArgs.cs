using TaskPilot.Core.Components.Data;

namespace TaskPilot.Core.Components.EventArgs
{
    /// <summary>
    /// Provides data for the <see cref="Services.INavigationService.Navigating"/> event.
    /// Allows cancellation of the navigation by setting <see cref="Cancel"/> to true.
    /// </summary>
    public class NavigatingEventArgs : System.EventArgs
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigatingEventArgs"/> class.
        /// </summary>
        /// <param name="context">The navigation context.</param>
        public NavigatingEventArgs(NavigationContext context)
        {
            Context = context;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the navigation context with source/target information.
        /// </summary>
        public NavigationContext Context { get; init; }

        /// <summary>
        /// Gets or sets a value indicating whether the navigation should be cancelled.
        /// </summary>
        public bool Cancel { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Checks if navigating away from the specified ViewModel.
        /// </summary>
        /// <param name="viewModelId">The ViewModel identifier to check.</param>
        public bool IsNavigatingFrom(Guid viewModelId) => Context.IsNavigatingFrom(viewModelId);

        /// <summary>
        /// Checks if navigating to the specified ViewModel.
        /// </summary>
        /// <param name="viewModelId">The ViewModel identifier to check.</param>
        public bool IsNavigatingTo(Guid viewModelId) => Context.IsNavigatingTo(viewModelId);

        #endregion
    }
}
