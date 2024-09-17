﻿using Cycode.VisualStudio.Extension.Shared.DTO;
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

        Command.Enabled = false;
        string originalText = Command.Text;
        Command.Text = "Scanning entire project...";

        try {
            ICycodeService cycode = ServiceLocator.GetService<ICycodeService>();
            await Task.WhenAll(
                cycode.StartSecretScanForCurrentProjectAsync(),
                cycode.StartScaScanForCurrentProjectAsync(),
                cycode.StartIacScanForCurrentProjectAsync(),
                cycode.StartSastScanForCurrentProjectAsync()
            );
        } finally {
            Command.Enabled = true;
            Command.Text = originalText;
        }
    }
}