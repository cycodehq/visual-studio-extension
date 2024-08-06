using System.Collections.Generic;
using System.Threading;
using Cycode.VisualStudio.Extension.Shared.DTO;
using Cycode.VisualStudio.Extension.Shared.Helpers;
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
        Func<CancellationToken, Task> taskFunction,
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
        Task task = taskFunction(handler.UserCancellation);
        handler.RegisterTask(task);

        try {
            await task; // wait for the task to complete, otherwise it will be run in the background
        } finally {
            data.PercentComplete = 100;
            handler.Progress.Report(data);
        }
    }
#else
    private static async Task WrapWithStatusCenterAsync(
        Func<CancellationToken, Task> taskFunction,
        string label,
        bool canBeCanceled // For old VS version; doesn't support TaskStatusCenter; doesn't support cancellation
    ) {
        // currentStep must have a value of 1 or higher!
        await VS.StatusBar.ShowProgressAsync(label, currentStep: 1, numberOfSteps: 2);

        try {
            await taskFunction(default);
        } finally {
            await VS.StatusBar.ShowProgressAsync(label, currentStep: 2, numberOfSteps: 2);
        }
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

    private async Task InstallCliIfNeededAndCheckAuthenticationAsyncInternalAsync(CancellationToken cancellationToken) {
        try {
            toolWindowMessengerService.Send(MessengerCommand.LoadLoadingControl);

            bool successfullyInit = await cliDownloadService.InitCliAsync();
            if (!successfullyInit) {
                logger.Warn("Failed to init Cycode CLI. Aborting health check...");
                return;
            }

            await cliService.HealthCheckAsync(cancellationToken);
            await cliService.CheckAuthAsync(cancellationToken);

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

    private async Task StartAuthInternalAsync(CancellationToken cancellationToken) {
        if (!_pluginState.CliAuthed) {
            logger.Debug("Start auth...");
            await cliService.DoAuthAsync(cancellationToken);
            UpdateToolWindowDependingOnState();
        } else {
            logger.Debug("Already authenticated with Cycode CLI");
        }
    }

    public async Task StartSecretScanForCurrentProjectAsync() {
        string projectRoot = SolutionHelper.GetSolutionRootDirectory();
        if (projectRoot == null) {
            logger.Warn("Failed to get current project root. Aborting scan...");
            return;
        }

        await StartPathSecretScanAsync(projectRoot, onDemand: true);
    }

    public async Task StartPathSecretScanAsync(string pathToScan, bool onDemand = false) {
        await StartPathSecretScanAsync([pathToScan], onDemand);
    }

    public async Task StartPathSecretScanAsync(List<string> pathsToScan, bool onDemand = false) {
        await WrapWithStatusCenterAsync(
            taskFunction: cancellationToken =>
                StartPathSecretScanInternalAsync(pathsToScan, onDemand, cancellationToken),
            label: "Cycode is scanning files for hardcoded secrets...",
            canBeCanceled: true
        );
    }

    private async Task StartPathSecretScanInternalAsync(
        List<string> pathsToScan, bool onDemand = false, CancellationToken cancellationToken = default
    ) {
        if (!_pluginState.CliAuthed) {
            logger.Debug("Not authenticated with Cycode CLI. Aborting scan...");
            return;
        }

        logger.Debug("[Secret] Start scanning paths: {0}", string.Join(", ", pathsToScan));
        await cliService.ScanPathsSecretsAsync(pathsToScan, onDemand, cancellationToken);
        logger.Debug("[Secret] Finish scanning paths: {0}", string.Join(", ", pathsToScan));
    }

    public async Task StartScaScanForCurrentProjectAsync() {
        string projectRoot = SolutionHelper.GetSolutionRootDirectory();
        if (projectRoot == null) {
            logger.Warn("Failed to get current project root. Aborting scan...");
            return;
        }

        await StartPathScaScanAsync(projectRoot, onDemand: true);
    }

    public async Task StartPathScaScanAsync(string pathToScan, bool onDemand = false) {
        await StartPathScaScanAsync([pathToScan], onDemand);
    }

    public async Task StartPathScaScanAsync(List<string> pathsToScan, bool onDemand = false) {
        await WrapWithStatusCenterAsync(
            taskFunction: cancellationToken => StartPathScaScanInternalAsync(pathsToScan, onDemand, cancellationToken),
            label: "Cycode is scanning files for package vulnerabilities...",
            canBeCanceled: true
        );
    }

    private async Task StartPathScaScanInternalAsync(
        List<string> pathsToScan, bool onDemand = false, CancellationToken cancellationToken = default
    ) {
        if (!_pluginState.CliAuthed) {
            logger.Debug("Not authenticated with Cycode CLI. Aborting scan...");
            return;
        }

        logger.Debug("[SCA] Start scanning paths: {0}", string.Join(", ", pathsToScan));
        await cliService.ScanPathsScaAsync(pathsToScan, onDemand, cancellationToken);
        logger.Debug("[SCA] Finish scanning paths: {0}", string.Join(", ", pathsToScan));
    }
}