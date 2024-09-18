using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cycode.VisualStudio.Extension.Shared.Cli;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult.Iac;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult.Sast;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult.Sca;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO.ScanResult.Secret;
using Cycode.VisualStudio.Extension.Shared.DTO;
using Cycode.VisualStudio.Extension.Shared.Helpers;
using Cycode.VisualStudio.Extension.Shared.Sentry;
using Cycode.VisualStudio.Extension.Shared.Services.ErrorList;

namespace Cycode.VisualStudio.Extension.Shared.Services;

public interface ICliService {
    void ResetPluginCliState();

    Task<bool> HealthCheckAsync(CancellationToken cancellationToken = default);
    Task<bool> CheckAuthAsync(CancellationToken cancellationToken = default);
    Task<bool> DoAuthAsync(CancellationToken cancellationToken = default);

    Task ScanPathsSecretsAsync(
        List<string> paths, bool onDemand = true, CancellationToken cancellationToken = default
    );

    Task ScanPathsScaAsync(
        List<string> paths, bool onDemand = true, CancellationToken cancellationToken = default
    );

    Task ScanPathsIacAsync(
        List<string> paths, bool onDemand = true, CancellationToken cancellationToken = default
    );

    Task ScanPathsSastAsync(
        List<string> paths, bool onDemand = true, CancellationToken cancellationToken = default
    );

    Task<bool> DoIgnoreAsync(
        CliScanType scanType, CliIgnoreType ignoreType, string value, CancellationToken cancellationToken = default
    );
}

public class CliService(
    ILoggerService logger,
    IStateService stateService,
    IScanResultsService scanResultsService,
    IErrorTaskCreatorService errorTaskCreatorService
) : ICliService {
    private readonly CliWrapper _cli = new(SolutionHelper.GetSolutionRootDirectory);
    private readonly ExtensionState _pluginState = stateService.Load();

    public async Task<bool> HealthCheckAsync(CancellationToken cancellationToken = default) {
        CliResult<VersionResult> result = await _cli.ExecuteCommandAsync<VersionResult>(["version"], cancellationToken);
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

    public async Task<bool> CheckAuthAsync(CancellationToken cancellationToken = default) {
        CliResult<AuthCheckResult> result =
            await _cli.ExecuteCommandAsync<AuthCheckResult>(["auth", "check"], cancellationToken);
        CliResult<AuthCheckResult> processedResult = ProcessResult(result);

        if (processedResult is CliResult<AuthCheckResult>.Success successResult) {
            _pluginState.CliInstalled = true;
            _pluginState.CliAuthed = successResult.Result.Result;
            stateService.Save();

            if (!_pluginState.CliAuthed)
                ShowErrorNotification("You are not authenticated in Cycode. Please authenticate");

            AuthCheckResultData scopeData = successResult.Result.Data;
            if (scopeData != null) SentryInit.SetupScope(scopeData.UserId, scopeData.TenantId);

            return _pluginState.CliAuthed;
        }

        ResetPluginCliState();
        return false;
    }

    public async Task<bool> DoAuthAsync(CancellationToken cancellationToken = default) {
        CliResult<AuthResult> result = await _cli.ExecuteCommandAsync<AuthResult>(["auth"], cancellationToken);
        CliResult<AuthResult> processedResult = ProcessResult(result);

        if (processedResult is not CliResult<AuthResult>.Success successResult) return false;

        _pluginState.CliAuthed = successResult.Result.Result;
        stateService.Save();

        if (!_pluginState.CliAuthed) ShowErrorNotification("Authentication failed. Please try again");

        return _pluginState.CliAuthed;
    }

    public async Task ScanPathsSecretsAsync(
        List<string> paths, bool onDemand = true, CancellationToken cancellationToken = default
    ) {
        CliResult<SecretScanResult> results =
            await ScanPathsAsync<SecretScanResult>(paths, CliScanType.Secret, cancellationToken);
        if (results == null) {
            logger.Warn("Failed to scan Secret paths: {0}", string.Join(", ", paths));
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

    public async Task ScanPathsScaAsync(
        List<string> paths, bool onDemand = true, CancellationToken cancellationToken = default
    ) {
        CliResult<ScaScanResult> results =
            await ScanPathsAsync<ScaScanResult>(paths, CliScanType.Sca, cancellationToken);
        if (results == null) {
            logger.Warn("Failed to scan SCA paths: {0}", string.Join(", ", paths));
            return;
        }

        int detectionsCount = 0;
        if (results is CliResult<ScaScanResult>.Success successResult) {
            detectionsCount = successResult.Result.Detections.Count;
            scanResultsService.SetScaResults(successResult.Result);
            errorTaskCreatorService.RecreateAsync().FireAndForget();
        }

        ShowScanFileResultNotification(CliScanType.Sca, detectionsCount, onDemand);
    }

    public async Task ScanPathsIacAsync(
        List<string> paths, bool onDemand = true, CancellationToken cancellationToken = default
    ) {
        CliResult<IacScanResult> results =
            await ScanPathsAsync<IacScanResult>(paths, CliScanType.Iac, cancellationToken);
        if (results == null) {
            logger.Warn("Failed to scan IaC paths: {0}", string.Join(", ", paths));
            return;
        }

        int detectionsCount = 0;
        if (results is CliResult<IacScanResult>.Success successResult) {
            successResult = new CliResult<IacScanResult>.Success(new IacScanResult {
                Detections = FilterUnsupportedIacDetections(successResult.Result.Detections),
                Errors = successResult.Result.Errors
            });

            detectionsCount = successResult.Result.Detections.Count;
            scanResultsService.SetIacResults(successResult.Result);
            errorTaskCreatorService.RecreateAsync().FireAndForget();
        }

        ShowScanFileResultNotification(CliScanType.Iac, detectionsCount, onDemand);
    }

    public async Task ScanPathsSastAsync(
        List<string> paths, bool onDemand = true, CancellationToken cancellationToken = default
    ) {
        CliResult<SastScanResult> results =
            await ScanPathsAsync<SastScanResult>(paths, CliScanType.Sast, cancellationToken);
        if (results == null) {
            logger.Warn("Failed to scan SAST paths: {0}", string.Join(", ", paths));
            return;
        }

        int detectionsCount = 0;
        if (results is CliResult<SastScanResult>.Success successResult) {
            detectionsCount = successResult.Result.Detections.Count;
            scanResultsService.SetSastResults(successResult.Result);
            errorTaskCreatorService.RecreateAsync().FireAndForget();
        }

        ShowScanFileResultNotification(CliScanType.Sast, detectionsCount, onDemand);
    }

    public void ResetPluginCliState() {
        logger.Debug("Resetting plugin CLI state");

        _pluginState.CliAuthed = false;
        _pluginState.CliInstalled = false;
        _pluginState.CliVer = null;
        stateService.Save();
    }

    public async Task<bool> DoIgnoreAsync(
        CliScanType scanType, CliIgnoreType ignoreType, string value, CancellationToken cancellationToken = default
    ) {
        string[] args = [
            "ignore", "-t", scanType.ToString().ToLower(), MapIgnoreTypeToOptionName(ignoreType), value
        ];
        CliResult<object> result = await _cli.ExecuteCommandAsync<object>(args, cancellationToken);
        CliResult<object> processedResult = ProcessResult(result);
        return processedResult is CliResult<object>.Success;
    }

    private static List<IacDetection> FilterUnsupportedIacDetections(List<IacDetection> detections) {
        return detections.Where(detection => {
            IacDetectionDetails detectionDetails = detection.DetectionDetails;
            string filePath = detectionDetails.GetFilePath();

            // TF plans are virtual files that do not exist in the file system
            // "file_name": "1711298252-/Users/ilyasiamionau/projects/cycode/ilya-siamionau-payloads/tfplan.tf",
            // skip such detections

            try {
                string _ = Path.GetFullPath(filePath);
                return true;
            } catch (Exception) {
                return false;
            }
        }).ToList();
    }

    private static void ShowErrorNotification(string message) {
        VS.StatusBar.ShowMessageAsync(message).FireAndForget();
    }

    private static void ShowScanFileResultNotification(CliScanType scanType, int detectionsCount, bool onDemand) {
        string scanTypeString = CliUtilities.GetScanTypeDisplayName(scanType);

        string message = "";
        if (detectionsCount > 0)
            message = $"Cycode has detected {detectionsCount} {scanTypeString} issues in your file.";
        else if (onDemand) message = $"No {scanTypeString} issues were found.";

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
                if (result is not CliResult<T>.Success { Result: ScanResultBase } successResult) return result;

                List<CliError> errors = (successResult.Result as ScanResultBase)?.Errors;
                if (errors == null || errors.Count == 0) return result;

                foreach (CliError error in errors) ShowErrorNotification(error.Message);

                // we trust that it is not possible to have both errors and detections
                return null;
        }
    }

    private static string[] GetCliScanOptions(CliScanType scanType) {
        List<string> options = [];

        if (scanType != CliScanType.Sca) return options.ToArray();

        // SCA specific options to performs it faster
        options.Add("--sync");
        options.Add("--no-restore");

        return options.ToArray();
    }

    private async Task<CliResult<T>> ScanPathsAsync<T>(
        List<string> paths, CliScanType scanType, CancellationToken cancellationToken = default
    ) {
        List<string> isolatedPaths = paths.Select(path => $"\"{path}\"").ToList();
        string scanTypeString = scanType.ToString().ToLower();
        CliResult<T> result = await _cli.ExecuteCommandAsync<T>(
            new[] { "scan", "-t", scanTypeString }.Concat(GetCliScanOptions(scanType)).Concat(new[] { "path" })
                .Concat(isolatedPaths).ToArray(),
            cancellationToken
        );

        return ProcessResult(result);
    }

    private static string MapIgnoreTypeToOptionName(CliIgnoreType type) {
        return type switch {
            CliIgnoreType.Value => "--by-value",
            CliIgnoreType.Rule => "--by-rule",
            CliIgnoreType.Path => "--by-path",
            _ => throw new ArgumentException("Invalid CliIgnoreType")
        };
    }
}