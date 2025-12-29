using Microsoft.UI.Composition;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Numerics;
using System.Reflection;
using System.Threading.Tasks;
using TaskPilot.Core.Components.Contracts;
using TaskPilot.Core.Components.EventArgs;
using Windows.Foundation;

namespace TaskPilot.Desktop.WinApp.Controls
{
    /// <summary>
    /// A custom ItemsControl that supports smooth drag-and-drop reordering with visual feedback.
    /// Provides immediate gap animation and ghost item visualization during drag operations.
    /// </summary>
    /// <remarks>
    /// Usage:
    /// 1. Set ItemsSource to an ObservableCollection
    /// 2. Set ReorderHandler to a ViewModel implementing IReorderHandler
    /// 3. In your ItemTemplate, mark drag handles with ReorderableItemsControl.IsDragHandle="True"
    /// </remarks>
    public partial class ReorderableItemsControl : ItemsControl
    {
        #region Fields

        private object? _draggedItem;
        private int _originalIndex = -1;
        private int _currentGapIndex = -1;
        private bool _isDragging;
        private ContentPresenter? _draggedContainer;
        private Visual? _draggedVisual;
        private Compositor? _compositor;
        private InputCursor? _originalCursor;

        // Animation configuration
        private const double AnimationDurationMs = 200;
        private static readonly TimeSpan AnimationDuration = TimeSpan.FromMilliseconds(AnimationDurationMs);

        #endregion

        #region Dependency Properties

        /// <summary>
        /// Identifies the ReorderHandler dependency property.
        /// </summary>
        public static readonly DependencyProperty ReorderHandlerProperty =
            DependencyProperty.Register(
                nameof(ReorderHandler),
                typeof(IReorderHandler),
                typeof(ReorderableItemsControl),
                new PropertyMetadata(null, OnReorderHandlerChanged));

        /// <summary>
        /// Identifies the CanReorder dependency property.
        /// </summary>
        public static readonly DependencyProperty CanReorderProperty =
            DependencyProperty.Register(
                nameof(CanReorder),
                typeof(bool),
                typeof(ReorderableItemsControl),
                new PropertyMetadata(true, OnCanReorderChanged));

        /// <summary>
        /// Identifies the IsDragHandle attached property.
        /// </summary>
        public static readonly DependencyProperty IsDragHandleProperty =
            DependencyProperty.RegisterAttached(
                "IsDragHandle",
                typeof(bool),
                typeof(ReorderableItemsControl),
                new PropertyMetadata(false, OnIsDragHandleChanged));

        /// <summary>
        /// Identifies the DragHandleVisibility dependency property.
        /// </summary>
        public static readonly DependencyProperty DragHandleVisibilityProperty =
            DependencyProperty.Register(
                nameof(DragHandleVisibility),
                typeof(Visibility),
                typeof(ReorderableItemsControl),
                new PropertyMetadata(Visibility.Visible));

        #endregion

        #region Events

        /// <summary>
        /// Occurs when a reorder operation is about to complete.
        /// Handlers can cancel the operation by setting Cancel = true.
        /// </summary>
        public event EventHandler<ReorderEventArgs>? ReorderCompleting;

        /// <summary>
        /// Occurs after a reorder operation has completed successfully.
        /// </summary>
        public event EventHandler<ReorderEventArgs>? ReorderCompleted;

        /// <summary>
        /// Occurs during drag when the target position changes.
        /// </summary>
        public event EventHandler<ReorderPreviewEventArgs>? ReorderPreview;

        #endregion

        #region Constructors

        public ReorderableItemsControl()
        {
            DefaultStyleKey = typeof(ReorderableItemsControl);
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the handler that processes reorder operations.
        /// The handler is responsible for persisting changes after reorder completes.
        /// </summary>
        public IReorderHandler? ReorderHandler
        {
            get => (IReorderHandler?)GetValue(ReorderHandlerProperty);
            set => SetValue(ReorderHandlerProperty, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether items can be reordered.
        /// When false, drag handles are hidden and drag operations are disabled.
        /// </summary>
        public bool CanReorder
        {
            get => (bool)GetValue(CanReorderProperty);
            set => SetValue(CanReorderProperty, value);
        }

        /// <summary>
        /// Gets or sets the visibility of drag handles.
        /// Automatically updated based on CanReorder property.
        /// </summary>
        public Visibility DragHandleVisibility
        {
            get => (Visibility)GetValue(DragHandleVisibilityProperty);
            set => SetValue(DragHandleVisibilityProperty, value);
        }

        #endregion

        #region Attached Property Accessors

        /// <summary>
        /// Gets the IsDragHandle attached property value.
        /// </summary>
        public static bool GetIsDragHandle(DependencyObject obj)
            => (bool)obj.GetValue(IsDragHandleProperty);

        /// <summary>
        /// Sets the IsDragHandle attached property value.
        /// Mark an element as a drag handle within an item template.
        /// </summary>
        public static void SetIsDragHandle(DependencyObject obj, bool value)
            => obj.SetValue(IsDragHandleProperty, value);

        #endregion

        #region Lifecycle

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;
            UpdateDragHandleVisibility();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            CancelDragOperation();
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            SetupImplicitAnimations();
        }

        #endregion

        #region Container Handling

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new ContentPresenter();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is ContentPresenter;
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            if (element is ContentPresenter container)
            {
                // Setup implicit animations for smooth reordering
                SetupContainerAnimations(container);

                // Handle opacity during drag
                if (_isDragging && item == _draggedItem)
                {
                    SetContainerOpacity(container, 0);
                }
            }
        }

        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            base.ClearContainerForItemOverride(element, item);

            if (element is ContentPresenter container)
            {
                SetContainerOpacity(container, 1);
            }
        }

        #endregion

        #region Animation Setup

        private void SetupImplicitAnimations()
        {
            if (_compositor == null) return;

            // Apply to ItemsPanel if it's a StackPanel
            if (ItemsPanelRoot is Panel panel)
            {
                var visual = ElementCompositionPreview.GetElementVisual(panel);
                // Panel doesn't need animations, items do
            }
        }

        private void SetupContainerAnimations(ContentPresenter container)
        {
            if (_compositor == null) return;

            var visual = ElementCompositionPreview.GetElementVisual(container);

            // Create smooth offset animation for when items shift
            var offsetAnimation = _compositor.CreateVector3KeyFrameAnimation();
            offsetAnimation.Target = "Offset";
            offsetAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
            offsetAnimation.Duration = AnimationDuration;

            // Create opacity animation
            var opacityAnimation = _compositor.CreateScalarKeyFrameAnimation();
            opacityAnimation.Target = "Opacity";
            opacityAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
            opacityAnimation.Duration = TimeSpan.FromMilliseconds(50); // Fast opacity change

            var implicitAnimations = _compositor.CreateImplicitAnimationCollection();
            implicitAnimations["Offset"] = offsetAnimation;
            implicitAnimations["Opacity"] = opacityAnimation;

            visual.ImplicitAnimations = implicitAnimations;
        }

        private void SetContainerOpacity(ContentPresenter container, float opacity)
        {
            var visual = ElementCompositionPreview.GetElementVisual(container);
            visual.Opacity = opacity;
        }

        #endregion

        #region Drag Handle Setup

        private static void OnIsDragHandleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element)
            {
                if ((bool)e.NewValue)
                {
                    element.PointerPressed += OnDragHandlePointerPressed;
                    element.PointerEntered += OnDragHandlePointerEntered;
                    element.PointerExited += OnDragHandlePointerExited;
                }
                else
                {
                    element.PointerPressed -= OnDragHandlePointerPressed;
                    element.PointerEntered -= OnDragHandlePointerEntered;
                    element.PointerExited -= OnDragHandlePointerExited;
                }
            }
        }

        private static void OnDragHandlePointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (sender is UIElement element)
            {
                var control = FindParentReorderableControl(element);
                if (control?.CanReorder == true)
                {
                    SetSizeAllCursor(element);
                }
            }
        }

        private static void OnDragHandlePointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (sender is UIElement element)
            {
                RestoreDefaultCursor(element);
            }
        }

        private static async void OnDragHandlePointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (sender is not UIElement element) return;

            var control = FindParentReorderableControl(element);
            if (control == null || !control.CanReorder) return;

            var container = FindParentContainer(element);
            if (container == null) return;

            var item = control.ItemFromContainer(container);
            if (item == null) return;

            await control.StartDragOperationAsync(item, container, e);
        }

        #endregion

        #region Drag Operations

        private async Task StartDragOperationAsync(object item, ContentPresenter container, PointerRoutedEventArgs e)
        {
            if (_isDragging) return;

            var list = GetItemsList();
            if (list == null) return;

            _draggedItem = item;
            _originalIndex = list.IndexOf(item);
            _currentGapIndex = _originalIndex;
            _draggedContainer = container;
            _isDragging = true;

            if (_originalIndex < 0)
            {
                CancelDragOperation();
                return;
            }

            // Immediately hide the dragged item to create the gap
            SetContainerOpacity(container, 0);

            // Apply visual offsets to show initial gap
            UpdateVisualGap(_originalIndex);

            // Capture pointer for tracking
            container.PointerMoved += OnDragPointerMoved;
            container.PointerReleased += OnDragPointerReleased;
            container.PointerCaptureLost += OnDragPointerCaptureLost;
            container.CapturePointer(e.Pointer);

            e.Handled = true;
        }

        private void OnDragPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (!_isDragging || _draggedItem == null) return;

            var position = e.GetCurrentPoint(this).Position;
            UpdateGapPosition(position);
            e.Handled = true;
        }

        private void OnDragPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            CompleteDragOperation();
            e.Handled = true;
        }

        private void OnDragPointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            // If capture is lost unexpectedly, cancel and revert
            if (_isDragging)
            {
                CancelDragOperation();
            }
        }

        private void UpdateGapPosition(Point pointerPosition)
        {
            var list = GetItemsList();
            if (list == null || _draggedItem == null) return;

            int targetIndex = CalculateTargetIndex(pointerPosition);

            if (targetIndex != _currentGapIndex && targetIndex >= 0 && targetIndex < list.Count)
            {
                int previousGapIndex = _currentGapIndex;
                _currentGapIndex = targetIndex;

                // Update visual gap (using translation, NOT moving in collection)
                UpdateVisualGap(targetIndex);

                // Raise preview event
                ReorderPreview?.Invoke(this, new ReorderPreviewEventArgs(_draggedItem, _originalIndex, targetIndex));
            }
        }

        /// <summary>
        /// Updates visual translations to show gap at targetIndex without moving items in collection.
        /// Items between originalIndex and targetIndex shift to make room.
        /// </summary>
        private void UpdateVisualGap(int targetIndex)
        {
            var list = GetItemsList();
            if (list == null || _draggedContainer == null) return;

            // Get the height of the dragged item for offset calculation
            double itemHeight = _draggedContainer.ActualHeight;

            for (int i = 0; i < list.Count; i++)
            {
                var container = ContainerFromIndex(i) as ContentPresenter;
                if (container == null) continue;

                var visual = ElementCompositionPreview.GetElementVisual(container);

                if (i == _originalIndex)
                {
                    // The dragged item stays hidden at its original position
                    continue;
                }

                float offsetY = 0;

                if (_originalIndex < targetIndex)
                {
                    // Dragging down: items between original+1 and target shift UP
                    if (i > _originalIndex && i <= targetIndex)
                    {
                        offsetY = (float)(-itemHeight);
                    }
                }
                else if (_originalIndex > targetIndex)
                {
                    // Dragging up: items between target and original-1 shift DOWN
                    if (i >= targetIndex && i < _originalIndex)
                    {
                        offsetY = (float)(itemHeight);
                    }
                }

                // Apply offset animation
                visual.Properties.InsertVector3("Translation", new Vector3(0, offsetY, 0));
            }
        }

        /// <summary>
        /// Resets all visual translations to zero.
        /// </summary>
        private void ResetVisualGap()
        {
            var list = GetItemsList();
            if (list == null) return;

            for (int i = 0; i < list.Count; i++)
            {
                var container = ContainerFromIndex(i) as ContentPresenter;
                if (container == null) continue;

                var visual = ElementCompositionPreview.GetElementVisual(container);
                visual.Properties.InsertVector3("Translation", Vector3.Zero);
            }
        }

        private int CalculateTargetIndex(Point position)
        {
            var list = GetItemsList();
            if (list == null) return -1;

            // We need to calculate based on ORIGINAL positions (before any visual shifts)
            // Get the original positions by checking the containers
            double accumulatedHeight = 0;
            double itemHeight = _draggedContainer?.ActualHeight ?? 50;

            for (int i = 0; i < list.Count; i++)
            {
                var container = ContainerFromIndex(i) as ContentPresenter;
                if (container == null) continue;

                double containerHeight = container.ActualHeight;
                double containerTop = accumulatedHeight;
                double containerBottom = containerTop + containerHeight;
                double midY = containerTop + containerHeight / 2;

                if (position.Y < midY)
                {
                    return i;
                }

                accumulatedHeight += containerHeight;
            }

            // If below all items, return last index
            return list.Count - 1;
        }

        private async void CompleteDragOperation()
        {
            if (!_isDragging) return;

            var item = _draggedItem;
            var originalIndex = _originalIndex;
            var newIndex = _currentGapIndex;

            // Cleanup pointer handlers
            if (_draggedContainer != null)
            {
                _draggedContainer.PointerMoved -= OnDragPointerMoved;
                _draggedContainer.PointerReleased -= OnDragPointerReleased;
                _draggedContainer.PointerCaptureLost -= OnDragPointerCaptureLost;
                _draggedContainer.ReleasePointerCaptures();

                // Restore opacity
                SetContainerOpacity(_draggedContainer, 1);
            }

            _isDragging = false;
            _draggedItem = null;
            _draggedContainer = null;
            _originalIndex = -1;
            _currentGapIndex = -1;

            if (item == null || originalIndex == newIndex) return;

            // Create event args for handlers
            var args = new ReorderEventArgs(item, originalIndex, newIndex);

            // Raise completing event (can be cancelled)
            ReorderCompleting?.Invoke(this, args);
            if (args.Cancel)
            {
                // Revert the move
                var list = GetItemsList();
                if (list != null)
                {
                    MoveItemInList(list, newIndex, originalIndex);
                }
                return;
            }

            // Notify handler for persistence
            if (ReorderHandler != null)
            {
                try
                {
                    await ReorderHandler.OnReorderCompletedAsync(item, originalIndex, newIndex);
                }
                catch (Exception ex)
                {
                    Serilog.Log.Error(ex, "Error in reorder handler");
                }
            }

            // Raise completed event
            ReorderCompleted?.Invoke(this, args);
        }

        private void CancelDragOperation()
        {
            if (!_isDragging) return;

            var list = GetItemsList();
            var item = _draggedItem;
            var originalIndex = _originalIndex;
            var currentIndex = _currentGapIndex;

            // Cleanup
            if (_draggedContainer != null)
            {
                _draggedContainer.PointerMoved -= OnDragPointerMoved;
                _draggedContainer.PointerReleased -= OnDragPointerReleased;
                _draggedContainer.PointerCaptureLost -= OnDragPointerCaptureLost;
                _draggedContainer.ReleasePointerCaptures();
                SetContainerOpacity(_draggedContainer, 1);
            }

            _isDragging = false;
            _draggedItem = null;
            _draggedContainer = null;
            _originalIndex = -1;
            _currentGapIndex = -1;

            // Revert to original position
            if (list != null && item != null && currentIndex != originalIndex && originalIndex >= 0)
            {
                MoveItemInList(list, currentIndex, originalIndex);
            }
        }

        #endregion

        #region Property Changed Handlers

        private static void OnReorderHandlerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ReorderableItemsControl control)
            {
                control.UpdateCanReorderFromHandler();
            }
        }

        private static void OnCanReorderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ReorderableItemsControl control)
            {
                control.UpdateDragHandleVisibility();
            }
        }

        private void UpdateCanReorderFromHandler()
        {
            if (ReorderHandler != null)
            {
                CanReorder = ReorderHandler.CanReorder;
            }
        }

        private void UpdateDragHandleVisibility()
        {
            DragHandleVisibility = CanReorder ? Visibility.Visible : Visibility.Collapsed;
        }

        #endregion

        #region Helpers

        private IList? GetItemsList()
        {
            return ItemsSource as IList;
        }

        private void MoveItemInList(IList list, int oldIndex, int newIndex)
        {
            if (oldIndex < 0 || oldIndex >= list.Count) return;
            if (newIndex < 0 || newIndex >= list.Count) return;
            if (oldIndex == newIndex) return;

            // Try to use Move method if available (ObservableCollection)
            var moveMethod = list.GetType().GetMethod("Move", new[] { typeof(int), typeof(int) });
            if (moveMethod != null)
            {
                moveMethod.Invoke(list, new object[] { oldIndex, newIndex });
            }
            else
            {
                // Fallback: Remove and insert
                var item = list[oldIndex];
                list.RemoveAt(oldIndex);
                list.Insert(newIndex, item);
            }
        }

        private static ReorderableItemsControl? FindParentReorderableControl(DependencyObject element)
        {
            var parent = VisualTreeHelper.GetParent(element);
            while (parent != null)
            {
                if (parent is ReorderableItemsControl control)
                    return control;
                parent = VisualTreeHelper.GetParent(parent);
            }
            return null;
        }

        private static ContentPresenter? FindParentContainer(DependencyObject element)
        {
            var parent = VisualTreeHelper.GetParent(element);
            while (parent != null)
            {
                if (parent is ContentPresenter presenter)
                {
                    // Verify this is a direct container of the ItemsControl
                    var itemsControl = FindParentReorderableControl(presenter);
                    if (itemsControl != null && itemsControl.ContainerFromItem(itemsControl.ItemFromContainer(presenter)) == presenter)
                    {
                        return presenter;
                    }
                }
                parent = VisualTreeHelper.GetParent(parent);
            }
            return null;
        }

        private static void SetSizeAllCursor(UIElement element)
        {
            try
            {
                var cursor = InputSystemCursor.Create(InputSystemCursorShape.SizeAll);
                var property = typeof(UIElement).GetProperty("ProtectedCursor",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                property?.SetValue(element, cursor);
            }
            catch
            {
                // Cursor change is non-critical
            }
        }

        private static void RestoreDefaultCursor(UIElement element)
        {
            try
            {
                var cursor = InputSystemCursor.Create(InputSystemCursorShape.Arrow);
                var property = typeof(UIElement).GetProperty("ProtectedCursor",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                property?.SetValue(element, cursor);
            }
            catch
            {
                // Cursor change is non-critical
            }
        }

        #endregion
    }
}