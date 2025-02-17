using System.Collections.Generic;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO;
using Cycode.VisualStudio.Extension.Shared.Services;

namespace Cycode.VisualStudio.Extension.Shared.Commands;

[Command(PackageIds.ToolbarRunAllScansCommand)]
internal sealed class RunAllScansCommand : BaseCommand<RunAllScansCommand> {
    protected override async Task ExecuteAsync(OleMenuCmdEventArgs e) {
        ITemporaryStateService tempState = ServiceLocator.GetService<ITemporaryStateService>();
        if (!tempState.CliAuthed) {
            await VS.StatusBar.ShowMessageAsync("Please authenticate with Cycode first.");
            return;
        }

        Command.Enabled = false;
        string originalText = Command.Text;
        Command.Text = "Scanning entire project...";

        try {
            ICycodeService cycode = ServiceLocator.GetService<ICycodeService>();
            List<Task> scanTasks = [];

            if (tempState.IsSecretScanningEnabled) {
                scanTasks.Add(cycode.StartProjectScanAsync(CliScanType.Secret));
            }
            if (tempState.IsScaScanningEnabled) {
                scanTasks.Add(cycode.StartProjectScanAsync(CliScanType.Sca));
            }
            if (tempState.IsIacScanningEnabled) {
                scanTasks.Add(cycode.StartProjectScanAsync(CliScanType.Iac));
            }
            if (tempState.IsSastScanningEnabled) {
                scanTasks.Add(cycode.StartProjectScanAsync(CliScanType.Sast));
            }

            await Task.WhenAll(scanTasks);
        } finally {
            Command.Enabled = true;
            Command.Text = originalText;
        }
    }
}