using Cycode.VisualStudio.Extension.Shared.DTO;
using Cycode.VisualStudio.Extension.Shared.Services;

namespace Cycode.VisualStudio.Extension.Shared.Commands;

internal static class TreeViewFilterBySeverityCommand {
    public static void Execute(OleMenuCommand command, string severity) {
        ITemporaryStateService tempState =
            ServiceLocator.GetService<ITemporaryStateService>();
        IToolWindowMessengerService toolWindowMessengerService =
            ServiceLocator.GetService<IToolWindowMessengerService>();

        switch (severity.ToLower()) {
            case "critical":
                tempState.IsTreeViewFilterByCriticalSeverityEnabled = !command.Checked;
                break;
            case "high":
                tempState.IsTreeViewFilterByHighSeverityEnabled = !command.Checked;
                break;
            case "medium":
                tempState.IsTreeViewFilterByMediumSeverityEnabled = !command.Checked;
                break;
            case "low":
                tempState.IsTreeViewFilterByLowSeverityEnabled = !command.Checked;
                break;
            case "info":
                tempState.IsTreeViewFilterByInfoSeverityEnabled = !command.Checked;
                break;
        }

        command.Checked = !command.Checked; // toggle checked state
        
        // refresh tree view to apply filter
        toolWindowMessengerService.Send(new MessageEventArgs(MessengerCommand.TreeViewRefresh));
    }
}

[Command(PackageIds.TreeViewFilterByCriticalSeverityCommand)]
internal sealed class TreeViewFilterByCriticalSeverityCommand : BaseCommand<TreeViewFilterByCriticalSeverityCommand> {
    protected override void Execute(object sender, EventArgs e) {
        if (sender is OleMenuCommand command) {
            TreeViewFilterBySeverityCommand.Execute(command, "critical");
        }
    }
}

[Command(PackageIds.TreeViewFilterByHighSeverityCommand)]
internal sealed class TreeViewFilterByHighSeverityCommand : BaseCommand<TreeViewFilterByHighSeverityCommand> {
    protected override void Execute(object sender, EventArgs e) {
        if (sender is OleMenuCommand command) {
            TreeViewFilterBySeverityCommand.Execute(command, "high");
        }
    }
}

[Command(PackageIds.TreeViewFilterByMediumSeverityCommand)]
internal sealed class TreeViewFilterByMediumSeverityCommand : BaseCommand<TreeViewFilterByMediumSeverityCommand> {
    protected override void Execute(object sender, EventArgs e) {
        if (sender is OleMenuCommand command) {
            TreeViewFilterBySeverityCommand.Execute(command, "medium");
        }
    }
}

[Command(PackageIds.TreeViewFilterByLowSeverityCommand)]
internal sealed class TreeViewFilterByLowSeverityCommand : BaseCommand<TreeViewFilterByLowSeverityCommand> {
    protected override void Execute(object sender, EventArgs e) {
        if (sender is OleMenuCommand command) {
            TreeViewFilterBySeverityCommand.Execute(command, "low");
        }
    }
}

[Command(PackageIds.TreeViewFilterByInfoSeverityCommand)]
internal sealed class TreeViewFilterByInfoSeverityCommand : BaseCommand<TreeViewFilterByInfoSeverityCommand> {
    protected override void Execute(object sender, EventArgs e) {
        if (sender is OleMenuCommand command) {
            TreeViewFilterBySeverityCommand.Execute(command, "info");
        }
    }
}
