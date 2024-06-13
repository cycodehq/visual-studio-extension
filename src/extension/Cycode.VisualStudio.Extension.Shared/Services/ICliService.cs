using System.Threading.Tasks;
using TaskCancelledCallback = System.Func<bool>;

namespace Cycode.VisualStudio.Extension.Shared.Services;

public interface ICliService {
    Task<bool> HealthCheckAsync(TaskCancelledCallback cancelledCallback = null);
    Task<bool> CheckAuthAsync(TaskCancelledCallback cancelledCallback = null);
}