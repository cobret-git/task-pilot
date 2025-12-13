using TaskPilot.Core.Services;

namespace TaskPilot.Core.ViewModel
{
    public partial class ProjectsBrowserViewModel : ObservablePageViewModelBase
    {
        public ProjectsBrowserViewModel(INavigationService navigationService) : base(navigationService)
        {
        }
    }
}
