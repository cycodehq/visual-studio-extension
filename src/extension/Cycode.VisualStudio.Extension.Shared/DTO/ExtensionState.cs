using System.Collections.Generic;

namespace Cycode.VisualStudio.Extension.Shared.DTO;

[Serializable]
public class ExtensionState {
    public bool CliInstalled { get; set; } = false;
    public bool CliAuthed { get; set; } = false;
    public string CliVer { get; set; } = null;
    public string CliHash { get; set; } = null;
    public Dictionary<string, string> CliDirHashes { get; set; } = null;
    public long? CliLastUpdateCheckedAt { get; set; } = null;
}