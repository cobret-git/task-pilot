using System;
using TaskPilot.Core.Services;

namespace TaskPilot.Desktop.WinApp.Services
{
    public class ServiceLocator : IServiceLocator
    {
        #region Fields
        private readonly App _application;
        #endregion

        #region Constructors
        public ServiceLocator(App application)
        {
            this._application = application;
        }
        #endregion

        #region Methods

        /// <inheritdoc/>
        public object? GetService(Type type)
        {
            return _application.Services.GetService(type);
        }
        #endregion

    }
}
