using System.IO;

namespace Cycode.VisualStudio.Extension.Shared;

public static class Constants {
    public const string AppName = "visual_studio_extension";

    public static readonly string PluginPath = GetPluginsPath();
    public static readonly string DefaultCliPath = GetDefaultCliPath();
    public const string RequiredCliVersion = "1.10.7";

    public const string CycodeDomain = "cycode.com";

    public const string CliGithubOrg = "cycodehq";
    public const string CliGithubRepo = "cycode-cli";
    public const string CliGithubTag = "v" + RequiredCliVersion;
    public const int CliCheckNewVersionEverySec = 24 * 60 * 60; // 24 hours
    public const string CliExecutableAssetName = "cycode-win.exe";
    public const string CliExecutableShaAssetName = $"{CliExecutableAssetName}.sha256";

    public const int PluginAutoSaveFlushInitialDelaySec = 0;
    public const int PluginAutoSaveFlushDelaySec = 5;

    public const string SentryDsn =
        "https://091cdc01001e4600a30ac02f1b82c4c5@o1026942.ingest.us.sentry.io/4507543901700096";
    public const bool SentryDebug = false;
    public const float SentrySampleRate = 1.0f;
    public const bool SentrySendDefaultPii = false;
    public const bool SentryAutoSessionTracking = true;

    private static string GetPluginsPath() {
        string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appDataPath, "Cycode", "VisualStudioExtension");
    }

    private static string GetDefaultCliPath() {
        return Path.Combine(GetPluginsPath(), "cycode-cli.exe");
    }
}