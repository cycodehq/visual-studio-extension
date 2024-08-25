using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult.Sca;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult.Secret;
using Cycode.VisualStudio.Extension.Shared.Components.TreeView.Nodes;
using Cycode.VisualStudio.Extension.Shared.Helpers;
using Cycode.VisualStudio.Extension.Shared.Icons;
using Cycode.VisualStudio.Extension.Shared.Services;

namespace Cycode.VisualStudio.Extension.Shared.Components.TreeView;

public partial class CycodeTreeViewControl {
    private static readonly IScanResultsService _scanResultsService = ServiceLocator.GetService<IScanResultsService>();
    private static readonly ILoggerService _logger = ServiceLocator.GetService<ILoggerService>();

    private RootNodesManager RootNodesManager { get; } = new();

    public CycodeTreeViewControl() {
        InitializeComponent();

        RootNodesManager.AddRootNodes(TreeView.Items);
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
        IEnumerable<IGrouping<string, DetectionBase>> detectionsByFile,
        Func<DetectionBase, BaseNode> createNodeCallback
    ) {
        // TODO calculate and set summary for root node here

        foreach (IGrouping<string, DetectionBase> detectionsInFile in detectionsByFile) {
            string filePath = detectionsInFile.Key;
            string summary = $"{detectionsInFile.Count()} vulnerabilities";

            FileNode fileNode = new() {
                Title = $"{GetRelativeToProjectPath(filePath)} {summary}",
                Icon = ExtensionIcons.GetFileIconPath(filePath)
            };

            foreach (DetectionBase detection in detectionsInFile) {
                fileNode.Items.Add(createNodeCallback(detection));
            }

            RootNodesManager.GetScanTypeNode(scanType).Items.Add(fileNode);
        }
    }

    private void CreateSecretDetectionNodes() {
        SecretScanResult secretResults = _scanResultsService.GetSecretResults();
        if (secretResults == null) {
            return;
        }

        // FIXME move to CreateDetectionNodes method
        List<SecretDetection> sortedDetections = secretResults.Detections
            .OrderByDescending(detection => GetSeverityWeight(detection.Severity))
            .ToList();
        IEnumerable<IGrouping<string, SecretDetection>> detectionsByFile =
            sortedDetections.GroupBy(detection => detection.DetectionDetails.GetFilePath());
        // end FIXME

        CreateDetectionNodes(CliScanType.Secret, detectionsByFile, detectionBase => {
            SecretDetection detection = (SecretDetection)detectionBase;
            SecretDetectionNode node = new() {
                Title = detection.GetFormattedNodeTitle(),
                Icon = ExtensionIcons.GetSeverityIconPath(detection.Severity),
            };

            return node;
        });
    }
    
    private void CreateScaDetectionNodes() {
        ScaScanResult scaResults = _scanResultsService.GetScaResults();
        if (scaResults == null) {
            return;
        }

        // FIXME move to CreateDetectionNodes method
        List<ScaDetection> sortedDetections = scaResults.Detections
            .OrderByDescending(detection => GetSeverityWeight(detection.Severity))
            .ToList();
        IEnumerable<IGrouping<string, ScaDetection>> detectionsByFile =
            sortedDetections.GroupBy(detection => detection.DetectionDetails.GetFilePath());
        // end FIXME

        CreateDetectionNodes(CliScanType.Sca, detectionsByFile, detectionBase => {
            ScaDetection detection = (ScaDetection)detectionBase;
            ScaDetectionNode node = new() {
                Title = detection.GetFormattedNodeTitle(),
                Icon = ExtensionIcons.GetSeverityIconPath(detection.Severity),
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
}