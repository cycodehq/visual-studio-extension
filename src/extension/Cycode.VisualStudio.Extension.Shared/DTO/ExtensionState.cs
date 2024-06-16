namespace Cycode.VisualStudio.Extension.Shared.DTO;

public class ExtensionState {
    public bool CliInstalled { get; set; } = false;
    public bool CliAuthed { get; set; } = false;
    public string CliVer { get; set; } = null;
    public string CliHash { get; set; } = null;
    public long? CliLastUpdateCheckedAt { get; set; } = null;
}