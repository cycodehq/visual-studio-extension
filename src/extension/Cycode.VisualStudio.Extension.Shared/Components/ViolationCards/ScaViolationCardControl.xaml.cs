using Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult.Sca;
using Cycode.VisualStudio.Extension.Shared.Helpers;
using Cycode.VisualStudio.Extension.Shared.Icons;

namespace Cycode.VisualStudio.Extension.Shared.Components.ViolationCards;

public partial class ScaViolationCardControl {
    private const int _firstPatchedVersionRowIndex = 5;
    private const int _dependencyPathRowIndex = 6;
    private const int _licenseRowIndex = 7;
    private const int _summaryRowIndex = 8;

    public ScaViolationCardControl(ScaDetection detection) {
        InitializeComponent();

        Header.Icon.Source = ExtensionIcons.GetCardSeverityBitmapSource(detection.Severity);
        Header.Title.Text = detection.DetectionDetails.Alert?.Summary ?? detection.Message;

        ShortSummary.MarkdownScrollViewer.Markdown = $"{StringHelper.Capitalize(detection.Severity)}";

        Package.Text = detection.DetectionDetails.PackageName;
        Version.Text = detection.DetectionDetails.PackageVersion;

        if (string.IsNullOrEmpty(detection.DetectionDetails.DependencyPaths)) {
            GridHelper.HideRow(Grid, _dependencyPathRowIndex);
        } else {
            GridHelper.ShowRow(Grid, _dependencyPathRowIndex);
            DependencyPath.Text = detection.DetectionDetails.DependencyPaths;
        }

        if (detection.DetectionDetails.Alert != null) {
            string renderedCwe = CweCveLinkHelper.RenderCweCveLinkMarkdown(detection.DetectionDetails.VulnerabilityId);
            ShortSummary.MarkdownScrollViewer.Markdown =
                $"{StringHelper.Capitalize(detection.Severity)} | {renderedCwe}";

            FirstPatchedVersion.Text = detection.DetectionDetails.Alert.FirstPatchedVersion ?? "Not fixed";
            GridHelper.ShowRow(Grid, _firstPatchedVersionRowIndex);

            GridHelper.HideRow(Grid, _licenseRowIndex);

            GridHelper.ShowRow(Grid, _summaryRowIndex);
            Summary.Markdown = detection.DetectionDetails.Alert.Description ?? string.Empty;
        } else {
            GridHelper.HideRow(Grid, _firstPatchedVersionRowIndex);

            GridHelper.ShowRow(Grid, _licenseRowIndex);
            License.Text = detection.DetectionDetails.License ?? "Unknown";

            GridHelper.HideRow(Grid, _summaryRowIndex);
        }
    }
}