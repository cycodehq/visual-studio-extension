using Newtonsoft.Json;

namespace Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult.Sca;

public class ScaDetectionDetails : DetectionDetailsBase {
    [JsonProperty(Required = Required.Always)]
    public string FileName { get; set; }

    [JsonProperty(Required = Required.Always)]
    public int StartPosition { get; set; }

    [JsonProperty(Required = Required.Always)]
    public int EndPosition { get; set; }

    [JsonProperty(Required = Required.Always)]
    public int Line { get; set; }

    [JsonProperty(Required = Required.Always)]
    public int LineInFile { get; set; }

    [JsonProperty(Required = Required.Always)]
    public string DependencyPaths { get; set; }

    [JsonProperty(Required = Required.Always)]
    public string PackageName { get; set; }

    [JsonProperty(Required = Required.Always)]
    public string PackageVersion { get; set; }

    public string License { get; set; }
    public string VulnerabilityDescription { get; set; }
    public string VulnerabilityId { get; set; }
    public ScaDetectionDetailsAlert Alert { get; set; }
    public string RemediationGuidelines { get; set; }
    public string CustomRemediationGuidelines { get; set; }

    public override string GetFilePath() {
        return FileName;
    }

    public override int GetLineNumber() {
        return LineInFile;
    }
}