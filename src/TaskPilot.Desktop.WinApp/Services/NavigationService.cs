using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskPilot.Core.Components.Data;
using TaskPilot.Core.Components.EventArgs;
using TaskPilot.Core.Services;
using TaskPilot.Core.ViewModel;
using TaskPilot.Desktop.WinApp.Pages;

namespace TaskPilot.Desktop.WinApp.Services
{
    /// <summary>
    /// WinUI implementation of <see cref="INavigationService"/> using Frame-based navigation.
    /// Maps ViewModel types to Page types and manages navigation state.
    /// </summary>
    public class NavigationService : INavigationService
    {
        #region Fields
        private readonly Dictionary<Type, Type> _viewModelToPageMapping;
        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationService"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider for resolving ViewModels.</param>
        public NavigationService(IServiceProvider serviceProvider)
        {
            _viewModelToPageMapping = new Dictionary<Type, Type>()
            {
                { typeof(ProjectFormViewModel), typeof(ProjectFormPage) }
            };
        }

        #endregion

        #region Events

        /// <inheritdoc/>
        public event EventHandler<NavigatedEventArgs>? Navigated;

        /// <inheritdoc/>
        public event EventHandler<NavigatingEventArgs>? Navigating;

        /// <inheritdoc/>
        public event EventHandler<NavigationFailedEventArgs>? NavigationFailed;

        #endregion

        #region Properties

        /// <inheritdoc/>
        public Guid? CurrentViewModelId { get; private set; }

        /// <inheritdoc/>
        public Type? CurrentViewModelType { get; private set; }

        /// <inheritdoc/>
        public bool CanGoBack => Frame?.CanGoBack ?? false;

        /// <summary>
        /// Gets or sets the Frame control used for navigation. Must be set before navigating.
        /// </summary>
        public Frame? Frame { get; set; }

        /// <summary>
        /// Gets or sets the service provider for resolving ViewModels. Must be set before navigating.
        /// </summary>
        public IServiceProvider? ServiceProvider { get; set; }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public async Task<NavigationResult> NavigateToAsync(PageRequestBase request)
        {
            if (!_viewModelToPageMapping.TryGetValue(request.ViewModelType, out var pageType))
            {
                return NavigationResult.Failed(
                    $"No page registered for ViewModel type {request.ViewModelType.Name}. " +
                    $"Call RegisterMapping<TViewModel, TPage>() first.");
            }

            try
            {
                ArgumentNullException.ThrowIfNull(Frame);
                ArgumentNullException.ThrowIfNull(ServiceProvider);

                var viewModel = ServiceProvider.GetService(request.ViewModelType) as IPageViewModel;

                if (viewModel == null)
                {
                    return NavigationResult.Failed(
                        $"Failed to resolve ViewModel of type {request.ViewModelType.Name} from service provider.");
                }
                viewModel.Id = Guid.NewGuid();

                var context = new NavigationContext
                {
                    Parameter = request.Data,
                    Mode = NavigationMode.New,
                    SourceViewModelId = CurrentViewModelId,
                    SourceViewModelType = CurrentViewModelType,
                    TargetViewModelId = viewModel.Id,
                    TargetViewModelType = request.ViewModelType
                };

                // Step 6: Raise Navigating event (global listeners can cancel)
                var navigatingArgs = new NavigatingEventArgs(context);
                Navigating?.Invoke(this, navigatingArgs);

                if (navigatingArgs.Cancel)
                {
                    return NavigationResult.Cancelled();
                }

                // Step 8: Perform actual Frame navigation
                var navigationSuccess = Frame.Navigate(pageType);

                if (!navigationSuccess)
                {
                    return NavigationResult.Failed("Frame navigation failed.");
                }

                viewModel.Data = request.Data;

                // Step 9: Set DataContext on the page
                if (Frame.Content is FrameworkElement page)
                {
                    page.DataContext = viewModel;
                }

                // Step 12: Update current state
                CurrentViewModelId = viewModel.Id;
                CurrentViewModelType = request.ViewModelType;

                // Step 13: Raise Navigated event
                Navigated?.Invoke(this, new NavigatedEventArgs { Context = context, Success = true });
                return NavigationResult.Successful(viewModel.Id);
            }
            catch (Exception ex)
            {
                var context = new NavigationContext
                {
                    Parameter = request.Data,
                    SourceViewModelId = CurrentViewModelId,
                    SourceViewModelType = CurrentViewModelType,
                    TargetViewModelType = request.ViewModelType
                };

                NavigationFailed?.Invoke(this, new NavigationFailedEventArgs(context, ex, ex.Message));
                return NavigationResult.Failed(ex.Message, ex);
            }
        }
        #endregion

        #region Helpers
        #endregion
    }
}
