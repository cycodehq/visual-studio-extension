using Newtonsoft.Json;

namespace Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult.Sca;

public class ScaDetectionDetailsAlert {
    [JsonProperty(Required = Required.Always)]
    public string Severity { get; set; }

    [JsonProperty(Required = Required.Always)]
    public string Summary { get; set; }

    [JsonProperty(Required = Required.Always)]
    public string Description { get; set; }

    public string VulnerableRequirements { get; set; }
    public string FirstPatchedVersion { get; set; }
    public string CveIdentifier { get; set; }
}