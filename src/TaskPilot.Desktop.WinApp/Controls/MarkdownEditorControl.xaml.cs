using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using System;
using System.Linq;

namespace TaskPilot.Desktop.WinApp.Controls;

/// <summary>
/// A reusable markdown editor control with Write/Preview tabs and formatting toolbar.
/// Supports bold, italic, links, and lists with simple click-time formatting detection.
/// 
/// Escape Special Characters:
/// Use backslash to insert literal markdown characters:
/// \* → * (asterisk)
/// \_ → _ (underscore)
/// \[ → [ (bracket)
/// \] → ] (bracket)
/// </summary>
public sealed partial class MarkdownEditorControl : UserControl
{
    #region Constructors

    public MarkdownEditorControl()
    {
        InitializeComponent();
    }

    #endregion

    #region Dependency Properties

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(MarkdownEditorControl),
            new PropertyMetadata(string.Empty, OnTextPropertyChanged));

    public static readonly DependencyProperty PlaceholderTextProperty =
        DependencyProperty.Register(
            nameof(PlaceholderText),
            typeof(string),
            typeof(MarkdownEditorControl),
            new PropertyMetadata("Type your description here...", OnPlaceholderTextPropertyChanged));

    public static readonly DependencyProperty IsReadOnlyProperty =
        DependencyProperty.Register(
            nameof(IsReadOnly),
            typeof(bool),
            typeof(MarkdownEditorControl),
            new PropertyMetadata(false, OnIsReadOnlyPropertyChanged));

    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(
            nameof(Header),
            typeof(string),
            typeof(MarkdownEditorControl),
            new PropertyMetadata(string.Empty));

    #endregion

    #region Properties

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string PlaceholderText
    {
        get => (string)GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    public string Header
    {
        get => (string)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    #endregion

    #region Events

    public event EventHandler<string>? TextChanged;

    #endregion

    #region Event Handlers - Button Clicks

    private void BoldButton_Click(object sender, RoutedEventArgs e)
    {
        ToggleMarkdownFormat("**");
        MarkdownTextBox.Focus(FocusState.Programmatic);
    }

    private void ItalicButton_Click(object sender, RoutedEventArgs e)
    {
        ToggleMarkdownFormat("*");
        MarkdownTextBox.Focus(FocusState.Programmatic);
    }

    private void LinkButton_Click(object sender, RoutedEventArgs e)
    {
        InsertLink();
        MarkdownTextBox.Focus(FocusState.Programmatic);
    }

    private void BulletListButton_Click(object sender, RoutedEventArgs e)
    {
        ToggleListFormat("-");
        MarkdownTextBox.Focus(FocusState.Programmatic);
    }

    private void NumberListButton_Click(object sender, RoutedEventArgs e)
    {
        ToggleListFormat("1.");
        MarkdownTextBox.Focus(FocusState.Programmatic);
    }

    private void MarkdownTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        // Sync text to dependency property
        if (Text != MarkdownTextBox.Text)
        {
            Text = MarkdownTextBox.Text;
        }

        // Raise event for external listeners
        TextChanged?.Invoke(this, MarkdownTextBox.Text);
    }

    private void EditorTabView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (EditorTabView.SelectedItem == PreviewTab)
        {
            RenderPreview();
        }
    }

    #endregion

    #region Property Changed Callbacks

    private static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MarkdownEditorControl control && e.NewValue is string newText)
        {
            if (control.MarkdownTextBox.Text != newText)
            {
                control.MarkdownTextBox.Text = newText;
            }
        }
    }

    private static void OnPlaceholderTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MarkdownEditorControl control && e.NewValue is string placeholder)
        {
            control.MarkdownTextBox.PlaceholderText = placeholder;
        }
    }

    private static void OnIsReadOnlyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MarkdownEditorControl control && e.NewValue is bool isReadOnly)
        {
            control.MarkdownTextBox.IsReadOnly = isReadOnly;
            control.FormattingToolbar.IsEnabled = !isReadOnly;
        }
    }

    #endregion

    #region Formatting Logic

    /// <summary>
    /// Toggles markdown formatting (bold/italic) around selection.
    /// Detects existing formatting at click time only.
    /// </summary>
    private void ToggleMarkdownFormat(string marker)
    {
        string text = MarkdownTextBox.Text;
        int selStart = MarkdownTextBox.SelectionStart;
        int selLength = MarkdownTextBox.SelectionLength;

        // If no selection, do nothing (could insert placeholder like "**text**")
        if (selLength == 0)
        {
            // Insert placeholder
            string placeholder = $"{marker}text{marker}";
            MarkdownTextBox.SelectedText = placeholder;

            // Select "text" part
            MarkdownTextBox.SelectionStart = selStart + marker.Length;
            MarkdownTextBox.SelectionLength = 4;
            return;
        }

        string selectedText = text.Substring(selStart, selLength);

        // Check if already formatted (markers immediately adjacent to selection)
        int markerLen = marker.Length;
        bool hasPrefixMarker = selStart >= markerLen &&
                               text.Substring(selStart - markerLen, markerLen) == marker;
        bool hasSuffixMarker = selStart + selLength + markerLen <= text.Length &&
                               text.Substring(selStart + selLength, markerLen) == marker;

        if (hasPrefixMarker && hasSuffixMarker)
        {
            // Remove markers
            string before = text.Substring(0, selStart - markerLen);
            string after = text.Substring(selStart + selLength + markerLen);

            MarkdownTextBox.Text = before + selectedText + after;
            MarkdownTextBox.SelectionStart = selStart - markerLen;
            MarkdownTextBox.SelectionLength = selectedText.Length;
        }
        else
        {
            // Add markers
            string before = text.Substring(0, selStart);
            string after = text.Substring(selStart + selLength);

            MarkdownTextBox.Text = before + marker + selectedText + marker + after;
            MarkdownTextBox.SelectionStart = selStart + markerLen;
            MarkdownTextBox.SelectionLength = selectedText.Length;
        }
    }

    /// <summary>
    /// Inserts or formats a markdown link [text](url).
    /// </summary>
    private void InsertLink()
    {
        string text = MarkdownTextBox.Text;
        int selStart = MarkdownTextBox.SelectionStart;
        int selLength = MarkdownTextBox.SelectionLength;

        if (selLength == 0)
        {
            // No selection - insert template
            string template = "[text](url)";
            MarkdownTextBox.SelectedText = template;

            // Select "text" part
            MarkdownTextBox.SelectionStart = selStart + 1;
            MarkdownTextBox.SelectionLength = 4;
        }
        else
        {
            // Has selection - wrap as link text
            string selectedText = text.Substring(selStart, selLength);
            string linkFormat = $"[{selectedText}](url)";

            MarkdownTextBox.SelectedText = linkFormat;

            // Select "url" part for easy replacement
            int urlStart = selStart + 1 + selectedText.Length + 2; // [text](|url)
            MarkdownTextBox.SelectionStart = urlStart;
            MarkdownTextBox.SelectionLength = 3;
        }
    }

    /// <summary>
    /// Toggles list formatting for selected lines.
    /// Supports both unordered (- * +) and ordered (1. 2. 3.) lists.
    /// </summary>
    private void ToggleListFormat(string marker)
    {
        string text = MarkdownTextBox.Text;
        int selStart = MarkdownTextBox.SelectionStart;
        int selLength = MarkdownTextBox.SelectionLength;

        // Find line boundaries for selection
        int lineStart = text.LastIndexOf('\n', selStart) + 1;
        int lineEnd = selLength > 0
            ? text.IndexOf('\n', selStart + selLength)
            : text.IndexOf('\n', selStart);

        if (lineEnd == -1) lineEnd = text.Length;

        // Get all lines in selection
        string selectedLines = text.Substring(lineStart, lineEnd - lineStart);
        string[] lines = selectedLines.Split('\n');

        // Check if all lines already have list markers
        bool isOrderedList = marker == "1.";
        bool allListed = true;

        foreach (string line in lines)
        {
            string trimmed = line.TrimStart();
            if (string.IsNullOrWhiteSpace(trimmed))
                continue; // Skip empty lines

            bool hasMarker;
            if (isOrderedList)
            {
                // Check for any number followed by period
                hasMarker = trimmed.Length > 0 && char.IsDigit(trimmed[0]) && trimmed.Contains('.');
            }
            else
            {
                // Check for -, *, or +
                hasMarker = trimmed.StartsWith("- ") || trimmed.StartsWith("* ") || trimmed.StartsWith("+ ");
            }

            if (!hasMarker)
            {
                allListed = false;
                break;
            }
        }

        // Toggle list formatting
        string[] processedLines = new string[lines.Length];

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            string trimmed = line.TrimStart();

            if (string.IsNullOrWhiteSpace(trimmed))
            {
                processedLines[i] = line; // Keep empty lines as-is
                continue;
            }

            if (allListed)
            {
                // Remove list markers
                if (isOrderedList)
                {
                    // Remove "1. " or "2. " etc
                    int dotIndex = trimmed.IndexOf('.');
                    if (dotIndex > 0 && char.IsDigit(trimmed[0]))
                    {
                        processedLines[i] = trimmed.Substring(dotIndex + 1).TrimStart();
                    }
                    else
                    {
                        processedLines[i] = line;
                    }
                }
                else
                {
                    // Remove "- " or "* " or "+ "
                    if (trimmed.StartsWith("- ") || trimmed.StartsWith("* ") || trimmed.StartsWith("+ "))
                    {
                        processedLines[i] = trimmed.Substring(2);
                    }
                    else
                    {
                        processedLines[i] = line;
                    }
                }
            }
            else
            {
                // Add list markers
                if (isOrderedList)
                {
                    processedLines[i] = $"{i + 1}. {trimmed}";
                }
                else
                {
                    processedLines[i] = $"{marker} {trimmed}";
                }
            }
        }

        // Rebuild text
        string newSelectedLines = string.Join("\n", processedLines);
        string before = text.Substring(0, lineStart);
        string after = lineEnd < text.Length ? text.Substring(lineEnd) : string.Empty;

        MarkdownTextBox.Text = before + newSelectedLines + after;
        MarkdownTextBox.SelectionStart = lineStart;
        MarkdownTextBox.SelectionLength = newSelectedLines.Length;
    }

    #endregion

    #region Preview Rendering

    /// <summary>
    /// Renders the markdown preview using simple RichTextBlock.
    /// For production, consider using Markdig library for full CommonMark support.
    /// </summary>
    private void RenderPreview()
    {
        string markdown = MarkdownTextBox.Text;

        if (string.IsNullOrWhiteSpace(markdown))
        {
            EmptyPreviewText.Visibility = Visibility.Visible;
            PreviewScrollViewer.Visibility = Visibility.Collapsed;
            return;
        }

        EmptyPreviewText.Visibility = Visibility.Collapsed;
        PreviewScrollViewer.Visibility = Visibility.Visible;

        // Simple markdown rendering
        // TODO: Replace with Markdig for production use
        RenderSimpleMarkdown(markdown);
    }

    /// <summary>
    /// Simple markdown renderer for basic bold and italic.
    /// This is a placeholder - use Markdig library for full markdown support.
    /// </summary>
    private void RenderSimpleMarkdown(string markdown)
    {
        PreviewRichText.Blocks.Clear();
        var paragraph = new Paragraph();

        int i = 0;
        while (i < markdown.Length)
        {
            // Check for escaped characters
            if (markdown[i] == '\\' && i + 1 < markdown.Length)
            {
                // Render next character literally
                paragraph.Inlines.Add(new Run { Text = markdown[i + 1].ToString() });
                i += 2;
                continue;
            }

            // Check for bold+italic (***)
            if (i + 2 < markdown.Length && markdown.Substring(i, 3) == "***")
            {
                int endIndex = markdown.IndexOf("***", i + 3);
                if (endIndex > i)
                {
                    string content = markdown.Substring(i + 3, endIndex - i - 3);
                    var boldItalic = new Bold();
                    var italic = new Italic();
                    italic.Inlines.Add(new Run { Text = content });
                    boldItalic.Inlines.Add(italic);
                    paragraph.Inlines.Add(boldItalic);
                    i = endIndex + 3;
                    continue;
                }
            }
            // Check for bold (**)
            else if (i + 1 < markdown.Length && markdown.Substring(i, 2) == "**")
            {
                int endIndex = markdown.IndexOf("**", i + 2);
                if (endIndex > i)
                {
                    string content = markdown.Substring(i + 2, endIndex - i - 2);
                    var bold = new Bold();
                    bold.Inlines.Add(new Run { Text = content });
                    paragraph.Inlines.Add(bold);
                    i = endIndex + 2;
                    continue;
                }
            }
            // Check for italic (*)
            else if (markdown[i] == '*')
            {
                int endIndex = markdown.IndexOf('*', i + 1);
                if (endIndex > i)
                {
                    string content = markdown.Substring(i + 1, endIndex - i - 1);
                    var italic = new Italic();
                    italic.Inlines.Add(new Run { Text = content });
                    paragraph.Inlines.Add(italic);
                    i = endIndex + 1;
                    continue;
                }
            }
            // Check for newline
            else if (markdown[i] == '\n')
            {
                paragraph.Inlines.Add(new LineBreak());
                i++;
                continue;
            }

            // Regular text
            paragraph.Inlines.Add(new Run { Text = markdown[i].ToString() });
            i++;
        }

        PreviewRichText.Blocks.Add(paragraph);
    }

    #endregion
}