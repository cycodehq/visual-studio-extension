using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace Cycode.VisualStudio.Extension.Shared.Services.ErrorList;

[Export(typeof(IViewTaggerProvider))]
[ContentType("text")]
[ContentType("projection")]
[TagType(typeof(IErrorTag))]
[TextViewRole(PredefinedTextViewRoles.Document)]
[TextViewRole(PredefinedTextViewRoles.Analyzable)]
public sealed class ErrorTaggerProvider : IViewTaggerProvider {
    private readonly Dictionary<ITextView, ErrorTagger> _createdTaggers = new();
    private readonly ILoggerService _logger = ServiceLocator.GetService<ILoggerService>();
    private readonly ITextDocumentFactoryService _textDocumentFactoryService;

    [ImportingConstructor]
    public ErrorTaggerProvider([Import] ITextDocumentFactoryService textDocumentFactoryService) {
        _textDocumentFactoryService = textDocumentFactoryService;
        CycodePackage.ErrorTaggerProvider = this;
    }

    public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag {
        // multiple views could have that buffer open simultaneously. create only one
        if (
            buffer != textView.TextBuffer ||
            typeof(IErrorTag) != typeof(T) ||
            !_textDocumentFactoryService.TryGetTextDocument(buffer, out ITextDocument document)
        )
            return null;

        if (_createdTaggers.TryGetValue(textView, out ErrorTagger createdTagger)) return createdTagger as ITagger<T>;

        _createdTaggers.Add(textView, new ErrorTagger(buffer, document));
        textView.Closed += (_, _) => _createdTaggers.Remove(textView);

        return _createdTaggers[textView] as ITagger<T>;
    }
    
    public ErrorTagger GetTagger(ITextView textView) {
        return _createdTaggers.TryGetValue(textView, out ErrorTagger createdTagger) ? createdTagger : null;
    }

    public void Rerender() {
        _logger.Debug("ErrorTaggerProvider: Rerender");
        foreach (KeyValuePair<ITextView, ErrorTagger> createdTagger in _createdTaggers) createdTagger.Value.Rerender();
    }
}