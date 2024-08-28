using System.Collections.Generic;
using System.Linq;

namespace Cycode.VisualStudio.Extension.Shared.Cli.DTO;

public static class ScaHelper {
    // keep in lowercase.
    // source: https://github.com/cycodehq/cycode-cli/blob/ec8333707ab2590518fd0f36454c8636ccbf1061/cycode/cli/consts.py#L50-L82
    private static readonly List<string> _scaConfigurationScanSupportedFiles = [
        "cargo.lock",
        "cargo.toml",
        "composer.json",
        "composer.lock",
        "go.sum",
        "go.mod",
        // "gopkg.toml", // FIXME(MarshalX): missed in CLI?
        "gopkg.lock",
        "pom.xml",
        "build.gradle",
        "gradle.lockfile",
        "build.gradle.kts",
        "package.json",
        "package-lock.json",
        "yarn.lock",
        "npm-shrinkwrap.json",
        "packages.config",
        "project.assets.json",
        "packages.lock.json",
        "nuget.config",
        ".csproj",
        "gemfile",
        "gemfile.lock",
        "build.sbt",
        "build.scala",
        "build.sbt.lock",
        "pyproject.toml",
        "poetry.lock",
        "pipfile",
        "pipfile.lock",
        "requirements.txt",
        "setup.py",
        "mix.exs",
        "mix.lock"
    ];

    private static readonly Dictionary<string, string> _scaConfigurationScanLockFileToPackageFile =
        new() {
            { "cargo.lock", "cargo.toml" },
            { "composer.lock", "composer.json" },
            { "go.sum", "go.mod" },
            { "gopkg.lock", "gopkg.toml" },
            { "gradle.lockfile", "build.gradle" },
            { "package-lock.json", "package.json" },
            { "yarn.lock", "package.json" },
            { "packages.lock.json", "nuget.config" },
            { "gemfile.lock", "gemfile" },
            { "build.sbt.lock", "build.sbt" }, // and build.scala?
            { "poetry.lock", "pyproject.toml" },
            { "pipfile.lock", "pipfile" },
            { "mix.lock", "mix.exs" }
        };

    private static readonly List<string> _scaConfigurationScanSupportedLockFiles =
        _scaConfigurationScanLockFileToPackageFile.Keys.ToList();

    public static bool IsSupportedPackageFile(string filename) {
        string lowercaseFilename = filename.ToLowerInvariant();
        return _scaConfigurationScanSupportedFiles.Any(file => lowercaseFilename.EndsWith(file));
    }

    public static bool IsSupportedLockFile(string filename) {
        string lowercaseFilename = filename.ToLowerInvariant();
        return _scaConfigurationScanSupportedLockFiles.Any(file => lowercaseFilename.EndsWith(file));
    }

    public static string GetPackageFileForLockFile(string filename) {
        string lowercaseFilename = filename.ToLowerInvariant();
        return _scaConfigurationScanLockFileToPackageFile.TryGetValue(lowercaseFilename, out string packageFile)
            ? packageFile
            : "package";
    }
}