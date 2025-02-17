using System.Windows;
using System.Windows.Controls;
using Cycode.VisualStudio.Extension.Shared.Cli.DTO;
using Cycode.VisualStudio.Extension.Shared.Services;

namespace Cycode.VisualStudio.Extension.Shared.Components.ToolWindows;

public partial class MainControl {
    public MainControl() {
        InitializeComponent();

        ScanSecretsBtn.IsEnabled = _tempState.IsSecretScanningEnabled;
        ScanScaBtn.IsEnabled = _tempState.IsScaScanningEnabled;
        ScanIacBtn.IsEnabled = _tempState.IsIacScanningEnabled;
        ScanSastBtn.IsEnabled = _tempState.IsSastScanningEnabled;
    }

    private static readonly ILoggerService _logger = ServiceLocator.GetService<ILoggerService>();
    private static readonly ICycodeService _cycode = ServiceLocator.GetService<ICycodeService>();
    private static readonly ITemporaryStateService _tempState = ServiceLocator.GetService<ITemporaryStateService>();

    private static async Task ExecuteWithButtonStateAsync(Button button, Func<Task> action) {
        string originalContent = button.Content.ToString();

        button.IsEnabled = false;
        button.Content = originalContent + "...";

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
                await _cycode.StartProjectScanAsync(CliScanType.Secret);
            } catch (Exception ex) {
                _logger.Error(ex, "Failed to scan secrets");
            }
        });
    }

    private async void ScanScaClickAsync(object sender, RoutedEventArgs e) {
        await ExecuteWithButtonStateAsync(ScanScaBtn, async () => {
            try {
                await _cycode.StartProjectScanAsync(CliScanType.Sca);
            } catch (Exception ex) {
                _logger.Error(ex, "Failed to scan SCA");
            }
        });
    }

    private async void ScanIacClickAsync(object sender, RoutedEventArgs e) {
        await ExecuteWithButtonStateAsync(ScanIacBtn, async () => {
            try {
                await _cycode.StartProjectScanAsync(CliScanType.Iac);
            } catch (Exception ex) {
                _logger.Error(ex, "Failed to scan IaC");
            }
        });
    }

    private async void ScanSastClickAsync(object sender, RoutedEventArgs e) {
        await ExecuteWithButtonStateAsync(ScanSastBtn, async () => {
            try {
                await _cycode.StartProjectScanAsync(CliScanType.Sast);
            } catch (Exception ex) {
                _logger.Error(ex, "Failed to scan SAST");
            }
        });
    }
}