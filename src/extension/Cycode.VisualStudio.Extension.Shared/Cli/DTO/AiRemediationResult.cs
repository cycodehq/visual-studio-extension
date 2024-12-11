using Newtonsoft.Json;

namespace Cycode.VisualStudio.Extension.Shared.Cli.DTO;

public class AiRemediationResult {
    [JsonProperty(Required = Required.Always)]
    public bool Result { get; set; }

    [JsonProperty(Required = Required.Always)]
    public string? Message { get; set; }

    [JsonProperty(Required = Required.AllowNull)]
    public AiRemediationResultData? Data { get; set; }
}

public class AiRemediationResultData {
    [JsonProperty(Required = Required.Always)]
    public string Remediation { get; set; }

    [JsonProperty(Required = Required.Always)]
    public bool IsFixAvailable { get; set; }
}