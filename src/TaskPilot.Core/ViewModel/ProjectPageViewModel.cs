using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TaskPilot.Core.Components.Data;
using TaskPilot.Core.Components.Entities;
using TaskPilot.Core.Services;

namespace TaskPilot.Core.ViewModel
{
    /// <summary>
    /// ViewModel for displaying and managing tasks within a project.
    /// </summary>
    public partial class ProjectPageViewModel : ObservablePageViewModelBase
    {
        #region Fields
        private readonly ITaskPilotDataService _dataService;
        private readonly IDispatcherService _dispatcherService;
        private readonly IDialogService _dialogService;
        private Project? _currentProject;

        private bool _isBusy;
        private bool _isRefreshing;
        private bool _disposed;

        [ObservableProperty] private TaskItem? _selectedTask;
        #endregion

        #region Constructors
        public ProjectPageViewModel(
            INavigationService navigationService,
            ITaskPilotDataService dataService,
            IDispatcherService dispatcherService,
            IDialogService dialogService,
            TasksListControlViewModel tasksListViewModel)
            : base(navigationService)
        {
            _dataService = dataService;
            _dispatcherService = dispatcherService;
            _dialogService = dialogService;
            TasksListViewModel = tasksListViewModel;

        }
        #endregion

        #region Properties

        public Project? CurrentProject
        {
            get => _currentProject;
            set => SetProperty(ref _currentProject, value);
        }

        public string SearchQuery
        {
            get => TasksListViewModel.SearchText;
            set
            {
                if (TasksListViewModel.SearchText == value) return;
                TasksListViewModel.SearchText = value;
                OnPropertyChanged(nameof(SearchQuery));
            }
        }

        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                if (SetProperty(ref _isBusy, value))
                    NotifyCanExecuteChanged();
            }
        }

        public bool IsRefreshing
        {
            get => _isRefreshing;
            private set
            {
                if (SetProperty(ref _isRefreshing, value))
                    NotifyCanExecuteChanged();
            }
        }

        public TasksListControlViewModel TasksListViewModel { get; }
        #endregion

        #region Commands

        [RelayCommand(CanExecute = nameof(CanCreateTask))]
        private async Task CreateTaskAsync()
        {
            if (CurrentProject == null) return;

            try
            {
                IsBusy = true;

                var newTask = new TaskItem()
                {
                    CreatedAt = DateTime.UtcNow,
                    Project = CurrentProject,
                    ProjectId = CurrentProject.Id,
                };

                var request = new TaskFormPageRequest(newTask, FormDialogAction.Create, CurrentProject, null);
                var navResult = await _navigationService.NavigateToAsync(request);

                if (!navResult.Success)
                {
                    Serilog.Log.Warning("Failed to navigate to task form: {Error}", navResult.ErrorMessage);
                    await _dialogService.ShowErrorAsync(
                        "Navigation Error",
                        "Failed to navigate to task creation form.");
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error creating task");
                await _dialogService.ShowErrorAsync(
                    "Unexpected Error",
                    "An unexpected error occurred while creating a task.");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand(CanExecute = nameof(CanNavigateToMilestones))]
        private async Task NavigateToMilestonesAsync()
        {
            if (CurrentProject == null) return;

            try
            {
                IsBusy = true;

                // TODO: Navigate to milestones browser when implemented
                await _dialogService.ShowInfoAsync(
                    "Not Implemented",
                    "Milestones browser will be implemented in a future update.");
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error navigating to milestones");
                await _dialogService.ShowErrorAsync(
                    "Unexpected Error",
                    "An unexpected error occurred.");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand(CanExecute = nameof(CanEditProject))]
        private async Task EditProjectAsync()
        {
            if (CurrentProject == null) return;

            try
            {
                IsBusy = true;

                var request = new ProjectFormPageRequest(CurrentProject, FormDialogAction.Edit);
                var navResult = await _navigationService.NavigateToAsync(request);

                if (!navResult.Success)
                {
                    Serilog.Log.Warning("Failed to navigate to project form: {Error}",
                        navResult.ErrorMessage);
                    await _dialogService.ShowErrorAsync(
                        "Navigation Error",
                        "Failed to navigate to project edit form.");
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error editing project {ProjectId}", CurrentProject?.Id);
                await _dialogService.ShowErrorAsync(
                    "Unexpected Error",
                    "An unexpected error occurred while editing the project.");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand(CanExecute = nameof(CanDeleteProject))]
        private async Task DeleteProjectAsync()
        {
            if (CurrentProject == null) return;

            var confirmResult = await _dialogService.ShowConfirmAsync(
                "Confirm Delete",
                $"Are you sure you want to delete '{CurrentProject.Name}'?\n\n" +
                $"This will also delete all tasks and milestones. This action cannot be undone.",
                new[] { "Delete", "Cancel" });

            if (confirmResult != 0) return;

            try
            {
                IsBusy = true;

                var result = await _dataService.DeleteProjectAsync(CurrentProject.Id);

                if (result.IsSuccess)
                {
                    await _dialogService.ShowSuccessAsync(
                        "Success",
                        $"Project '{CurrentProject.Name}' was deleted successfully.");
                    Serilog.Log.Information("Project deleted: {ProjectId} - {Name}",
                        CurrentProject.Id, CurrentProject.Name);

                    await _navigationService.NavigateToAsync(new ProjectsBrowserPageRequest());
                }
                else
                {
                    Serilog.Log.Warning("Failed to delete project {ProjectId}: {Error}",
                        CurrentProject.Id, result.ErrorMessage);
                    await _dialogService.ShowErrorAsync(
                        "Delete Failed",
                        $"Failed to delete project: {result.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error deleting project {ProjectId}", CurrentProject?.Id);
                await _dialogService.ShowErrorAsync(
                    "Unexpected Error",
                    "An unexpected error occurred while deleting the project.");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand(CanExecute = nameof(CanArchiveProject))]
        private async Task ArchiveProjectAsync()
        {
            if (CurrentProject == null) return;

            var confirmResult = await _dialogService.ShowConfirmAsync(
                "Confirm Archive",
                $"Are you sure you want to archive '{CurrentProject.Name}'?",
                new[] { "Archive", "Cancel" });

            if (confirmResult != 0) return;

            try
            {
                IsBusy = true;

                CurrentProject.IsArchived = true;
                var result = await _dataService.UpdateProjectAsync(CurrentProject);

                if (result.IsSuccess)
                {
                    await _dialogService.ShowSuccessAsync(
                        "Success",
                        $"Project '{CurrentProject.Name}' was archived successfully.");
                    Serilog.Log.Information("Project archived: {ProjectId} - {Name}",
                        CurrentProject.Id, CurrentProject.Name);

                    await _navigationService.NavigateToAsync(new ProjectsBrowserPageRequest());
                }
                else
                {
                    Serilog.Log.Warning("Failed to archive project {ProjectId}: {Error}",
                        CurrentProject.Id, result.ErrorMessage);
                    await _dialogService.ShowErrorAsync(
                        "Archive Failed",
                        $"Failed to archive project: {result.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error archiving project {ProjectId}", CurrentProject?.Id);
                await _dialogService.ShowErrorAsync(
                    "Unexpected Error",
                    "An unexpected error occurred while archiving the project.");
            }
            finally
            {
                IsBusy = false;
            }
        }

        #endregion

        #region CanExecute

        private bool CanCreateTask() => !IsBusy && CurrentProject != null;
        private bool CanNavigateToMilestones() => !IsBusy && CurrentProject != null;
        private bool CanEditProject() => !IsBusy && CurrentProject != null;
        private bool CanDeleteProject() => !IsBusy && CurrentProject != null;
        private bool CanArchiveProject() => !IsBusy && CurrentProject != null;
        #endregion

        #region Handlers

        protected override void OnDataChanged(object? newData)
        {
            base.OnDataChanged(newData);

            if (newData is Project project)
            {
                CurrentProject = project;
                TasksListViewModel.SelectedProject = project;
                Title = project.Name;
            }
        }
        #endregion

        #region Helpers

        private void NotifyCanExecuteChanged()
        {
            _dispatcherService.Run(() =>
            {
                CreateTaskCommand.NotifyCanExecuteChanged();
                NavigateToMilestonesCommand.NotifyCanExecuteChanged();
                EditProjectCommand.NotifyCanExecuteChanged();
                DeleteProjectCommand.NotifyCanExecuteChanged();
                ArchiveProjectCommand.NotifyCanExecuteChanged();
            });
        }

        #endregion

        #region IDisposable

        public override void Dispose()
        {
            base.Dispose();

            if (_disposed) return;

            _disposed = true;

            Serilog.Log.Debug("ProjectPageViewModel disposed");
        }
        #endregion
    }
}
