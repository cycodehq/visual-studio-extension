using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;

namespace Cycode.VisualStudio.Extension.Shared.Services.ErrorTagger;

public static class ErrorTaggerUtilities {
    public static int CalculateColumn(ITextSnapshot snapshot, int offset) {
        ITextSnapshotLine textLine = snapshot.GetLineFromPosition(offset);
        int newLinesCount = textLine.LineNumber; // positioning VS SDK system ignores it
        return offset - textLine.Start.Position + newLinesCount;
    }

    public static string ConvertSeverityToErrorType(string severity) {
        return severity.ToLower() switch {
            "critical" => PredefinedErrorTypeNames.SyntaxError,
            "high" => PredefinedErrorTypeNames.SyntaxError,
            "medium" => PredefinedErrorTypeNames.Warning,
            "low" => PredefinedErrorTypeNames.Suggestion,
            "info" => PredefinedErrorTypeNames.Suggestion,
            _ => PredefinedErrorTypeNames.Warning
        };
    }
}