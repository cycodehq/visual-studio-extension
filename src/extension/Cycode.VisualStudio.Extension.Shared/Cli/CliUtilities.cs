using Cycode.VisualStudio.Extension.Shared.Cli.DTO;

namespace Cycode.VisualStudio.Extension.Shared.Cli;


public static class CliUtilities {
    public static string GetScanTypeDisplayName(CliScanType scanType) {
        return scanType switch {
            CliScanType.Secret => "Hardcoded Secrets",
            CliScanType.Sca => "Open Source Threat",
            CliScanType.Sast => "Code Security",
            CliScanType.Iac => "Infrastructure As Code",
            _ => "Unknown"
        };
    }
}