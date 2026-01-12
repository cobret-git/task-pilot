using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using TaskPilot.Core.Components.Data;
using TaskPilot.Core.Components.Entities;
using TaskPilot.Core.Services;

namespace TaskPilot.Core.ViewModel
{
    /// <summary>
    /// ViewModel for browsing and managing projects.
    /// Displays all available projects with create, edit, and delete operations.
    /// </summary>
    public partial class ProjectsBrowserViewModel : ObservablePageViewModelBase
    {
        #region Fields
        private readonly ITaskPilotDataService _dataService;
        private readonly IDispatcherService _dispatcherService;
        private readonly IDialogService _dialogService;
        private bool _isBusy;
        private bool _isRefreshing;
        private bool _disposed;
        private string _searchQuery = string.Empty;
        private bool _showArchived = false;
        private ProjectSortBy _sortBy = ProjectSortBy.SortOrder;
        private bool _sortByName = false;
        private bool _sortByCreatedDate = false;
        private bool _sortByUpdatedDate = false;
        private bool _sortBySortOrder = true;
        private bool _sortAscending = true;
        private bool _sortDescending = false;
        private bool _projectsExistInDatabase;
        private List<Project> _allProjects = new();
        [ObservableProperty] private Project? _selectedItem;
        #endregion

        #region Constructors
        public ProjectsBrowserViewModel(
            INavigationService navigationService,
            ITaskPilotDataService dataService,
            IDispatcherService dispatcherService,
            IDialogService dialogService)
            : base(navigationService)
        {
            _dataService = dataService;
            _dispatcherService = dispatcherService;
            _dialogService = dialogService;

            Projects = new ObservableCollection<Project>();

            Title = "Projects";
        }
        #endregion

        #region Properties

        /// <summary>
        /// Gets the filtered and sorted collection of projects to display.
        /// </summary>
        public ObservableCollection<Project> Projects { get; }

        /// <summary>
        /// Gets whether any projects exist in the database (regardless of filters/search).
        /// Used to determine if we should show search/filter UI.
        /// </summary>
        public bool ProjectsExistInDatabase
        {
            get => _projectsExistInDatabase;
            private set
            {
                if (SetProperty(ref _projectsExistInDatabase, value))
                {
                    OnPropertyChanged(nameof(ShowCreateFirstProjectPrompt));
                    OnPropertyChanged(nameof(ShowNoSearchResultsMessage));
                }
            }
        }

        /// <summary>
        /// Gets or sets the search query for filtering projects by name.
        /// </summary>
        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (SetProperty(ref _searchQuery, value))
                {
                    ApplyFilterAndSort();
                }
            }
        }

        /// <summary>
        /// Gets or sets whether to show archived projects.
        /// </summary>
        public bool ShowArchived
        {
            get => _showArchived;
            set
            {
                if (SetProperty(ref _showArchived, value))
                {
                    ApplyFilterAndSort();
                }
            }
        }

        /// <summary>
        /// Gets or sets the sorting criteria.
        /// </summary>
        public ProjectSortBy SortBy
        {
            get => _sortBy;
            set
            {
                if (SetProperty(ref _sortBy, value))
                {
                    ApplyFilterAndSort();
                }
                OnSortByChanged(value);
            }
        }

        /// <summary>
        /// Gets or sets the sorting criteria by name.
        /// </summary>
        public bool SortByName
        {
            get => _sortByName;
            set
            {
                if (SetProperty(ref _sortByName, value))
                {
                    if (value)
                    {
                        SortByCreatedDate = false;
                        SortByUpdatedDate = false;
                        SortBySortOrder = false;
                        SortBy = ProjectSortBy.Name;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the sorting criteria by created date.
        /// </summary>
        public bool SortByCreatedDate
        {
            get => _sortByCreatedDate;
            set
            {
                if (SetProperty(ref _sortByCreatedDate, value))
                {
                    if (value)
                    {
                        SortByName = false;
                        SortByUpdatedDate = false;
                        SortBySortOrder = false;
                        SortBy = ProjectSortBy.CreatedDate;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the sorting criteria by updated date.
        /// </summary>
        public bool SortByUpdatedDate
        {
            get => _sortByUpdatedDate;
            set
            {
                if (SetProperty(ref _sortByUpdatedDate, value))
                {
                    if (value)
                    {
                        SortByName = false;
                        SortByCreatedDate = false;
                        SortBySortOrder = false;
                        SortBy = ProjectSortBy.UpdatedDate;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the sorting criteria by sort order.
        /// </summary>
        public bool SortBySortOrder
        {
            get => _sortBySortOrder;
            set
            {
                if (SetProperty(ref _sortBySortOrder, value))
                {
                    if (value)
                    {
                        SortByName = false;
                        SortByCreatedDate = false;
                        SortByUpdatedDate = false;
                        SortBy = ProjectSortBy.SortOrder;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets whether to sort in ascending order.
        /// </summary>
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

        /// <summary>
        /// Gets or sets whether to sort in descending order.
        /// </summary>
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

        /// <summary>
        /// Gets a value indicating whether any operation is in progress.
        /// Used to disable commands during operations.
        /// </summary>
        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                if (SetProperty(ref _isBusy, value))
                {
                    NotifyCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the projects list is currently being refreshed.
        /// </summary>
        public bool IsRefreshing
        {
            get => _isRefreshing;
            private set
            {
                if (SetProperty(ref _isRefreshing, value))
                {
                    NotifyCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// Gets whether any projects match the current search/filter criteria.
        /// Used to show/hide the ListView.
        /// </summary>
        public bool ProjectsMatchCurrentFilter => Projects.Any();

        /// <summary>
        /// Gets whether to show the "create your first project" empty state.
        /// True when the database is completely empty.
        /// </summary>
        public bool ShowCreateFirstProjectPrompt => !ProjectsExistInDatabase;

        /// <summary>
        /// Gets whether to show the "no search results" message.
        /// True when projects exist in database but none match current filters.
        /// </summary>
        public bool ShowNoSearchResultsMessage => ProjectsExistInDatabase && !ProjectsMatchCurrentFilter;

        public bool CanReorderProjects => SortBy == ProjectSortBy.SortOrder;

        #endregion

        #region Commands

        /// <summary>
        /// Refreshes the projects list from the database.
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanRefresh))]
        private async Task RefreshProjectsAsync()
        {
            if (IsRefreshing) return;

            IsRefreshing = true;
            IsBusy = true;

            try
            {
                var result = await _dataService.GetAllProjectsAsync();

                if (result.IsSuccess && result.Data != null)
                {
                    _allProjects = result.Data.ToList();
                    ProjectsExistInDatabase = _allProjects.Any();
                    ApplyFilterAndSort();

                    Serilog.Log.Information("Projects refreshed successfully. Total count: {Count}", _allProjects.Count);
                }
                else
                {
                    Serilog.Log.Warning("Failed to refresh projects: {Error}", result.ErrorMessage);
                    await _dialogService.ShowErrorAsync(
                        "Database Error",
                        $"Failed to load projects: {result.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error refreshing projects");
                await _dialogService.ShowErrorAsync(
                    "Unexpected Error",
                    "An unexpected error occurred while refreshing projects.");
            }
            finally
            {
                IsRefreshing = false;
                IsBusy = false;
            }
        }

        /// <summary>
        /// Opens the specified project by navigating to its detail page.
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanOpenProject))]
        private async Task OpenProjectAsync(Project? project)
        {
            if (project == null)
            {
                Serilog.Log.Warning("OpenProject called with null project");
                return;
            }

            try
            {
                IsBusy = true;

                var request = new ProjectPageRequest(project);
                var navResult = await _navigationService.NavigateToAsync(request);

                if (!navResult.Success)
                {
                    Serilog.Log.Warning("Failed to navigate to project page: {Error}", navResult.ErrorMessage);
                    await _dialogService.ShowErrorAsync(
                        "Navigation Error",
                        "Failed to open project.");
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error opening project {ProjectId}", project.Id);
                await _dialogService.ShowErrorAsync(
                    "Unexpected Error",
                    "An unexpected error occurred while opening the project.");
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Creates a new project and navigates to the project form.
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanCreateProject))]
        private async Task CreateProjectAsync()
        {
            try
            {
                IsBusy = true;

                var newProject = new Project
                {
                    Name = string.Empty,
                    CreatedAt = DateTime.UtcNow
                };

                var request = new ProjectFormPageRequest(newProject, FormDialogAction.Create);
                var navResult = await _navigationService.NavigateToAsync(request);

                if (!navResult.Success)
                {
                    Serilog.Log.Warning("Failed to navigate to project form: {Error}", navResult.ErrorMessage);
                    await _dialogService.ShowErrorAsync(
                        "Navigation Error",
                        "Failed to navigate to project creation form.");
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error creating project");
                await _dialogService.ShowErrorAsync(
                    "Unexpected Error",
                    "An unexpected error occurred while creating a project.");
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Edits the specified project by navigating to the project form.
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanEditProject))]
        private async Task EditProjectAsync(Project? project)
        {
            if (project == null)
            {
                Serilog.Log.Warning("EditProject called with null project");
                return;
            }

            try
            {
                IsBusy = true;

                var request = new ProjectFormPageRequest(project, FormDialogAction.Edit);
                var navResult = await _navigationService.NavigateToAsync(request);

                if (!navResult.Success)
                {
                    Serilog.Log.Warning("Failed to navigate to project form for editing: {Error}", navResult.ErrorMessage);
                    await _dialogService.ShowErrorAsync(
                        "Navigation Error",
                        "Failed to navigate to project edit form.");
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error editing project {ProjectId}", project.Id);
                await _dialogService.ShowErrorAsync(
                    "Unexpected Error",
                    "An unexpected error occurred while editing the project.");
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Deletes the specified project after confirmation.
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanDeleteProject))]
        private async Task DeleteProjectAsync(Project? project)
        {
            if (project == null)
            {
                Serilog.Log.Warning("DeleteProject called with null project");
                return;
            }

            var confirmResult = await _dialogService.ShowConfirmAsync(
                "Confirm Delete",
                $"Are you sure you want to delete '{project.Name}'?\n\nThis action cannot be undone.",
                new[] { "Delete", "Cancel" });

            if (confirmResult != 0) // User clicked Cancel or dismissed
            {
                Serilog.Log.Information("Project deletion cancelled by user");
                return;
            }

            try
            {
                IsBusy = true;

                var result = await _dataService.DeleteProjectAsync(project.Id);

                if (result.IsSuccess)
                {
                    await _dialogService.ShowSuccessAsync(
                        "Success",
                        $"Project '{project.Name}' was deleted successfully.");

                    Serilog.Log.Information("Project deleted: {ProjectId} - {Name}", project.Id, project.Name);

                    // Refresh the list
                    await RefreshProjectsAsync();
                }
                else
                {
                    Serilog.Log.Warning("Failed to delete project {ProjectId}: {Error}", project.Id, result.ErrorMessage);
                    await _dialogService.ShowErrorAsync(
                        "Delete Failed",
                        $"Failed to delete project: {result.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error deleting project {ProjectId}", project.Id);
                await _dialogService.ShowErrorAsync(
                    "Unexpected Error",
                    "An unexpected error occurred while deleting the project.");
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Reorders a project by moving it from oldIndex to newIndex and updates the database.
        /// </summary>
        public async Task ReorderProjectAsync(Project project, int oldIndex, int newIndex)
        {
            if (project == null || oldIndex == newIndex || oldIndex < 0 || newIndex < 0)
                return;

            try
            {
                IsBusy = true;

                // Move in ObservableCollection
                _dispatcherService.Run(() =>
                {
                    Projects.RemoveAt(oldIndex);
                    Projects.Insert(newIndex, project);
                });

                // Update SortOrder for affected projects
                await UpdateSortOrdersAfterReorderAsync(oldIndex, newIndex);

                Serilog.Log.Information("Project reordered: {ProjectName} from index {OldIndex} to {NewIndex}",
                    project.Name, oldIndex, newIndex);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error reordering project");
                await _dialogService.ShowErrorAsync(
                    "Reorder Failed",
                    "Failed to reorder the project. Please try again.");

                // Refresh to restore correct order
                await RefreshProjectsAsync();
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Toggles the sort order between ascending and descending.
        /// </summary>
        [RelayCommand]
        private void ToggleSortOrder()
        {
            SortAscending = !SortAscending;
        }

        #endregion

        #region CanExecute

        private bool CanRefresh() => !IsBusy && !IsRefreshing;
        private bool CanCreateProject() => !IsBusy;
        private bool CanOpenProject() => !IsBusy;
        private bool CanEditProject() => !IsBusy;
        private bool CanDeleteProject() => !IsBusy;

        #endregion

        #region Handlers

        /// <summary>
        /// Called when the Data property changes. Triggers initial project load.
        /// </summary>
        protected override void OnDataChanged(object? newData)
        {
            base.OnDataChanged(newData);

            // Auto-refresh projects when navigating to this page
            _ = RefreshProjectsAsync();
        }

        private void OnSortByChanged(ProjectSortBy value)
        {
            OnPropertyChanged(nameof(CanReorderProjects));
        }
        #endregion

        #region Helpers

        /// <summary>
        /// Applies the current filter and sort settings to the projects collection.
        /// </summary>
        private void ApplyFilterAndSort()
        {
            _dispatcherService.Run(() =>
            {
                // Start with all projects
                IEnumerable<Project> filtered = _allProjects;

                // Filter by archived status
                if (!ShowArchived)
                {
                    filtered = filtered.Where(p => !p.IsArchived);
                }

                // Filter by search query
                if (!string.IsNullOrWhiteSpace(SearchQuery))
                {
                    var query = SearchQuery.Trim();
                    var words = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    
                    filtered = filtered.Where(p =>
                    {
                        var searchableText = $"{p.Name} {p.Description}".ToLowerInvariant();
                        
                        // All words must match (AND logic)
                        return words.All(word => 
                            searchableText.Contains(word.ToLowerInvariant())
                        );
                    });
                }

                // Sort
                filtered = SortBy switch
                {
                    ProjectSortBy.Name => SortAscending
                        ? filtered.OrderBy(p => p.Name)
                        : filtered.OrderByDescending(p => p.Name),

                    ProjectSortBy.CreatedDate => SortAscending
                        ? filtered.OrderBy(p => p.CreatedAt)
                        : filtered.OrderByDescending(p => p.CreatedAt),

                    ProjectSortBy.UpdatedDate => SortAscending
                        ? filtered.OrderBy(p => p.UpdatedAt ?? p.CreatedAt)
                        : filtered.OrderByDescending(p => p.UpdatedAt ?? p.CreatedAt),

                    ProjectSortBy.SortOrder => SortAscending
                        ? filtered.OrderBy(p => p.SortOrder).ThenBy(p => p.Name)
                        : filtered.OrderByDescending(p => p.SortOrder).ThenByDescending(p => p.Name),

                    _ => filtered.OrderBy(p => p.Name)
                };

                // Update the observable collection
                Projects.Clear();
                foreach (var project in filtered)
                {
                    Projects.Add(project);
                }

                OnPropertyChanged(nameof(ProjectsMatchCurrentFilter));
                OnPropertyChanged(nameof(ShowCreateFirstProjectPrompt));
                OnPropertyChanged(nameof(ShowNoSearchResultsMessage));
            });
        }

        /// <summary>
        /// Notifies all commands that their CanExecute state may have changed.
        /// </summary>
        private void NotifyCanExecuteChanged()
        {
            _dispatcherService.Run(() =>
            {
                RefreshProjectsCommand.NotifyCanExecuteChanged();
                CreateProjectCommand.NotifyCanExecuteChanged();
                OpenProjectCommand.NotifyCanExecuteChanged();
                EditProjectCommand.NotifyCanExecuteChanged();
                DeleteProjectCommand.NotifyCanExecuteChanged();
            });
        }

        /// <summary>
        /// Updates SortOrder values in the database after a reorder operation.
        /// </summary>
        private async Task UpdateSortOrdersAfterReorderAsync(int oldIndex, int newIndex)
        {
            // Determine range to update
            int startIndex = Math.Min(oldIndex, newIndex);
            int endIndex = Math.Max(oldIndex, newIndex);

            // Update SortOrder for affected projects
            for (int i = startIndex; i <= endIndex && i < Projects.Count; i++)
            {
                Projects[i].SortOrder = i;
                var updateResult = await _dataService.UpdateProjectAsync(Projects[i]);

                if (!updateResult.IsSuccess)
                {
                    Serilog.Log.Warning("Failed to update SortOrder for project {ProjectId}: {Error}",
                        Projects[i].Id, updateResult.ErrorMessage);
                }
            }
        }
        #endregion

        #region IDisposable

        public override void Dispose()
        {
            base.Dispose();

            if (_disposed) return;

            _dispatcherService.Run(() =>
            {
                Projects.Clear();
                _allProjects.Clear();
            });

            _disposed = true;

            Serilog.Log.Debug("ProjectsBrowserViewModel disposed");
        }

        #endregion
    }

    /// <summary>
    /// Defines the available sorting options for projects.
    /// </summary>
    public enum ProjectSortBy
    {
        Name,
        CreatedDate,
        UpdatedDate,
        SortOrder
    }
}