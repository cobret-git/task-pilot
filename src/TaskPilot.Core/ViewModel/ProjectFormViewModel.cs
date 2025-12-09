using CommunityToolkit.Mvvm.ComponentModel;
using TaskPilot.Core.Components.Data;

namespace TaskPilot.Core.ViewModel
{
    /// <summary>
    /// ViewModel for the project form dialog.
    /// Handles creating and editing projects with data binding support.
    /// </summary>
    public partial class ProjectFormViewModel : ObservableObject, IFormViewModel<ProjectFormData>
    {
        #region Fields

        private ProjectFormData? _data;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the project form data containing the project and action.
        /// </summary>
        public ProjectFormData? Data
        {
            get => _data;
            set => SetProperty(ref _data, value);
        }

        #endregion
    }
}
