using System.Collections.Generic;
using System.Threading;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO;
using Cycode.VisualStudio.Extension.Shared.DTO;
using Cycode.VisualStudio.Extension.Shared.Helpers;
using Cycode.VisualStudio.Extension.Shared.Services.ErrorList;
#if VS16 || VS17
using Microsoft.VisualStudio.TaskStatusCenter;
#endif

namespace Cycode.VisualStudio.Extension.Shared.Services;

public interface ICycodeService {
    Task InstallCliIfNeededAndCheckAuthenticationAsync();
    Task StartAuthAsync();
    Task StartSecretScanForCurrentProjectAsync();
    Task StartPathSecretScanAsync(string pathToScan, bool onDemand = false);
    Task StartPathSecretScanAsync(List<string> pathsToScan, bool onDemand = false);
    Task StartScaScanForCurrentProjectAsync();
    Task StartPathScaScanAsync(string pathToScan, bool onDemand = false);
    Task StartPathScaScanAsync(List<string> pathsToScan, bool onDemand = false);
    Task StartIacScanForCurrentProjectAsync();
    Task StartPathIacScanAsync(string pathToScan, bool onDemand = false);
    Task StartPathIacScanAsync(List<string> pathsToScan, bool onDemand = false);
    Task StartSastScanForCurrentProjectAsync();
    Task StartPathSastScanAsync(string pathToScan, bool onDemand = false);
    Task StartPathSastScanAsync(List<string> pathsToScan, bool onDemand = false);

    Task ApplyDetectionIgnoreAsync(
        CliScanType scanType, CliIgnoreType ignoreType, string value
    );
}

public class CycodeService(
    ILoggerService logger,
    IStateService stateService,
    ICliDownloadService cliDownloadService,
    ICliService cliService,
    IToolWindowMessengerService toolWindowMessengerService,
    IScanResultsService scanResultsService,
    IErrorTaskCreatorService errorTaskCreatorService
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
        await VS.StatusBar.ShowProgressAsync(label, 1, 2);

        try {
            await taskFunction(default);
        } finally {
            await VS.StatusBar.ShowProgressAsync(label, 2, 2);
        }
    }
#endif

    private void UpdateToolWindowDependingOnState() {
        if (_pluginState.CliAuthed) {
            logger.Info("Successfully authenticated with Cycode CLI");
            toolWindowMessengerService.Send(new MessageEventArgs(MessengerCommand.LoadMainControl));
        } else {
            logger.Info("Failed to authenticate with Cycode CLI");
            toolWindowMessengerService.Send(new MessageEventArgs(MessengerCommand.LoadAuthControl));
        }
    }

    public async Task InstallCliIfNeededAndCheckAuthenticationAsync() {
        logger.Debug("Checking if Cycode CLI is installed and authenticated...");
        await WrapWithStatusCenterAsync(
            InstallCliIfNeededAndCheckAuthenticationAsyncInternalAsync,
            "Cycode is loading...",
            false
        );
    }

    private async Task InstallCliIfNeededAndCheckAuthenticationAsyncInternalAsync(CancellationToken cancellationToken) {
        try {
            toolWindowMessengerService.Send(new MessageEventArgs(MessengerCommand.LoadLoadingControl));

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
            StartAuthInternalAsync,
            "Authenticating to Cycode...",
            true
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

        await StartPathSecretScanAsync(projectRoot, true);
    }

    public async Task StartPathSecretScanAsync(string pathToScan, bool onDemand = false) {
        await StartPathSecretScanAsync([pathToScan], onDemand);
    }

    public async Task StartPathSecretScanAsync(List<string> pathsToScan, bool onDemand = false) {
        await WrapWithStatusCenterAsync(
            cancellationToken =>
                StartPathSecretScanInternalAsync(pathsToScan, onDemand, cancellationToken),
            "Cycode is scanning files for hardcoded secrets...",
            true
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

        await StartPathScaScanAsync(projectRoot, true);
    }

    public async Task StartPathScaScanAsync(string pathToScan, bool onDemand = false) {
        await StartPathScaScanAsync([pathToScan], onDemand);
    }

    public async Task StartPathScaScanAsync(List<string> pathsToScan, bool onDemand = false) {
        await WrapWithStatusCenterAsync(
            cancellationToken => StartPathScaScanInternalAsync(pathsToScan, onDemand, cancellationToken),
            "Cycode is scanning files for package vulnerabilities...",
            true
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

    public async Task StartIacScanForCurrentProjectAsync() {
        string projectRoot = SolutionHelper.GetSolutionRootDirectory();
        if (projectRoot == null) {
            logger.Warn("Failed to get current project root. Aborting scan...");
            return;
        }

        await StartPathIacScanAsync(projectRoot, true);
    }

    public async Task StartPathIacScanAsync(string pathToScan, bool onDemand = false) {
        await StartPathIacScanAsync([pathToScan], onDemand);
    }

    public async Task StartPathIacScanAsync(List<string> pathsToScan, bool onDemand = false) {
        await WrapWithStatusCenterAsync(
            cancellationToken => StartPathIacScanInternalAsync(pathsToScan, onDemand, cancellationToken),
            "Cycode is scanning files for Infrastructure As Code...",
            true
        );
    }

    private async Task StartPathIacScanInternalAsync(
        List<string> pathsToScan, bool onDemand = false, CancellationToken cancellationToken = default
    ) {
        if (!_pluginState.CliAuthed) {
            logger.Debug("Not authenticated with Cycode CLI. Aborting scan...");
            return;
        }

        logger.Debug("[IaC] Start scanning paths: {0}", string.Join(", ", pathsToScan));
        await cliService.ScanPathsIacAsync(pathsToScan, onDemand, cancellationToken);
        logger.Debug("[IaC] Finish scanning paths: {0}", string.Join(", ", pathsToScan));
    }

    public async Task StartSastScanForCurrentProjectAsync() {
        string projectRoot = SolutionHelper.GetSolutionRootDirectory();
        if (projectRoot == null) {
            logger.Warn("Failed to get current project root. Aborting scan...");
            return;
        }

        await StartPathSastScanAsync(projectRoot, true);
    }

    public async Task StartPathSastScanAsync(string pathToScan, bool onDemand = false) {
        await StartPathSastScanAsync([pathToScan], onDemand);
    }

    public async Task StartPathSastScanAsync(List<string> pathsToScan, bool onDemand = false) {
        await WrapWithStatusCenterAsync(
            cancellationToken => StartPathSastScanInternalAsync(pathsToScan, onDemand, cancellationToken),
            "Cycode is scanning files for Code Security...",
            true
        );
    }

    private async Task StartPathSastScanInternalAsync(
        List<string> pathsToScan, bool onDemand = false, CancellationToken cancellationToken = default
    ) {
        if (!_pluginState.CliAuthed) {
            logger.Debug("Not authenticated with Cycode CLI. Aborting scan...");
            return;
        }

        logger.Debug("[SAST] Start scanning paths: {0}", string.Join(", ", pathsToScan));
        await cliService.ScanPathsSastAsync(pathsToScan, onDemand, cancellationToken);
        logger.Debug("[SAST] Finish scanning paths: {0}", string.Join(", ", pathsToScan));
    }

    private void ApplyDetectionIgnoreInUi(CliIgnoreType ignoreType, string value) {
        switch (ignoreType) {
            case CliIgnoreType.Value:
                scanResultsService.ExcludeResultsByValue(value);
                break;
            case CliIgnoreType.Path:
                break;
            case CliIgnoreType.Rule:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(ignoreType), ignoreType, null);
        }

        errorTaskCreatorService.RecreateAsync().FireAndForget();
        // since this ignore action could be done only from violation card screen, we need to close it
        toolWindowMessengerService.Send(new MessageEventArgs(MessengerCommand.BackToHomeScreen));
    }

    public async Task ApplyDetectionIgnoreAsync(
        CliScanType scanType, CliIgnoreType ignoreType, string value
    ) {
        await WrapWithStatusCenterAsync(
            cancellationToken => ApplyDetectionIgnoreInternalAsync(scanType, ignoreType, value, cancellationToken),
            "Cycode is applying ignores...",
            false // we do not allow to cancel this because we will instantly remove it from UI
        );
    }

    private async Task ApplyDetectionIgnoreInternalAsync(
        CliScanType scanType, CliIgnoreType ignoreType, string value, CancellationToken cancellationToken = default
    ) {
        // we are removing is from UI first to show how it's blazing fast and then apply it in the background
        ApplyDetectionIgnoreInUi(ignoreType, value);

        logger.Debug("[IGNORE] Start ignoring by {0}", ignoreType);
        await cliService.DoIgnoreAsync(scanType, ignoreType, value, cancellationToken);
        logger.Debug("[IGNORE] Finish ignoring by {0}", ignoreType);
    }
}