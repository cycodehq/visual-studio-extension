using Cycode.VisualStudio.Extension.Shared.Services;

namespace Cycode.VisualStudio.Extension.Shared;

public class ToolWindowMessengerService : IToolWindowMessengerService {
    public void Send(MessageEventArgs args) {
        MessageReceived?.Invoke(this, args);
    }

    public event EventHandler<MessageEventArgs> MessageReceived;
}