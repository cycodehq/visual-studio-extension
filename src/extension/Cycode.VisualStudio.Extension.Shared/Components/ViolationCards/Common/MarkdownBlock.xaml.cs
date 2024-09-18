using System.Windows;

namespace Cycode.VisualStudio.Extension.Shared.Components.ViolationCards.Common;

public partial class MarkdownBlock {
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        nameof(Title), typeof(string), typeof(MarkdownBlock), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty MarkdownProperty = DependencyProperty.Register(
        nameof(Markdown), typeof(string), typeof(MarkdownBlock), new PropertyMetadata(string.Empty));

    public MarkdownBlock() {
        InitializeComponent();
        DataContext = this;
    }

    public string Title {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Markdown {
        get => (string)GetValue(MarkdownProperty);
        set {
            SetValue(MarkdownProperty, value);
            CommonMarkdown.MarkdownScrollViewer.Markdown = value.Replace("<br/>", "\n");
        }
    }
}