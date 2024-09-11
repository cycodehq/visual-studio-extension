using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Cycode.VisualStudio.Extension.Shared.Services.SuggestedActions;

[Export(typeof(ISuggestedActionsSourceProvider))]
[Name(_categoryName)]
[ContentType("text")]
[method: ImportingConstructor]
public class SuggestedActionsSourceProvider(
    ILightBulbBroker lightBulbBroker
) : ISuggestedActionsSourceProvider {
    private const string _categoryName = "Cycode Suggested Actions";
    private readonly ILoggerService _logger = ServiceLocator.GetService<ILoggerService>();

    public ISuggestedActionsSource CreateSuggestedActionsSource(ITextView textView, ITextBuffer textBuffer) {
        if (textView == null || textBuffer == null) {
            _logger.Debug("SuggestedActionsSourceProvider: textView or textBuffer is null");
            return null;
        }

        _logger.Debug("SuggestedActionsSourceProvider: CreateSuggestedActionsSource");
        return new SuggestedActionsSource(lightBulbBroker, textView);
    }
}