using Cycode.VisualStudio.Extension.Shared.Components.ToolWindows;

namespace Cycode.VisualStudio.Extension.Shared;

[Command(PackageIds.ViewOpenToolWindowCommand)]
internal sealed class ViewOpenToolWindowCommand : BaseCommand<ViewOpenToolWindowCommand> {
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