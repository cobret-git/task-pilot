using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.IO.Compression;
using System.Reflection;
using Serilog;

namespace TaskPilot.Core.Services
{
    public class LoggingService : IDisposable
    {
        #region Fields

        private readonly IFileSystem _fileSystem;
        private readonly IPathProvider pathProvider;
        private string? _logDirectory;
        private bool _isInitialized;
        private bool _disposed;

        #endregion

        #region Constructors

        public LoggingService(IFileSystem fileSystem, IPathProvider pathProvider)
        {
            _fileSystem = fileSystem;
            this.pathProvider = pathProvider;

        }

        #endregion

        #region Properties

        public string LogDirectory => _logDirectory ?? throw new InvalidOperationException("LoggingService not initialized. Call Initialize() first.");

        #endregion

        #region Methods

        public void Initialize()
        {
            if (_isInitialized)
                return;

            try
            {
                // Determine log directory based on build configuration
                _logDirectory = GetLogDirectory();

                // Ensure log directory exists
                EnsureLogDirectoryExists();

                // Compress old logs before starting new session
                CompressOldLogs();

                // Configure Serilog
                ConfigureSerilog();

                _isInitialized = true;

                Log.Information("LoggingService initialized successfully. Log directory: {LogDirectory}", _logDirectory);
            }
            catch (Exception ex)
            {
                // If logging setup fails, at least try to log to console
                Console.WriteLine($"Failed to initialize LoggingService: {ex}");
                throw;
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            Log.CloseAndFlush();
            _disposed = true;
        }

        #endregion

        #region Handlers

        #endregion

        #region Helpers

        private string GetLogDirectory()
        {
            return pathProvider.GetLogsDirectoryPath();
        }

        private void EnsureLogDirectoryExists()
        {
            if (string.IsNullOrEmpty(_logDirectory))
                throw new InvalidOperationException("Log directory is not set.");

            pathProvider.EnsureDirectoriesExist();
        }

        private void ConfigureSerilog()
        {
            if (string.IsNullOrEmpty(_logDirectory))
                throw new InvalidOperationException("Log directory is not set.");

            string logFilePath = _fileSystem.Path.Combine(_logDirectory, "log.txt");

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(
                    logFilePath,
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                    retainedFileCountLimit: 7) // Keep last 7 days uncompressed
                .CreateLogger();
        }

        private void CompressOldLogs()
        {
            if (string.IsNullOrEmpty(_logDirectory))
                return;

            try
            {
                if (!_fileSystem.Directory.Exists(_logDirectory))
                    return;

                // Get all log files (excluding current day's log)
                var logFiles = _fileSystem.Directory.GetFiles(_logDirectory, "log*.txt")
                    .Select(path => _fileSystem.FileInfo.New(path))
                    .Where(fileInfo => IsOldLogFile(fileInfo))
                    .ToList();

                foreach (var logFile in logFiles)
                {
                    CompressLogFile(logFile);
                }
            }
            catch (Exception ex)
            {
                // Don't fail initialization if compression fails
                Console.WriteLine($"Failed to compress old logs: {ex}");
            }
        }

        private bool IsOldLogFile(IFileInfo fileInfo)
        {
            // Consider a log file "old" if it's from yesterday or earlier
            DateTime today = DateTime.Today;
            return fileInfo.LastWriteTime.Date < today;
        }

        private void CompressLogFile(IFileInfo logFile)
        {
            try
            {
                DateTime logDate = logFile.LastWriteTime.Date;

                // Build archive path: logs/archive/YYYY/MM/DD.txt
                string archivePath = _fileSystem.Path.Combine(
                    _logDirectory!,
                    "archive",
                    logDate.Year.ToString("D4"),
                    logDate.Month.ToString("D2"));

                // Ensure archive directory exists
                if (!_fileSystem.Directory.Exists(archivePath))
                {
                    _fileSystem.Directory.CreateDirectory(archivePath);
                }

                // Create zip file name: YYYY-MM.zip
                string zipFileName = $"{logDate.Year:D4}-{logDate.Month:D2}.zip";
                string zipFilePath = _fileSystem.Path.Combine(archivePath, zipFileName);

                // Entry name inside zip: DD.txt
                string entryName = $"{logDate.Day:D2}.txt";

                // Add or update entry in zip
                using (var archive = ZipFile.Open(zipFilePath, ZipArchiveMode.Update))
                {
                    // Remove existing entry if it exists
                    var existingEntry = archive.GetEntry(entryName);
                    existingEntry?.Delete();

                    // Add new entry
                    archive.CreateEntryFromFile(logFile.FullName, entryName, CompressionLevel.Optimal);
                }

                // Delete original log file after successful compression
                _fileSystem.File.Delete(logFile.FullName);

                Console.WriteLine($"Compressed log file: {logFile.Name} -> {zipFilePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to compress log file {logFile.Name}: {ex.Message}");
            }
        }

        #endregion
    }
}
