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

    Task SyncStatusAsync(CancellationToken cancellationToken = default);
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
    
    Task<AiRemediationResultData> GetAiRemediationAsync(string detectionId, CancellationToken cancellationToken = default);
}

public class CliService(
    ILoggerService logger,
    IStateService stateService,
    ITemporaryStateService tempState,
    IScanResultsService scanResultsService,
    IErrorTaskCreatorService errorTaskCreatorService
) : ICliService {
    private readonly CliWrapper _cli = new(SolutionHelper.GetSolutionRootDirectory);
    private readonly ExtensionState _pluginState = stateService.Load();

    public async Task SyncStatusAsync(CancellationToken cancellationToken = default) {
        CliResult<StatusResult> result = await _cli.ExecuteCommandAsync<StatusResult>(["status"], cancellationToken);
        CliResult<StatusResult>.Success processedResult = ProcessResult(result);

        if (processedResult == null) {
            ResetPluginCliState();
            return;
        }

        tempState.CliInstalled = true;
        tempState.CliAuthed = processedResult.Result.IsAuthenticated;
        tempState.CliStatus = processedResult.Result;

        _pluginState.CliVer = processedResult.Result.Version;
        stateService.Save();

        if (!tempState.CliAuthed)
            ShowErrorNotification("You are not authenticated in Cycode. Please authenticate");
        else {
            if (processedResult.Result.UserId != null && processedResult.Result.TenantId != null)
                SentryInit.SetupScope(processedResult.Result.UserId, processedResult.Result.TenantId);
        }
    }

    public async Task<bool> DoAuthAsync(CancellationToken cancellationToken = default) {
        CliResult<AuthResult> result = await _cli.ExecuteCommandAsync<AuthResult>(["auth"], cancellationToken);
        CliResult<AuthResult>.Success processedResult = ProcessResult(result);
        if (processedResult == null) return false;

        tempState.CliAuthed = processedResult.Result.Result;

        if (!tempState.CliAuthed) ShowErrorNotification("Authentication failed. Please try again");

        return tempState.CliAuthed;
    }

    public async Task ScanPathsSecretsAsync(
        List<string> paths, bool onDemand = true, CancellationToken cancellationToken = default
    ) {
        CliResult<SecretScanResult>.Success results =
            await ScanPathsAsync<SecretScanResult>(paths, CliScanType.Secret, cancellationToken);
        if (results == null) {
            logger.Warn("Failed to scan Secret paths: {0}", string.Join(", ", paths));
            return;
        }

        await ProcessCliScanResultAsync(CliScanType.Secret, results.Result.Detections, onDemand);
    }

    public async Task ScanPathsScaAsync(
        List<string> paths, bool onDemand = true, CancellationToken cancellationToken = default
    ) {
        CliResult<ScaScanResult>.Success results =
            await ScanPathsAsync<ScaScanResult>(paths, CliScanType.Sca, cancellationToken);
        if (results == null) {
            logger.Warn("Failed to scan SCA paths: {0}", string.Join(", ", paths));
            return;
        }

        await ProcessCliScanResultAsync(CliScanType.Sca, results.Result.Detections, onDemand);
    }

    public async Task ScanPathsIacAsync(
        List<string> paths, bool onDemand = true, CancellationToken cancellationToken = default
    ) {
        CliResult<IacScanResult>.Success results =
            await ScanPathsAsync<IacScanResult>(paths, CliScanType.Iac, cancellationToken);
        if (results == null) {
            logger.Warn("Failed to scan IaC paths: {0}", string.Join(", ", paths));
            return;
        }

        results = new CliResult<IacScanResult>.Success(new IacScanResult {
            Detections = FilterUnsupportedIacDetections(results.Result.Detections),
            Errors = results.Result.Errors
        });

        await ProcessCliScanResultAsync(CliScanType.Iac, results.Result.Detections, onDemand);
    }

    public async Task ScanPathsSastAsync(
        List<string> paths, bool onDemand = true, CancellationToken cancellationToken = default
    ) {
        CliResult<SastScanResult>.Success results =
            await ScanPathsAsync<SastScanResult>(paths, CliScanType.Sast, cancellationToken);
        if (results == null) {
            logger.Warn("Failed to scan SAST paths: {0}", string.Join(", ", paths));
            return;
        }

        await ProcessCliScanResultAsync(CliScanType.Sast, results.Result.Detections, onDemand);
    }

    public void ResetPluginCliState() {
        logger.Debug("Resetting plugin CLI state");

        tempState.CliAuthed = false;
        tempState.CliInstalled = false;
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
        return processedResult != null;
    }

    private async Task ProcessCliScanResultAsync<T>(CliScanType scanType, List<T> detections, bool onDemand) {
        scanResultsService.SetDetections(scanType, detections);
        await errorTaskCreatorService.RecreateAsync();
        ShowScanFileResultNotification(scanType, detections.Count, onDemand);
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

    private CliResult<T>.Success ProcessResult<T>(CliResult<T> result) {
        switch (result) {
            case CliResult<T>.Error errorResult:
                logger.Info("[ProcessResult] Error result. Showing error notification");
                ShowErrorNotification(errorResult.Result.Message);
                return null;
            case CliResult<T>.Panic { ExitCode: ExitCode.Termination }:
                logger.Info("[ProcessResult] User termination panic result");
                return null;
            case CliResult<T>.Panic panicResult:
                logger.Info("[ProcessResult] Panic result. Showing error notification");
                ShowErrorNotification(panicResult.ErrorMessage);
                return null;
            case CliResult<T>.Success { Result: ScanResultBase } successScanResult:
                logger.Info("[ProcessResult] Success scan result");
                List<CliError> errors = (successScanResult.Result as ScanResultBase)?.Errors;
                if (errors == null || errors.Count == 0) return successScanResult;

                foreach (CliError error in errors) ShowErrorNotification(error.Message);

                // we trust that it is not possible to have both errors and detections
                return null;
            case CliResult<T>.Success successResult:
                logger.Info("[ProcessResult] Success result (not scan one)");
                return successResult;
            default:
                ShowErrorNotification("Unknown CliResult type");
                logger.Error("[ProcessResult] Unknown CliResult type: {0}", result.GetType().Name);
                return null;
        }
    }

    private static string[] GetCliScanOptions(CliScanType scanType) {
        List<string> options = [];

        if (scanType != CliScanType.Sast) options.Add("--sync");

        if (scanType == CliScanType.Sca) options.Add("--no-restore");

        return options.ToArray();
    }

    private async Task<CliResult<T>.Success> ScanPathsAsync<T>(
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
            CliIgnoreType.Cve => "--by-cve",
            _ => throw new ArgumentException("Invalid CliIgnoreType")
        };
    }
    
    public async Task<AiRemediationResultData> GetAiRemediationAsync(string detectionId, CancellationToken cancellationToken = default) {
        CliResult<AiRemediationResult> result = await _cli.ExecuteCommandAsync<AiRemediationResult>(["ai_remediation", detectionId], cancellationToken);
        CliResult<AiRemediationResult>.Success processedResult = ProcessResult(result);
        if (processedResult == null) {
            logger.Warn("Failed to get AI remediation for detection: {0}", detectionId);
            return null;
        }

        if (processedResult.Result.Result && processedResult.Result.Data != null) return processedResult.Result.Data;

        logger.Warn("AI remediation is not available for detection: {0}", detectionId);
        ShowErrorNotification("AI remediation is not available for this detection");
        return null;
    }
}