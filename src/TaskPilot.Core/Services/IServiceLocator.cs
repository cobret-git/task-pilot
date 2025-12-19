namespace TaskPilot.Core.Services
{
    /// <summary>
    /// Provides a service location abstraction for resolving services from the application's dependency injection container.
    /// This interface acts as a bridge to access the underlying <see cref="IServiceProvider"/> without direct coupling.
    /// </summary>
    /// <remarks>
    /// This interface is typically used in scenarios where direct dependency injection is not feasible,
    /// such as locating views/windows based on their associated view models, or resolving services
    /// in contexts where constructor injection is not available.
    /// </remarks>
    public interface IServiceLocator
    {
        /// <summary>
        /// Retrieves a service instance of the specified type from the dependency injection container.
        /// </summary>
        /// <param name="type">The type of service to retrieve.</param>
        /// <returns>
        /// An instance of the requested service if found; otherwise, <c>null</c> if the service is not registered.
        /// </returns>
        /// <example>
        /// <code>
        /// // Resolve a specific service
        /// var viewModel = serviceLocator.GetService(typeof(MainViewModel)) as MainViewModel;
        /// 
        /// // Locate a window by its view model type
        /// var window = serviceLocator.GetService(typeof(SettingsWindow)) as Window;
        /// </code>
        /// </example>
        object? GetService(Type type);
    }
}
