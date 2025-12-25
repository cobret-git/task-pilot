using Microsoft.UI.Composition;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Threading.Tasks;
using TaskPilot.Core.Components.Entities;
using TaskPilot.Core.ViewModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;

namespace TaskPilot.Desktop.WinApp.Pages
{
    public sealed partial class ProjectsBrowser : Page
    {
        #region Fields
        private InputCursor? _originalCursor;
        private Project? _draggedProject = null;
        private int _originalIndex = -1;
        private bool _isReordering = false;
        private const string DRAG_DATA_KEY = "ProjectReorder";

        // Timer-based throttle for reordering to keep the UI thread breathing
        private DispatcherTimer? _reorderThrottle;
        private Point _lastDragPosition;
        #endregion

        public ProjectsBrowser()
        {
            InitializeComponent();
            ProjectsListView.ContainerContentChanging += ProjectsListView_ContainerContentChanging;

            _reorderThrottle = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) }; // ~60fps
            _reorderThrottle.Tick += (s, e) => ProcessReorder();
        }

        #region Properties
        public ProjectsBrowserViewModel ViewModel => (ProjectsBrowserViewModel)DataContext;
        #endregion

        #region Visual State Management

        private void ProjectsListView_ContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            // Use lighter-weight implicit animations
            EnableImplicitReorderAnimations(args.ItemContainer);

            if (_draggedProject != null && args.Item == _draggedProject)
            {
                // Force collapse the visibility immediately to ensure the gap is handled by the layout engine
                args.ItemContainer.Opacity = 0;
            }
            else
            {
                args.ItemContainer.Opacity = 1;
            }
        }

        private void EnableImplicitReorderAnimations(SelectorItem container)
        {
            var visual = ElementCompositionPreview.GetElementVisual(container);
            var compositor = visual.Compositor;

            var animationGroup = compositor.CreateAnimationGroup();

            var offsetAnimation = compositor.CreateVector3KeyFrameAnimation();
            offsetAnimation.Target = "Offset";
            offsetAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
            // Fast duration for snappiness, ease-out for a premium feel
            offsetAnimation.Duration = TimeSpan.FromMilliseconds(200);

            animationGroup.Add(offsetAnimation);

            var implicitAnimations = compositor.CreateImplicitAnimationCollection();
            implicitAnimations["Offset"] = animationGroup;

            visual.ImplicitAnimations = implicitAnimations;
        }

        #endregion

        #region Drag Handle Events

        private async void DragHandle_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (sender is FrameworkElement handle && ViewModel.CanReorderProjects)
            {
                if (handle.DataContext is Project project)
                {
                    var container = GetParentListViewItem(handle);
                    var dragVisual = container as UIElement ?? handle;

                    _draggedProject = project;
                    _originalIndex = ViewModel.Projects.IndexOf(project);

                    // Immediate visual feedback
                    if (container != null) container.Opacity = 0;

                    var dataPackage = new DataPackage();
                    dataPackage.RequestedOperation = DataPackageOperation.Move;
                    dataPackage.Properties.Add(DRAG_DATA_KEY, project.Id);

                    // Start throttle
                    _reorderThrottle?.Start();

                    var result = await dragVisual.StartDragAsync(e.GetCurrentPoint(dragVisual));

                    // Stop throttle
                    _reorderThrottle?.Stop();

                    if (result != DataPackageOperation.Move)
                    {
                        int currentIndex = ViewModel.Projects.IndexOf(project);
                        if (currentIndex != _originalIndex && _originalIndex >= 0)
                        {
                            ViewModel.Projects.Move(currentIndex, _originalIndex);
                        }
                    }

                    _draggedProject = null;
                    _originalIndex = -1;

                    // Reset all visuals
                    foreach (var item in ViewModel.Projects)
                    {
                        if (ProjectsListView.ContainerFromItem(item) is ListViewItem c) c.Opacity = 1;
                    }
                }
            }
        }

        private void DragHandle_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (sender is UIElement element && ViewModel.CanReorderProjects)
            {
                var cursor = InputSystemCursor.Create(InputSystemCursorShape.SizeAll);
                PropertyInfo property = typeof(UIElement).GetProperty("ProtectedCursor",
                    BindingFlags.Instance | BindingFlags.NonPublic)!;

                var current = (InputCursor)property.GetValue(element)!;
                if (current != null) _originalCursor = current;

                typeof(UIElement).InvokeMember("ProtectedCursor",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetProperty,
                    null, element, new object[] { cursor });
            }
        }

        private void DragHandle_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (sender is UIElement element && _originalCursor != null)
            {
                typeof(UIElement).InvokeMember("ProtectedCursor",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetProperty,
                    null, element, new object[] { _originalCursor });
            }
        }

        #endregion

        #region Reordering Logic

        private void ProjectsListView_DragOver(object sender, DragEventArgs e)
        {
            if (!ViewModel.CanReorderProjects || _draggedProject == null)
            {
                e.AcceptedOperation = DataPackageOperation.None;
                return;
            }

            e.AcceptedOperation = DataPackageOperation.Move;
            e.DragUIOverride.IsCaptionVisible = false;
            e.DragUIOverride.IsGlyphVisible = false;

            // Track last position for the throttle timer
            _lastDragPosition = e.GetPosition(ProjectsListView);
            e.Handled = true;
        }

        private void ProcessReorder()
        {
            if (_isReordering || _draggedProject == null) return;

            var projectUnderMouse = GetProjectAtPositionRobust(ProjectsListView, _lastDragPosition);

            if (projectUnderMouse != null && projectUnderMouse != _draggedProject)
            {
                _isReordering = true;

                int oldIndex = ViewModel.Projects.IndexOf(_draggedProject);
                int newIndex = ViewModel.Projects.IndexOf(projectUnderMouse);

                if (oldIndex != -1 && newIndex != -1 && oldIndex != newIndex)
                {
                    ViewModel.Projects.Move(oldIndex, newIndex);
                }

                _isReordering = false;
            }
        }

        private async void ProjectsListView_Drop(object sender, DragEventArgs e)
        {
            if (!ViewModel.CanReorderProjects || _draggedProject == null)
                return;

            try
            {
                int newIndex = ViewModel.Projects.IndexOf(_draggedProject);
                //await ViewModel.SaveProjectOrderAsync(_draggedProject, newIndex);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error finalizing drop");
                await ViewModel.RefreshProjectsCommand.ExecuteAsync(null);
            }
        }

        #endregion

        #region Helpers

        private Project? GetProjectAtPositionRobust(ListView listView, Point position)
        {
            // Scan realized items
            for (int i = 0; i < listView.Items.Count; i++)
            {
                if (listView.ContainerFromIndex(i) is ListViewItem container)
                {
                    var transform = container.TransformToVisual(listView);
                    var topLeft = transform.TransformPoint(new Point(0, 0));
                    var bounds = new Rect(topLeft, new Size(container.ActualWidth, container.ActualHeight));

                    // Check if point is inside bounds
                    if (bounds.Contains(position))
                    {
                        return container.Content as Project;
                    }
                }
            }
            return null;
        }

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

        private async void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is Project project)
            {
                await ViewModel.OpenProjectCommand.ExecuteAsync(project);
            }
        }

        private void ListView_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (e.OriginalSource is FrameworkElement element)
            {
                var item = GetParentListViewItem(element);
                if (item?.Content is Project project)
                {
                    ViewModel.SelectedItem = project;
                }
            }
        }

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
        #endregion
    }
}