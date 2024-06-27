namespace Cycode.VisualStudio.Extension.Shared;

/// <summary>
///     Helper class that exposes all GUIDs used across VS Package.
/// </summary>
internal sealed class PackageGuids {
    public const string CycodeString = "a330ace2-2fd0-4e01-ad7b-c04cbd33defb";
    public static Guid Cycode = new(CycodeString);
}

/// <summary>
///     Helper class that encapsulates all CommandIDs uses across VS Package.
/// </summary>
internal sealed class PackageIds {
    public const int ViewOpenToolWindowCommand = 0x1001;

    public const int TWindowToolbar = 0x1050;
    public const int ToolbarOpenSettingsCommand = 0x1052;

    public const int TopMenuCycodeCommand = 0x1103;
    public const int TopMenuOpenSettingsCommand = 0x1104;
}