using Newtonsoft.Json;

namespace Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult.Iac;

public class IacDetectionDetails : DetectionDetailsBase {
    [JsonProperty(Required = Required.Always)]
    public string Info { get; set; }

    [JsonProperty(Required = Required.Always)]
    public string FailureType { get; set; }

    [JsonProperty(Required = Required.Always)]
    public string InfraProvider { get; set; }

    [JsonProperty(Required = Required.Always)]
    public int LineInFile { get; set; }

    [JsonProperty(Required = Required.Always)]
    public int StartPosition { get; set; }

    [JsonProperty(Required = Required.Always)]
    public int EndPosition { get; set; }

    [JsonProperty(Required = Required.Always)]
    public string FilePath { get; set; }

    [JsonProperty(Required = Required.Always)]
    public string FileName { get; set; }

    public string Description { get; set; }
    public string RemediationGuidelines { get; set; }
    public string CustomRemediationGuidelines { get; set; }
    public string PolicyDisplayName { get; set; }

    public override string GetFilePath() {
        return FileName;
    }
}