using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using TaskPilot.Core.Components.Data;
using TaskPilot.Core.Components.Entities;
using TaskPilot.Core.Services;

namespace TaskPilot.Core.ViewModel
{
    /// <summary>
    /// ViewModel for the task form dialog.
    /// Handles creating and editing tasks with data binding support.
    /// </summary>
    public partial class TaskFormViewModel : ObservablePageViewModelBase
    {
        #region Fields
        private readonly ITaskPilotDataService _taskPilotDataService;
        private readonly IDispatcherService _dispatcherService;
        private readonly IDialogService _dialogService;
        private bool _disposed;
        private TaskFormData _data = new(new TaskItem(), FormDialogAction.Create);
        private Project? _selectedProject;
        private Milestone? _selectedMilestone;
        private TaskType? _selectedTaskType;
        private TaskPriority _selectedPriority = TaskPriority.Normal;
        private DateTimeOffset? _dueDate;
        private TimeSpan? _dueTime;
        #endregion

        #region Constructors
        public TaskFormViewModel(INavigationService navService,
            IDispatcherService dispatcherService,
            IDialogService dialogService,
            ITaskPilotDataService pilotDataService)
            : base(navService)
        {
            _taskPilotDataService = pilotDataService;
            _dispatcherService = dispatcherService;
            _dialogService = dialogService;
            Title = "Create Task";
        }
        #endregion

        #region Properties
        public string TaskTitle
        {
            get => _data.Task.Title;
            set
            {
                if (_data.Task.Title == value) return;
                _data.Task.Title = value;
                OnPropertyChanged();
                NotifyCanExecuteChanged();
            }
        }

        public string? Description
        {
            get => _data.Task.Description;
            set
            {
                _data.Task.Description = value;
                OnPropertyChanged();
            }
        }

        public Project? SelectedProject
        {
            get => _selectedProject;
            set
            {
                if (_selectedProject == value) return;
                var oldProject = _selectedProject;
                _selectedProject = value;
                _data.Task.ProjectId = value?.Id;
                OnPropertyChanged();
                OnProjectChanged(oldProject, value);
                NotifyCanExecuteChanged();
            }
        }

        public Milestone? SelectedMilestone
        {
            get => _selectedMilestone;
            set
            {
                if (_selectedMilestone == value) return;
                _selectedMilestone = value;
                _data.Task.MilestoneId = value?.Id;
                OnPropertyChanged();
                NotifyCanExecuteChanged();
            }
        }

        public TaskType? SelectedTaskType
        {
            get => _selectedTaskType;
            set
            {
                if (_selectedTaskType == value) return;
                _selectedTaskType = value;
                _data.Task.TaskTypeId = value?.Id;
                OnPropertyChanged();
                NotifyCanExecuteChanged();
            }
        }

        public TaskPriority SelectedPriority
        {
            get => _selectedPriority;
            set
            {
                if (_selectedPriority == value) return;
                _selectedPriority = value;
                _data.Task.Priority = value;
                OnPropertyChanged();
            }
        }

        private DateTime? _dueDateTime;

        public DateTime? DueDateTime
        {
            get => _dueDateTime;
            set
            {
                if (_dueDateTime == value) return;
                _dueDateTime = value;
                _data.Task.DueDate = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Project> Projects { get; } = new();
        public ObservableCollection<Milestone> Milestones { get; } = new();
        public ObservableCollection<TaskType> TaskTypes { get; } = new();
        public ObservableCollection<PopupData> Popups { get; } = new();

        public TaskPriority[] Priorities { get; } = Enum.GetValues<TaskPriority>();
        #endregion

        #region Methods

        [RelayCommand(CanExecute = nameof(CanSaveChanges))]
        private async Task SaveChangesAsync()
        {
            try
            {
                var result = _data.Action == FormDialogAction.Create
                    ? await _taskPilotDataService.CreateTaskAsync(_data.Task)
                    : await _taskPilotDataService.UpdateTaskAsync(_data.Task);

                if (result.IsSuccess)
                {
                    _data.IsChangesSaved = true;
                    if (result is Result<int> _creationResult)
                        _data.Task.Id = _creationResult.Data;
                }
                else
                {
                    await _dialogService.ShowErrorAsync("Database Error", "Error at saving task.");
                    return;
                }

                // TODO: Add the navigation to the TaskPage;
                // Navigate back to Projects Browser

                NavigationResult? navResult = null; 
                
                if (SelectedMilestone != null)
                {
                    // TODO: Add the navigation to the Milestone Page;
                    throw new NotImplementedException();
                }
                else if (SelectedProject != null)
                {
                    navResult = await _navigationService.NavigateToAsync(new ProjectPageRequest(SelectedProject));
                }
                if (navResult == null || !navResult.Success)
                {
                    await _dialogService.ShowErrorAsync("Navigation Error", "Error at navigating.");
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error saving task form.");
                await _dialogService.ShowErrorAsync("Unexpected Error", "Error saving task form.");
            }
        }

        [RelayCommand]
        private async Task CancelAsync()
        {
            try
            {
                _data.IsChangesSaved = false;
                NavigationResult? navResult = null;

                if (SelectedMilestone != null)
                {
                    // TODO: Add the navigation to the Milestone Page;
                    throw new NotImplementedException();
                }
                else if (SelectedProject != null)
                {
                    navResult = await _navigationService.NavigateToAsync(new ProjectPageRequest(SelectedProject));
                }
                if (navResult == null || !navResult.Success)
                {
                    await _dialogService.ShowErrorAsync("Navigation Error", "Error at navigating.");
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error cancelling task form.");
                await _dialogService.ShowErrorAsync("Unexpected Error", "Error cancelling task form.");
            }
        }

        [RelayCommand]
        private void ClearProject()
        {
            SelectedProject = null;
        }

        [RelayCommand]
        private void ClearMilestone()
        {
            SelectedMilestone = null;
        }

        [RelayCommand]
        private void ClearTaskType()
        {
            SelectedTaskType = null;
        }

        [RelayCommand]
        private void ClearDueDate()
        {
            DueDateTime = null;
            _data.Task.DueDate = null;
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var projectsResult = await _taskPilotDataService.GetAllProjectsAsync();
                if (projectsResult.IsSuccess)
                {
                    _dispatcherService.Run(() =>
                    {
                        Projects.Clear();
                        foreach (var project in projectsResult.Data)
                            Projects.Add(project);
                    });
                }

                var taskTypesResult = await _taskPilotDataService.GetAllTaskTypesAsync();
                if (taskTypesResult.IsSuccess)
                {
                    _dispatcherService.Run(() =>
                    {
                        TaskTypes.Clear();
                        foreach (var taskType in taskTypesResult.Data)
                            TaskTypes.Add(taskType);
                    });
                }

                await LoadMilestonesAsync();
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error loading task form data.");
                await _dialogService.ShowErrorAsync("Unexpected Error", "Error loading task form data.");
            }
        }

        private async Task LoadMilestonesAsync()
        {
            try
            {
                Result<IEnumerable<Milestone>> milestonesResult;
                if (SelectedProject != null)
                {
                    milestonesResult = await _taskPilotDataService.GetMilestonesByProjectIdAsync(SelectedProject.Id);
                }
                else
                {
                    milestonesResult = await _taskPilotDataService.GetAllMilestonesAsync();
                }

                if (milestonesResult.IsSuccess)
                {
                    _dispatcherService.Run(() =>
                    {
                        Milestones.Clear();
                        foreach (var milestone in milestonesResult.Data)
                            Milestones.Add(milestone);
                    });
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error loading milestones.");
            }
        }

        private void UpdateDueDateFromPickers()
        {
            if (DueDateTime.HasValue)
            {
                _data.Task.DueDate = DueDateTime;
            }
            else
            {
                _data.Task.DueDate = null;
            }
        }

        private void OnProjectChanged(Project? oldProject, Project? newProject)
        {
            // If milestone is selected and it doesn't belong to new project, clear it
            if (SelectedMilestone != null && newProject != null && SelectedMilestone.ProjectId != newProject.Id)
            {
                _dispatcherService.Run(() =>
                {
                    Popups.Clear();
                    Popups.Add(new PopupData(PopupType.Warning, "Milestone Reset",
                        $"Milestone '{SelectedMilestone.Name}' has been cleared because it doesn't belong to the selected project."));
                });
                SelectedMilestone = null;
            }

            // Reload milestones for new project
            _ = LoadMilestonesAsync();
            NotifyCanExecuteChanged();
        }

        private bool ValidateEnteredData()
        {
            var popupsList = new List<PopupData>();
            var result = false;

            if (string.IsNullOrWhiteSpace(TaskTitle))
                popupsList.Add(new PopupData(PopupType.Warning, "Warning", "Task title cannot be null."));
            if (SelectedMilestone != null && SelectedProject == null)
                popupsList.Add(new PopupData(PopupType.Warning, "Warning", "When milestone is provided project must also be set."));
            if (SelectedTaskType == null)
                popupsList.Add(new PopupData(PopupType.Warning, "Warning", "Task type cannot be null."));

            if (popupsList.Count == 0)
            {
                popupsList.Add(new PopupData(PopupType.Success, "Success", "Everything looks good. You can continue."));
                result = true;
            }

            _dispatcherService.Run(() =>
            {
                Popups.Clear();
                foreach (var popup in popupsList)
                    Popups.Add(popup);
            });

            return result;
        }
        #endregion

        #region Handlers
        protected override void OnDataChanged(object? data)
        {
            if (data is not TaskFormData value)
                throw new ArgumentException("Data must be of type TaskFormData", nameof(data));

            _data = value;

            // Load projects and task types
            _ = LoadDataAsync();

            // Set selected values from context
            if (value.ContextProject != null)
                SelectedProject = Projects.First(x => x.Id == value.ContextProject.Id);

            if (value.ContextMilestone != null)
                SelectedMilestone = Milestones.First(x => x.Id == value.ContextMilestone.Id);

            // For edit mode, load existing values
            if (value.Action == FormDialogAction.Edit)
            {
                Title = "Edit Task";
                OnPropertyChanged(nameof(Title));

                // Find and set project
                if (value.Task.ProjectId.HasValue)
                {
                    var project = Projects.FirstOrDefault(p => p.Id == value.Task.ProjectId.Value);
                    if (project != null)
                        SelectedProject = project;
                }

                // Find and set milestone
                if (value.Task.MilestoneId.HasValue)
                {
                    var milestone = Milestones.FirstOrDefault(m => m.Id == value.Task.MilestoneId.Value);
                    if (milestone != null)
                        SelectedMilestone = milestone;
                }

                // Find and set task type
                if (value.Task.TaskTypeId.HasValue)
                {
                    var taskType = TaskTypes.FirstOrDefault(t => t.Id == value.Task.TaskTypeId.Value);
                    if (taskType != null)
                        SelectedTaskType = taskType;
                }

                // Set priority
                SelectedPriority = value.Task.Priority;

                // Set due date and time
                if (value.Task.DueDate.HasValue)
                {
                    DueDateTime = value.Task.DueDate.Value;
                }
            }

            OnPropertyChanged(nameof(TaskTitle));
            OnPropertyChanged(nameof(Description));
        }
        
        #endregion

        #region CanExecute
        private bool CanSaveChanges()
        {
            return ValidateEnteredData();
        }
        #endregion

        #region Overrides
        public override void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            base.Dispose();
        }

        private void NotifyCanExecuteChanged()
        {
            SaveChangesCommand.NotifyCanExecuteChanged();
        }
        #endregion
    }
}
