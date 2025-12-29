namespace TaskPilot.Core.Components.EventArgs
{
    /// <summary>
    /// Provides data for reorder-related events.
    /// </summary>
    public class ReorderEventArgs : System.EventArgs
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ReorderEventArgs"/> class.
        /// </summary>
        /// <param name="item">The item being reordered.</param>
        /// <param name="oldIndex">The original index of the item.</param>
        /// <param name="newIndex">The new index of the item.</param>
        public ReorderEventArgs(object item, int oldIndex, int newIndex)
        {
            Item = item;
            OldIndex = oldIndex;
            NewIndex = newIndex;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the item being reordered.
        /// </summary>
        public object Item { get; }

        /// <summary>
        /// Gets the original index of the item before the reorder.
        /// </summary>
        public int OldIndex { get; }

        /// <summary>
        /// Gets the new index of the item after the reorder.
        /// </summary>
        public int NewIndex { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the reorder operation should be cancelled.
        /// Set to true in event handlers to prevent the reorder from completing.
        /// </summary>
        public bool Cancel { get; set; }

        #endregion
    }

    /// <summary>
    /// Provides data for reorder preview events that occur during drag operations.
    /// </summary>
    public class ReorderPreviewEventArgs : System.EventArgs
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ReorderPreviewEventArgs"/> class.
        /// </summary>
        /// <param name="item">The item being dragged.</param>
        /// <param name="currentIndex">The current index of the item.</param>
        /// <param name="targetIndex">The target index where the item would be dropped.</param>
        public ReorderPreviewEventArgs(object item, int currentIndex, int targetIndex)
        {
            Item = item;
            CurrentIndex = currentIndex;
            TargetIndex = targetIndex;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the item being dragged.
        /// </summary>
        public object Item { get; }

        /// <summary>
        /// Gets the current index of the item in the collection.
        /// </summary>
        public int CurrentIndex { get; }

        /// <summary>
        /// Gets the target index where the item would be placed if dropped now.
        /// </summary>
        public int TargetIndex { get; }

        #endregion
    }
}