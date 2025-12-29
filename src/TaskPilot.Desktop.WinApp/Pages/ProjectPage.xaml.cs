using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using TaskPilot.Core.Components.Data;
using TaskPilot.Core.Components.Entities;
using TaskPilot.Core.ViewModel;

namespace TaskPilot.Desktop.WinApp.Pages
{
    /// <summary>
    /// Page for displaying and managing tasks within a project.
    /// </summary>
    public sealed partial class ProjectPage : Page
    {
        public ProjectPage()
        {
            InitializeComponent();
        }

        #region Properties
        public ProjectPageViewModel ViewModel => (ProjectPageViewModel)DataContext;
        #endregion

        #region Event Handlers
        
        private async void TasksListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is TaskItem task)
            {
                await ViewModel.OpenTaskCommand.ExecuteAsync(task);
            }
        }

        private void TasksListView_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (e.OriginalSource is FrameworkElement element)
            {
                // Find the parent ListViewItem
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
