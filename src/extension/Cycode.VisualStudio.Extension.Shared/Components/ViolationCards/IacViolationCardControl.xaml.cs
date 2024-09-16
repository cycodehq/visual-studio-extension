using System.IO;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult.Iac;
using Cycode.VisualStudio.Extension.Shared.Helpers;
using Cycode.VisualStudio.Extension.Shared.Icons;

namespace Cycode.VisualStudio.Extension.Shared.Components.ViolationCards;

public partial class IacViolationCardControl {
    private const int _customRemediationGuidelinesHrRowIndex = 9;
    private const int _customRemediationGuidelinesTitleRowIndex = 10;
    private const int _customRemediationGuidelinesMarkdownRowIndex = 11;

    private const int _cycodeRemediationGuidelinesHrRowIndex = 12;
    private const int _cycodeRemediationGuidelinesTitleRowIndex = 13;
    private const int _cycodeRemediationGuidelinesMarkdownRowIndex = 14;

    public IacViolationCardControl(IacDetection detection) {
        InitializeComponent();

        Header.Icon.Source = ExtensionIcons.GetCardSeverityBitmapSource(detection.Severity);
        Header.Title.Text = detection.GetFormattedMessage();

        ShortSummary.Text = StringHelper.Capitalize(detection.Severity);
        File.Text = Path.GetFileName(detection.DetectionDetails.FileName);
        Provider.Text = detection.DetectionDetails.InfraProvider;
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

        if (string.IsNullOrEmpty(detection.DetectionDetails.RemediationGuidelines)) {
            GridHelper.HideRow(Grid, _cycodeRemediationGuidelinesHrRowIndex);
            GridHelper.HideRow(Grid, _cycodeRemediationGuidelinesTitleRowIndex);
            GridHelper.HideRow(Grid, _cycodeRemediationGuidelinesMarkdownRowIndex);
        } else {
            string mdWithNewLines = detection.DetectionDetails.RemediationGuidelines.Replace("<br/>", "\n");
            CycodeGuidelines.Viewer.Markdown = mdWithNewLines;
            GridHelper.ShowRow(Grid, _cycodeRemediationGuidelinesHrRowIndex);
            GridHelper.ShowRow(Grid, _cycodeRemediationGuidelinesTitleRowIndex);
            GridHelper.ShowRow(Grid, _cycodeRemediationGuidelinesMarkdownRowIndex);
        }
    }
}