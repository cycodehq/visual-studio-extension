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

    private void Log(string level, string message) {
        ThreadHelper.ThrowIfNotOnUIThread();

        if (_pane == null) {
            InitializePane();
        }

        string formattedMessage = $"[{DateTime.Now:dd-MM-yyyy HH:mm:ss}] [{level}] {message}{Environment.NewLine}";

        _pane?.OutputString(formattedMessage);
        Console.WriteLine(formattedMessage);
    }
    
    public void Debug(string message) {
        ThreadHelper.ThrowIfNotOnUIThread();
        Log("DEBUG", message);
    }

    public void Error(string message) {
        ThreadHelper.ThrowIfNotOnUIThread();
        Log("ERROR", message);
    }

    public void Warn(string message) {
        ThreadHelper.ThrowIfNotOnUIThread();
        Log("WARNING", message);
    }

    public void Info(string message) {
        ThreadHelper.ThrowIfNotOnUIThread();
        Log("INFO", message);
    }
}