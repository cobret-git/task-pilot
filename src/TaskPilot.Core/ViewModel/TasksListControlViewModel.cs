using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using TaskPilot.Core.Components.Data;
using TaskPilot.Core.Components.Entities;
using TaskPilot.Core.Services;

namespace TaskPilot.Core.ViewModel
{
    /// <summary>
    /// ViewModel for the TasksListControl.
    /// Provides task filtering, sorting, and display logic for reuse across pages.
    /// </summary>
    public partial class TasksListControlViewModel : ObservableObject, IDisposable
    {
        #region Fields
        private readonly ITaskPilotDataService _dataService;
        private readonly IDispatcherService _dispatcherService;
        private readonly IDialogService _dialogService;
        private readonly INavigationService _navigationService;
        
        private List<TaskItem> _allTasks = new();
        private CancellationTokenSource? _loadCancellationTokenSource;
        private CancellationTokenSource? _searchDebounceCancellationTokenSource;
        private bool _disposed;
        
        private string _searchText = string.Empty;
        private Project? _selectedProject;
        private Milestone? _selectedMilestone;
        private bool _showProject = true;
        private bool _showMilestone = true;
        
        private TaskSortBy _sortBy = TaskSortBy.Name;
        private bool _sortAscending = true;
        
        private bool _showNotStarted = true;
        private bool _showInProgress = true;
        private bool _showCompleted = false;
        private bool _showCancelled = false;
        
        private bool _sortByName = true;
        private bool _sortByPriority = false;
        private bool _sortByCreatedDate = false;
        private bool _sortByUpdatedDate = false;
        private bool _sortByDueDate = false;
        private bool _sortByStartDate = false;
        
        private bool _isLoading;
        
        [ObservableProperty] private TaskItem? _selectedTask;
        #endregion

        #region Constructors
        public TasksListControlViewModel(
            ITaskPilotDataService dataService,
            IDispatcherService dispatcherService,
            IDialogService dialogService,
            INavigationService navigationService)
        {
            _dataService = dataService;
            _dispatcherService = dispatcherService;
            _dialogService = dialogService;
            _navigationService = navigationService;
            
            Tasks = new ObservableCollection<TaskItem>();
        }
        #endregion

        #region Properties
        
        /// <summary>
        /// Gets the filtered and sorted collection of tasks to display.
        /// </summary>
        public ObservableCollection<TaskItem> Tasks { get; }
        
        /// <summary>
        /// Gets or sets the search text for filtering tasks.
        /// Bound from parent control's dependency property.
        /// </summary>
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    DebouncedSearchAsync();
                }
            }
        }
        
        /// <summary>
        /// Gets or sets the selected project for filtering tasks.
        /// </summary>
        public Project? SelectedProject
        {
            get => _selectedProject;
            set
            {
                if (SetProperty(ref _selectedProject, value))
                {
                    _ = RefreshTasksAsync();
                }
            }
        }
        
        /// <summary>
        /// Gets or sets the selected milestone for filtering tasks.
        /// </summary>
        public Milestone? SelectedMilestone
        {
            get => _selectedMilestone;
            set
            {
                if (SetProperty(ref _selectedMilestone, value))
                {
                    _ = RefreshTasksAsync();
                }
            }
        }
        
        /// <summary>
        /// Gets or sets whether to show project information in task items.
        /// </summary>
        public bool ShowProject
        {
            get => _showProject;
            set => SetProperty(ref _showProject, value);
        }
        
        /// <summary>
        /// Gets or sets whether to show milestone information in task items.
        /// </summary>
        public bool ShowMilestone
        {
            get => _showMilestone;
            set => SetProperty(ref _showMilestone, value);
        }
        
        /// <summary>
        /// Gets or sets whether tasks are currently being loaded.
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            private set => SetProperty(ref _isLoading, value);
        }
        
        /// <summary>
        /// Gets whether any tasks exist in the current scope (before filtering).
        /// </summary>
        public bool TasksExistInDatabase => _allTasks.Any();
        
        /// <summary>
        /// Gets whether any tasks match current filters.
        /// </summary>
        public bool TasksMatchCurrentFilter => Tasks.Any();
        
        /// <summary>
        /// Gets whether to show the "create first task" empty state.
        /// </summary>
        public bool ShowCreateFirstTaskPrompt => !TasksExistInDatabase;
        
        /// <summary>
        /// Gets whether to show the "no search results" message.
        /// </summary>
        public bool ShowNoSearchResultsMessage => TasksExistInDatabase && !TasksMatchCurrentFilter;
        
        /// <summary>
        /// Gets the count of visible tasks.
        /// </summary>
        public int TaskCount => Tasks.Count;
        
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
        
        public TaskSortBy SortBy
        {
            get => _sortBy;
            private set
            {
                if (SetProperty(ref _sortBy, value))
                    ApplyFilterAndSort();
            }
        }
        
        public bool SortAscending
        {
            get => _sortAscending;
            set
            {
                if (SetProperty(ref _sortAscending, value))
                    ApplyFilterAndSort();
            }
        }
        
        public bool SortDescending
        {
            get => !_sortAscending;
            set
            {
                if (SetProperty(ref _sortAscending, !value, nameof(SortAscending)))
                    ApplyFilterAndSort();
            }
        }
        
        #endregion

        #region Commands
        
        /// <summary>
        /// Refreshes the tasks list from the database.
        /// </summary>
        [RelayCommand]
        private async Task RefreshTasksAsync()
        {
            // Cancel any existing load operation
            _loadCancellationTokenSource?.Cancel();
            _loadCancellationTokenSource = new CancellationTokenSource();
            var token = _loadCancellationTokenSource.Token;
            
            IsLoading = true;
            
            try
            {
                Result<IEnumerable<TaskItem>> result;
                
                // Load tasks based on scope
                if (SelectedMilestone != null)
                {
                    result = await _dataService.GetTasksByMilestoneIdAsync(SelectedMilestone.Id);
                }
                else if (SelectedProject != null)
                {
                    result = await _dataService.GetTasksByProjectIdAsync(SelectedProject.Id);
                }
                else
                {
                    result = await _dataService.GetAllTasksAsync();
                }
                
                // Check for cancellation
                if (token.IsCancellationRequested) return;
                
                if (result.IsSuccess && result.Data != null)
                {
                    _allTasks = result.Data.ToList();
                    ApplyFilterAndSort();
                    
                    Serilog.Log.Information("Tasks refreshed successfully. Count: {Count}", _allTasks.Count);
                }
                else
                {
                    Serilog.Log.Warning("Failed to refresh tasks: {Message}", result.ErrorMessage);
                    _allTasks.Clear();
                    ApplyFilterAndSort();
                }
            }
            catch (OperationCanceledException)
            {
                Serilog.Log.Debug("Task refresh cancelled");
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error refreshing tasks");
                _allTasks.Clear();
                ApplyFilterAndSort();
            }
            finally
            {
                if (!token.IsCancellationRequested)
                {
                    IsLoading = false;
                }
            }
        }
        
        /// <summary>
        /// Opens the selected task (shows message for now).
        /// </summary>
        [RelayCommand]
        private async Task OpenTaskAsync(TaskItem? task)
        {
            if (task == null) return;
            
            await _dialogService.ShowInfoAsync(
                "Task Details",
                $"Task detail page not implemented yet.\n\nTask: {task.Title}");
        }
        
        /// <summary>
        /// Toggles task completion status.
        /// </summary>
        [RelayCommand]
        private async Task ToggleCompletedAsync(TaskItem? task)
        {
            if (task == null) return;
            
            var newStatus = task.Status == Components.Data.TaskStatus.Completed
                ? Components.Data.TaskStatus.NotStarted
                : Components.Data.TaskStatus.Completed;
            
            await ChangeTaskStatusAsync(task, newStatus);
        }
        
        /// <summary>
        /// Changes task status.
        /// </summary>
        [RelayCommand]
        private async Task ChangeTaskStatusAsync(object? parameter)
        {
            if (SelectedTask == null || parameter is not Components.Data.TaskStatus newStatus)
                return;
            
            await ChangeTaskStatusAsync(SelectedTask, newStatus);
        }
        
        private async Task ChangeTaskStatusAsync(TaskItem task, Components.Data.TaskStatus newStatus)
        {
            try
            {
                task.Status = newStatus;
                task.UpdatedAt = DateTime.UtcNow;
                
                var result = await _dataService.UpdateTaskAsync(task);
                
                if (result.IsSuccess)
                {
                    Serilog.Log.Information("Task status updated: {TaskId} -> {Status}", task.Id, newStatus);
                    await RefreshTasksAsync();
                }
                else
                {
                    Serilog.Log.Warning("Failed to update task status: {Message}", result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error updating task status");
            }
        }
        
        /// <summary>
        /// Edits the selected task.
        /// </summary>
        [RelayCommand]
        private async Task EditTaskAsync(TaskItem? task)
        {
            if (task == null) return;
            
            var request = new TaskFormPageRequest(task, FormDialogAction.Edit, task.Project, task.Milestone);
            await _navigationService.NavigateToAsync(request);
        }
        
        /// <summary>
        /// Deletes the selected task.
        /// </summary>
        [RelayCommand]
        private async Task DeleteTaskAsync(TaskItem? task)
        {
            if (task == null) return;
            
            var confirmed = await _dialogService.ShowConfirmAsync(
                "Delete Task",
                $"Are you sure you want to delete '{task.Title}'?", 
                new string[] {"Delete", "Cancel" }
                );
            
            if (confirmed != 0) return;
            
            try
            {
                var result = await _dataService.DeleteTaskAsync(task.Id);
                
                if (result.IsSuccess)
                {
                    Serilog.Log.Information("Task deleted: {TaskId}", task.Id);
                    await RefreshTasksAsync();
                }
                else
                {
                    Serilog.Log.Warning("Failed to delete task: {Message}", result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error deleting task");
            }
        }
        
        #endregion

        #region Helpers
        
        /// <summary>
        /// Applies filter and sort to the tasks collection.
        /// </summary>
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
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    var query = SearchText.Trim().ToLowerInvariant();
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
                    PopulateKnownNavigationProperties(task);
                    Tasks.Add(task);
                }
                
                OnPropertyChanged(nameof(TasksExistInDatabase));
                OnPropertyChanged(nameof(TasksMatchCurrentFilter));
                OnPropertyChanged(nameof(ShowCreateFirstTaskPrompt));
                OnPropertyChanged(nameof(ShowNoSearchResultsMessage));
                OnPropertyChanged(nameof(TaskCount));
            });
        }
        
        /// <summary>
        /// Debounces search input to avoid excessive filtering operations.
        /// </summary>
        private async void DebouncedSearchAsync()
        {
            _searchDebounceCancellationTokenSource?.Cancel();
            _searchDebounceCancellationTokenSource = new CancellationTokenSource();
            var token = _searchDebounceCancellationTokenSource.Token;
            
            try
            {
                await Task.Delay(300, token);
                if (!token.IsCancellationRequested)
                {
                    ApplyFilterAndSort();
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when typing quickly
            }
        }

        /// <summary>
        /// Populates navigation properties that are known from the current context
        /// but not included in the database query for performance reasons.
        /// </summary>
        private void PopulateKnownNavigationProperties(TaskItem task)
        {
            if (SelectedMilestone != null && task.Milestone == null)
                task.Milestone = SelectedMilestone;

            if (SelectedProject != null && task.Project == null)
                task.Project = SelectedProject;
        }
        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (_disposed) return;
            
            _loadCancellationTokenSource?.Cancel();
            _loadCancellationTokenSource?.Dispose();
            
            _searchDebounceCancellationTokenSource?.Cancel();
            _searchDebounceCancellationTokenSource?.Dispose();
            
            _dispatcherService.Run(() =>
            {
                Tasks.Clear();
                _allTasks.Clear();
            });
            
            _disposed = true;
            
            Serilog.Log.Debug("TasksListControlViewModel disposed");
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
