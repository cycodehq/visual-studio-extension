using Newtonsoft.Json;

namespace Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult.Secret;

public class SecretDetection : DetectionBase {
    [JsonProperty(Required = Required.Always)]
    public string Message { get; set; }

    [JsonProperty(Required = Required.Always)]
    public SecretDetectionDetails DetectionDetails { get; set; }

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
        return Message.Replace("within '' repository", ""); // BE bug
    }

    public override string GetFormattedTitle() {
        return $"{Type}. {GetFormattedMessage()}";
    }

    public override string GetFormattedNodeTitle() {
        return $"line {DetectionDetails.GetLineNumber()}: a hardcoded {Type} is used";
    }
}