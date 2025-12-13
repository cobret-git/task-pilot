using TaskPilot.Core.Components.Data;
using TaskPilot.Core.Components.EventArgs;

namespace TaskPilot.Core.Services
{
    /// <summary>
    /// Provides platform-agnostic page navigation services for MVVM applications.
    /// Manages ViewModel-based navigation with cancellation support and event notifications.
    /// </summary>
    public interface INavigationService
    {
        #region Events

        /// <summary>
        /// Occurs after navigation has completed successfully.
        /// </summary>
        event EventHandler<NavigatedEventArgs>? Navigated;

        /// <summary>
        /// Occurs before navigation starts. Can be cancelled via <see cref="NavigatingEventArgs.Cancel"/>.
        /// </summary>
        event EventHandler<NavigatingEventArgs>? Navigating;

        /// <summary>
        /// Occurs when navigation fails due to an exception.
        /// </summary>
        event EventHandler<NavigationFailedEventArgs>? NavigationFailed;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the unique identifier of the currently active ViewModel.
        /// </summary>
        Guid? CurrentViewModelId { get; }

        /// <summary>
        /// Gets the type of the currently active ViewModel.
        /// </summary>
        Type? CurrentViewModelType { get; }

        /// <summary>
        /// Gets a value indicating whether backward navigation is possible.
        /// </summary>
        bool CanGoBack { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Navigates to a page based on the specified request.
        /// Resolves the ViewModel from DI, assigns data, and performs frame navigation.
        /// </summary>
        /// <param name="request">The navigation request containing target ViewModel type and data.</param>
        /// <returns>A result indicating success or failure of the navigation.</returns>
        Task<NavigationResult> NavigateToAsync(PageRequestBase request);

        #endregion
    }
}
