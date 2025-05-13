using System.Collections.Generic;
using Newtonsoft.Json;

namespace Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult.Sast;

public class SastDetectionDetails : DetectionDetailsBase {
    [JsonProperty(Required = Required.Always)]
    public string ExternalScannerId { get; set; }

    [JsonProperty(Required = Required.Always)]
    public int LineInFile { get; set; }

    [JsonProperty(Required = Required.Always)]
    public int StartPosition { get; set; }

    [JsonProperty(Required = Required.Always)]
    public int EndPosition { get; set; }

    [JsonProperty(Required = Required.Always)]
    public string FilePath { get; set; }

    [JsonProperty(Required = Required.Always)]
    public string FileName { get; set; }

    [JsonProperty(Required = Required.Always)]
    public List<string> Cwe { get; set; }

    [JsonProperty(Required = Required.Always)]
    public List<string> Owasp { get; set; }

    [JsonProperty(Required = Required.Always)]
    public string Category { get; set; }

    [JsonProperty(Required = Required.Always)]
    public List<string> Languages { get; set; }

    [JsonProperty(Required = Required.Always)]
    public string Description { get; set; }

    [JsonProperty(Required = Required.Always)]
    public string PolicyDisplayName { get; set; }

    public string RemediationGuidelines { get; set; }
    public string CustomRemediationGuidelines { get; set; }

    public override string GetFilePath() {
        return FilePath;
    }
    
    public override int GetLineNumber() {
        return LineInFile;
    }
}