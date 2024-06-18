using System.Collections.Generic;
using Newtonsoft.Json;

namespace Cycode.VisualStudio.Extension.Shared.DTO;

public class GitHubRelease {
    [JsonProperty(Required = Required.Always)]
    public string TagName { get; set; }

    [JsonProperty(Required = Required.Always)]
    public string Name { get; set; }

    [JsonProperty(Required = Required.Always)]
    public List<GitHubReleaseAsset> Assets { get; set; }
}