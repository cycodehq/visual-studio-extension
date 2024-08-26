using Newtonsoft.Json;

namespace Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult.Sca;

public class ScaDetection : DetectionBase {
    [JsonProperty(Required = Required.Always)]
    public string Message { get; set; }

    [JsonProperty(Required = Required.Always)]
    public ScaDetectionDetails DetectionDetails { get; set; }

    [JsonProperty(Required = Required.Always)]
    public string Type { get; set; }

    [JsonProperty(Required = Required.Always)]
    public string DetectionRuleId { get; set; }

    [JsonProperty(Required = Required.Always)]
    public string DetectionTypeId { get; set; }

    public override DetectionDetailsBase GetDetectionDetails() {
        return DetectionDetails;
    }

    public override string GetFormattedMessage() {
        return Message;
    }

    public override string GetFormattedTitle() {
        string message = DetectionDetails.VulnerabilityDescription ?? GetFormattedMessage();
        return $"{DetectionDetails.PackageName}@{DetectionDetails.PackageVersion} - {message}";
    }

    public override string GetFormattedNodeTitle() {
        return $"line {DetectionDetails.LineInFile}: {GetFormattedTitle()}";
    }
}