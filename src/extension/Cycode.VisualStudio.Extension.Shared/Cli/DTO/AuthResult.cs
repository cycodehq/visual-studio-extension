using Newtonsoft.Json;

namespace Cycode.VisualStudio.Extension.Shared.Cli.DTO;

public class AuthResult {
    [JsonProperty(Required = Required.Always)]
    public bool Result { get; set; }

    [JsonProperty(Required = Required.Always)]
    public string Message { get; set; }
}