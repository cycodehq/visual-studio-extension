using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult.Sast;
using Cycode.VisualStudio.Extension.Shared.Helpers;
using Cycode.VisualStudio.Extension.Shared.Icons;
using Cycode.VisualStudio.Extension.Shared.Services;

namespace Cycode.VisualStudio.Extension.Shared.Components.ViolationCards;

public partial class SastViolationCardControl {
    private const int _customRemediationGuidelinesRowIndex = 9;
    private const int _cycodeRemediationGuidelinesRowIndex = 10;
    private const int _aiRemediationRowIndex = 11;

    private static readonly ICycodeService _cycodeService = ServiceLocator.GetService<ICycodeService>();
    private static readonly ITemporaryStateService _tempState = ServiceLocator.GetService<ITemporaryStateService>();

    private readonly SastDetection _detection;

    public SastViolationCardControl(SastDetection detection) {
        InitializeComponent();
        _detection = detection;

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