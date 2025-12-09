using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using System;
using System.IO.Abstractions;
using TaskPilot.Core.Components.Data;
using TaskPilot.Core.Model;
using TaskPilot.Core.Services;
using TaskPilot.Desktop.WinApp.Services;

namespace TaskPilot.Desktop.WinApp
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        #region Fields
        private Window? _window;
        #endregion

        #region Constructors

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            Services = ConfigureServices();
            InitializeComponent();
        }
        #endregion

        #region Properties

        /// <summary>
        /// Gets the current <see cref="App"/> instance in use
        /// </summary>
        public new static App Current => (App)Application.Current;

        /// <summary>
        /// Gets the <see cref="IServiceProvider"/> instance to resolve application services.
        /// </summary>
        public IServiceProvider Services { get; }
        #endregion

        #region Methods

        /// <summary>
        /// Configures the services for the application.
        /// </summary>
        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            ConfigureDatabaseServices(services);

            services.AddSingleton<IFileSystem, FileSystem>();
            services.AddSingleton<IPathProvider, PathProvider>();
            services.AddSingleton<LoggingService>();
            services.AddSingleton<ITaskPilotDataService, TaskPilotDataService>();

            return services.BuildServiceProvider();
        }
        private static void ConfigureDatabaseServices(ServiceCollection services)
        {
            // Database configuration
            var dbConfig = CreateDatabaseConfiguration();
            services.AddSingleton(dbConfig);

            // DbContext factory
            services.AddDbContextFactory<LocalDbContext>(options =>
            {
                options.UseSqlite(dbConfig.ToConnectionString());
#if DEBUG
                options.EnableDetailedErrors();
                options.EnableSensitiveDataLogging();
#endif
            });
        }
        private static DatabaseConfiguration CreateDatabaseConfiguration()
        {
            var pathProvider = new PathProvider();
            pathProvider.EnsureDirectoriesExist();
            return new DatabaseConfiguration
            {
                DatabasePath = pathProvider.GetDataDirectoryPath(),
                DatabaseName = "TaskPilot.db"
            };
        }
        #endregion

        #region Handlers

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override async void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            // Resolve and initialize logging service
            var loggingService = Services.GetService<LoggingService>();
            loggingService?.Initialize();

            // Initialize database before showing UI
            var dbService = Services.GetRequiredService<ITaskPilotDataService>();
            var result = await dbService.InitializeDatabaseAsync();

            _window = new MainWindow();
            _window.Activate();
        }
        #endregion
    }
}
