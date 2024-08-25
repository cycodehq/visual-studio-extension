using System.ComponentModel;
using System.Windows.Controls;
using Cycode.VisualStudio.Extension.Shared.Components.TreeView;
using Cycode.VisualStudio.Extension.Shared.DTO;
using Cycode.VisualStudio.Extension.Shared.Services;

namespace Cycode.VisualStudio.Extension.Shared.Components.ToolWindows;

public class CycodeToolWindowViewModel : INotifyPropertyChanged {
    private UserControl _leftSideView;
    private UserControl _rightSideView;

    private readonly CycodeTreeViewControl _cycodeTreeView;

    public UserControl LeftSideView {
        get => _leftSideView;
        set {
            _leftSideView = value;
            OnPropertyChanged(nameof(LeftSideView));
        }
    }

    public UserControl RightSideView {
        get => _rightSideView;
        set {
            _rightSideView = value;
            OnPropertyChanged(nameof(RightSideView));
        }
    }

    public CycodeToolWindowViewModel(IToolWindowMessengerService toolWindowMessengerService) {
        toolWindowMessengerService.MessageReceived += OnMessageReceived;

        _cycodeTreeView = new CycodeTreeViewControl();

        LeftSideView = _cycodeTreeView;
        RightSideView = new LoadingControl();
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged(string propertyName) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void OnMessageReceived(object sender, string e) {
        RightSideView = e switch {
            MessengerCommand.LoadLoadingControl => new LoadingControl(),
            MessengerCommand.LoadAuthControl => new AuthControl(),
            MessengerCommand.LoadMainControl => new MainControl(),
            _ => RightSideView
        };

        if (e == MessengerCommand.RefreshTreeView) {
            _cycodeTreeView.RefreshTree();
        }
    }
}