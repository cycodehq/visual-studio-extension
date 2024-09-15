using System.Windows;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult.Secret;
using Cycode.VisualStudio.Extension.Shared.Helpers;
using Cycode.VisualStudio.Extension.Shared.Icons;
using Cycode.VisualStudio.Extension.Shared.Services;

namespace Cycode.VisualStudio.Extension.Shared.Components.ViolationCards;

public partial class SecretViolationCardControl {
    private const int _customRemediationGuidelinesHrRowIndex = 9;
    private const int _customRemediationGuidelinesTitleRowIndex = 10;
    private const int _customRemediationGuidelinesMarkdownRowIndex = 11;
    private readonly ICycodeService _cycodeService = ServiceLocator.GetService<ICycodeService>();

    private readonly SecretDetection _detection;

    public SecretViolationCardControl(SecretDetection detection) {
        InitializeComponent();
        _detection = detection;

        Header.Icon.Source = ExtensionIcons.GetCardSeverityBitmapSource(detection.Severity);
        Header.Title.Text = $"Hardcoded {detection.Type} is used";

        ShortSummary.Text = StringHelper.Capitalize(detection.Severity);
        File.Text = detection.DetectionDetails.FileName;
        Sha.Text = detection.DetectionDetails.Sha512;
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

    private async void IgnoreButton_OnClickAsync(object sender, RoutedEventArgs e) {
        await _cycodeService.ApplyDetectionIgnoreAsync(
            CliScanType.Secret, CliIgnoreType.Value, _detection.DetectionDetails.DetectedValue
        );
    }
}