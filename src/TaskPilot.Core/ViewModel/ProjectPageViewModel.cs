using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
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
        private List<TaskItem> _allTasks = new();
        private string _searchQuery = string.Empty;
        private TaskSortBy _sortBy = TaskSortBy.Name;
        private bool _sortAscending = true;
        
        // Status filters - default: NotStarted and InProgress visible
        private bool _showNotStarted = true;
        private bool _showInProgress = true;
        private bool _showCompleted = false;
        private bool _showCancelled = false;
        
        // Sort options
        private bool _sortByName = true;
        private bool _sortByPriority = false;
        private bool _sortByCreatedDate = false;
        private bool _sortByUpdatedDate = false;
        private bool _sortByDueDate = false;
        private bool _sortByStartDate = false;
        
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
            IDialogService dialogService)
            : base(navigationService)
        {
            _dataService = dataService;
            _dispatcherService = dispatcherService;
            _dialogService = dialogService;
            Tasks = new ObservableCollection<TaskItem>();
        }
        #endregion

        #region Properties
        
        public Project? CurrentProject
        {
            get => _currentProject;
            set
            {
                if (SetProperty(ref _currentProject, value))
                {
                    OnPropertyChanged(nameof(TaskCount));
                    OnPropertyChanged(nameof(ProjectColor));
                }
            }
        }
        
        public string? ProjectColor => CurrentProject?.Color;
        
        public ObservableCollection<TaskItem> Tasks { get; }
        
        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (SetProperty(ref _searchQuery, value))
                    ApplyFilterAndSort();
            }
        }
        
        public TaskSortBy SortBy
        {
            get => _sortBy;
            set
            {
                if (SetProperty(ref _sortBy, value))
                    ApplyFilterAndSort();
                OnSortByChanged(value);
            }
        }
        
        public bool SortAscending
        {
            get => _sortAscending;
            set
            {
                if (SetProperty(ref _sortAscending, value))
                {
                    SetProperty(ref _sortDescending, !value, nameof(SortDescending));
                    ApplyFilterAndSort();
                }
            }
        }
        
        private bool _sortDescending = false;
        public bool SortDescending
        {
            get => _sortDescending;
            set
            {
                if (SetProperty(ref _sortDescending, value))
                {
                    SetProperty(ref _sortAscending, !value, nameof(SortAscending));
                    ApplyFilterAndSort();
                }
            }
        }
        
        // Status filter properties
        public bool ShowNotStarted
        {
            get => _showNotStarted;
            set
            {
                if (SetProperty(ref _showNotStarted, value))
                    ApplyFilterAndSort();
            }
        }
        
        public bool ShowInProgress
        {
            get => _showInProgress;
            set
            {
                if (SetProperty(ref _showInProgress, value))
                    ApplyFilterAndSort();
            }
        }
        
        public bool ShowCompleted
        {
            get => _showCompleted;
            set
            {
                if (SetProperty(ref _showCompleted, value))
                    ApplyFilterAndSort();
            }
        }
        
        public bool ShowCancelled
        {
            get => _showCancelled;
            set
            {
                if (SetProperty(ref _showCancelled, value))
                    ApplyFilterAndSort();
            }
        }
        
        // Sort option properties
        public bool SortByName
        {
            get => _sortByName;
            set
            {
                if (SetProperty(ref _sortByName, value) && value)
                {
                    SortByPriority = false;
                    SortByCreatedDate = false;
                    SortByUpdatedDate = false;
                    SortByDueDate = false;
                    SortByStartDate = false;
                    SortBy = TaskSortBy.Name;
                }
            }
        }
        
        public bool SortByPriority
        {
            get => _sortByPriority;
            set
            {
                if (SetProperty(ref _sortByPriority, value) && value)
                {
                    SortByName = false;
                    SortByCreatedDate = false;
                    SortByUpdatedDate = false;
                    SortByDueDate = false;
                    SortByStartDate = false;
                    SortBy = TaskSortBy.Priority;
                }
            }
        }
        
        public bool SortByCreatedDate
        {
            get => _sortByCreatedDate;
            set
            {
                if (SetProperty(ref _sortByCreatedDate, value) && value)
                {
                    SortByName = false;
                    SortByPriority = false;
                    SortByUpdatedDate = false;
                    SortByDueDate = false;
                    SortByStartDate = false;
                    SortBy = TaskSortBy.CreatedDate;
                }
            }
        }
        
        public bool SortByUpdatedDate
        {
            get => _sortByUpdatedDate;
            set
            {
                if (SetProperty(ref _sortByUpdatedDate, value) && value)
                {
                    SortByName = false;
                    SortByPriority = false;
                    SortByCreatedDate = false;
                    SortByDueDate = false;
                    SortByStartDate = false;
                    SortBy = TaskSortBy.UpdatedDate;
                }
            }
        }
        
        public bool SortByDueDate
        {
            get => _sortByDueDate;
            set
            {
                if (SetProperty(ref _sortByDueDate, value) && value)
                {
                    SortByName = false;
                    SortByPriority = false;
                    SortByCreatedDate = false;
                    SortByUpdatedDate = false;
                    SortByStartDate = false;
                    SortBy = TaskSortBy.DueDate;
                }
            }
        }
        
        public bool SortByStartDate
        {
            get => _sortByStartDate;
            set
            {
                if (SetProperty(ref _sortByStartDate, value) && value)
                {
                    SortByName = false;
                    SortByPriority = false;
                    SortByCreatedDate = false;
                    SortByUpdatedDate = false;
                    SortByDueDate = false;
                    SortBy = TaskSortBy.StartDate;
                }
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
        
        public bool TasksExistInDatabase => _allTasks.Any();
        public bool TasksMatchCurrentFilter => Tasks.Any();
        public bool ShowCreateFirstTaskPrompt => !TasksExistInDatabase;
        public bool ShowNoSearchResultsMessage => TasksExistInDatabase && !TasksMatchCurrentFilter;
        public string TaskCount => $"{_allTasks.Count} task{(_allTasks.Count != 1 ? "s" : "")}";
        
        #endregion

        #region Commands
        
        [RelayCommand(CanExecute = nameof(CanRefresh))]
        private async Task RefreshTasksAsync()
        {
            if (IsRefreshing || CurrentProject == null) return;
            
            IsRefreshing = true;
            IsBusy = true;
            
            try
            {
                var result = await _dataService.GetTasksByProjectIdAsync(CurrentProject.Id);
                
                if (result.IsSuccess && result.Data != null)
                {
                    _allTasks = result.Data.ToList();
                    ApplyFilterAndSort();
                    Serilog.Log.Information("Tasks refreshed for project {ProjectId}. Count: {Count}", 
                        CurrentProject.Id, _allTasks.Count);
                }
                else
                {
                    Serilog.Log.Warning("Failed to refresh tasks: {Error}", result.ErrorMessage);
                    await _dialogService.ShowErrorAsync(
                        "Database Error", 
                        $"Failed to load tasks: {result.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error refreshing tasks");
                await _dialogService.ShowErrorAsync(
                    "Unexpected Error", 
                    "An unexpected error occurred while refreshing tasks.");
            }
            finally
            {
                IsRefreshing = false;
                IsBusy = false;
            }
        }
        
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
        
        [RelayCommand(CanExecute = nameof(CanOpenTask))]
        private async Task OpenTaskAsync(TaskItem? task)
        {
            if (task == null) return;
            
            try
            {
                IsBusy = true;
                
                // TODO: Navigate to task detail page when implemented
                await _dialogService.ShowInfoAsync(
                    "Not Implemented", 
                    "Task detail page will be implemented in a future update.");
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error opening task {TaskId}", task.Id);
                await _dialogService.ShowErrorAsync(
                    "Unexpected Error", 
                    "An unexpected error occurred while opening the task.");
            }
            finally
            {
                IsBusy = false;
            }
        }
        
        [RelayCommand(CanExecute = nameof(CanEditTask))]
        private async Task EditTaskAsync(TaskItem? task)
        {
            if (task == null)
            {
                Serilog.Log.Warning("EditTask called with null task");
                return;
            }

            try
            {
                IsBusy = true;

                var request = new TaskFormPageRequest(task, FormDialogAction.Edit, CurrentProject, null);
                var navResult = await _navigationService.NavigateToAsync(request);

                if (!navResult.Success)
                {
                    Serilog.Log.Warning("Failed to navigate to task form for editing: {Error}", navResult.ErrorMessage);
                    await _dialogService.ShowErrorAsync(
                        "Navigation Error",
                        "Failed to navigate to task edit form.");
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error editing task {TaskId}", task.Id);
                await _dialogService.ShowErrorAsync(
                    "Unexpected Error", 
                    "An unexpected error occurred while editing the task.");
            }
            finally
            {
                IsBusy = false;
            }
        }
        
        [RelayCommand(CanExecute = nameof(CanDeleteTask))]
        private async Task DeleteTaskAsync(TaskItem? task)
        {
            if (task == null) return;
            
            var confirmResult = await _dialogService.ShowConfirmAsync(
                "Confirm Delete",
                $"Are you sure you want to delete '{task.Title}'?\n\nThis action cannot be undone.",
                new[] { "Delete", "Cancel" });
            
            if (confirmResult != 0) return;
            
            try
            {
                IsBusy = true;
                
                var result = await _dataService.DeleteTaskAsync(task.Id);
                
                if (result.IsSuccess)
                {
                    await _dialogService.ShowSuccessAsync(
                        "Success", 
                        $"Task '{task.Title}' was deleted successfully.");
                    Serilog.Log.Information("Task deleted: {TaskId} - {Title}", task.Id, task.Title);
                    await RefreshTasksAsync();
                }
                else
                {
                    Serilog.Log.Warning("Failed to delete task {TaskId}: {Error}", 
                        task.Id, result.ErrorMessage);
                    await _dialogService.ShowErrorAsync(
                        "Delete Failed", 
                        $"Failed to delete task: {result.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error deleting task {TaskId}", task.Id);
                await _dialogService.ShowErrorAsync(
                    "Unexpected Error", 
                    "An unexpected error occurred while deleting the task.");
            }
            finally
            {
                IsBusy = false;
            }
        }
        
        [RelayCommand(CanExecute = nameof(CanToggleCompleted))]
        private async Task ToggleCompletedAsync(TaskItem? task)
        {
            if (task == null) return;
            
            try
            {
                // Toggle between Completed and NotStarted
                task.Status = task.Status == Components.Data.TaskStatus.Completed
                    ? Components.Data.TaskStatus.NotStarted
                    : Components.Data.TaskStatus.Completed;
                
                var result = await _dataService.UpdateTaskAsync(task);
                
                if (result.IsSuccess)
                {
                    Serilog.Log.Information("Task status toggled: {TaskId} -> {Status}", 
                        task.Id, task.Status);
                    await RefreshTasksAsync();
                }
                else
                {
                    Serilog.Log.Warning("Failed to update task status {TaskId}: {Error}", 
                        task.Id, result.ErrorMessage);
                    await _dialogService.ShowErrorAsync(
                        "Update Failed", 
                        $"Failed to update task status: {result.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error toggling task status {TaskId}", task.Id);
                await _dialogService.ShowErrorAsync(
                    "Unexpected Error", 
                    "An unexpected error occurred while updating task status.");
            }
        }
        
        [RelayCommand(CanExecute = nameof(CanChangeTaskStatus))]
        private async Task ChangeTaskStatusAsync(Components.Data.TaskStatus newStatus)
        {
            if (SelectedTask == null) return;
            
            try
            {
                SelectedTask.Status = newStatus;
                var result = await _dataService.UpdateTaskAsync(SelectedTask);
                
                if (result.IsSuccess)
                {
                    Serilog.Log.Information("Task status changed: {TaskId} -> {Status}",
                        SelectedTask.Id, newStatus);
                    await RefreshTasksAsync();
                }
                else
                {
                    Serilog.Log.Warning("Failed to change task status {TaskId}: {Error}",
                        SelectedTask.Id, result.ErrorMessage);
                    await _dialogService.ShowErrorAsync(
                        "Update Failed", 
                        $"Failed to change task status: {result.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error changing task status {TaskId}", SelectedTask.Id);
                await _dialogService.ShowErrorAsync(
                    "Unexpected Error", 
                    "An unexpected error occurred while changing task status.");
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
        
        [RelayCommand]
        private void ToggleSortOrder()
        {
            SortAscending = !SortAscending;
        }
        
        #endregion

        #region CanExecute
        
        private bool CanRefresh() => !IsBusy && !IsRefreshing;
        private bool CanCreateTask() => !IsBusy && CurrentProject != null;
        private bool CanOpenTask() => !IsBusy;
        private bool CanEditTask() => !IsBusy;
        private bool CanDeleteTask() => !IsBusy;
        private bool CanToggleCompleted() => !IsBusy;
        private bool CanChangeTaskStatus() => !IsBusy;
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
                Title = project.Name;
                _ = RefreshTasksAsync();
            }
        }
        
        private void OnSortByChanged(TaskSortBy value)
        {
            // Update UI properties when sort changes
        }
        
        #endregion

        #region Helpers
        
        private void ApplyFilterAndSort()
        {
            _dispatcherService.Run(() =>
            {
                IEnumerable<TaskItem> filtered = _allTasks;
                
                // Filter by status
                filtered = filtered.Where(t =>
                    (ShowNotStarted && t.Status == Components.Data.TaskStatus.NotStarted) ||
                    (ShowInProgress && t.Status == Components.Data.TaskStatus.InProgress) ||
                    (ShowCompleted && t.Status == Components.Data.TaskStatus.Completed) ||
                    (ShowCancelled && t.Status == Components.Data.TaskStatus.Cancelled));
                
                // Filter by search query
                if (!string.IsNullOrWhiteSpace(SearchQuery))
                {
                    var query = SearchQuery.Trim().ToLowerInvariant();
                    filtered = filtered.Where(t => 
                        t.Title.ToLowerInvariant().Contains(query));
                }
                
                // Sort
                filtered = SortBy switch
                {
                    TaskSortBy.Name => SortAscending
                        ? filtered.OrderBy(t => t.Title)
                        : filtered.OrderByDescending(t => t.Title),
                    
                    TaskSortBy.Priority => SortAscending
                        ? filtered.OrderBy(t => t.Priority)
                        : filtered.OrderByDescending(t => t.Priority),
                    
                    TaskSortBy.CreatedDate => SortAscending
                        ? filtered.OrderBy(t => t.CreatedAt)
                        : filtered.OrderByDescending(t => t.CreatedAt),
                    
                    TaskSortBy.UpdatedDate => SortAscending
                        ? filtered.OrderBy(t => t.UpdatedAt ?? t.CreatedAt)
                        : filtered.OrderByDescending(t => t.UpdatedAt ?? t.CreatedAt),
                    
                    TaskSortBy.DueDate => SortAscending
                        ? filtered.OrderBy(t => t.DueDate ?? DateTime.MaxValue)
                        : filtered.OrderByDescending(t => t.DueDate ?? DateTime.MinValue),
                    
                    TaskSortBy.StartDate => SortAscending
                        ? filtered.OrderBy(t => t.StartDate ?? DateTime.MaxValue)
                        : filtered.OrderByDescending(t => t.StartDate ?? DateTime.MinValue),
                    
                    _ => filtered.OrderBy(t => t.Title)
                };
                
                Tasks.Clear();
                foreach (var task in filtered)
                {
                    Tasks.Add(task);
                }
                
                OnPropertyChanged(nameof(TasksExistInDatabase));
                OnPropertyChanged(nameof(TasksMatchCurrentFilter));
                OnPropertyChanged(nameof(ShowCreateFirstTaskPrompt));
                OnPropertyChanged(nameof(ShowNoSearchResultsMessage));
                OnPropertyChanged(nameof(TaskCount));
            });
        }
        
        private void NotifyCanExecuteChanged()
        {
            _dispatcherService.Run(() =>
            {
                RefreshTasksCommand.NotifyCanExecuteChanged();
                CreateTaskCommand.NotifyCanExecuteChanged();
                OpenTaskCommand.NotifyCanExecuteChanged();
                EditTaskCommand.NotifyCanExecuteChanged();
                DeleteTaskCommand.NotifyCanExecuteChanged();
                ToggleCompletedCommand.NotifyCanExecuteChanged();
                ChangeTaskStatusCommand.NotifyCanExecuteChanged();
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
            
            _dispatcherService.Run(() =>
            {
                Tasks.Clear();
                _allTasks.Clear();
            });
            
            _disposed = true;
            
            Serilog.Log.Debug("ProjectPageViewModel disposed");
        }
        
        #endregion
    }

    /// <summary>
    /// Defines sorting options for tasks.
    /// </summary>
    public enum TaskSortBy
    {
        Name,
        Priority,
        CreatedDate,
        UpdatedDate,
        DueDate,
        StartDate
    }
}
