using System.Collections.ObjectModel;

namespace Cycode.VisualStudio.Extension.Shared.Components.TreeView.Nodes;

public class BaseNode {
    public string Title { get; set; }
    public string Summary { get; set; }
    public string Icon { get; set; }
    public ObservableCollection<BaseNode> Items { get; set; } = [];
}