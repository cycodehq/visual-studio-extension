using Cycode.VisualStudio.Extension.Shared.Cli.DTO;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult;
using Microsoft.VisualStudio.Text.Tagging;

namespace Cycode.VisualStudio.Extension.Shared.Services.ErrorTagger;

public class DetectionTag(
    CliScanType detectionType,
    DetectionBase detection,
    string errorType,
    object toolTipContent
)
    : ErrorTag(errorType, toolTipContent) {
    public DetectionBase Detection { get; } = detection;
    public CliScanType DetectionType { get; } = detectionType;
}