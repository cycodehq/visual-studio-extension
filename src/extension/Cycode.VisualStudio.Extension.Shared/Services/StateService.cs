using Cycode.VisualStudio.Extension.Shared.DTO;

namespace Cycode.VisualStudio.Extension.Shared.Services;

using System.IO;
using Newtonsoft.Json;

public class StateService : IStateService {
    private readonly ILoggerService _loggerService;

    private readonly string _storageFilePath;
    // we are using a single instance of ExtensionState to avoid data desynchronization
    private readonly ExtensionState _extensionState;

    public StateService(ILoggerService loggerService) {
        _loggerService = loggerService;
        _extensionState = new ExtensionState();

        Directory.CreateDirectory(Constants.PluginPath);
        _storageFilePath = Path.Combine(Constants.PluginPath, "state.dat");
    }
    
    private void MergeState(ExtensionState extensionState) {
        _extensionState.CliInstalled = extensionState.CliInstalled;
        _extensionState.CliAuthed = extensionState.CliAuthed;
        _extensionState.CliVer = extensionState.CliVer;
        _extensionState.CliHash = extensionState.CliHash;
        _extensionState.CliLastUpdateCheckedAt = extensionState.CliLastUpdateCheckedAt;
    }

    public ExtensionState Load() {
        if (!File.Exists(_storageFilePath)) {
            _loggerService.Debug("State file does not exist, creating new state");
            Save();
        }

        string extensionStateJson = File.ReadAllText(_storageFilePath);
        MergeState(JsonConvert.DeserializeObject<ExtensionState>(extensionStateJson));

        _loggerService.Debug($"Loaded extension state: {extensionStateJson}");

        return _extensionState;
    }

    public void Save() {
        string extensionStateJson = JsonConvert.SerializeObject(_extensionState);
        File.WriteAllText(_storageFilePath, extensionStateJson);

        _loggerService.Debug($"Saved extension state: {extensionStateJson}");
    }
}