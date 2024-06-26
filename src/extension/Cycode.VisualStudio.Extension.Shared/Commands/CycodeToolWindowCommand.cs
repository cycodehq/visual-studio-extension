namespace Cycode.VisualStudio.Extension.Shared;

[Command(PackageIds.CycodeToolWindowCommand)]
internal sealed class CycodeToolWindowCommand : BaseCommand<CycodeToolWindowCommand> {
    protected override Task ExecuteAsync(OleMenuCmdEventArgs e) {
        return CycodeToolWindow.ShowAsync();
    }
}

[Command(PackageIds.TopMenuCycodeCommand)]
internal sealed class TopMenuCycodeCommand : BaseCommand<TopMenuCycodeCommand> {
    protected override Task ExecuteAsync(OleMenuCmdEventArgs e) {
        return CycodeToolWindow.ShowAsync();
    }
}