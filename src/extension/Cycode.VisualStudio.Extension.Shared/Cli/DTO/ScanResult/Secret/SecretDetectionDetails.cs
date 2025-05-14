using Newtonsoft.Json;

namespace Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult.Secret;

public class SecretDetectionDetails : DetectionDetailsBase {
    [JsonProperty(Required = Required.Always)]
    public string Sha512 { get; set; }

    [JsonProperty(Required = Required.Always)]
    public string Provider { get; set; }

    [JsonProperty(Required = Required.Always)]
    public string ConcreteProvider { get; set; }

    [JsonProperty(Required = Required.Always)]
    public int Length { get; set; }

    [JsonProperty(Required = Required.Always)]
    public int StartPosition { get; set; }

    [JsonProperty(Required = Required.Always)]
    public int Line { get; set; }

    [JsonProperty(Required = Required.Always)]
    public string CommittedAt { get; set; } // TODO(MarshalX): actually DateTime. annotate?

    [JsonProperty(Required = Required.Always)]
    public string FilePath { get; set; }

    [JsonProperty(Required = Required.Always)]
    public string FileName { get; set; }

    public string FileExtension { get; set; }
    public string Description { get; set; }
    public string RemediationGuidelines { get; set; }
    public string CustomRemediationGuidelines { get; set; }
    public string PolicyDisplayName { get; set; }
    public string DetectedValue { get; set; } // custom field out of CLI JSON schema

    public override string GetFilePath() {
        return $"{FilePath}{FileName}";
    }

    public override int GetLineNumber() {
        return Line + 1; // 1-indexed
    }
}