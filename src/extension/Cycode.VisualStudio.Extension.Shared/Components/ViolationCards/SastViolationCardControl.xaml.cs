using System.Collections.Generic;
using System.Linq;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult.Sast;
using Cycode.VisualStudio.Extension.Shared.Helpers;
using Cycode.VisualStudio.Extension.Shared.Icons;

namespace Cycode.VisualStudio.Extension.Shared.Components.ViolationCards;

public partial class SastViolationCardControl {
    private const int _customRemediationGuidelinesRowIndex = 9;

    public SastViolationCardControl(SastDetection detection) {
        InitializeComponent();

        Header.Icon.Source = ExtensionIcons.GetCardSeverityBitmapSource(detection.Severity);
        Header.Title.Text = detection.GetFormattedMessage();

        IEnumerable<string> renderedCwe =
            detection.DetectionDetails.Cwe?.Select(CweCveLinkHelper.RenderCweCveLinkMarkdown);
        string cwe = string.Join(", ", renderedCwe ?? new string[] { });
        ShortSummary.MarkdownScrollViewer.Markdown = string.IsNullOrEmpty(cwe)
            ? StringHelper.Capitalize(detection.Severity)
            : $"{StringHelper.Capitalize(detection.Severity)} | {cwe}";

        File.Text = detection.DetectionDetails.FileName;
        Subcategory.Text = detection.DetectionDetails.Category;
        Language.Text = string.Join(", ", detection.DetectionDetails.Languages);

        Dictionary<string, string> engineIdToDisplayName = new() {
            { "5db84696-88dc-11ec-a8a3-0242ac120002", "Semgrep OSS (Orchestrated by Cycode)" },
            { "560a0abd-d7da-4e6d-a3f1-0ed74895295c", "Bearer (Powered by Cycode)" }
        };
        SecurityTool.Text =
            engineIdToDisplayName.TryGetValue(detection.DetectionDetails.ExternalScannerId,
                out string engineDisplayName)
                ? engineDisplayName
                : "Unknown";

        Rule.Text = detection.DetectionRuleId;
        Summary.Markdown = detection.DetectionDetails.Description ?? detection.GetFormattedMessage();

        if (string.IsNullOrEmpty(detection.DetectionDetails.CustomRemediationGuidelines)) {
            GridHelper.HideRow(Grid, _customRemediationGuidelinesRowIndex);
        } else {
            CompanyGuidelines.Markdown = detection.DetectionDetails.CustomRemediationGuidelines;
            GridHelper.ShowRow(Grid, _customRemediationGuidelinesRowIndex);
        }
    }
}