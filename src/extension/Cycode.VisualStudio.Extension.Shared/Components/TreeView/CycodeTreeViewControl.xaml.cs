using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult.Sca;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult.Secret;
using Cycode.VisualStudio.Extension.Shared.Components.TreeView.Nodes;
using Cycode.VisualStudio.Extension.Shared.DTO;
using Cycode.VisualStudio.Extension.Shared.Helpers;
using Cycode.VisualStudio.Extension.Shared.Icons;
using Cycode.VisualStudio.Extension.Shared.Services;

namespace Cycode.VisualStudio.Extension.Shared.Components.TreeView;

public partial class CycodeTreeViewControl {
    private static readonly IScanResultsService _scanResultsService = ServiceLocator.GetService<IScanResultsService>();

    private static readonly IToolWindowMessengerService _toolWindowMessengerService =
        ServiceLocator.GetService<IToolWindowMessengerService>();

    private static readonly ILoggerService _logger = ServiceLocator.GetService<ILoggerService>();

    public CycodeTreeViewControl() {
        InitializeComponent();

        RootNodesManager.AddRootNodes(TreeView.Items);
    }

    private RootNodesManager RootNodesManager { get; } = new();

    private void OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
        string filePath = string.Empty;
        int line = 0;

        switch (e.NewValue) {
            case SecretDetectionNode secretDetectionNode: {
                SecretDetection detection = secretDetectionNode.Detection;
                filePath = detection.DetectionDetails.GetFilePath();
                line = detection.DetectionDetails.Line + 1;
                _toolWindowMessengerService.Send(
                    new MessageEventArgs(MessengerCommand.LoadSecretViolationCardControl, detection)
                );
                break;
            }
            case ScaDetectionNode scaDetectionNode: {
                ScaDetection detection = scaDetectionNode.Detection;
                filePath = detection.DetectionDetails.GetFilePath();
                line = detection.DetectionDetails.LineInFile;
                _toolWindowMessengerService.Send(
                    new MessageEventArgs(MessengerCommand.LoadScaViolationCardControl, detection)
                );
                break;
            }
        }

        if (string.IsNullOrEmpty(filePath)) return;
        FileNavigator.NavigateToFileAndLine(filePath, line);
    }

    private static int GetSeverityWeight(string severity) {
        return severity.ToLower() switch {
            "critical" => 5,
            "high" => 4,
            "medium" => 3,
            "low" => 2,
            "info" => 1,
            _ => 0
        };
    }

    private static string GetRootNodeSummary(IEnumerable<DetectionBase> sortedDetections) {
        // detections must be sorted by severity already
        IEnumerable<IGrouping<string, DetectionBase>> groupedBySeverity = sortedDetections
            .GroupBy(detection => detection.Severity);
        return string.Join(" | ", groupedBySeverity
            .Select(group => $"{group.Key} - {group.Count()}"));
    }

    private static string GetRelativeToProjectPath(string filePath) {
        string projectRootPath = Path.GetFullPath(SolutionHelper.GetSolutionRootDirectory());
        string normalizedFilePath = Path.GetFullPath(filePath);

        string relativePath = normalizedFilePath.Replace(projectRootPath, string.Empty);

        return relativePath.StartsWith(Path.DirectorySeparatorChar.ToString())
            ? relativePath.Substring(1)
            : relativePath;
    }

    private void CreateDetectionNodes(
        CliScanType scanType,
        ScanResultBase scanResults,
        Func<DetectionBase, BaseNode> createNodeCallback
    ) {
        List<DetectionBase> sortedDetections = scanResults.GetDetections()
            .OrderByDescending(detection => GetSeverityWeight(detection.Severity))
            .ToList();
        IEnumerable<IGrouping<string, DetectionBase>> detectionsByFile =
            sortedDetections.GroupBy(detection => detection.GetDetectionDetails().GetFilePath());

        ScanTypeNode scanTypeNode = RootNodesManager.GetScanTypeNode(scanType);
        scanTypeNode.Summary = GetRootNodeSummary(sortedDetections);

        foreach (IGrouping<string, DetectionBase> detectionsInFile in detectionsByFile) {
            string filePath = detectionsInFile.Key;

            FileNode fileNode = new() {
                Title = GetRelativeToProjectPath(filePath),
                Summary = $"{detectionsInFile.Count()} vulnerabilities",
                Icon = ExtensionIcons.GetFileIconPath(filePath)
            };

            foreach (DetectionBase detection in detectionsInFile) fileNode.Items.Add(createNodeCallback(detection));

            scanTypeNode.Items.Add(fileNode);
        }
    }

    private void CreateSecretDetectionNodes() {
        SecretScanResult secretResults = _scanResultsService.GetSecretResults();
        if (secretResults == null) return;

        CreateDetectionNodes(CliScanType.Secret, secretResults, detectionBase => {
            SecretDetection detection = (SecretDetection)detectionBase;
            SecretDetectionNode node = new() {
                Title = detection.GetFormattedNodeTitle(),
                Icon = ExtensionIcons.GetSeverityIconPath(detection.Severity),
                Detection = detection
            };

            return node;
        });
    }

    private void CreateScaDetectionNodes() {
        ScaScanResult scaResults = _scanResultsService.GetScaResults();
        if (scaResults == null) return;

        CreateDetectionNodes(CliScanType.Sca, scaResults, detectionBase => {
            ScaDetection detection = (ScaDetection)detectionBase;
            ScaDetectionNode node = new() {
                Title = detection.GetFormattedNodeTitle(),
                Icon = ExtensionIcons.GetSeverityIconPath(detection.Severity),
                Detection = detection
            };

            return node;
        });
    }

    private void CreateNodes() {
        RootNodesManager.AddRootNodes(TreeView.Items);
        CreateSecretDetectionNodes();
        CreateScaDetectionNodes();
    }

    public void RefreshTree() {
        _logger.Debug("Refresh tree view");
        TreeView.Items.Clear();
        CreateNodes();
    }

    private void SetIsExpandedToAllNodes(bool isExpanded) {
        Stack<BaseNode> stack = new(TreeView.Items.Cast<BaseNode>());
        while (stack.Any()) {
            BaseNode node = stack.Pop();
            foreach (BaseNode child in node.Items) stack.Push(child);
            node.IsExpanded = isExpanded;
        }

        TreeView.UpdateLayout();
    }

    public void ExpandAllNodes() {
        _logger.Debug("Expand all nodes");
        SetIsExpandedToAllNodes(true);
    }

    public void CollapseAllNodes() {
        _logger.Debug("Collapse all nodes");
        SetIsExpandedToAllNodes(false);
    }
}