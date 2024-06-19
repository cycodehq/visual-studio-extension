using System.Windows;
using Cycode.VisualStudio.Extension.Shared.Services;

namespace Cycode.VisualStudio.Extension.Shared;

public partial class MainControl {
    public MainControl() {
        InitializeComponent();
    }

    private async void ScanSecretsClickAsync(object sender, RoutedEventArgs e) {
        ScanSecretsBtn.IsEnabled = false;
        ScanSecretsBtn.Content = "Scanning...";

        ILoggerService logger = ServiceLocator.GetService<ILoggerService>();

        try {
            // tome sleep for demo
            await Task.Delay(3000);
        } catch (Exception ex) {
            logger.Error(ex, "Failed to scan secrets");
        } finally {
            ScanSecretsBtn.IsEnabled = true;
            ScanSecretsBtn.Content = "Scan for hardcoded secrets";
        }
    }
}