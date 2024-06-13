using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Cycode.VisualStudio.Extension.Shared.Cli;

public class CliWrapper(string executablePath, string workDirectory = null) {
    private string[] _defaultCliArgs = [];

    private readonly JsonSerializerSettings _jsonSettings = new() {
        // TODO(MarshalX): add naming strategy?
        MissingMemberHandling = MissingMemberHandling.Ignore
    };

    private async Task<string[]> GetDefaultCliArgsAsync() {
        // cache
        if (_defaultCliArgs.Length > 0) return _defaultCliArgs;

        _defaultCliArgs = new[] {
            "-o", "json",
            "--user-agent", await UserAgent.GetUserAgentAsync()
        };

        return _defaultCliArgs;
    }

    private readonly PluginSettings _pluginSettings = new();

    public async Task<CliResult<T>> ExecuteCommandAsync<T>(string[] arguments, Func<bool> cancelledCallback = null) {
        ProcessStartInfo startInfo = new() {
            FileName = executablePath,
            Arguments = string.Join(" ", (await GetDefaultCliArgsAsync()).Concat(arguments)),
            WorkingDirectory = workDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8,
            UseShellExecute = false,
            CreateNoWindow = true,
            EnvironmentVariables = {
                ["CYCODE_API_URL"] = _pluginSettings.CliApiUrl,
                ["CYCODE_APP_URL"] = _pluginSettings.CliAppUrl
            }
        };

        string[] additionalArgs = _pluginSettings.CliAdditionalParams
            .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (additionalArgs.Length > 0) startInfo.Arguments += " " + string.Join(" ", additionalArgs);

        Process process = new() { StartInfo = startInfo, EnableRaisingEvents = true };

        TaskCompletionSource<int> tcs = new();

        StringBuilder output = new();
        StringBuilder error = new();

        process.OutputDataReceived += (_, e) => output.AppendLine(e.Data);
        process.ErrorDataReceived += (_, e) => error.AppendLine(e.Data);
        process.Exited += (_, _) => tcs.SetResult(process.ExitCode);

        process.Start();
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

        if (exitCode == ExitCode.AbnormalTermination) {
            ErrorCode errorCode = ErrorHandling.DetectErrorCode(stderr);
            if (errorCode == ErrorCode.Unknown) {
                // TODO(MarshalX): log unknown error
            } else {
                return new CliResult<T>.Panic(exitCode, ErrorHandling.GetUserFriendlyCliErrorMessage(errorCode));
            }
        }

        if (typeof(T) == typeof(void)) return new CliResult<T>.Success((T)(object)null);

        try {
            T result = JsonConvert.DeserializeObject<T>(stdout, _jsonSettings);
            return new CliResult<T>.Success(result);
        } catch (Exception) {
            try {
                CliError cliError = JsonConvert.DeserializeObject<CliError>(stdout, _jsonSettings);
                return new CliResult<T>.Error(cliError);
            } catch {
                return new CliResult<T>.Panic(exitCode, stderr);
            }
        }
    }
}