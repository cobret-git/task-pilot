using Microsoft.UI.Dispatching;
using System;
using System.Threading.Tasks;
using TaskPilot.Core.Services;

namespace TaskPilot.Desktop.WinApp.Services
{
    /// <summary>
    /// WinUI implementation of <see cref="IDispatcherService"/> using <see cref="DispatcherQueue"/>.
    /// Enables platform-independent ViewModels to safely update UI-bound properties.
    /// </summary>
    public class DispatcherService : IDispatcherService
    {
        #region Fields
        private readonly DispatcherQueue _dispatcherQueue;
        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DispatcherService"/> class.
        /// </summary>
        /// <param name="dispatcherQueue">The WinUI dispatcher queue for the UI thread.</param>
        public DispatcherService(DispatcherQueue dispatcherQueue)
        {
            _dispatcherQueue = dispatcherQueue ?? throw new ArgumentNullException(nameof(dispatcherQueue));
        }

        #endregion

        #region Properties

        /// <inheritdoc/>
        public bool HasThreadAccess => _dispatcherQueue.HasThreadAccess;

        #endregion

        #region Methods

        /// <inheritdoc/>
        public void Run(Action action)
        {
            ArgumentNullException.ThrowIfNull(action);

            if (HasThreadAccess)
                action();
            else
                _dispatcherQueue.TryEnqueue(() => action());
        }

        /// <inheritdoc/>
        public Task RunAsync(Func<Task> asyncAction)
        {
            ArgumentNullException.ThrowIfNull(asyncAction);

            if (HasThreadAccess)
                return asyncAction();

            var tcs = new TaskCompletionSource();
            _dispatcherQueue.TryEnqueue(async () =>
            {
                try
                {
                    await asyncAction();
                    tcs.SetResult();
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });
            return tcs.Task;
        }

        #endregion
    }
}
