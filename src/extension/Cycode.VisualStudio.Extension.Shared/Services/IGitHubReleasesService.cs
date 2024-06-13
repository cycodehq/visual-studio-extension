using System.Collections.Generic;
using System.Threading.Tasks;
using Cycode.VisualStudio.Extension.Shared.DTO;

namespace Cycode.VisualStudio.Extension.Shared.Services;

public interface IGitHubReleasesService {
    Task<GitHubRelease> GetReleaseInfoByTagAsync(string owner, string repo, string tag);
    GitHubReleaseAsset FindAssetByFilename(List<GitHubReleaseAsset> assets, string filename);
}