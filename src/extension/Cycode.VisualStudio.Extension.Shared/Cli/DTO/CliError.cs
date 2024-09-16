using Newtonsoft.Json;

namespace Cycode.VisualStudio.Extension.Shared.Cli.DTO;

public class CliError {
    [JsonProperty(Required = Required.Always)]
    public string Message { get; set; }

    public string Code { get; set; } = "Unknown";
    public string Error { get; set; } = "Unknown";

    public bool SoftFail { get; set; } = false;
}