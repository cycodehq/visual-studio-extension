using Cycode.VisualStudio.Extension.Shared.DTO;
using Cycode.VisualStudio.Extension.Shared.Services;
using Cycode.VisualStudio.Extension.Shared.Services.ErrorList;

namespace Cycode.VisualStudio.Extension.Shared.Commands;

[Command(PackageIds.ToolbarClearScanResultsCommand)]
internal sealed class ClearScanResultsCommand : BaseCommand<ClearScanResultsCommand> {
    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e) {
        IErrorTaskCreatorService errorTaskCreatorService =
            ServiceLocator.GetService<IErrorTaskCreatorService>();
        IToolWindowMessengerService toolWindowMessengerService =
            ServiceLocator.GetService<IToolWindowMessengerService>();

        await errorTaskCreatorService.ClearErrorsAsync();
        // in case violation card was opened, close it
        toolWindowMessengerService.Send(new MessageEventArgs(MessengerCommand.BackToHomeScreen));
    }
}