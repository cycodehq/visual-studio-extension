namespace Cycode.VisualStudio.Extension.Shared.Cli;

public abstract class CliResult<T> {
    public class Success(T result) : CliResult<T> {
        public T Result { get; } = result;
    }

    public class Error(CliError result) : CliResult<T> {
        public CliError Result { get; } = result;
    }

    public class Panic(int exitCode, string errorMessage) : CliResult<T> {
        public int ExitCode { get; } = exitCode;
        public string ErrorMessage { get; } = errorMessage;
    }
}