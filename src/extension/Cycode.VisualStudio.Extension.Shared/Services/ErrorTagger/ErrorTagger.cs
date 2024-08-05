using System.Collections.Generic;
using System.Linq;
using Cycode.VisualStudio.Extension.Shared.Services.ErrorList.TagSpansCreators;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;

namespace Cycode.VisualStudio.Extension.Shared.Services.ErrorList;

public class ErrorTagger : ITagger<IErrorTag> {
    private readonly ILoggerService _logger;

    private readonly ITextBuffer _buffer;
    private readonly ITextDocument _document;
    private ITextSnapshot _currentSnapshot; // save it here to compare with new snapshot

    private readonly List<ITagSpan<ErrorTag>> _tagSpans = [];

    public ErrorTagger(ITextBuffer buffer, ITextDocument document) {
        _logger = ServiceLocator.GetService<ILoggerService>();

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
            CreateTagSpans();
        }

        foreach (ITagSpan<ErrorTag> tagSpan in _tagSpans.Where(tagSpan => spans.IntersectsWith(tagSpan.Span))) {
            yield return tagSpan;
        }
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

    private void CreateTagSpans() {
        _tagSpans.AddRange(SecretsTagSpansCreator.CreateTagSpans(_currentSnapshot, _document));
        _tagSpans.AddRange(ScaTagSpansCreator.CreateTagSpans(_currentSnapshot, _document));
    }
}