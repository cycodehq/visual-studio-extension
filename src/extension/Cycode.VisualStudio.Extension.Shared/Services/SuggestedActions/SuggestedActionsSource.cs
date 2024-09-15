using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cycode.VisualStudio.Extension.Shared.Services.SuggestedActions.Actions;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Cycode.VisualStudio.Extension.Shared.Services.SuggestedActions;

public class SuggestedActionsSource(
    ILightBulbBroker lightBulbBroker,
    ITextView textView
) : ISuggestedActionsSource {
    private readonly ILoggerService _logger = ServiceLocator.GetService<ILoggerService>();
    private ErrorTagger.ErrorTagger _tagger;

    public void Dispose() {
        if (_tagger == null) return;
        _tagger.TagsChanged -= ErrorTagsChanged;
    }

    public bool TryGetTelemetryId(out Guid telemetryId) {
        telemetryId = Guid.Empty;
        return false;
    }

    public IEnumerable<SuggestedActionSet> GetSuggestedActions(
        ISuggestedActionCategorySet requestedActionCategories,
        SnapshotSpan range,
        CancellationToken cancellationToken
    ) {
        _logger.Debug("SuggestedActionsSource: GetSuggestedActions");

        List<ISuggestedAction> actions = [];
        actions.AddRange(_tagger.GetErrorTags(range).Select(tag => new OpenViolationCardAction(tag)));

        return new[] {
            new SuggestedActionSet(actions)
        };
    }

    public async Task<bool> HasSuggestedActionsAsync(
        ISuggestedActionCategorySet requestedActionCategories,
        SnapshotSpan range,
        CancellationToken cancellationToken
    ) {
        if (TryUpdateTagger() == false) return false;

        return _tagger.GetErrorTags(range).Count > 0;
    }

    public event EventHandler<EventArgs> SuggestedActionsChanged;

    private bool TryUpdateTagger() {
        if (_tagger != null) return true;

        ErrorTagger.ErrorTagger errorTagger = CycodePackage.ErrorTaggerProvider?.GetTagger(textView);
        if (errorTagger == null) {
            _logger.Debug("SuggestedActionsSource: Tagger is not created yet");
            return false;
        }

        _tagger = errorTagger;
        _tagger.TagsChanged += ErrorTagsChanged;

        return true;
    }

    private void ErrorTagsChanged(object sender, SnapshotSpanEventArgs e) {
        _logger.Debug("SuggestedActionsSource: ErrorTagsChanged");
        ThreadHelper.ThrowIfNotOnUIThread();
        lightBulbBroker.DismissSession(textView);
        SuggestedActionsChanged?.Invoke(this, EventArgs.Empty);
    }
}