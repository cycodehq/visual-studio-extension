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
        ICycodeService cycode = ServiceLocator.GetService<ICycodeService>();

        try {
            await cycode.StartSecretScanForCurrentProject();
        } catch (Exception ex) {
            logger.Error(ex, "Failed to scan secrets");
        } finally {
            ScanSecretsBtn.IsEnabled = true;
            ScanSecretsBtn.Content = "Scan for hardcoded secrets";
        }
    }
}