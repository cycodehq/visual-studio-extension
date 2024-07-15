using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cycode.VisualStudio.Extension.Shared.Cli;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult.Secret;
using Cycode.VisualStudio.Extension.Shared.DTO;
using Cycode.VisualStudio.Extension.Shared.Sentry;
using Cycode.VisualStudio.Extension.Shared.Services.ErrorList;

namespace Cycode.VisualStudio.Extension.Shared.Services;

using TaskCancelledCallback = Func<bool>;

public class CliService(
    ILoggerService logger,
    IStateService stateService,
    IScanResultsService scanResultsService,
    IErrorTaskCreatorService errorTaskCreatorService
) : ICliService {
    private readonly ExtensionState _pluginState = stateService.Load();
    private readonly CliWrapper _cli = new(GetProjectRootDirectory());

    private static string GetProjectRootDirectory() {
        return VS.Solutions.GetCurrentSolution()?.FullPath;
    }


    private void ResetPluginCliState() {
        logger.Debug("Resetting plugin CLI state");

        _pluginState.CliAuthed = false;
        _pluginState.CliInstalled = false;
        _pluginState.CliVer = null;
        stateService.Save();
    }

    private static void ShowErrorNotification(string message) {
        VS.StatusBar.ShowMessageAsync(message).FireAndForget();
    }

    private static void ShowScanFileResultNotification(CliScanType scanType, int detectionsCount, bool onDemand) {
        string scanTypeString = CliUtilities.GetScanTypeDisplayName(scanType);

        string message = "";
        if (detectionsCount > 0) {
            message = $"Cycode has detected {detectionsCount} {scanTypeString} issues in your file.";
        } else if (onDemand) {
            message = $"No {scanTypeString} issues were found.";
        }

        VS.StatusBar.ShowMessageAsync(message).FireAndForget();
    }

    private static CliResult<T> ProcessResult<T>(CliResult<T> result) {
        switch (result) {
            case CliResult<T>.Error errorResult:
                ShowErrorNotification(errorResult.Result.Message);
                return null;
            case CliResult<T>.Panic { ExitCode: ExitCode.Termination }:
                return null;
            case CliResult<T>.Panic panicResult:
                ShowErrorNotification(panicResult.ErrorMessage);
                return null;
            default:
                if (result is not CliResult<T>.Success { Result: ScanResultBase } successResult) {
                    return result;
                }

                List<CliError> errors = (successResult.Result as ScanResultBase)?.Errors;
                if (errors == null || errors.Count == 0) {
                    return result;
                }

                foreach (CliError error in errors) {
                    ShowErrorNotification(error.Message);
                }

                // we trust that it is not possible to have both errors and detections
                return null;
        }
    }

    public async Task<bool> HealthCheckAsync(TaskCancelledCallback cancelledCallback = null) {
        CliResult<VersionResult> result = await _cli.ExecuteCommandAsync<VersionResult>(["version"], cancelledCallback);
        CliResult<VersionResult> processedResult = ProcessResult(result);

        if (processedResult is CliResult<VersionResult>.Success successResult) {
            _pluginState.CliInstalled = true;
            _pluginState.CliVer = successResult.Result.Version;
            stateService.Save();
            return true;
        }

        ResetPluginCliState();
        return false;
    }

    public async Task<bool> CheckAuthAsync(TaskCancelledCallback cancelledCallback = null) {
        CliResult<AuthCheckResult> result =
            await _cli.ExecuteCommandAsync<AuthCheckResult>(["auth", "check"], cancelledCallback);
        CliResult<AuthCheckResult> processedResult = ProcessResult(result);

        if (processedResult is CliResult<AuthCheckResult>.Success successResult) {
            _pluginState.CliInstalled = true;
            _pluginState.CliAuthed = successResult.Result.Result;
            stateService.Save();

            if (!_pluginState.CliAuthed) {
                ShowErrorNotification("You are not authenticated in Cycode. Please authenticate");
            }

            SentryInit.SetupScope(
                successResult.Result.Data.UserId,
                successResult.Result.Data.TenantId
            );

            return _pluginState.CliAuthed;
        }

        ResetPluginCliState();
        return false;
    }

    public async Task<bool> DoAuthAsync(TaskCancelledCallback cancelledCallback = null) {
        CliResult<AuthResult> result = await _cli.ExecuteCommandAsync<AuthResult>(["auth"], cancelledCallback);
        CliResult<AuthResult> processedResult = ProcessResult(result);

        if (processedResult is not CliResult<AuthResult>.Success successResult) {
            return false;
        }

        _pluginState.CliAuthed = successResult.Result.Result;
        stateService.Save();

        if (!_pluginState.CliAuthed) {
            ShowErrorNotification("Authentication failed. Please try again");
        }

        return _pluginState.CliAuthed;
    }

    private static string[] GetCliScanOptions(CliScanType scanType) {
        // TODO(MarshalX): for Sca
        return [];
    }

    private async Task<CliResult<T>> ScanPathsAsync<T>(
        List<string> paths, CliScanType scanType, TaskCancelledCallback cancelledCallback = null
    ) {
        List<string> isolatedPaths = paths.Select(path => $"\"{path}\"").ToList();
        string scanTypeString = scanType.ToString().ToLower();
        CliResult<T> result = await _cli.ExecuteCommandAsync<T>(
            new[] { "scan", "-t", scanTypeString }.Concat(GetCliScanOptions(scanType)).Concat(new[] { "path" })
                .Concat(isolatedPaths).ToArray(),
            cancelledCallback
        );

        return ProcessResult(result);
    }

    public async Task ScanPathsSecretsAsync(
        List<string> paths, bool onDemand = true, TaskCancelledCallback cancelledCallback = null
    ) {
        CliResult<SecretScanResult> results =
            await ScanPathsAsync<SecretScanResult>(paths, CliScanType.Secret, cancelledCallback);
        if (results == null) {
            logger.Warn("Failed to scan paths: {0}", string.Join(", ", paths));
            return;
        }

        int detectionsCount = 0;
        if (results is CliResult<SecretScanResult>.Success successResult) {
            detectionsCount = successResult.Result.Detections.Count;
            scanResultsService.SetSecretResults(successResult.Result);
            errorTaskCreatorService.RecreateAsync().FireAndForget();
        }

        ShowScanFileResultNotification(CliScanType.Secret, detectionsCount, onDemand);
    }
}