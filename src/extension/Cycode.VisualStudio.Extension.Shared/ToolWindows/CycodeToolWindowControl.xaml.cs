using System.Windows;
using Cycode.VisualStudio.Extension.Shared.Services;

namespace Cycode.VisualStudio.Extension.Shared;

public partial class CycodeToolWindowControl {
    public CycodeToolWindowControl(IToolWindowMessengerService toolWindowMessengerService) {
        toolWindowMessengerService.MessageReceived += OnMessageReceived;
        InitializeComponent();
    }

    private void OnMessageReceived(object sender, string e) {
        ThreadHelper.JoinableTaskFactory.RunAsync(async () => {
            switch (e) {
                case "test":
                    await Test();
                    break;
            }
        }).FireAndForget();
    }

    private async Task Test() {
        await VS.MessageBox.ShowAsync("Test message");
    }

    private async void AuthClickAsync(object sender, RoutedEventArgs e) {
        AuthBtn.IsEnabled = false;
        AuthBtn.Content = "Authenticating...";

        ICycodeService cycode = ServiceLocator.GetService<ICycodeService>();
        ILoggerService logger = ServiceLocator.GetService<ILoggerService>();

        try {
            await cycode.StartAuthAsync();
        } catch (Exception ex) {
            logger.Error(ex, "Failed to auth");
        } finally {
            AuthBtn.IsEnabled = true;
            AuthBtn.Content = "Authenticate";
        }
    }
}