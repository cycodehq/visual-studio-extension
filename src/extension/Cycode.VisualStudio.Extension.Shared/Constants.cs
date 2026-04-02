using System.IO;

namespace Cycode.VisualStudio.Extension.Shared;

public static class Constants {
    public const string AppName = "visual_studio_extension";
    public const string RequiredCliVersion = "3.12.2";

    public const string CycodeDomain = "cycode.com";

    public const string CliGithubOrg = "cycodehq";
    public const string CliGithubRepo = "cycode-cli";
    public const string CliGithubTag = "v" + RequiredCliVersion;
    public const int CliCheckNewVersionEverySec = 24 * 60 * 60; // 24 hours
    public const string CliExecutableAssetName = "cycode-win.exe";
    public const string CliExecutableShaAssetName = $"{CliExecutableAssetName}.sha256";

    public const int PluginAutoSaveFlushInitialDelaySec = 0;
    public const int PluginAutoSaveFlushDelaySec = 5;

    public static readonly string PluginPath = GetPluginsPath();
    public static readonly string DefaultCliPath = GetDefaultCliPath();

    private static string GetPluginsPath() {
        string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appDataPath, "Cycode", "VisualStudioExtension");
    }

    private static string GetDefaultCliPath() {
        return Path.Combine(GetPluginsPath(), "cycode-cli.exe");
    }
}
