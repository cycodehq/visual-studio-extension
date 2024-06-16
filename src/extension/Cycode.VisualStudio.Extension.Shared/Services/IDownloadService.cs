using System.IO;
using System.Threading.Tasks;

namespace Cycode.VisualStudio.Extension.Shared.Services;

public interface IDownloadService {
    Task<string> RetrieveFileTextContentAsync(string url);
    Task<FileInfo> DownloadFileAsync(string url, string checksum, string localPath);
}