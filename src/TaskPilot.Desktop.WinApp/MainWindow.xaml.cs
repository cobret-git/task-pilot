using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using TaskPilot.Core.Components.Data;
using TaskPilot.Core.Services;
using TaskPilot.Core.ViewModel;
using TaskPilot.Desktop.WinApp.Services;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TaskPilot.Desktop.WinApp
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        #region Fields
        private readonly Microsoft.UI.Windowing.AppWindow _appWindow;
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;
        private bool navigationFrameLoaded = false;
        #endregion

        #region Constructors
        public MainWindow()
        {
            InitializeComponent();
            _appWindow = GetAppWindowForCurrentWindow();
            _navigationService = App.Current.Services.GetRequiredService<INavigationService>();
            _dialogService = App.Current.Services.GetRequiredService<IDialogService>();
            this.ExtendsContentIntoTitleBar = true; // Extend the content into the title bar and hide the default titlebar
            this.SetTitleBar(titleBar); // Set the custom title bar
        }
        #endregion

        #region Handlers
        private void titleBar_PaneToggleRequested(TitleBar sender, object args)
        {
            navView.IsPaneOpen = !navView.IsPaneOpen;
        }
        private void navView_Loaded(object sender, RoutedEventArgs e)
        {
            // nothing here yet
        }
        private async void navView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItem is not NavigationViewItem item) return;
            var tagStringVvalue = item.Tag?.ToString();
            var request = tagStringVvalue switch
            {
                "Projects" => new ProjectsBrowserPageRequest(),
                _ => null!
            };
            if (request == null) return;
            await _navigationService.NavigateToAsync(request);
            // nothing here yet
        }
        private void navFrame_Loaded(object sender, RoutedEventArgs e)
        {
            if (navigationFrameLoaded) return;
            var navigationService = (NavigationService)_navigationService;
            navigationService.Frame = this.navFrame;
            navigationFrameLoaded = true;
        }
        private void gridXamlRoot_Loaded(object sender, RoutedEventArgs e)
        {
            (_dialogService as DialogService)!.XamlRoot = gridXamlRoot.XamlRoot;
        }
        #endregion

        #region Helpers
        private AppWindow GetAppWindowForCurrentWindow()
        {
            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WindowId windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
            return AppWindow.GetFromWindowId(windowId);
        }

        #endregion

    }
}
