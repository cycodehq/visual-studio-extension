using Newtonsoft.Json;

namespace Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult.Sast;

public class SastDetection : DetectionBase {
    [JsonProperty(Required = Required.Always)]
    public string Message { get; set; }

    [JsonProperty(Required = Required.Always)]
    public SastDetectionDetails DetectionDetails { get; set; }

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
        return DetectionDetails.PolicyDisplayName;
    }

    public override string GetFormattedTitle() {
        return GetFormattedMessage();
    }

    public override string GetFormattedNodeTitle() {
        return $"line {DetectionDetails.GetLineNumber()}: {GetFormattedMessage()}";
    }
}