namespace Cycode.VisualStudio.Extension.Shared.Cli;

// TODO(MarshalX): remove it from here and implement real plugin settings
public class PluginSettings {
    public string CliPath { get; set; } =
        "C:\\Users\\Shadow\\AppData\\Roaming\\Cycode\\VisualStudioExtension\\cycode-cli.exe";

    public bool CliAutoManaged { get; set; } = true;
    public string CliApiUrl { get; set; } = "https://api.cycode.com";
    public string CliAppUrl { get; set; } = "https://app.cycode.com";
    public string CliAdditionalParams { get; set; } = "";
}