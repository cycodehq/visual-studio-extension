using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Cycode.VisualStudio.Extension.Shared.Helpers;

namespace Cycode.VisualStudio.Extension.Shared.Services;

public class DownloadService : IDownloadService {
    private readonly HttpClient _client;
    private readonly ILoggerService _logger;

    public DownloadService(ILoggerService loggerService) {
        _logger = loggerService;
        _client = new HttpClient();

        // Required by GitHub API
        _client.DefaultRequestHeaders.Add("User-Agent", "Cycode.VisualStudio.Extension");
    }

    public async Task<string> RetrieveFileTextContentAsync(string url) {
        _logger.Debug("Retrieving text content of {0}", url);

        try {
            return await _client.GetStringAsync(url);
        } catch (Exception e) {
            _logger.Error(e, "Failed to retrieve file");
        }

        return null;
    }

    public async Task<FileInfo> DownloadFileAsync(string url, string checksum, string localPath) {
        _logger.Debug("Downloading {0} with checksum {1}", url, checksum);
        _logger.Debug("Expecting to download to {0}", localPath);

        FileInfo file = new(localPath);
        string tempFile = Path.GetTempFileName();

        _logger.Debug("Temp path: {0}", tempFile);

        try {
            using (Stream inputStream = await _client.GetStreamAsync(url))
            using (FileStream outputStream = new(tempFile, FileMode.Create)) {
                await inputStream.CopyToAsync(outputStream);
            }

            if (await ShouldSaveFileAsync(tempFile, checksum)) {
                if (file.Exists) file.Delete();

                if (file.DirectoryName == null) {
                    _logger.Error("Failed to get directory for {0}", file);
                    return null;
                }

                try {
                    Directory.CreateDirectory(file.DirectoryName);
                } catch (Exception e) {
                    _logger.Warn(e, "Failed to create directories for {0}. Probably exists already", file);
                }

                File.Move(tempFile, file.FullName);

                return file;
            }
        } catch (Exception e) {
            _logger.Error(e, "Failed to download file");
        } finally {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }

        return null;
    }

    private static async Task<bool> ShouldSaveFileAsync(string tempFilePath, string checksum) {
        // if we don't expect checksum validation or checksum is valid
        return checksum == null || await HashHelper.VerifyFileChecksumAsync(tempFilePath, checksum);
    }
}