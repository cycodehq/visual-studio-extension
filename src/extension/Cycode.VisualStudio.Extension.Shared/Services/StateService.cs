using Cycode.VisualStudio.Extension.Shared.DTO;

namespace Cycode.VisualStudio.Extension.Shared.Services;

using System;
using System.IO;
using Newtonsoft.Json;

public class StateService : IStateService {
    private readonly ILoggerService _loggerService;

    private readonly string _storageFilePath;
    private ExtensionState _extensionState;

    public StateService(ILoggerService loggerService) {
        _loggerService = loggerService;

        string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        string extensionFolder = Path.Combine(appDataPath, "Cycode", "VisualStudioExtension");
        Directory.CreateDirectory(extensionFolder);

        _storageFilePath = Path.Combine(extensionFolder, "state.dat");
    }

    public ExtensionState Load() {
        if (!File.Exists(_storageFilePath)) {
            _loggerService.LogDebug("State file does not exist, creating new state");
            _extensionState = new ExtensionState();
            Save();
        }

        string extensionStateJson = File.ReadAllText(_storageFilePath);
        _extensionState = JsonConvert.DeserializeObject<ExtensionState>(extensionStateJson);
        
        _loggerService.LogDebug($"Loaded extension state: {extensionStateJson}");

        return _extensionState;
    }

    public void Save(ExtensionState extensionState = null) {
        if (extensionState != null) {
            _extensionState = extensionState;
        }

        if (_extensionState == null) {
            _loggerService.LogError("Extension state is null on save");
            return;
        }

        string extensionStateJson = JsonConvert.SerializeObject(_extensionState);
        File.WriteAllText(_storageFilePath, extensionStateJson);

        _loggerService.LogDebug($"Saved extension state: {extensionStateJson}");
    }
}