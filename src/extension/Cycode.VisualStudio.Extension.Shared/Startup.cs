using Cycode.VisualStudio.Extension.Shared.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Cycode.VisualStudio.Extension.Shared;

public static class Startup {
    public static void ConfigureServices(IServiceCollection services) {
        services.AddSingleton<ILoggerService, LoggerService>();
        services.AddSingleton<IStateService, StateService>();
        services.AddSingleton<IDownloadService, DownloadService>();
        services.AddSingleton<IGitHubReleasesService, GithubReleasesService>();
        services.AddSingleton<ICliDownloadService, CliDownloadService>();
        services.AddSingleton<ICliService, CliService>();
        services.AddSingleton<ICycodeService, CycodeService>();
        services.AddSingleton<IToolWindowMessengerService, ToolWindowMessengerService>();
    }
}