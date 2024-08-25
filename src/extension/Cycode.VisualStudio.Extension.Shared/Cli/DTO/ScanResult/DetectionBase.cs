namespace Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult;

public abstract class DetectionBase {
    public string Severity { get; set; }
    public DetectionDetailsBase DetectionDetails { get; set; }

    public abstract string GetFormattedMessage();
    public abstract string GetFormattedTitle();
    public abstract string GetFormattedNodeTitle();
}