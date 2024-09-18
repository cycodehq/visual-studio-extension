using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult.Sast;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace Cycode.VisualStudio.Extension.Shared.Services.ErrorTagger.TagSpansCreators;

public static class SastTagSpansCreator {
    private static readonly IScanResultsService _scanResultsService = ServiceLocator.GetService<IScanResultsService>();

    public static List<ITagSpan<DetectionTag>> CreateTagSpans(ITextSnapshot snapshot, ITextDocument document) {
        List<ITagSpan<DetectionTag>> tagSpans = [];

        SastScanResult sastScanResult = _scanResultsService.GetSastResults();
        if (sastScanResult == null) return tagSpans;

        string normalizedFilePath = Path.GetFullPath(document.FilePath);
        List<SastDetection> detections = sastScanResult.Detections
            .Where(detection => {
                string normalizedDetectionPath = Path.GetFullPath(detection.DetectionDetails.GetFilePath());
                return normalizedFilePath == normalizedDetectionPath;
            })
            .ToList();

        tagSpans.AddRange(from detection in detections
            let line = detection.DetectionDetails.LineInFile - 1
            let startSnapshotPoint = snapshot.GetLineFromLineNumber(line).Start
            let endSnapshotPoint = snapshot.GetLineFromLineNumber(line).End
            let snapshotSpan = new SnapshotSpan(startSnapshotPoint, endSnapshotPoint)
            let toolTipContent = $"""
                                  Severity: {detection.Severity}
                                  Rule: {detection.GetFormattedMessage()}
                                  In file: {detection.DetectionDetails.FileName}
                                  """
            let errorType = ErrorTaggerUtilities.ConvertSeverityToErrorType(detection.Severity)
            select new TagSpan<DetectionTag>(snapshotSpan, new DetectionTag(
                CliScanType.Sast, detection, errorType, toolTipContent
            ))
        );

        return tagSpans;
    }
}