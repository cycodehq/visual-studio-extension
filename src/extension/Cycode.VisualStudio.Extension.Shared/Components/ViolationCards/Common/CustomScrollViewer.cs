using System.Windows.Controls;
using System.Windows.Input;

namespace Cycode.VisualStudio.Extension.Shared.Components.ViolationCards.Common;

internal class CustomScrollViewer : ScrollViewer {
    public CustomScrollViewer() {
        PreviewMouseWheel += ScrollViewer_MouseWheel;
        Focusable = false;
        VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
    }

    private void ScrollViewer_MouseWheel(object sender, MouseWheelEventArgs e) {
        // If the mouse is not over the control, we don't want to scroll it
        if (!IsMouseOver) return;

        // Raise the event on the parent control
        RaiseEvent(new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta) {
            RoutedEvent = MouseWheelEvent
        });

        e.Handled = true;
    }
}
