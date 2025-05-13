using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult.Iac;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult.Sast;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult.Sca;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult.Secret;
using Cycode.VisualStudio.Extension.Shared.Components.TreeView.Nodes;
using Cycode.VisualStudio.Extension.Shared.Components.TreeView.Nodes.DetectionNodes;
using Cycode.VisualStudio.Extension.Shared.DTO;
using Cycode.VisualStudio.Extension.Shared.Helpers;
using Cycode.VisualStudio.Extension.Shared.Icons;
using Cycode.VisualStudio.Extension.Shared.Services;

namespace Cycode.VisualStudio.Extension.Shared.Components.TreeView;

public partial class CycodeTreeViewControl {
    private static readonly IScanResultsService _scanResultsService = ServiceLocator.GetService<IScanResultsService>();

    private static readonly IToolWindowMessengerService _toolWindowMessengerService =
        ServiceLocator.GetService<IToolWindowMessengerService>();
    
    private static readonly ITemporaryStateService _tempState =
        ServiceLocator.GetService<ITemporaryStateService>();

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
                line = detection.DetectionDetails.GetLineNumber();
                _toolWindowMessengerService.Send(
                    new MessageEventArgs(MessengerCommand.LoadSecretViolationCardControl, detection)
                );
                break;
            }
            case ScaDetectionNode scaDetectionNode: {
                ScaDetection detection = scaDetectionNode.Detection;
                filePath = detection.DetectionDetails.GetFilePath();
                line = detection.DetectionDetails.GetLineNumber();
                _toolWindowMessengerService.Send(
                    new MessageEventArgs(MessengerCommand.LoadScaViolationCardControl, detection)
                );
                break;
            }
            case IacDetectionNode iacDetectionNode: {
                IacDetection detection = iacDetectionNode.Detection;
                filePath = detection.DetectionDetails.GetFilePath();
                line = detection.DetectionDetails.GetLineNumber();
                _toolWindowMessengerService.Send(
                    new MessageEventArgs(MessengerCommand.LoadIacViolationCardControl, detection)
                );
                break;
            }
            case SastDetectionNode sastDetectionNode: {
                SastDetection detection = sastDetectionNode.Detection;
                filePath = detection.DetectionDetails.GetFilePath();
                line = detection.DetectionDetails.GetLineNumber();
                _toolWindowMessengerService.Send(
                    new MessageEventArgs(MessengerCommand.LoadSastViolationCardControl, detection)
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
    
    private static HashSet<string> GetEnabledSeverityFilters() {
        HashSet<string> enabledSeverityFilters = [];

        if (_tempState.IsTreeViewFilterByCriticalSeverityEnabled) {
            enabledSeverityFilters.Add("critical");
        }
        if (_tempState.IsTreeViewFilterByHighSeverityEnabled) {
            enabledSeverityFilters.Add("high");
        }
        if (_tempState.IsTreeViewFilterByMediumSeverityEnabled) {
            enabledSeverityFilters.Add("medium");
        }
        if (_tempState.IsTreeViewFilterByLowSeverityEnabled) {
            enabledSeverityFilters.Add("low");
        }
        if (_tempState.IsTreeViewFilterByInfoSeverityEnabled) {
            enabledSeverityFilters.Add("info");
        }

        return enabledSeverityFilters;
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
        IEnumerable<DetectionBase> detections,
        Func<DetectionBase, BaseNode> createNodeCallback
    ) {
        HashSet<string> enabledSeverityFilters = GetEnabledSeverityFilters();
        List<DetectionBase> severityFilteredDetections = detections
            .Where(detection => !enabledSeverityFilters.Contains(detection.Severity.ToLower()))
            .ToList();
        IEnumerable<IGrouping<string, DetectionBase>> detectionsByFile =
            severityFilteredDetections.GroupBy(detection => detection.GetDetectionDetails().GetFilePath());

        ScanTypeNode scanTypeNode = RootNodesManager.GetScanTypeNode(scanType);
        scanTypeNode.Summary = GetRootNodeSummary(severityFilteredDetections);

        foreach (IGrouping<string, DetectionBase> detectionsInFile in detectionsByFile) {
            string filePath = detectionsInFile.Key;

            FileNode fileNode = new() {
                Title = GetRelativeToProjectPath(filePath),
                Summary = $"{detectionsInFile.Count()} vulnerabilities",
                Icon = ExtensionIcons.GetFileIconPath(filePath)
            };

            List<DetectionBase> sortedDetectionsInFile = detectionsInFile
                .OrderByDescending(detection => GetSeverityWeight(detection.Severity))
                .ThenBy(detection => detection.GetDetectionDetails().GetLineNumber())
                .ToList();

            foreach (DetectionBase detection in sortedDetectionsInFile) fileNode.Items.Add(createNodeCallback(detection));

            scanTypeNode.Items.Add(fileNode);
        }
    }

    private void CreateSecretDetectionNodes() {
        IEnumerable<DetectionBase> detections = _scanResultsService.GetDetections(CliScanType.Secret);

        CreateDetectionNodes(CliScanType.Secret, detections, detectionBase => {
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
        IEnumerable<DetectionBase> detections = _scanResultsService.GetDetections(CliScanType.Sca);

        CreateDetectionNodes(CliScanType.Sca, detections, detectionBase => {
            ScaDetection detection = (ScaDetection)detectionBase;
            ScaDetectionNode node = new() {
                Title = detection.GetFormattedNodeTitle(),
                Icon = ExtensionIcons.GetSeverityIconPath(detection.Severity),
                Detection = detection
            };

            return node;
        });
    }

    private void CreateIacDetectionNodes() {
        IEnumerable<DetectionBase> detections = _scanResultsService.GetDetections(CliScanType.Iac);

        CreateDetectionNodes(CliScanType.Iac, detections, detectionBase => {
            IacDetection detection = (IacDetection)detectionBase;
            IacDetectionNode node = new() {
                Title = detection.GetFormattedNodeTitle(),
                Icon = ExtensionIcons.GetSeverityIconPath(detection.Severity),
                Detection = detection
            };

            return node;
        });
    }

    private void CreateSastDetectionNodes() {
        IEnumerable<DetectionBase> detections = _scanResultsService.GetDetections(CliScanType.Sast);

        CreateDetectionNodes(CliScanType.Sast, detections, detectionBase => {
            SastDetection detection = (SastDetection)detectionBase;
            SastDetectionNode node = new() {
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
        CreateIacDetectionNodes();
        CreateSastDetectionNodes();
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