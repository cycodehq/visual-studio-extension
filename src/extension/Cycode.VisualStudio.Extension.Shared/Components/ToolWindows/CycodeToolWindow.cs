using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Cycode.VisualStudio.Extension.Shared.Services;
using Microsoft.VisualStudio.Imaging;

namespace Cycode.VisualStudio.Extension.Shared.Components.ToolWindows;

public class CycodeToolWindow : BaseToolWindow<CycodeToolWindow> {
    public override Type PaneType => typeof(Pane);

    public override string GetTitle(int toolWindowId) {
        return "Cycode";
    }

    public override Task<FrameworkElement> CreateAsync(int toolWindowId, CancellationToken cancellationToken) {
        IToolWindowMessengerService toolWindowMessengerService =
            ServiceLocator.GetService<IToolWindowMessengerService>();
        return Task.FromResult<FrameworkElement>(new CycodeToolWindowControl(toolWindowMessengerService));
    }

    [Guid("df1eb4de-941c-47bc-b772-862331f0a68a")]
    internal class Pane : ToolkitToolWindowPane {
        public Pane() {
            BitmapImageMoniker = KnownMonikers.ToolWindow;
            ToolBar = new CommandID(PackageGuids.Cycode, PackageIds.TWindowToolbar);
        }
    }
}