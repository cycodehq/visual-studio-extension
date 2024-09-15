using System.Collections.ObjectModel;
using System.Windows;

namespace Cycode.VisualStudio.Extension.Shared.Components.TreeView.Nodes;

public class BaseNode : DependencyObject {
    public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register(
        nameof(IsExpanded), typeof(bool), typeof(BaseNode), new PropertyMetadata(false));

    public string Title { get; set; }
    public string Summary { get; set; }
    public string Icon { get; set; }
    public ObservableCollection<BaseNode> Items { get; set; } = [];

    public bool IsExpanded {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }
}