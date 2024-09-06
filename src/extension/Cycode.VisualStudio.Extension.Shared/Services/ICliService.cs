using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Cycode.VisualStudio.Extension.Shared.Services;

public interface ICliService {
    void ResetPluginCliState();

    Task<bool> HealthCheckAsync(CancellationToken cancellationToken = default);
    Task<bool> CheckAuthAsync(CancellationToken cancellationToken = default);
    Task<bool> DoAuthAsync(CancellationToken cancellationToken = default);

    Task ScanPathsSecretsAsync(
        List<string> paths, bool onDemand = true, CancellationToken cancellationToken = default
    );

    Task ScanPathsScaAsync(
        List<string> paths, bool onDemand = true, CancellationToken cancellationToken = default
    );
}