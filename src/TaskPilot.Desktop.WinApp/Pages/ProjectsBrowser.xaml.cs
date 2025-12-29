using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using TaskPilot.Core.Components.Entities;
using TaskPilot.Core.Components.EventArgs;
using TaskPilot.Core.ViewModel;

namespace TaskPilot.Desktop.WinApp.Pages
{
    /// <summary>
    /// Page for browsing and managing projects.
    /// Uses ReorderableItemsControl for drag-and-drop reordering.
    /// </summary>
    public sealed partial class ProjectsBrowser : Page
    {
        #region Constructors

        public ProjectsBrowser()
        {
            InitializeComponent();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the ViewModel for this page.
        /// </summary>
        public ProjectsBrowserViewModel ViewModel => (ProjectsBrowserViewModel)DataContext;

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the ReorderCompleted event from the ReorderableItemsControl.
        /// </summary>
        private void ProjectsListControl_ReorderCompleted(object sender, ReorderEventArgs e)
        {
            Serilog.Log.Debug("Reorder completed: {Item} moved from {Old} to {New}",
                (e.Item as Project)?.Name, e.OldIndex, e.NewIndex);
        }

        /// <summary>
        /// Handles pointer entered on project items for hover effects.
        /// </summary>
        private void ProjectItem_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Grid grid)
            {
                var defaultIcon = FindChildByName<FontIcon>(grid, "DefaultIcon");
                var hoverIcon = FindChildByName<FontIcon>(grid, "HoverIcon");
                if (defaultIcon != null) defaultIcon.Opacity = 0;
                if (hoverIcon != null) hoverIcon.Opacity = 1;
            }
        }

        /// <summary>
        /// Handles pointer exited on project items for hover effects.
        /// </summary>
        private void ProjectItem_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Grid grid)
            {
                var defaultIcon = FindChildByName<FontIcon>(grid, "DefaultIcon");
                var hoverIcon = FindChildByName<FontIcon>(grid, "HoverIcon");
                if (defaultIcon != null) defaultIcon.Opacity = 1;
                if (hoverIcon != null) hoverIcon.Opacity = 0;
            }
        }

        /// <summary>
        /// Handles the Open context menu click.
        /// </summary>
        private async void OpenProject_Click(object sender, RoutedEventArgs e)
        {
            if (GetProjectFromMenuFlyoutItem(sender) is Project project)
            {
                await ViewModel.OpenProjectCommand.ExecuteAsync(project);
            }
        }

        /// <summary>
        /// Handles the Edit context menu click.
        /// </summary>
        private async void EditProject_Click(object sender, RoutedEventArgs e)
        {
            if (GetProjectFromMenuFlyoutItem(sender) is Project project)
            {
                await ViewModel.EditProjectCommand.ExecuteAsync(project);
            }
        }

        /// <summary>
        /// Handles the Delete context menu click.
        /// </summary>
        private async void DeleteProject_Click(object sender, RoutedEventArgs e)
        {
            if (GetProjectFromMenuFlyoutItem(sender) is Project project)
            {
                await ViewModel.DeleteProjectCommand.ExecuteAsync(project);
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Finds a child element by name within a parent element.
        /// </summary>
        private T? FindChildByName<T>(DependencyObject parent, string name) where T : FrameworkElement
        {
            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T element && (string.IsNullOrEmpty(name) || element.Name == name))
                    return element;

                var result = FindChildByName<T>(child, name);
                if (result != null)
                    return result;
            }
            return null;
        }

        /// <summary>
        /// Gets the Project from a MenuFlyoutItem's DataContext.
        /// </summary>
        private Project? GetProjectFromMenuFlyoutItem(object sender)
        {
            if (sender is MenuFlyoutItem menuItem)
            {
                // Navigate up to find the Grid with the Project DataContext
                var parent = menuItem.Parent;
                while (parent != null)
                {
                    if (parent is MenuFlyout flyout && flyout.Target is FrameworkElement target)
                    {
                        return target.DataContext as Project;
                    }
                    parent = (parent as FrameworkElement)?.Parent;
                }
            }
            return null;
        }

        #endregion
    }
}