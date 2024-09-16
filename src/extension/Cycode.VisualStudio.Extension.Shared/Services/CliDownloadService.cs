using System.IO;
using System.Threading.Tasks;
using Cycode.VisualStudio.Extension.Shared.DTO;
using Cycode.VisualStudio.Extension.Shared.Helpers;
using Cycode.VisualStudio.Extension.Shared.Options;

namespace Cycode.VisualStudio.Extension.Shared.Services;

public interface ICliDownloadService {
    Task<bool> InitCliAsync();
}

public class CliDownloadService(
    ILoggerService logger,
    IStateService stateService,
    IDownloadService downloadService,
    IGitHubReleasesService githubReleaseService
) : ICliDownloadService {
    private readonly ExtensionState _pluginState = stateService.Load();
    private GitHubRelease _githubReleaseInfo;

    public async Task<bool> InitCliAsync() {
        General general = await General.GetLiveInstanceAsync();

        if (
            general.CliPath != Constants.DefaultCliPath ||
            !general.CliAutoManaged ||
            !await ShouldDownloadCliAsync()
        ) {
            logger.Info("CLI path is not overriden or executable is not auto managed or no need to download.");
            return true;
        }

        // if the CLI path is not overriden and executable is auto managed, and need to download - download it.
        FileInfo fileInfo = await DownloadCliAsync();
        if (fileInfo == null) {
            logger.Warn("Failed to download/update CLI");
            return false;
        }

        logger.Info("CLI was successfully downloaded/updated. Path: {0}", fileInfo.FullName);
        return true;
    }

    private async Task<GitHubRelease> GetGitHubSupportedReleaseAsync(bool forceRefresh = false) {
        // prevent sending many requests
        if (_githubReleaseInfo != null && !forceRefresh) return _githubReleaseInfo;

        _githubReleaseInfo = await githubReleaseService.GetReleaseInfoByTagAsync(
            Constants.CliGithubOrg,
            Constants.CliGithubRepo,
            Constants.CliGithubTag
        );
        return _githubReleaseInfo;
    }

    private async Task<bool> ShouldDownloadNewRemoteCliAsync(string localPath) {
        if (_pluginState.CliVer != Constants.RequiredCliVersion) {
            logger.Warn("Should download CLI because version mismatch");
            return true;
        }

        long timeNow = DateTimeOffset.Now.ToUnixTimeMilliseconds();

        if (_pluginState.CliLastUpdateCheckedAt == null) {
            _pluginState.CliLastUpdateCheckedAt = timeNow;
            stateService.Save();
            logger.Warn("Should not download CLI because cliLastUpdateCheckedAt is Null. First plugin run.");
            return false;
        }

        long diffInSec = (timeNow - _pluginState.CliLastUpdateCheckedAt.Value) / 1000;
        if (diffInSec < Constants.CliCheckNewVersionEverySec) {
            logger.Warn(
                "Should not check remote CLI version because diffInSec is {0} (less than {1})",
                diffInSec, Constants.CliCheckNewVersionEverySec
            );
            return false;
        }

        _pluginState.CliLastUpdateCheckedAt = timeNow;
        stateService.Save();

        string remoteChecksum = await GetRemoteChecksumFileAsync(true);
        if (remoteChecksum == null) {
            logger.Warn("Should not download new CLI because can't get remoteChecksum. Maybe no internet connection.");
            return false;
        }

        if (await HashHelper.VerifyFileChecksumAsync(localPath, remoteChecksum)) return false;

        logger.Warn("Should download CLI because checksum doesn't match remote checksum");
        return true;
    }

    private async Task<bool> ShouldDownloadCliAsync() {
        if (_pluginState.CliHash == null) {
            logger.Warn("Should download CLI because cliHash is Null");
            return true;
        }

        if (!await HashHelper.VerifyFileChecksumAsync(Constants.DefaultCliPath, _pluginState.CliHash)) {
            logger.Warn("Should download CLI because checksum is invalid");
            return true;
        }

        if (await ShouldDownloadNewRemoteCliAsync(Constants.DefaultCliPath)) return true;

        logger.Warn("CLI is downloaded and the checksum is valid.");
        return false;
    }

    private async Task<string> GetRemoteChecksumFileAsync(bool forceRefresh = false) {
        GitHubRelease releaseInfo = await GetGitHubSupportedReleaseAsync(forceRefresh);
        if (releaseInfo == null) {
            logger.Warn("Failed to get latest release info");
            return null;
        }

        GitHubReleaseAsset executableHashAsset = githubReleaseService.FindAssetByFilename(
            releaseInfo.Assets, Constants.CliExecutableShaAssetName
        );
        if (executableHashAsset != null)
            return await downloadService.RetrieveFileTextContentAsync(executableHashAsset.BrowserDownloadUrl);

        logger.Warn("Failed to find executableHashAsset");
        return null;
    }

    private async Task<GitHubReleaseAsset> GetExecutableAssetAsync() {
        GitHubRelease releaseInfo = await GetGitHubSupportedReleaseAsync();
        if (releaseInfo == null) {
            logger.Warn("Failed to get latest release info");
            return null;
        }

        GitHubReleaseAsset executableAsset = githubReleaseService.FindAssetByFilename(
            releaseInfo.Assets, Constants.CliExecutableAssetName
        );
        if (executableAsset != null) return executableAsset;

        logger.Warn("Failed to find executableAsset");
        return null;
    }

    private async Task<ChecksumAndAsset> GetAssetAndFileChecksumAsync() {
        GitHubReleaseAsset executableAsset = await GetExecutableAssetAsync();
        if (executableAsset == null) {
            logger.Warn("Failed to get executableAsset");
            return null;
        }

        string expectedFileChecksum = await GetRemoteChecksumFileAsync();
        if (expectedFileChecksum != null) return new ChecksumAndAsset(executableAsset, expectedFileChecksum);

        logger.Warn("Failed to get expectedFileChecksum");
        return null;
    }

    private async Task<FileInfo> DownloadCliAsync() {
        ChecksumAndAsset assetAndFileChecksum = await GetAssetAndFileChecksumAsync();
        if (assetAndFileChecksum == null) {
            logger.Warn("Failed to get assetAndFileChecksum");
            return null;
        }

        FileInfo downloadedFile = await downloadService.DownloadFileAsync(
            assetAndFileChecksum.Asset.BrowserDownloadUrl,
            assetAndFileChecksum.Checksum,
            Constants.DefaultCliPath
        );

        _pluginState.CliHash = assetAndFileChecksum.Checksum;
        stateService.Save();

        return downloadedFile;
    }

    private class ChecksumAndAsset(GitHubReleaseAsset asset, string checksum) {
        public string Checksum { get; } = checksum;
        public GitHubReleaseAsset Asset { get; } = asset;
    }
}