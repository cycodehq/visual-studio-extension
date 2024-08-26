using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cycode.VisualStudio.Extension.Shared.DTO;
using Cycode.VisualStudio.Extension.Shared.JsonContractResolvers;
using Newtonsoft.Json;

namespace Cycode.VisualStudio.Extension.Shared.Services;

public class GithubReleasesService(ILoggerService logger, IDownloadService downloadService) : IGitHubReleasesService {
    private readonly JsonSerializerSettings _jsonSerializerSettings = new() {
        ContractResolver = new SnakeCasePropertyNamesContractResolver()
    };

    public async Task<GitHubRelease> GetReleaseInfoByTagAsync(string owner, string repo, string tag) {
        string apiUrl = $"https://api.github.com/repos/{owner}/{repo}/releases/tags/{tag}";

        try {
            string response = await downloadService.RetrieveFileTextContentAsync(apiUrl);
            if (response == null) return null;

            GitHubRelease release = JsonConvert.DeserializeObject<GitHubRelease>(response, _jsonSerializerSettings);
            return release;
        } catch (Exception e) {
            logger.Error(e, "Failed to get release info");
            return null;
        }
    }

    public GitHubReleaseAsset FindAssetByFilename(List<GitHubReleaseAsset> assets, string filename) {
        return assets.FirstOrDefault(asset => asset.Name == filename);
    }
}