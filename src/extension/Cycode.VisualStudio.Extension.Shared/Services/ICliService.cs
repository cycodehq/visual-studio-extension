using System.Collections.Generic;
using System.Threading.Tasks;
using TaskCancelledCallback = System.Func<bool>;

namespace Cycode.VisualStudio.Extension.Shared.Services;

public interface ICliService {
    Task<bool> HealthCheckAsync(TaskCancelledCallback cancelledCallback = null);
    Task<bool> CheckAuthAsync(TaskCancelledCallback cancelledCallback = null);
    Task<bool> DoAuthAsync(TaskCancelledCallback cancelledCallback = null);

    Task ScanPathsSecretsAsync(
        List<string> paths, bool onDemand = true, TaskCancelledCallback cancelledCallback = null
    );
    Task ScanPathsScaAsync(
        List<string> paths, bool onDemand = true, TaskCancelledCallback cancelledCallback = null
    );
}