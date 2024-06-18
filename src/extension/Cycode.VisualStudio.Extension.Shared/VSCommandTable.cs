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
    public const int CycodeCommand = 0x0100;
    public const int OpenSettingsCommand = 0x0111;
    public const int TWindowToolbar = 0x1000;
    public const int TWindowToolbarGroup = 0x1050;
}