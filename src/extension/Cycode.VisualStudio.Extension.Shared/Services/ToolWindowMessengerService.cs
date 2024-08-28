using Cycode.VisualStudio.Extension.Shared.Services;

namespace Cycode.VisualStudio.Extension.Shared;

public class ToolWindowMessengerService : IToolWindowMessengerService {
    public void Send(string message) {
        MessageReceived?.Invoke(this, message);
    }

    public event EventHandler<string> MessageReceived;
}