using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

namespace Cycode.VisualStudio.Extension.Shared;

internal static class OptionsProvider {
    [ComVisible(true)]
    public class GeneralOptions : BaseOptionPage<General> {
        protected override void OnApply(PageApplyEventArgs e) {
            General general = (General)AutomationObject;

            if (!Uri.TryCreate(general.CliApiUrl, UriKind.Absolute, out _)) {
                VS.MessageBox.ShowError("Invalid API URL. Please enter a valid URL.");
                return;
            }

            if (!Uri.TryCreate(general.CliAppUrl, UriKind.Absolute, out _)) {
                VS.MessageBox.ShowError("Invalid APP URL. Please enter a valid URL.");
                return;
            }

            if (!File.Exists(general.CliPath)) {
                VS.MessageBox.ShowError(
                    "The specified executable path does not exist. Please provide a valid path to the executable file."
                );
                return;
            }

            if (Path.GetExtension(general.CliPath) != ".exe") {
                VS.MessageBox.ShowError(
                    "The specified file is not an executable (.exe) file. Please provide a valid path to an executable file."
                );
                return;
            }

            base.OnApply(e);
        }
    }
}

public class General : BaseOptionModel<General> {
    private const string _cliCategory = "Cycode CLI settings";
    private const string _onPremiseCategory = "On-premise settings";
    private const string _ideCategory = "IDE settings";

    [Category(_cliCategory)]
    [DisplayName("Enable executable auto-management")]
    [DefaultValue(true)]
    public bool CliAutoManaged { get; set; } = true;

    [Category(_cliCategory)]
    [DisplayName("Path to executable")]
    [DefaultValue("")]
    public string CliPath { get; set; } = Constants.DefaultCliPath;

    [Category(_cliCategory)]
    [DisplayName("Additional parameters")]
    [DefaultValue("")]
    public string CliAdditionalParams { get; set; } = "";


    [Category(_onPremiseCategory)]
    [DisplayName("API URL")]
    [DefaultValue("https://api.cycode.com")]
    public string CliApiUrl { get; set; } = "https://api.cycode.com";

    [Category(_onPremiseCategory)]
    [DisplayName("APP URL")]
    [DefaultValue("https://app.cycode.com")]
    public string CliAppUrl { get; set; } = "https://app.cycode.com";


    [Category(_ideCategory)]
    [DisplayName("Enable Scan on Save")]
    [DefaultValue(true)]
    public bool ScanOnSave { get; set; } = true;
    
    public bool IsOnPremiseInstallation() {
        return !CliApiUrl.EndsWith(Constants.CycodeDomain);
    }
}