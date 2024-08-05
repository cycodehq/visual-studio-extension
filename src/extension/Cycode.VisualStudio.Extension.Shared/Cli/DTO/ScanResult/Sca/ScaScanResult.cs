using System.Collections.Generic;

namespace Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult.Sca;

public class ScaScanResult : ScanResultBase {
    public List<ScaDetection> Detections { get; set; }
}