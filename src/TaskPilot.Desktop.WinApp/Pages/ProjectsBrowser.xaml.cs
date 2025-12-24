using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TaskPilot.Core.Components.Entities;
using TaskPilot.Core.ViewModel;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TaskPilot.Desktop.WinApp.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ProjectsBrowser : Page
    {
        public ProjectsBrowser()
        {
            InitializeComponent();
        }

        #region Properties
        public ProjectsBrowserViewModel ViewModel => (ProjectsBrowserViewModel)DataContext;
        #endregion

        #region Handlers
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

        private async void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is Project project)
            {
                await ViewModel.OpenProjectCommand.ExecuteAsync(project);
            }
        }

        private void ListView_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            // Get the data context of the right-tapped element
            if (e.OriginalSource is FrameworkElement element)
            {
                // Walk up the visual tree to find the ListViewItem
                var item = element;
                while (item != null && item is not ListViewItem)
                {
                    item = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetParent(item) as FrameworkElement;
                }

                // If we found a ListViewItem, get its DataContext (the Project)
                if (item is ListViewItem listViewItem && listViewItem.Content is Project project)
                {
                    ViewModel.SelectedItem = project;
                }
            }
        }
        #endregion
    }
}
