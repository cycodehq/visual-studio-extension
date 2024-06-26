namespace Cycode.VisualStudio.Extension.Shared.Services;

public interface ILoggerService {
    void Initialize();

    void Info(string message, params object[] args);
    void Info(Exception exception, string message, params object[] args);
    void Info(string message, Exception exception, params object[] args);

    void Warn(string message, params object[] args);
    void Warn(Exception exception, string message, params object[] args);
    void Warn(string message, Exception exception, params object[] args);

    void Error(string message, params object[] args);
    void Error(Exception exception, string message, params object[] args);
    void Error(string message, Exception exception, params object[] args);

    void Debug(string message, params object[] args);
    void Debug(Exception exception, string message, params object[] args);
    void Debug(string message, Exception exception, params object[] args);
}