using Newtonsoft.Json;

namespace Cycode.VisualStudio.Extension.Shared.Cli.DTO;

public class StatusResult {
    [JsonProperty(Required = Required.Always)]
    public string Program { get; set; }
    
    [JsonProperty(Required = Required.Always)]
    public string Version { get; set; }
    
    [JsonProperty(Required = Required.Always)]
    public bool IsAuthenticated { get; set; }
    
    [JsonProperty(Required = Required.AllowNull)]
    public string? UserId { get; set; }
    
    [JsonProperty(Required = Required.AllowNull)]
    public string? TenantId { get; set; }

    [JsonProperty(Required = Required.Always)]
    public SupportedModulesStatus SupportedModules { get; set; }
}

public class SupportedModulesStatus {
    // TODO(MarshalX): respect enabled/disabled scanning modules
    [JsonProperty(Required = Required.Always)]
    public bool SecretScanning { get; set; }
    [JsonProperty(Required = Required.Always)]
    public bool ScaScanning { get; set; }
    [JsonProperty(Required = Required.Always)]
    public bool IacScanning { get; set; }
    [JsonProperty(Required = Required.Always)]
    public bool SastScanning { get; set; }
    [JsonProperty(Required = Required.Always)]
    public bool AiLargeLanguageModel { get; set; }
}