using System;

namespace TaskPilot.Core.Services
{
    /// <summary>
    /// Provides access to application-specific directory paths.
    /// Abstracts path resolution for logs, data, configuration, and other application folders.
    /// </summary>
    public interface IPathProvider
    {
        /// <summary>
        /// Gets the path to the application's data directory.
        /// Used for storing database files and other persistent data.
        /// </summary>
        /// <returns>The full path to the data directory.</returns>
        string GetDataDirectoryPath();

        /// <summary>
        /// Gets the path to the application's logs directory.
        /// Used for storing application log files.
        /// </summary>
        /// <returns>The full path to the logs directory.</returns>
        string GetLogsDirectoryPath();

        /// <summary>
        /// Gets the path to the application's temporary directory.
        /// Used for storing temporary files that can be safely deleted.
        /// </summary>
        /// <returns>The full path to the temporary directory.</returns>
        string GetTempDirectoryPath();

        /// <summary>
        /// Ensures that all application directories exist, creating them if necessary.
        /// Should be called during application initialization.
        /// </summary>
        void EnsureDirectoriesExist();
    }
}
