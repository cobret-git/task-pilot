using System;
using System.IO;
using Windows.Storage;
using TaskPilot.Core.Services;

namespace TaskPilot.Desktop.WinApp.Services
{
    /// <summary>
    /// Provides access to application-specific directory paths using Windows.Storage.
    /// Uses ApplicationData.Current.LocalFolder for storing application data.
    /// </summary>
    public class PathProvider : IPathProvider
    {
        #region Fields

        private readonly string _baseDirectoryPath;
        private const string DataFolderName = "data";
        private const string LogsFolderName = "logs";
        private const string TempFolderName = "temp";

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PathProvider"/> class.
        /// </summary>
        public PathProvider()
        {
            // Get the local application data folder path
            _baseDirectoryPath = ApplicationData.Current.LocalFolder.Path;
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public string GetDataDirectoryPath()
        {
            return Path.Combine(_baseDirectoryPath, DataFolderName);
        }

        /// <inheritdoc/>
        public string GetLogsDirectoryPath()
        {
            return Path.Combine(_baseDirectoryPath, LogsFolderName);
        }

        /// <inheritdoc/>
        public string GetTempDirectoryPath()
        {
            return Path.Combine(_baseDirectoryPath, TempFolderName);
        }

        /// <inheritdoc/>
        public void EnsureDirectoriesExist()
        {
            EnsureDirectoryExists(GetDataDirectoryPath());
            EnsureDirectoryExists(GetLogsDirectoryPath());
            EnsureDirectoryExists(GetTempDirectoryPath());
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Ensures that the specified directory exists, creating it if necessary.
        /// </summary>
        /// <param name="directoryPath">The full path to the directory.</param>
        private void EnsureDirectoryExists(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        #endregion
    }
}
