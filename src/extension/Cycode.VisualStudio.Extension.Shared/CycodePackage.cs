global using System;
global using Community.VisualStudio.Toolkit;
global using Microsoft.VisualStudio.Shell;
global using Task = System.Threading.Tasks.Task;
using System.Runtime.InteropServices;
using System.Threading;
using Cycode.VisualStudio.Extension.Shared.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Cycode.VisualStudio.Extension.Shared;

[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
[InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
[ProvideToolWindow(typeof(CycodeToolWindow.Pane), Style = VsDockStyle.Tabbed, Window = WindowGuids.SolutionExplorer)]
[ProvideMenuResource("Menus.ctmenu", 1)]
[Guid(PackageGuids.CycodeString)]
public sealed class CycodePackage : ToolkitPackage {
    protected override async Task InitializeAsync(CancellationToken cancellationToken,
        IProgress<ServiceProgressData> progress) {
        ServiceCollection serviceCollection = [];
        Startup.ConfigureServices(serviceCollection);
        ServiceLocator.SetLocatorProvider(serviceCollection.BuildServiceProvider());

        // it will initialize the output pane
        ILoggerService logger = ServiceLocator.GetService<ILoggerService>();
        logger.LogInfo("CycodePackage.InitializeAsync started.");

        IStateService stateService = ServiceLocator.GetService<IStateService>();
        stateService.Load();

        await this.RegisterCommandsAsync();
        this.RegisterToolWindows();

        logger.LogInfo("CycodePackage.InitializeAsync completed.");
    }
}