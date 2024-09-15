using Cycode.VisualStudio.Extension.Shared.DTO;
using Cycode.VisualStudio.Extension.Shared.Services;

namespace Cycode.VisualStudio.Extension.Shared.Commands;

[Command(PackageIds.ToolbarRunAllScansCommand)]
internal sealed class RunAllScansCommand : BaseCommand<RunAllScansCommand> {
    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e) {
        IStateService stateService = ServiceLocator.GetService<IStateService>();
        ExtensionState pluginState = stateService.Load();
        if (!pluginState.CliAuthed) {
            await VS.StatusBar.ShowMessageAsync("Please authenticate with Cycode first.");
            return;
        }

        ICycodeService cycode = ServiceLocator.GetService<ICycodeService>();
        cycode.StartSecretScanForCurrentProjectAsync().FireAndForget();
        cycode.StartScaScanForCurrentProjectAsync().FireAndForget();
    }
}