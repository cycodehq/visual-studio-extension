using Cycode.VisualStudio.Extension.Shared.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Cycode.VisualStudio.Extension.Shared;

public static class Startup {
    public static void ConfigureServices(IServiceCollection services) {
        services.AddSingleton<ILoggerService, LoggerService>();
        services.AddSingleton<IStateService, StateService>();
    }
}