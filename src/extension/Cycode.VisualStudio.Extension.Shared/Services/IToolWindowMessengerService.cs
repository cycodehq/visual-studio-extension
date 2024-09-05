namespace Cycode.VisualStudio.Extension.Shared.Services;

public class MessageEventArgs(string message, object data = null) : EventArgs {
    public string Command { get; set; } = message;
    public object Data { get; set; } = data;
}

public interface IToolWindowMessengerService {
    void Send(MessageEventArgs args);
    event EventHandler<MessageEventArgs> MessageReceived;
}