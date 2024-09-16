using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult.Iac;

public class IacScanResult : ScanResultBase {
    [JsonProperty(Required = Required.Always)]
    public List<IacDetection> Detections { get; set; } = [];

    public override List<DetectionBase> GetDetections() {
        return Detections.Cast<DetectionBase>().ToList();
    }
}