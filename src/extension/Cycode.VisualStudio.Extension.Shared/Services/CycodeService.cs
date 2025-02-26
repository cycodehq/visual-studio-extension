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

using PerformScanFunc = Func<List<string>, bool, CancellationToken, Task>;

public interface ICycodeService {
    Task InstallCliIfNeededAndCheckAuthenticationAsync();
    Task StartAuthAsync();
    Task StartProjectScanAsync(CliScanType scanType, bool onDemand = false);
    Task StartPathScanAsync(CliScanType scanType, List<string> pathsToScan, bool onDemand = false);

    Task ApplyDetectionIgnoreAsync(
        CliScanType scanType, CliIgnoreType ignoreType, string value
    );

    Task GetAiRemediationAsync(string detectionId, Action<AiRemediationResultData> onSuccess);
}

public class CycodeService(
    ILoggerService logger,
    ITemporaryStateService tempState,
    ICliDownloadService cliDownloadService,
    ICliService cliService,
    IToolWindowMessengerService toolWindowMessengerService,
    IScanResultsService scanResultsService,
    IErrorTaskCreatorService errorTaskCreatorService
) : ICycodeService {
    private readonly IDictionary<CliScanType, PerformScanFunc> _scanFunctions =
        new Dictionary<CliScanType, PerformScanFunc> {
            { CliScanType.Secret, cliService.ScanPathsSecretsAsync },
            { CliScanType.Sca, cliService.ScanPathsScaAsync },
            { CliScanType.Iac, cliService.ScanPathsIacAsync },
            { CliScanType.Sast, cliService.ScanPathsSastAsync }
        };

    private readonly IDictionary<CliScanType, string> _scanStatusLabels = new Dictionary<CliScanType, string> {
        { CliScanType.Secret, "Cycode is scanning files for hardcoded secrets..." },
        { CliScanType.Sca, "Cycode is scanning files for package vulnerabilities..." },
        { CliScanType.Iac, "Cycode is scanning files for Infrastructure As Code..." },
        { CliScanType.Sast, "Cycode is scanning files for Code Security..." }
    };

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
        if (tempState.CliAuthed) {
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
            canBeCanceled: false
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

            await cliService.SyncStatusAsync(cancellationToken);

            UpdateToolWindowDependingOnState();
        } catch (Exception e) {
            logger.Error(e, "Failed to check Cycode CLI health and authentication");
        }
    }

    public async Task StartAuthAsync() {
        await WrapWithStatusCenterAsync(
            StartAuthInternalAsync,
            "Authenticating to Cycode...",
            canBeCanceled: true
        );
    }

    private async Task StartAuthInternalAsync(CancellationToken cancellationToken) {
        if (!tempState.CliAuthed) {
            logger.Debug("[AUTH] Start authing");
            await cliService.DoAuthAsync(cancellationToken);
            await cliService.SyncStatusAsync(cancellationToken);
            UpdateToolWindowDependingOnState();
            logger.Debug("[AUTH] Finish authing");
        } else {
            logger.Debug("Already authenticated with Cycode CLI");
        }
    }

    public async Task StartProjectScanAsync(CliScanType scanType, bool onDemand = true) {
        string projectRoot = SolutionHelper.GetSolutionRootDirectory();
        if (projectRoot is null) {
            logger.Warn("Failed to get current project root. Aborting scan...");
            return;
        }

        await StartPathScanAsync(scanType, [projectRoot], onDemand);
    }

    public async Task StartPathScanAsync(CliScanType scanType, List<string> pathsToScan, bool onDemand = false) {
        if (!tempState.CliAuthed) {
            logger.Debug("Not authenticated with Cycode CLI. Aborting scan...");
            return;
        }

        if (!_scanStatusLabels.TryGetValue(scanType, out string label)) {
            logger.Error("Status label for {0} does not exist", scanType);
            return;
        }

        await WrapWithStatusCenterAsync(
            async cancellationToken => {
                logger.Debug("[{0}] Start scanning paths: {1}", scanType, string.Join(", ", pathsToScan));

                if (!_scanFunctions.TryGetValue(scanType, out PerformScanFunc performScanFunc)) {
                    logger.Error("Scan function for {0} does not exist", scanType);
                    return;
                }

                await performScanFunc.Invoke(pathsToScan, onDemand, cancellationToken);

                logger.Debug("[{0}] Finish scanning paths: {1}", scanType, string.Join(", ", pathsToScan));
            },
            label,
            canBeCanceled: true
        );
    }

    private async Task ApplyDetectionIgnoreInUiAsync(CliIgnoreType ignoreType, string value) {
        switch (ignoreType) {
            case CliIgnoreType.Value:
                scanResultsService.ExcludeResultsByValue(value);
                break;
            case CliIgnoreType.Path:
            case CliIgnoreType.Rule:
                break;
            case CliIgnoreType.Cve:
                scanResultsService.ExcludeResultsByCve(value);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(ignoreType), ignoreType, null);
        }

        await errorTaskCreatorService.RecreateAsync();
        // since this ignore action could be done only from violation card screen, we need to close it
        toolWindowMessengerService.Send(new MessageEventArgs(MessengerCommand.BackToHomeScreen));
    }

    public async Task ApplyDetectionIgnoreAsync(
        CliScanType scanType, CliIgnoreType ignoreType, string value
    ) {
        await WrapWithStatusCenterAsync(
            cancellationToken => ApplyDetectionIgnoreInternalAsync(scanType, ignoreType, value, cancellationToken),
            "Cycode is applying ignores...",
            canBeCanceled: false // we do not allow to cancel this because we will instantly remove it from UI
        );
    }

    private async Task ApplyDetectionIgnoreInternalAsync(
        CliScanType scanType, CliIgnoreType ignoreType, string value, CancellationToken cancellationToken = default
    ) {
        // we are removing is from UI first to show how it's blazing fast and then apply it in the background
        await ApplyDetectionIgnoreInUiAsync(ignoreType, value);

        logger.Debug("[IGNORE] Start ignoring by {0}", ignoreType);
        await cliService.DoIgnoreAsync(scanType, ignoreType, value, cancellationToken);
        logger.Debug("[IGNORE] Finish ignoring by {0}", ignoreType);
    }

    public async Task GetAiRemediationAsync(string detectionId, Action<AiRemediationResultData> onSuccess) {
        await WrapWithStatusCenterAsync(
            async cancellationToken => {
                logger.Debug("[AI REMEDIATION] Start generating remediation for {0}", detectionId);
                AiRemediationResultData aiRemediation = await cliService.GetAiRemediationAsync(detectionId, cancellationToken);
                logger.Debug("[AI REMEDIATION] Finish generating remediation for {0}", detectionId);
                if (aiRemediation != null)
                    onSuccess(aiRemediation);
            },
            "Cycode is generating AI remediation...",
            canBeCanceled: true
        );
    }
}