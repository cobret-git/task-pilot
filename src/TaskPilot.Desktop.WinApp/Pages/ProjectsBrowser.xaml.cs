using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System.Reflection;
using TaskPilot.Core.Components.Entities;
using TaskPilot.Core.ViewModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TaskPilot.Desktop.WinApp.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ProjectsBrowser : Page
    {
        #region Fields
        private InputCursor? _originalCursor;
        #endregion

        public ProjectsBrowser()
        {
            InitializeComponent();
        }

        #region Properties
        public ProjectsBrowserViewModel ViewModel => (ProjectsBrowserViewModel)DataContext;
        #endregion

        #region Handlers
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

        private async void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is Project project)
            {
                await ViewModel.OpenProjectCommand.ExecuteAsync(project);
            }
        }

        private void ListView_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            // Get the data context of the right-tapped element
            if (e.OriginalSource is FrameworkElement element)
            {
                // Walk up the visual tree to find the ListViewItem
                var item = element;
                while (item != null && item is not ListViewItem)
                {
                    item = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetParent(item) as FrameworkElement;
                }

                // If we found a ListViewItem, get its DataContext (the Project)
                if (item is ListViewItem listViewItem && listViewItem.Content is Project project)
                {
                    ViewModel.SelectedItem = project;
                }
            }
        }

        private void ProjectItem_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Grid grid)
            {
                // Find the named elements
                var defaultIcon = FindChildByName<FontIcon>(grid, "DefaultIcon");
                var hoverIcon = FindChildByName<FontIcon>(grid, "HoverIcon");
                var menuButton = FindChildByName<Button>(grid, "MenuButton");

                if (defaultIcon != null) defaultIcon.Opacity = 0;
                if (hoverIcon != null) hoverIcon.Opacity = 1;
                if (menuButton != null) menuButton.Opacity = 1;
            }
        }

        private void ProjectItem_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Grid grid)
            {
                var defaultIcon = FindChildByName<FontIcon>(grid, "DefaultIcon");
                var hoverIcon = FindChildByName<FontIcon>(grid, "HoverIcon");
                var menuButton = FindChildByName<Button>(grid, "MenuButton");

                if (defaultIcon != null) defaultIcon.Opacity = 1;
                if (hoverIcon != null) hoverIcon.Opacity = 0;
                if (menuButton != null) menuButton.Opacity = 0;
            }
        }

        private void DragArea_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (sender is UIElement element && ViewModel.CanReorderProjects)
            {
                var cursor = InputSystemCursor.Create(InputSystemCursorShape.SizeAll);
                PropertyInfo property = typeof(UIElement).GetProperty("ProtectedCursor",
                    BindingFlags.Instance | BindingFlags.NonPublic)!;
                _originalCursor = (InputCursor)property.GetValue(element)!;

                typeof(UIElement).InvokeMember("ProtectedCursor",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetProperty,
                    null, element, new object[] { cursor });
            }
        }

        private void DragArea_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (sender is FrameworkElement element)
            {
                var cursor = InputSystemCursor.Create(InputSystemCursorShape.Arrow);
                typeof(UIElement).InvokeMember("ProtectedCursor",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetProperty,
                    null, element, new object[] { cursor });
            }
        }
        #endregion

        #region Helpers
        private T? FindChildByName<T>(DependencyObject parent, string name) where T : FrameworkElement
        {
            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T element && element.Name == name)
                    return element;

                var result = FindChildByName<T>(child, name);
                if (result != null)
                    return result;
            }
            return null;
        }
        #endregion
    }
}
