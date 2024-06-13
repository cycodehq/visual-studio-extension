using Cycode.VisualStudio.Extension.Shared.Helpers;

namespace Cycode.VisualStudio.Extension.Shared.Services;

using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

public class DownloadService: IDownloadService {
    private readonly ILoggerService _logger;
    private readonly HttpClient _client;

    public DownloadService(ILoggerService loggerService) {
        _logger = loggerService;
        _client = new HttpClient();

        // Required by GitHub API
        _client.DefaultRequestHeaders.Add("User-Agent", "Cycode.VisualStudio.Extension");
    }

    private static async Task<bool> ShouldSaveFileAsync(string tempFilePath, string checksum) {
        // if we don't expect checksum validation or checksum is valid
        return checksum == null || await HashHelper.VerifyFileChecksumAsync(tempFilePath, checksum);
    }

    public async Task<string> RetrieveFileTextContentAsync(string url) {
        _logger.Debug($"Retrieving text content of {url}");

        try {
            return await _client.GetStringAsync(url);
        } catch (Exception e) {
            _logger.Error($"Failed to retrieve file {e}");
        }

        return null;
    }

    public async Task<FileInfo> DownloadFileAsync(string url, string checksum, string localPath) {
        _logger.Debug($"Downloading {url} with checksum {checksum}");
        _logger.Debug($"Expecting to download to {localPath}");

        FileInfo file = new(localPath);
        string tempFile = Path.GetTempFileName();

        _logger.Debug($"Temp path: {tempFile}");

        try {
            using (Stream inputStream = await _client.GetStreamAsync(url))
            using (FileStream outputStream = new(tempFile, FileMode.Create)) {
                await inputStream.CopyToAsync(outputStream);
            }

            if (await ShouldSaveFileAsync(tempFile, checksum)) {
                if (file.Exists) {
                    file.Delete();
                }

                if (file.DirectoryName == null) {
                    _logger.Error($"Failed to get directory for {file}");
                    return null;
                }

                try {
                    Directory.CreateDirectory(file.DirectoryName);
                } catch (Exception e) {
                    _logger.Warn($"Failed to create directories for {file}. Probably exists already. {e}");
                }

                File.Move(tempFile, file.FullName);

                return file;
            }
        } catch (Exception e) {
            _logger.Error($"Failed to download file {e}");
        } finally {
            if (File.Exists(tempFile)) {
                File.Delete(tempFile);
            }
        }

        return null;
    }
}