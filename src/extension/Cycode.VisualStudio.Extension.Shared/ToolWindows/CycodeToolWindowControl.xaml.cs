using System.Windows;
using Cycode.VisualStudio.Extension.Shared.Cli;

namespace Cycode.VisualStudio.Extension.Shared;

public partial class CycodeToolWindowControl {
    public CycodeToolWindowControl() {
        InitializeComponent();
    }

    private async void ButtonClickAsync(object sender, RoutedEventArgs e) {
        string ua = await UserAgent.GetUserAgentAsync();
        await VS.MessageBox.ShowAsync("Cycode", ua);
    }
}