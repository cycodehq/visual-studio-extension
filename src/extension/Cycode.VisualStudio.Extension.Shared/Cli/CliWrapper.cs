using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cycode.VisualStudio.Extension.Shared.JsonContractResolvers;
using Cycode.VisualStudio.Extension.Shared.Services;
using Newtonsoft.Json;

namespace Cycode.VisualStudio.Extension.Shared.Cli;

public class CliWrapper(string workDirectory = null) {
    private readonly ILoggerService _logger = ServiceLocator.GetService<ILoggerService>();

    private string[] _defaultCliArgs = [];

    private readonly JsonSerializerSettings _jsonSettings = new() {
        ContractResolver = new SnakeCasePropertyNamesContractResolver(),
    };

    private async Task<string[]> GetDefaultCliArgsAsync() {
        // cache
        if (_defaultCliArgs.Length > 0) return _defaultCliArgs;

        _defaultCliArgs = new[] {
            "-o", "json",
            "--user-agent", await UserAgent.GetUserAgentEscapedAsync()
        };

        _logger.Debug("Default CLI args: {0}", string.Join(" ", _defaultCliArgs));

        return _defaultCliArgs;
    }

    public async Task<CliResult<T>> ExecuteCommandAsync<T>(string[] arguments, Func<bool> cancelledCallback = null) {
        General general = await General.GetLiveInstanceAsync();

        ProcessStartInfo startInfo = new() {
            FileName = general.CliPath,
            Arguments = string.Join(" ", (await GetDefaultCliArgsAsync()).Concat(arguments)),
            WorkingDirectory = workDirectory,
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

        string[] additionalArgs = general.CliAdditionalParams.Split(
            new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries
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
            if (cancelledCallback != null && cancelledCallback()) {
                process.Kill();
                return new CliResult<T>.Panic(ExitCode.Termination, "Execution was canceled");
            }

            await Task.Delay(1000);
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
            _logger.Warn(e, "Failed to deserialize CLI result. Result: {0}", stdout);

            try {
                CliError cliError = JsonConvert.DeserializeObject<CliError>(stdout, _jsonSettings);
                if (cliError == null) {
                    throw new Exception("Deserialized CLI Error is null");
                }

                return new CliResult<T>.Error(cliError);
            } catch (Exception ex) {
                _logger.Error(ex, "Failed to parse ANY CLI output. Output: {0}", stdout);
                return new CliResult<T>.Panic(exitCode, stderr);
            }
        }
    }
}