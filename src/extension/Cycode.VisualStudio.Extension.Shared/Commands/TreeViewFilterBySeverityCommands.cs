using Cycode.VisualStudio.Extension.Shared.DTO;
using Cycode.VisualStudio.Extension.Shared.Services;

namespace Cycode.VisualStudio.Extension.Shared.Commands;

internal static class TreeViewFilterBySeverityCommand {
    public static void Execute(OleMenuCommand command, string severity) {
        ITemporaryStateService tempState =
            ServiceLocator.GetService<ITemporaryStateService>();
        IToolWindowMessengerService toolWindowMessengerService =
            ServiceLocator.GetService<IToolWindowMessengerService>();

        /*
         * We must flip the bool first to reflect the new state.
         * 
         * Unchecked means that we do not want to see that severity in the tree view.
         * In other words, if the button highlighted, it means that we WANT to see that severity in the tree view.
         */
        command.Checked = !command.Checked;
        bool isFilterEnabled = !command.Checked;

        switch (severity.ToLower()) {
            case "critical":
                tempState.IsTreeViewFilterByCriticalSeverityEnabled = isFilterEnabled;
                break;
            case "high":
                tempState.IsTreeViewFilterByHighSeverityEnabled = isFilterEnabled;
                break;
            case "medium":
                tempState.IsTreeViewFilterByMediumSeverityEnabled = isFilterEnabled;
                break;
            case "low":
                tempState.IsTreeViewFilterByLowSeverityEnabled = isFilterEnabled;
                break;
            case "info":
                tempState.IsTreeViewFilterByInfoSeverityEnabled = isFilterEnabled;
                break;
        }

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
