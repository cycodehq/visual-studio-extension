using System.Threading.Tasks;
using Cycode.VisualStudio.Extension.Shared.Cli;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO;
using Cycode.VisualStudio.Extension.Shared.DTO;

namespace Cycode.VisualStudio.Extension.Shared.Services;

using TaskCancelledCallback = Func<bool>;

public class CliService(ILoggerService logger, IStateService stateService) : ICliService {
    private readonly ExtensionState _pluginState = stateService.Load();
    private static readonly PluginSettings _pluginSettings = new();
    private readonly CliWrapper _cli = new(_pluginSettings.CliPath, GetProjectRootDirectory());

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
                // TODO(MarshalX): implement ScanResultBase
                // if (result is CliResult<T>.Success successResult && successResult.Result is ScanResultBase) {
                //     var errors = successResult.Result.Errors;
                //     if (errors.Count == 0) {
                //         return successResult;
                //     }
                //
                //     foreach (var error in errors) {
                //         ShowErrorNotification(error.Message);
                //     }
                //
                //     return null;
                // }

                return result;
        }
    }

    public async Task<bool> HealthCheckAsync(TaskCancelledCallback cancelledCallback = null) {
        CliResult<VersionResult> result =
            await _cli.ExecuteCommandAsync<VersionResult>(["version"], cancelledCallback: cancelledCallback);
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
            await _cli.ExecuteCommandAsync<AuthCheckResult>(["auth", "check"], cancelledCallback: cancelledCallback);
        CliResult<AuthCheckResult> processedResult = ProcessResult(result);

        if (processedResult is CliResult<AuthCheckResult>.Success successResult) {
            _pluginState.CliInstalled = true;
            _pluginState.CliAuthed = successResult.Result.Result;
            stateService.Save();

            if (!_pluginState.CliAuthed) {
                ShowErrorNotification("You are not authenticated in Cycode. Please authenticate");
            }

            return _pluginState.CliAuthed;
        }

        ResetPluginCliState();
        return false;
    }
}