using System.Collections.Generic;
using Newtonsoft.Json;

namespace Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult;

public abstract class ScanResultBase {
    [JsonProperty(Required = Required.Always)]
    public List<CliError> Errors { get; set; }

    public abstract List<DetectionBase> GetDetections();
}