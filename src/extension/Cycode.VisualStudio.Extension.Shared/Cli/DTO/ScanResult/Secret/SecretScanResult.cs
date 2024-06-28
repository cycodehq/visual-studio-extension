using System.Collections.Generic;

namespace Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult.Secret;

public class SecretScanResult: ScanResultBase {
    public List<SecretDetection> Detections { get; set; }
}