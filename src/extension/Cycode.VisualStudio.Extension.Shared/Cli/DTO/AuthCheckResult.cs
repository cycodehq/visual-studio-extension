#nullable enable

using Newtonsoft.Json;

namespace Cycode.VisualStudio.Extension.Shared.Cli.DTO;

public class AuthCheckResult {
    [JsonProperty(Required = Required.Always)]
    public bool Result { get; set; }

    [JsonProperty(Required = Required.Always)]
    public string? Message { get; set; }

    [JsonProperty(Required = Required.AllowNull)]
    public AuthCheckResultData? Data { get; set; }
}

public class AuthCheckResultData {
    [JsonProperty(Required = Required.Always)]
    public string? UserId { get; set; }

    [JsonProperty(Required = Required.Always)]
    public string? TenantId { get; set; }
}