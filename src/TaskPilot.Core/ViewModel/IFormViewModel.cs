namespace TaskPilot.Core.ViewModel
{
    /// <summary>
    /// Generic interface for ViewModels that receive typed data from dialog requests.
    /// Used in dialogs presented via <see cref="IDialogService"/>.
    /// The <see cref="Data"/> property is populated by the dialog service before the dialog is shown.
    /// </summary>
    /// <typeparam name="TData">The type of data this ViewModel expects to receive from the dialog request.</typeparam>
    /// <remarks>
    /// This interface enables the dialog service to:
    /// 1. Identify ViewModels that expect data injection
    /// 2. Pass dialog-specific data from <see cref="DialogRequestBase.Data"/>
    /// 3. Provide compile-time type safety for different dialog scenarios
    ///
    /// Implementations automatically receive the dialog data before presentation.
    /// </remarks>
    /// <example>
    /// public class ProjectFormViewModel : ObservableObject, IFormViewModel&lt;ProjectFormData&gt;
    /// {
    ///     public ProjectFormData? Data { get; set; }
    /// }
    /// </example>
    public interface IFormViewModel<TData> where TData : class
    {
        /// <summary>
        /// Gets or sets the typed dialog data payload.
        /// Set by <see cref="IDialogService"/> before dialog presentation.
        /// </summary>
        TData? Data { get; set; }
    }
}
