using Cycode.VisualStudio.Extension.Shared.Services;

namespace Cycode.VisualStudio.Extension.Shared;

public partial class CycodeToolWindowControl {
    public CycodeToolWindowControl(IToolWindowMessengerService toolWindowMessengerService) {
        DataContext = new CycodeToolWindowViewModel(toolWindowMessengerService);
        InitializeComponent();
    }
}