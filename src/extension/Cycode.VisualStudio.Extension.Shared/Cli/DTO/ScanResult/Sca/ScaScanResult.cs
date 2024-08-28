using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult.Sca;

public class ScaScanResult : ScanResultBase {
    [JsonProperty(Required = Required.Always)]
    public List<ScaDetection> Detections { get; set; } = [];

    public override List<DetectionBase> GetDetections() {
        return Detections.Cast<DetectionBase>().ToList();
    }
}