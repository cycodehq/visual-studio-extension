using System.Collections.Generic;
using System.Linq;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult.Sast;
using Cycode.VisualStudio.Extension.Shared.Helpers;
using Cycode.VisualStudio.Extension.Shared.Icons;

namespace Cycode.VisualStudio.Extension.Shared.Components.ViolationCards;

public partial class SastViolationCardControl {
    private const int _customRemediationGuidelinesHrRowIndex = 11;
    private const int _customRemediationGuidelinesTitleRowIndex = 12;
    private const int _customRemediationGuidelinesMarkdownRowIndex = 13;

    public SastViolationCardControl(SastDetection detection) {
        InitializeComponent();

        Header.Icon.Source = ExtensionIcons.GetCardSeverityBitmapSource(detection.Severity);
        Header.Title.Text = detection.GetFormattedMessage();

        IEnumerable<string> renderedCwe =
            detection.DetectionDetails.Cwe?.Select(CweCveLinkHelper.RenderCweCveLinkMarkdown);
        string cwe = string.Join(", ", renderedCwe ?? new string[] { });
        ShortSummary.Viewer.Markdown = string.IsNullOrEmpty(cwe)
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
        Summary.Viewer.Markdown = detection.DetectionDetails.Description ?? detection.GetFormattedMessage();

        if (string.IsNullOrEmpty(detection.DetectionDetails.CustomRemediationGuidelines)) {
            GridHelper.HideRow(Grid, _customRemediationGuidelinesHrRowIndex);
            GridHelper.HideRow(Grid, _customRemediationGuidelinesTitleRowIndex);
            GridHelper.HideRow(Grid, _customRemediationGuidelinesMarkdownRowIndex);
        } else {
            string mdWithNewLines = detection.DetectionDetails.CustomRemediationGuidelines.Replace("<br/>", "\n");
            CompanyGuidelines.Viewer.Markdown = mdWithNewLines;
            GridHelper.ShowRow(Grid, _customRemediationGuidelinesHrRowIndex);
            GridHelper.ShowRow(Grid, _customRemediationGuidelinesTitleRowIndex);
            GridHelper.ShowRow(Grid, _customRemediationGuidelinesMarkdownRowIndex);
        }
    }
}