using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult.Sca;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace Cycode.VisualStudio.Extension.Shared.Services.ErrorTagger.TagSpansCreators;

public static class ScaTagSpansCreator {
    private static readonly IScanResultsService _scanResultsService = ServiceLocator.GetService<IScanResultsService>();

    public static List<ITagSpan<DetectionTag>> CreateTagSpans(ITextSnapshot snapshot, ITextDocument document) {
        List<ITagSpan<DetectionTag>> tagSpans = [];

        string normalizedFilePath = Path.GetFullPath(document.FilePath);
        List<ScaDetection> detections = _scanResultsService.GetScaDetections()
            .Where(detection => {
                string normalizedDetectionPath = Path.GetFullPath(detection.DetectionDetails.GetFilePath());
                return normalizedFilePath == normalizedDetectionPath;
            })
            .ToList();

        tagSpans.AddRange(from detection in detections
            let line = detection.DetectionDetails.GetLineNumber() - 1
            let length = snapshot.GetLineFromLineNumber(line).Length
            let startSnapshotPoint = snapshot.GetLineFromLineNumber(line).Start.Add(0)
            let endSnapshotPoint = snapshot.GetLineFromLineNumber(line).Start.Add(length)
            let snapshotSpan = new SnapshotSpan(startSnapshotPoint, endSnapshotPoint)
            let firstPatchedVersion = detection.DetectionDetails.Alert?.FirstPatchedVersion
            let firstPatchedVersionMessage =
                firstPatchedVersion != null ? $"First patched version: {firstPatchedVersion}" : ""
            let fileName = Path.GetFileName(document.FilePath)
            let lockFileNote = ScaHelper.IsSupportedLockFile(fileName)
                ? $"\n\nAvoid manual packages upgrades in lock files. Update the {ScaHelper.GetPackageFileForLockFile(fileName)} file and re-generate the lock file."
                : ""
            let toolTipContent = $"""
                                  Severity: {detection.Severity}
                                  {firstPatchedVersionMessage}
                                  {detection.GetFormattedMessage()}
                                  {lockFileNote}
                                  """
            let errorType = ErrorTaggerUtilities.ConvertSeverityToErrorType(detection.Severity)
            select new TagSpan<DetectionTag>(snapshotSpan, new DetectionTag(
                CliScanType.Sca, detection, errorType, toolTipContent
            ))
        );

        return tagSpans;
    }
}