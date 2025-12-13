using CommunityToolkit.Mvvm.ComponentModel;
using TaskPilot.Core.Components.EventArgs;
using TaskPilot.Core.Services;

namespace TaskPilot.Core.ViewModel
{
    /// <summary>
    /// Base class for page ViewModels with navigation awareness and MVVM support.
    /// Subscribes to navigation events and provides virtual handlers for derived classes.
    /// </summary>
    public abstract class ObservablePageViewModelBase : ObservableObject, IPageViewModel, IDisposable
    {
        #region Fields
        protected readonly INavigationService _navigationService;
        private string title = string.Empty;
        private object? data;
        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservablePageViewModelBase"/> class.
        /// </summary>
        /// <param name="navigationService">The navigation service for page transitions.</param>
        public ObservablePageViewModelBase(INavigationService navigationService)
        {
            _navigationService = navigationService;
            _navigationService.Navigated += OnNavigated;
            _navigationService.Navigating += OnNavigating;
        }

        #endregion

        #region Properties

        /// <inheritdoc/>
        public Guid Id { get; set; }

        /// <inheritdoc/>
        public object? Data { get => data; set { data = value; OnDataChanged(value); } }

        /// <inheritdoc/>
        public string Title { get => title; set { SetProperty(ref title, value); } }

        #endregion

        #region Methods

        /// <summary>
        /// Releases resources and unsubscribes from navigation events.
        /// </summary>
        public virtual void Dispose()
        {
            _navigationService.Navigated -= OnNavigated;
            _navigationService.Navigating -= OnNavigating;
        }

        #endregion

        #region Handlers

        /// <summary>
        /// Called after navigation away from this ViewModel completes. Override to perform cleanup.
        /// </summary>
        /// <param name="e">The navigation event data.</param>
        protected virtual void OnNavigatedFrom(NavigatedEventArgs e) { }

        /// <summary>
        /// Called before navigation away from this ViewModel. Override to cancel or prepare.
        /// </summary>
        /// <param name="e">The navigation event data. Set <see cref="NavigatingEventArgs.Cancel"/> to prevent navigation.</param>
        protected virtual void OnNavigatingFrom(NavigatingEventArgs e) { }

        /// <summary>
        /// Called when the <see cref="Data"/> property changes. Override to process incoming data.
        /// </summary>
        /// <param name="newData">The new data value.</param>
        protected virtual void OnDataChanged(object? newData) { }

        private void OnNavigated(object? sender, NavigatedEventArgs e)
        {
            if (e.IsNavigatingFrom(Id)) OnNavigatedFrom(e);
        }

        private void OnNavigating(object? sender, NavigatingEventArgs e)
        {
            if (e.IsNavigatingFrom(Id)) OnNavigatingFrom(e);
        }

        #endregion
    }
}
