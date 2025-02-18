using System.Windows;
using System.Windows.Input;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult.Secret;
using Cycode.VisualStudio.Extension.Shared.Helpers;
using Cycode.VisualStudio.Extension.Shared.Icons;
using Cycode.VisualStudio.Extension.Shared.Services;

namespace Cycode.VisualStudio.Extension.Shared.Components.ViolationCards;

public partial class SecretViolationCardControl {
    private const int _customRemediationGuidelinesRowIndex = 7;
    private const int _cycodeRemediationGuidelinesRowIndex = 8;
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

    private async void IgnoreButton_OnClickAsync(object sender, RoutedEventArgs e) {
        await _cycodeService.ApplyDetectionIgnoreAsync(
            CliScanType.Secret, CliIgnoreType.Value, _detection.DetectionDetails.DetectedValue
        );
    }

    private void ScrollViewer_MouseWheel(object sender, MouseWheelEventArgs e) {
        // If the mouse is not over the control, we don't want to scroll it
        if (!IsMouseOver) return;

        // Raise the event on the parent control
        Scroll.RaiseEvent(new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta) {
            RoutedEvent = MouseWheelEvent
        });

        e.Handled = true;
    }
}