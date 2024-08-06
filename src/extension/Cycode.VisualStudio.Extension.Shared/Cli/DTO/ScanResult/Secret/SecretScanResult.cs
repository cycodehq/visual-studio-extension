using System.Collections.Generic;
using Newtonsoft.Json;

namespace Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult.Secret;

public class SecretScanResult : ScanResultBase {
    [JsonProperty(Required = Required.Always)]
    public List<SecretDetection> Detections { get; set; } = [];
}