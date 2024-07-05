using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult.Secret;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Tagging;

namespace Cycode.VisualStudio.Extension.Shared.Services.ErrorList;

public class ErrorTagger : ITagger<IErrorTag> {
    private readonly ILoggerService _logger;
    private readonly IScanResultsService _scanResultsService;

    private readonly ITextBuffer _buffer;
    private readonly ITextDocument _document;
    private ITextSnapshot _currentSnapshot; // save it here to compare with new snapshot

    private readonly List<ITagSpan<ErrorTag>> _tagSpans = [];

    public ErrorTagger(ITextBuffer buffer, ITextDocument document) {
        _logger = ServiceLocator.GetService<ILoggerService>();
        _scanResultsService = ServiceLocator.GetService<IScanResultsService>();

        _buffer = buffer;
        _document = document;
        _currentSnapshot = _buffer.CurrentSnapshot;
    }

    private bool IsNewSnapshot(NormalizedSnapshotSpanCollection spans) {
        return spans[0].Snapshot != _currentSnapshot;
    }

    public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

    public void Rerender() {
#if DEBUG
        _logger.Debug("ErrorTagger: Rerender for file={0}", _document.FilePath);
#endif
        _tagSpans.Clear();

        SnapshotSpan span = new(_currentSnapshot, 0, _currentSnapshot.Length);
        SnapshotSpanEventArgs spanEvent = new(span);

        TagsChanged?.Invoke(this, spanEvent);
    }

    public IEnumerable<ITagSpan<IErrorTag>> GetTags(NormalizedSnapshotSpanCollection spans) {
        if (IsNewSnapshot(spans)) {
#if DEBUG
            _logger.Debug("ErrorTagger: New snapshot for file={0}", _document.FilePath);
#endif
            _currentSnapshot = spans[0].Snapshot;
            TranslateTagSpans();
        }

        if (_tagSpans.Count == 0) {
            CreateSecretsTagSpans();
        }

        foreach (ITagSpan<ErrorTag> tagSpan in _tagSpans.Where(tagSpan => spans.IntersectsWith(tagSpan.Span))) {
            yield return tagSpan;
        }
    }

    private int CalculateColumn(int offset) {
        ITextSnapshotLine textLine = _currentSnapshot.GetLineFromPosition(offset);
        int newLinesCount = textLine.LineNumber; // positioning VS SDK system ignores it
        return offset - textLine.Start.Position + newLinesCount;
    }

    private static string ConvertSeverityToErrorType(string severity) {
        return severity.ToLower() switch {
            "critical" => PredefinedErrorTypeNames.SyntaxError,
            "high" => PredefinedErrorTypeNames.SyntaxError,
            "medium" => PredefinedErrorTypeNames.Warning,
            "low" => PredefinedErrorTypeNames.Suggestion,
            "info" => PredefinedErrorTypeNames.Suggestion,
            _ => PredefinedErrorTypeNames.Warning
        };
    }

    private void TranslateTagSpans() {
        List<TagSpan<ErrorTag>> translatedTagSpans = [];
        translatedTagSpans.AddRange(
            from tagSpan in _tagSpans
            let translatedTagSpan = tagSpan.Span.TranslateTo(_currentSnapshot, SpanTrackingMode.EdgeExclusive)
            select new TagSpan<ErrorTag>(translatedTagSpan, tagSpan.Tag)
        );

        _tagSpans.Clear();
        _tagSpans.AddRange(translatedTagSpans);
    }

    private void CreateSecretsTagSpans() {
        SecretScanResult secretScanResult = _scanResultsService.GetSecretResults();
        if (secretScanResult == null) return;

        List<SecretDetection> detections = secretScanResult.Detections
            .Where(detection => {
                string normalizedDetectionPath = Path.GetFullPath(detection.DetectionDetails.GetFilePath());
                string normalizedFilePath = Path.GetFullPath(_document.FilePath);

                return normalizedFilePath == normalizedDetectionPath;
            })
            .ToList();

        foreach (SecretDetection detection in detections) {
            int line = detection.DetectionDetails.Line;
            int column = CalculateColumn(detection.DetectionDetails.StartPosition);
            int length = detection.DetectionDetails.Length;

            SnapshotPoint startSnapshotPoint = _currentSnapshot.GetLineFromLineNumber(line).Start.Add(column);
            SnapshotPoint endSnapshotPoint = _currentSnapshot.GetLineFromLineNumber(line).Start.Add(column + length);
            SnapshotSpan snapshotSpan = new(startSnapshotPoint, endSnapshotPoint);

            string toolTipContent = $"""
                                     Severity: {detection.Severity}
                                     {detection.Type}: {detection.GetFormattedMessage()}
                                     In file: {detection.DetectionDetails.FileName}
                                     Secret SHA: {detection.DetectionDetails.Sha512}
                                     """;

            _tagSpans.Add(new TagSpan<ErrorTag>(snapshotSpan, new ErrorTag(
                ConvertSeverityToErrorType(detection.Severity),
                toolTipContent
            )));
        }
    }
}