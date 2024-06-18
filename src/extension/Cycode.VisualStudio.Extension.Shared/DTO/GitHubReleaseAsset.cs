using Newtonsoft.Json;

namespace Cycode.VisualStudio.Extension.Shared.DTO;

public class GitHubReleaseAsset {
    [JsonProperty(Required = Required.Always)]
    public string Name { get; set; }

    [JsonProperty(Required = Required.Always)]
    public string BrowserDownloadUrl { get; set; }
}