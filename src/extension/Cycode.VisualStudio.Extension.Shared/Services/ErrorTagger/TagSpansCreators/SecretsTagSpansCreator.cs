﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult.Secret;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace Cycode.VisualStudio.Extension.Shared.Services.ErrorList.TagSpansCreators;

public static class SecretsTagSpansCreator {
    private static readonly IScanResultsService _scanResultsService = ServiceLocator.GetService<IScanResultsService>();

    public static List<ITagSpan<ErrorTag>> CreateTagSpans(ITextSnapshot snapshot, ITextDocument document) {
        List<ITagSpan<ErrorTag>> tagSpans = [];

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
            let toolTipContent = $"""
                                  Severity: {detection.Severity}
                                  {detection.Type}: {detection.GetFormattedMessage()}
                                  In file: {detection.DetectionDetails.FileName}
                                  Secret SHA: {detection.DetectionDetails.Sha512}
                                  """
            select new TagSpan<ErrorTag>(snapshotSpan, new ErrorTag(
                ErrorTaggerUtilities.ConvertSeverityToErrorType(detection.Severity), toolTipContent))
        );

        return tagSpans;
    }
}