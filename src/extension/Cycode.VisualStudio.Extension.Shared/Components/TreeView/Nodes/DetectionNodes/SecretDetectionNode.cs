using Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult.Secret;

namespace Cycode.VisualStudio.Extension.Shared.Components.TreeView.Nodes;

public class SecretDetectionNode : BaseNode {
    public SecretDetection Detection { get; set; }
}