namespace Cycode.VisualStudio.Extension.Shared.Services;

public interface ILoggerService {
    void Info(string message);
    void Warn(string message);
    void Error(string message);
    void Debug(string message);
}