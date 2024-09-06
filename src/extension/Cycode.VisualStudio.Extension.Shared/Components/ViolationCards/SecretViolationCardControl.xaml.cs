using Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult.Secret;
using Cycode.VisualStudio.Extension.Shared.Helpers;
using Cycode.VisualStudio.Extension.Shared.Icons;

namespace Cycode.VisualStudio.Extension.Shared.Components.ViolationCards;

public partial class SecretViolationCardControl {
    private const int _customRemediationGuidelinesRowIndex = 10;

    public SecretViolationCardControl(SecretDetection detection) {
        InitializeComponent();

        Header.Icon.Source = ExtensionIcons.GetCardSeverityBitmapSource(detection.Severity);
        Header.Title.Text = $"Hardcoded {detection.Type} is used";

        ShortSummary.Text = StringHelper.Capitalize(detection.Severity);
        File.Text = detection.DetectionDetails.FileName;
        Sha.Text = detection.DetectionDetails.Sha512;
        Rule.Text = detection.DetectionRuleId;
        Summary.Viewer.Markdown = detection.DetectionDetails.Description ?? detection.GetFormattedMessage();

        if (string.IsNullOrEmpty(detection.DetectionDetails.CustomRemediationGuidelines)) {
            GridHelper.HideRow(Grid, _customRemediationGuidelinesRowIndex);
        } else {
            string mdWithNewLines = detection.DetectionDetails.CustomRemediationGuidelines.Replace("<br/>", "\n");
            CompanyGuidelines.Viewer.Markdown = mdWithNewLines;
            GridHelper.ShowRow(Grid, _customRemediationGuidelinesRowIndex);
        }
    }
}