using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Navigation;

namespace Cycode.VisualStudio.Extension.Shared.Components.ToolWindows;

public static class HyperlinkExtensions {
    public static readonly DependencyProperty IsExternalProperty =
        DependencyProperty.RegisterAttached("IsExternal", typeof(bool), typeof(HyperlinkExtensions),
            new UIPropertyMetadata(false, OnIsExternalChanged));

    public static bool GetIsExternal(DependencyObject obj) {
        return (bool)obj.GetValue(IsExternalProperty);
    }

    public static void SetIsExternal(DependencyObject obj, bool value) {
        obj.SetValue(IsExternalProperty, value);
    }

    private static void OnIsExternalChanged(object sender, DependencyPropertyChangedEventArgs args) {
        if (sender is not Hyperlink hyperlink)
            return;

        if ((bool)args.NewValue)
            hyperlink.RequestNavigate += Hyperlink_RequestNavigate;
        else
            hyperlink.RequestNavigate -= Hyperlink_RequestNavigate;
    }

    private static void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e) {
        Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
        e.Handled = true;
    }
}