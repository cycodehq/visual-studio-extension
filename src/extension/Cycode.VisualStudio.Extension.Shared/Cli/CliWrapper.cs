using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cycode.VisualStudio.Extension.Shared.JsonContractResolvers;
using Cycode.VisualStudio.Extension.Shared.Services;
using Newtonsoft.Json;

namespace Cycode.VisualStudio.Extension.Shared.Cli;

public class CliWrapper(Func<string> getWorkDirectory) {
    private readonly ILoggerService _logger = ServiceLocator.GetService<ILoggerService>();

    private string[] _defaultCliArgs = [];

    private readonly JsonSerializerSettings _jsonSettings = new() {
        ContractResolver = new SnakeCasePropertyNamesContractResolver(),
    };

    private async Task<string[]> GetDefaultCliArgsAsync() {
        // cache
        if (_defaultCliArgs.Length > 0) return _defaultCliArgs;

        _defaultCliArgs = [
            "-o", "json",
            "--user-agent", await UserAgent.GetUserAgentEscapedAsync()
        ];

        _logger.Debug("Default CLI args: {0}", string.Join(" ", _defaultCliArgs));

        return _defaultCliArgs;
    }

    public async Task<CliResult<T>> ExecuteCommandAsync<T>(
        string[] arguments,
        CancellationToken cancellationToken = default
    ) {
        General general = await General.GetLiveInstanceAsync();

        ProcessStartInfo startInfo = new() {
            FileName = general.CliPath,
            Arguments = string.Join(" ", (await GetDefaultCliArgsAsync()).Concat(arguments)),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8,
            UseShellExecute = false,
            CreateNoWindow = true,
            EnvironmentVariables = {
                ["CYCODE_API_URL"] = general.CliApiUrl,
                ["CYCODE_APP_URL"] = general.CliAppUrl
            }
        };

        string workingDirectory = getWorkDirectory();
        if (Directory.Exists(workingDirectory)) {
            startInfo.WorkingDirectory = workingDirectory;
        }

        string[] additionalArgs = general.CliAdditionalParams.Split(
            [' '], StringSplitOptions.RemoveEmptyEntries
        );
        if (additionalArgs.Length > 0) {
            startInfo.Arguments = $"{string.Join(" ", additionalArgs)} {startInfo.Arguments}";
        }

        _logger.Debug("Executing CLI command: {0} {1}", startInfo.FileName, startInfo.Arguments);

        Process process = new() { StartInfo = startInfo, EnableRaisingEvents = true };

        TaskCompletionSource<int> tcs = new();

        StringBuilder output = new();
        StringBuilder error = new();

        process.OutputDataReceived += (_, e) => output.AppendLine(e.Data);
        process.ErrorDataReceived += (_, e) => {
            if (e.Data == null) return;
            _logger.Debug(e.Data.ToString().Trim());
            error.AppendLine(e.Data);
        };
        process.Exited += (_, _) => tcs.SetResult(process.ExitCode);

        try {
            process.Start();
        } catch (Exception e) {
            _logger.Error(e, "Failed to start CLI process");
            return new CliResult<T>.Panic(ExitCode.Termination, "Failed to start CLI process");
        }

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        while (!process.HasExited) {
            try {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Delay(1000, cancellationToken);
            } catch (Exception e) when (e is ObjectDisposedException or OperationCanceledException) {
                process.Kill();
                _logger.Debug("CLI Execution was canceled by user");
                return new CliResult<T>.Panic(ExitCode.Termination, "Execution was canceled");
            }
        }

        int exitCode = await tcs.Task;
        string stdout = output.ToString().Trim();
        string stderr = error.ToString().Trim();

        _logger.Debug("CLI command exited with code {0}; stdout: {1};", exitCode, stdout);

        if (exitCode == ExitCode.AbnormalTermination) {
            ErrorCode errorCode = ErrorHandling.DetectErrorCode(stderr);
            if (errorCode == ErrorCode.Unknown) {
                _logger.Error("Unknown error with abnormal termination: {0}; {1}", stdout, stderr);
            } else {
                return new CliResult<T>.Panic(exitCode, ErrorHandling.GetUserFriendlyCliErrorMessage(errorCode));
            }
        }

        if (typeof(T) == typeof(void)) {
            _logger.Debug("CLI command executed successfully with no result.");
            return new CliResult<T>.Success((T)(object)null);
        }

        try {
            T cliResult = JsonConvert.DeserializeObject<T>(stdout, _jsonSettings);
            if (cliResult == null) {
                throw new Exception("Deserialized CLI Result is null");
            }

            return new CliResult<T>.Success(cliResult);
        } catch (Exception e) {
            _logger.Info(e, "Failed to deserialize CliResult<T>.Success");

            try {
                CliError cliError = JsonConvert.DeserializeObject<CliError>(stdout, _jsonSettings);
                if (cliError == null) {
                    throw new Exception("Deserialized CliError is null");
                }

                _logger.Info("Successfully deserialized to CliResult<T>.Error");
                return new CliResult<T>.Error(cliError);
            } catch (Exception ex) {
                _logger.Error(ex, "Failed to parse any output. Returning CliResult<T>.Panic");
                return new CliResult<T>.Panic(exitCode, stderr);
            }
        }
    }
}