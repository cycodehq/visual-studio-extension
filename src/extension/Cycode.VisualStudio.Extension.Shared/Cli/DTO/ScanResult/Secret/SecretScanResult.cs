using System.Collections.Generic;

namespace Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult.Secret;

public class SecretScanResult: ScanResultBase {
    public new List<SecretDetection> Detections { get; set; }
    public new List<CliError> Errors { get; set; }
}