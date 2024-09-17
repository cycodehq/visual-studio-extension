using System.Collections.Generic;
using System.Linq;

namespace Cycode.VisualStudio.Extension.Shared.Cli.DTO;

public static class IacHelper {
    // keep in lowercase.
    // source: https://github.com/cycodehq/cycode-cli/blob/ec8333707ab2590518fd0f36454c8636ccbf1061/cycode/cli/consts.py#L16
    private static readonly List<string> _infraConfigurationScanSupportedFileSuffixes = [
        ".tf",
        ".tf.json",
        ".json",
        ".yaml",
        ".yml",
        "dockerfile"
    ];

    public static bool IsSupportedIacFile(string filename) {
        string lowercaseFilename = filename.ToLowerInvariant();
        return _infraConfigurationScanSupportedFileSuffixes.Any(file => lowercaseFilename.EndsWith(file));
    }
}