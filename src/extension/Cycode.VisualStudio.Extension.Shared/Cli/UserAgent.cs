using System.Threading.Tasks;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO;
using Cycode.VisualStudio.Extension.Shared.JsonContractResolvers;
using Microsoft.VisualStudio.Shell.Interop;
using Newtonsoft.Json;

namespace Cycode.VisualStudio.Extension.Shared.Cli;

public static class UserAgent {
    private static async Task<IdeUserAgent> RetrieveIdeUserAgentAsync() {
        string envName = await GetEnvNameAsync();
        string envVersion = (await GetVsVersionAsync()).ToString();

        return new IdeUserAgent {
            AppName = Constants.AppName,
            AppVersion = Vsix.Version,
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
    
    public static async Task<string> GetUserAgentEscapedAsync() {
        return JsonConvert.ToString(await GetUserAgentAsync());
    }
}