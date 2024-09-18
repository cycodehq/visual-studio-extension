using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult.Sast;

public class SastScanResult : ScanResultBase {
    [JsonProperty(Required = Required.Always)]
    public List<SastDetection> Detections { get; set; } = [];

    public override List<DetectionBase> GetDetections() {
        return Detections.Cast<DetectionBase>().ToList();
    }
}