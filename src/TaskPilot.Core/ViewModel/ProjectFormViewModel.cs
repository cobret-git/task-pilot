using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using TaskPilot.Core.Components.Data;
using TaskPilot.Core.Components.Entities;
using TaskPilot.Core.Services;

namespace TaskPilot.Core.ViewModel
{
    /// <summary>
    /// ViewModel for the project form dialog.
    /// Handles creating and editing projects with data binding support.
    /// </summary>
    public partial class ProjectFormViewModel : ObservablePageViewModelBase
    {
        #region Fields
        private readonly ITaskPilotDataService _taskPilotDataService;
        private readonly IDispatcherService _dispatcherService;
        private readonly IDialogService _dialogService;
        private CancellationTokenSource? _nameValidationCts;
        private bool _isCheckingProjectName;
        private bool _isProjectNameTaken;
        private bool _disposed;
        private ProjectFormData _data = new(new Project(), FormDialogAction.Create);
        private ColorData _color;
        #endregion

        #region Constructors
        public ProjectFormViewModel(INavigationService navService, 
            IDispatcherService dispatcherService, 
            IDialogService dialogService,
            ITaskPilotDataService pilotDataService)
            : base(navService)
        {
            _taskPilotDataService = pilotDataService;
            _dispatcherService = dispatcherService;
            _dialogService = dialogService;
            Colors = GetColors().ToArray();
            Title = "Create Project";
            _color = Colors.First(x => x.Name == "Mint Fresh");
        }
        #endregion

        #region Properties
        public string ProjectName
        {
            get => _data.Project.Name;
            set
            {
                if (_data.Project.Name == value) return;
                _data.Project.Name = value;
                OnPropertyChanged();
                ValidateProjectNameAsync(value);
            }
        }
        public string? Description { get => _data.Project.Description; set { _data.Project.Description = value; OnPropertyChanged(); } }
        public ColorData Color { get => _color; set { _color = value; _data.Project.Color = value.HexValue; OnPropertyChanged(); } }
        public ColorData[] Colors { get; }
        public bool IsCheckingProjectName
        {
            get => _isCheckingProjectName;
            private set { if (_isCheckingProjectName == value) return; _isCheckingProjectName = value; OnPropertyChanged(); }
        }
        public bool IsProjectNameTaken
        {
            get => _isProjectNameTaken;
            private set { if (_isProjectNameTaken == value) return; _isProjectNameTaken = value; OnPropertyChanged(); }
        }
        public ObservableCollection<PopupData> Popups { get; } = new();
        #endregion

        #region Methods
        [RelayCommand(CanExecute = nameof(CanSaveChanges))] private async Task SaveChangesAsync()
        {
            try
            {
                var result = _data.Action == FormDialogAction.Create
                    ? await _taskPilotDataService.CreateProjectAsync(_data.Project)
                    : await _taskPilotDataService.UpdateProjectAsync(_data.Project);

                if (result.IsSuccess)
                {
                    _data.IsChangesSaved = true;
                    _data.Project.Id = result is Result<int> _creationResult ? _creationResult.Data : default;
                }
                else
                {
                    await _dialogService.ShowErrorAsync("Database Error", "Error at saving project.");
                    return;
                }
                var navResult = await _navigationService.NavigateToAsync(new ProjectPageRequest(_data.Project));
                if (!navResult.Success)
                {
                    await _dialogService.ShowErrorAsync("Navigation Error", "Error at navigating.");
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error saving project form.");
                await _dialogService.ShowErrorAsync("Unexpected Error", "Error saving project form.");
            }
        }
        [RelayCommand()] private async Task CancelAsync()
        {
            try
            {
                _data.IsChangesSaved = false;
                var navResult = await _navigationService.NavigateToAsync(new ProjectsBrowserPageRequest());
                if (!navResult.Success)
                    await _dialogService.ShowErrorAsync("Navigation Error", "Error at navigating.");
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error cancelling project form.");
                await _dialogService.ShowErrorAsync("Unexpected Error", "Error cancelling project form.");
            }
        }
        private async void ValidateProjectNameAsync(string projectName)
        {
            _nameValidationCts?.Cancel();
            _nameValidationCts?.Dispose();
            _nameValidationCts = new CancellationTokenSource();

            var token = _nameValidationCts.Token;

            if (string.IsNullOrWhiteSpace(projectName))
            {
                IsProjectNameTaken = false;
                IsCheckingProjectName = false;
                _dispatcherService.Run(() =>
                {
                    Popups.Clear();
                    Popups.Add(new PopupData(PopupType.Warning, "Warning", "Project name cannot be null."));
                });
                return;
            }

            IsCheckingProjectName = true;
            NotifyCanExecuteChanged();

            try
            {
                await Task.Delay(300, token);

                var excludeId = _data.Action == FormDialogAction.Edit ? _data.Project.Id : (int?)null;
                var result = await _taskPilotDataService.ProjectNameExistsAsync(projectName, excludeId);

                if (token.IsCancellationRequested) return;

                IsProjectNameTaken = result.IsSuccess && result.Data == true;
                _dispatcherService.Run(() =>
                {
                    Popups.Clear();
                    if (IsProjectNameTaken)
                        Popups.Add(new PopupData(PopupType.Warning, "Warning", "Project name is already taken."));
                    else
                        Popups.Add(new PopupData(PopupType.Success, "Success", "Everything looks good. You can continue."));
                });
            }
            catch (OperationCanceledException)
            {
                Serilog.Log.Verbose("Project name validation cancelled.");
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error validating project name.");
                _dispatcherService.Run(() =>
                {
                    Popups.Clear();
                    Popups.Add(new PopupData(PopupType.Error, "Unexpected error", "Error validating project name."));
                });
                IsProjectNameTaken = false;
            }
            finally
            {
                if (!token.IsCancellationRequested)
                    IsCheckingProjectName = false;
                NotifyCanExecuteChanged();
            }
        }
        #endregion

        #region Handlers
        protected override void OnDataChanged(object? data)
        {
            if (data is not ProjectFormData value)
                throw new ArgumentException("Data must be of type ProjectFormData", nameof(data));

            _color = Colors.FirstOrDefault(x => x.HexValue == value.Project.Color) ?? _color;
            OnPropertyChanged(nameof(Color));

            if (value.Action == FormDialogAction.Edit)
            {
                Title = "Edit Project";
                OnPropertyChanged(nameof(Title));
            }

            ValidateProjectNameAsync(ProjectName);
        }
        #endregion

        #region CanExecute
        private bool CanSaveChanges() => !string.IsNullOrWhiteSpace(ProjectName) && !IsProjectNameTaken && !IsCheckingProjectName;
        #endregion

        #region Helpers
        private IEnumerable<ColorData> GetColors()
        {
            yield return new ColorData("Royal Purple", "7C5CFF");
            yield return new ColorData("Deep Violet", "6D3DF2");
            yield return new ColorData("Lavender Glow", "A78BFA");
            yield return new ColorData("Magenta Rose", "E056FD");

            yield return new ColorData("Indigo Blue", "4F46E5");
            yield return new ColorData("Electric Blue", "3B82F6");
            yield return new ColorData("Sky Cyan", "22D3EE");
            yield return new ColorData("Teal Aqua", "14B8A6");

            yield return new ColorData("Emerald Green", "10B981");
            yield return new ColorData("Lime Green", "84CC16");
            yield return new ColorData("Mint Fresh", "2DD4BF");

            yield return new ColorData("Golden Amber", "F59E0B");
            yield return new ColorData("Sun Yellow", "FACC15");
            yield return new ColorData("Soft Orange", "FB923C");

            yield return new ColorData("Coral Red", "F87171");
            yield return new ColorData("Crimson", "EF4444");

            yield return new ColorData("Slate Gray", "64748B");
            yield return new ColorData("Cool Silver", "94A3B8");

        }
        private void NotifyCanExecuteChanged()
        {
            _dispatcherService.Run(() =>
            {
                SaveChangesCommand.NotifyCanExecuteChanged();
            });
        }
        #endregion

        #region IDisposable
        public override void Dispose()
        {
            base.Dispose();

            if (_disposed) return;

            _nameValidationCts?.Cancel();
            _nameValidationCts?.Dispose();
            _nameValidationCts = null;

            _disposed = true;
        }
        #endregion
    }
}
