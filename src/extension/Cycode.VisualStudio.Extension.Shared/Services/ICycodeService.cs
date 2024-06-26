using System.Collections.Generic;

namespace Cycode.VisualStudio.Extension.Shared.Services;

public interface ICycodeService {
    Task InstallCliIfNeededAndCheckAuthenticationAsync();
    Task StartAuthAsync();
    Task StartSecretScanForCurrentProjectAsync();
    Task StartPathSecretScanAsync(string pathToScan, bool onDemand = false);
    Task StartPathSecretScanAsync(List<string> pathsToScan, bool onDemand = false);
}