namespace Cycode.VisualStudio.Extension.Shared.Services;

public interface IToolWindowMessengerService {
    void Send(string message);
    event EventHandler<string> MessageReceived;
}