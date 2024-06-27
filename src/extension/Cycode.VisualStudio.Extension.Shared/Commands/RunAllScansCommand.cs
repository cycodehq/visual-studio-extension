using Cycode.VisualStudio.Extension.Shared.Services;

namespace Cycode.VisualStudio.Extension.Shared;

[Command(PackageIds.ToolbarRunAllScansCommand)]
internal sealed class RunAllScansCommand : BaseCommand<RunAllScansCommand> {
    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e) {
        ICycodeService cycode = ServiceLocator.GetService<ICycodeService>();
        await cycode.StartSecretScanForCurrentProjectAsync();
    }
}