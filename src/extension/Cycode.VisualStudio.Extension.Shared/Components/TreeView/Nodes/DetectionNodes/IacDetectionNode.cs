using Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult.Iac;

namespace Cycode.VisualStudio.Extension.Shared.Components.TreeView.Nodes.DetectionNodes;

public class IacDetectionNode : BaseNode {
    public IacDetection Detection { get; set; }
}