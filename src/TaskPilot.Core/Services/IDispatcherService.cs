namespace TaskPilot.Core.Services
{
    /// <summary>
    /// Provides platform-agnostic UI thread dispatching.
    /// Allows ViewModels in the Core library to execute code on the UI thread
    /// without direct platform dependencies.
    /// </summary>
    public interface IDispatcherService
    {
        /// <summary>
        /// Gets a value indicating whether the current thread has access to the UI thread.
        /// </summary>
        bool HasThreadAccess { get; }

        /// <summary>
        /// Executes the specified action on the UI thread.
        /// If already on the UI thread, executes immediately.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        void Run(Action action);

        /// <summary>
        /// Executes the specified async action on the UI thread and returns when complete.
        /// If already on the UI thread, executes immediately.
        /// </summary>
        /// <param name="asyncAction">The async action to execute.</param>
        /// <returns>A task that completes when the action has finished.</returns>
        Task RunAsync(Func<Task> asyncAction);
    }
}
