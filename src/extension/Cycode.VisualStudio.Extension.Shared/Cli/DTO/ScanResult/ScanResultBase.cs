using System.Collections.Generic;
using Newtonsoft.Json;

namespace Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult;

public class ScanResultBase {
    [JsonProperty(Required = Required.Always)]
    public List<DetectionBase> Detections { get; set; } = [];

    [JsonProperty(Required = Required.Always)]
    public List<CliError> Errors { get; set; } = [];
}