using System.Collections.Generic;
using Cycode.VisualStudio.Extension.Shared.DTO;
#if VS16 || VS17
using Microsoft.VisualStudio.TaskStatusCenter;
#endif

namespace Cycode.VisualStudio.Extension.Shared.Services;

public class CycodeService(
    ILoggerService logger,
    IStateService stateService,
    ICliDownloadService cliDownloadService,
    ICliService cliService,
    IToolWindowMessengerService toolWindowMessengerService
) : ICycodeService {
    private readonly ExtensionState _pluginState = stateService.Load();

#if VS16 || VS17 // We don't have VS16 constant because we support range of versions in one project
    private static async Task WrapWithStatusCenterAsync(
        Func<Task> taskFunction,
        string label,
        bool canBeCanceled
    ) {
        IVsTaskStatusCenterService tsc = await VS.Services.GetTaskStatusCenterAsync();

        TaskHandlerOptions options = default;
        options.Title = label;
        options.ActionsAfterCompletion = CompletionActions.None;

        TaskProgressData data = default;
        data.CanBeCanceled = canBeCanceled;

        ITaskHandler handler = tsc.PreRegister(options, data);
        // TODO(MarshalX): Support CancellationToken!
        // Task task = taskFunction(handler.UserCancellation);
        Task task = taskFunction();
        handler.RegisterTask(task);

        await task; // wait for the task to complete, otherwise it will be run in the background

        data.PercentComplete = 100;
        handler.Progress.Report(data);
    }
#else
    private static async Task WrapWithStatusCenterAsync(
        Func<Task> taskFunction,
        string label,
        bool canBeCanceled
    ) {
        // For old VS version; doesn't support TaskStatusCenter; doesn't support cancellation
        await VS.StatusBar.ShowProgressAsync(label, currentStep: 0, numberOfSteps: 1);
        await taskFunction();
        await VS.StatusBar.ShowProgressAsync(label, currentStep: 1, numberOfSteps: 1);
    }
#endif

    private void UpdateToolWindowDependingOnState() {
        if (_pluginState.CliAuthed) {
            logger.Info("Successfully authenticated with Cycode CLI");
            toolWindowMessengerService.Send(MessengerCommand.LoadMainControl);
        } else {
            logger.Info("Failed to authenticate with Cycode CLI");
            toolWindowMessengerService.Send(MessengerCommand.LoadAuthControl);
        }
    }

    public async Task InstallCliIfNeededAndCheckAuthenticationAsync() {
        logger.Debug("Checking if Cycode CLI is installed and authenticated...");
        await WrapWithStatusCenterAsync(
            taskFunction: InstallCliIfNeededAndCheckAuthenticationAsyncInternalAsync,
            label: "Cycode is loading...",
            canBeCanceled: false
        );
    }

    private async Task InstallCliIfNeededAndCheckAuthenticationAsyncInternalAsync() {
        try {
            toolWindowMessengerService.Send(MessengerCommand.LoadLoadingControl);

            await cliDownloadService.InitCliAsync();
            await cliService.HealthCheckAsync();
            await cliService.CheckAuthAsync();

            UpdateToolWindowDependingOnState();
        } catch (Exception e) {
            logger.Error(e, "Failed to check Cycode CLI health and authentication");
        }
    }

    public async Task StartAuthAsync() {
        await WrapWithStatusCenterAsync(
            taskFunction: StartAuthInternalAsync,
            label: "Authenticating to Cycode...",
            canBeCanceled: false // TODO(MarshalX): Should be cancellable. Not implemented yet
        );
    }

    private async Task StartAuthInternalAsync() {
        if (!_pluginState.CliAuthed) {
            logger.Debug("Start auth...");
            await cliService.DoAuthAsync();
            UpdateToolWindowDependingOnState();
        } else {
            logger.Debug("Already authenticated with Cycode CLI");
        }
    }
    
    public async Task StartSecretScanForCurrentProject() {
        string projectRoot = (await VS.Solutions.GetCurrentSolutionAsync())?.FullPath;
        await StartPathSecretScanAsync(projectRoot, onDemand: true);
    }

    public async Task StartPathSecretScanAsync(string pathToScan, bool onDemand = false) {
        await StartPathSecretScanAsync([pathToScan], onDemand);
    }

    public async Task StartPathSecretScanAsync(List<string> pathsToScan, bool onDemand = false) {
        await WrapWithStatusCenterAsync(
            taskFunction: () => StartPathSecretScanInternalAsync(pathsToScan, onDemand),
            label: "Cycode is scanning files for hardcoded secrets...",
            canBeCanceled: false // TODO(MarshalX): Should be cancellable. Not implemented yet
        );
    }

    private async Task StartPathSecretScanInternalAsync(List<string> pathsToScan, bool onDemand = false) {
        if (!_pluginState.CliAuthed) {
            logger.Debug("Not authenticated with Cycode CLI. Aborting scan...");
            return;
        }

        logger.Debug("[Secret] Start scanning paths: {0}", string.Join(",", pathsToScan));
        await cliService.ScanPathsSecretsAsync(pathsToScan, onDemand);
        logger.Debug("[Secret] Finish scanning paths: {0}", string.Join(",", pathsToScan));
    }
}