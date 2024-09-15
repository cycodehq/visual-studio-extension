using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult.Secret;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace Cycode.VisualStudio.Extension.Shared.Services.ErrorTagger.TagSpansCreators;

public static class SecretsTagSpansCreator {
    private static readonly IScanResultsService _scanResultsService = ServiceLocator.GetService<IScanResultsService>();

    public static List<ITagSpan<DetectionTag>> CreateTagSpans(ITextSnapshot snapshot, ITextDocument document) {
        List<ITagSpan<DetectionTag>> tagSpans = [];

        SecretScanResult secretScanResult = _scanResultsService.GetSecretResults();
        if (secretScanResult == null) return tagSpans;

        string normalizedFilePath = Path.GetFullPath(document.FilePath);
        List<SecretDetection> detections = secretScanResult.Detections
            .Where(detection => {
                string normalizedDetectionPath = Path.GetFullPath(detection.DetectionDetails.GetFilePath());
                return normalizedFilePath == normalizedDetectionPath;
            })
            .ToList();

        tagSpans.AddRange(from detection in detections
            let line = detection.DetectionDetails.Line
            let column = ErrorTaggerUtilities.CalculateColumn(snapshot, detection.DetectionDetails.StartPosition)
            let length = detection.DetectionDetails.Length
            let startSnapshotPoint = snapshot.GetLineFromLineNumber(line).Start.Add(column)
            let endSnapshotPoint = snapshot.GetLineFromLineNumber(line).Start.Add(column + length)
            let snapshotSpan = new SnapshotSpan(startSnapshotPoint, endSnapshotPoint)
            let detectedValue = snapshot.GetText(snapshotSpan)
            // FIXME(MarshalX): detected value will be empty until user will not open the file to trigger error tagger
            let _ = detection.DetectionDetails.DetectedValue = detectedValue
            let toolTipContent = $"""
                                  Severity: {detection.Severity}
                                  {detection.Type}: {detection.GetFormattedMessage()}
                                  In file: {detection.DetectionDetails.FileName}
                                  Secret SHA: {detection.DetectionDetails.Sha512}
                                  """
            let errorType = ErrorTaggerUtilities.ConvertSeverityToErrorType(detection.Severity)
            select new TagSpan<DetectionTag>(snapshotSpan, new DetectionTag(
                CliScanType.Secret, detection, errorType, toolTipContent
            ))
        );

        return tagSpans;
    }
}