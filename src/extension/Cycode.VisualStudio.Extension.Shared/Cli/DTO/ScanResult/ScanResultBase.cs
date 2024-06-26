using System.Collections.Generic;

namespace Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult;

public class ScanResultBase {
    public List<DetectionBase> Detections { get; set; }
    public List<CliError> Errors { get; set; }
}