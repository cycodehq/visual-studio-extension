using System.Windows;
using Cycode.VisualStudio.Extension.Shared.Services;

namespace Cycode.VisualStudio.Extension.Shared;

public partial class AuthControl {
    public AuthControl() {
        InitializeComponent();
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