using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;

namespace TaskPilot.Desktop.WinApp.Controls;

public sealed partial class CollapsibleDescription : UserControl
{
    private const double CollapsedHeight = 80;
    private bool _isLoaded;
    private double _fullTextHeight;

    public CollapsibleDescription()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        SizeChanged += OnSizeChanged;
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

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (_isLoaded && e.NewSize.Width != e.PreviousSize.Width)
        {
            UpdateVisualState();
        }
    }

    private void UpdateVisualState()
    {
        // Get available width
        var availableWidth = RootGrid.ActualWidth > 0 ? RootGrid.ActualWidth : 400;

        // Measure text to get full height
        DescriptionText.Measure(new Size(availableWidth, double.PositiveInfinity));
        _fullTextHeight = DescriptionText.DesiredSize.Height;

        if (_fullTextHeight <= CollapsedHeight)
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
        // Set target height for animation
        if (_fullTextHeight > CollapsedHeight)
        {
            DescriptionContainer.MaxHeight = _fullTextHeight;
        }

        VisualStateManager.GoToState(this, "Expanded", true);
    }

    private void ShowMoreButton_Unchecked(object sender, RoutedEventArgs e)
    {
        VisualStateManager.GoToState(this, "Collapsed", true);
    }
}