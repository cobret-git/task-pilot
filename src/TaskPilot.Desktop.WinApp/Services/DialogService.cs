using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TaskPilot.Core.Components.Data;
using TaskPilot.Core.Services;
using TaskPilot.Core.ViewModel;

namespace TaskPilot.Desktop.WinApp.Services
{
    /// <summary>
    /// WinUI implementation of <see cref="IDialogService"/> using ContentDialog.
    /// Provides modal dialog presentation for both ViewModel-based forms and simple message dialogs.
    /// </summary>
    /// <remarks>
    /// Required services:
    /// - <see cref="IServiceLocator"/> - For resolving ViewModels and their associated Views
    /// - <see cref="IDispatcherService"/> - For ensuring UI thread access when showing dialogs
    /// </remarks>
    public class DialogService : IDialogService
    {
        #region Fields
        private readonly IServiceLocator _serviceLocator;
        private readonly IDispatcherService _dispatcherService;
        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DialogService"/> class.
        /// </summary>
        /// <param name="serviceLocator">Service locator for resolving ViewModels and Views.</param>
        /// <param name="dispatcherService">Dispatcher service for UI thread marshaling.</param>
        public DialogService(IServiceLocator serviceLocator, IDispatcherService dispatcherService)
        {
            _serviceLocator = serviceLocator ?? throw new ArgumentNullException(nameof(serviceLocator));
            _dispatcherService = dispatcherService ?? throw new ArgumentNullException(nameof(dispatcherService));
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the XamlRoot for displaying dialogs.
        /// Must be set before showing any dialogs (similar to Frame in NavigationService).
        /// </summary>
        public XamlRoot? XamlRoot { get; set; }

        #endregion

        #region ViewModel Dialogs

        /// <inheritdoc/>
        public async Task ShowDialogAsync(DialogRequestBase request, CancellationToken cancellationToken = default)
        {
            if (XamlRoot == null)
                throw new InvalidOperationException("XamlRoot must be set before showing dialogs.");

            cancellationToken.ThrowIfCancellationRequested();

            // Resolve ViewModel
            var viewModel = _serviceLocator.GetService(request.ViewModelType);
            if (viewModel == null)
            {
                throw new InvalidOperationException(
                    $"Failed to resolve ViewModel of type {request.ViewModelType.Name} from service provider.");
            }

            // Inject data if ViewModel implements IFormViewModel<T>
            if (request.Data != null)
            {
                var formViewModelInterface = viewModel.GetType().GetInterfaces()
                    .FirstOrDefault(i => i.IsGenericType &&
                                       i.GetGenericTypeDefinition() == typeof(IFormViewModel<>));

                if (formViewModelInterface != null)
                {
                    var dataProperty = formViewModelInterface.GetProperty("Data");
                    dataProperty?.SetValue(viewModel, request.Data);
                }
            }

            // Create dialog
            var dialog = new ContentDialog
            {
                XamlRoot = XamlRoot,
                Content = viewModel,
                DefaultButton = ContentDialogButton.Primary
            };

            // Set buttons based on request
            if (request.Buttons.Length > 0)
                dialog.PrimaryButtonText = request.Buttons[0];
            if (request.Buttons.Length > 1)
                dialog.SecondaryButtonText = request.Buttons[1];
            if (request.Buttons.Length > 2)
                dialog.CloseButtonText = request.Buttons[2];

            // Show dialog with cancellation support
            var result = await ShowDialogWithCancellationAsync(dialog, cancellationToken);

            // Map ContentDialogResult to button index
            request.Result = result switch
            {
                ContentDialogResult.Primary => 0,
                ContentDialogResult.Secondary => 1,
                ContentDialogResult.None when !string.IsNullOrEmpty(dialog.CloseButtonText) => 2,
                _ => null
            };
        }

        #endregion

        #region Message Dialogs

        /// <inheritdoc/>
        public Task<int?> ShowErrorAsync(string title, string message, CancellationToken cancellationToken = default)
            => ShowMessageDialogAsync(MessageDialogType.Error, title, message, new[] { "OK" }, cancellationToken);

        /// <inheritdoc/>
        public Task<int?> ShowErrorAsync(string title, string message, string[] buttons, CancellationToken cancellationToken = default)
            => ShowMessageDialogAsync(MessageDialogType.Error, title, message, buttons, cancellationToken);

        /// <inheritdoc/>
        public Task<int?> ShowWarningAsync(string title, string message, CancellationToken cancellationToken = default)
            => ShowMessageDialogAsync(MessageDialogType.Warning, title, message, new[] { "OK" }, cancellationToken);

        /// <inheritdoc/>
        public Task<int?> ShowWarningAsync(string title, string message, string[] buttons, CancellationToken cancellationToken = default)
            => ShowMessageDialogAsync(MessageDialogType.Warning, title, message, buttons, cancellationToken);

        /// <inheritdoc/>
        public Task<int?> ShowInfoAsync(string title, string message, CancellationToken cancellationToken = default)
            => ShowMessageDialogAsync(MessageDialogType.Info, title, message, new[] { "OK" }, cancellationToken);

        /// <inheritdoc/>
        public Task<int?> ShowInfoAsync(string title, string message, string[] buttons, CancellationToken cancellationToken = default)
            => ShowMessageDialogAsync(MessageDialogType.Info, title, message, buttons, cancellationToken);

        /// <inheritdoc/>
        public Task<int?> ShowSuccessAsync(string title, string message, CancellationToken cancellationToken = default)
            => ShowMessageDialogAsync(MessageDialogType.Success, title, message, new[] { "OK" }, cancellationToken);

        /// <inheritdoc/>
        public Task<int?> ShowSuccessAsync(string title, string message, string[] buttons, CancellationToken cancellationToken = default)
            => ShowMessageDialogAsync(MessageDialogType.Success, title, message, buttons, cancellationToken);

        /// <inheritdoc/>
        public Task<int?> ShowConfirmAsync(string title, string message, CancellationToken cancellationToken = default)
            => ShowMessageDialogAsync(MessageDialogType.Confirm, title, message, new[] { "Yes", "No" }, cancellationToken);

        /// <inheritdoc/>
        public Task<int?> ShowConfirmAsync(string title, string message, string[] buttons, CancellationToken cancellationToken = default)
            => ShowMessageDialogAsync(MessageDialogType.Confirm, title, message, buttons, cancellationToken);

        #endregion

        #region Helpers

        /// <summary>
        /// Shows a message dialog with the specified type, title, message, and buttons.
        /// This is the single bind method that implements all message dialog scenarios.
        /// </summary>
        private async Task<int?> ShowMessageDialogAsync(
            MessageDialogType dialogType,
            string title,
            string message,
            string[] buttons,
            CancellationToken cancellationToken)
        {
            if (XamlRoot == null)
                throw new InvalidOperationException("XamlRoot must be set before showing dialogs.");

            cancellationToken.ThrowIfCancellationRequested();

            // Create content with icon and message
            var content = CreateMessageContent(dialogType, message);

            // Create dialog
            var dialog = new ContentDialog
            {
                XamlRoot = XamlRoot,
                Title = title,
                Content = content,
                DefaultButton = ContentDialogButton.Primary
            };

            // Set buttons
            if (buttons.Length > 0)
                dialog.PrimaryButtonText = buttons[0];
            if (buttons.Length > 1)
                dialog.SecondaryButtonText = buttons[1];
            if (buttons.Length > 2)
                dialog.CloseButtonText = buttons[2];

            // Show dialog with cancellation support
            var result = await ShowDialogWithCancellationAsync(dialog, cancellationToken);

            // Map ContentDialogResult to button index
            return result switch
            {
                ContentDialogResult.Primary => 0,
                ContentDialogResult.Secondary => 1,
                ContentDialogResult.None when !string.IsNullOrEmpty(dialog.CloseButtonText) => 2,
                _ => null
            };
        }

        /// <summary>
        /// Creates the content UI for a message dialog with icon and text.
        /// Uses Segoe MDL2 Assets icons for visual feedback.
        /// </summary>
        private FrameworkElement CreateMessageContent(MessageDialogType dialogType, string message)
        {
            var icon = GetIconForDialogType(dialogType);
            var iconColor = GetColorForDialogType(dialogType);

            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 16
            };

            var fontIcon = new FontIcon
            {
                Glyph = icon,
                FontSize = 32,
                Foreground = new SolidColorBrush(iconColor)
            };

            var textBlock = new TextBlock
            {
                Text = message,
                TextWrapping = TextWrapping.Wrap,
                VerticalAlignment = VerticalAlignment.Center
            };

            stackPanel.Children.Add(fontIcon);
            stackPanel.Children.Add(textBlock);

            return stackPanel;
        }

        /// <summary>
        /// Gets the Segoe MDL2 Assets icon glyph for the specified dialog type.
        /// </summary>
        private static string GetIconForDialogType(MessageDialogType dialogType) => dialogType switch
        {
            MessageDialogType.Error => "\uE783",      // ErrorBadge
            MessageDialogType.Success => "\uE73E",    // CheckMark
            MessageDialogType.Warning => "\uE7BA",    // Warning
            MessageDialogType.Info => "\uE946",       // Info
            MessageDialogType.Confirm => "\uE11B",    // Help
            _ => "\uE946"                              // Default to Info
        };

        /// <summary>
        /// Gets the color for the specified dialog type icon.
        /// </summary>
        private static Windows.UI.Color GetColorForDialogType(MessageDialogType dialogType) => dialogType switch
        {
            MessageDialogType.Error => Colors.Red,
            MessageDialogType.Success => Colors.Green,
            MessageDialogType.Warning => Colors.Orange,
            MessageDialogType.Info => Colors.Blue,
            MessageDialogType.Confirm => Colors.Blue,
            _ => Colors.Gray
        };

        /// <summary>
        /// Shows a ContentDialog with cancellation token support.
        /// If the cancellation token is triggered, the dialog is hidden and OperationCanceledException is caught.
        /// </summary>
        private async Task<ContentDialogResult> ShowDialogWithCancellationAsync(
            ContentDialog dialog,
            CancellationToken cancellationToken)
        {
            // Register cancellation callback to hide dialog
            using var registration = cancellationToken.Register(() =>
            {
                _dispatcherService.Run(() => dialog.Hide());
            });

            try
            {
                return await dialog.ShowAsync();
            }
            catch (OperationCanceledException)
            {
                // Dialog was cancelled via cancellation token
                return ContentDialogResult.None;
            }
        }

        #endregion
    }
}