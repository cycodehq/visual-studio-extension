using System.IO;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult.Iac;
using Cycode.VisualStudio.Extension.Shared.Helpers;
using Cycode.VisualStudio.Extension.Shared.Icons;

namespace Cycode.VisualStudio.Extension.Shared.Components.ViolationCards;

public partial class IacViolationCardControl {
    private const int _customRemediationGuidelinesRowIndex = 7;
    private const int _cycodeRemediationGuidelinesRowIndex = 8;

    public IacViolationCardControl(IacDetection detection) {
        InitializeComponent();

        Header.Icon.Source = ExtensionIcons.GetCardSeverityBitmapSource(detection.Severity);
        Header.Title.Text = detection.GetFormattedMessage();

        ShortSummary.Text = StringHelper.Capitalize(detection.Severity);
        File.Text = Path.GetFileName(detection.DetectionDetails.FileName);
        Provider.Text = detection.DetectionDetails.InfraProvider;
        Rule.Text = detection.DetectionRuleId;
        Summary.Markdown = detection.DetectionDetails.Description ?? detection.GetFormattedMessage();

        if (string.IsNullOrEmpty(detection.DetectionDetails.CustomRemediationGuidelines)) {
            GridHelper.HideRow(Grid, _customRemediationGuidelinesRowIndex);
        } else {
            CompanyGuidelines.Markdown = detection.DetectionDetails.CustomRemediationGuidelines;
            GridHelper.ShowRow(Grid, _customRemediationGuidelinesRowIndex);
        }

        if (string.IsNullOrEmpty(detection.DetectionDetails.RemediationGuidelines)) {
            GridHelper.HideRow(Grid, _cycodeRemediationGuidelinesRowIndex);
        } else {
            CycodeGuidelines.Markdown = detection.DetectionDetails.RemediationGuidelines;
            GridHelper.ShowRow(Grid, _cycodeRemediationGuidelinesRowIndex);
        }
    }
}