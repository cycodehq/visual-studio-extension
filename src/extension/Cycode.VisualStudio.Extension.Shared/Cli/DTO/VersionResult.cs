using Newtonsoft.Json;

namespace Cycode.VisualStudio.Extension.Shared.Cli.DTO;

public class VersionResult {
    [JsonProperty(Required = Required.Always)]
    public string Name { get; set; }

    [JsonProperty(Required = Required.Always)]
    public string Version { get; set; }
}