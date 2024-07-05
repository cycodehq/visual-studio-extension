global using System;
global using Community.VisualStudio.Toolkit;
global using Microsoft.VisualStudio.Shell;
global using Task = System.Threading.Tasks.Task;
using System.Runtime.InteropServices;
using System.Threading;
using Cycode.VisualStudio.Extension.Shared.Services;
using Cycode.VisualStudio.Extension.Shared.Services.ErrorList;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.Shell.Interop;

namespace Cycode.VisualStudio.Extension.Shared;

[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
[InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
[ProvideOptionPage(typeof(OptionsProvider.GeneralOptions), Vsix.Name, "General", 0, 0, true, SupportsProfiles = true)]
[ProvideProfile(typeof(OptionsProvider.GeneralOptions), Vsix.Name, "General", 0, 0, true)]
[ProvideToolWindow(typeof(CycodeToolWindow.Pane), Style = VsDockStyle.Tabbed, Window = WindowGuids.SolutionExplorer)]
[ProvideMenuResource("Menus.ctmenu", 1)]
[Guid(PackageGuids.CycodeString)]
public sealed class CycodePackage : ToolkitPackage {
    public static ErrorTaggerProvider ErrorTaggerProvider; // FIXME(MarshalX): move me out of here

    protected override async Task InitializeAsync(
        CancellationToken cancellationToken, IProgress<ServiceProgressData> progress
    ) {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        ServiceCollection serviceCollection = [];
        Startup.ConfigureServices(serviceCollection);
        ServiceLocator.SetLocatorProvider(serviceCollection.BuildServiceProvider());

        ILoggerService logger = ServiceLocator.GetService<ILoggerService>();
        logger.Initialize();

        logger.Info("CycodePackage.InitializeAsync started.");

        IErrorListService errorsService = ServiceLocator.GetService<IErrorListService>();
        errorsService.Initialize(this);

        IStateService stateService = ServiceLocator.GetService<IStateService>();
        stateService.Load();

        await this.RegisterCommandsAsync();
        this.RegisterToolWindows();

        IDocTableEventsHandlerService docTableEventsHandlerService =
            ServiceLocator.GetService<IDocTableEventsHandlerService>();
        docTableEventsHandlerService.Initialize(this);

        General.Saved += OnSettingsSaved;

        ICycodeService cycodeService = ServiceLocator.GetService<ICycodeService>();
        cycodeService.InstallCliIfNeededAndCheckAuthenticationAsync().FireAndForget();

        logger.Info("CycodePackage.InitializeAsync completed.");
    }

    private static void OnSettingsSaved(General obj) {
        // reload CLI on settings save
        // apply executable path, on-premise settings, etc.
        // TODO(MarshalX): Check is there are real changes or the values are the same
        ICycodeService cycodeService = ServiceLocator.GetService<ICycodeService>();
        cycodeService.InstallCliIfNeededAndCheckAuthenticationAsync().FireAndForget();
    }
}