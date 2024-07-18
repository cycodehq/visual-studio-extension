using System.Threading.Tasks;

namespace Cycode.VisualStudio.Extension.Shared.Services;
    public interface ICliDownloadService {
        Task<bool> InitCliAsync();
    }
