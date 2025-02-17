namespace Cycode.VisualStudio.Extension.Shared.DTO;

public class ExtensionState {
    public string CliVer { get; set; }
    public string CliHash { get; set; }
    public long? CliLastUpdateCheckedAt { get; set; }
}