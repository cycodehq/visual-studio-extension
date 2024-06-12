using System.Windows;
using System.Windows.Controls;
using Cycode.VisualStudio.Extension.Shared.Cli;

namespace Cycode.VisualStudio.Extension.Shared;

public partial class CycodeToolWindowControl : UserControl {
    public CycodeToolWindowControl() {
        InitializeComponent();
    }

    private async void button1_Click(object sender, RoutedEventArgs e) {
        string ua = await UserAgent.GetUserAgentAsync();
        await VS.MessageBox.ShowAsync("Cycode", ua);
    }
}