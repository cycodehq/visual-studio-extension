using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell.Interop;
using Newtonsoft.Json;

namespace Cycode.VisualStudio.Extension.Shared.Cli;

public class IdeUserAgent {
    public string AppName { get; set; }
    public string AppVersion { get; set; }
    public string EnvName { get; set; }
    public string EnvVersion { get; set; }
}

public static class UserAgent {
    private static async Task<IdeUserAgent> RetrieveIdeUserAgentAsync() {
        const string appName = "visual_studio_plugin";
        const string appVersion = Vsix.Version;

        string envName = await GetEnvNameAsync();
        string envVersion = (await GetVsVersionAsync()).ToString();

        return new IdeUserAgent {
            AppName = appName,
            AppVersion = appVersion,
            EnvName = envName,
            EnvVersion = envVersion
        };
    }

    /// <summary>
    /// Gets the version of Visual Studio.
    /// </summary>
    private static async Task<Version> GetVsVersionAsync() {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        IVsShell shell = await VS.Services.GetShellAsync();

        shell.GetProperty((int)__VSSPROPID5.VSSPROPID_ReleaseVersion, out object value);

        if (value is string raw) {
            return Version.Parse(raw.Split(' ')[0]);
        }

        return Version.Parse("0.0.0.0");
    }

    private static async Task<string> GetEnvNameAsync() {
        Version envVersion = await GetVsVersionAsync();

        return envVersion.Major switch {
            17 => "Visual Studio 2022",
            16 => "Visual Studio 2019",
            15 => "Visual Studio 2017",
            14 => "Visual Studio 2015",
            _ => "Unknown Visual Studio version"
        };
    }

    public static async Task<string> GetUserAgentAsync() {
        IdeUserAgent userAgent = await RetrieveIdeUserAgentAsync();
        return JsonConvert.SerializeObject(userAgent, new JsonSerializerSettings {
            ContractResolver = new SnakeCasePropertyNamesContractResolver()
        });
    }
}