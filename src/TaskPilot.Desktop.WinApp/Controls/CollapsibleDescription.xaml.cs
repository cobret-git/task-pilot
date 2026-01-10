using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace TaskPilot.Desktop.WinApp.Controls;

public sealed partial class CollapsibleDescription : UserControl
{
    private const double CollapsedHeight = 80;
    private bool _isLoaded;

    public CollapsibleDescription()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(CollapsibleDescription),
            new PropertyMetadata(string.Empty, OnTextChanged));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CollapsibleDescription control)
        {
            control.DescriptionText.Text = e.NewValue as string ?? string.Empty;
            
            if (control._isLoaded)
            {
                control.UpdateVisualState();
            }
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _isLoaded = true;
        UpdateVisualState();
    }

    private void UpdateVisualState()
    {
        // Force layout update to measure actual text height
        DescriptionText.Measure(new Windows.Foundation.Size(
            DescriptionContainer.ActualWidth > 0 ? DescriptionContainer.ActualWidth : RootGrid.ActualWidth, 
            double.PositiveInfinity));

        var textHeight = DescriptionText.DesiredSize.Height;

        if (textHeight <= CollapsedHeight)
        {
            // Content is short enough to fit without expanding
            VisualStateManager.GoToState(this, "ShortContent", false);
            ShowMoreButton.IsChecked = false;
        }
        else if (ShowMoreButton.IsChecked == true)
        {
            // Content is long and currently expanded
            VisualStateManager.GoToState(this, "Expanded", false);
        }
        else
        {
            // Content is long and currently collapsed
            VisualStateManager.GoToState(this, "Collapsed", false);
        }
    }

    private void ShowMoreButton_Checked(object sender, RoutedEventArgs e)
    {
        VisualStateManager.GoToState(this, "Expanded", true);
    }

    private void ShowMoreButton_Unchecked(object sender, RoutedEventArgs e)
    {
        VisualStateManager.GoToState(this, "Collapsed", true);
    }
}
