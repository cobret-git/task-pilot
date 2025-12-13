using TaskPilot.Core.Services;

namespace TaskPilot.Core.ViewModel
{
    public partial class ProjectPageViewModel : ObservablePageViewModelBase
    {
        #region Constructors
        public ProjectPageViewModel(INavigationService navigationService) : base(navigationService)
        {
        }
        #endregion
    }
}
