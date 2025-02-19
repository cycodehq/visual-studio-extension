using Cycode.VisualStudio.Extension.Shared.Cli.DTO;

namespace Cycode.VisualStudio.Extension.Shared.Services;

public interface ITemporaryStateService {
    bool CliInstalled { get; set; }
    bool CliAuthed { get; set; }

    StatusResult CliStatus { get; set; }

    bool IsSecretScanningEnabled { get; }
    bool IsScaScanningEnabled { get; }
    bool IsIacScanningEnabled { get; }
    bool IsSastScanningEnabled { get; }
    bool IsAiLargeLanguageModelEnabled { get; }
    
    bool IsTreeViewFilterByCriticalSeverityEnabled { get; set; }
    bool IsTreeViewFilterByHighSeverityEnabled { get; set; }
    bool IsTreeViewFilterByMediumSeverityEnabled { get; set; }
    bool IsTreeViewFilterByLowSeverityEnabled { get; set; }
    bool IsTreeViewFilterByInfoSeverityEnabled { get; set; }
}

public class TemporaryStateService : ITemporaryStateService {
    private StatusResult _cliStatus;
    private readonly ILoggerService _loggerService;

    public bool CliInstalled { get; set; } = false;

    public bool CliAuthed { get; set; } = false;

    public StatusResult CliStatus {
        get => _cliStatus;
        set {
            _cliStatus = value;
            _loggerService.Info("cliStatus set");
        }
    }

    public bool IsSecretScanningEnabled => _cliStatus?.SupportedModules?.SecretScanning == true;
    public bool IsScaScanningEnabled => _cliStatus?.SupportedModules?.ScaScanning == true;
    public bool IsIacScanningEnabled => _cliStatus?.SupportedModules?.IacScanning == true;
    public bool IsSastScanningEnabled => _cliStatus?.SupportedModules?.SastScanning == true;
    public bool IsAiLargeLanguageModelEnabled => _cliStatus?.SupportedModules?.AiLargeLanguageModel == true;

    public bool IsTreeViewFilterByCriticalSeverityEnabled { get; set; } = false;
    public bool IsTreeViewFilterByHighSeverityEnabled { get; set; } = false;
    public bool IsTreeViewFilterByMediumSeverityEnabled { get; set; } = false;
    public bool IsTreeViewFilterByLowSeverityEnabled { get; set; } = false;
    public bool IsTreeViewFilterByInfoSeverityEnabled { get; set; } = false;

    public TemporaryStateService(ILoggerService loggerService) {
        _loggerService = loggerService;
        _loggerService.Info("CycodeTemporaryStateService init");
    }
}