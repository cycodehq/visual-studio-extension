using System.IO;
using Cycode.VisualStudio.Extension.Shared.DTO;
using Newtonsoft.Json;

namespace Cycode.VisualStudio.Extension.Shared.Services;

public class StateService : IStateService {
    // we are using a single instance of ExtensionState to avoid data desynchronization
    private readonly ExtensionState _extensionState;
    private readonly object _lockObject = new();
    private readonly ILoggerService _loggerService;

    private readonly string _storageFilePath;

    public StateService(ILoggerService loggerService) {
        _loggerService = loggerService;
        _extensionState = new ExtensionState();

        Directory.CreateDirectory(Constants.PluginPath);
        _storageFilePath = Path.Combine(Constants.PluginPath, "state.dat");
    }

    public ExtensionState Load() {
        lock (_lockObject) {
            if (!File.Exists(_storageFilePath)) {
                _loggerService.Debug("State file does not exist, creating new state");
                Save();
            }

            string extensionStateJson = File.ReadAllText(_storageFilePath);
            MergeState(JsonConvert.DeserializeObject<ExtensionState>(extensionStateJson));

            _loggerService.Debug("Loaded extension state: {0}", extensionStateJson);

            return _extensionState;
        }
    }

    public void Save() {
        lock (_lockObject) {
            string extensionStateJson = JsonConvert.SerializeObject(_extensionState);
            File.WriteAllText(_storageFilePath, extensionStateJson);

            _loggerService.Debug("Saved extension state: {0}", extensionStateJson);
        }
    }

    private void MergeState(ExtensionState extensionState) {
        lock (_lockObject) {
            _extensionState.CliInstalled = extensionState.CliInstalled;
            _extensionState.CliAuthed = extensionState.CliAuthed;
            _extensionState.CliVer = extensionState.CliVer;
            _extensionState.CliHash = extensionState.CliHash;
            _extensionState.CliLastUpdateCheckedAt = extensionState.CliLastUpdateCheckedAt;
        }
    }
}