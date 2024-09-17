using Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult.Sast;

namespace Cycode.VisualStudio.Extension.Shared.Components.TreeView.Nodes.DetectionNodes;

public class SastDetectionNode : BaseNode {
    public SastDetection Detection { get; set; }
}