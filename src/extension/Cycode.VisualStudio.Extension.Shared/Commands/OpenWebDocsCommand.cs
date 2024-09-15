using System.Diagnostics;

namespace Cycode.VisualStudio.Extension.Shared.Commands;

[Command(PackageIds.ToolbarOpenWebDocsCommand)]
internal sealed class OpenWebDocsCommand : BaseCommand<OpenWebDocsCommand> {
    private const string _webDocsUrl = "https://github.com/cycodehq/visual-studio-extension/blob/main/README.md";

    protected override void Execute(object sender, EventArgs e) {
        Process.Start(new ProcessStartInfo(_webDocsUrl));
    }
}