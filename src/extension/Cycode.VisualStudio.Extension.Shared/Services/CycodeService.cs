using Cycode.VisualStudio.Extension.Shared.DTO;

namespace Cycode.VisualStudio.Extension.Shared.Services;

public class CycodeService(
    ILoggerService logger,
    IStateService stateService,
    ICliDownloadService cliDownloadService,
    ICliService cliService
) : ICycodeService {
    private readonly ExtensionState _pluginState = stateService.Load();

    public async Task InstallCliIfNeededAndCheckAuthenticationAsync() {
        logger.Debug("Checking if Cycode CLI is installed and authenticated...");
        await VS.StatusBar.ShowProgressAsync("Cycode is loading...", 1, 2);

        try {
            await cliDownloadService.InitCliAsync();
            await cliService.HealthCheckAsync();
            await cliService.CheckAuthAsync();
        } finally {
            await VS.StatusBar.ShowProgressAsync("Cycode is loading...", 2, 2);
        }
    }
}