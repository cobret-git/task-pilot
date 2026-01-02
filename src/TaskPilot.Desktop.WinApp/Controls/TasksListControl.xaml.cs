using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using TaskPilot.Core.Components.Entities;
using TaskPilot.Core.ViewModel;

namespace TaskPilot.Desktop.WinApp.Controls
{
    /// <summary>
    /// Reusable control for displaying a filterable and sortable list of tasks.
    /// Can be used across different pages (ProjectPage, MilestonePage, TasksPage).
    /// </summary>
    public sealed partial class TasksListControl : UserControl
    {
        #region Constructors
        
        public TasksListControl()
        {
            InitializeComponent();
        }
        
        #endregion

        #region Properties
        
        public TasksListControlViewModel? ViewModel => DataContext as TasksListControlViewModel;
        
        #endregion

        #region Dependency Properties
        
        /// <summary>
        /// Identifies the <see cref="SearchText"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SearchTextProperty =
            DependencyProperty.Register(
                nameof(SearchText),
                typeof(string),
                typeof(TasksListControl),
                new PropertyMetadata(string.Empty, OnSearchTextChanged));
        
        /// <summary>
        /// Identifies the <see cref="SelectedProject"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedProjectProperty =
            DependencyProperty.Register(
                nameof(SelectedProject),
                typeof(Project),
                typeof(TasksListControl),
                new PropertyMetadata(null, OnSelectedProjectChanged));
        
        /// <summary>
        /// Identifies the <see cref="SelectedMilestone"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedMilestoneProperty =
            DependencyProperty.Register(
                nameof(SelectedMilestone),
                typeof(Milestone),
                typeof(TasksListControl),
                new PropertyMetadata(null, OnSelectedMilestoneChanged));
        
        /// <summary>
        /// Identifies the <see cref="ShowProject"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ShowProjectProperty =
            DependencyProperty.Register(
                nameof(ShowProject),
                typeof(bool),
                typeof(TasksListControl),
                new PropertyMetadata(true, OnShowProjectChanged));
        
        /// <summary>
        /// Identifies the <see cref="ShowMilestone"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ShowMilestoneProperty =
            DependencyProperty.Register(
                nameof(ShowMilestone),
                typeof(bool),
                typeof(TasksListControl),
                new PropertyMetadata(true, OnShowMilestoneChanged));
        
        /// <summary>
        /// Gets or sets the search text for filtering tasks.
        /// </summary>
        public string SearchText
        {
            get => (string)GetValue(SearchTextProperty);
            set => SetValue(SearchTextProperty, value);
        }
        
        /// <summary>
        /// Gets or sets the selected project for filtering tasks.
        /// When set, only tasks belonging to this project are displayed.
        /// </summary>
        public Project? SelectedProject
        {
            get => (Project?)GetValue(SelectedProjectProperty);
            set => SetValue(SelectedProjectProperty, value);
        }
        
        /// <summary>
        /// Gets or sets the selected milestone for filtering tasks.
        /// When set, only tasks belonging to this milestone are displayed.
        /// </summary>
        public Milestone? SelectedMilestone
        {
            get => (Milestone?)GetValue(SelectedMilestoneProperty);
            set => SetValue(SelectedMilestoneProperty, value);
        }
        
        /// <summary>
        /// Gets or sets whether to show project information in task items.
        /// Set to false when displaying tasks within a specific project context.
        /// </summary>
        public bool ShowProject
        {
            get => (bool)GetValue(ShowProjectProperty);
            set => SetValue(ShowProjectProperty, value);
        }
        
        /// <summary>
        /// Gets or sets whether to show milestone information in task items.
        /// Set to false when displaying tasks within a specific milestone context.
        /// </summary>
        public bool ShowMilestone
        {
            get => (bool)GetValue(ShowMilestoneProperty);
            set => SetValue(ShowMilestoneProperty, value);
        }
        
        #endregion

        #region Dependency Property Changed Handlers
        
        private static void OnSearchTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TasksListControl control && control.ViewModel != null)
            {
                control.ViewModel.SearchText = (string)e.NewValue;
            }
        }
        
        private static void OnSelectedProjectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TasksListControl control && control.ViewModel != null)
            {
                control.ViewModel.SelectedProject = (Project?)e.NewValue;
            }
        }
        
        private static void OnSelectedMilestoneChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TasksListControl control && control.ViewModel != null)
            {
                control.ViewModel.SelectedMilestone = (Milestone?)e.NewValue;
            }
        }
        
        private static void OnShowProjectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TasksListControl control && control.ViewModel != null)
            {
                control.ViewModel.ShowProject = (bool)e.NewValue;
            }
        }
        
        private static void OnShowMilestoneChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TasksListControl control && control.ViewModel != null)
            {
                control.ViewModel.ShowMilestone = (bool)e.NewValue;
            }
        }
        
        #endregion

        #region Event Handlers
        
        private async void TasksListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is TaskItem task && ViewModel != null)
            {
                await ViewModel.OpenTaskCommand.ExecuteAsync(task);
            }
        }
        
        private void TasksListView_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (e.OriginalSource is FrameworkElement element && ViewModel != null)
            {
                var item = GetParentListViewItem(element);
                if (item?.Content is TaskItem task)
                {
                    ViewModel.SelectedTask = task;
                }
            }
        }
        
        #endregion

        #region Helpers
        
        private ListViewItem? GetParentListViewItem(DependencyObject child)
        {
            var parent = VisualTreeHelper.GetParent(child);
            while (parent != null)
            {
                if (parent is ListViewItem item) return item;
                parent = VisualTreeHelper.GetParent(parent);
            }
            return null;
        }
        
        #endregion
    }
}
