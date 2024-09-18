using System.Windows;
using System.Windows.Controls;
using Cycode.VisualStudio.Extension.Shared.Services;

namespace Cycode.VisualStudio.Extension.Shared.Components.ToolWindows;

public partial class MainControl {
    public MainControl() {
        InitializeComponent();
    }

    private static ILoggerService Logger => ServiceLocator.GetService<ILoggerService>();
    private static ICycodeService Cycode => ServiceLocator.GetService<ICycodeService>();

    private static async Task ExecuteWithButtonStateAsync(Button button, Func<Task> action) {
        string originalContent = button.Content.ToString();

        button.IsEnabled = false;
        button.Content = "Scanning...";

        try {
            await action();
        } finally {
            button.IsEnabled = true;
            button.Content = originalContent;
        }
    }

    private async void ScanSecretsClickAsync(object sender, RoutedEventArgs e) {
        await ExecuteWithButtonStateAsync(ScanSecretsBtn, async () => {
            try {
                await Cycode.StartSecretScanForCurrentProjectAsync();
            } catch (Exception ex) {
                Logger.Error(ex, "Failed to scan secrets");
            }
        });
    }

    private async void ScanScaClickAsync(object sender, RoutedEventArgs e) {
        await ExecuteWithButtonStateAsync(ScanScaBtn, async () => {
            try {
                await Cycode.StartScaScanForCurrentProjectAsync();
            } catch (Exception ex) {
                Logger.Error(ex, "Failed to scan SCA");
            }
        });
    }

    private async void ScanIacClickAsync(object sender, RoutedEventArgs e) {
        await ExecuteWithButtonStateAsync(ScanIacBtn, async () => {
            try {
                await Cycode.StartIacScanForCurrentProjectAsync();
            } catch (Exception ex) {
                Logger.Error(ex, "Failed to scan IaC");
            }
        });
    }

    private async void ScanSastClickAsync(object sender, RoutedEventArgs e) {
        await ExecuteWithButtonStateAsync(ScanSastBtn, async () => {
            try {
                await Cycode.StartSastScanForCurrentProjectAsync();
            } catch (Exception ex) {
                Logger.Error(ex, "Failed to scan SAST");
            }
        });
    }
}