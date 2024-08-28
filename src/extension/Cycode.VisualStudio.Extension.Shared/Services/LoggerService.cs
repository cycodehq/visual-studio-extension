using Cycode.VisualStudio.Extension.Shared.DTO;
using Microsoft.VisualStudio.Shell.Interop;
using Sentry;

namespace Cycode.VisualStudio.Extension.Shared.Services;

public class LoggerService : ILoggerService {
    private const string _paneTitle = "Cycode";

    private static readonly Guid _paneGuid = new("AF772BC0-5CBF-4A44-AE52-7F428C5659A1");
    private IVsOutputWindowPane _pane;

    public void Initialize() {
        InitializePane();
    }

    public void Debug(string message, params object[] args) {
        Log(LogLevel.Debug, null, message, args);
    }

    public void Debug(Exception exception, string message, params object[] args) {
        Log(LogLevel.Debug, exception, message, args);
    }

    public void Debug(string message, Exception exception, params object[] args) {
        Log(LogLevel.Debug, exception, message, args);
    }

    public void Error(string message, params object[] args) {
        Log(LogLevel.Error, null, message, args);
    }

    public void Error(Exception exception, string message, params object[] args) {
        Log(LogLevel.Error, exception, message, args);
    }

    public void Error(string message, Exception exception, params object[] args) {
        Log(LogLevel.Error, exception, message, args);
    }

    public void Warn(string message, params object[] args) {
        Log(LogLevel.Warning, null, message, args);
    }

    public void Warn(Exception exception, string message, params object[] args) {
        Log(LogLevel.Warning, exception, message, args);
    }

    public void Warn(string message, Exception exception, params object[] args) {
        Log(LogLevel.Warning, exception, message, args);
    }

    public void Info(string message, params object[] args) {
        Log(LogLevel.Info, null, message, args);
    }

    public void Info(string message, Exception exception, params object[] args) {
        Log(LogLevel.Info, exception, message, args);
    }

    public void Info(Exception exception, string message, params object[] args) {
        Log(LogLevel.Info, exception, message, args);
    }

    private void InitializePane() {
        ThreadHelper.ThrowIfNotOnUIThread();

        if (Package.GetGlobalService(typeof(SVsOutputWindow)) is not IVsOutputWindow outputWindow)
            throw new InvalidOperationException("Cannot get SVsOutputWindow service.");

        outputWindow.CreatePane(_paneGuid, _paneTitle, 1, 1);
        outputWindow.GetPane(_paneGuid, out _pane);
    }

    private static string GetLogLevelPrefix(LogLevel level) {
        return level switch {
            LogLevel.Debug => "DEBUG",
            LogLevel.Info => "INFO",
            LogLevel.Warning => "WARNING",
            LogLevel.Error => "ERROR",
            _ => "UNKNOWN"
        };
    }

    private void Log(LogLevel level, Exception exception, string message, params object[] args) {
        if (_pane == null) throw new InvalidOperationException("Output pane is not initialized.");

        string formattedMessage = message;

        try {
            formattedMessage = string.Format(message, args);
        } catch (FormatException) {
            // ignore. CLI verbose logs
        }

        if (exception != null) {
            // log traceback in separate output window pane
            exception.Log();

            // add dot if not exists
            if (!formattedMessage.EndsWith(".")) formattedMessage += ".";

            formattedMessage += $" {exception.GetType()} - {exception.Message}";
        }

        string logLevel = GetLogLevelPrefix(level);
        string logMessage = $"{DateTime.Now:o} [{logLevel}] {formattedMessage}{Environment.NewLine}";

        _pane?.OutputString(logMessage);
        Console.Write(logMessage);

        if (level != LogLevel.Error) return;

        if (exception != null)
            SentrySdk.CaptureException(exception);
        else
            SentrySdk.CaptureMessage(formattedMessage, SentryLevel.Error);
    }
}