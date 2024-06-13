using System.Collections.Generic;

namespace Cycode.VisualStudio.Extension.Shared.DTO;

public class GitHubRelease {
    public string TagName { get; set; }

    public string Name { get; set; }

    public List<GitHubReleaseAsset> Assets { get; set; }
}