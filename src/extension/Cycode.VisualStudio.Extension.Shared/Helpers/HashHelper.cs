using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Cycode.VisualStudio.Extension.Shared.Helpers;

public static class HashHelper {
    public static async Task<bool> VerifyFileChecksumAsync(string filePath, string expectedChecksum) {
        if (!File.Exists(filePath)) return false;

        using FileStream stream = File.OpenRead(filePath);
        SHA256 sha256 = SHA256.Create();
        byte[] hash = await Task.Run(() => sha256.ComputeHash(stream));
        string actualChecksum = BitConverter.ToString(hash).Replace("-", "").ToLower();
        return actualChecksum == expectedChecksum.ToLower();
    }
}