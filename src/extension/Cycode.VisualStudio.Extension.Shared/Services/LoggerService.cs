using Cycode.VisualStudio.Extension.Shared.DTO;
using Microsoft.VisualStudio.Shell.Interop;

namespace Cycode.VisualStudio.Extension.Shared.Services;

using System;
using Microsoft.VisualStudio.Shell;

public class LoggerService : ILoggerService {
    private IVsOutputWindowPane _pane;

    private static readonly Guid _paneGuid = new("AF772BC0-5CBF-4A44-AE52-7F428C5659A1");
    private const string _paneTitle = "Cycode";

    public LoggerService() {
        ThreadHelper.ThrowIfNotOnUIThread();
        InitializePane();
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
            _ => "UNKNOWN",
        };
    }

    private void Log(LogLevel level, Exception exception, string message, params object[] args) {
        ThreadHelper.ThrowIfNotOnUIThread();

        if (_pane == null) {
            InitializePane();
        }

        string formattedMessage = string.Format(message, args);
        if (exception != null) {
            // log traceback in separate output window pane
            exception.Log();

            // add dot if not exists
            if (!formattedMessage.EndsWith(".")) {
                formattedMessage += ".";
            }

            formattedMessage += $" {exception.GetType()} - {exception.Message}";
        }

        string logLevel = GetLogLevelPrefix(level);
        formattedMessage = $"{DateTime.Now:MMM dd HH:mm:ss} [{logLevel}] {formattedMessage}{Environment.NewLine}";

        _pane?.OutputString(formattedMessage);
        Console.WriteLine(formattedMessage);
    }

    public void Debug(string message, params object[] args) {
        ThreadHelper.ThrowIfNotOnUIThread();
        Log(LogLevel.Debug, null, message, args);
    }

    public void Debug(Exception exception, string message, params object[] args) {
        ThreadHelper.ThrowIfNotOnUIThread();
        Log(LogLevel.Debug, exception, message, args);
    }

    public void Debug(string message, Exception exception, params object[] args) {
        ThreadHelper.ThrowIfNotOnUIThread();
        Log(LogLevel.Debug, exception, message, args);
    }

    public void Error(string message, params object[] args) {
        ThreadHelper.ThrowIfNotOnUIThread();
        Log(LogLevel.Error, null, message, args);
    }

    public void Error(Exception exception, string message, params object[] args) {
        ThreadHelper.ThrowIfNotOnUIThread();
        Log(LogLevel.Error, exception, message, args);
    }

    public void Error(string message, Exception exception, params object[] args) {
        ThreadHelper.ThrowIfNotOnUIThread();
        Log(LogLevel.Error, exception, message, args);
    }

    public void Warn(string message, params object[] args) {
        ThreadHelper.ThrowIfNotOnUIThread();
        Log(LogLevel.Warning, null, message, args);
    }

    public void Warn(Exception exception, string message, params object[] args) {
        ThreadHelper.ThrowIfNotOnUIThread();
        Log(LogLevel.Warning, exception, message, args);
    }

    public void Warn(string message, Exception exception, params object[] args) {
        ThreadHelper.ThrowIfNotOnUIThread();
        Log(LogLevel.Warning, exception, message, args);
    }

    public void Info(string message, params object[] args) {
        ThreadHelper.ThrowIfNotOnUIThread();
        Log(LogLevel.Info, null, message, args);
    }

    public void Info(string message, Exception exception, params object[] args) {
        ThreadHelper.ThrowIfNotOnUIThread();
        Log(LogLevel.Info, exception, message, args);
    }

    public void Info(Exception exception, string message, params object[] args) {
        ThreadHelper.ThrowIfNotOnUIThread();
        Log(LogLevel.Info, exception, message, args);
    }
}