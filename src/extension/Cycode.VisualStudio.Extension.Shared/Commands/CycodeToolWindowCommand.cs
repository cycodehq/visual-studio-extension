﻿namespace Cycode.VisualStudio.Extension.Shared
{
    [Command(PackageIds.CycodeCommand)]
    internal sealed class CycodeToolWindowCommand : BaseCommand<CycodeToolWindowCommand>
    {
        protected override Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            return CycodeToolWindow.ShowAsync();
        }
    }
}