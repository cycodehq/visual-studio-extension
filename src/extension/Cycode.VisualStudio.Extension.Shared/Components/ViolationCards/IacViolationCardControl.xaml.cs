using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult.Iac;
using Cycode.VisualStudio.Extension.Shared.Helpers;
using Cycode.VisualStudio.Extension.Shared.Icons;
using Cycode.VisualStudio.Extension.Shared.Services;

namespace Cycode.VisualStudio.Extension.Shared.Components.ViolationCards;

public partial class IacViolationCardControl {
    private const int _customRemediationGuidelinesRowIndex = 7;
    private const int _cycodeRemediationGuidelinesRowIndex = 8;
    private const int _aiRemediationRowIndex = 9;

    private static readonly ICycodeService _cycodeService = ServiceLocator.GetService<ICycodeService>();
    private static readonly ITemporaryStateService _tempState = ServiceLocator.GetService<ITemporaryStateService>();
    
    private readonly IacDetection _detection;

    public IacViolationCardControl(IacDetection detection) {
        InitializeComponent();
        _detection = detection;

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

        GridHelper.HideRow(Grid, _aiRemediationRowIndex);
        GenerateAiRemediationButton.IsEnabled = _tempState.IsAiLargeLanguageModelEnabled;
    }

    private async void GenerateAiRemediationButton_OnClickAsync(object sender, RoutedEventArgs e) {
        await _cycodeService.GetAiRemediationAsync(_detection.Id, OnSuccess);
        return;

        void OnSuccess(AiRemediationResultData remediationResult) {
            AiRemediation.Markdown = remediationResult.Remediation;
            GridHelper.ShowRow(Grid, _aiRemediationRowIndex);

            GenerateAiRemediationButton.Visibility = Visibility.Collapsed;

            if (remediationResult.IsFixAvailable) {
                ApplyAiFixButton.Visibility = Visibility.Visible;
            }
        }
    }

    private async void ApplyAiFixButton_OnClickAsync(object sender, RoutedEventArgs e) {
        ApplyAiFixButton.IsEnabled = false;
        ApplyAiFixButton.Content = "Applying fix...";

        await Task.Delay(3000);

        ApplyAiFixButton.Content = "Fix Applied";
        // TODO: Implement actual file modification logic here
    }
}