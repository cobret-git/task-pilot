using System.Collections;
using System.Threading.Tasks;

namespace TaskPilot.Core.Components.Contracts
{
    /// <summary>
    /// Defines the contract for handling reorder operations in a list.
    /// ViewModels implement this interface to receive reorder callbacks from the UI control.
    /// </summary>
    public interface IReorderHandler
    {
        /// <summary>
        /// Gets or sets a value indicating whether reordering is currently enabled.
        /// When false, drag handles should be hidden and drag operations disabled.
        /// </summary>
        bool CanReorder { get; }

        /// <summary>
        /// Gets the items collection that supports reordering.
        /// Must be an observable collection that supports Move operations.
        /// </summary>
        IList Items { get; }

        /// <summary>
        /// Called when a reorder operation completes and needs to be persisted.
        /// </summary>
        /// <param name="item">The item that was moved.</param>
        /// <param name="oldIndex">The original index before the move.</param>
        /// <param name="newIndex">The new index after the move.</param>
        /// <returns>A task representing the asynchronous persistence operation.</returns>
        Task OnReorderCompletedAsync(object item, int oldIndex, int newIndex);
    }

    /// <summary>
    /// Generic version of <see cref="IReorderHandler"/> for type-safe item handling.
    /// </summary>
    /// <typeparam name="T">The type of items in the reorderable collection.</typeparam>
    public interface IReorderHandler<T> : IReorderHandler where T : class
    {
        /// <summary>
        /// Called when a reorder operation completes and needs to be persisted.
        /// </summary>
        /// <param name="item">The item that was moved.</param>
        /// <param name="oldIndex">The original index before the move.</param>
        /// <param name="newIndex">The new index after the move.</param>
        /// <returns>A task representing the asynchronous persistence operation.</returns>
        Task OnReorderCompletedAsync(T item, int oldIndex, int newIndex);
    }
}