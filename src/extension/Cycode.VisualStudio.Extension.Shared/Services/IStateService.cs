using Cycode.VisualStudio.Extension.Shared.DTO;

namespace Cycode.VisualStudio.Extension.Shared.Services;

public interface IStateService {
    ExtensionState Load();
    void Save(ExtensionState extensionState = null);
}