namespace TaskPilot.Core.ViewModel
{
    /// <summary>
    /// Defines the contract for page ViewModels used with <see cref="Services.INavigationService"/>.
    /// Provides identification, data binding, and display title support.
    /// </summary>
    public interface IPageViewModel
    {
        #region Properties

        /// <summary>
        /// Gets or sets the unique identifier for this ViewModel instance.
        /// Assigned by the navigation service during navigation.
        /// </summary>
        Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the data payload passed during navigation.
        /// </summary>
        object? Data { get; set; }

        /// <summary>
        /// Gets the display title for the page.
        /// </summary>
        string Title { get; }

        #endregion
    }
}
