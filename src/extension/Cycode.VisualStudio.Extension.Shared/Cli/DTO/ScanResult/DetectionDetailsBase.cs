namespace Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult;

public abstract class DetectionDetailsBase {
    public abstract string GetFilePath();
    // This method returns a 1-indexed line number
    public abstract int GetLineNumber();
}