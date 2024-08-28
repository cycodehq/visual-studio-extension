using System.Collections.Generic;
using System.Windows.Controls;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO;
using Cycode.VisualStudio.Extension.Shared.Icons;

namespace Cycode.VisualStudio.Extension.Shared.Components.TreeView.Nodes;

public class RootNodesManager {
    private readonly List<ScanTypeNode> _allNodes;

    private readonly ScanTypeNode _iacScanTypeNode = new() {
        Title = "Infrastructure As Code",
        Summary = "(coming soon)",
        Icon = ExtensionIcons.ScanTypeIac
    };

    private readonly ScanTypeNode _sastScanTypeNode = new() {
        Title = "Code Security",
        Summary = "(coming soon)",
        Icon = ExtensionIcons.ScanTypeSast
    };

    private readonly Dictionary<CliScanType, ScanTypeNode> _scanTypeToNode = new();

    private readonly ScanTypeNode _scaScanTypeNode = new() {
        Title = "Open Source Threat",
        Icon = ExtensionIcons.ScanTypeSca
    };

    private readonly ScanTypeNode _secretScanTypeNode = new() {
        Title = "Hardcoded Secrets",
        Icon = ExtensionIcons.ScanTypeSecrets
    };

    public RootNodesManager() {
        _allNodes = [
            // this order is important
            // this is order of nodes in the tree view
            _secretScanTypeNode,
            _scaScanTypeNode,
            _sastScanTypeNode,
            _iacScanTypeNode
        ];

        _scanTypeToNode[CliScanType.Secret] = _secretScanTypeNode;
        _scanTypeToNode[CliScanType.Sca] = _scaScanTypeNode;
        _scanTypeToNode[CliScanType.Sast] = _sastScanTypeNode;
        _scanTypeToNode[CliScanType.Iac] = _iacScanTypeNode;
    }

    public ScanTypeNode GetScanTypeNode(CliScanType scanType) {
        return _scanTypeToNode[scanType];
    }

    public void AddRootNodes(ItemCollection items) {
        items.Clear();

        foreach (ScanTypeNode scanTypeNode in _allNodes) {
            scanTypeNode.Items.Clear();
            // TODO remove coming soon hardcode when feature is implemented
            scanTypeNode.Summary = scanTypeNode == _sastScanTypeNode || scanTypeNode == _iacScanTypeNode
                ? "(coming soon)"
                : string.Empty;
            items.Add(scanTypeNode);
        }
    }
}