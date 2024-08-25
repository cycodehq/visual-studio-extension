using System.Collections.Generic;
using System.Windows.Controls;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO;
using Cycode.VisualStudio.Extension.Shared.Icons;

namespace Cycode.VisualStudio.Extension.Shared.Components.TreeView.Nodes;

public class RootNodesManager {
    private readonly ScanTypeNode _secretScanTypeNode = new() {
        Title = "Hardcoded Secrets",
        Icon = ExtensionIcons.ScanTypeSecrets,
    };

    private readonly ScanTypeNode _scaScanTypeNode = new() {
        Title = "Open Source Threat",
        Icon = ExtensionIcons.ScanTypeSca,
    };
    
    private readonly ScanTypeNode _sastScanTypeNode = new() {
        Title = "Code Security (coming soon)",
        Icon = ExtensionIcons.ScanTypeSast,
    };
    
    private readonly ScanTypeNode _iacScanTypeNode = new() {
        Title = "Infrastructure As Code (coming soon)",
        Icon = ExtensionIcons.ScanTypeIac,
    };

    private readonly Dictionary<CliScanType, ScanTypeNode> _scanTypeToNode = new();

    public RootNodesManager() {
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
        
        _secretScanTypeNode.Items.Clear();
        _scaScanTypeNode.Items.Clear();
        _sastScanTypeNode.Items.Clear();
        _iacScanTypeNode.Items.Clear();

        // TODO reset summary

        // the order of adding nodes is important
        items.Add(_secretScanTypeNode);
        items.Add(_scaScanTypeNode);
        items.Add(_sastScanTypeNode);
        items.Add(_iacScanTypeNode);
    }
}