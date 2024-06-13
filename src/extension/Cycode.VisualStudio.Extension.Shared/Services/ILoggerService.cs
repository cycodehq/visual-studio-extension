namespace Cycode.VisualStudio.Extension.Shared.Services;

public interface ILoggerService {
    void LogInfo(string message);
    void LogWarning(string message);
    void LogError(string message);
    void LogDebug(string message);
}