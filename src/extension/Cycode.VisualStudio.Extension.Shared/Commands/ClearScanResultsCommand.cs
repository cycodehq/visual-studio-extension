using Cycode.VisualStudio.Extension.Shared.Services.ErrorList;

namespace Cycode.VisualStudio.Extension.Shared;

[Command(PackageIds.ToolbarClearScanResultsCommand)]
internal sealed class ClearScanResultsCommand : BaseCommand<ClearScanResultsCommand> {
    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e) {
        IErrorTaskCreatorService errorTaskCreatorService = ServiceLocator.GetService<IErrorTaskCreatorService>();
        await errorTaskCreatorService.ClearErrorsAsync();
    }
}