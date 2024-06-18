using Cycode.VisualStudio.Extension.Shared.Services;

namespace Cycode.VisualStudio.Extension.Shared;

[Command(PackageIds.OpenSettingsCommand)]
internal sealed class OpenSettingsCommand : BaseCommand<OpenSettingsCommand> {
    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e) {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
        ThreadHelper.JoinableTaskFactory.RunAsync(async () => {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            IToolWindowMessengerService toolWindowMessengerService = ServiceLocator.GetService<IToolWindowMessengerService>();
            toolWindowMessengerService.Send("test");
        }).FireAndForget();
    }
}