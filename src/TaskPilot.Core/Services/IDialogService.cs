using TaskPilot.Core.Components.Data;

namespace TaskPilot.Core.Services
{
    /// <summary>
    /// Provides platform-agnostic dialog presentation services.
    /// Implementations resolve ViewModels via dependency injection and display dialogs
    /// using framework-specific UI (ContentDialog for WinUI, Window for WPF).
    /// </summary>
    public interface IDialogService
    {
        /// <summary>
        /// Shows a dialog asynchronously based on the provided request configuration.
        /// The dialog ViewModel is resolved from the service provider using the ViewModelType
        /// specified in the request. If the ViewModel implements <see cref="IFormViewModel{TData}"/>,
        /// the request data is injected before presentation.
        /// </summary>
        /// <param name="request">The dialog configuration containing ViewModel type, data, and button definitions.</param>
        /// <param name="cancellationToken">Cancellation token to abort the dialog operation.</param>
        /// <returns>A task that completes when the dialog is closed. The <paramref name="request"/>.Result
        /// property is updated with the index of the button clicked (0-based, or null if cancelled).</returns>
        /// <exception cref="InvalidOperationException">Thrown if the ViewModel type cannot be resolved from services.</exception>
        Task ShowDialogAsync(DialogRequestBase request, CancellationToken cancellationToken = default);
    }
}
